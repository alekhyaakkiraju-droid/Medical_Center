# Email Strategy — Local, Staging, and Production

**Reference:** WO-031 · REQ-007 (non-prod email strategy)  
**Related:** WO-030 (MailHog local capture), `.forge/pipeline.yaml`, `docker-compose.staging.yml`

Medical Center sends transactional email for registration confirmation, password reset, and appointment notifications. Each environment uses a different SMTP backend so developers and QA can verify email flows without sending real mail to patients.

## Overview

| Environment | SMTP backend | Purpose | Captured mail access |
|-------------|--------------|---------|----------------------|
| **Local dev** | MailHog (`mailhog:1025`) | Developer email flow testing | http://localhost:8025 |
| **Staging** | Forge IDP SMTP relay | Shared QA / UAT email capture | IDP web UI (`IDP_SMTP_WEB_URL`) |
| **Production** | Real SMTP provider (future) | Patient-facing transactional mail | Provider delivery logs |

Credentials for staging and production are **never** committed to source control. They are injected via Docker secrets (local) or Forge pipeline secrets (staging/production).

---

## Local Development — MailHog (WO-030)

MailHog captures all outbound SMTP from the API container during `docker compose up`.

### Setup

1. Copy secrets and env files per `README.md` / `.env.example`.
2. Start the stack: `docker compose up --build`.
3. MailHog listens on container port `1025` (SMTP) and exposes the web UI on host port **8025**.

### API configuration (docker-compose.yml)

The API service overrides `SmtpSettings` to route through MailHog:

- `SmtpSettings__Host=mailhog`
- `SmtpSettings__Port=1025`
- `SmtpSettings__UseTls=false`

SMTP username/password still load from Docker secrets (`secrets/smtp_email_*`); MailHog accepts any credentials in development.

### Verify captured email

1. Open **http://localhost:8025** in a browser.
2. Trigger a registration or password-reset flow against the local API.
3. Confirm the message appears in the MailHog inbox within a few seconds.

---

## Staging — Forge IDP SMTP (WO-031)

Staging deployments on AWS ECS use the Forge IDP platform SMTP integration. Outbound email is captured centrally so the whole QA team can inspect messages without sharing MailHog on a developer laptop.

### Configuration files

| File | Role |
|------|------|
| `docker-compose.staging.yml` | Compose override mapping IDP SMTP env vars (local staging validation) |
| `.env.staging.example` | Documents required `IDP_SMTP_*` variables |
| `.forge/pipeline.yaml` | Injects `SmtpSettings__*` and `EmailSettings__*` on **Staging Deploy API** |

### Required pipeline secrets

Configure these in Forge Shipping / IDP (never in git):

| Secret / variable | Maps to | Notes |
|-------------------|---------|-------|
| `IDP_SMTP_HOST` | `SmtpSettings__Host` | Forge IDP SMTP relay hostname |
| `IDP_SMTP_PORT` | `SmtpSettings__Port` | Default `587` (STARTTLS) |
| `IDP_SMTP_USE_TLS` | `SmtpSettings__UseTls` | Default `true` |
| `IDP_SMTP_USERNAME` | `EmailSettings__EmailUsername` | Pipeline secret |
| `IDP_SMTP_PASSWORD` | `EmailSettings__EmailPassword` | Pipeline secret |
| `IDP_SMTP_WEB_URL` | — | IDP console URL for viewing captured mail |

### Access captured staging email

1. Sign in to the Forge IDP console for your tenant.
2. Open the **SMTP capture** / outbound mail viewer (URL from `IDP_SMTP_WEB_URL`).
3. Run a registration or password-reset flow against the staging deployment.
4. Confirm subject, body, and recipient in the IDP inbox.

### Staging smoke verification

After **Staging Deploy API**, the pipeline runs **Staging Smoke Tests** and **Staging E2E Tests** (`./scripts/run-e2e-smoke.sh`). A passing `/health` check confirms the API started with the injected IDP SMTP configuration active (misconfigured SMTP env vars would prevent normal startup in strict environments).

---

## Production — Real SMTP Provider (Future)

Production will use a HIPAA-eligible transactional email provider (e.g., Amazon SES, SendGrid) with:

- Credentials stored in AWS Secrets Manager / ECS task secrets
- TLS enforced (`SmtpSettings__UseTls=true`)
- Rotation per `docs/secrets-management.md` (90-day SMTP credential policy)
- BAA in place before go-live (`docs/compliance/hipaa-checklist.md`)

Do not point production at MailHog or IDP capture endpoints.

---

## Troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| No mail in MailHog locally | API not using MailHog overrides | Confirm `mailhog` service is up; check `SmtpSettings__Host` in `docker-compose.yml` |
| SMTP auth errors in API logs | Missing or expired secrets | Refresh `secrets/smtp_email_*` locally; rotate IDP pipeline secrets for staging |
| Staging health check fails after deploy | Invalid `IDP_SMTP_*` values | Verify pipeline secrets; check ECS task env in AWS console |
| Mail sent to real addresses in staging | Wrong SMTP host | Confirm staging task uses IDP host, not `smtp.gmail.com` |

See also: `docs/deployment-runbook.md`, `docs/secrets-management.md`.
