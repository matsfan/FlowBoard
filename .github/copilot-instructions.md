## FlowBoard – AI Contributor Guide
Focused rules so an AI agent can extend the system safely and fast.

### 1. Architecture (Clean, Layered)
Solution: `src/FlowBoard.sln` (.NET 9).
Layers & direction: Web → Application → Domain. Infrastructure sits beside Web and is depended on (not the other way). ServiceDefaults is cross‑cutting. Tests mirror layers + Architecture + Infrastructure tests.
* Domain (`FlowBoard.Domain`): Aggregates (`Board`), value objects, `Result`/`Error`, repository contracts (`IBoardRepository`), time abstraction (`IClock`). No external library types.
* Application (`FlowBoard.Application`): Use case handlers (e.g., `CreateBoardHandler`), DTOs, DI registration. Pure .NET + Domain only.
* Infrastructure (`FlowBoard.Infrastructure`): Persistence implementations: in‑memory & EF Core (`FlowBoardDbContext`, `EfBoardRepository`). Chooses provider via config.
* Web (`FlowBoard.WebApi`): FastEndpoints endpoints (`Endpoints/**`), Swagger, composition root. Minimal logic; delegates to handlers.
* ServiceDefaults: OpenTelemetry, health checks, resilience wiring.
* Architecture tests: enforce dependency rules (NetArchTest).

## FlowBoard — AI contributor quick guide

Short: FlowBoard is a small Clean‑Architecture .NET 9 solution (Web → Application → Domain). Infrastructure (EF/in‑memory) and ServiceDefaults are cross‑cutting helpers. Keep changes within these layers and preserve the dependency direction.

Core projects (where to look):
- `src/FlowBoard.Domain` — aggregates, `Result` pattern, `IBoardRepository`, `IClock` (no external libs here).
- `src/FlowBoard.Application` — handlers (e.g. `Boards/CreateBoard.cs`), DTOs, DI helpers (`DependencyInjection.cs`).
- `src/FlowBoard.Infrastructure` — `FlowBoardDbContext`, `EfBoardRepository`, `InMemoryBoardRepository` and persistence wiring.
- `src/FlowBoard.WebApi` — FastEndpoints endpoints under `Endpoints/**`, `Program.cs` composition root.

Patterns & repo conventions (concrete):
- Endpoints are thin: map request → call handler → map `Result` to HTTP. See `src/FlowBoard.WebApi/Endpoints/Boards/Create.cs`.
- Domain and Application return `Result` / `Result<T>` (expected failures are not thrown). Endpoints translate to 400/appropriate codes.
- Repositories: interface lives in Domain (`IBoardRepository`); infra provides EF and in‑memory implementations. Toggle via `Persistence:UseInMemory` in configuration.
- Time uses `IClock` (`SystemClock.cs`) — inject for testable timestamps.

Persistence & migration notes:
- Default EF provider uses Sqlite; connection key `ConnectionStrings:FlowBoard` (default file `flowboard.db`).
- Add migrations with: `dotnet ef migrations add <Name> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi`.

Build / run / test (exact commands):
- Build: `dotnet build src/FlowBoard.sln`
- Run API: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
- Run web UI: `cd src/FlowBoard.WebApp && npm install && npm run dev` (vite dev at http://localhost:5173)
- Tests: `dotnet test src/FlowBoard.sln` (CI also collects coverage via XPlat Code Coverage + ReportGenerator).

Integration points & test stacks:
- FastEndpoints (WebApi) — endpoints live under `Endpoints/`.
- EF Core (Infrastructure) and InMemory repository used in tests.
- Tests: Domain tests avoid mocks; Application tests use NSubstitute; Infra tests use in‑memory Sqlite.
- CI uploads coverage (ReportGenerator + Codecov) — see `.github/workflows/ci.yml` and README for coverage commands.

Guardrails (do not change):
- Domain/Application must not reference FastEndpoints or EF Core.
- Do not return EF entities directly from endpoints — map to DTOs in Application.
- Keep endpoint files small (≈ ≤30 lines). Push business rules into domain factories/handlers.

Quick example files to inspect:
- `src/FlowBoard.Domain/Board.cs`
- `src/FlowBoard.Domain/Result.cs`
- `src/FlowBoard.Application/Boards/CreateBoard.cs`
- `src/FlowBoard.Infrastructure/Data/FlowBoardDbContext.cs`
- `src/FlowBoard.Infrastructure/Boards/EfBoardRepository.cs`
- `src/FlowBoard.WebApi/Endpoints/Boards/Create.cs`

When adding a vertical slice (short): Domain (aggregate + factory returning Result) → add repo contract → Application handler + DTO → Infra implementations (in‑memory + EF) → Web endpoints → tests (domain + handler + infra).

If anything in this guide is unclear or you want deeper examples (handler tests, EF mappings, or CI details), tell me which area to expand.
