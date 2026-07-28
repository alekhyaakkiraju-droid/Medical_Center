# HIPAA Compliance Checklist

Status legend: **Compliant** | **In Progress** | **In Progress with timeline** | **Pending** | **Deferred**

Last reviewed: 2026-07-27

## Phase 3 Compliance Summary

Phase 3 (WO-037 through WO-041) addressed automatic logoff, NPP acknowledgment, and BAA tracking gating.

| Status | Before Phase 3 | After Phase 3 |
|--------|----------------|---------------|
| Compliant | 8 | 11 |
| In Progress / In Progress with timeline | 7 | 6 |
| Pending | 8 | 5 |

**Improvement:** Compliant controls increased from ~35% to ~48%. Remaining top gaps: BAA execution with vendors, emergency access procedure, contingency plan, and security awareness training.

> This checklist documents CareShift HIPAA alignment posture. It does not constitute HIPAA certification, which requires independent audit and BAA execution with all business associates.

## Administrative Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Security management process | Compliant | Documented 90-day secret rotation policy and procedures in [`docs/secrets-management.md`](../secrets-management.md); risk assessment in compliance docs |
| Assigned security responsibility | Pending | Designated HIPAA Security Officer role not yet formalized — Target: 2026-09-01, Responsible: Privacy Officer |
| Workforce security | In Progress | Role-based access via ASP.NET Identity (`admin`, `doctor`, `user` policies) |
| Information access management | In Progress | `OwnershipValidator`, role policies; minimum-necessary review ongoing |
| Security awareness training | Deferred | Deferred — requires LMS vendor selection (prerequisite) |
| Security incident procedures | In Progress | `BreachNotificationService`, `POST /api/admin/breach-assessment`, and `BreachNotification.html` template (see data-subject-rights.md) |
| Contingency plan | Pending | Backup/DR runbooks not yet documented — Target: 2026-10-01, Responsible: Engineering Lead |
| Evaluation | In Progress | Forge CI/CD provides change audit trail; authorization regression gate in `.forge/pipeline.yaml`; mapped authorization tests in [`authorization-test-manifest.md`](authorization-test-manifest.md); continuous security control verification via [`scripts/run-security-regression.sh`](../../scripts/run-security-regression.sh) |

## Physical Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Facility access controls | Pending | Cloud/hosting provider BAA required — Target: 2026-10-15 (see [`baa-tracking.md`](baa-tracking.md)) |
| Workstation use | In Progress | Developer guidance in CONTRIBUTING.md |
| Device and media controls | Deferred | Deferred — requires production hosting selection (prerequisite) |

## Technical Safeguards

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Access control (unique user ID) | Compliant | ASP.NET Core Identity with unique user IDs |
| Emergency access procedure | Pending | Break-glass account procedure not documented — Target: 2026-09-15, Responsible: Security Officer |
| Automatic logoff | Compliant | WO-037: `IdleTimeoutService`, `SessionTimeoutWarningComponent`; WO-038: session termination, `NoCachePhiActionFilter`, back-button prevention; Tests: `idle-timeout.service.spec.ts`, `SessionTimeoutIntegrationTests.cs` |
| Encryption and decryption | In Progress | TLS in transit; SQL Server encryption at rest depends on deployment |
| Audit controls | Compliant | `AuditMiddleware`, `AuditLog` entity, Serilog structured logging |
| Integrity controls | In Progress | FluentValidation, EF concurrency patterns; checksums pending |
| Person or entity authentication | Compliant | JWT cookies, refresh tokens, Google OAuth, email confirmation |
| Transmission security | In Progress | HTTPS enforced in production cookie policy; CORS hardening in progress |

## Privacy Rule (Selected)

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Notice of privacy practices | Compliant | WO-039: `NppController`, `NppService`, AuditLog storage; WO-040: `NppGuard`, `NppAcknowledgmentComponent`; Tests: `NppServiceTests.cs`, `NppControllerTests.cs`, `npp.guard.spec.ts` |
| Individual rights (access, amendment) | In Progress | Procedures in `data-subject-rights.md` |
| Minimum necessary | In Progress | DTO projections limit exposed fields; ongoing review |
| Business associate agreements | In Progress with timeline | WO-041: `BaaFeatureFlags`, updated [`baa-tracking.md`](baa-tracking.md); SMTP target 2026-09-30, AWS target 2026-10-15 |

## DevSecOps Alignment

| Control | Status | Evidence / Notes |
|---------|--------|------------------|
| Secret scanning | Compliant | Gitleaks pre-commit + Forge pipeline |
| SAST | Compliant | Semgrep in `.forge/pipeline.yaml` |
| Dependency scanning | Compliant | npm audit, Grype container scan |
| Change audit trail | Compliant | Git history, Forge work orders, CI logs |

## Next Actions

1. Execute BAAs with SMTP and AWS providers per [`baa-tracking.md`](baa-tracking.md) timelines.
2. Harden CORS to explicit origins with credentials in production (WO-043/WO-046).
3. Document emergency access and contingency procedures.
4. Select LMS vendor and deploy security awareness training.
