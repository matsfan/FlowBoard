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

Access Swagger UI:

Once the API is running, navigate to:
- **https://localhost:56157/swagger** (HTTPS)
- **http://localhost:56158/swagger** (HTTP)

The Swagger UI provides interactive documentation for all available endpoints and allows you to test the API directly from the browser.

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

## Database Migrations

FlowBoard uses Entity Framework Core for data persistence with SQLite. When making changes to the domain model, you'll need to create and apply database migrations.

### Prerequisites

Install the Entity Framework Core tools globally (if not already installed):

```bash
dotnet tool install --global dotnet-ef
```

Add the tools directory to your PATH (for macOS/Linux with zsh):

```bash
export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"
```

Or add permanently to your shell profile:

```bash
echo 'export PATH="$PATH:/Users/$(whoami)/.dotnet/tools"' >> ~/.zprofile
```

### Creating a New Migration

When you modify domain entities, value objects, or DbContext configuration:

```bash
dotnet ef migrations add YourMigrationName -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

Example:

```bash
dotnet ef migrations add AddCardPriorityField -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

This creates migration files in `src/FlowBoard.Infrastructure/Persistence/Ef/Migrations/`.

### Applying Migrations

Apply pending migrations to create/update the database:

```bash
dotnet ef database update -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

### Viewing Migration Status

Check which migrations have been applied:

```bash
dotnet ef migrations list -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

### Rolling Back Migrations

Revert to a specific migration:

```bash
dotnet ef database update PreviousMigrationName -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

### Removing the Last Migration

If you haven't applied a migration yet and want to remove it:

```bash
dotnet ef migrations remove -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

### Development Database

The development database (`flowboard.dev.db`) is created automatically when you run the application for the first time. The application includes seed data for testing purposes.

To reset your development database:

1. Stop the application
2. Delete the `flowboard.dev.db` file from the WebApi project root
3. Run `dotnet ef database update` to recreate with seed data

## Using Custom Mediator in FlowBoard

This repo uses a custom mediator implementation for application-level commands and queries. Handlers live in `src/FlowBoard.Application/UseCases/**/` and implement `IRequestHandler<TRequest, TResponse>`.

- Commands: mutate state and return `Result` or `Result<TDto>` (see `CreateBoardCommand`, `AddCardCommand`).
- Queries: read-only and return `Result<TDto>` collections or single DTOs (see `ListBoardsQuery`).

Registration:

- The custom mediator is registered in the Application layer's `ServiceRegistration.cs`, which automatically discovers and registers all handlers.
- The Web API composition root calls `AddApplicationServices()` to register the mediator and handlers: see `src/FlowBoard.WebApi/Program.cs`.

Endpoint pattern:

- Inject `IMediator` into FastEndpoints endpoints and call `await mediator.Send(new YourCommand(...), ct)`.
- Translate `Result` to HTTP in the endpoint (400 for domain/validation errors, 201/200 for success).

Custom Mediator Implementation:

- **Interfaces**: `IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`, and `IMediator` in `src/FlowBoard.Application/Abstractions/`
- **Implementation**: Simple reflection-based mediator in `src/FlowBoard.Application/Services/Mediator.cs`
- **Benefits**: No external dependencies, no licensing costs, full control over the implementation

Add a new use case by:

1. Creating a command or query record in `UseCases/<Area>/` implementing `IRequest<Result|Result<T>>`.
2. Implementing a handler in `UseCases/<Area>/` implementing `IRequestHandler<TRequest, Result|Result<T>>` and orchestrating the Domain.
3. Calling it from a FastEndpoint via `IMediator`.

## API (REST CRUD)

The Web API now follows a conventional RESTful CRUD surface. Earlier fine‑grained mutation endpoints (e.g. `/rename`, `/reorder`, `/move`, `/wip`, `/archive`, `/description`) have been removed in favor of expressing state changes through standard `PUT` (full update) or `DELETE` (removal) requests. This keeps the transport surface small while Domain/Application layers still enforce rich invariants.

Base URL (dev): `http://localhost:5000` (or whatever Kestrel assigns). Routes shown relative.

### Boards

- `POST /boards` – create `{ name }` → 201 returns `{ id, name, createdUtc }`
- `GET /boards` – list → 200 `[ { id, name, createdUtc } ]`
- `GET /boards/{boardId}` – fetch single → 200 `{ id, name, createdUtc }` or 404
- `PUT /boards/{boardId}` – full update `{ name }` → 200 updated dto

### Columns

- `POST /boards/{boardId}/columns` – create `{ name, wipLimit? }` → 201 `{ id, boardId, name, order, wipLimit }`
- `GET /boards/{boardId}/columns` – list → 200 `[ { id, name, order, wipLimit } ]`
- `GET /boards/{boardId}/columns/{columnId}` – fetch → 200 `{ id, name, order, wipLimit }`
- `PUT /boards/{boardId}/columns/{columnId}` – full update `{ name, order, wipLimit? }` (null clears WIP limit) → 200 updated dto

### Cards

- `POST /boards/{boardId}/columns/{columnId}/cards` – create `{ title, description? }` → 201 `{ id, title, description, order, isArchived, createdUtc, columnId, boardId }`
- `GET /boards/{boardId}/columns/{columnId}/cards` – list cards in a column → 200 `[ ... ]`
- `GET /boards/{boardId}/columns/{columnId}/cards/{cardId}` – fetch → 200 card dto
- `PUT /boards/{boardId}/columns/{columnId}/cards/{cardId}` – full update. Body may change:
  - `title`
  - `description` (null/empty allowed)
  - `columnId` (moving between columns)
  - `order` (reordering within target column)
  - `isArchived` (true to archive, false to unarchive)
    Returns updated dto.
- `DELETE /boards/{boardId}/columns/{columnId}/cards/{cardId}` – remove card → 200 (idempotent if already deleted/archive-delete semantics handled in Domain).

### Error Handling

All application/domain validation failures return HTTP 400 with FastEndpoints default problem shape (an `Errors` array). Typical domain codes:

- Not Found: `Board.NotFound`, `Column.NotFound`, `Card.NotFound`
- Validation: `Card.Title.Empty`, `Card.Title.TooLong`, `Column.WipLimit.Invalid`, `Column.WipLimit.Violation`, `Card.Move.InvalidOrder`

### Rationale for Consolidation

| Concern                 | Old Style (Fine-Grained)       | New REST CRUD                           | Benefit                                        |
| ----------------------- | ------------------------------ | --------------------------------------- | ---------------------------------------------- |
| Rename / Reorder / Move | Separate verb-like POST routes | Express via single `PUT` with new state | Fewer endpoints; simpler client SDK generation |
| Archive / Unarchive     | Custom `/archive` route        | `PUT` toggling `isArchived`             | Symmetric & discoverable                       |
| Set/Clear WIP           | `/wip` route                   | `PUT` with `wipLimit` nullable          | Consistent update semantics                    |
| Description edits       | Dedicated route                | Part of `PUT` body                      | Reduced chattiness                             |

The domain model still distinguishes operations (and enforces rules like WIP limits and order recalculation); only the HTTP surface changed.

### Partial vs Full Updates

Currently `PUT` expects a complete representation for the resource fields we manage (name/order/wipLimit for columns; title/description/columnId/order/isArchived for cards). If partial updates become desirable, a `PATCH` style (JSON Patch or merge-patch) can be introduced later—avoiding overloading current `PUT`.

---

Keep README concise; expand only when developer onboarding friction appears.
