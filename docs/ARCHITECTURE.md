# RestoRate Architecture Index

This document is the navigation entrypoint for architecture documentation. Use it to find the right source of truth instead of repeating the same overview across multiple files.

- Proposal and system context: [docs/proposal.md](./proposal.md)
  Explains what the system is, why the bounded contexts exist, and which architectural invariants matter.
- Diagrams and flows: [docs/diagrams.md](./diagrams.md)
  Contains Mermaid diagrams for topology, key sequences, and simplified event/domain views.
- Layout and layering: [docs/layout.md](./layout.md)
  Describes repository structure, allowed dependencies, and layer responsibilities.
- Contracts layout: [docs/layout.contracts.md](./layout.contracts.md)
  Defines how shared contracts and related packages should be organized.
- Testing strategy: [docs/testing.md](./testing.md)
  Covers unit, integration, and E2E testing workflows.
- Service-specific interactions: [docs/services](./services/)
  Contains focused integration summaries for Restaurant, Review, Rating, and Moderation services.

## Repository Snapshot

- Solution entrypoint: `RestoRate.slnx`
- Runtime orchestration: `src/RestoRate.AppHost`
- Current bounded contexts: `RestaurantService`, `ReviewService`, `RatingService`, `ModerationService`
- Service shape: each context is split into `Domain`, `Application`, `Infrastructure`, and `Api`

## Shared Packages

- `RestoRate.ServiceDefaults` — hosting defaults, resilience, telemetry, service discovery
- `RestoRate.Auth` — auth and identity helpers shared across services
- `RestoRate.Contracts` — cross-service contracts and integration events
- `RestoRate.BuildingBlocks` — reusable infrastructure helpers
- `RestoRate.Abstractions` — application-level abstractions and behaviors
- `RestoRate.SharedKernel` — framework-free primitives and shared domain concepts

## Notes

- Diagnostics constants live in `RestoRate.SharedKernel.Diagnostics`.
- Older `RestoRate.Shared.*` packages were consolidated into more focused packages to reduce coupling.
