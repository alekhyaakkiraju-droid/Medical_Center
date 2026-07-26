# Medical Center — Forge dev deploy (Sales AWS account)

Deploy target: **`https://medical-center-yarp-dev.agent.opsera.dev`** (UAT/dev, not production).

## AWS account (via Forge — no local `aws configure` required)

| Setting | Value |
|---------|--------|
| Account | **792373136340** (SrinivasanAWS-Sales connector in Forge) |
| Region | **us-west-2** |
| Spoke cluster | **opsera-usw2-np** |
| ArgoCD hub | **argocd-usw2** |
| Namespace | **opsera-medical-center-dev** |

Forge stores AWS credentials in the **connector**. Pipelines use that connector at run time — your Mac's `opsera@pipeline` user (969898466769) is unrelated.

## Forge Shipping pipeline

- **Name:** Medical Center Dev EKS
- **ID:** `be5b3a9e-9348-4d1a-b184-8e18a175bb6b`
- **Connectors:** Alekhya-GH (GitHub) + SrinivasanAWS-Sales (AWS)

### Pre-flight checklist (run once before clicking Run)

| # | Check | Status |
|---|--------|--------|
| 1 | `front-end/package-lock.json` tracked (PR #98) | Done on `main` |
| 2 | `front-end/src/app/api/generated/api.ts` + `openapi/swagger.json` tracked (PR #99) | Done on `main` |
| 3 | ECR repos exist: `medical-center-api`, `-yarp`, `-frontend` | Run `bootstrap-ecr.sh` once |
| 4 | K8s secret `medical-center-app-secrets` in namespace | Apply `secrets.example.yaml` once |
| 5 | Trigger run from **Forge UI** (not MCP — MCP runs can stay queued) | Required |

### Pipeline phases

**Phase 1 — Build + push (target: green)**  
scan → sync/build api → build yarp → build frontend → bootstrap ECR → push all three images

**Phase 2 — Deploy (after secrets exist)**  
render manifests with `$GIT_SHA_SHORT` → kubectl apply (see `.opsera-medical-center/k8s/README.md`)

Deploy is intentionally deferred until namespace secrets exist — applying API manifests without SQL/JWT secrets causes crash loops, not a useful failure mode.

### Trigger from Forge UI

1. Open project **CareShift (Copy) V2** in Forge.
2. Go to **Shipping → Pipelines → Medical Center Dev EKS**.
3. Run **Validate connectors** / **Preflight**.
4. Click **Run pipeline** on branch **`main`**.

### UAT logins

See [`test-data-manifest.md`](test-data-manifest.md). Password is documented there for non-production UAT only.

Reference implementation: `gayathri-opsera/UBR-Open-NMS` (`.opsera-ubr-nms/`).
