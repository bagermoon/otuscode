# Архитектура
Ниже несколько вариантов схем в формате Mermaid (легко вставить в Markdown, GitHub, Obsidian, mkdocs), которые можно импортировать в draw.io (через плагин Mermaid) или преобразовать в Visio (экспорт → SVG → вставить в Visio). Я сделал несколько уровней: Общая архитектура, C4-подобный Container, Потоки (Sequence), Деплой/Инфраструктура, а также схема событий.

1. Общая архитектура (High-Level)
    ```mermaid
    flowchart LR
    %% Client Layer - Left
    subgraph ClientSide[Клиенты]
        direction LR
        BZ["`Blazor Dashboard<br/>(:5005)`"]
    end

    %% PublicAPI Layer - Left-Center (public)
    subgraph PublicAPI[Публичные API]
        direction TB
        KC["`Keycloak<br/>(IdP)`"]
        GW["`API Gateway<br/>(YARP)`"]
        
    end

    %% Service Layer - Center-Right
    subgraph Services[Микросервисы]
        direction TB
        RSVC["`Restaurant Service<br/>(:5001)`"]
        RS["`Review Service<br/>(:5002)`"]
        RT["`Rating Service<br/>(:5003)`"]
        MS["`Moderation Service<br/>(:5004)`"]
    end

    %% Infrastructure Layer - Right
    subgraph Infra["`Инфраструктура`"]
        direction TB
        subgraph DBs[Базы данных]
            direction TB
            PG["`PostgreSQL<br/>(restaurants)`"]
            MG["`MongoDB<br/>(reviews + moderation)`"]
            RD["`Redis<br/>(ratings cache)`"]
        end
        MQ["`RabbitMQ<br/>(events)`"]
    end

    %% Horizontal main flow (left to right)
    ClientSide --> GW
    ClientSide -->|OIDC login| KC
    GW -.->|token exchange / introspection| KC
    GW --> Services
    
    ClientSide -.- PublicAPI
    PublicAPI -.- Services
    Services -.- Infra

    %% Direct service-to-storage connections
    RSVC -.-> PG
    RS -.-> MG
    MS -.-> MG
    RT -.-> RD

    %% Event connections to message queue
    RS -.->|publish/consume| MQ
    RT -.->|consume| MQ
    MS -.->|consume/publish| MQ
    RSVC -.->|publish| MQ

    %% Styling
    classDef svc fill:#2563eb,stroke:#0f172a,color:#fff
    classDef store fill:#f59e0b,stroke:#92400e,color:#111
    classDef client fill:#10b981,stroke:#065f46,color:#fff
    classDef gateway fill:#7c3aed,stroke:#3730a3,color:#fff
    classDef idp fill:#14b8a6,stroke:#0f766e,color:#fff
    
    class RSVC,RS,RT,MS svc
    class PG,MG,RD,MQ store
    class BZ client
    class GW gateway
    class KC idp
    ```
1. Контейнерная (C4-подобная)
    ```mermaid
    flowchart LR
    actor(Client):::actor --> Gateway[API Gateway]
    actor(Client):::actor --> Dashboard[Blazor Dashboard]
    

    subgraph Boundary_System[System: RestoRate]
        Keycloak["`Keycloak<br/>(IdP)`"]
        Gateway --> RestaurantSvc[(Restaurant Service)]
        Gateway --> ReviewSvc[(Review Service)]
        Gateway --> RatingSvc[(Rating Service)]
        Gateway --> ModerationSvc[(Moderation Service)]
        
        Dashboard --> Gateway
    end

    %% Identity and token flows
    Dashboard -.OIDC login.-> Keycloak
    Gateway -.token exchange/introspection.-> Keycloak

    RestaurantSvc -->|CRUD Restaurants| PostgreSQL[(PostgreSQL)]
    ReviewSvc -->|CRUD Reviews| MongoDB[(MongoDB)]
    ModerationSvc -->|Tasks + Results| MongoDB
    RatingSvc -->|Cache Ratings| Redis[(Redis)]

    ReviewSvc -.events.-> RabbitMQ[(RabbitMQ)]
    RestaurantSvc -.events.-> RabbitMQ
    ModerationSvc -.events.-> RabbitMQ
    RatingSvc -.consume.-> RabbitMQ
    ModerationSvc -.consume.-> RabbitMQ
    ReviewSvc -.consume.-> RabbitMQ

    classDef actor fill:#fff,stroke:#000,font-weight:bold
    classDef db fill:#fde68a,stroke:#d97706
    classDef comp fill:#60a5fa,stroke:#1e3a8a,color:#fff
    classDef idp fill:#14b8a6,stroke:#0f766e,color:#fff
    class RestaurantSvc,ReviewSvc,RatingSvc,ModerationSvc,Gateway,Dashboard comp
    class Keycloak idp
    class PostgreSQL,MongoDB,Redis,RabbitMQ db
    ```
1. Поток: Добавление отзыва и модерация (Sequence)
    ```mermaid
    sequenceDiagram
        autonumber
        participant C as Клиент
        participant GW as API Gateway
        participant RS as Review Service
        participant MQ as RabbitMQ
        participant MS as Moderation Service
        participant RT as Rating Service
        participant DBR as MongoDB (reviews)
        participant DBM as MongoDB (moderation)
        participant RD as Redis (ratings)

        C->>GW: POST /api/restaurants/{id}/reviews
        GW->>RS: POST /reviews (restaurantId, text, rating)
        RS->>DBR: Insert Review (status=Pending)
        RS-->>C: 202 Accepted (На модерации)
        RS->>MQ: Publish ReviewAddedEvent

        MQ->>MS: Deliver ReviewAddedEvent
        MS->>MS: Авто-проверка текста
        MS->>DBM: Create ModerationTask (Pending или AutoApproved)
        alt AutoApproved
            MS->>MQ: Publish ReviewModeratedEvent(Approved)
        else Pending
            Note over MS: Ожидает действия модератора (UI)
        end

        MQ->>RS: (если Approved/Rejected) ReviewModeratedEvent
        RS->>DBR: Update Review Status
        RS->>MQ: Publish ReviewUpdatedEvent (status changed)

        MQ->>RT: ReviewAddedEvent (provisional rating)
        RT->>DBR: (опц.) Fetch Approved Reviews
        RT->>RD: Update Cached Rating (provisional)

        MQ->>RT: ReviewUpdatedEvent (final rating calculation)
        RT->>DBR: Fetch Approved Reviews
        RT->>RD: Recalculate & Cache (final)
        C->>GW: GET /api/restaurants/{id}/rating
        GW->>RT: GET rating
        RT->>RD: Get rating
        RT-->>GW: Rating DTO
        GW-->>C: 200 OK
    ```

2. Схема событий
   
    Подробные потоки интеграционных событий описаны по сервисам:

    - Restaurant Service: см. [RestoRate.RestaurantService.md](./services/RestoRate.RestaurantService.md#интеграционные-события)
    - Review Service: см. [ReviewService.md](./services/ReviewService.md#интеграционные-события)
    - Moderation Service: см. [ModerationService.md](./services/ModerationService.md#интеграционные-события)
    - Rating Service: см. [RatingService.md](./services/RatingService.md#интеграционные-события)

3. DDD Упрощённая модель (Entities + Events)
    ```mermaid
    classDiagram
        class Restaurant {
            RestaurantId Id
            string Name
            string Description
            Address Address
            string Cuisine
            Rating Rating
            Photos Photos
            OpenHours OpenHours
            Tags Tags
            Location Location
            UpdateRating(newRate, newCheck, newCount)
            UpdateAverageCheck(amount, currency)
            +DomainEvents
        }

        class Review {
            ReviewId Id
            RestaurantId RestaurantId
            UserId UserId
            string Text
            int Rate (1..10)
            string Comment
            Tags Tags
            Money SuggestedAverageCheck
            ReviewStatus Status
            string RejectionReason
            CreatedAt
        }

        class ModerationTask {
            ModerationTaskId Id
            ReviewId ReviewId
            string ReviewText
            int Rating
            RestaurantId RestaurantId
            UserId UserId
            ModerationStatus Status
            ModeratorId
            ModerationReason
            CreatedAt
        }

        class RatingSnapshot {
            RestaurantId RestaurantId
            decimal AverageRate
            Money AverageCheck
            int ReviewCount
            UpdatedAt
        }

        %% Domain Events
        class RestaurantCreatedEvent
        class RestaurantRatingUpdatedEvent
        class RestaurantAverageCheckUpdatedEvent
        class ReviewAddedEvent
        class ReviewUpdatedEvent
        class ReviewModeratedEvent

        %% Relationships
        Restaurant "1" <-- "many" Review : has
        Review "1" --> "0..1" ModerationTask : triggers moderation
        Restaurant "1" --> "0..1" RatingSnapshot : cached rating

        %% Event Sources
        Restaurant --> RestaurantCreatedEvent
        Restaurant --> RestaurantRatingUpdatedEvent
        Restaurant --> RestaurantAverageCheckUpdatedEvent
        Review --> ReviewAddedEvent
        Review --> ReviewUpdatedEvent
        ModerationTask --> ReviewModeratedEvent

        %% Event Flows (Services publish these)
        ReviewAddedEvent ..> ModerationTask : triggers
        ReviewAddedEvent ..> RatingSnapshot : provisional calculation
        ReviewModeratedEvent ..> Review : updates status (approved/rejected)
        ReviewUpdatedEvent ..> RatingSnapshot : recalculate with new values
    ```

        Примечание: блок выше показывает доменные события. Интеграционные контракты могут отличаться (например, доменное “RestaurantRatingUpdated” → интеграционное “RestaurantRatingRecalculatedEvent”). Фактические интеграционные потоки — в разделах «Интеграционные события» по сервисам.
