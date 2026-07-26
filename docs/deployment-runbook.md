# Medical Center Deployment Runbook

**Reference:** REF-CARESHIFT-P2 · WO-001  
**Pipeline:** `.forge/pipeline.yaml` (Forge Shipping / Opsera)

## Environments

| Environment | Purpose | Gate |
|-------------|---------|------|
| `dev` | Build and unit tests on PR/push | Automated CI only |
| `staging` | Full container build, ECR push, ECS deploy, smoke + E2E | Automated tests |
| `production` | Promoted release serving patient traffic | Staging pass **and** IDP manual approval |

## Required IDP / pipeline variables

Configure in IDP (never commit secrets to git):

- `CONTAINER_REGISTRY_URL` — ECR registry URL
- `AWS_REGION` — default `us-east-1`
- `ECS_STAGING_CLUSTER` / `ECS_PRODUCTION_CLUSTER`
- `STAGING_BASE_URL` / `STAGING_API_URL`
- `PRODUCTION_BASE_URL` / `PRODUCTION_API_URL`
- ECS service names (or use defaults in pipeline YAML)

## Promotion flow

1. Merge to `main` triggers Forge Shipping pipeline.
2. **Staging** runs build → scan → Docker → Grype → ECR push (tags `$GIT_SHA` and `latest`) → ECS staging deploy → smoke → E2E.
3. **Production Promotion Gate** (`gate:idp-approval`) requires manual IDP sign-off after staging passes.
4. **Production deploy** pulls promoted `$GIT_SHA` images to production ECS services.
5. **Post-deploy smoke** runs against `PRODUCTION_BASE_URL` / `PRODUCTION_API_URL`.

**Target cycle time:** merge-to-main through production deploy ≤ 15 minutes (measure via pipeline timestamps).

## Rollback (≤ 5 minutes)

If post-deploy smoke fails or a production defect is detected:

1. Identify `$PREVIOUS_GIT_SHA` from the last known-good production deployment (IDP deployment history or ECR image tags).
2. Re-run the **Production Rollback** step (or manually trigger production deploy with `IMAGE_TAG=$PREVIOUS_GIT_SHA` for all three services: API, YARP, frontend).
3. Re-run smoke tests against production URLs.
4. Record incident in change log; do not re-promote until root cause is fixed on a new commit.

### Rollback verification scenario

1. Deploy commit `A` to production; confirm smoke pass.
2. Deploy commit `B`; inject a failing smoke check or simulate failure.
3. Execute rollback to commit `A` image tags.
4. Confirm production smoke pass within 5 minutes of rollback start.

## Local smoke (pre-push)

```bash
docker compose up --build
SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8081/api ./scripts/smoke-tests.sh
SMOKE_BASE_URL=http://localhost:8081 SMOKE_API_URL=http://localhost:8081/api ./scripts/run-e2e-smoke.sh
```
