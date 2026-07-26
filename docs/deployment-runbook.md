# Medical Center Deployment Runbook

**Reference:** REF-CARESHIFT-P2 · WO-005  
**Pipeline:** `.forge/pipeline.yaml` (Forge Shipping / Opsera)  
**Local stack:** `docker-compose.yml`  
**Verification scripts:** `scripts/smoke-tests.sh`, `scripts/run-e2e-smoke.sh`  
**Secrets templates:** `secrets.example/`

This runbook documents the complete operational lifecycle for promoting staging to production, rolling back, configuring environments, and recovering from common deployment failures. Follow the sections in order during a release; use Troubleshooting when a step fails.

## Architecture Overview

```
Developer → merge main → Forge Shipping (.forge/pipeline.yaml)
                              │
         ┌────────────────────┼────────────────────┐
         ▼                    ▼                    ▼
      dev (CI)            staging              production
   build + tests    ECR push + ECS deploy   IDP gate + ECS deploy
                    smoke + E2E               post-deploy smoke
                                              rollback path
```

| Environment | Purpose | Gate |
|-------------|---------|------|
| `dev` | Build and unit tests on PR/push | Automated CI only |
| `staging` | Full container build, ECR push, ECS deploy, smoke + E2E | Automated tests |
| `production` | Promoted release serving patient traffic | Staging pass **and** IDP manual approval |

---

## Prerequisites

Before promoting or rolling back production, confirm you have:

| Requirement | Details |
|-------------|---------|
| **IDP / Forge access** | Permission to approve the **Production Promotion Gate** and trigger **Production Rollback** in Forge Shipping |
| **AWS access** | IAM rights to view ECS services, ECR image tags, and CloudWatch logs in `${AWS_REGION:-us-east-1}` |
| **Pipeline variables** | All IDP variables configured (see [Environment Configuration](#environment-configuration)) |
| **CLI tools** | `git`, `docker`, `docker compose`, `curl`, AWS CLI (optional, for ECS/ECR inspection) |
| **Repository checkout** | Clone of `Medical_Center` with `main` up to date |
| **Known-good SHA** | For rollbacks, identify `$PREVIOUS_GIT_SHA` from IDP deployment history or ECR tags before promoting |

**Required IDP / pipeline variables** (never commit secrets to git):

- `CONTAINER_REGISTRY_URL` — ECR registry URL
- `AWS_REGION` — default `us-east-1`
- `ECS_STAGING_CLUSTER` / `ECS_PRODUCTION_CLUSTER`
- `STAGING_BASE_URL` / `STAGING_API_URL`
- `PRODUCTION_BASE_URL` / `PRODUCTION_API_URL`
- ECS service names (defaults in `.forge/pipeline.yaml`: `medical-center-*-staging` / `medical-center-*-production`)

**Local prerequisites** (pre-push validation):

```bash
cp .env.example .env
mkdir -p secrets && cp secrets.example/* secrets/
docker compose up --build
```

---

## Pre-Deployment Checklist

Complete this checklist before approving production promotion:

- [ ] **Branch state:** Change merged to `main`; no open blockers on the release PR
- [ ] **CI green:** `dev` environment in `.forge/pipeline.yaml` passed — Backend Build, Frontend Build, Backend Unit Tests, Frontend Unit Tests
- [ ] **Security scans:** Gitleaks, Semgrep, and Grype reported no critical/high breaches (`failOnBreach: fail`)
- [ ] **Docker images:** All three images built — `medical-center-api`, `medical-center-yarp`, `medical-center-frontend`
- [ ] **Registry tags:** ECR push completed for `$GIT_SHA` and `latest` on all three images
- [ ] **Staging deploy:** ECS staging services updated with `$GIT_SHA` (API, YARP, frontend)
- [ ] **Staging smoke:** `Staging Smoke Tests` step passed (`./scripts/smoke-tests.sh` against `STAGING_BASE_URL` / `STAGING_API_URL`)
- [ ] **Staging E2E:** `Staging E2E Tests` step passed (`./scripts/run-e2e-smoke.sh`)
- [ ] **Database migrations:** API startup applied pending EF migrations without error (see [Database Migration Verification](#database-migration-verification))
- [ ] **Secrets current:** JWT, SMTP, and database credentials valid in target environment (see [Secrets Management](#secrets-management-docker-secrets-from-wo-003))
- [ ] **Rollback SHA recorded:** Note `$PREVIOUS_GIT_SHA` from last known-good production deployment
- [ ] **Change window:** On-call engineer available for post-deploy smoke monitoring

**Local pre-push smoke** (optional but recommended):

```bash
docker compose up --build
SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8081/api ./scripts/smoke-tests.sh
SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8081/api ./scripts/run-e2e-smoke.sh
```

---

## Staging-to-Production Promotion

Production promotion is governed by `.forge/pipeline.yaml`. Staging must pass before the production block runs.

### Automated staging path (triggered by merge to `main`)

1. Merge to `main` triggers the Forge Shipping pipeline (`webhook_branch: main`).
2. **Staging environment** runs (see `environments.staging` in `.forge/pipeline.yaml`):
   - Security scans → builds → unit tests
   - Docker build for API, YARP, frontend
   - Grype container scan
   - ECR push with tags `$GIT_SHA` and `latest`
   - **Staging Deploy API / YARP / Frontend** (`variantId: deploy:aws_ecs`, `IMAGE_TAG: "$GIT_SHA"`)
   - **Staging Smoke Tests:** `SMOKE_BASE_URL="${STAGING_BASE_URL}" SMOKE_API_URL="${STAGING_API_URL}" ./scripts/smoke-tests.sh`
   - **Staging E2E Tests:** `SMOKE_BASE_URL="${STAGING_BASE_URL}" SMOKE_API_URL="${STAGING_API_URL}" ./scripts/run-e2e-smoke.sh`

### Production promotion (manual IDP gate)

After staging passes, the **production** block executes:

1. **Production Promotion Gate** (`variantId: gate:idp-approval`) — **manual IDP sign-off required**
   - Open the Forge Shipping run in IDP
   - Verify staging smoke and E2E steps are green
   - Approve the gate (1 approver required per pipeline config)
2. **Production Deploy API** — deploys `medical-center-api` with `IMAGE_TAG: "$GIT_SHA"` to `${ECS_PRODUCTION_CLUSTER}` / `${ECS_PRODUCTION_API_SERVICE}`
3. **Production Deploy YARP** — deploys `medical-center-yarp`
4. **Production Deploy Frontend** — deploys `medical-center-frontend`
5. **Production Post-Deploy Smoke Tests** — runs `./scripts/smoke-tests.sh` against `PRODUCTION_BASE_URL` / `PRODUCTION_API_URL`

**Target cycle time:** merge-to-main through production deploy ≤ 15 minutes (measure via pipeline timestamps).

### Manual promotion trigger (if re-running production only)

If staging already passed and you need to re-deploy a specific SHA to production:

1. Set pipeline variable `GIT_SHA` to the target commit
2. Trigger the **production** environment steps only in Forge Shipping
3. Complete the **Production Promotion Gate** approval
4. Monitor **Production Post-Deploy Smoke Tests**

---

## Rollback Procedure (5 min)

If post-deploy smoke fails or a production defect is detected, restore the last known-good release within **5 minutes**.

### Step-by-step rollback

| Step | Action | Expected time |
|------|--------|---------------|
| 1 | Identify `$PREVIOUS_GIT_SHA` from IDP deployment history or ECR image tags (`medical-center-api`, `medical-center-yarp`, `medical-center-frontend`) | 1 min |
| 2 | Set pipeline variable `PREVIOUS_GIT_SHA` to that commit | 30 sec |
| 3 | Trigger **Production Rollback** in Forge Shipping (`.forge/pipeline.yaml` — `variantId: deploy:aws_ecs`, `IMAGE_TAG: "$PREVIOUS_GIT_SHA"`, `ROLLBACK: "true"`) | 2 min |
| 4 | Manually re-deploy YARP and frontend with the same `$PREVIOUS_GIT_SHA` if the rollback step only targets the API service | 1 min |
| 5 | Re-run smoke tests against production URLs | 1 min |

```bash
# Verify production health after rollback
SMOKE_BASE_URL="${PRODUCTION_BASE_URL}" SMOKE_API_URL="${PRODUCTION_API_URL}" ./scripts/smoke-tests.sh
```

6. Record the incident in the change log; do not re-promote until root cause is fixed on a new commit.

### Rollback verification scenario

1. Deploy commit `A` to production; confirm smoke pass.
2. Deploy commit `B`; inject a failing smoke check or simulate failure.
3. Execute rollback to commit `A` image tags.
4. Confirm production smoke pass within 5 minutes of rollback start.

---

## Environment Configuration

Environment-specific settings are split between IDP pipeline variables (ECS/cloud) and local `.env` (Docker Compose).

### Pipeline / cloud variables (`.forge/pipeline.yaml`)

| Variable | Staging | Production | Purpose |
|----------|---------|------------|---------|
| `STAGING_BASE_URL` / `PRODUCTION_BASE_URL` | ✓ | ✓ | Frontend URL for smoke tests |
| `STAGING_API_URL` / `PRODUCTION_API_URL` | ✓ | ✓ | API URL for smoke tests |
| `ECS_STAGING_CLUSTER` / `ECS_PRODUCTION_CLUSTER` | ✓ | ✓ | ECS cluster name |
| `ECS_*_API_SERVICE` | staging defaults | production defaults | ECS service names |
| `CONTAINER_REGISTRY_URL` | shared | shared | ECR registry |
| `AWS_REGION` | shared | shared | AWS region (default `us-east-1`) |

Default ECS service names (override via IDP if needed):

- Staging: `medical-center-api-staging`, `medical-center-yarp-staging`, `medical-center-frontend-staging`
- Production: `medical-center-api-production`, `medical-center-yarp-production`, `medical-center-frontend-production`

### Local Docker Compose (`.env` from `.env.example`)

| Variable | Default | Purpose |
|----------|---------|---------|
| `JWT_VALID_ISSUER` | `medical-center` | JWT issuer (non-secret) |
| `JWT_VALID_AUDIENCE` | `medical-center-client` | JWT audience (non-secret) |
| `API_PUBLIC_URL` | `/api` | Frontend build arg for API base |
| `YARP_HOST_PORT` | `8080` | Host port for YARP reverse proxy |
| `FRONTEND_HOST_PORT` | `8081` | Host port for Angular frontend |
| `GOOGLE_AUTH_CLIENT_ID` | placeholder | Google OAuth (optional) |
| `GOOGLE_AUTH_CLIENT_SECRET` | placeholder | Google OAuth (optional) |

### CORS and feature flags

- **CORS origins:** Configure in ECS task environment for staging/production (allow `STAGING_BASE_URL` / `PRODUCTION_BASE_URL` origins). Local development uses YARP on port 8080 and frontend on 8081.
- **Feature flags:** Set via ECS environment variables on the API service task definition; redeploy after changes.

### Changing settings per environment

1. **Local:** Edit `.env`, restart `docker compose up --build`
2. **Staging / Production:** Update IDP pipeline variables or ECS task definition env vars, then redeploy the affected ECS service

---

## Database Migration Verification

The API applies EF Core migrations automatically at startup via `DatabaseMigrationStartup.ApplyPendingMigrationsAsync` (WO-002). Failed migrations exit the process with code 1.

### Verify migrations after deploy

1. Check API container logs for: `Database migrations applied successfully.`
2. Confirm API health endpoint responds: `curl -sf "${PRODUCTION_API_URL}/health"`
3. Run E2E smoke to exercise DB-backed endpoints: `./scripts/run-e2e-smoke.sh`

### Local verification

```bash
docker compose up --build
docker compose logs api | grep -i migration
curl -sf http://localhost:8080/health   # via YARP, or internal api:8080
```

### Migration failure response

| Symptom | Action |
|---------|--------|
| API container exits immediately | Check logs for migration error; fix migration or rollback deploy |
| Schema mismatch after rollback | Roll back to `$PREVIOUS_GIT_SHA` — migrations are tied to app version |
| Manual inspection needed | Connect to SQL Server and verify `__EFMigrationsHistory` table |

**Do not** run manual `dotnet ef database update` against production without a change ticket; rely on automated startup migrations.

---

## Secrets Management (Docker Secrets from WO-003)

Sensitive credentials are **never** stored in git or plain environment variables in `docker-compose.yml`. They are loaded from Docker Secrets mounted at `/run/secrets/<name>`.

### Secret files (`secrets.example/`)

| File | Maps to (API config) | Used by |
|------|----------------------|---------|
| `jwt_secret` | `Jwt:Secret` | API |
| `mssql_sa_password` | `ConnectionStrings:SaPassword` | SQL Server, API |
| `smtp_email_username` | `EmailSettings:EmailUsername` | API |
| `smtp_email_password` | `EmailSettings:EmailPassword` | API |

**Setup (local):**

```bash
mkdir -p secrets
cp secrets.example/* secrets/
# Edit secrets/* with real values — never commit secrets/
docker compose up --build
```

See `secrets.example/README.md` for the same workflow.

### ECS / cloud secrets

In staging and production, inject equivalent values via ECS task secrets or AWS Secrets Manager — map to the same config keys the API expects (`DockerSecretConfigurationProvider` in `backend/AngularApi/Infrastructure/`).

### Rotation procedure

| Secret | Rotation steps | Validation |
|--------|----------------|------------|
| **JWT_SECRET** (`jwt_secret`) | 1. Generate new secret 2. Update secret in ECS/Secrets Manager 3. Rolling redeploy API 4. Invalidate old tokens if needed | Login flow works; API accepts new tokens |
| **SMTP credentials** | 1. Update `smtp_email_username` / `smtp_email_password` 2. Redeploy API | Send test email from registration/password-reset flow |
| **Database password** (`mssql_sa_password`) | 1. Change SA password in SQL Server 2. Update secret file/manager 3. Redeploy SQL + API with new password | `sqlcmd` health check passes; API connects |

**Rotation checklist:**

- [ ] Update secret in target environment (not in git)
- [ ] Redeploy affected services (`docker compose up --build` locally; ECS rolling deploy in cloud)
- [ ] Run `./scripts/smoke-tests.sh` and `./scripts/run-e2e-smoke.sh`
- [ ] Revoke/delete old secret values after validation

---

## Health Check Validation

Health checks are defined in `docker-compose.yml` and must pass before dependent services start.

| Service | Health check | Dependency chain |
|---------|--------------|------------------|
| `sqlserver` | `sqlcmd` SELECT 1 (uses `MSSQL_SA_PASSWORD` from secret) | — |
| `api` | `curl -f http://localhost:8080/health` | waits for `sqlserver` healthy |
| `yarp-proxy` | `curl -f http://localhost:8080/health` | waits for `api` healthy |
| `angular-frontend` | (depends on YARP healthy) | waits for `yarp-proxy` healthy |

### Post-deploy validation (cloud)

```bash
# Basic smoke (health + frontend root)
SMOKE_BASE_URL="${PRODUCTION_BASE_URL}" SMOKE_API_URL="${PRODUCTION_API_URL}" ./scripts/smoke-tests.sh

# Extended E2E (login, register, API endpoints)
SMOKE_BASE_URL="${PRODUCTION_BASE_URL}" SMOKE_API_URL="${PRODUCTION_API_URL}" ./scripts/run-e2e-smoke.sh
```

### Local validation

```bash
docker compose ps   # all services should be healthy
curl -sf http://localhost:8080/health        # YARP
curl -sf http://localhost:8081/              # frontend
SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8081/api ./scripts/smoke-tests.sh
```

**Expected smoke output:** `PASS: API health check`, `PASS: Frontend root responds`, `All smoke tests passed.`

---

## Troubleshooting

### 1. Staging smoke tests fail (`scripts/smoke-tests.sh`)

**Symptoms:** Pipeline fails at **Staging Smoke Tests**; `curl` errors on `/health` or frontend root.

**Diagnose:**

```bash
# Check ECS service status and task logs
aws ecs describe-services --cluster $ECS_STAGING_CLUSTER --services $ECS_STAGING_API_SERVICE
curl -v "${STAGING_API_URL}/health"
curl -v "${STAGING_BASE_URL}/"
```

**Resolve:** Verify `STAGING_BASE_URL` / `STAGING_API_URL` match ALB/CloudFront URLs; confirm ECS tasks are running `$GIT_SHA` images; check API logs for migration or secret load failures.

### 2. E2E smoke fails — login/register pages unreachable

**Symptoms:** **Staging E2E Tests** fails; `run-e2e-smoke.sh` reports login or registration page failures.

**Diagnose:**

```bash
curl -sf "${STAGING_BASE_URL}/login" | head
curl -sf "${STAGING_BASE_URL}/register" | head
docker compose logs angular-frontend   # local
```

**Resolve:** Confirm frontend ECS service deployed; verify `API_PUBLIC_URL` / routing through YARP; rebuild frontend image if API base URL changed.

### 3. Production Promotion Gate blocked

**Symptoms:** Pipeline paused at **Production Promotion Gate**; production deploy never starts.

**Diagnose:** Open Forge Shipping run — confirm staging smoke and E2E are green; check approver permissions.

**Resolve:** Fix failing staging steps first; obtain IDP approval from authorized approver; re-trigger production environment if gate expired.

### 4. API container crash loop — database migration failure

**Symptoms:** API task restarts; logs show `Failed to apply database migrations during startup.`

**Diagnose:**

```bash
docker compose logs api   # local
# Cloud: CloudWatch logs for medical-center-api-production
```

**Resolve:** Fix migration in code and redeploy; or rollback to `$PREVIOUS_GIT_SHA` if migration is incompatible; verify SQL Server connectivity and `mssql_sa_password` secret.

### 5. Docker Secrets not loaded — authentication or DB connection errors

**Symptoms:** API fails to start; empty JWT secret or SQL connection refused; `Secret file '...' is empty` in logs.

**Diagnose:**

```bash
ls -la secrets/                    # local — files must exist
docker compose config | grep secrets
docker exec medical-center-api ls /run/secrets/
```

**Resolve:** Run `cp secrets.example/* secrets/` and populate values; for ECS, verify task secret mounts match `DockerSecretConfigurationProvider` mappings (`jwt_secret`, `mssql_sa_password`, `smtp_email_*`).

### 6. Container vulnerability scan (Grype) blocks deploy

**Symptoms:** Pipeline fails at **Container Vulnerability Scan** with critical findings.

**Diagnose:** Review Grype report in pipeline artifacts; identify affected base image or package.

**Resolve:** Update base images (`Dockerfile`) or patch dependencies; rebuild and re-run pipeline; do not bypass `failOnBreach: fail` without security approval.

### 7. Rollback completed but production still unhealthy

**Symptoms:** **Production Rollback** ran but smoke tests still fail.

**Diagnose:** Confirm all three services (API, YARP, frontend) use `$PREVIOUS_GIT_SHA`; check ECR tags.

**Resolve:** Manually deploy previous SHA to YARP and frontend ECS services; re-run `./scripts/smoke-tests.sh`; escalate if database schema drift occurred between versions.

---

## Quick Reference

| Task | Command / location |
|------|-------------------|
| Pipeline config | `.forge/pipeline.yaml` |
| Local stack | `docker compose up --build` |
| Smoke tests | `./scripts/smoke-tests.sh` |
| E2E tests | `./scripts/run-e2e-smoke.sh` |
| Secret templates | `secrets.example/` |
| Approve production | IDP → **Production Promotion Gate** |
| Rollback | IDP → **Production Rollback** with `$PREVIOUS_GIT_SHA` |
