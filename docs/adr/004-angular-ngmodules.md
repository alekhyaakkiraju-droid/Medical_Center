# ADR-004: Angular NgModules (Not Standalone Components)

## Status

Accepted

## Context

Angular 18 supports standalone components as the recommended greenfield pattern. Medical Center's frontend was scaffolded with NgModule-based architecture (`AppModule`, feature modules for auth and general pages) before standalone APIs matured.

Migrating to full standalone would require refactoring every component, route, and test across `front-end/src/app` with limited functional benefit for the current release scope.

## Decision

**Continue using NgModules** for the Medical Center Angular frontend:

- **Root module** — `AppModule` bootstraps `AppComponent`, layout components, and imports `AuthModule`, `GeneralModule`, routing, and shared providers.
- **Feature modules** — Domain pages grouped under `pages/auth/auth.module.ts` and `pages/general/general.module.ts`.
- **Hybrid providers** — Modern APIs (`provideHttpClient`, `provideClientHydration`, `withInterceptors`) registered in `AppModule` providers alongside module imports.
- **Incremental adoption** — New components may use standalone declarations within existing modules where practical; no big-bang migration planned.

Future major Angular upgrades will re-evaluate standalone migration when tooling and team capacity allow.

## Consequences

### Positive

- No large-scale refactor risk before healthcare feature delivery milestones.
- Existing module boundaries (`AuthModule`, `GeneralModule`) provide clear feature ownership.
- Compatible with Angular 18 LTS and current `ng build` pipeline in Forge Shipping.

### Negative

- Differs from Angular's latest default scaffolding (standalone-first).
- Slightly larger bundle tree-shaking constraints compared to fully standalone apps.
- New contributors familiar only with standalone patterns need onboarding on NgModule imports/exports.

### Neutral

- Unit tests use `TestBed.configureTestingModule` with module imports, consistent with current codebase patterns.
