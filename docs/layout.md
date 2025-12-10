# Структура монорепозитория для микросервисов по DDD (с .NET Aspire и Blazor)

Документ фиксирует рекомендуемую структуру проектов и правила разделения ответственности между `ServiceDefaults`, `Contracts`, `Abstractions`, `BuildingBlocks`, `SharedKernel`, `Common` и константами в контексте .NET 9, .NET Aspire (AppHost, ServiceDefaults), Gateway (YARP) и Blazor Server Dashboard.

## Цели
- Чёткие границы между доменом и инфраструктурой.
- Единый подход к service discovery и устойчивым HTTP‑клиентам (через ServiceDefaults).
- Повторно используемые, но корректно изолированные общие компоненты.
- Удобная локальная разработка и запуск через Aspire AppHost.

## Как принимать решение (быстрый чек‑лист)
- Общий доменный базовый тип/утилита для нескольких сервисов → `RestoRate.SharedKernel`.
- Порт/интерфейс для реализации инфраструктурой (репозиторий, шина, время, контекст пользователя) → `RestoRate.Abstractions` (а также application‑pipeline behaviors — валидация/логирование на базе Mediator и logging abstractions).
- Переносимый «wire»‑контракт (HTTP DTO, интеграционное событие) между сервисами → `RestoRate.Contracts`.

## Базовая структура репозитория

- src/
  - `RestoRate.AppHost` — оркестратор Aspire (Keycloak, RabbitMQ, Gateway, UI и доменные сервисы).
  - `RestoRate.ServiceDefaults` — настройки хостинга: service discovery, resilience HttpClient, OpenTelemetry, health.
  - `RestoRate.Contracts` — межсервисные DTO и интеграционные события (версионирование, сериализация). [Структура и рекомендации](./layout.contracts.md)
  - `RestoRate.Abstractions` — интерфейсы/примитивы и application‑pipeline behaviors (напр. `Messaging.IIntegrationEventBus`, Mediation behaviors). Без зависимостей от транспорта/ORM/веб‑фреймворков; допускается зависимость от Mediator и `Microsoft.Extensions.Logging.Abstractions`.
  - `RestoRate.BuildingBlocks` — переиспользуемые инфраструктурные реализации (MassTransitEventBus, миграции/сидеры EF, resilience middleware).
  - `RestoRate.SharedKernel` — доменные строительные блоки (entity base, value object, domain event, Result).
  - `RestoRate.Migrations` — дизайн‑тайм хост для EF Core (используется как `--startup-project` для CLI и в Aspire при выполнении миграций). Хранит только запуск и DI, без доменной логики.
  - `RestoRate.Gateway` — шлюз (YARP) + токен exchange.
  - `RestoRate.BlazorDashboard` — Blazor Server UI.
  - Доменные сервисы по границам (Bounded Contexts), каждый в 4 слоя:
    - `RestoRate.<Context>.Domain`
    - `RestoRate.<Context>.Application`
    - `RestoRate.<Context>.Infrastructure`
    - `RestoRate.<Context>.Api`
  - Текущий прогресс по контекстам:
    - `Restaurant` — все 4 слоя присутствуют.
    - `Moderation`, `Rating` — пока есть слой `Api` (заготовки); остальные планируются.
    - `Review` — в планах.
- tests/
  - Юнит‑тесты домена/приложения и интеграционные тесты инфраструктуры (с Testcontainers).

Примеры контекстов: `Restaurant`, `Review`, `Rating`, `Moderation`.

### Зависимости между слоями (строго)
- `Api` → `Application` → `Domain`.
- `Infrastructure` → `Application` + `Domain`.
- `Application` и `Infrastructure` могут ссылаться на `RestoRate.Abstractions` и `RestoRate.SharedKernel`; `Domain` остаётся зависимым только от `RestoRate.SharedKernel`.
- `RestoRate.Abstractions` допускает зависимость от `RestoRate.SharedKernel`, но сам `SharedKernel` не ссылается на слои выше.
- Сервисы не ссылаются на `Domain/Application` других сервисов. Межсервисный обмен — только через `Contracts`.

### Общие пакеты (границы использования)
  - `RestoRate.Abstractions` — допустимо в слоях `Api`, `Application`, `Infrastructure`; НЕ в `Domain`. Пакет может ссылаться на `RestoRate.SharedKernel`, если портам нужны доменные базовые типы.
  - `RestoRate.BuildingBlocks` — только `Api` и `Infrastructure`; НЕ в `Domain`; в `Application` только если тип не тянет инфраструктуру (редко).
  - `RestoRate.ServiceDefaults` — в каждом `Program.cs` внешних точек входа.
  - `RestoRate.Contracts` — в `Api`, `Application`, `Infrastructure`; НЕ в `Domain`.
  - `RestoRate.SharedKernel` — первоочередно для доменных проектов; может использоваться в `Application`, если необходимо работать с доменными событиями, при этом пакет не ссылается на слои выше.

## Ответственность слоёв (Layer Responsibilities)

- Api (внешний слой сервиса)
  - Endpoints (Controllers/Minimal APIs/SignalR) и HTTP‑контракты сервиса.
  - Аутентификация/авторизация (Keycloak/JWT), политики/роллинг‑зоны доступа.
  - Валидация входа на уровне API (model binding, basic validation) и преобразование в команды/запросы Application.
  - Делегирование бизнес‑действий через Mediator: только `Send()`/`Publish()` без бизнес‑логики.
  - Версионирование HTTP‑контракта (если нужно), OpenAPI в Dev, `app.MapDefaultEndpoints()` для health.
  - Никаких доступов к БД/шинам/внешним API напрямую — только через Application.

- Application (use‑cases, orchestration)
  - Команды и запросы (Mediator) + их обработчики: бизнес‑правила сценариев и координация домена.
  - Порты/интерфейсы для инфраструктуры (например, репозитории, брокеры сообщений, внешние API, кэш).
  - Транзакционные границы use‑case’ов; применение доменных событий/правил; идемпотентность при необходимости.
  - Pipeline Behaviors (валидация, логирование, ретраи, performance) — без привязки к конкретной инфраструктуре.
  - DTO для входа/выхода use‑case’ов (внутренние для сервиса). Межсервисные DTO — только из `RestoRate.Contracts`.
  - Никаких EF/RabbitMQ/HTTP‑клиентов напрямую: только через абстракции.

- Domain (чистый домен)
  - Сущности/агрегаты, Value Objects, доменные сервисы, доменные события, инварианты.
  - Полная независимость от инфраструктуры и фреймворков; только .NET/язык.
  - Логика изменения состояния и генерация доменных событий (агрегаты регистрируют события, Application публикует их после успешной транзакции через адаптер к Mediator); отсутствие побочных эффектов наружу.

- Infrastructure (адаптеры и реализации портов)
  - Реализации портов Application: репозитории (EF Core/Dapper), клиенты внешних сервисов/HTTP, кэш, брокер сообщений.
  - Маппинг к `RestoRate.Contracts` для публикации/чтения интеграционных событий.
  - Транспорт/провайдер‑специфика: подключения, миграции, конфигурация хранилищ, Outbox/Inbox, ретраи.
  - Регистрация DI (composition root) и интеграция с `RestoRate.ServiceDefaults` (resilience, discovery, telemetry).
  - Никакой бизнес‑логики; только технические детали и маппинг к домену/Application.

## Общие пакеты

### Матрица использования пакетов

| Пакет           | Domain | Application | Infrastructure | Api | Содержит                                 | Избегать                                   |
|-----------------|:------:|:-----------:|:--------------:|:---:|-------------------------------------------|--------------------------------------------|
| SharedKernel    |   ✔    |      ◐      |       ✖        |  ✖  | Доменные базовые типы/утилиты; Diagnostics IDs/sources | Порты/интерфейсы, инфраструктурные детали  |
| Abstractions    |   ✖    |      ✔      |       ✔        |  ✔  | Порты/интерфейсы, application pipeline behaviors | Доменные базовые типы, конкретные реализации|
| Contracts       |   ✖    |      ✔      |       ✔        |  ✔  | Wire DTO/интеграционные события           | Бизнес‑логика                               |
| BuildingBlocks  |   ✖    |     ◐       |       ✔        |  ✔  | Инфраструктурные реализации/DI‑расширения | Доменные типы                               |
| ServiceDefaults |   ✖    |      ✖      |       ✔        |  ✔  | Хостинг, discovery, resilience, telemetry | Бизнес‑логика                               |

Примечание: ◐ — допускается редко и только если тип не тянет инфраструктуру. `Application` может брать отдельные базовые типы из `SharedKernel` (например, доменные события) без формирования обратных зависимостей.

### Auth (Общий проект аутентификации)

`RestoRate.Auth` — это общий проект, предоставляющий примитивы и утилиты для аутентификации, используемые всеми частями приложения (API, Gateway, UI и др.).

- Не является самостоятельным сервисом или точкой входа.
- Не взаимодействует напрямую с Keycloak или внешними IdP.
- Используется для реализации проверки токенов, ролей, и других аспектов аутентификации во всех сервисах.
- Gateway отвечает за интеграцию с Keycloak и обмен токенов, а сервисы используют `RestoRate.Auth` для доверенной обработки JWT.

Рекомендуется ссылаться на этот проект во всех сервисах, где требуется аутентификация или авторизация, чтобы обеспечить единообразие и повторное использование логики.

### Abstractions и BuildingBlocks

Эволюция: прежние `RestoRate.Application`/`RestoRate.Infrastructure` заменены на более чёткие:

- `RestoRate.Abstractions`
  - Декларация портов/контрактов (например, `Messaging.IIntegrationEventBus`, `IIntegrationEvent`) и application‑pipeline behaviors (валидация, логирование) на базе Mediator.
  - Без зависимостей от транспортов/ORM/веб‑фреймворков; допускаются Mediator и logging abstractions.
- `RestoRate.BuildingBlocks`
  - Реализации и DI‑расширения провайдеров (MassTransit/RabbitMQ), миграции/сидеры EF, технические middleware.
  - Примеры:
    - `Messaging.MassTransitEventBus`, `Messaging.MassTransitExtensions` (читает `ConnectionStrings:RabbitMQ`, использует `AppHostProjects.RabbitMQ`).
    - `Migrations.AddMigration<TContext, TSeeder>()` и `IDbSeeder<TContext>`.

Границы:
- `Abstractions` не тянут инфраструктуру и не попадают в Domain.
- `BuildingBlocks` не просачиваются в Domain и минимально используются в Application.

## SharedKernel vs Abstractions vs Contracts — различия и правила

- Назначение:
  - `RestoRate.SharedKernel`: общие доменные строительные блоки (чистый код домена), переиспользуемые в разных bounded context‑ах.
  - `RestoRate.Abstractions`: кросс‑срезовые порты/интерфейсы и небольшие контракты, реализуемые инфраструктурой, чтобы избежать жёсткой связности.

- Содержимое (ориентиры):
  - `SharedKernel`: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent` и базовые реализации доменных событий, `Result/Maybe`, Guard/Specification‑утилиты, простые доменные исключения.
  - `Abstractions`: интерфейсы шины и сообщений (`Messaging.IIntegrationEventBus`, `IIntegrationEvent`), `IClock`, `IUserContext`, контракты репозиториев/юнита работы, маркерные интерфейсы/атрибуты; по необходимости могут ссылаться на типы из `SharedKernel`, но не на конкретные домены.

- Зависимости и область применения:
  - `SharedKernel`: без зависимостей от ASP.NET/EF/MassTransit и т.п.; используется в проектах слоя `...Domain` и, при необходимости, в `Application` для работы с доменными событиями/базовыми типами.
  - `Abstractions`: без инфраструктурных реализаций и фреймворков; допустим в `Api`, `Application`, `Infrastructure`; НЕ в `Domain`. Пакет может зависеть от `SharedKernel`, но не наоборот.

- Чего НЕ должно быть:
  - В `SharedKernel`: никаких интерфейсов, требующих реализации инфраструктурой; никаких межсервисных DTO/«wire»‑контрактов.
  - В `Abstractions`: никаких базовых доменных типов и логики; никаких конкретных реализаций/клиентов.
  - В обоих: отсутствие внешних транспортов/ORM/веб‑зависимостей.

- Связь с `RestoRate.Contracts`:
  - `Contracts` держит публичные «wire»‑модели (HTTP DTO, интеграционные события) для обмена между сервисами.
  - `SharedKernel` предоставляет доменные основы внутри границ сервиса.
  - `Abstractions` объявляет порты (например, `IIntegrationEventBus`, репозитории), которые реализует слой `Infrastructure`.
  - Межсервисный обмен выполняется ТОЛЬКО через `Contracts`.

- Константы и диагностика:
  - Доменные инварианты размещайте в конкретном `...Domain`; если они действительно обще‑доменные — в `SharedKernel`.
  - Диагностические константы — в `RestoRate.SharedKernel.Diagnostics`: `ActivitySources` (строки) и `LoggingEventIds` (int). Преобразование в `EventId` — на местах использования.
  - Избегайте «магических» строк: группируйте в единые константы и не смешивайте доменные константы с инфраструктурными.

### Именование DI‑расширений
- Application DI: `Add<Context>Application()` в `<Context>.Application` (пространство имён `Microsoft.Extensions.DependencyInjection` или используемое из проекта).
- API host: `Add<Context>Api(this IHostApplicationBuilder)` в `<Context>.Api` (пространство имён `Microsoft.Extensions.Hosting`).
- Папка с обработчиками событий/уведомлений в Application — `Handlers`.

- Быстрые правила выбора:
  - Нужен общий доменный базовый тип/утилита? → `SharedKernel`.
  - Нужен интерфейс/порт, который реализует инфраструктура? → `Abstractions`.
  - Нужен переносимый контракт между сервисами? → `Contracts`.

<!-- Объединено в раздел "SharedKernel vs Abstractions vs Contracts — различия и правила" -->

### Доменные события

- Агрегаты регистрируют события через базовые типы `SharedKernel` (например, `RegisterDomainEvent`).
- После успешного сохранения (`Application`/`Infrastructure`) события выгружаются и передаются в адаптер Mediator (Dispatcher), реализованный на стороне `Application`.
- `Domain` не зависит от Mediator или других фреймворков; адаптация происходят выше слоя.
- При добавлении новых событий держите синхронизацию с обработчиками `Application` и, при необходимости, интеграционными событиями в `Contracts`.

## Runtime и инфраструктура

## Aspire: внутренние и внешние эндпойнты
- Публичные точки входа (Gateway, Blazor UI, Keycloak UI) помечайте как внешние:
  - `builder.AddProject<...>("gateway").WithExternalHttpEndpoints();`
  - `builder.AddProject<...>("blazor-dashboard").WithExternalHttpEndpoints();`
  - `builder.AddKeycloak(...).WithExternalHttpEndpoints();`
- Внутренние доменные сервисы обычно не публикуются наружу; доступны через service discovery и Gateway.
- Для discovery используйте URI вида `https+http://{serviceName}` и `HttpClient`, сконфигурированные `ServiceDefaults` (устойчивость + discovery по умолчанию).

## Правила для Program.cs (каждый Api/UI)
- `builder.AddServiceDefaults();` — Общие метрики, расширения аутентификации HttpClient и OpenTelemetry.
- Регистрировать DI для Application/Infrastructure.
- `app.MapDefaultEndpoints();` — health‑эндпойнты (в Dev).
- В Dev — OpenAPI (по необходимости), в Prod — согласно политике безопасности.

Дополнительно для EF Core:
- В `DbContext` используем `UseSnakeCaseNamingConvention()` для таблиц/столбцов (PostgreSQL / Npgsql).
- Явные `HasColumnName(...)` не требуются, если совпадают с результатом конвенции (рекомендовано удалять избыточные маппинги).
- С `AddDbContextPool` перехватчики EF должны быть безопасны для корневого контейнера. Наш `EventDispatchInterceptor` регистрируется как singleton и вытягивает scoped‑сервисы внутри обработчика через контекст.

Смотрите `RestoRate.BuildingBlocks/Data/DbContextExtensions.cs` и `.../Data/Interceptors/EventDispatchInterceptor.cs`.

## Тестирование (???)
- Юнит‑тесты домена (Domain) без инфраструктуры.
- Тесты Application‑слоя (моки портов).
- Интеграционные тесты Infrastructure с Testcontainers (PostgreSQL/MongoDB/RabbitMQ/Redis).
- Контрактные тесты для `Contracts` (совместимость DTO/событий между сервисами).

## Рекомендации по именованию
- Проекты: `RestoRate.<Context>.<Layer>`.
- Пространства имён соответствуют структуре каталогов.
- События и маршруты RabbitMQ: доменные префиксы (`restaurant.*`, `review.*`).


## Чек‑лист
- Доменные объекты — только в `Domain` (или в `SharedKernel`, если общий доменный блок).
- Никакой инфраструктуры в `Domain` и `SharedKernel`.

- Межсервисный обмен — через `Contracts`.
- Публичные точки входа — `WithExternalHttpEndpoints()`; остальное — через Gateway и discovery.

## Миграции EF Core и инструменты

- Для генерации/просмотра миграций используем дизайн‑тайм хост `RestoRate.Migrations` и инфраструктуру `RestoRate.<Context>.Infrastructure`.
- Документация и команды: см. `docs/migrations.md` (есть команды для списка и добавления миграций для сервиса Restaurant). Для корректной работы рекомендуется запускать через Aspire AppHost, чтобы были доступны БД и конфигурация.
