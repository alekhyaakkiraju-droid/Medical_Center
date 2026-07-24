# ADR-003: YARP Reverse Proxy Retention

## Status

Accepted

## Context

Medical Center runs as a multi-container stack: SQL Server, ASP.NET Core API, Angular frontend, and a reverse proxy. Early architecture discussions considered removing YARP and exposing the API and frontend on separate ports or using a cloud load balancer exclusively.

YARP (Yet Another Reverse Proxy) is already integrated in `backend/YARPReverseProxy` with route configuration in `docker/yarp.config.json` and a dedicated Docker image in Forge Shipping CI/CD.

## Decision

**Retain YARP** as the edge reverse proxy for containerized and staging deployments:

- **Single public entry point** — External traffic hits YARP on port 8080; routes `/api/*` to the API and static/UI routes to the Angular container.
- **Configuration-driven routing** — Routes and clusters loaded from `ReverseProxy` configuration section; no code changes required for path rewrites.
- **Health checks** — YARP exposes `/health` for orchestration and smoke tests (`scripts/smoke-tests.sh`).
- **CI/CD parity** — `.forge/pipeline.yaml` builds and scans the YARP Docker image alongside API and frontend images.

Direct API access remains available for local development (`dotnet run` on AngularApi) without YARP.

## Consequences

### Positive

- Unified origin for browser clients simplifies cookie and CORS configuration.
- TLS termination and path-based routing can be centralized at the proxy layer in future deployments.
- Matches production-like topology in Docker Compose and staging smoke tests.

### Negative

- Additional container to build, scan, and monitor.
- Extra network hop between client and API (minimal latency in same Docker network).
- Developers must understand proxy config when debugging routing issues.

### Neutral

- YARP is a Microsoft-supported library aligned with the .NET 8 stack; no third-party proxy dependency.
