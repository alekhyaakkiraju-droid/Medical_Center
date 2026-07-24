# Contributing to Medical Center

Thank you for contributing to the Medical Center healthcare platform. This guide covers local setup, coding standards, and the pull request process.

## Prerequisites

Install the following before contributing:

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 8.0+ | Backend API |
| Node.js | 22.x | Angular frontend |
| SQL Server | 2022+ (or Docker) | Database |
| Git | Latest | Version control |
| pre-commit | Latest | Secret scanning hooks |
| Docker & Compose | Latest (optional) | Full stack locally |

## Local Setup

### 1. Clone and install hooks

```bash
git clone <repository-url>
cd Medical_Center
pip install pre-commit
pre-commit install
```

### 2. Backend

```bash
cd backend/AngularApi
dotnet restore
dotnet ef database update   # requires connection string in appsettings.Development.json
dotnet run
```

API runs on the configured Kestrel port (see `launchSettings.json`). Swagger UI is available in Development at `/swagger`.

### 3. Frontend

```bash
cd front-end
npm ci
npm start
```

The dev server proxies API calls per `proxy.conf.json`.

### 4. Docker Compose (full stack)

```bash
cp .env.example .env   # if present; configure MSSQL_SA_PASSWORD, JWT_SECRET, etc.
docker compose up --build
```

Services: SQL Server → API → YARP (port 8080) → Angular frontend (port 8081).

## Coding Standards

### Backend (.NET)

- Target **.NET 8**; enable nullable reference types.
- Follow existing folder layout: `Controllers/`, `Services/Interfaces/`, `Services/impelementation/`, `Models/`, `Validators/`, `Middleware/`.
- Use **FluentValidation** for request DTO validation; register validators in `Program.cs`.
- Prefer **async/await** for I/O-bound operations.
- Add or update **xUnit tests** in `backend/AngularApi.Tests/` for every new or changed behavior.
- Use **Serilog** structured logging; include correlation IDs from `CorrelationIdMiddleware`.
- Never commit secrets; use environment variables or user secrets locally.

### Frontend (Angular)

- Angular **18** with **NgModules** (see [ADR-004](docs/adr/004-angular-ngmodules.md)).
- Place feature code under `pages/<feature>/`; shared utilities under `core/`.
- Use Angular Material for UI components where applicable.
- Send credentials via `credentialsInterceptor` for cookie-based auth.
- Add `.spec.ts` tests for components, services, and guards.

### Security & Compliance

- Run `pre-commit run --all-files` before pushing.
- Review [data classification](docs/compliance/data-classification.md) before handling PHI-related fields.
- Follow [HIPAA checklist](docs/compliance/hipaa-checklist.md) status items when touching patient data flows.

### Documentation

- Significant architectural decisions require an ADR in `docs/adr/`.
- Update `README.md` when setup or deployment steps change.

## Pull Request Process

1. **Branch** — Create a feature branch from `main` (e.g. `wo/WO-030-docs`, `feature/appointment-pagination`). Never push directly to `main`.
2. **Work order linkage** — Reference the Forge work order ID in the PR title or description when applicable.
3. **Tests** — All tests must pass locally:
   ```bash
   dotnet test backend/AngularApi.Tests/AngularApi.Tests.csproj -c Release
   cd front-end && npm run test -- --watch=false --browsers=ChromeHeadless
   ```
4. **Pre-commit** — Hooks must pass (Gitleaks secret scan).
5. **Open PR** — Target `main`. Fill in summary, test plan, and any compliance notes.
6. **CI** — Forge Shipping pipeline (`.forge/pipeline.yaml`) runs security scans, builds, tests, and Docker image scans automatically.
7. **Review** — Address reviewer feedback; one work order per atomic commit when using Forge workflow.
8. **Merge** — Squash or merge per repository policy after approval and green CI.

## Architecture References

- [ADR index](docs/adr/) — Modular monolith, JWT cookies, YARP, NgModules, CI/CD
- [Compliance docs](docs/compliance/) — Data classification, HIPAA checklist, data subject rights

## Questions

Open a discussion in the linked Forge work order or repository issue tracker for design questions before large refactors.
