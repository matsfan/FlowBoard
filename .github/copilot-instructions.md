# FlowBoard – AI Contributor Essentials

Purpose: Give an AI agent just enough repo-specific context (Clean Architecture + DDD + CQRS + FastEndpoints) to ship a safe vertical slice fast.

## Architecture & Layering

- **Flow**: WebApi → Application → Domain. Infrastructure is plugged via DI; Domain & Application never depend on EF, FastEndpoints, or web types. See tests in `tests/FlowBoard.Architecture.Tests/ArchitectureRules.cs`.
- **REST surface**: Pure CRUD on Boards/Columns/Cards. Rename/reorder/move/archive/WIP are expressed as full `PUT` resource updates (or future consensual `PATCH`). Don't add verb/action routes.
- **Time via `IClock` only** (`src/FlowBoard.Domain/SystemClock.cs`). Domain returns `Result`/`Result<T>` for expected failures; don't throw for control flow.

## Endpoints & Handlers

- **Use FastEndpoints**. Endpoints are thin (<~30 LOC) and delegate to MediatR handlers. Example: `Endpoints/Boards/Create/CreateBoardEndpoint.cs` sends `CreateBoardCommand` and returns `BoardDto`.
- **Queries** typically go through handlers (e.g., `ListBoardsEndpoint` → `ListBoardsQuery`), but simple reads may use repositories directly (e.g., `GetById` endpoint injects `IBoardRepository`). Keep endpoints free of business logic.
- **Map domain → DTO exclusively in handlers** (see `UseCases/Boards/Create/CreateBoardHandler.cs` and `UseCases/Boards/BoardDto.cs`).
- **All endpoints use `AllowAnonymous()`** for dev simplicity. Route groups: `BoardsGroup` for `/boards`, `CardsGroup` for `/boards/{boardId}/columns/{columnId}/cards`.

## Domain Model

- **Aggregate**: `src/FlowBoard.Domain/Aggregates/Board.cs` manages Columns and Cards. Invariants: name rules, order normalization, WIP limits, idempotent archive/unarchive, move/reorder validation.
- **Value objects** live under `ValueObjects/*` (e.g., `BoardName`, `OrderIndex`, `WipLimit`, `*Id`).

## Persistence & DI

- **Contract**: `Application/Abstractions/IBoardRepository.cs`. Implement in BOTH:
  - **EF Core**: `Infrastructure/Persistence/Ef/Repositories/EfBoardRepository.cs`; model in `Infrastructure/Persistence/Ef/FlowBoardDbContext.cs` (ValueConverters for IDs/Order, owned types for names, owns-many for Columns/Cards with tables `Columns`/`Cards`).
  - **InMemory**: `Infrastructure/Persistence/InMemory/InMemoryBoardRepository.cs`.
- **Composition**: `WebApi/Program.cs` registers FastEndpoints, Swagger, MediatR (scans Application), and CORS for `http://localhost:5173`. Infra DI (`Infrastructure/Services/ServiceRegistration.cs`) registers `IClock` and chooses EF vs InMemory via config key `Persistence:UseInMemory` (false → SQLite `FlowBoard` connection string by default).

## HTTP Error Mapping

- Handlers return `Result`; endpoints translate: 400 for validation/conflict (current convention), 404 for not found (see `GetById`). Avoid exposing domain/EF types over HTTP.

## Working Rules (Checklist)

- **Commands mutate; Queries read-only**—never mix behavior in one handler.
- Add new persistence ops by extending the domain repository interface, then implement in EF + InMemory. Don't "fix" ordering in repos—aggregates enforce it.

## Quick Commands

### Build & Test

```bash
# Build entire solution
dotnet build src/FlowBoard.sln

# Run all tests
dotnet test src/FlowBoard.sln

# Coverage report (macOS)
dotnet test src/FlowBoard.sln --collect:"XPlat Code Coverage" --results-directory coverage
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.*
reportgenerator -reports:coverage/**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
open coverage-report/index.html
```

### Dev Server Management (Critical)

**Always check for running processes before starting servers** to avoid port conflicts:

```bash
# Check for processes on dev ports (macOS/Linux)
lsof -i :5173 -i :5174 -i :56157 -i :56158

# Kill processes on specific ports if needed
lsof -ti:5173 | xargs kill -9  # Repeat for other ports
```

**Default ports:**

- WebApi: https://localhost:56157, http://localhost:56158 (see `src/FlowBoard.WebApi/Properties/launchSettings.json`)
- WebApp (Vite): http://localhost:5173 by default

**Start servers:**

```bash
# Start WebApi (from repo root)
dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj

# Start WebApp (from repo root)
cd src/FlowBoard.WebApp && npm install && npm run dev
```

**Access points:**

- **Swagger UI**: http://localhost:56158/swagger (or https://localhost:56157/swagger)
- **WebApp**: http://localhost:5173
- **API ping**: http://localhost:56158/ping

### Database Operations

```bash
# Add migration
dotnet ef migrations add <MigrationName> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi

# Apply migrations
dotnet ef database update -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi

# Reset dev database (stop app first)
rm src/FlowBoard.WebApi/flowboard.dev.db
dotnet ef database update -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi
```

## Frontend Integration

- **Vite config** (`src/FlowBoard.WebApp/vite.config.ts`) proxies `/api` calls to `http://localhost:56158`
- **Hash routing**: `#/` → boards list, `#/boards/:id` → board page
- **API calls** use `/api` prefix, automatically proxied to backend
- **CORS** enabled for `http://localhost:5173` in `Program.cs`

## Adding a Vertical Slice (TL;DR)

1. **Domain behavior** (return `Result`)
2. **Extend `IBoardRepository`** if needed
3. **Command/Query + Handler** returning `Result<Dto>`
4. **EF + InMemory repos** + DbContext mapping
5. **Endpoint** calling handler and mapping `Result` → HTTP
6. **Tests** (domain + handler; EF if mapping changed)

## Testing Strategy

- **Domain tests**: `tests/FlowBoard.Domain.Tests/`
- **Application tests**: `tests/FlowBoard.Application.Tests/`
- **Infrastructure tests**: `tests/FlowBoard.Infrastructure.Tests/`
- **Integration tests**: `tests/FlowBoard.WebApi.Tests/` (uses `WebApplicationFactory`)
- **Architecture tests**: `tests/FlowBoard.Architecture.Tests/ArchitectureRules.cs`

## Common Patterns

- **Endpoint structure**: Configure route/group/auth → delegate to handler → map result to HTTP
- **Handler structure**: Validate → call domain/repository → map to DTO → return Result
- **Domain structure**: Value objects for validation → aggregate methods for business logic → return Result
- **Repository structure**: Interface in Application → implement in both EF and InMemory