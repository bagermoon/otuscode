# Агрегаты сервиса ReviewService

## Review

```csharp
namespace RestoRate.ReviewService.Domain;

public sealed class Review : AggregateRoot<ReviewId>
{
    private readonly List<ReviewStatusTransition> _history = new();

    private Review() { }

    private Review(
        ReviewId id,
        RestaurantId restaurantId,
        UserId authorId,
        int rating,
        string text,
        IEnumerable<string> tags,
        Money? suggestedAverageCheck)
    {
        Id = id;
        RestaurantId = restaurantId;
        AuthorId = authorId;
        Rating = rating;
        Comment = text;
        Tags = tags.ToArray();
        SuggestedAverageCheck = suggestedAverageCheck;
        Status = ReviewStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ReviewAddedDomainEvent(id, restaurantId, rating));
    }

    public static Review Create(
        ReviewId id,
        RestaurantId restaurantId,
        UserId authorId,
        int rating,
        string text,
        IEnumerable<string> tags,
        Money? suggestedAverageCheck = null)
        => new(id, restaurantId, authorId, rating, text, tags, suggestedAverageCheck);

    public RestaurantId RestaurantId { get; private set; }
    public UserId AuthorId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public string[] Tags { get; private set; } = Array.Empty<string>();
    public Money? SuggestedAverageCheck { get; private set; }
    public ReviewStatus Status { get; private set; }
    public string? ModerationReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void UpdateContent(int rating, string text, IEnumerable<string> tags, Money? suggestedAverageCheck)
    {
        Rating = rating;
        Comment = text;
        Tags = tags.ToArray();
        SuggestedAverageCheck = suggestedAverageCheck;
        AddDomainEvent(new ReviewUpdatedDomainEvent(Id, RestaurantId, rating));
    }

    public void ApplyModerationDecision(ReviewStatus status, string? reason, string moderatorId)
    {
        if (Status == status) return;

        Status = status;
        ModerationReason = reason;
        AddDomainEvent(new ReviewModeratedDomainEvent(Id, RestaurantId, status, reason, moderatorId));
    }
}
```

## Интеграционные события

- Публикует: `ReviewAddedEvent`, `ReviewApprovedEvent`, `ReviewRejectedEvent`
- Подписывается на: `ReviewModeratedEvent` (публикуется сервисом ModerationService),
  `RestaurantCreatedEvent`, `RestaurantUpdatedEvent`, `RestaurantArchivedEvent`

```mermaid
flowchart LR
    %% Справочные события от Restaurant
    RSVC[Restaurant Service] -- RestaurantCreatedEvent / RestaurantUpdatedEvent / RestaurantArchivedEvent --> MQ[(RabbitMQ)]
    MQ --> RV

    %% Исходящие события сервиса Review
    subgraph Review_Service[Review Service]
        RV[API/Application]
    end

    RV -- ReviewAddedEvent / ReviewApprovedEvent / ReviewRejectedEvent --> MQ[(RabbitMQ)]

    MQ --> ModSvc[Moderation Service]

    MQ --> RatingSvc[Rating Service]

    %% Входящие результаты модерации
    ModSvc -- ReviewModeratedEvent --> MQ
    MQ --> RV

    %% Link styling to visualize event families (order-dependent)
    %% 0: RSVC->MQ (restaurant events)
    linkStyle 0 stroke:#f59e0b,stroke-width:2px
    %% 1: MQ->RV (restaurant events delivery)
    linkStyle 1 stroke:#f59e0b,stroke-dasharray: 4 2

    %% 2: RV->MQ (review events)
    linkStyle 2 stroke:#10b981,stroke-width:2px
    %% 3: MQ->ModSvc (review events to Moderation)
    linkStyle 3 stroke:#10b981,stroke-dasharray: 4 2
    %% 4: MQ->RatingSvc (review events to Rating)
    linkStyle 4 stroke:#10b981,stroke-dasharray: 4 2

    %% 5: ModSvc->MQ (moderation result)
    linkStyle 5 stroke:#7c3aed,stroke-width:2px
    %% 6: MQ->RV (moderation result delivery)
    linkStyle 6 stroke:#7c3aed,stroke-dasharray: 4 2
```

### Примечания

- Review Service поддерживает локальную проекцию «разрешённых ресторанов»,
    синхронизируемую событиями `RestaurantCreatedEvent` / `RestaurantUpdatedEvent` / `RestaurantArchivedEvent`.

- При создании отзыва сервис валидирует `RestaurantId` по этой проекции:
    отзыв можно добавить только для существующего и не архивированного ресторана.

Примечание по цветам стрелок:

- Оранжевый — события Restaurant (Created/Updated/Archived)
- Зелёный — события Review (Added/Approved)
- Фиолетовый — событие Moderation (ReviewModerated)
Пунктир — доставка события от RabbitMQ к потребителю; сплошная линия — публикация события.

## Последовательность событий (Sequence)

Последовательность создания и модерации отзыва с точки зрения Review Service.

```mermaid
sequenceDiagram
    autonumber
    participant C as Клиент
    participant GW as API Gateway
    participant RV as Review Service
    participant DBR as MongoDB (reviews)
    participant MQ as RabbitMQ (events)
    participant MS as Moderation Service
    participant RT as Rating Service

    C->>GW: POST /api/restaurants/{id}/reviews
    GW->>RV: POST /reviews (restaurantId, text, rating)
    RV->>DBR: Insert Review (status=Pending)
    RV-->>C: 202 Accepted (На модерации)
    RV->>MQ: Publish ReviewAddedEvent

    MQ->>MS: Deliver ReviewAddedEvent
    MS->>MS: Авто-проверка/или ожидание решения модератора
    MS->>MQ: Publish ReviewModeratedEvent(Approved/Rejected)

    MQ->>RV: Deliver ReviewModeratedEvent
    RV->>DBR: Update Review Status
    RV->>MQ: Publish ReviewApprovedEvent / ReviewRejectedEvent (final result)

    note over RV,RT: Rating может потреблять ReviewAddedEvent (черновой расчёт)
    note over RV,RT: и ReviewApprovedEvent / ReviewRejectedEvent (финальная фиксация/откат рейтинга)
    MQ->>RT: Deliver ReviewAddedEvent / ReviewApprovedEvent / ReviewRejectedEvent
```

### Saga Обновление ресторана

```mermaid
sequenceDiagram
    participant RS as RestaurantService
    participant MQ as RabbitMQ
    participant RVS as ReviewValidationSaga (by ReviewId)
    participant RstVS as RestaurantValidationSaga (by RestaurantId)
    participant RefC as RestaurantRefConsumer (projection)
    participant DB as ReviewService DB (RestaurantReference)

    RVS->>MQ: ReviewAddedEvent(reviewId, restaurantId)
    MQ->>RstVS: ReviewAddedEvent

    RstVS->>DB: Query RestaurantReference(restaurantId)
    alt status known + acceptable
    RstVS->>MQ: ValidationOk(reviewId, restaurantId)
    else missing/unknown
    RstVS->>RS: GetRestaurantStatusRequest(restaurantId)
    RstVS->>RstVS: Schedule ValidationTimeout(restaurantId)
    RS-->>RstVS: GetRestaurantStatusResponse(exists, status)
    RstVS->>DB: Upsert RestaurantReference(restaurantId,status)
    RstVS->>MQ: ValidationOk/ValidationFailed(for each pending reviewId)
    end

    RS->>MQ: RestaurantCreated/Updated/Archived(restaurantId,status)
    MQ->>RefC: Restaurant*Event
    RefC->>DB: Upsert RestaurantReference(restaurantId,status)

    MQ->>RVS: ValidationOk(reviewId, restaurantId)
    RVS->>MQ: ReviewReadyForModerationEvent(reviewId, restaurantId)
```

### Замечания по надёжности

- Публикации осуществляются через outbox; потребители — идемпотентны.
- Временные сбои в ModerationService или RatingService не блокируют основную операцию создания отзыва — события будут доставлены повторно.
