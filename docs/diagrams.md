# Архитектура

Ниже приведены актуальные схемы в формате Mermaid для текущего состояния репозитория. Они отражают AppHost-конфигурацию, текущие хранилища сервисов, gateway/token exchange и фактические интеграции через RabbitMQ/MassTransit.

## 1. Общая архитектура

```mermaid
flowchart LR
    subgraph Clients[Клиенты]
        BZ["Blazor Dashboard"]
        EXT["Внешний клиент"]
    end

    subgraph Public[Публичный периметр]
        KC["Keycloak"]
        GW["API Gateway\nYARP + Token Exchange"]
    end

    subgraph Services[Микросервисы]
        REST["Restaurant Service"]
        REV["Review Service"]
        RAT["Rating Service"]
        MOD["Moderation Service"]
    end

    subgraph Infra[Инфраструктура]
        PG["PostgreSQL\nrestaurantdb"]
        MG["MongoDB\nreviewdb + ratingdb"]
        RD["Redis\nrating cache + gateway output cache"]
        MQ["RabbitMQ\nintegration events"]
    end

    BZ --> GW
    EXT --> GW
    BZ -->|OIDC login| KC
    EXT -->|OIDC/JWT| GW
    GW -.->|token exchange| KC

    GW --> REST
    GW --> REV
    GW --> RAT
    GW --> MOD

    REST -.-> PG
    REV -.-> MG
    RAT -.-> MG
    RAT -.-> RD
    GW -.-> RD

    REST -.->|publish/consume| MQ
    REV -.->|publish/consume| MQ
    RAT -.->|publish/consume| MQ
    MOD -.->|consume/publish| MQ

    classDef svc fill:#2563eb,stroke:#0f172a,color:#fff
    classDef store fill:#f59e0b,stroke:#92400e,color:#111
    classDef client fill:#10b981,stroke:#065f46,color:#fff
    classDef gateway fill:#7c3aed,stroke:#3730a3,color:#fff
    classDef idp fill:#14b8a6,stroke:#0f766e,color:#fff

    class REST,REV,RAT,MOD svc
    class PG,MG,RD,MQ store
    class BZ,EXT client
    class GW gateway
    class KC idp
```

Примечания:

- `Moderation Service` в текущем AppHost не использует отдельное persistent storage.
- `Rating Service` использует `MongoDB` для собственных данных и `Redis` для кэша/дебаунса пересчёта.
- `Gateway` использует `Redis` для output cache.

## 2. Контейнерная схема

```mermaid
flowchart LR
    Client[Клиент] --> Gateway[API Gateway]
    Dashboard[Blazor Dashboard] --> Gateway
    Dashboard -.OIDC login.-> Keycloak[Keycloak]
    Gateway -.token exchange.-> Keycloak

    subgraph RestoRate[System: RestoRate]
        Gateway --> RestaurantSvc[Restaurant Service]
        Gateway --> ReviewSvc[Review Service]
        Gateway --> RatingSvc[Rating Service]
        Gateway --> ModerationSvc[Moderation Service]
    end

    RestaurantSvc -->|CRUD restaurants| PostgreSQL[(PostgreSQL)]
    ReviewSvc -->|reviews + sagas| ReviewMongo[(MongoDB: reviewdb)]
    RatingSvc -->|rating projections| RatingMongo[(MongoDB: ratingdb)]
    RatingSvc -->|cache + debounce| Redis[(Redis)]
    Gateway -->|output cache| Redis

    RestaurantSvc -.publish/consume.-> RabbitMQ[(RabbitMQ)]
    ReviewSvc -.publish/consume.-> RabbitMQ
    RatingSvc -.publish/consume.-> RabbitMQ
    ModerationSvc -.consume/publish.-> RabbitMQ
```

## 3. Поток: создание отзыва, валидация, модерация и пересчёт рейтинга

```mermaid
sequenceDiagram
    autonumber
    participant C as Клиент
    participant GW as API Gateway
    participant REV as Review Service
    participant REVDB as MongoDB reviewdb
    participant MQ as RabbitMQ
    participant REST as Restaurant Service
    participant MOD as Moderation Service
    participant RAT as Rating Service
    participant RATDB as MongoDB ratingdb
    participant REDIS as Redis

    C->>GW: POST /api/reviews
    GW->>REV: create review request
    REV->>REVDB: create review
    REV-->>GW: 201 Created
    GW-->>C: 201 Created

    REV->>MQ: Publish ReviewValidationRequested
    MQ->>REST: GetRestaurantStatusRequest
    REST-->>MQ: GetRestaurantStatusResponse
    MQ-->>REV: RestaurantValidationCompleted
    REV->>REV: Validate user and aggregate validation results

    alt Validation failed
        REV->>REVDB: mark review as rejected
        Note over REV: Do not publish ReviewRejectedEvent
    else Validation passed
        REV->>REVDB: move review to moderation pending
        REV->>MQ: Publish ReviewAddedEvent

        MQ->>MOD: Consume ReviewAddedEvent
        MOD->>MOD: moderate text
        MOD->>MQ: Publish ReviewModeratedEvent

        MQ->>REV: Consume ReviewModeratedEvent
        alt Approved
            REV->>REVDB: mark review approved
            REV->>MQ: Publish ReviewApprovedEvent
            MQ->>RAT: Consume ReviewApprovedEvent
        else Rejected
            REV->>REVDB: mark review rejected
            REV->>MQ: Publish ReviewRejectedEvent
            MQ->>RAT: Consume ReviewRejectedEvent
        end

        RAT->>RATDB: update rating projection
        RAT->>REDIS: refresh cache
        RAT->>MQ: Publish RestaurantRatingRecalculatedEvent
        MQ->>REST: Consume RestaurantRatingRecalculatedEvent
        REST->>PG: update restaurant rating snapshot
    end
```

## 4. Схема событий

Подробные потоки интеграционных событий описаны по сервисам:

- Restaurant Service: см. [RestaurantService.md](./services/RestaurantService.md#интеграционные-события)
- Review Service: см. [ReviewService.md](./services/ReviewService.md#интеграционные-события)
- Moderation Service: см. [ModerationService.md](./services/ModerationService.md#интеграционные-события)
- Rating Service: см. [RatingService.md](./services/RatingService.md#интеграционные-события)

## 5. Упрощённая доменная и интеграционная модель

```mermaid
classDiagram
    class Restaurant {
        RestaurantId Id
        string Name
        string Description
        Address Address
        string Cuisine
        RatingSnapshot Rating
        OpenHours OpenHours
        Tags Tags
        Location Location
        +DomainEvents
    }

    class Review {
        ReviewId Id
        RestaurantId RestaurantId
        UserId UserId
        int Rating
        string Comment
        Money SuggestedAverageCheck
        ReviewStatus Status
        string RejectionReason
        CreatedAt
    }

    class RatingSnapshot {
        RestaurantId RestaurantId
        decimal AverageRate
        Money AverageCheck
        int ReviewCount
        UpdatedAt
    }

    class RestaurantCreatedEvent
    class RestaurantUpdatedEvent
    class RestaurantArchivedEvent
    class ReviewAddedEvent
    class ReviewApprovedEvent
    class ReviewRejectedEvent
    class ReviewModeratedEvent
    class RestaurantRatingRecalculatedEvent

    Restaurant "1" <-- "many" Review : has
    Restaurant "1" --> "0..1" RatingSnapshot : snapshot

    Restaurant --> RestaurantCreatedEvent
    Restaurant --> RestaurantUpdatedEvent
    Restaurant --> RestaurantArchivedEvent
    Review --> ReviewAddedEvent
    Review --> ReviewApprovedEvent
    Review --> ReviewRejectedEvent

    ReviewAddedEvent ..> ReviewModeratedEvent : moderation outcome
    ReviewApprovedEvent ..> RestaurantRatingRecalculatedEvent : recalculate
    ReviewRejectedEvent ..> RestaurantRatingRecalculatedEvent : recalculate
    RestaurantRatingRecalculatedEvent ..> RatingSnapshot : update snapshot
```

Примечание: эта схема намеренно упрощена. Она показывает текущие ключевые сущности и интеграционные события, но не заменяет документацию по конкретным use case, saga и consumer-ам.
