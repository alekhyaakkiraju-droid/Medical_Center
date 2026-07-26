# ADR-006: Service Layer Refactor Verification

## Status

Accepted

## Context

Five administrative controllers originally accessed `MedicalCenterDbContext` directly from controller actions, bypassing the service layer pattern used elsewhere in the API. Work orders WOREF-012 through WOREF-022 migrated these controllers to dedicated service interfaces:

| Controller | Service Interface |
|------------|-------------------|
| `AppointmentStatusController` | `IAppointmentStatusService` |
| `SpecializationsController` | `ISpecializationService` |
| `MedicalCentersController` | `IMedicalCenterService` |
| `MedicalCenterDoctorAvailabilitiesController` | `IMedicalCenterDoctorAvailabilityService` |
| `PatientReviewsController` | `IPatientReviewService` |

After migration, the architectural constraint (no direct DbContext in controllers, DTOs for mutating endpoints) was enforced only through code review. EPIC-03 required an automated quality gate before the refactor could be considered complete.

## Decision

Adopt **reflection-based architecture tests** and **integration regression tests** as the permanent verification mechanism:

1. **`ServiceLayerArchitectureTests`** (`backend/AngularApi.Tests/Architecture/ServiceLayerArchitectureTests.cs`)
   - Scans all controller constructors via reflection and fails if any inject `MedicalCenterDbContext`.
   - Scans POST/PUT action methods on the five refactored controllers and fails if body parameters use entity model types from `AngularApi.Models`.
   - Verifies all five service interfaces resolve from the DI container.
   - Verifies FluentValidation validators for refactored DTOs are auto-discovered via `AddValidatorsFromAssemblyContaining`.

2. **`ServiceLayerRegressionTests`** (`backend/AngularApi.Tests/Architecture/ServiceLayerRegressionTests.cs`)
   - Exercises full Create/Read/Update/Delete flows for all five controllers through `MedicalCenterWebApplicationFactory`.
   - Uses existing test infrastructure: JWT Bearer auth, antiforgery tokens, and in-memory database seeding.

3. **CI enforcement** — both test classes run as part of the standard `dotnet test` backend suite in Forge Shipping (see ADR-005).

## Consequences

### Positive

- Future controllers that inject `MedicalCenterDbContext` directly will fail CI immediately.
- Mutating endpoints that accept entity models instead of DTOs are caught before merge.
- End-to-end regression coverage confirms auth, CSRF, validation, service layer, and persistence work together for all five resources.
- Verification approach is documented for developers extending the API.

### Negative

- Reflection-based tests may need updates if controller conventions change (e.g., new binding attributes).
- Regression tests add moderate integration test runtime; acceptable given EPIC-03 quality gate role.

### Neutral

- Controllers that legitimately require DbContext for non-CRUD operations can be excluded from static analysis with an explicit documented exception (none required at time of writing).
- GET-by-ID endpoints may still return entity types; the scope of this ADR is service-layer separation and mutating-endpoint DTO usage.
