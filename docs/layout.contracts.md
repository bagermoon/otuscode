# RestoRate.Contracts — структура и рекомендации

Этот документ описывает рекомендуемую структуру проекта `RestoRate.Contracts` — пакета межсервисных контрактов (интеграционные события и переносимые DTO).

Цели:

- Однозначные, сериализуемые и версионируемые контракты для взаимодействия между сервисами.
- Упрощение миграции схем (поддержка нескольких версий одновременно).
- Минимальная зависимость от инфраструктуры и отсутствие бизнес-логики.

Текущая структура (по коду в репозитории) и рекомендации по расширению:

```fundamental
RestoRate.Contracts/
├── Restaurant/
│   ├── Dtos/
│   │   ├── RestaurantDto.cs               # плоские данные каталога
│   │   └── RestaurantDetailsDto.cs        # детали (опционально)
│   └── Events/
│       ├── RestaurantCreatedEvent.cs
│       ├── RestaurantUpdatedEvent.cs
│       └── RestaurantArchivedEvent.cs
├── Review/
│   ├── Dtos/
│   │   └── ReviewDto.cs                   # переносимый DTO отзыва (без доменных типов)
│   └── Events/
│       ├── ReviewAddedEvent.cs
│       └── ReviewApprovedEvent.cs
├── Moderation/
│   ├── Dtos/
│   │   └── ModerationTaskDto.cs           # опционально, если нужен обмен DTO
│   └── Events/
│       └── ReviewModeratedEvent.cs
└── Rating/
    ├── Dtos/
    │   └── RatingDto.cs                   # опционально, переносимая проекция
    └── Events/
        └── RestaurantRatingRecalculatedEvent.cs
```

Ключевые принципы:

- Простота вместо версионирования пространств имён: держим одну актуальную версию контрактов без `V1/V2` в путях и неймспейсах.
- Если потребуются несовместимые изменения — повышайте мажорную версию NuGet‑пакета и синхронно обновляйте потребителей.
- Контракты — только DTO и события; никакой бизнес‑логики.
- Зависимости: `RestoRate.Contracts` ссылается только на `RestoRate.Abstractions` (для маркерного интерфейса событий) и не зависит от доменных проектов.
- Сериализация: использовать простые record/POCO, избегать доменных типов и интерфейсов в полях; типы должны быть сериализуемыми (System.Text.Json по умолчанию).

Интерфейс маркера интеграционного события расположен в `RestoRate.Abstractions`:

```csharp
namespace RestoRate.Abstractions.Messaging;

public interface IIntegrationEvent { }
```

Пример события (из `Review/Events/ReviewAddedEvent.cs`):

```csharp
using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Review.Events;

public sealed record ReviewAddedEvent(
    Guid ReviewId,
    Guid RestaurantId,
    Guid AuthorId,
    int Rating,
    MoneyDto? AverageCheck,
    string? Comment,
    string[]? Tags) : IIntegrationEvent;
```

Замечания по распространению:

- Публикуйте единый NuGet‑пакет без параллельных `V1/V2` пространств имён и папок.
- Для больших несовместимых изменений — повышайте мажорную версию пакета и выполняйте согласованное обновление сервисов.
- Фиксируйте несовместимые изменения в CHANGELOG и добавляйте краткие инструкции миграции.

Тесты:

- Контрактные тесты (consumer‑driven или snapshot) для проверки сериализации и совместимости типов.

Дополнительно:

- При межъязыковой интеграции возможно вынесение схем (JSON Schema/Avro) в отдельный артефакт, но в текущей кодовой базе это не используется.

---
