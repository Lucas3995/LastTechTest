# AGENTS.md

## Cursor Cloud specific instructions

### Overview

LastTechTest is a receivables anticipation platform (Brazilian fintech domain). It consists of:

- **Backend**: .NET 10 / ASP.NET Core Web API with Clean Architecture, SQLite (embedded, zero-config), JWT auth
- **Frontend**: Angular 20 SPA with Angular Material, TypeScript, Vitest unit tests, Playwright E2E

### Running services

**Backend API** (port 5114 for proxy compatibility):
```bash
cd backend
ASPNETCORE_ENVIRONMENT=Development dotnet run --project LastTechTest.API/LastTechTest.API.csproj --urls "http://localhost:5114"
```

> **Gotcha**: The `launchSettings.json` default profile uses port 5062, but `frontend/proxy.conf.json` expects port 5114. Always start the backend with `--urls "http://localhost:5114"` for local dev, or the frontend proxy will fail.

**Frontend dev server** (port 4200):
```bash
cd frontend
npm run start
```

### Standard commands

See `frontend/README.md` for all frontend commands (`npm run lint`, `npm run test`, `npm run e2e`, etc.).

Backend tests from repo root: `dotnet test backend/LastTechTest.Testes/LastTechTest.Testes.csproj -c Release`

### Seed user

The backend auto-seeds an admin user on startup:
- Email: `usu_acesso_total@example.com`
- Password: `Acess0@t0ta1`
- Role: `Admin`

### Non-obvious notes

- SQLite DB is auto-created by EF Core on backend startup. No external database setup needed.
- The backend migration log may show an `ERR` for `ALTER TABLE ... ADD COLUMN "RequestedAtUtc"` on startup if the column already exists — this is benign and does not affect functionality.
- Frontend "Nova solicitação" and "Simular antecipação" buttons on the empty state are not wired up in the UI yet; use the API directly (`POST /api/v1/anticipations`) to create requests.
- Playwright E2E tests require `npx playwright install chromium` before first run.
