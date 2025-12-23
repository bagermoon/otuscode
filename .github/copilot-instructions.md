# Copilot instructions for this repo

Goal: Make AI agents productive immediately in this .NET 9 + Aspire microservices mono‑repo.

## Big Picture
- Solution: `RestoRate.slnx` (code under `src/`). Docs in `docs/` (`ARCHITECTURE.md`, `layout.md`, `diagrams.md`, `proposal.md`).
- Orchestration: Aspire AppHost (`src/RestoRate.AppHost`) runs services and applies shared hosting defaults from `RestoRate.ServiceDefaults`.
- Services (per bounded context): `<Context>.Api` → `<Context>.Application` → `<Context>.Domain`; `<Context>.Infrastructure` implements ports. Current contexts: RestaurantService (all 4 layers), ModerationService/RatingService (Api placeholders), ReviewService.
- Data/Infra: RestaurantService → PostgreSQL; ReviewService → MongoDB; RatingService → Redis; Messaging via RabbitMQ (planned, see diagrams).
- Auth: Keycloak as IdP; API Gateway validates/exchanges tokens before proxying.

## Build & Run
- Build all: `dotnet build RestoRate.slnx`
- Run everything (preferred): `dotnet run --project src/RestoRate.AppHost`
- Run a service: `dotnet run --project src/<Service>/<Service>.csproj`
- Health (dev): `/health`, `/alive`. OpenAPI only in Development.
- VS Code tasks (see `.vscode/tasks.json`):
  - `install certs`: `pwsh ./setup-certs.ps1` (from AppHost dir)
  - EF (Restaurant): `List Migrations`, `Add Migration`, `Remove Last Migration`, `Update Database`

## ServiceDefaults (use in every entrypoint)
- In `Program.cs`: `builder.AddServiceDefaults();` and `app.MapDefaultEndpoints();`
- Enables: service discovery + resilient `HttpClient`, OpenTelemetry logs/metrics/traces (OTLP auto if `OTEL_EXPORTER_OTLP_ENDPOINT` set). Health endpoints are excluded from tracing.

## Auth & Gateway
- Gateway handles OIDC/JWT with Keycloak and forwards only validated requests.
- Reference: `src/RestoRate.Gateway/TokenExchangeMiddleware.cs` and `Program.cs`.
- Services trust Gateway JWTs; do not call Keycloak from microservices.

### User Identity (`IUserContext`)
- Access current user info via `RestoRate.Abstractions.Identity.IUserContext`:
  - `UserId` (from JWT `sub` claim), `Name`, `FullName`, `Email`, `IsAuthenticated`
  - `Roles` (from JWT `"roles"` claim array)
- Register in services: `builder.AddItentityServices();` (part of auth setup in each API's `ConfigureAuthentication`)
- Implementation: `RestoRate.Auth.Identity.HttpContextUserContext` reads claims from `HttpContext.User`
- JWT configuration sets `RoleClaimType = "roles"` so ASP.NET Core recognizes the `"roles"` claim for `[Authorize(Roles = "...")]` and policy checks
- Inject `IUserContext` into Application handlers or API endpoints to access authenticated user details

## Application Pattern
- Use Mediator in `Application` for use‑cases:
  - Write: `ICommand<T>` + `ICommandHandler<T, TRes>` (e.g., `DeleteRestaurantHandler`).
  - Read: `IQuery<T>` + `IQueryHandler<T, TRes>`.
  - Plain: `IRequest<T>` works but loses command/query pipeline separation.
- API layer (Minimal APIs/Controllers) maps HTTP → Mediator. Example: `CreateRestaurantEndpoint` posts `CreateRestaurantCommand` via `ISender`.

## EF Core (Restaurant) — migrations
- Design‑time host: `src/RestoRate.Migrations` (used as `--startup-project`).
- Commands (from repo root), also available as VS Code tasks:
  - List: `dotnet ef migrations --startup-project .\src\RestoRate.Migrations --project .\src\RestaurantService\RestoRate.RestaurantService.Infrastructure list`
  - Add:  `dotnet ef migrations --startup-project .\src\RestoRate.Migrations --project .\src\RestaurantService\RestoRate.RestaurantService.Infrastructure add <Name>`
- Prefer running with AppHost up so DB/config are available. See `docs/migrations.md`.

## Gotchas
- Mediator source generator: Handlers live in `Application`. Ensure `Mediator.Abstractions` + `Mediator.SourceGenerator` are referenced in the project that defines handlers so they are generated and discoverable; handlers can stay `internal`.
- Layering: `Domain` is framework‑free; no EF/MQ/HTTP clients. `Infrastructure` wires providers and implements ports. Cross‑service exchange only via `RestoRate.Contracts`. `RestoRate.Abstractions` is an application‑level package and may depend on `SharedKernel`, but `SharedKernel` never references higher layers and `Domain` never depends on `Abstractions`.
- Domain events: Aggregates raise events using `SharedKernel` primitives; the Application layer (after persistence succeeds) dispatches them through the Mediator adapter. Keep `Domain` unaware of Mediator interfaces.
- Service boundaries: keep Gateway “dumb” (route/transform/auth). Prefer events over cross‑domain synchronous calls.

## Diagnostics
- Constants live in `RestoRate.SharedKernel.Diagnostics`:
  - `ActivitySources` (string names for tracing sources)
  - `LoggingEventIds` (int IDs for LoggerMessage)
- Bind to frameworks at call sites (e.g., `EventId`) rather than in `SharedKernel`.

## Extension Naming
- Application DI: `Add<Context>Application()` in `<Context>.Application`.
- API host: `Add<Context>Api(this IHostApplicationBuilder)` in `<Context>.Api` under `Microsoft.Extensions.Hosting`.

## Key Files
- AppHost: `src/RestoRate.AppHost/AppHost.cs`
- Service defaults: `src/RestoRate.ServiceDefaults/Extensions.cs`
- Gateway: `src/RestoRate.Gateway/Program.cs`, `TokenExchangeMiddleware.cs`
- Restaurant API example endpoint: `src/RestaurantService/RestoRate.RestaurantService.Api/Endpoints/Restaurants/CreateRestaurantEndpoint.cs`
- Architecture index: `docs/ARCHITECTURE.md`; Layout rules: `docs/layout.md`; Diagrams: `docs/diagrams.md`; Testing: `docs/testing.md`
- Testing & OpenAPI docs: `.github/copilot-testing.md`

## When updating architecture
- Keep `docs/diagrams.md` and `docs/layout.md` in sync with changes (Keycloak above Gateway; token exchange in C4). Register new services in `AppHost` and expose external endpoints where needed.

If something here diverges from the current code, update this file alongside the relevant code/docs so agents stay aligned.
