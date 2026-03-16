# Copilot instructions for this repo

Goal: Make AI agents productive immediately in this .NET 9 + Aspire microservices mono‑repo.

## Big Picture
- Solution: `RestoRate.slnx` (code under `src/`).
- Start with `docs/proposal.md` for system context and `docs/diagrams.md` for visual flows.
- Aspire AppHost (`src/RestoRate.AppHost`) runs the local environment and applies shared hosting defaults from `RestoRate.ServiceDefaults`.
- Bounded contexts: RestaurantService, ReviewService, RatingService, ModerationService. Each follows the `<Context>.Api` → `<Context>.Application` → `<Context>.Domain` split, with `<Context>.Infrastructure` implementing ports.
- Authentication is centralized in the API Gateway: it validates requests, handles token exchange, and forwards trusted identities to services.

## Build & Run
- Build all: `dotnet build RestoRate.slnx`
- Run everything (preferred): `dotnet run --project src/RestoRate.AppHost`
- Run a service: `dotnet run --project src/<Service>/<Service>.csproj`
- Health (dev): `/health`, `/alive`. OpenAPI is available in Development, not in Production.
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
- Register in services: `builder.AddIdentityServices();` (part of auth setup in each API's `ConfigureAuthentication`)
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
- MassTransit `IConsumer<T>` implementations belong in API projects under `Consumers` and should use the `*Consumer` suffix to avoid mixing them with Mediator/Application handlers.
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
- Infrastructure DI: `Add<Context>Infrastructure()` in `<Context>.Infrastructure`.
- API host: `Add<Context>Api(this IHostApplicationBuilder)` in `<Context>.Api` under `Microsoft.Extensions.Hosting`.

## Key Files
- AppHost: `src/RestoRate.AppHost/AppHost.cs`
- Service defaults: `src/RestoRate.ServiceDefaults/Extensions.cs`
- Gateway: `src/RestoRate.Gateway/Program.cs`, `TokenExchangeMiddleware.cs`
- Restaurant API example endpoint: `src/RestaurantService/RestoRate.RestaurantService.Api/Endpoints/Restaurants/CreateRestaurantEndpoint.cs`
- Architecture index: `docs/ARCHITECTURE.md`; Layout rules: `docs/layout.md`; Contracts layout: `docs/layout.contracts.md`; Diagrams: `docs/diagrams.md`; Testing: `docs/testing.md`
- Testing & OpenAPI docs: `.github/copilot-testing.md`

## When updating architecture
- Keep `docs/proposal.md`, `docs/diagrams.md`, and `docs/layout.md` in sync with architecture changes. Register new services in `AppHost`, update visual flows when integration paths change, and keep gateway/token exchange wording aligned with the code.

If something here diverges from the current code, update this file alongside the relevant code/docs so agents stay aligned.
