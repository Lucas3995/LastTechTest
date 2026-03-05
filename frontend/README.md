# Frontend (Angular 20)

Base Angular frontend for LastTechTest: structure by layers and by guide (areas, modules, pages, components, directives, services), layout shell, design tokens, Angular Material + CDK, Vitest for unit tests, Playwright for E2E. See `.cursor/rules/angular-frontend.mdc` and `.cursor/guia-angular.txt` for conventions.

## Prerequisites

- **Node.js 20+** (22 recommended). If the host has an older Node, use Docker (see below).

## Install and run

```bash
cd frontend
npm ci
npm run start
```

Open `http://localhost:4200/`.  
**API proxy:** The dev server proxies `/auth`, `/api` and `/user` to `http://localhost:5114`. Start the backend (e.g. `docker compose -f docker/docker-compose.yml up api` or `dotnet run` from `backend`) so login and anticipation requests reach the API.

## Commands

| Command | Description |
|--------|-------------|
| `npm run start` | Dev server (port 4200) |
| `npm run build` | Production build → `dist/frontend` |
| `npm run test` | Unit tests (Vitest) |
| `npm run test:coverage` | Unit tests with coverage report |
| `npm run e2e` | E2E tests (Playwright; starts dev server if needed) |
| `npm run e2e:headed` | E2E tests in headed browser |
| `npm run lint` | ESLint (includes template accessibility rules) |

**E2E:** First time or after Playwright update, run `npx playwright install chromium` (or `npx playwright install` for all browsers).

## Tests: where to put what

- **Unit:** next to source as `*.spec.ts` (Vitest).
- **Integration:** same runner; use `*.integration.spec.ts` or a dedicated folder; use `TestBed`, `provideHttpClientTesting()`, mocks for domain interfaces.
- **E2E:** in `e2e/*.spec.ts` (Playwright).

## Structure (layers + guide)

- **Layers:** `domain/`, `application/`, `infrastructure/`, `core/`, `shared/`, `features/`, `areas/`.
- **Guide:** Areas = `areas/<area>/`; modules = `features/<module>/` with `pages/`, `components/`, optionally `directives/`, `services/`; pages = `features/<module>/pages/<page>/`; shared = `shared/components/`, `shared/directives/`, `shared/pipes/` + layout shell.

## Docker: front + back with one command

From the repo root:

```bash
docker compose -f docker/docker-compose.yml up --build
```

- **API:** http://localhost:5114  
- **Frontend:** http://localhost:4200  

The app calls the backend directly (e.g. `http://localhost:5114/auth/login`). In the browser’s Network tab you will see requests to the backend port and route (e.g. login to port 5114). Ensure the API allows CORS from the frontend origin (e.g. `http://localhost:4200`).  

## Frontend tests in Docker (no local Node 20+)

From the repo root:

```bash
./scripts/frontend-test-docker.sh
```

Runs `npm ci` and `npm run test` inside a Node 22 container. Optional: add E2E in the script with `npx playwright install chromium && npm run e2e`.

## Linting and format

- **ESLint:** `npm run lint` (includes `templateAccessibility` for HTML).
- **Prettier:** config in `.prettierrc`; run via editor or `npx prettier --write "src/**/*.{ts,html,scss}"`.
