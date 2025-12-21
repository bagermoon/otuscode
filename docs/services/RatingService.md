# Агрегаты сервиса Rating

## RestaurantRatingSnapshot

```csharp
namespace RestoRate.Rating.Domain;

public sealed class RestaurantRatingSnapshot : AggregateRoot<RestaurantId>
{
    private readonly Dictionary<ReviewId, ReviewScore> _scores = new();

    private RestaurantRatingSnapshot() { }

    private RestaurantRatingSnapshot(RestaurantId id)
    {
        Id = id;
        Current = RatingSummary.Empty();
    }

    public static RestaurantRatingSnapshot Create(RestaurantId restaurantId)
        => new(restaurantId);

    public RatingSummary Current { get; private set; }
    public RatingSummary Provisional { get; private set; } = RatingSummary.Empty();
    public IReadOnlyDictionary<ReviewId, ReviewScore> Scores => _scores;

    public void ApplyReviewCreated(ReviewId reviewId, int rating, bool isApproved)
    {
        _scores[reviewId] = new ReviewScore(rating, isApproved ? ReviewVisibility.Approved : ReviewVisibility.Pending);
        Recalculate();
    }

    public void ApplyReviewUpdated(ReviewId reviewId, int rating)
    {
        if (!_scores.TryGetValue(reviewId, out var entry)) return;
        _scores[reviewId] = entry with { Rating = rating };
        Recalculate();
    }

    public void ApplyReviewModerated(ReviewId reviewId, bool approved)
    {
        if (!_scores.TryGetValue(reviewId, out var entry)) return;
        _scores[reviewId] = entry with { Visibility = approved ? ReviewVisibility.Approved : ReviewVisibility.Rejected };
        Recalculate();
    }

    private void Recalculate()
    {
        Current = RatingSummary.From(_scores.Values.Where(s => s.Visibility == ReviewVisibility.Approved));
        Provisional = RatingSummary.From(_scores.Values.Where(s => s.Visibility != ReviewVisibility.Rejected));
        AddDomainEvent(new RestaurantRatingRecalculatedDomainEvent(Id, Current, Provisional));
    }
}
```

## ReviewScore и RatingSummary (вспомогательные типы)

```csharp
namespace RestoRate.Rating.Domain;

public sealed record ReviewScore(int Rating, ReviewVisibility Visibility);

public enum ReviewVisibility
{
    Pending,
    Approved,
    Rejected
}

public sealed record RatingSummary(decimal AverageRate, int ReviewCount, Money AverageCheck)
{
    public static RatingSummary Empty()
        => new(0m, 0, Money.Zero("RUB"));

    public static RatingSummary From(IEnumerable<ReviewScore> scores)
    {
        var list = scores.ToList();
        if (list.Count == 0) return Empty();
        var average = list.Average(s => s.Rating);
        return new RatingSummary((decimal)average, list.Count, Money.Zero("RUB"));
    }
}
```

## Интеграционные события

- Подписывается на: `ReviewAddedEvent`, `ReviewUpdatedEvent`
- Публикует: `RestaurantRatingRecalculatedEvent`

```mermaid
flowchart LR
    %% Входящие события от Review
    RV[Review Service] -- ReviewAddedEvent / ReviewUpdatedEvent --> MQ[(RabbitMQ)]
    MQ --> RT

    subgraph Rating_Service[Rating Service]
        RT[Workers/Handlers]
    end

    %% Исходящая проекция рейтинга
    RT -- RestaurantRatingRecalculatedEvent --> MQ
    MQ --> RS[Restaurant Service]

    %% Подсветка стрелок для визуализации семейств событий
    %% 0: RV->MQ (события Review)
    linkStyle 0 stroke:#10b981,stroke-width:2px
    %% 1: MQ->RT (доставка событий Review)
    linkStyle 1 stroke:#10b981,stroke-dasharray: 4 2

    %% 2: RT->MQ (события Rating)
    linkStyle 2 stroke:#2563eb,stroke-width:2px
    %% 3: MQ->RS (доставка событий Rating)
    linkStyle 3 stroke:#2563eb,stroke-dasharray: 4 2
```

### Примечания

- Зелёный — события Review (Added/Updated): публикация из Review и доставка в Rating.
- Синий — события Rating (`RestaurantRatingRecalculatedEvent`): публикация из Rating и доставка в Restaurant для проекции.
- Пунктир — доставка события от RabbitMQ к потребителю; сплошная линия — публикация события в RabbitMQ.

## Последовательность событий (Sequence)

Ниже показан порядок обработки событий рейтингом ресторана.

```mermaid
sequenceDiagram
    autonumber
    participant RV as Review Service
    participant MQ as RabbitMQ (events)
    participant RT as Rating Service
    participant DBR as MongoDB (reviews)
    participant RD as Redis (ratings cache)
    participant RS as Restaurant Service

    RV->>MQ: Publish ReviewAddedEvent
    MQ->>RT: Deliver ReviewAddedEvent
    RT->>DBR: (опц.) Fetch Approved Reviews
    RT->>RD: Update Cached Rating (provisional)

    RV->>MQ: Publish ReviewUpdatedEvent
    MQ->>RT: Deliver ReviewUpdatedEvent
    RT->>DBR: Fetch Approved Reviews
    RT->>RD: Recalculate & Cache (final)
    RT->>MQ: Publish RestaurantRatingRecalculatedEvent
    MQ->>RS: Deliver RestaurantRatingRecalculatedEvent (projection/update)
```

### Замечания по надёжности

- Обработчики событий идемпотентны; кэш в Redis обновляется атомарно.
- В случае временной недоступности Redis — выполняется повторная попытка, состояние может быть восстановлено из проекций/хранилища отзывов.
