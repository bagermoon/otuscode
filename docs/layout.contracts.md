# RestoRate.Contracts — структура и рекомендации

Этот документ описывает текущую структуру проекта `RestoRate.Contracts` и правила для её расширения. Пакет используется для межсервисных контрактов: интеграционных событий, переносимых DTO и отдельных request/response моделей.

## Что хранить в `RestoRate.Contracts`

- DTO для обмена между сервисами.
- Интеграционные события.
- Request/response модели, если они нужны для межсервисного взаимодействия.
- Общие переносимые примитивы, не завязанные на доменную модель конкретного сервиса.

В `RestoRate.Contracts` не должно быть бизнес-логики, поведения агрегатов, EF-специфики, web-specific типов и ссылок на доменные проекты сервисов.

## Текущая структура

```fundamental
RestoRate.Contracts/
├── Common/
│   ├── Dtos/
│   │   ├── MoneyDto.cs
│   │   └── TagDto.cs
│   └── PagedResult.cs
├── Moderation/
│   └── Events/
│       └── ReviewModeratedEvent.cs
├── Rating/
│   ├── Dtos/
│   │   └── RestaurantRatingDto.cs
│   └── Events/
│       └── RestaurantRatingRecalculatedEvent.cs
├── Restaurant/
│   ├── DTOs/
│   │   ├── AddressDto.cs
│   │   ├── LocationDto.cs
│   │   ├── OpenHoursDto.cs
│   │   ├── RatingDto.cs
│   │   ├── RestaurantDto.cs
│   │   ├── RestaurantImageDto.cs
│   │   └── CRUD/
│   │       ├── CreateRestaurantDto.cs
│   │       ├── CreateRestaurantImageDto.cs
│   │       ├── ModerationRestaurantDto.cs
│   │       └── UpdateRestaurantDto.cs
│   ├── Events/
│   │   ├── RestaurantArchivedEvent.cs
│   │   ├── RestaurantCreatedEvent.cs
│   │   └── RestaurantUpdatedEvent.cs
│   ├── Requests/
│   │   ├── GetRestaurantStatusRequest.cs
│   │   └── GetRestaurantStatusResponse.cs
│   └── RestaurantStatus.cs
└── Review/
    ├── Dtos/
    │   ├── CreateReviewDto.cs
    │   ├── ReviewDto.cs
    │   └── UserReferenceDto.cs
    ├── Events/
    │   ├── ReviewAddedEvent.cs
    │   ├── ReviewApprovedEvent.cs
    │   └── ReviewRejectedEvent.cs
    └── ReviewStatus.cs
```

Примечание: в текущем репозитории есть смешение `Dtos` и `DTOs`. Это отражает текущее состояние кода. При расширении существующего раздела сохраняйте текущий namespace и структуру конкретного bounded context, а не переименовывайте папки точечно внутри одного изменения.

## Правила расширения

- Держим одну актуальную версию контрактов без `V1`/`V2` в путях и namespaces.
- Для несовместимых изменений повышаем мажорную версию пакета и синхронно обновляем потребителей.
- Контракты должны оставаться сериализуемыми через `System.Text.Json` без специальных доменных конвертеров.
- Используем простые `record` или POCO.
- Не переносим в Contracts доменные value objects, сущности, репозитории и application services.
- `RestoRate.Contracts` может зависеть от `RestoRate.Abstractions` для маркерных интерфейсов событий, но не от доменных проектов сервисов.

## Маркер интеграционного события

Интерфейс маркера интеграционного события расположен в `RestoRate.Abstractions`:

```csharp
namespace RestoRate.Abstractions.Messaging;

public interface IIntegrationEvent { }
```

Пример события:

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

## Практические рекомендации

- Новые контракты группируйте по bounded context, а не по техническому типу на верхнем уровне.
- Если контракт используется несколькими контекстами и не принадлежит одному из них, размещайте его в `Common`.
- Если рядом с DTO нужен межсервисный запрос/ответ, допустима отдельная папка `Requests`, как в `Restaurant`.
- При больших несовместимых изменениях фиксируйте миграцию в CHANGELOG и обновляйте все потребители согласованно.

## Тесты

- Контрактные тесты (consumer-driven или snapshot) для проверки сериализации и совместимости типов.

## Дополнительно

- При межъязыковой интеграции возможно вынесение схем (JSON Schema/Avro) в отдельный артефакт, но в текущей кодовой базе это не используется.
