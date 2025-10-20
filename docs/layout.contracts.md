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
│   │   ├── V1/
│   │   │   ├── Dtos/
│   │   │   │   ├── RestaurantDto.cs
│   │   │   │   ├── RestaurantDetailsDto.cs
│   │   │   │   └── CreateRestaurantRequest.cs
│   │   │   └── Events/
│   │   │       ├── RestaurantCreatedEvent.cs
│   │   │       └── RestaurantUpdatedEvent.cs
│   │   └── V2/
│   │       └── ...
│   ├── Review/
│   │   ├── V1/
│   │   │   ├── Dtos/
│   │   │   └── Events/
│   │   └── V2/
│   ├── Rating/
│   │   └── V1/
│   └── Moderation/
│       └── V1/
├── src/Common/
│   ├── IntegrationEvent.cs
│   ├── IIntegrationEventHandler.cs
│   └── EventNames.cs
├── samples/           # Optional usage examples / snippets
└── README.md
```

Ключевые принципы:
- Версионирование по пространствам имён (`RestoRate.Contracts.Restaurant.V1`) и/или по папкам — позволяет иметь V1 и V2 в одном пакете.
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
        public const string Created = "review.created";
        public const string Updated = "review.updated";
        public const string Moderated = "review.moderated";
    }
}
```

Пример DTO (сверху в `Restaurant/V1/Dtos/RestaurantDto.cs`):

```csharp
namespace RestoRate.Contracts.Restaurant.V1.Dtos;

public sealed record RestaurantDto(
    Guid Id,
    string Name,
    string? Description,
    string? Address
);
```

Замечания по распространению:
- Можно публиковать единый NuGet-пакет, содержащий несколько версий (V1, V2) в разных пространствах имён.
- Для больших изменений — выпускать новую мажорную версию пакета и дополнительно оставить старые V-сборки внутри пакета до тех пор, пока миграция не завершится.
- Документировать несовместимые изменения в CHANGELOG и ссылаться на примеры миграции.

Тесты:
- Контрактные тесты (consumer-driven или snapshot tests) для проверки сериализации и совместимости.

Дополнительно:
- Рассмотреть отдельный `RestoRate.Contracts.Schema` (JSON Schema/Avro) если потребуется межъязыковая интеграция.

---
