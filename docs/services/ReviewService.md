# Агрегаты сервиса Review

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
        AppendStatus(Status, null, "initial submission");
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
    public IReadOnlyCollection<ReviewStatusTransition> History => _history.AsReadOnly();

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
        AppendStatus(status, moderatorId, reason);
        AddDomainEvent(new ReviewModeratedDomainEvent(Id, RestaurantId, status, reason, moderatorId));
    }

    private void AppendStatus(ReviewStatus status, string? actorId, string? comment)
    {
        _history.Add(new ReviewStatusTransition(status, DateTimeOffset.UtcNow, actorId, comment));
    }
}
```

## Интеграционные события

- Публикует: `ReviewAddedEvent`, `ReviewUpdatedEvent`
- Подписывается на: `ReviewModeratedEvent` (публикуется сервисом Moderation),
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

    RV -- ReviewAddedEvent / ReviewUpdatedEvent --> MQ[(RabbitMQ)]

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
- Зелёный — события Review (Added/Updated)
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
    RV->>MQ: Publish ReviewUpdatedEvent (status changed)

    note over RV,RT: Rating может потреблять ReviewAddedEvent (черновой расчёт)
    note over RV,RT: и ReviewUpdatedEvent (финальная фиксация рейтинга)
    MQ->>RT: Deliver ReviewAddedEvent / ReviewUpdatedEvent
```

### Замечания по надёжности

- Публикации осуществляются через outbox; потребители — идемпотентны.
- Временные сбои в Moderation или Rating не блокируют основную операцию создания отзыва — события будут доставлены повторно.
