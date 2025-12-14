# Агрегаты сервиса Moderation

## ModerationTask

```csharp
namespace RestoRate.Moderation.Domain;

public sealed class ModerationTask : AggregateRoot<ModerationTaskId>
{
    private ModerationTask() { }

    private ModerationTask(
        ModerationTaskId id,
        ReviewId reviewId,
        RestaurantId restaurantId,
        UserId authorId,
        int rating,
        string comment,
        IReadOnlyCollection<string> tags)
    {
        Id = id;
        ReviewId = reviewId;
        RestaurantId = restaurantId;
        AuthorId = authorId;
        Rating = rating;
        Comment = comment;
        Tags = tags;
        Status = ModerationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static ModerationTask FromReviewSnapshot(ReviewCreatedSnapshot snapshot)
        => new(
            ModerationTaskId.New(),
            snapshot.ReviewId,
            snapshot.RestaurantId,
            snapshot.AuthorId,
            snapshot.Rating,
            snapshot.Text,
            snapshot.Tags);

    public ReviewId ReviewId { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
    public UserId AuthorId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> Tags { get; private set; } = Array.Empty<string>();
    public ModerationStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public string? ModeratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void AutoApprove(string automatedRuleId)
    {
        if (Status != ModerationStatus.Pending) return;
        ModeratorId = automatedRuleId;
        Status = ModerationStatus.Approved;
        DecisionReason = "Auto-approved";
        AddDomainEvent(new ReviewModeratedDomainEvent(ReviewId, RestaurantId, true, DecisionReason, ModeratorId));
    }

    public void Approve(string moderatorId, string? reason)
    {
        if (Status == ModerationStatus.Approved) return;
        Status = ModerationStatus.Approved;
        ModeratorId = moderatorId;
        DecisionReason = reason;
        AddDomainEvent(new ReviewModeratedDomainEvent(ReviewId, RestaurantId, true, reason, moderatorId));
    }

    public void Reject(string moderatorId, string reason)
    {
        Status = ModerationStatus.Rejected;
        ModeratorId = moderatorId;
        DecisionReason = reason;
        AddDomainEvent(new ReviewModeratedDomainEvent(ReviewId, RestaurantId, false, reason, moderatorId));
    }
}
```

## Интеграционные события

- Подписывается на: `ReviewAddedEvent`
- Публикует: `ReviewModeratedEvent`

```mermaid
flowchart LR
    %% Входящие события
    RV[Review Service] -- ReviewAddedEvent --> MQ[(RabbitMQ)]

    subgraph Moderation_Service[Moderation Service]
        MS[Workers/Handlers]
    end

    MQ --> MS

    %% Исходящее решение модерации
    MS -- ReviewModeratedEvent --> MQ
    MQ --> RV
    %% Rating reacts indirectly via ReviewUpdatedEvent

    %% Подсветка стрелок для визуализации семейств событий
    %% 0: RV->MQ (события Review)
    linkStyle 0 stroke:#10b981,stroke-width:2px
    %% 1: MQ->MS (доставка событий Review)
    linkStyle 1 stroke:#10b981,stroke-dasharray:4 2
    %% 2: MS->MQ (события Moderation)
    linkStyle 2 stroke:#7c3aed,stroke-width:2px
    %% 3: MQ->RV (доставка событий Moderation)
    linkStyle 3 stroke:#7c3aed,stroke-dasharray:4 2
```

### Примечания

- Зелёный — события Review (Added): публикация из Review и доставка в Moderation.
- Фиолетовый — событие Moderation (`ReviewModeratedEvent`): публикация из Moderation и доставка в Review.
- Пунктир — доставка события от RabbitMQ к потребителю; сплошная линия — публикация события в RabbitMQ.

## Последовательность событий (Sequence)

Ниже представлена последовательность обмена интеграционными событиями вокруг модерации отзыва.

```mermaid
sequenceDiagram
    autonumber
    participant RS as Review Service
    participant MQ as RabbitMQ (events)
    participant MS as Moderation Service
    participant RT as Rating Service

    RS->>MQ: Publish ReviewAddedEvent
    MQ->>MS: Deliver ReviewAddedEvent
    MS->>MS: Автоматическая проверка правил/фильтров
    alt Auto-Approved
        MS->>MQ: Publish ReviewModeratedEvent(Approved)
    else Требует модератора
        MS->>MS: Ожидание решения модератора (UI)
        MS->>MQ: Publish ReviewModeratedEvent(Approved/Rejected)
    end

    MQ->>RS: Deliver ReviewModeratedEvent
    RS->>MQ: Publish ReviewUpdatedEvent(status changed)

    note over RS,RT: Rating может реагировать как на ReviewAddedEvent (черновой расчёт),
    note over RS,RT: так и на ReviewUpdatedEvent (финализация)

    MQ->>RT: Deliver ReviewAddedEvent (optional)
    MQ->>RT: Deliver ReviewUpdatedEvent
```

### Замечания по надёжности

- Все публикации/доставки подразумевают ретраи и идемпотентность обработчиков.
- В случае сбоев Moderation Service не публикует событие до тех пор,
  пока решение по задаче модерации не будет зафиксировано (outbox/транзакции на уровне инфраструктуры).

