# ADR-002: JWT Cookie Migration

## Status

Accepted

## Context

The Medical Center API originally exposed JWT tokens to the Angular client via response bodies or local storage patterns, increasing XSS exposure. Browser-based SPAs benefit from HttpOnly cookies that JavaScript cannot read, while still supporting Bearer tokens for non-browser API consumers.

The platform also uses ASP.NET Core Identity with Google OAuth, refresh tokens, and antiforgery protection for mutating requests.

## Decision

Migrate primary browser authentication to **HttpOnly JWT cookies** while retaining Bearer header support:

1. **Cookie-based JWT delivery** — `JwtService` issues tokens stored in an HttpOnly, Secure, SameSite-configured cookie (`MedCenter.Auth` by default, configurable via `Jwt:AuthCookieName`).
2. **Dual extraction** — `JwtBearerEvents.OnMessageReceived` reads the JWT from the auth cookie when no `Authorization` header is present (`ServiceCollectionExtensions.AddAuthenticationServices`).
3. **Refresh token rotation** — `RefreshToken` entities stored server-side with hashed tokens; `RefreshTokenService` handles rotation and revocation.
4. **Antiforgery for mutations** — `ValidateAntiforgeryForMutatingRequestsFilter` and `X-XSRF-TOKEN` header protect state-changing API calls when using cookies.
5. **Angular credentials** — `credentialsInterceptor` sends cookies on cross-origin requests where CORS policy allows credentials.
6. **Backward-compatible Bearer scheme** — Swagger and API clients may still use `Authorization: Bearer <token>` for programmatic access.

## Consequences

### Positive

- Reduced XSS token theft risk (HttpOnly cookies not accessible to JavaScript).
- Aligns with OWASP guidance for SPA session management.
- Refresh token flow supports silent re-authentication without exposing long-lived JWTs to the client.

### Negative

- CSRF protections are mandatory for cookie-based auth; adds complexity to frontend and API filters.
- CORS must be configured carefully (`AllowCredentials` vs. current permissive policy needs hardening in production).
- Non-browser clients must continue using Bearer headers explicitly.

### Neutral

- Google OAuth sign-in still flows through ASP.NET Core Identity cookie scheme for external login, then issues JWT cookies for API access.
