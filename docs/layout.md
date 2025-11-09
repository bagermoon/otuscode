# Структура монорепозитория для микросервисов по DDD (с .NET Aspire и Blazor)

Документ фиксирует рекомендуемую структуру проектов и правила разделения ответственности между `ServiceDefaults`, `Contracts`, `Abstractions`, `BuildingBlocks`, `SharedKernel`, `Common` и константами в контексте .NET 9, .NET Aspire (AppHost, ServiceDefaults), Gateway (YARP) и Blazor Server Dashboard.

## Цели
- Чёткие границы между доменом и инфраструктурой.
- Единый подход к service discovery и устойчивым HTTP‑клиентам (через ServiceDefaults).
- Повторно используемые, но корректно изолированные общие компоненты.
- Удобная локальная разработка и запуск через Aspire AppHost.

## Базовая структура репозитория

- src/
  - `RestoRate.AppHost` — оркестратор Aspire (Keycloak, RabbitMQ, Gateway, UI и доменные сервисы).
  - `RestoRate.ServiceDefaults` — настройки хостинга: service discovery, resilience HttpClient, OpenTelemetry, health.
  - `RestoRate.Contracts` — межсервисные DTO и интеграционные события (версионирование, сериализация). [Структура и рекомендации](./layout.contracts.md)
  - `RestoRate.Abstractions` — чистые интерфейсы/примитивы (напр. `Messaging.IIntegrationEventBus`) без зависимостей от транспорта/EF.
  - `RestoRate.BuildingBlocks` — переиспользуемые инфраструктурные реализации (MassTransitEventBus, миграции/сидеры EF, resilience middleware).
  - `RestoRate.SharedKernel` — доменные строительные блоки (entity base, value object, domain event, Result).
  - `RestoRate.Migrations` — дизайн‑тайм хост для EF Core (используется как `--startup-project` для CLI и в Aspire при выполнении миграций). Хранит только запуск и DI, без доменной логики.
  - `RestoRate.Gateway` — шлюз (YARP) + токен exchange.
  - `RestoRate.BlazorDashboard` — Blazor Server UI.
  - Доменные сервисы по границам (Bounded Contexts), каждый в 4 слоя:
    - `RestoRate.<Context>.Domain`
    - `RestoRate.<Context>.Application`
    - `RestoRate.<Context>.Infrastructure`
    - `RestoRate.<Context>.Api`
  - Текущий прогресс по контекстам:
    - `Restaurant` — все 4 слоя присутствуют.
    - `Moderation`, `Rating` — пока есть слой `Api` (заготовки); остальные планируются.
    - `Review` — в планах.
- tests/
  - Юнит‑тесты домена/приложения и интеграционные тесты инфраструктуры (с Testcontainers).

Примеры контекстов: `Restaurant`, `Review`, `Rating`, `Moderation`.

### Зависимости между слоями (строго)
- `Api` → `Application` → `Domain`.
- `Infrastructure` → `Application` + `Domain`.
- Сервисы не ссылаются на `Domain/Application` других сервисов. Межсервисный обмен — только через `Contracts`.

### Общие пакеты (границы использования)
  - `RestoRate.Abstractions` — допустимо в слоях `Api`, `Application`, `Infrastructure`; НЕ в `Domain`.
  - `RestoRate.BuildingBlocks` — только `Api` и `Infrastructure`; НЕ в `Domain`; в `Application` только если тип не тянет инфраструктуру (редко).
  - `RestoRate.ServiceDefaults` — в каждом `Program.cs` внешних точек входа.
  - `RestoRate.Contracts` — в `Api`, `Application`, `Infrastructure`; НЕ в `Domain`.
  - `RestoRate.SharedKernel` — только доменные проекты.

## Общие пакеты: Abstractions и BuildingBlocks

Эволюция: прежние `RestoRate.Application`/`RestoRate.Infrastructure` заменены на более чёткие:

- `RestoRate.Abstractions`
  - Декларация чистых портов/контрактов (например, `Messaging.IIntegrationEventBus`, `IIntegrationEvent`).
  - Без зависимостей от EF/MassTransit/ASP.NET.
- `RestoRate.BuildingBlocks`
  - Реализации и DI‑расширения провайдеров (MassTransit/RabbitMQ), миграции/сидеры EF, технические middleware.
  - Примеры:
    - `Messaging.MassTransitEventBus`, `Messaging.MassTransitExtensions` (читает `ConnectionStrings:RabbitMQ`, использует `AppHostProjects.RabbitMQ`).
    - `Migrations.AddMigration<TContext, TSeeder>()` и `IDbSeeder<TContext>`.

Границы:
- `Abstractions` не тянут инфраструктуру и не попадают в Domain.
- `BuildingBlocks` не просачиваются в Domain и минимально используются в Application.

## Messaging через MassTransit (RabbitMQ)

Ниже — минимальный пример, как подключить общую шину событий и публиковать события из обработчиков.

1) Регистрация MassTransit/RabbitMQ в сервисе (Api/Infrastructure слой):

```csharp
// Program.cs
using RestoRate.BuildingBlocks.Messaging;
using MassTransit;
// ...
builder.Services.AddMassTransitEventBus(
  builder.Configuration,
  addConsumers: x =>
  {
    // При необходимости добавить консьюмеры:
    // x.AddConsumer<ReviewAddedConsumer>();
    // Либо: x.AddConsumers(typeof(Program).Assembly);
  });
```

2) Публикация интеграционного события из обработчика Application (Mediator):

```csharp
using RestoRate.Abstractions.Messaging; // IIntegrationEventBus
using RestoRate.Contracts.Events;       // ReviewAddedEvent
// ...

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand>
{
  private readonly IIntegrationEventBus _bus;

  public CreateReviewHandler(IIntegrationEventBus bus) => _bus = bus;

  public async ValueTask Handle(CreateReviewCommand request, CancellationToken ct)
  {
    // ... доменная логика, сохранение ...

    var evt = new ReviewAddedEvent(
      request.ReviewId,
      request.RestaurantId,
      request.UserId,
      request.Rating,
      request.Text
    );

    await _bus.PublishAsync(evt, ct);
    // также доступен перегруженный метод с заголовками:
    // await _bus.PublishAsync(evt, new Dictionary<string, object?> { ["tenant"] = request.TenantId }, ct);
  }
}
```

3) Конфигурация строки подключения RabbitMQ
- В `appsettings*.json` сервисов используйте `ConnectionStrings:RabbitMQ` (имя берётся из [`AppHostProjects.RabbitMQ`](../src/RestoRate.ServiceDefaults/AppHostProjects.cs)).
- При запуске через Aspire AppHost значение обычно заполняется автоматически.

Примечания и следующая эволюция:
- Outbox/Inbox, ретраи, idempotency — планируются в сервисных `Infrastructure` слоях.
- Для консистентной сериализации событий используйте типы из `RestoRate.Contracts` (каждое интеграционное событие реализует `IIntegrationEvent`).

## Ответственность слоёв (Layer Responsibilities)

- Api (внешний слой сервиса)
  - Endpoints (Controllers/Minimal APIs/SignalR) и HTTP‑контракты сервиса.
  - Аутентификация/авторизация (Keycloak/JWT), политики/роллинг‑зоны доступа.
  - Валидация входа на уровне API (model binding, basic validation) и преобразование в команды/запросы Application.
  - Делегирование бизнес‑действий через Mediator: только `Send()`/`Publish()` без бизнес‑логики.
  - Версионирование HTTP‑контракта (если нужно), OpenAPI в Dev, `app.MapDefaultEndpoints()` для health.
  - Никаких доступов к БД/шинам/внешним API напрямую — только через Application.

- Application (use‑cases, orchestration)
  - Команды и запросы (Mediator) + их обработчики: бизнес‑правила сценариев и координация домена.
  - Порты/интерфейсы для инфраструктуры (например, репозитории, брокеры сообщений, внешние API, кэш).
  - Транзакционные границы use‑case’ов; применение доменных событий/правил; идемпотентность при необходимости.
  - Pipeline Behaviors (валидация, логирование, ретраи, performance) — без привязки к конкретной инфраструктуре.
  - DTO для входа/выхода use‑case’ов (внутренние для сервиса). Межсервисные DTO — только из `RestoRate.Contracts`.
  - Никаких EF/RabbitMQ/HTTP‑клиентов напрямую: только через абстракции.

- Domain (чистый домен)
  - Сущности/агрегаты, Value Objects, доменные сервисы, доменные события, инварианты.
  - Полная независимость от инфраструктуры и фреймворков; только .NET/язык.
  - Логика изменения состояния и генерация доменных событий; отсутствие побочных эффектов наружу.

- Infrastructure (адаптеры и реализации портов)
  - Реализации портов Application: репозитории (EF Core/Dapper), клиенты внешних сервисов/HTTP, кэш, брокер сообщений.
  - Маппинг к `RestoRate.Contracts` для публикации/чтения интеграционных событий.
  - Транспорт/провайдер‑специфика: подключения, миграции, конфигурация хранилищ, Outbox/Inbox, ретраи.
  - Регистрация DI (composition root) и интеграция с `RestoRate.ServiceDefaults` (resilience, discovery, telemetry).
  - Никакой бизнес‑логики; только технические детали и маппинг к домену/Application.

## SharedKernel vs Contracts vs Константы

- `SharedKernel` (только домен):
  - Доменные строительные блоки: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent`, `Result` и т.п.
  - Никакой ASP.NET, Aspire, Keycloak, HTTP, RabbitMQ, EF — только домен.
  - Доменные константы (инварианты) допустимы, если не завязаны на инфраструктуру.

- `Contracts` (межсервисные договорённости):
  - DTO и интеграционные события: сериализуемые, стабильные, версиируемые.
  - Без бизнес‑логики — только данные.

- Константы:
  - Доменные константы — в конкретном `...Domain` или, если действительно обще‑доменные, в `SharedKernel`.
  - Избегать «магических» строк: выносить в единые константы.

## Aspire: внутренние и внешние эндпойнты
- Публичные точки входа (Gateway, Blazor UI, Keycloak UI) помечайте как внешние:
  - `builder.AddProject<...>("gateway").WithExternalHttpEndpoints();`
  - `builder.AddProject<...>("blazor-dashboard").WithExternalHttpEndpoints();`
  - `builder.AddKeycloak(...).WithExternalHttpEndpoints();`
- Внутренние доменные сервисы обычно не публикуются наружу; доступны через service discovery и Gateway.
- Для discovery используйте URI вида `https+http://{serviceName}` и `HttpClient`, сконфигурированные `ServiceDefaults` (устойчивость + discovery по умолчанию).

## Правила для Program.cs (каждый Api/UI)
- `builder.AddServiceDefaults();` — Общие метрики, расширения аутентификации HttpClient и OpenTelemetry.
- Регистрировать DI для Application/Infrastructure.
- `app.MapDefaultEndpoints();` — health‑эндпойнты (в Dev).
- В Dev — OpenAPI (по необходимости), в Prod — согласно политике безопасности.

Дополнительно для EF Core:
- В `DbContext` используем `UseSnakeCaseNamingConvention()` для таблиц/столбцов (PostgreSQL / Npgsql).
- Явные `HasColumnName(...)` не требуются, если совпадают с результатом конвенции (рекомендовано удалять избыточные маппинги).
- С `AddDbContextPool` перехватчики EF должны быть безопасны для корневого контейнера. Наш `EventDispatchInterceptor` регистрируется как singleton и вытягивает scoped‑сервисы внутри обработчика через контекст.

Смотрите `RestoRate.BuildingBlocks/Data/DbContextExtensions.cs` и `.../Data/Interceptors/EventDispatchInterceptor.cs`.

## Тестирование (???)
- Юнит‑тесты домена (Domain) без инфраструктуры.
- Тесты Application‑слоя (моки портов).
- Интеграционные тесты Infrastructure с Testcontainers (PostgreSQL/MongoDB/RabbitMQ/Redis).
- Контрактные тесты для `Contracts` (совместимость DTO/событий между сервисами).

## Рекомендации по именованию
- Проекты: `RestoRate.<Context>.<Layer>`.
- Пространства имён соответствуют структуре каталогов.
- События и маршруты RabbitMQ: доменные префиксы (`restaurant.*`, `review.*`).


## Чек‑лист
- Доменные объекты — только в `Domain` (или в `SharedKernel`, если общий доменный блок).
- Никакой инфраструктуры в `Domain` и `SharedKernel`.

- Межсервисный обмен — через `Contracts`.
- Публичные точки входа — `WithExternalHttpEndpoints()`; остальное — через Gateway и discovery.

## Миграции EF Core и инструменты

- Для генерации/просмотра миграций используем дизайн‑тайм хост `RestoRate.Migrations` и инфраструктуру `RestoRate.<Context>.Infrastructure`.
- Документация и команды: см. `docs/migrations.md` (есть команды для списка и добавления миграций для сервиса Restaurant). Для корректной работы рекомендуется запускать через Aspire AppHost, чтобы были доступны БД и конфигурация.
