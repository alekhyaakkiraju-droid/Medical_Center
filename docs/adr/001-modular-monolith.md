# ADR-001: Modular Monolith Architecture

## Status

Accepted

## Context

Medical Center is a full-stack healthcare platform serving patients, doctors, and administrators. The team needed a deployable unit that balances development velocity with clear domain boundaries, without the operational overhead of microservices at the current scale.

The backend combines ASP.NET Core Identity, appointment scheduling, payments, and audit logging in a single `AngularApi` project. The frontend is a separate Angular application, and a YARP reverse proxy handles routing in containerized deployments.

## Decision

Adopt a **modular monolith** pattern:

- **Single deployable API** (`backend/AngularApi`) organized by feature areas: Controllers, Services, Models, Validators, and Middleware.
- **Shared database** (SQL Server) with EF Core migrations and domain entities co-located in the API project.
- **Separate frontend** (`front-end`) communicating via REST over HTTPS through YARP.
- **Cross-cutting concerns** (auth, audit, rate limiting, validation) implemented as middleware, filters, and extension methods rather than separate services.

Domain modules are expressed through folder structure and service interfaces (`IAppointmentService`, `IPatientService`, etc.), not separate deployable services.

## Consequences

### Positive

- Simpler local development and debugging (single API process, one database).
- Atomic transactions across related entities (appointments, payments, users).
- Lower infrastructure cost and fewer network hops than microservices.
- Straightforward CI/CD: one .NET build artifact plus frontend and proxy images.

### Negative

- All API modules scale together; cannot independently scale appointment vs. auth workloads.
- A defect in one module can affect the entire API process.
- Future extraction to microservices requires deliberate boundary work (already partially prepared via service interfaces).

### Neutral

- Docker Compose and Forge Shipping build three images (API, YARP, frontend) but the API remains a single logical monolith.
