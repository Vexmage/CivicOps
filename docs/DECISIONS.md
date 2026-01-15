# Decisions (ADR-lite)

This file records key architectural decisions so future changes are intentional.

## 001 — Minimal APIs
**Decision:** Use ASP.NET Core Minimal APIs with endpoint groups.  
**Rationale:** Lightweight, modern, clear routing; avoids controller ceremony for a small service.  
**Tradeoffs:** Requires discipline to avoid a “giant Program.cs”; mitigated by grouping and extension methods.

## 002 — PostgreSQL for persistence
**Decision:** Use PostgreSQL with EF Core (Npgsql).  
**Rationale:** Free/low-cost hosting options, strong relational features, solid EF provider.  
**Tradeoffs:** Slightly different tooling than SQL Server; acceptable for MVP.

## 003 — OAuth for login, JWT for API access
**Decision:** External OAuth provider for identity; API issues JWT access tokens.  
**Rationale:** Avoid password storage; realistic auth pattern; clear security story.  
**Tradeoffs:** Token handling complexity; mitigated via refresh token cookie rotation.

## 004 — Contracts project for DTOs
**Decision:** All request/response models live in `CivicOps.Contracts`.  
**Rationale:** Prevents EF leakage, stabilizes API surface, allows UI to share types.  
**Tradeoffs:** Extra project and explicit mapping work; worth it.

## 005 — Start with a single vertical slice
**Decision:** Build auth + `/api/me` before work item features.  
**Rationale:** Auth shapes everything; proves the stack early.  
**Tradeoffs:** Delays visible “app features” briefly; reduces rework.
