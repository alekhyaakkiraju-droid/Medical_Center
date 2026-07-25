# Phase 1 Modernization Manifest

**Project:** CareShift / Medical Center  
**Reference:** REF-CARESHIFT-P1  
**Repository:** https://github.com/alekhyaakkiraju-droid/Medical_Center  
**Canonical branch:** `main` (integrated from `wo/WO-031-compliance-docs`)  
**Git tag:** `v1.0-modernization`

## Summary

Phase 1 delivered 31 work orders (WO-001 through WO-031) modernizing security, API quality, frontend patterns, Docker, CI/CD, testing, documentation, and compliance artifacts.

Phase 1 integration (post-WO-031) adds Docker demo baseline fixes: same-origin `/api` proxy, navigation/preloader fixes, development seed data, and corrected environment defaults.

## Work Orders (WO-001 → WO-031)

| WO | Theme | PR |
|----|-------|-----|
| WO-001 | Global fallback authorization policy | #1 |
| WO-002 | Restore `[Authorize]` attributes | #2 |
| WO-003 | Ownership validation | #3 |
| WO-004 | Remove hardcoded credentials | #4 |
| WO-005 | Pre-commit secret scanning | #5 |
| WO-006 | JWT HttpOnly cookies | #6 |
| WO-007 | Frontend cookie auth | #7 |
| WO-008 | Route guards | #8 |
| WO-009 | Remove jQuery CDN | #9 |
| WO-010 | Angular DOM replacements | #10 |
| WO-011 | Environment config | #11 |
| WO-012 | FluentValidation | #12 |
| WO-013 | OpenAPI / TypeScript types | #13 |
| WO-014 | Select projections | #14 |
| WO-015 | Pagination | #15 |
| WO-016 | Appointment fixes | #16 |
| WO-017 | Async MailKit email | #17 |
| WO-018 | Auth rate limiting | #18 |
| WO-019 | Angular Material v18 | #19 |
| WO-020 | Serilog structured logging | #20 |
| WO-021 | SharedModule | #21 |
| WO-022 | Production Dockerfiles | #22 |
| WO-023 | Docker Compose | #23 |
| WO-024 | Forge Shipping CI/CD | #24 |
| WO-025 | Audit columns & indexes | #25 |
| WO-026 | Service layer | #26 |
| WO-027 | WCAG 2.1 AA + i18n readiness | #27 |
| WO-028 | Authorization integration tests | #28 |
| WO-029 | E2E smoke scripts | #29 |
| WO-030 | ADRs & API documentation | #30 |
| WO-031 | HIPAA compliance docs | #31 |

## Integration baseline (included in `main` after merge)

- Nginx `/api` reverse proxy to YARP (same-origin frontend calls)
- `API_PUBLIC_URL=/api` in Docker build and `.env.example`
- Global route preloader dismissal via `AppComponent`
- `routerLink` for primary navigation links
- Removed hardcoded `localhost:5004` API URLs in auth services
- `DevelopmentDataSeeder` — seeds 3 specializations in Development
- Removed duplicate `AngularApi - Backup.csproj`
- Fixed frontend Dockerfile nginx user startup issue

## Known limitations (Phase 2 scope)

- Demo users (admin/doctor/patient) and sample appointments not seeded
- Register flow requires SMTP unless dev bypass added
- Email/auth redirect URLs still reference `localhost:4200` in some API paths
- EF migrations not auto-applied on `docker compose up` (manual `dotnet ef database update`)
- Google OAuth and SMTP use placeholder `.env` values
- Contact form has no backend API
- Full appointment booking requires login + doctors in database

## Phase 2

Create a **new Forge project** (REF-CARESHIFT-P2) scanning `main` at tag `v1.0-modernization` for demo-ready and completion work orders (WO-032+).
