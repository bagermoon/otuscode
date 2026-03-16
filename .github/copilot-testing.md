# Testing Guidelines for Copilot

This document is a compact Copilot-facing testing guide. Use [docs/testing.md](../docs/testing.md) as the canonical team document for the full testing strategy, E2E setup, and detailed authentication patterns.

This repository uses the Microsoft Testing Platform (MTP) for CI and local `dotnet test` runs. xUnit MTP configuration lives in `testconfig.json` inside test projects under `tests/*`, and CI runs `dotnet restore --locked-mode`, so dependency changes must include updated `packages.lock.json` files.

## Related Documentation

- [Copilot Instructions](copilot-instructions.md) - Main AI agent guidelines for this repository
- [Architecture](../docs/ARCHITECTURE.md) - Documentation index
- [Testing Strategy](../docs/testing.md) - Canonical testing approach and auth details

## Finding OpenAPI Endpoints

Prefer the full build-time OpenAPI JSON as the canonical source for integration-test generation.

- **Primary (use for tests):** `artifacts/obj/RestoRate.{Project}/RestoRate.{Project}.json` — full OpenAPI JSON produced at build time.
- **Supplementary (quick listing):** `artifacts/obj/RestoRate.{Project}/debug/ApiEndpoints.json` — lightweight endpoint map useful for quick discovery, but not a complete OpenAPI contract.

Generate or refresh these artifacts by building the solution or the specific API project:

```powershell
dotnet build RestoRate.slnx -c Debug
```

Or build a single API project:

```powershell
dotnet build src/RestaurantService/RestoRate.RestaurantService.Api/RestoRate.RestaurantService.Api.csproj -c Debug
```

Search pattern for artifacts:

```
artifacts/obj/<ProjectName>/RestoRate.<ProjectName>.json
artifacts/obj/<ProjectName>/debug/ApiEndpoints.json
```

### Accessing OpenAPI Documentation at Runtime

When services are running in Development, OpenAPI is available at runtime:

- **OpenAPI JSON:** `https://localhost:<port>/openapi/v1.json` (each service)

Note: OpenAPI endpoints are only available in Development environment, not in Production.

## Quick Test Commands

### Unit Tests
Run unit tests for a specific bounded context:
```powershell
dotnet test tests/<Context>/<Context>.UnitTests
```

### Integration Tests
Run integration tests (requires Docker for infrastructure dependencies):
```powershell
dotnet test tests/<Context>/<Context>.IntegrationTests
```

### All Tests
```powershell
dotnet test RestoRate.slnx
```

For broader testing guidance, E2E setup, and CI notes, see [docs/testing.md](../docs/testing.md).

## Authentication in Tests

### Integration Test Authentication

Integration tests use `TestAuthHandler` from `RestoRate.Testing.Common` to simulate authenticated users without a real Keycloak instance.

Key references:

- `tests/RestoRate.Testing.Common/Auth/TestAuthHandler.cs`
- `tests/RestoRate.Testing.Common/Auth/TestUsers.cs`
- `tests/RestoRate.Testing.Common/WebApplicationFactoryExtensions.cs`

Important behavior:

- test claims match production JWT conventions, including the `"roles"` claim;
- `IUserContext` behaves the same in tests and production;
- role-based authorization policies continue to work without changes.

### E2E Test Authentication

E2E tests use real Keycloak authentication with Playwright browser automation. See `docs/testing.md` for details.

## Troubleshooting

### ApiEndpoints.json Not Found
1. Ensure you're building in Debug configuration (default)
2. Check that the build completed successfully
3. Verify the project has API endpoints defined
4. Look in `artifacts/obj/<ProjectName>/debug/` directory

### OpenAPI Not Available at Runtime
1. Confirm you're running in Development environment
2. Check `ASPNETCORE_ENVIRONMENT` is set to `Development`
3. Verify the service started successfully (check logs)

## Additional Resources

- [Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Minimal APIs OpenAPI](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/openapi)
