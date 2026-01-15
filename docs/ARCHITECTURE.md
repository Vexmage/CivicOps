# Architecture

CivicOps is a layered .NET application designed to stay understandable as it grows.

## Layers
- **Domain (`CivicOps.Domain`)**
  - Entities, enums, and business rules
  - No EF Core, no web concerns, no infrastructure dependencies

- **Contracts (`CivicOps.Contracts`)**
  - Request/response DTOs (API surface)
  - Stable contract layer used by API and UI

- **Data (`CivicOps.Data`)**
  - EF Core DbContext, mappings, migrations
  - Persistence and database-specific concerns

- **API (`CivicOps.Api`)**
  - Minimal API endpoints grouped under `/api`
  - AuthN/AuthZ, validation, error handling, logging

- **UI (`CivicOps.Ui`)**
  - Blazor front-end consuming the API
  - Role-aware UI but server remains source of truth for permissions

## Key Principles
- **No EF entities in API responses.** Use DTOs.
- **Authorization is enforced in the API.** UI hides/disabled actions, but cannot grant access.
- **Prefer explicitness over cleverness.** Especially around auth, data access, and status transitions.

## Planned Endpoint Groups
- `/api/auth/*`
- `/api/me`
- `/api/users/*` (Admin only)
- `/api/work-items/*`
- `/api/reports/*`

## Data Model (MVP)
- Users, ExternalLogins, RefreshTokens
- WorkItems, Comments, ActivityLogs
- Tags (many-to-many)
