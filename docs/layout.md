# Структура монорепозитория для микросервисов по DDD (с .NET Aspire и Blazor)

Документ фиксирует рекомендуемую структуру проектов и правила разделения ответственности между `Common`, `SharedKernel`, `Contracts` и константами в контексте .NET 9, .NET Aspire (AppHost, ServiceDefaults), Gateway (YARP/Ocelot) и Blazor Server Dashboard.

## Цели
- Чёткие границы между доменом и инфраструктурой.
- Единый подход к service discovery и устойчивым HTTP‑клиентам (через ServiceDefaults).
- Повторно используемые, но корректно изолированные общие компоненты.
- Удобная локальная разработка и запуск через Aspire AppHost.

## Базовая структура репозитория

- src/
  - `RestoRate.AppHost` — оркестратор Aspire (поднимает Keycloak, RabbitMQ, Gateway, UI и доменные сервисы).
  - `RestoRate.ServiceDefaults` — общие настройки хостинга: service discovery, устойчивость HTTP, OpenTelemetry, health.
  - `RestoRate.Common` — общие прикладные типы и настройки (см. раздел ниже).
  - `RestoRate.Contracts` — межсервисные DTO и интеграционные события между сервисами (версионируемые, сериализуемые). [Структура и рекомендации](./layout.contracts.md)
  - `RestoRate.SharedKernel` — доменные строительные блоки (см. раздел ниже).
  - `RestoRate.Gateway` — шлюз (YARP/Ocelot).
  - `RestoRate.BlazorDashboard` — Blazor Server UI.
  - Доменные сервисы по границам (Bounded Contexts), каждый в 4 слоя:
    - `RestoRate.<Context>.Domain`
    - `RestoRate.<Context>.Application`
    - `RestoRate.<Context>.Infrastructure`
    - `RestoRate.<Context>.Api`
- tests/
  - Юнит‑тесты домена/приложения и интеграционные тесты инфраструктуры (с Testcontainers).

Примеры контекстов: `Restaurant`, `Review`, `Rating`, `Moderation`.

### Зависимости между слоями (строго)
- `Api` → `Application` → `Domain`.
- `Infrastructure` → `Application` + `Domain`.
- Сервисы не ссылаются на `Domain/Application` других сервисов. Межсервисный обмен — только через `Contracts`.

## Ответственность слоёв (Layer Responsibilities)

- Api (внешний слой сервиса)
  - Endpoints (Controllers/Minimal APIs/SignalR) и HTTP‑контракты сервиса.
  - Аутентификация/авторизация (Keycloak/JWT), политики/роллинг‑зоны доступа.
  - Валидация входа на уровне API (model binding, basic validation) и преобразование в команды/запросы Application.
  - Делегирование бизнес‑действий через MediatR: только `Send()`/`Publish()` без бизнес‑логики.
  - Версионирование HTTP‑контракта (если нужно), OpenAPI в Dev, `app.MapDefaultEndpoints()` для health.
  - Никаких доступов к БД/шинам/внешним API напрямую — только через Application.

- Application (use‑cases, orchestration)
  - Команды и запросы (MediatR) + их обработчики: бизнес‑правила сценариев и координация домена.
  - Порты/интерфейсы для инфраструктуры (например, репозитории, брокеры сообщений, внешние API, кэш).
  - Транзакционные границы use‑case’ов; применение доменных событий/правил; идемпотентность при необходимости.
  - Pipeline Behaviors (валидация, логирование, ретраи, performance) — без привязки к конкретной инфраструктуре.
  - DTO для входа/выхода use‑case’ов (внутренние для сервиса). Межсервисные DTO — только из `RestoRate.Contracts`.
  - Никаких EF/RabbitMQ/HTTP‑клиентов напрямую: только через абстракции.

- Domain (чистый домен)
  - Сущности/агрегаты, Value Objects, доменные сервисы, доменные события, инварианты.
  - Полная независимость от инфраструктуры и фреймворков; только .NET/язык.
  - Логика изменения состояния и генерация доменных событий; отсутствие побочных эффектов наружу.

- Infrastructure (адаптеры и реализации портов)
  - Реализации портов Application: репозитории (EF Core/Dapper), клиенты внешних сервисов/HTTP, кэш, брокер сообщений.
  - Маппинг к `RestoRate.Contracts` для публикации/чтения интеграционных событий.
  - Транспорт/провайдер‑специфика: подключения, миграции, конфигурация хранилищ, Outbox/Inbox, ретраи.
  - Регистрация DI (composition root) и интеграция с `RestoRate.ServiceDefaults` (resilience, discovery, telemetry).
  - Никакой бизнес‑логики; только технические детали и маппинг к домену/Application.

## Common vs SharedKernel vs Contracts vs Константы

- `SharedKernel` (только домен):
  - Доменные строительные блоки: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent`, `Result` и т.п.
  - Никакой ASP.NET, Aspire, Keycloak, HTTP, RabbitMQ, EF — только домен.
  - Доменные константы (инварианты) допустимы, если не завязаны на инфраструктуру.

- `Contracts` (межсервисные договорённости):
  - DTO и интеграционные события: сериализуемые, стабильные, версиируемые.
  - Без бизнес‑логики — только данные.

- `Common` (кросс‑сервисная прикладная инфраструктура):
  - Типизированные настройки и их `SectionName` (напр., `KeycloakSettingsOptions`).
  - Константы, связанные с хостингом/инфраструктурой: имена ресурсов Aspire и имена `HttpClient` (напр., `AppHostProjects.Keycloak`, `AppHostProjects.Gateway`).
  - Общие расширения DI, утилиты для аутентификации/авторизации, Blazor‑специфичные helper’ы, обработчики токенов.
  - Не содержит доменной логики.

- Константы:
  - Имена сервисов/ресурсов Aspire и HttpClient — в `Common`.
  - Доменные константы — в конкретном `...Domain` или, если действительно обще‑доменные, в `SharedKernel`.
  - Избегать «магических» строк: выносить в единые константы.

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

## Тестирование (???)
- Юнит‑тесты домена (Domain) без инфраструктуры.
- Тесты Application‑слоя (моки портов).
- Интеграционные тесты Infrastructure с Testcontainers (PostgreSQL/MongoDB/RabbitMQ/Redis).
- Контрактные тесты для `Contracts` (совместимость DTO/событий между сервисами).

## Рекомендации по именованию
- Проекты: `RestoRate.<Context>.<Layer>`.
- Пространства имён соответствуют структуре каталогов.
- События и маршруты RabbitMQ: доменные префиксы (`restaurant.*`, `review.*`).
- HttpClient‑имена и имена ресурсов Aspire — централизовать в `Common.AppHostProjects`.

## Чек‑лист
- Доменные объекты — только в `Domain` (или в `SharedKernel`, если общий доменный блок).
- Никакой инфраструктуры в `Domain` и `SharedKernel`.
- `Common` — прикладные/инфраструктурные общие вещи (опции, константы, расширения), без бизнес‑логики.
- Межсервисный обмен — через `Contracts`.
- Публичные точки входа — `WithExternalHttpEndpoints()`; остальное — через Gateway и discovery.
