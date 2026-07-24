# ADR-005: Forge Shipping CI/CD

## Status

Accepted

## Context

Medical Center requires automated build, test, security scanning, and staging verification on every push to `main` and on pull requests. Manual deployment steps are error-prone and insufficient for HIPAA-aligned change control (documented audit trail, reproducible builds).

The team uses Forge Shipping with the Opsera engine and a pipeline definition checked into the repository.

## Decision

Adopt **Forge Shipping CI/CD** defined in `.forge/pipeline.yaml`:

| Stage | Steps |
|-------|-------|
| Security | Gitleaks secret detection, Semgrep SAST, npm audit (critical threshold) |
| Build | .NET 8 API Release build, Angular production build (Node 22) |
| Test | Backend xUnit tests, frontend Karma/ChromeHeadless tests |
| Container | Docker images for API, YARP, and frontend; Grype scan (critical threshold) |
| Staging | Smoke tests via `scripts/smoke-tests.sh` |

**Triggers:** push and pull_request on `main`; webhooks enabled.

**Environments:**

- `dev` — build and test only (no Docker deploy).
- `staging` — full pipeline including container scans and smoke tests.

Pre-commit hooks (Gitleaks via `.pre-commit-config.yaml`) complement but do not replace CI scans.

## Consequences

### Positive

- Every merge candidate runs the same security and test gates.
- Docker images are built and scanned before staging promotion.
- Pipeline configuration is version-controlled and reviewable in PRs.
- `ForgePipelineConfigurationTests` validates pipeline structure in the test suite.

### Negative

- Pipeline runtime increases with parallel security and Docker stages.
- Opsera/Forge connector configuration is external to the repo; pipeline YAML alone does not guarantee runner availability.
- Failed scans block merges; teams must remediate or suppress findings through approved processes.

### Neutral

- Production deployment promotion beyond staging is a separate work stream (not fully automated in current pipeline).
