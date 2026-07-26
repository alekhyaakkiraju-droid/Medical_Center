# HIPAA Compliance Checklist

Status legend: **Compliant** | **In Progress** | **Pending**

Last reviewed: 2026-07-25

## Administrative Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Security management process | Compliant | Documented 90-day secret rotation policy and procedures in [`docs/secrets-management.md`](../secrets-management.md); risk assessment in compliance docs |
| Assigned security responsibility | Pending | Designated HIPAA Security Officer role not yet formalized |
| Workforce security | In Progress | Role-based access via ASP.NET Identity (`admin`, `doctor`, `user` policies) |
| Information access management | In Progress | `OwnershipValidator`, role policies; minimum-necessary review ongoing |
| Security awareness training | Pending | No LMS integration yet |
| Security incident procedures | Pending | Breach notification infra pending (see data-subject-rights.md) |
| Contingency plan | Pending | Backup/DR runbooks not yet documented |
| Evaluation | In Progress | Forge CI/CD provides change audit trail; annual evaluation schedule TBD |

## Physical Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Facility access controls | Pending | Cloud/hosting provider BAA required |
| Workstation use | In Progress | Developer guidance in CONTRIBUTING.md |
| Device and media controls | Pending | Depends on production hosting selection |

## Technical Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Access control (unique user ID) | Compliant | ASP.NET Core Identity with unique user IDs |
| Emergency access procedure | Pending | Break-glass account procedure not documented |
| Automatic logoff | In Progress | JWT expiration + refresh rotation; session timeout UX pending |
| Encryption and decryption | In Progress | TLS in transit; SQL Server encryption at rest depends on deployment |
| Audit controls | Compliant | `AuditMiddleware`, `AuditLog` entity, Serilog structured logging |
| Integrity controls | In Progress | FluentValidation, EF concurrency patterns; checksums pending |
| Person or entity authentication | Compliant | JWT cookies, refresh tokens, Google OAuth, email confirmation |
| Transmission security | In Progress | HTTPS enforced in production cookie policy; CORS hardening in progress |

## Privacy Rule (Selected)

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Notice of privacy practices | Pending | NPP not published in application UI |
| Individual rights (access, amendment) | In Progress | Procedures in `data-subject-rights.md` |
| Minimum necessary | In Progress | DTO projections limit exposed fields; ongoing review |
| Business associate agreements | Pending | BAAs with SMTP, cloud, analytics vendors not finalized |

## DevSecOps Alignment

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Secret scanning | Compliant | Gitleaks pre-commit + Forge pipeline |
| SAST | Compliant | Semgrep in `.forge/pipeline.yaml` |
| Dependency scanning | Compliant | npm audit, Grype container scan |
| Change audit trail | Compliant | Git history, Forge work orders, CI logs |

## Next Actions

1. Finalize breach notification automation (currently **Pending** — infrastructure not deployed).
2. Execute BAAs with hosting and email providers.
3. Publish Notice of Privacy Practices in the patient-facing UI.
4. Harden CORS to explicit origins with credentials in production.
