# Testing Guidelines for Copilot

This document provides testing instructions and guidance for finding OpenAPI documentation in the RestoRate project.

## Related Documentation

- [Copilot Instructions](copilot-instructions.md) - Main AI agent guidelines for this repository
- [Architecture](../docs/ARCHITECTURE.md) - System architecture overview
- [Testing Strategy](../docs/testing.md) - Comprehensive testing approach

## Finding OpenAPI Endpoints

Prefer the full build-time OpenAPI JSON as the canonical source for integration-test generation. The OpenAPI generator emits a complete JSON document during `dotnet build` which should be used as the primary contract.

- **Primary (use for tests):** `artifacts/obj/RestoRate.{Project}/RestoRate.{Project}.json` — full OpenAPI JSON produced at build time.
- **Supplementary (quick listing):** `artifacts/obj/RestoRate.{Project}/debug/ApiEndpoints.json` — lightweight endpoint map useful for quick discovery, but not a complete OpenAPI contract.

Generate or refresh these artifacts by building the solution or the specific API project:

```powershell
dotnet build RestoRate.slnx -c Debug
```

Or build a single API project:

```powershell
dotnet build src/Restaurant/RestoRate.Restaurant.Api/RestoRate.Restaurant.Api.csproj -c Debug
```

Search pattern for artifacts:

```
artifacts/obj/<ProjectName>/RestoRate.<ProjectName>.json
artifacts/obj/<ProjectName>/debug/ApiEndpoints.json
```

### Accessing OpenAPI Documentation at Runtime

When the services are running (via AppHost or individually), OpenAPI documentation is available in Development mode:

- **OpenAPI JSON:** `https://localhost:<port>/openapi/v1.json` (each service)

Note: OpenAPI endpoints are only available in Development environment, not in Production.

## Testing Strategy

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

## Service-Specific Testing

### Restaurant Service
- **API Project:** `src/Restaurant/RestoRate.Restaurant.Api`
- **Tests:** `tests/Restaurant/RestoRate.Restaurant.UnitTests`, `tests/Restaurant/RestoRate.Restaurant.IntegrationTests`
- **Database:** PostgreSQL (via Aspire or standalone)

### Review Service
- **API Project:** `src/RestoRate.ReviewService.Api`
- **Database:** MongoDB (planned)

### Rating Service
- **API Project:** `src/RestoRate.Rating.Api` (placeholder)
- **Cache:** Redis (planned)

## Health Checks

All services expose health check endpoints (Development and Production):
- `/health` - Detailed health status
- `/alive` - Simple liveness probe

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
