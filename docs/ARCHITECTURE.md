# RestoRate Architecture Index

- Proposal and system overview: [docs/proposal.md](./proposal.md)
- Diagrams (High-level, C4, Sequence, Events): [docs/diagrams.md](./diagrams.md)
- Layout and layering (structure, responsibilities): [docs/layout.md](./layout.md)
- Common hosting defaults (telemetry/auth/resilience): `RestoRate.ServiceDefaults`
- Cross-service contracts (integration events & DTOs): `RestoRate.Contracts`
- Reusable infrastructure & technical helpers (messaging, migrations, seeding, MassTransit, EF helpers): `RestoRate.BuildingBlocks`
- Application‑level abstractions and pipeline behaviors: `RestoRate.Abstractions` (may depend on `SharedKernel`; contains Mediation behaviors and app‑level contracts; no transports/ORM/web frameworks). `SharedKernel` remains framework‑free and independent of higher layers.

Diagnostics
- Diagnostic constants live in `RestoRate.SharedKernel.Diagnostics`:
	- `ActivitySources` — string names for tracing sources
	- `LoggingEventIds` — int IDs for LoggerMessage
- Convert to framework types (e.g., `EventId`) at call sites.

Historic note: older `RestoRate.Shared.*` (Application/Infrastructure/SharedKernel) packages were consolidated into focused `Contracts`, `BuildingBlocks`, and optional `Abstractions` to avoid leaking application layer cross‑service and to reduce coupling.