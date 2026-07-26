# Authorization Integration Test Manifest

Phase 1 authorization integration tests verify HIPAA Technical and Administrative Safeguards remain enforced after Phase 2 changes. The Forge **Authorization Regression Gate** (`.forge/pipeline.yaml`) runs this suite on every dev and staging build:

```bash
dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj --filter FullyQualifiedName~Authorization -c Release
```

Last verified: 2026-07-26 (WO-041)

## Test Infrastructure

| Component | Path | HIPAA Control | Purpose |
|-----------|------|---------------|---------|
| `MedicalCenterWebApplicationFactory` | `backend/AngularApi.Tests/Infrastructure/MedicalCenterWebApplicationFactory.cs` | Technical Safeguard: Access control | In-memory test host with JWT, CORS, and Google OAuth configuration aligned to production keys |
| `TestJwtFactory` | `backend/AngularApi.Tests/Infrastructure/TestJwtFactory.cs` | Technical Safeguard: Person or entity authentication | Issues signed JWTs using the same issuer, audience, and secret keys as the test host |
| `AntiforgeryTestHelper` | `backend/AngularApi.Tests/Infrastructure/AntiforgeryTestHelper.cs` | Technical Safeguard: Integrity controls | Applies CSRF tokens for mutating requests, matching production antiforgery behavior |
| `AuthorizationIntegrationTestBase` | `backend/AngularApi.Tests/Authorization/AuthorizationIntegrationTestBase.cs` | Technical Safeguard: Access control | Shared helpers for role-scoped authenticated HTTP clients across controller tests |

## Authorization Integration Test Classes

| Test Class | HIPAA Safeguard | Control Verified |
|------------|-----------------|------------------|
| `AccountControllerAuthorizationIntegrationTests` | Technical | Access control — account endpoints enforce authentication and role policies |
| `AppointmentStatusControllerAuthorizationIntegrationTests` | Technical | Access control — reference data mutations restricted to authorized roles |
| `AppointmentsControllerAuthorizationIntegrationTests` | Technical | Access control — appointment CRUD enforces authentication and ownership |
| `AuditLoggingIntegrationTests` | Technical | Audit controls — mutating operations persist actor identity to `AuditLog` |
| `AuthRateLimitingIntegrationTests` | Technical | Person or entity authentication — login brute-force throttling returns HTTP 429 |
| `AuthorizationPolicyIntegrationTests` | Technical | Access control — global authorization policies reject unauthenticated access |
| `ControllerAuthorizationIntegrationTests` | Technical | Access control — admin-only endpoints reject non-admin roles |
| `CookieAuthIntegrationTests` | Technical | Person or entity authentication — HttpOnly secure cookies, no tokens in response body |
| `DoctorsControllerAuthorizationIntegrationTests` | Technical | Access control — doctor-scoped endpoints enforce role and ownership |
| `MedicalCenterDoctorAvailabilitiesControllerAuthorizationIntegrationTests` | Technical | Access control — availability mutations restricted to authorized roles |
| `MedicalCentersControllerAuthorizationIntegrationTests` | Administrative | Information access management — medical center data limited to owning users |
| `OwnershipValidationIntegrationTests` | Administrative | Information access management — cross-user resource access returns HTTP 403 |
| `PatientReviewsControllerAuthorizationIntegrationTests` | Technical | Access control — review mutations enforce authentication and ownership |
| `PatientsControllerAuthorizationIntegrationTests` | Technical | Access control — patient PHI endpoints enforce authentication and role policies |
| `SpecializationsControllerAuthorizationIntegrationTests` | Technical | Access control — specialization mutations restricted to admin role |

## Regression Policy

Any failure in the Authorization namespace blocks merge to `main`. Do not disable, skip, or weaken these tests without a documented HIPAA risk assessment and Security Officer approval.
