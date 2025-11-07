# RestoRate Architecture Index

- Proposal and system overview: [docs/proposal.md](./proposal.md)
- Diagrams (High-level, C4, Sequence, Events): [docs/diagrams.md](./diagrams.md)
- Layout and layering (structure, responsibilities): [docs/layout.md](./layout.md)
- Common hosting defaults (telemetry/auth/resilience): `RestoRate.ServiceDefaults`
- Cross-service contracts (integration events & DTOs): `RestoRate.Contracts`
- Reusable infrastructure & technical helpers (messaging, migrations, seeding, MassTransit, EF helpers): `RestoRate.BuildingBlocks`
- Pure framework‑free abstractions (interfaces / primitives) if used: `RestoRate.Abstractions`

Historic note: older `RestoRate.Shared.*` (Application/Infrastructure/SharedKernel) packages were consolidated into focused `Contracts`, `BuildingBlocks`, and optional `Abstractions` to avoid leaking application layer cross‑service and to reduce coupling.