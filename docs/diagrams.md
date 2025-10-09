# Архитектура
Ниже несколько вариантов схем в формате Mermaid (легко вставить в Markdown, GitHub, Obsidian, mkdocs), которые можно импортировать в draw.io (через плагин Mermaid) или преобразовать в Visio (экспорт → SVG → вставить в Visio). Я сделал несколько уровней: Общая архитектура, C4-подобный Container, Потоки (Sequence), Деплой/Инфраструктура, а также схема событий.

1. Общая архитектура (High-Level)
    ```mermaid
    flowchart LR
    %% Client Layer - Left
    subgraph ClientSide[Клиенты]
        direction LR
        W[Web / Mobile]
        BZ["`Blazor Dashboard<br/>(:5005)`"]
    end

    %% Gateway Layer - Left-Center
    GW["`API Gateway<br/>(YARP/Ocelot)`"]

    %% BFF Layer - Center
    subgraph BFFLayer[BFF Services]
        direction LR
        PBFF["`Public BFF<br/>(:5080)`"]
        ABFF["`Admin BFF<br/>(:5081)`"]
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
    GW --> BFFLayer
    GW --> Services
    BFFLayer --> Services
    Services --> Infra

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
    classDef bff fill:#dc2626,stroke:#7f1d1d,color:#fff
    
    class RSVC,RS,RT,MS svc
    class PG,MG,RD,MQ store
    class W,BZ client
    class GW gateway
    class PBFF,ABFF bff
    ```
1. Контейнерная (C4-подобная)
    ```mermaid
    flowchart LR
    actor(Client):::actor --> Gateway[API Gateway]
    actor(Client):::actor --> Dashboard[Blazor Dashboard]

    subgraph Boundary_System[System: RestoRate]
        Gateway --> PublicBFF[Public BFF]
        Gateway --> AdminBFF[Admin BFF]
        Gateway --> RestaurantSvc[(Restaurant Service)]
        Gateway --> ReviewSvc[(Review Service)]
        Gateway --> RatingSvc[(Rating Service)]
        Gateway --> ModerationSvc[(Moderation Service)]
        
        PublicBFF --> RestaurantSvc
        PublicBFF --> ReviewSvc
        PublicBFF --> RatingSvc
        
        AdminBFF --> RestaurantSvc
        AdminBFF --> ReviewSvc
        AdminBFF --> RatingSvc
        AdminBFF --> ModerationSvc
        
        Dashboard --> Gateway
    end

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
    classDef bff fill:#dc2626,stroke:#7f1d1d,color:#fff
    class RestaurantSvc,ReviewSvc,RatingSvc,ModerationSvc,Gateway,Dashboard comp
    class PublicBFF,AdminBFF bff
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

1. Поток: Получение карточки ресторана (Aggregated View)
    ```mermaid
    sequenceDiagram
        participant User
        participant GW as API Gateway
        participant AGG as Public BFF API
        participant RSVC as Restaurant Service
        participant RT as Rating Service
        participant RS as Review Service

        User->>GW: GET /api/restaurants/{id}/details
        GW->>AGG: GET /restaurants/{id}/details
        par Parallel fan-out
            AGG->>RSVC: GET /restaurants/{id}
            AGG->>RT: GET /restaurants/{id}/rating
            AGG->>RS: GET /restaurants/{id}/reviews?limit=10
        end
        AGG-->>GW: { restaurant, rating, reviews }
        GW-->>User: 200 OK
    ```

1. Схема событий (Топики / Routing Keys)
    ```mermaid
    flowchart LR
        subgraph Producers
            RSVC[Restaurant Service]
            RS[Review Service]
            MS[Moderation Service]
        end
        subgraph Exchange[RabbitMQ Events]
            R1{{restaurant.created}}
            R2{{review.created}}
            R3{{review.moderated}}
            R4{{review.updated}}
        end
        subgraph Consumers
                RS_C["Review Service"]
                MS_C[Moderation Service]
                RT_C[Rating Service]
        end

        RSVC --> R1
        RS --> R2
        RS --> R4
        MS --> R3

            R1 --> RS_C
            R2 --> MS_C
            R2 --> RT_C
            R3 --> RS_C
            R4 --> RT_C

        classDef prod fill:#2563eb,stroke:#1e3a8a,color:#fff
        classDef ex fill:#f59e0b,stroke:#92400e
        classDef cons fill:#10b981,stroke:#065f46,color:#fff
        class RSVC,RS,MS prod
        class R1,R2,R3,R4 ex
        class RS_C,MS_C,RT_C cons
    ```

1. DDD Упрощённая модель (Entities + Events)
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