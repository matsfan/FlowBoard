## FlowBoard – Focused AI Contributor Guide

Lean rules so an agent can add features safely and fast. This repo follows Clean Architecture, Domain‑Driven Design (DDD), Command Query Responsibility Segregation (CQRS), and the REPR interaction flow. Keep edits inside these boundaries.

### Architecture (DO NOT BREAK)

Flow: WebApi → Application → Domain (Clean Architecture). Infrastructure is an implementation detail used by Web (and registered in DI) but Domain/Application never reference EF, FastEndpoints, or UI. ServiceDefaults supplies cross‑cutting telemetry/resilience.

Interaction model uses REPR: Request → Endpoint → Processor → Response.

- Request: HTTP/transport contract and validation.
- Endpoint: Thin adapter that translates request → Application command/query and maps Result → HTTP.
- Processor: Application handler (use case) invoking Domain aggregates and repositories.
- Response: DTOs shaped for clients; never return domain entities directly.

Projects (inspect first):

- Domain (`src/FlowBoard.Domain`): DDD model (aggregates like `Aggregates/Board`), value objects, `Result` pattern (`Primitives/Result*`), repository contracts (`Abstractions/IBoardRepository`), time (`SystemClock` via `IClock`). No external library types. Business invariants live here.
- Application (`src/FlowBoard.Application`): CQRS use cases under `UseCases/**` (e.g. `UseCases/Boards/Handlers/CreateBoardHandler.cs`), commands/queries (records), DTOs, DI (`Services/ServiceRegistration.cs`). Handlers orchestrate domain behavior and map to DTOs. Only Domain + BCL.
- Infrastructure (`src/FlowBoard.Infrastructure`): EF Core + in‑memory persistence (`Data/FlowBoardDbContext.cs`, `Boards/EfBoardRepository.cs`, `Boards/InMemoryBoardRepository.cs`). Chooses implementation via `Persistence:UseInMemory` flag in `appsettings*`.
- WebApi (`src/FlowBoard.WebApi`): FastEndpoints endpoints (`Endpoints/**`), composition root (`Program.cs`), Swagger. Endpoints implement the REPR adapter: parse input, call Application handler, translate `Result` to HTTP.
- Tests (`tests/**`): Domain (no mocks), Application (NSubstitute), Infrastructure (in‑memory Sqlite), Architecture (dependency rules).

### Core Conventions

- Endpoints stay thin (< ~30 LOC). Example: `Endpoints/Boards/Create.cs` just: parse → call handler → translate `Result` to HTTP.
- All business decisions/invariants live in Domain or Application handler; never in endpoint or EF repo.
- Use the `Result` / `Result<T>` pattern for expected failures (no throwing for flow control).
- Never return EF entities or Domain objects directly from WebApi; map to DTOs in Application (see `CreateBoardHandler` producing `BoardDto`).
- Time: request `IClock` (do not call `DateTime.UtcNow` directly) to keep tests deterministic.
- CQRS separation:
  - Commands change state and return minimal info or identifiers/DTOs.
  - Queries do not change state and may read optimized projections.
  - Do not mix command and query concerns in a single handler.
- REPR adherence:
  - Request contracts live at the edge (WebApi).
  - Endpoint is a pass‑through adapter.
  - Processor = Application handler.
  - Response is shaped DTO with appropriate HTTP codes.

### Adding a Vertical Slice (Board-like example)

1. Domain (DDD): Add/extend aggregate or value objects; expose factory/behavior returning `Result` and enforcing invariants.
2. Contract: Add/extend repository interface in Domain if new persistence operations needed.
3. Application (CQRS): Create a command or query record + handler (async method returning `Result<Dto>` for commands or `Result<QueryDto>` for queries). Register in `Services/ServiceRegistration.cs`.
4. Infrastructure: Implement repo changes in both `Ef...Repository` and `InMemory...Repository`; update `Data/FlowBoardDbContext.cs` mapping if needed.
5. WebApi (REPR): Add FastEndpoint in `Endpoints/<Area>/` that binds the Request, calls the handler (Processor), and returns a Response (DTO) with proper HTTP status from the `Result`.
6. Tests: Domain invariants (no mocks); Application handler tests using in‑memory repo substitute; Infrastructure tests (if mapping/data) with in‑memory Sqlite. Include both command and query tests when applicable.

### Key Files To Model

`Board.cs`, `Result.cs`, `UseCases/Boards/Handlers/CreateBoardHandler.cs`, `Endpoints/Boards/Create.cs`, `Data/FlowBoardDbContext.cs`.
Also see examples of CQRS and REPR mapping in `tests/FlowBoard.Application.Tests/*HandlerTests.cs` and `src/FlowBoard.WebApi/Endpoints/**`.

### Persistence & Migrations

Sqlite default file `flowboard.db` (connection: `ConnectionStrings:FlowBoard`). Create migration:
`dotnet ef migrations add <Name> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi`

### Daily Commands

Build: `dotnet build src/FlowBoard.sln`
Run API: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
Run Web UI: `cd src/FlowBoard.WebApp && npm install && npm run dev` (Vite @ http://localhost:5173)
Tests (all): `dotnet test src/FlowBoard.sln`

### Guardrails (Enforced / Expected)

- No reverse dependencies (Arch tests will fail if broken).
- Do not introduce external libs into Domain.
- Preserve Result-based flow; handle errors at boundaries.
- Keep endpoints deterministic & side‑effect free beyond delegating.

Additional guardrails for DDD/CQRS/REPR:

- Keep aggregates persistence‑ignorant; do not inject DbContext or infrastructure types into Domain/Application.
- Keep commands free of read‑heavy logic; prefer separate query handlers and projections where necessary.
- Do not leak HTTP concepts into Application/Domain; WebApi translates to/from HTTP.
- Maintain deterministic handlers by using `IClock` and repository abstractions.

Need deeper detail (e.g. telemetry wiring, DTO mapping tips, performance)? Ask specifying the area and file you want expanded.
