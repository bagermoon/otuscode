---
title: RestoRate Contracts layout
---

# RestoRate.Contracts — структура и рекомендации

Этот документ описывает рекомендуемую структуру проекта `RestoRate.Contracts` — пакета межсервисных DTO и интеграционных событий.

Цели:
- Однозначные, сериализуемые и версионируемые контракты для взаимодействия между сервисами.
- Упрощение миграции схем (поддержка нескольких версий одновременно).
- Минимальная зависимость от инфраструктуры и отсутствие бизнес-логики.

Рекомендуемая структура (файлы/папки):

```
RestoRate.Contracts/
├── src/
│   ├── Restaurant/
│   │   ├── Dtos/
│   │   │   ├── RestaurantDto.cs
│   │   │   ├── RestaurantDetailsDto.cs
│   │   │   └── CreateRestaurantRequest.cs
│   │   └── Events/
│   │       ├── RestaurantCreatedEvent.cs
│   │       └── RestaurantUpdatedEvent.cs
│   ├── Review/
│   │   ├── Dtos/
│   │   └── Events/
│   ├── Rating/
│   │   └── ...
│   └── Moderation/
│       └── ...
├── src/Common/
│   ├── IntegrationEvent.cs
│   ├── IIntegrationEventHandler.cs
│   └── EventNames.cs
├── samples/           # Optional usage examples / snippets
└── README.md
```

Ключевые принципы:
- Простота вместо версионирования пространств имён: держим одну актуальную версию контрактов без `V1/V2` в путях и неймспейсах.
- Если потребуются несовместимые изменения — повышайте мажорную версию NuGet‑пакета и синхронно обновляйте потребителей.
- Контракты — только DTO и события; никакой бизнес‑логики.
- Избегать ссылок на доменные проекты; допускается только на `SharedKernel` types если это явно необходимо (рекомендуется минимизировать).
- Сериализация: использовать простые POCO, избегать интерфейсов в полях, явно задавать версии и типы (для JSON: System.Text.Json attributes или Newtonsoft, договориться в проекте).

Пример `EventNames.cs`:

```csharp
namespace RestoRate.Contracts.Common;

public static class EventNames
{
    public static class Restaurant
    {
        public const string Created = "restaurant.created";
        public const string Updated = "restaurant.updated";
    }

    public static class Review
    {
        public const string Added = "review.added";
        public const string Updated = "review.updated";
        public const string Moderated = "review.moderated";
    }
}
```

Пример DTO (сверху в `Restaurant/Dtos/RestaurantDto.cs`):

```csharp
namespace RestoRate.Contracts.Restaurant.Dtos;

public sealed record RestaurantDto(
    Guid Id,
    string Name,
    string? Description,
    string? Address
);
```

Замечания по распространению:
- Публикуйте единый NuGet‑пакет без параллельных `V1/V2` пространств имён и папок.
- Для больших несовместимых изменений — повышайте мажорную версию пакета и выполняйте согласованное обновление сервисов.
- Документировать несовместимые изменения в CHANGELOG и ссылаться на примеры миграции.

Тесты:
- Контрактные тесты (consumer-driven или snapshot tests) для проверки сериализации и совместимости.

Дополнительно:
- Рассмотреть отдельный `RestoRate.Contracts.Schema` (JSON Schema/Avro) если потребуется межъязыковая интеграция.

---
