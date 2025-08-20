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

### 2. Persistence Strategy
Config toggle `Persistence:UseInMemory` (bool) drives DI:
* In-memory: `InMemoryBoardRepository` (simple `ConcurrentDictionary`).
* EF Core (default): Sqlite connection `ConnectionStrings:FlowBoard` (default `flowboard.db`). `FlowBoardDbContext` configures `Board` with unique index on `Name`.
Add migrations with: `dotnet ef migrations add <Name> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi` (not yet added).

### 3. Patterns & Conventions
* Endpoints: `FlowBoard.Web/Endpoints/<Feature>/<Action>.cs`, inherit `Endpoint<Req,Resp>` (create) or `EndpointWithoutRequest<T>` (read). Keep them thin: map request → handler → map result.
* Result Handling: Domain/Application return `Result` / `Result<T>` (no throwing for expected failures). Endpoints translate failures to 400 (improve to 409 for conflicts later).
* Repositories: Interface in Domain. Infra implements; Application only consumes abstractions.
* DI Extensions: `AddApplication()`, `AddInfrastructure(configuration)` called in `Program.cs` after FastEndpoints/Swagger.
* Time: Use injected `IClock` inside domain factories for testable timestamps.
* Tests: Domain tests have no mocks. Application tests can mock repositories via NSubstitute. Infrastructure tests use in-memory Sqlite. Architecture tests forbid outward references.

### 4. Adding a Vertical Slice (Example: New Aggregate)
1. Domain: Create aggregate + invariants + factory returning `Result<T>`.
2. Contract: Add repository interface methods (only aggregate-lifecycle ops, not query projections).
3. Application: Add handler + DTO (map aggregate fields only). Return `Result<Dto>`.
4. Infra: Implement repository (both in-memory & EF if needed). Add config/migration if schema change.
5. Web: Add endpoints in new folder; translate errors (validation/conflict) to proper HTTP codes.
6. Tests: Domain invariants; handler happy + failure paths; repository EF test if new persistence logic; architecture stays green.

### 5. Build/Test Commands
Build: `dotnet build src/FlowBoard.sln`
Run API: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
Test all: `dotnet test src/FlowBoard.sln`
Focused test project example: `dotnet test tests/FlowBoard.Domain.Tests/FlowBoard.Domain.Tests.csproj -t`.

### 6. Guardrails
* Never reference FastEndpoints or EF Core from Domain/Application.
* Do not return EF entities directly from endpoints—map to DTOs.
* Keep endpoint code ≤ ~30 lines; push logic into handler or domain methods.
* New dependencies belong in Infrastructure unless truly cross-cutting.
* Maintain architecture tests when moving/adding projects (update markers, not rules, unless direction changes intentionally).

### 7. Extensibility Hooks (Future)
* Plan for domain events (not yet implemented) – design aggregates so state changes localize invariants.
* Query side may later introduce read models (separate interfaces rather than inflating repositories).

### 8. Quick Reference File Map
`Domain/Board.cs` – aggregate & invariants.
`Domain/Result.cs` – Result & Error pattern.
`Application/Boards/CreateBoard.cs` – handler + DTO pattern.
`Infrastructure/Data/FlowBoardDbContext.cs` – EF model config.
`Infrastructure/Boards/EfBoardRepository.cs` – EF persistence.
`Web/Endpoints/Boards/Create.cs` – endpoint pattern (POST + GET example).
`tests/**` – layer-specific tests; `ArchitectureRules.cs` enforces layering.

### 9. Common Improvements (When Needed)
* Add 409 Conflict mapping for duplicate names.
* Introduce migration project if schema complexity grows.
* Extract error → HTTP mapping helper for consistent responses.

Keep this file updated when adding a new cross-cutting pattern or modifying dependency direction.
