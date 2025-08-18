# Copilot Project Instructions: FlowBoard

Concise guide for AI agents to work productively in this repo.

## 1. Architecture Overview
Solution: `src/FlowBoard.sln` (.NET 9). Layers:
- `FlowBoard.Domain`: Core model (currently empty stubs). Must stay free of external deps.
- `FlowBoard.Application`: Use cases / orchestrators (empty now). Should depend only on Domain.
- `FlowBoard.Infrastructure`: Implementations of persistence/integration (not yet added). Depends on Domain & Application if needed, not vice‑versa.
- `FlowBoard.Web`: HTTP edge using FastEndpoints + Swagger. Composition root; references ServiceDefaults.
- `FlowBoard.ServiceDefaults`: Centralized cross-cutting setup (OpenTelemetry, health checks, service discovery, resilience policies).
- `FlowBoard.AppHost`: Aspire hosting/bootstrap (currently minimal).
- Tests: One project per layer + `FlowBoard.Architecture.Tests` (placeholder for dependency rule tests).

Clean Architecture intent: Dependencies point inward (Web -> Application -> Domain). Infrastructure plugs in via interfaces. Enforce via architecture tests when implemented.

## 2. Current HTTP Stack
Using FastEndpoints (see `FlowBoard.Web/Program.cs`). Endpoints live under `FlowBoard.Web/Endpoints/**`. Example: `PingEndpoint.cs` defines `GET /ping`.
Add new endpoints by creating classes inheriting `Endpoint<Req,Resp>` (or `EndpointWithoutRequest<T>`), then FastEndpoints auto-discovers.

Swagger: Configured via `builder.Services.SwaggerDocument(...)`; exposed with `app.UseSwaggerGen()`. Title: "FlowBoard API".

## 3. Conventions (Adopt / Maintain)
- Place endpoint classes in `FlowBoard.Web/Endpoints/<Feature>/` folders.
- Keep domain entities/value objects inside `FlowBoard.Domain` only; do NOT introduce EF Core or framework types here.
- Application layer will host command/query handlers or services; keep them free of FastEndpoints or ASP.NET types.
- Introduce interfaces (e.g., `IBoardRepository`) in Domain or Application; implement in Infrastructure.
- Cross-cutting (logging, tracing, health) belongs in ServiceDefaults or composition root, not in Domain/Application code.

## 4. Planned Patterns (Respect Existing Shape)
Though not implemented yet, code you add should anticipate:
- CQRS style handlers (optionally Mediatr later). Consider creating a thin dispatcher abstraction if needed.
- Result type or error notification pattern instead of throwing for expected failures.
- Domain events raised inside entities and dispatched after persistence (design for but do not prematurely optimize).

## 5. Adding a Feature (Example Workflow)
1. Define domain model (e.g., `Board` aggregate) in `FlowBoard.Domain` with invariants.
2. Add repository interface (e.g., `IBoardRepository`).
3. Create application use case (e.g., `CreateBoardHandler`) consuming repository.
4. Implement repository in Infrastructure (e.g., in-memory placeholder first) and register in Web `Program.cs` (or a dedicated DI extension).
5. Expose via FastEndpoint (e.g., `Endpoints/Boards/Create.cs`) calling application handler.
6. Write unit tests in Domain.Tests & Application.Tests; add architecture test locking dependency directions.

## 6. Testing Guidance
- Prefer pure unit tests for Domain invariants (no mocks needed).
- Application tests may mock repository interfaces.
- Architecture tests (to be implemented) should assert no forbidden references (e.g., Domain not referencing Web or Infrastructure).

## 7. Build & Run
Build solution:
```
dotnet build src/FlowBoard.sln
```
Run Web API:
```
dotnet run --project src/FlowBoard.Web/FlowBoard.Web.csproj
```
Tests:
```
dotnet test src/FlowBoard.sln
```

## 8. External Dependencies
- FastEndpoints (+ Swagger package) for minimal endpoint hosting.
- OpenTelemetry & resilience via ServiceDefaults (Aspire style).
No persistence library added yet (expect EF Core or alternative later).

## 9. DI & Composition
All service registrations currently occur directly in `Program.cs` (FastEndpoints, Swagger, ServiceDefaults). Future additions: add extension methods (e.g., `AddApplication()`, `AddInfrastructure()`) to keep Program.cs slim.

## 10. Agent Do / Avoid
Do:
- Keep new framework dependencies out of Domain/Application.
- Add small vertical slices (Domain -> Application -> Endpoint) incrementally.
- Provide tests with new logic.
Avoid:
- Adding business logic directly in endpoint handlers.
- Coupling Infrastructure details (EF contexts, HTTP clients) to Domain types.
- Expanding ServiceDefaults with feature logic.

## 11. Open Tasks / Gaps (Good First Enhancements)
- Introduce first domain entity & repository abstraction.
- Implement in-memory Infrastructure layer.
- Add architecture enforcement tests.
- Add Result type and validation strategy.

Keep instructions concise; update this file if conventions evolve.
