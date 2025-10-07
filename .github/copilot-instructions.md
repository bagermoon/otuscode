# Copilot instructions for this repo

Goal: Help AI agents be productive immediately in this .NET/Aspire microservices workspace.

## Big picture
- Solution: `RestoRate.sln`; code in `src/` with an Aspire AppHost (`RestoRate.AppHost`) that starts projects and wires common defaults via `RestoRate.ServiceDefaults`.
- Docs live in `docs/`: read `proposal.md` (system goals and events) and `diagrams.md` (High-level, C4, Sequence, Events). Index: `docs/ARCHITECTURE.md`.
- Current runnable services are placeholders: `RestoRate.MessageSender`, `RestoRate.MessageConsumer`. Real services (Restaurant/Review/Rating/Moderation, BFFs, Gateway) are described in docs and expected to be added.

## Build & run
- Build everything: `dotnet build RestoRate.sln`.
- Preferred run: AppHost (Aspire) orchestrates projects: `dotnet run --project src/RestoRate.AppHost`.
- Run an individual service: `dotnet run --project src/<Service>/<Service>.csproj`.
- Health endpoints (dev only): `/health`, `/alive`. OpenAPI is mapped in Development only.

## ServiceDefaults pattern (use in every service)
- Add `builder.AddServiceDefaults()` in Program.cs to enable:
  - Service discovery + resilient HttpClient (via `AddServiceDiscovery` and `AddStandardResilienceHandler`).
  - OpenTelemetry logging/metrics/traces; OTLP exporter auto-enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.
- Call `app.MapDefaultEndpoints()` to expose health checks (dev only).
- Health endpoints are excluded from tracing.

## Aspire AppHost
- `src/RestoRate.AppHost/AppHost.cs` registers projects via `builder.AddProject<…>("logical-name")` and then `await builder.Build().RunAsync();`.
- To add a new service, create a project in `src/`, reference `RestoRate.ServiceDefaults`, and register it in AppHost with `AddProject<Your_Project>()`.

## Communication & integration (from docs)
- Event-driven via RabbitMQ (planned): events like `restaurant.created`, `review.created|updated|moderated`.
- Consumers (per docs): Moderation consumes `review.created`; Review consumes `review.moderated`; Rating consumes `review.created` and `review.updated`.
- HTTP: API Gateway routes to services; BFFs aggregate for UI-specific endpoints (e.g., `/api/restaurants/{id}/details`). Keep gateway “dumb”; aggregation lives in BFFs.

## Conventions & tips
- Keep service boundaries: Avoid a service calling cross-domain logic; prefer events.
- Naming/events: Use the documented event names and flows in `docs/diagrams.md`.
- Ports and exact endpoints for domain services are specified in docs and may be introduced as services are implemented.

## Useful file map
- AppHost: `src/RestoRate.AppHost/AppHost.cs`
- Common defaults: `src/RestoRate.ServiceDefaults/Extensions.cs`
- Example services: `src/RestoRate.MessageSender/Program.cs`, `src/RestoRate.MessageConsumer/Program.cs`
- Architecture: `docs/proposal.md`, `docs/diagrams.md`, `docs/ARCHITECTURE.md`

If anything here diverges from the code as you implement real services (Restaurant/Review/Rating/Moderation, BFFs, Gateway), update this file and diagrams to keep agents aligned.
