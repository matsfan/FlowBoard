## FlowBoard – AI Contributor Essentials

Purpose: Give an agent just enough project-specific context (Clean Architecture + DDD + CQRS + REPR) to ship a safe vertical slice fast.

### Immutable Commitments

1. Layer flow: WebApi → Application → Domain. Infrastructure only plugged in via DI; Domain & Application never depend on EF, FastEndpoints, or web types.
2. REST surface is pure CRUD (POST/GET/PUT/DELETE) on Boards, Columns, Cards. All rename/reorder/move/archive/WIP changes happen via full `PUT` resource updates (or future consensual `PATCH`). Never add verb/action endpoints.
3. Domain + Application use the `Result` / `Result<T>` pattern for all expected failures (validation, not found, conflict). No throwing for control flow.
4. Time via `IClock` only (see `SystemClock`).

### REPR Mapping

Request (DTO in Endpoint) → Endpoint (thin adapter) → Processor (Application handler) → Response (DTO). Example: `Endpoints/Boards/Create.cs` calls `CreateBoardHandler` returning `BoardDto`.

### Project Landmarks

Domain: `Aggregates/Board`, value objects (`ValueObjects/*`), result primitives (`Primitives/*`), contracts (`Abstractions/IBoardRepository.cs`).
Application: Commands/Queries under `UseCases/**/Commands|Queries`, handlers in `UseCases/**/Handlers`, DTOs beside them, DI in `Services/ServiceRegistration.cs`.
Infrastructure: EF + InMemory repos (`Boards/EfBoardRepository.cs`, `Boards/InMemoryBoardRepository.cs`), context `Data/FlowBoardDbContext.cs`.
WebApi: FastEndpoints in `Endpoints/**`, composition root `Program.cs`.
Tests: Architecture rules (`FlowBoard.Architecture.Tests`), Domain invariants, Application handler tests, Infrastructure EF tests.

### Core Conventions

- Endpoint size target < ~30 LOC; zero business logic inside.
- Map domain entities → DTOs only inside Application handlers.
- Commands mutate; Queries read only—never mix.
- Add new persistence ops by extending the Domain repository interface then implementing in BOTH EF + InMemory repos.
- Keep order/WIP invariants enforced by aggregates (Board/Column); don’t “fix” ordering in repositories.

### Adding a Vertical Slice

1. Domain: add/extend VO or aggregate behavior returning `Result` (e.g. adjust ordering or WIP validation). Update tests.
2. Contract: extend `IBoardRepository` if new persistence semantics are required.
3. Application: create command/query record + handler (async) returning `Result<Dto>`; map domain → DTO.
4. Infrastructure: update EF model + both repositories; adjust `FlowBoardDbContext` mapping if needed.
5. WebApi: add FastEndpoint (bind request ⇢ mediator/handler ⇢ translate `Result` to HTTP 200/201/400/404/409).
6. Tests: Domain invariant tests, Application handler test (using InMemory repo or substitute), EF test if mapping changed.

### Quick Commands

Build: `dotnet build src/FlowBoard.sln` | Tests: `dotnet test src/FlowBoard.sln`
Run API: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
Run Web UI: `cd src/FlowBoard.WebApp && npm install && npm run dev` (Vite @ 5173)
Migration: `dotnet ef migrations add <Name> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi`

### Guardrails (Fail Arch Tests if broken)

- No upward (reverse) dependencies; respect layer boundaries.
- No external libs in Domain; keep it pure.
- Never expose Domain/EF types over HTTP.
- Always use `IClock` + `Result`.
- Don’t introduce side effects in endpoints beyond dispatching handlers.

Need deeper detail (telemetry, projections, performance)? Ask with the target file/area.
