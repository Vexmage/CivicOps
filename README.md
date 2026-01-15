# CivicOps

CivicOps is a lightweight internal operations platform for small organizations to track work items, roles, and progress.  
Built with ASP.NET Core Minimal APIs, PostgreSQL, OAuth + JWT authentication, and a Blazor UI.

## Problem
Small organizations often track internal work using ad-hoc tools (email, spreadsheets, chat). This leads to poor visibility, unclear ownership, and weak accountability. CivicOps provides a lightweight internal system for tracking work items, roles, and progress.

## Goals (MVP)
- **Visibility:** filterable lists, status summaries, recent activity
- **Ownership:** explicit assignment, role-based permissions
- **Accountability:** auditable changes (activity log), status transitions with rules, basic reporting

## Non-goals (MVP)
- Notifications (email/Slack)
- File attachments/uploads
- Multi-tenant organizations
- Complex workflows beyond a simple status enum
- Microservices

## Tech Stack
- **Backend:** ASP.NET Core (Minimal APIs)
- **Auth:** OAuth login + JWT access tokens (refresh token cookie recommended)
- **Data:** PostgreSQL + EF Core
- **UI:** Blazor (Server for MVP)
- **DevOps:** Docker Compose (local), GitHub Actions (CI)

## Repo Structure
- `src/CivicOps.Api` — Minimal API backend
- `src/CivicOps.Domain` — Domain entities and rules
- `src/CivicOps.Data` — EF Core DbContext + migrations
- `src/CivicOps.Contracts` — DTOs / API contracts
- `src/CivicOps.Ui` — Blazor UI
- `tests/CivicOps.ApiTests` — Integration tests (API)

## Documentation
- [Architecture](docs/ARCHITECTURE.md)
- [Decisions](docs/DECISIONS.md)
- [Roadmap](docs/ROADMAP.md)

## Getting Started (placeholder)
Coming soon. Initial focus is establishing a vertical slice: authentication + `/api/me`.

## License
TBD