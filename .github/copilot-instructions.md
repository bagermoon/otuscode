# Copilot instructions for this repo

Goal: Help AI agents be productive immediately in this .NET/Aspire microservices workspace.

## Big picture
- Solution: `RestoRate.sln`. Code lives under `src/`.
- Orchestration: Aspire AppHost (`RestoRate.AppHost`) starts projects and wires common defaults via `RestoRate.ServiceDefaults`.
- Auth: Keycloak (IdP) is reachable by clients. The API Gateway exchanges/validates access tokens with Keycloak before forwarding to microservices.
- Docs live in `docs/`:
  - Index: `docs/ARCHITECTURE.md`
  - System goals and events: `docs/proposal.md`
  - High-level, C4, Sequence, Events diagrams: `docs/diagrams.md`
  - Layout and layering (incl. layer responsibilities): `docs/layout.md`
- Current runnable services include placeholders: `RestoRate.MessageSender`, `RestoRate.MessageConsumer`. Real services (Restaurant/Review/Rating/Moderation, Gateway, Dashboard) are described in docs and expected to be added.

## Build & run
- Build everything:
  - `dotnet build RestoRate.sln`
- Preferred run: AppHost orchestrates services:
  - `dotnet run --project src/RestoRate.AppHost`
- Run an individual service:
  - `dotnet run --project src/<Service>/<Service>.csproj`
- Health endpoints (dev only): `/health`, `/alive`
- OpenAPI is mapped in Development only

## ServiceDefaults pattern (use in every service)
In each service `Program.cs`:
- `builder.AddServiceDefaults()` enables:
  - Service discovery + resilient HttpClient (via `AddServiceDiscovery` and `AddStandardResilienceHandler`)
  - OpenTelemetry logs/metrics/traces; OTLP exporter auto-enables if `OTEL_EXPORTER_OTLP_ENDPOINT` is set
- `app.MapDefaultEndpoints()` exposes health checks (dev only)
- Health endpoints are excluded from tracing

## AuthN/Z and API Gateway
- Keycloak acts as the public IdP. Clients (e.g., Dashboard) perform OIDC login against Keycloak.
- The API Gateway validates and/or exchanges incoming tokens with Keycloak before proxying to microservices.
  - See `src/RestoRate.Gateway/TokenExchangeMiddleware.cs`
- Services should trust validated JWTs provided by the Gateway and avoid calling Keycloak directly.
- Prefer scopes/roles for Authorization; propagate user identity/claims downstream only as needed.

## Aspire AppHost
- `src/RestoRate.AppHost/AppHost.cs` registers projects via `builder.AddProject<…>("logical-name")` and then `await builder.Build().RunAsync();`.
- To add a new service:
  1) Create a project in `src/` and reference `RestoRate.ServiceDefaults`
  2) Register it in AppHost with `AddProject<Your_Project>("your-service-name")`
  3) If the service must be externally reachable, configure its external endpoint in the AppHost as appropriate
  4) Update diagrams and docs accordingly (`docs/diagrams.md`, `docs/ARCHITECTURE.md`, `docs/layout.md`)

## Communication & integration
- Event-driven messaging via RabbitMQ (planned). Key event families:
  - `restaurant.created`
  - `review.created`, `review.updated`, `review.moderated`
- Consumers (from docs):
  - Moderation consumes `review.created`
  - Review consumes `review.moderated`
  - Rating consumes `review.created` and `review.updated`
- HTTP: API Gateway routes requests to domain services. No BFF services are used; aggregation stays minimal in Gateway—prefer domain services + events.

## Layering and responsibilities (summary)
- API layer
  - HTTP endpoints, auth (JWT/OIDC), basic input validation, OpenAPI/health (dev)
  - Dispatches to Application via MediatR; no direct infra access
- Application layer
  - Use cases as MediatR Commands/Queries + Handlers; orchestration and transactional boundaries
  - Pipeline behaviors (validation, logging, idempotency)
  - Defines ports/interfaces that Infrastructure implements; no direct EF/RabbitMQ/HTTP clients
- Domain layer
  - Entities, Aggregates, Value Objects, domain services/events, invariants
  - Pure .NET; no infrastructure dependencies
- Infrastructure layer
  - Implements ports (repositories, brokers, external clients, cache)
  - Persistence, migrations, outbox/inbox, retries, provider configuration
  - DI wiring and ServiceDefaults integration; no business logic

Tip: Place all MediatR Commands/Queries/Handlers in the Application layer. Controllers/Minimal APIs only translate HTTP to MediatR requests.

## Conventions & tips
- Keep service boundaries: avoid cross-domain logic calls; prefer events.
- Naming/events: use `docs/diagrams.md` naming and flows.
- Keep Gateway “dumb”: route/transform, auth/token exchange; let services own aggregation and business rules.
- Add `RestoRate.ServiceDefaults` to every service for consistency.

## Useful file map
- AppHost: `src/RestoRate.AppHost/AppHost.cs`
- Common defaults: `src/RestoRate.ServiceDefaults/Extensions.cs`
- API Gateway: `src/RestoRate.Gateway/Program.cs`, `TokenExchangeMiddleware.cs`
- Dashboard (Blazor): `src/RestoRate.BlazorDashboard/Program.cs`
- Example messaging services: `src/RestoRate.MessageSender/Program.cs`, `src/RestoRate.MessageConsumer/Program.cs`
- Architecture: `docs/ARCHITECTURE.md` (links to proposal, diagrams, layout)

## When you update architecture, also update
- `docs/diagrams.md`
  - High-Level: ensure Keycloak is above Gateway; no BFFs
  - C4-like: reflect Keycloak as external IdP and Gateway token exchange
- `docs/layout.md`
  - Keep layer responsibilities and MediatR placement accurate
- `src/RestoRate.AppHost/AppHost.cs`
  - Register new services and external endpoints where applicable

## Quick checklist for adding a new service
- Project scaffold under `src/<ServiceName>/`
- Reference `RestoRate.ServiceDefaults`
- Minimal Program.cs with `AddServiceDefaults()` and `MapDefaultEndpoints()`
- API endpoints → MediatR (Application layer) → Domain → Infrastructure ports
- Register in AppHost
- Update diagrams and layout docs
- Add health checks and minimal OpenAPI (dev)

If anything here diverges from the code as you implement real services (Restaurant/Review/Rating/Moderation, Gateway), update this file and the diagrams to keep agents aligned.
