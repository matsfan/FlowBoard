# FlowBoard

[![CI](https://github.com/matsfan/FlowBoard/actions/workflows/ci.yml/badge.svg)](https://github.com/matsfan/FlowBoard/actions/workflows/ci.yml)
[![Coverage](https://codecov.io/gh/matsfan/FlowBoard/branch/main/graph/badge.svg?token=YOUR_TOKEN_HERE)](https://codecov.io/gh/matsfan/FlowBoard)

A kanban board app (clean architecture, FastEndpoints, EF Core optional, OpenTelemetry).

## CI & Coverage

Continuous Integration runs on pushes & pull requests to `main` (`.github/workflows/ci.yml`). Steps:

1. Restore & build solution targeting .NET 9.
2. Run all tests with Coverlet (`--collect:"XPlat Code Coverage"`).
3. Generate HTML + Markdown coverage report (ReportGenerator).
4. Upload `coverage-report/` as an artifact and (on PRs) comment a sticky summary.
5. Upload raw coverage to Codecov (monitoring only – no gates enforced).

### Local Coverage Run

```bash
dotnet test src/FlowBoard.sln --collect:"XPlat Code Coverage" --results-directory coverage
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.*
reportgenerator -reports:coverage/**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
open coverage-report/index.html  # (macOS) view report
```

### Codecov Setup

Coverage is uploaded automatically. To finish setup:

1. Create the project at [https://about.codecov.io/](https://about.codecov.io/) and note the repository slug.
2. Add a repository secret `CODECOV_TOKEN` with the upload token (Settings → Secrets → Actions).
3. (Optional) Replace `YOUR_TOKEN_HERE` in the badge URL above with the public token if required (some org setups can omit it).
4. Visit the Codecov dashboard after a CI run to view trends, file hotspots, and PR diffs.

No coverage gates are configured; add them later by setting `fail_ci_if_error: true` or enabling Codecov status checks.

## Project Structure (Brief)

`src/FlowBoard.Domain` – Aggregates, Result pattern, contracts.
`src/FlowBoard.Application` – Use case handlers & DTOs.
`src/FlowBoard.Infrastructure` – EF Core / in-memory persistence.
`src/FlowBoard.WebApi` – FastEndpoints API & composition root.
`tests/**` – Unit, integration, architecture tests.

## Quick Commands

Build:

```bash
dotnet build src/FlowBoard.sln
```

Run API:

```bash
dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj
```

Run Web UI (Vite React + Tailwind):

```bash
cd src/FlowBoard.WebApp
npm install # first time
npm run dev
```

The dev server runs on <http://localhost:5173> and proxies API calls starting with `/api` to the backend (adjust target port in `vite.config.ts` if your ASP.NET app listens on a different port). CORS is enabled for the dev origin.

Production build:

```bash
cd src/FlowBoard.WebApp && npm run build
```

Resulting static assets are emitted to `dist/` (you can later integrate a hosting strategy – e.g. serve via ASP.NET or deploy separately to static hosting).

Run tests:

```bash
dotnet test src/FlowBoard.sln
```

---
Keep README concise; expand only when developer onboarding friction appears.
