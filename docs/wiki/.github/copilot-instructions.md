# Copilot Instructions for ADNC Repository

## Build, Test, and Lint Commands
- This repository is documentation-focused and does not contain source code, solution files, or test scripts directly. The actual codebase referenced is at https://github.com/alphayu/adnc.
- For build, test, and lint instructions, refer to the main ADNC repository. This wiki provides architecture, conventions, and configuration guidance.

## High-Level Architecture
- **ADNC** is a .NET 8-based open-source distributed/microservices framework, also suitable for monoliths.
- The main codebase (see main repo) is organized as:
  - `src/Infrastructures`: Infrastructure integrations (Consul, EventBus, cache, etc.)
  - `src/ServiceShared`: Shared service layer (WebApi bootstrapping, remote calls, middleware)
  - `src/Gateways`: API gateway (Ocelot)
  - `src/Demo`: Demo microservices (Admin, Maint, Cust, Ord, Whse) showing different service organization patterns
- **Request Flow**: Client → Gateway → Service WebApi → Shared Middleware → Application Layer (business logic, DB/cache/event calls, inter-service communication via HTTP/gRPC/events)
- **Configuration**: Centralized in `appsettings.shared.Development.json` for shared settings; service-specific configs in each API project's `appsettings.Development.json`.
- **Service Discovery**: Supports Direct, Consul, and CoreDns modes.

## Key Conventions
- **Layering**: Clear separation of Repository, Service (Application), and API layers. DDD is used where appropriate, but not enforced for all services.
- **Repository Layer**: Focuses on data access, not business logic. Entities inherit from base classes (e.g., `EfEntity`, `EfFullAuditEntity`) and may implement interfaces like `ISoftDelete`.
- **Service Layer**: Handles business orchestration, DTO mapping (Mapster/AutoMapper), and transaction control. Interfaces are defined in `Application.Contracts`, implementations in `Application`.
- **API Layer**: Handles routing, input validation, authentication/authorization, and error handling. Controllers are named after resources (e.g., `StudentController`).
- **Configuration**: Use shared and per-service appsettings files. RegisterType and ConfigurationType control service discovery and config loading.
- **Service Communication**: HTTP (Refit), gRPC, and event-based (CAP) are all supported and documented in the wiki.

## Additional AI Assistant Configs
- No other AI assistant config files (Claude, Cursor, Codex, Windsurf, Aider, Cline) were found in this repository.

---

This file summarizes the architecture, conventions, and integration points for Copilot and other AI assistants. If you want to adjust coverage or add details for specific areas, let me know!