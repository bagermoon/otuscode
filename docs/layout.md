# Структура монорепозитория и границы слоёв

Этот документ фиксирует текущую структуру репозитория и правила зависимостей между проектами. Детали по контрактам, тестированию, диаграммам и миграциям вынесены в отдельные документы:

- [Структура контрактов](./layout.contracts.md)
- [Тестирование](./testing.md)
- [Диаграммы и взаимодействия](./diagrams.md)
- [Миграции EF Core](./migrations.md)

## Структура репозитория

- `src/`
  - `RestoRate.AppHost` — Aspire-оркестратор: поднимает инфраструктуру и сервисы.
  - `RestoRate.ServiceDefaults` — общие hosting defaults: service discovery, resilience, telemetry, health.
  - `RestoRate.Contracts` — межсервисные DTO, запросы и интеграционные события.
  - `RestoRate.Abstractions` — application-level порты и общие интерфейсы: messaging, identity, pipeline behaviors.
  - `RestoRate.BuildingBlocks` — переиспользуемые инфраструктурные реализации и технические расширения.
  - `RestoRate.SharedKernel` — общие доменные примитивы и диагностические константы.
  - `RestoRate.Migrations` — design-time host для EF Core CLI.
  - `RestoRate.Gateway` — API gateway и token exchange.
  - `RestoRate.Auth` — общие примитивы аутентификации и работы с claims/JWT.
  - `RestoRate.BlazorDashboard` — UI.
  - Доменные контексты, каждый в 4 слоя:
    - `RestoRate.<Context>.Domain`
    - `RestoRate.<Context>.Application`
    - `RestoRate.<Context>.Infrastructure`
    - `RestoRate.<Context>.Api`
- `tests/`
  - unit- и integration-тесты по сервисам;
  - общий тестовый код в `RestoRate.Testing.Common`.

Примеры текущих контекстов: `RestaurantService`, `ReviewService`, `RatingService`, `ModerationService`.

## Правила зависимостей

- `Api` зависит от `Application`.
- `Application` зависит от `Domain`.
- `Infrastructure` зависит от `Application` и `Domain`.
- `Domain` не зависит от `Application`, `Infrastructure`, `Api` и не использует инфраструктурные библиотеки.
- `Domain` может использовать только собственный код сервиса и `RestoRate.SharedKernel`.
- `Application` и `Infrastructure` могут использовать `RestoRate.Abstractions`, `RestoRate.Contracts` и `RestoRate.SharedKernel` в допустимых сценариях.
- Сервисы не ссылаются на `Domain` или `Application` других сервисов. Межсервисный обмен идёт только через `RestoRate.Contracts`.
- `RestoRate.SharedKernel` не зависит от слоёв выше. `RestoRate.Abstractions` может зависеть от `RestoRate.SharedKernel`, но не наоборот.

## Общие пакеты

| Пакет | Где использовать | Назначение | Чего избегать |
| --- | --- | --- | --- |
| `RestoRate.SharedKernel` | в первую очередь `Domain`, при необходимости `Application` | общие доменные базовые типы, domain events, diagnostics | порты, инфраструктурные реализации, web/ORM/transports |
| `RestoRate.Abstractions` | `Api`, `Application`, `Infrastructure` | порты, identity, messaging contracts, pipeline behaviors | доменная логика и конкретные реализации |
| `RestoRate.Contracts` | `Api`, `Application`, `Infrastructure` | межсервисные DTO, запросы, интеграционные события | бизнес-логика и доменные типы |
| `RestoRate.BuildingBlocks` | `Infrastructure`, `Api` | технические реализации, DI-расширения, EF/messaging helpers | использование в `Domain`; широкое протаскивание в `Application` |
| `RestoRate.ServiceDefaults` | entrypoints (`Program.cs`, hosting setup) | discovery, resilience, telemetry, health | бизнес-логика |
| `RestoRate.Auth` | `Api`, `Gateway`, `UI`, общий auth setup | работа с JWT/claims и общие auth primitives | прямую интеграцию сервиса с внешним IdP |

## Ответственность слоёв

- `Api`
  - HTTP endpoints, auth/authz, OpenAPI, binding и преобразование входа в команды/запросы.
  - Вызов use case через Mediator/ISender.
  - Без прямой работы с БД, брокером сообщений и внешними провайдерами.
- `Application`
  - use cases, команды/запросы и их handlers;
  - orchestration домена и вызов портов;
  - pipeline behaviors и application-level policies.
  - Без EF Core, HTTP clients, брокеров и provider-specific кода.
- `Domain`
  - агрегаты, сущности, value objects, инварианты, доменные события.
  - Без зависимостей на ASP.NET Core, EF Core, MassTransit, Keycloak и другие фреймворки.
- `Infrastructure`
  - реализация портов из `Application`;
  - persistence, messaging, внешние клиенты, mapping к `Contracts`, DI wiring.
  - Без бизнес-логики, которая должна жить в `Domain` или `Application`.

## Именование и размещение кода

- Проекты именуются как `RestoRate.<Context>.<Layer>`.
- DI-расширения:
  - `Add<Context>Application()` — в `<Context>.Application`;
  - `Add<Context>Infrastructure()` — в `<Context>.Infrastructure`;
  - `Add<Context>Api(this IHostApplicationBuilder)` — в `<Context>.Api`.
- API endpoints размещаются в `Api/Endpoints/<Feature>`.
- Mediator-обработчики уведомлений и доменных событий размещаются в `Application/Handlers`.
- MassTransit `IConsumer<T>` размещаются в `Api/Consumers` и используют суффикс `Consumer`.

## Entry points и runtime

- Публичные entry points (`Gateway`, UI, Keycloak UI) публикуются через `WithExternalHttpEndpoints()`.
- Внутренние сервисы по умолчанию работают через service discovery и gateway.
- Каждый HTTP entry point должен подключать `builder.AddServiceDefaults();`.
- Для health endpoints используется `app.MapDefaultEndpoints();`.
- Подробности по инфраструктурным сценариям и настройке окружения см. в [docs/migrations.md](./migrations.md), [docs/testing.md](./testing.md) и [docs/diagrams.md](./diagrams.md).

## Короткий чек-лист

- Если нужен общий доменный базовый тип, это `RestoRate.SharedKernel`.
- Если нужен порт, который реализует инфраструктура, это `RestoRate.Abstractions`.
- Если нужен переносимый межсервисный контракт, это `RestoRate.Contracts`.
- Если код зависит от конкретного транспорта, БД или провайдера, он не должен попадать в `Domain`.
- Если сервису нужен другой сервис, связь должна идти через gateway, event-driven интеграцию или `RestoRate.Contracts`, а не через прямую ссылку на его внутренние слои.
