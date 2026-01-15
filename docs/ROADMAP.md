# Roadmap

## MVP Milestones
1. **Project scaffolding**
   - Solution structure, references, target framework alignment
   - Basic docs and repo hygiene

2. **Auth vertical slice**
   - OAuth callback -> local user upsert
   - JWT issuance + validation
   - Refresh token flow (recommended)
   - `GET /api/me`

3. **Work items core**
   - Create + list + detail
   - Assignment + status transitions
   - Activity log generation

4. **Filters, tags, reporting**
   - Tagging and search filters
   - Status trend report

5. **UI MVP**
   - Dashboard, list, detail, admin user management
   - Role-aware actions

6. **Production hardening**
   - Docker compose (API + UI + DB)
   - Integration tests (Testcontainers)
   - CI pipeline
   - README “How to run” and runbook notes

## Stretch goals (post-MVP)
- Pagination for comments/activity
- Better audit metadata for ActivityLog
- UI polish: virtualization, charts, accessibility
- Safer token exchange (avoid tokens-in-query)
