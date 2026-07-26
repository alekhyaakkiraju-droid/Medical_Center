# Secrets Management and Rotation Runbook

**Reference:** WO-009 · REQ-003 · WOREF-007 / WOREF-008  
**Last reviewed:** 2026-07-25  
**Companion docs:** [Deployment Runbook](./deployment-runbook.md), [HIPAA Checklist](./compliance/hipaa-checklist.md)

This document defines the Medical Center **90-day maximum rotation policy**, step-by-step rotation procedures for each credential type, emergency rotation for suspected compromise, post-rotation verification, and audit trail requirements. It is the operational runbook for DevOps engineers and satisfies the HIPAA administrative safeguard for documented security management processes.

## Overview and Scope

| Item | Detail |
|------|--------|
| **Purpose** | Standardize how production and staging credentials are rotated, verified, and audited |
| **Audience** | DevOps engineers, security team, on-call release engineers |
| **Environments** | Local (`docker compose`), staging ECS, production ECS |
| **Infrastructure** | Docker Secrets (local) and ECS task secrets / AWS Secrets Manager (cloud) per WOREF-007/WOREF-008 |
| **Out of scope** | Application feature secrets (e.g., third-party analytics keys not yet in inventory) — add to inventory before first production use |

Sensitive values are **never** committed to git. Templates live in `secrets.example/`; runtime values live in `secrets/` (local, gitignored) or the cloud secret store.

### Docker Secrets Infrastructure (WOREF-007 / WOREF-008)

Local Docker Compose mounts secret files at `/run/secrets/<name>`. The API loads them via `DockerSecretConfigurationProvider` (`backend/AngularApi/Infrastructure/DockerSecretConfigurationProvider.cs`).

**Update a local secret file:**

```bash
# 1. Edit the file (do not commit secrets/)
vim secrets/jwt_secret

# 2. Recreate containers so mounts pick up the new file
docker compose up -d --force-recreate api
# For SQL password changes, recreate sqlserver and api:
docker compose up -d --force-recreate sqlserver api
```

**Update a cloud secret:**

1. Update the value in AWS Secrets Manager or the ECS task secret definition (same logical keys as below).
2. Trigger a rolling redeploy of affected ECS services (API, and SQL Server task if database password changed).
3. Confirm new tasks mount/read the updated secret before draining old tasks.

See [Deployment Runbook — Secrets Management](./deployment-runbook.md#secrets-management-docker-secrets-from-wo-003) for environment-specific promotion context.

---

## Rotation Policy

All production secrets **must** be rotated on a scheduled cadence. The **maximum** allowed interval is **90 days** unless a credential type specifies a shorter interval. Rotations must be tracked in the audit log (see [Audit Trail Requirements](#audit-trail-requirements)).

| Credential | Maximum interval | Recommended interval | Notes |
|------------|------------------|----------------------|-------|
| JWT signing key (`jwt_secret`) | 90 days | 90 days | Invalidates existing access tokens at next expiry; refresh tokens may need re-login |
| MSSQL SA password (`mssql_sa_password`) | 90 days | 90 days | Coordinate SQL Server + API redeploy |
| SMTP username / password | 90 days | 90 days | Two files; rotate together or username-only if provider allows |
| Google OAuth client ID / secret | 180 days | 90 days | Google Console rotation; treat as 90 days in production for HIPAA alignment |

**Policy rules:**

- Calendar reminders at **day 75** for all credentials due within 15 days.
- **No credential** may exceed its maximum interval in production; overdue items are tracked as compliance findings.
- Emergency rotation (compromise suspected) overrides the schedule — rotate immediately regardless of last rotation date.
- After any rotation, complete the [Verification Steps](#verification-steps) checklist before closing the change ticket.

---

## Credential Inventory

| Secret name | Docker secret file / source | Configuration key | Rotation interval | Responsible role |
|-------------|----------------------------|-------------------|-------------------|------------------|
| JWT signing key | `secrets/jwt_secret` → `/run/secrets/jwt_secret` | `Jwt:Secret` | 90 days | DevOps / Security |
| MSSQL SA password | `secrets/mssql_sa_password` → `/run/secrets/mssql_sa_password` | `ConnectionStrings:SaPassword` | 90 days | DevOps / DBA |
| SMTP username | `secrets/smtp_email_username` | `EmailSettings:EmailUsername` | 90 days | DevOps |
| SMTP password | `secrets/smtp_email_password` | `EmailSettings:EmailPassword` | 90 days | DevOps |
| Google OAuth client ID | `.env` / ECS env `GOOGLE_AUTH_CLIENT_ID` | `GoogleAuth:ClientId` | 180 days (90 recommended) | DevOps / App owner |
| Google OAuth client secret | `.env` / ECS env `GOOGLE_AUTH_CLIENT_SECRET` | `GoogleAuth:ClientSecret` | 180 days (90 recommended) | DevOps / App owner |

Non-secret JWT metadata (`Jwt:ValidIssuer`, `Jwt:ValidAudience`) is configured via environment variables in `docker-compose.yml` and ECS task definitions; update only when issuer/audience URLs change.

---

## Rotation Procedures

### JWT Signing Key Rotation

**Impact:** Active access tokens remain valid until expiry; refresh tokens signed with the old key will fail after rotation — users may need to sign in again.

1. **Generate a new key** — at least 256 bits of entropy (example):

   ```bash
   openssl rand -base64 64 | tr -d '\n' > /tmp/new_jwt_secret
   ```

2. **Update the secret**
   - **Local:** `cp /tmp/new_jwt_secret secrets/jwt_secret && shred -u /tmp/new_jwt_secret`
   - **Cloud:** Update the Secrets Manager / ECS secret mapped to `Jwt:Secret`.

3. **Redeploy the API** — rolling restart so all tasks read the new file:

   ```bash
   docker compose up -d --force-recreate api
   # Cloud: rolling deploy medical-center-api-* service
   ```

4. **Verify** — see [JWT verification](#jwt-signing-key) below.

5. **Retire old value** — delete previous secret version from the store after verification (keep one prior version for 24h rollback window if policy allows).

---

### MSSQL SA Password Rotation

**Impact:** Brief connectivity loss if API and SQL Server are not updated in sequence.

1. **Connect to SQL Server** (local example):

   ```bash
   docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
     -S localhost -U sa -P "$(cat secrets/mssql_sa_password)" -C
   ```

2. **Set a new password** in T-SQL (use a strong generated password):

   ```sql
   ALTER LOGIN sa WITH PASSWORD = 'NEW_STRONG_PASSWORD' OLD_PASSWORD = 'CURRENT_PASSWORD';
   GO
   ```

3. **Update secret files / cloud secret** with the new password (`secrets/mssql_sa_password` → `ConnectionStrings:SaPassword`).

4. **Restart services** — SQL Server entrypoint reads the secret at start; API builds the connection string from `ConnectionStrings:SaPassword`:

   ```bash
   docker compose up -d --force-recreate sqlserver
   # Wait for sqlserver healthy, then:
   docker compose up -d --force-recreate api
   ```

5. **Verify** — see [MSSQL verification](#mssql-sa-password) below.

---

### SMTP Credentials Rotation

1. **Rotate with email provider** — create new app password or API credentials in the SMTP provider console; disable the old credential after verification.

2. **Update secret files**
   - `secrets/smtp_email_username` → `EmailSettings:EmailUsername`
   - `secrets/smtp_email_password` → `EmailSettings:EmailPassword`

3. **Redeploy API:**

   ```bash
   docker compose up -d --force-recreate api
   ```

4. **Verify** — see [SMTP verification](#smtp-credentials) below.

---

### Google OAuth Credentials Rotation

Google OAuth client ID and secret are supplied via environment variables (`GOOGLE_AUTH_CLIENT_ID`, `GOOGLE_AUTH_CLIENT_SECRET` in `.env` locally; ECS task environment in cloud).

1. **Google Cloud Console** — APIs & Services → Credentials → OAuth 2.0 Client IDs → create new client secret (or new client if rotating client ID).

2. **Update authorized redirect URIs** if the client ID changed — must match production/staging callback URLs configured in the app.

3. **Update environment**
   - **Local:** edit `.env` (from `.env.example` template); never commit.
   - **Cloud:** update ECS task definition / Secrets Manager env injection for `GoogleAuth:ClientId` and `GoogleAuth:ClientSecret`.

4. **Redeploy API and frontend** if client ID changed (frontend may embed client ID in build args):

   ```bash
   docker compose up -d --build api angular-frontend
   ```

5. **Verify** — see [Google OAuth verification](#google-oauth) below.

6. **Revoke old OAuth client secret** in Google Cloud Console after successful verification.

---

## Emergency Rotation Procedures

Use when a credential is **suspected or confirmed compromised** (leak in git, exposed logs, vendor breach, lost laptop with `.env`, etc.).

### Immediate actions (within 1 hour)

1. **Rotate the affected credential** using the standard procedure above — do not wait for the 90-day schedule.
2. **Rotate related credentials** if the compromise scope is unknown (e.g., if `.env` was exposed, rotate JWT, SMTP, DB, and Google OAuth together).
3. **Invalidate sessions** — after JWT rotation, consider clearing refresh tokens in the database or forcing global re-authentication if breach scope includes token theft.
4. **Block old values** — revoke old SMTP app passwords, Google OAuth secrets, and delete superseded secret versions in AWS.

### Notification steps

| Audience | Action | Timeline |
|----------|--------|----------|
| Security team / HIPAA Security Officer | Notify via incident channel with credential type, environment, suspected exposure vector | Within 1 hour |
| Engineering on-call | Page if production auth/email/DB is impaired | Immediately if user impact |
| Management / Legal | Escalate if PHI access is possible | Per incident response plan |

### Post-incident requirements

1. **Audit log review** — export Serilog / CloudWatch logs for the exposure window; search for unauthorized API access, failed auth spikes, or email sends.
2. **Incident ticket** — document timeline, root cause, credentials rotated, users affected, and whether PHI was accessed.
3. **Breach assessment** — coordinate with Legal; see [Data Subject Rights](./compliance/data-subject-rights.md) if notification may be required.
4. **Prevent recurrence** — run Gitleaks locally, confirm secret scanning in CI, rotate any other secrets stored alongside the leaked material.

---

## Verification Steps

Complete **all** applicable checks before closing a rotation change ticket.

### JWT signing key

- [ ] `curl -sf http://localhost:8080/health` (or production URL) returns 200
- [ ] New user login succeeds (email/password)
- [ ] Issued JWT cookie is accepted on authenticated API call (e.g., `/api/account/me` or equivalent)
- [ ] Refresh token flow succeeds OR users can re-login after expected refresh invalidation
- [ ] `./scripts/smoke-tests.sh` passes against target environment

### MSSQL SA password

- [ ] SQL Server health check passes (`docker compose ps` shows `sqlserver` healthy)
- [ ] API health check passes and API logs show no connection errors
- [ ] `./scripts/run-e2e-smoke.sh` passes (exercises DB-backed endpoints)

### SMTP credentials

- [ ] Trigger password-reset or registration email in staging/production
- [ ] Confirm delivery in provider dashboard / mailbox within 5 minutes
- [ ] API logs show no SMTP authentication errors

### Google OAuth

- [ ] "Sign in with Google" completes end-to-end in target environment
- [ ] New OAuth token maps to expected user account / registration flow
- [ ] Old client secret rejected in Google Console test (optional curl to token endpoint)

### Full stack (recommended after any production rotation)

```bash
SMOKE_BASE_URL="${PRODUCTION_BASE_URL}" SMOKE_API_URL="${PRODUCTION_API_URL}" ./scripts/smoke-tests.sh
SMOKE_BASE_URL="${PRODUCTION_BASE_URL}" SMOKE_API_URL="${PRODUCTION_API_URL}" ./scripts/run-e2e-smoke.sh
```

---

## Audit Trail Requirements

Every rotation event (scheduled or emergency) must produce an auditable record.

### Required log fields

| Field | Example |
|-------|---------|
| Event type | `secret_rotation` |
| Credential | `jwt_secret`, `mssql_sa_password`, etc. |
| Environment | `staging`, `production` |
| Actor | Engineer identity / IAM role |
| Timestamp (UTC) | ISO-8601 |
| Change ticket | Jira/Forge WO reference |
| Verification status | `passed` / `failed` with notes |
| Emergency flag | `true` if compromise-driven |

### Where to record

1. **Change management** — Forge work order or change ticket with rotation date, verifier name, and test evidence (CI run link or smoke script output).
2. **Git** — no secret values; optional commit updating `Last reviewed` in this document when policy changes.
3. **Application audit log** — `AuditMiddleware` / `AuditLog` entity captures auth and admin events; review after JWT/OAuth rotation for anomalies.
4. **Infrastructure logs** — ECS deployment events, Secrets Manager `PutSecretValue` CloudTrail entries, Google Cloud audit logs for OAuth client changes.
5. **Retention** — keep rotation records **minimum 6 years** per HIPAA documentation retention guidance (align with organizational policy).

### Rotation register (recommended spreadsheet or ticket query)

Maintain a register with columns: `Credential`, `Environment`, `Last rotated`, `Next due`, `Rotated by`, `Verified by`, `Ticket ID`. Review monthly; escalate items within 15 days of due date.

---

## Related References

- `secrets.example/README.md` — local secret file setup
- `backend/AngularApi/Infrastructure/DockerSecretConfigurationProvider.cs` — file-to-config mappings
- `docs/deployment-runbook.md` — promotion, rollback, and ECS secret injection
- `docs/compliance/hipaa-checklist.md` — administrative safeguard cross-reference
