# Система рецензирования и рейтинга ресторанов ("RestoRate")

## Концепция системы: "RestoRate"

Система позволяет пользователям оставлять рецензии на рестораны и выставлять оценки (1–10). Рецензия может содержать текст, теги ("семейный", "шумно", "кафе"), субъективный средний чек, впечатления. Контент проходит полуавтоматическую модерацию. Агрегированный рейтинг (средняя оценка, количество рецензий, распределение по оценкам) пересчитывается асинхронно.

---

## Архитектура и компоненты

1. **API Gateway (YARP):**
    * Единая точка входа для клиента.
    * Маршрутизация запросов к соответствующим микросервисам.

2. **Микросервисы (каждый — отдельный .NET проект):**
    * **Restaurant Service:** Управление каталогом ресторанов (CRUD). `Порт: 5001*`
        * **Хранилище:** PostgreSQL.
        * **Сущности DDD:** `Restaurant` (Aggregate Root), `RestaurantId`.
        * **Поля:** Id, Name, Description, Address (City, Street, Building, Apartment, PostalCode), Photos (список изображений), Cuisine, Rating (AverageRate, AverageCheck, ReviewCount), OpenHours (часы работы), Tags (веган, бар), Location (гео координаты).
    * **Review Service:** Управление рецензиями. `Порт: 5002*`
        * **Хранилище:** MongoDB.
        * **Сущности:** `Review`, `ReviewId`, `UserId`, `RestaurantId`.
        * **Поля:** Id рецензии, RestaurantId, UserId, Rate (1–10), Comment, Tags (уютно, шумно), SuggestedAverageCheck, Status (pending/approved/rejected), RejectionReason, CreatedAt.
    * **Rating Service:** Агрегирование и кеширование. `Порт: 5003*`
        * **Кеш:** Redis.
        * **Метрики:** AverageRate (средняя оценка), ReviewCount (количество рецензий), AverageCheck (средний чек).
        * **Источник:** Слушает `ReviewAddedEvent`, `ReviewApprovedEvent` (финальная фиксация приходит после модерации через Review Service).

    Примечание: порты со звёздочкой (*) показаны для режима `AppHostConfiguration.UseDedicatedPorts` в Aspire AppHost; иначе Aspire назначает порты динамически.

3. **Связь между сервисами:**
    * **События (RabbitMQ):**
        * `RestaurantCreatedEvent`, `RestaurantUpdatedEvent`, `RestaurantArchivedEvent` (от Restaurant Service).
        * `ReviewAddedEvent`, `ReviewApprovedEvent` (от Review Service).
        * `ReviewModeratedEvent` (от Moderation Service).
    * **Синхронные вызовы (минимально):**
        * Review Service при отсутствии локально кэшируемого значения может проверить ресторан через REST.

4. **Инфраструктура:**
    * **RabbitMQ:** Поток доменных событий.
    * **PostgreSQL:** Каталог ресторанов.
    * **MongoDB:** Рецензии + задачи модерации.
    * **Redis:** Горячие агрегаты рейтингов.

---

## Реализация стека технологий

### 1. DDD и Value Objects

```csharp
// Address Value Object
public record Address
{
    public string City { get; private init; }
    public string Street { get; private init; }
    public string Building { get; private init; }
    public string Apartment { get; private init; }
    public string PostalCode { get; private init; }
    public string FormattedAddress => $"{Street} {Building}, {City}, {PostalCode}";
    private Address() { }
    public Address(string city, string street, string building, string apartment = null, string postalCode = null)
    {
        City = city;
        Street = street;
        Building = building;
        Apartment = apartment;
        PostalCode = postalCode;
    }
}

// Rating projection + Money
// В текущей реализации RestaurantService использует:
// - `NodaMoney.Money` для денежных значений
// - `RatingSnapshot` как проекцию агрегированного рейтинга ресторана

// Restaurant Aggregate
public class Restaurant : AggregateRoot<RestaurantId>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Address Address { get; private set; }
    public string Cuisine { get; private set; }
    public RatingSnapshot Rating { get; private set; }
    // ... другие поля ...
    // Рейтинг обновляется через методы проекции (например, ApplyApproved/ApplyProvisional)
    // и хранит как финальные, так и предварительные значения.
}
```

### 2. Асинхронные операции и Сериализация

* **Везде `async/await`.** Например, `MongoDbReviewRepository`:

```csharp
public class MongoDbReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _collection;
    public MongoDbReviewRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Review>("reviews");
        // Сериализация/десериализация Review в BSON handled by driver
    }

    public async Task AddAsync(Review review)
    {
        await _collection.InsertOneAsync(review);
    }

    public async Task<Review> GetByIdAsync(ReviewId id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }
}
```

* **MassTransit + RabbitMQ (рекомендуется)** — краткий пример публикации, потребления и настройки:

```csharp
// Publish (например, в Review Service)
await publish.Publish(new ReviewAddedEvent(reviewId, restaurantId, authorId, rating, null, text, tags));

// Consumer (например, в Moderation Service)
public sealed class ReviewAddedConsumer : IConsumer<ReviewAddedEvent>
{
    public Task Consume(ConsumeContext<ReviewAddedEvent> ctx)
        => HandleAsync(ctx.Message);
}

// Minimal setup (в любом сервисе)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReviewAddedConsumer>();
    x.UsingRabbitMq((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
});
```

### 3. Логи и Метрики

* **Логи:** `ILogger<T>` + Serilog для структурированного логирования в JSON/Elasticsearch.
* **Метрики:** `System.Diagnostics.Metrics` + OpenTelemetry для сбора метрик (количество запросов, ошибок, длительность операций) и экспорта в Prometheus/Grafana.

### 4. Тестирование

* **Unit-тесты (xUnit, NSubstitute):** Тестирование доменной логики (например, расчет рейтинга в Rating Service), обработчиков команд.
* **Интеграционные тесты (TestContainers):** Запуск реальных контейнеров с БД и RabbitMQ для тестирования репозиториев и обработчиков событий.

---

## Поток данных: Пользователь добавляет рецензию

1. **Клиент** отправляет `POST /api/restaurants/{id}/reviews`.
2. **Gateway** перенаправляет в **Review Service**.
3. **Review Service** сохраняет рецензию (status=pending) и публикует `ReviewAddedEvent`.
4. **Moderation Service** (если требуется) обрабатывает и публикует `ReviewModeratedEvent`.
5. **Review Service** принимает `ReviewModeratedEvent`, обновляет статус и (если Approved) публикует `ReviewApprovedEvent`.
6. **Rating Service** на `ReviewAddedEvent`/`ReviewApprovedEvent` пересчитывает агрегаты и обновляет Redis.
7. **Клиент** при чтении получает актуальный рейтинг из кеша.

---

## Как это развернуть и показать "без заморочек"

1. **`docker-compose.yml`** для поднятия всей инфраструктуры (RabbitMQ, PostgreSQL, MongoDB, Redis) одной командой.
2. **Код на GitHub:** Отдельные папки для каждого микросервиса, API Gateway и общих библиотек (Domain Events, Shared Kernel).
3. **`README.md`** с инструкцией: `docker-compose up -d` и `dotnet run` для каждого сервиса в своем терминале.
4. **Postman-коллекция** с примерами запросов ко всем API.

Вот расширенная схема и описание:

## Архитектурная схема RestoRate

```fundamental
                       ┌───────────────────────────────────┐    ┌─────────────────┐
                       │         API Gateway               │    │   Blazor Server │
                       │           (YARP)                  │◄──►│   Dashboard     │
                       └───────────────────────────────────┘    └─────────────────┘
                              │              │                         │
                              │              │                         │
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Микросервисы                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │ Restaurant  │  │ Review      │  │ Rating      │  │ Moderation  │         │
│  │ Service     │  │ Service     │  │ Service     │  │ Service     │         │
│  │ (5001)      │  │ (5002)      │  │ (5003)      │  │ (5004)      │         │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘         │
│         │               │               │               │                   │
│    PostgreSQL      MongoDB         Redis Queue     MongoDB                  │
│    (Restaurants)   (Reviews)       (Ratings)       (Moderation)             │
└─────────┼───────────────┼───────────────┼───────────────┼───────────────────┘
          │               │               │               │
          └───────────────┼───────────────┼───────────────┘
                          │               │
                    ┌─────▼───────────────▼─────┐
                    │    RabbitMQ (Events)      │
                    │  ┌─────────────────────┐  │
                    │  │ review.added        │  │
                    │  │ review.updated      │  │
                    │  │ review.moderated    │  │
                    │  │ restaurant.created  │  │
                    │  └─────────────────────┘  │
                    └───────────────────────────┘
```

## Новые компоненты

### 1. Moderation Service (`:5004`)

```csharp
// Domain Event для модерации
public record ReviewModerationResultEvent(
    ReviewId ReviewId, 
    bool IsApproved, 
    string ModeratorId, 
    string Reason);

// Модель модерации в MongoDB
public class ModerationTask
{
    public ModerationTaskId Id { get; set; }
    public ReviewId ReviewId { get; set; }
    public string ReviewText { get; set; }
    public int Rating { get; set; }
    public string RestaurantId { get; set; }
    public string UserId { get; set; }
    public ModerationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModeratorId { get; set; }
    public string ModerationReason { get; set; }
}
```

### 2. Blazor Server Dashboard (`:5005`)

```csharp
// Pages/Moderation.razor
@page "/moderation"
@inject IModerationService ModerationService

<h3>Moderation Queue</h3>

@foreach (var task in moderationTasks)
{
    <div class="moderation-item">
        <p><strong>Review:</strong> @task.ReviewText</p>
        <p><strong>Rating:</strong> @task.Rating</p>
        <button @onclick="() => Approve(task.Id)" class="btn btn-success">Approve</button>
        <button @onclick="() => Reject(task.Id)" class="btn btn-danger">Reject</button>
    </div>
}

@code {
    private List<ModerationTask> moderationTasks = new();

    protected override async Task OnInitializedAsync()
    {
        moderationTasks = await ModerationService.GetPendingTasksAsync();
    }

    private async Task Approve(string taskId)
    {
        await ModerationService.ApproveReviewAsync(taskId, "Moderator_1");
        moderationTasks = await ModerationService.GetPendingTasksAsync();
        StateHasChanged();
    }
}
```

## Полный поток данных: Добавление и модерация отзыва

```fundamental
1. Клиент ───POST /reviews───► Gateway ───► Review Service ───► MongoDB
2. Review Service ───ReviewAddedEvent───► RabbitMQ
3. RabbitMQ ───► Moderation Service (авто-проверка + создание задачи)
4. RabbitMQ ───► Rating Service (временный расчет)
5. Модератор в Blazor Dashboard ───► Moderation Service
6. Moderation Service ───ReviewModeratedEvent───► RabbitMQ
7. RabbitMQ ───► Review Service (обновляет статус и публикует ReviewApprovedEvent)
8. RabbitMQ ───► Rating Service (финальный расчет на ReviewApprovedEvent)
9. Клиент видит одобренный отзыв
```

## API Endpoints через Gateway

### Gateway API (требует авторизации)

На текущей реализации все проксируемые маршруты в Gateway защищены (`RequireAuthorization()`), и endpoints сервиса Restaurant также требуют авторизацию на группе `/restaurants`.

| Метод | Эндпоинт | Сервис | Описание |
| --- | --- | --- | --- |
| `GET` | `/api/restaurants` | Restaurant | Список ресторанов |
| `GET` | `/api/restaurants/{id}/reviews` | Review | Рецензии ресторана |
| `POST` | `/api/restaurants/{id}/reviews` | Review | Добавить рецензию |
| `GET` | `/api/restaurants/{id}/rating` | Rating | Рейтинг ресторана |

### Moderation Service API

| Метод | Эндпоинт | Сервис | Описание |
| --- | --- | --- | --- |
| `GET` | `/api/moderation/pending` | ModerationService | Задачи на модерацию |
| `POST` | `/api/moderation/{taskId}/approve` | ModerationService | Одобрить отзыв |
| `POST` | `/api/moderation/{taskId}/reject` | ModerationService | Отклонить отзыв |

### Blazor Pages

| URL | Компонент | Назначение |
| --- | --- | --- |
| `/` | RestaurantCatalog.razor | Каталог ресторанов |
| `/restaurant/{id}` | RestaurantDetail.razor | Детали ресторана + рецензии |
| `/moderation` | Moderation.razor | Панель модерации |

## Детали реализации Moderation Service

```csharp
// Автоматическая проверка на запрещенные слова + создание задачи
public class ReviewAddedEventHandler
{
    private readonly IModerationTaskRepository _repository;
    private readonly IProfanityFilter _filter;

    public async Task Handle(ReviewAddedEvent @event)
    {
        // Авто-проверка
        var autoCheck = _filter.Check(@event.Text);
        
        var task = new ModerationTask
        {
            ReviewId = @event.ReviewId,
            ReviewText = @event.Text,
            Rating = @event.Rating,
            RestaurantId = @event.RestaurantId,
            UserId = @event.AuthorId.ToString(),
            Status = autoCheck.IsApproved ? 
                ModerationStatus.AutoApproved : 
                ModerationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(task);

        // Если авто-одобрен - сразу публикуем результат
        if (autoCheck.IsApproved)
        {
            PublishModerationResult(task, true, "Auto-approved");
        }
    }
}
```

## Blazor Dashboard Features

```csharp
// Реализация главной страницы каталога
@page "/"
@inject IRestaurantService RestaurantService
@inject IRatingService RatingService

<div class="row">
    @foreach (var restaurant in restaurants)
    {
        <div class="col-md-4 mb-4">
            <div class="card">
                <div class="card-body">
                    <h5>@restaurant.Name</h5>
                    <p>@restaurant.Cuisine - @restaurant.City</p>
                    <div class="rating">
                        <strong>Рейтинг: </strong>
                        <span>@(ratings[restaurant.Id]?.AverageRate ?? 0)/5</span>
                        <span>(@(ratings[restaurant.Id]?.ReviewCount ?? 0) рецензий)</span>
                        <span>Средний чек: @(ratings[restaurant.Id]?.AverageCheck?.ToString() ?? "-")</span>
                    </div>
                    <a href="/restaurant/@restaurant.Id" class="btn btn-primary">Подробнее</a>
                </div>
            </div>
        </div>
    }
</div>

@code {
    private List<RestaurantDto> restaurants = new();
    private Dictionary<string, RatingDto> ratings = new();

    protected override async Task OnInitializedAsync()
    {
        restaurants = await RestaurantService.GetRestaurantsAsync();
        foreach (var r in restaurants)
        {
            ratings[r.Id] = await RatingService.GetRatingAsync(r.Id);
        }
    }
}
```

## Преимущества такой архитектуры

1. **Полный стек**: От БД до UI
2. **Реальные use cases**: Модерация, кеширование, асинхронные операции
3. **Разные типы БД**: Реляционная + документная + кеш
4. **Разные коммуникации**: HTTP + Message Queue
5. **Разные клиенты**: REST API + Blazor UI
6. **Примитивы синхронизации**: Lock в RatingService при обновлении рейтинга
7. **Тестируемость**: Каждый сервис изолирован

Вся система демонстрирует полный цикл работы современного .NET приложения с микросервисной архитектурой!
