## FlowBoard – AI Contributor Essentials

Purpose: Give an AI agent just enough repo-specific context (Clean Architecture + DDD + CQRS + REPR) to ship a safe vertical slice fast.

Architecture & layering
- Flow: WebApi → Application → Domain. Infrastructure is plugged via DI; Domain & Application never depend on EF, FastEndpoints, or web types. See tests in `tests/FlowBoard.Architecture.Tests/ArchitectureRules.cs`.
- REST surface is pure CRUD on Boards/Columns/Cards. Rename/reorder/move/archive/WIP are expressed as full `PUT` resource updates (or future consensual `PATCH`). Don’t add verb/action routes.
- Time via `IClock` only (`src/FlowBoard.Domain/SystemClock.cs`). Domain returns `Result`/`Result<T>` for expected failures; don’t throw for control flow.

Endpoints & handlers
- Use FastEndpoints. Endpoints are thin (<~30 LOC) and delegate to MediatR handlers. Example: `Endpoints/Boards/Create/CreateBoardEndpoint.cs` sends `CreateBoardCommand` and returns `BoardDto`.
- Queries typically go through handlers (e.g., `ListBoardsEndpoint` → `ListBoardsQuery`), but simple reads may use repositories directly (e.g., `GetById` endpoint injects `IBoardRepository`). Keep endpoints free of business logic.
- Map domain → DTO exclusively in handlers (see `UseCases/Boards/Create/CreateBoardHandler.cs` and `UseCases/Boards/BoardDto.cs`).

Domain model
- Aggregate: `src/FlowBoard.Domain/Aggregates/Board.cs` manages Columns and Cards. Invariants: name rules, order normalization, WIP limits, idempotent archive/unarchive, move/reorder validation. Value objects live under `ValueObjects/*` (e.g., `BoardName`, `OrderIndex`, `WipLimit`, `*Id`).

Persistence & DI
- Contract: `Application/Abstractions/IBoardRepository.cs`. Implement in BOTH:
	- EF Core: `Infrastructure/Persistence/Ef/Repositories/EfBoardRepository.cs`; model in `Infrastructure/Persistence/Ef/FlowBoardDbContext.cs` (ValueConverters for IDs/Order, owned types for names, owns-many for Columns/Cards with tables `Columns`/`Cards`).
	- InMemory: `Infrastructure/Persistence/InMemory/InMemoryBoardRepository.cs`.
- Composition: `WebApi/Program.cs` registers FastEndpoints, Swagger, MediatR (scans Application), and CORS for `http://localhost:5173`. Infra DI (`Infrastructure/Services/ServiceRegistration.cs`) registers `IClock` and chooses EF vs InMemory via config key `Persistence:UseInMemory` (false → Sqlite `FlowBoard` connection string by default).

HTTP error mapping
- Handlers return `Result`; endpoints translate: 400 for validation/conflict (current convention), 404 for not found (see `GetById`). Avoid exposing domain/EF types over HTTP.

Working rules (checklist)
- Commands mutate; Queries read-only—never mix behavior in one handler.
- Add new persistence ops by extending the domain repository interface, then implement in EF + InMemory. Don’t “fix” ordering in repos—aggregates enforce it.

Quick commands
- Build: `dotnet build FlowBoard.sln`  |  Test: `dotnet test FlowBoard.sln`
- Run API: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
- Run Web UI (Vite): `cd src/FlowBoard.WebApp && npm install && npm run dev` (dev at 5173; CORS allowed)
- EF migrations: `dotnet ef migrations add <Name> -p src/FlowBoard.Infrastructure -s src/FlowBoard.WebApi`

Dev server hygiene (important)
- Before starting servers, always check if the frontend (Vite) or backend (ASP.NET WebApi) is already running. If they are, kill them first to avoid port conflicts and orphan processes.
- Default ports:
	- WebApi: https 56157, http 56158 (see `src/FlowBoard.WebApi/Properties/launchSettings.json`)
	- WebApp (Vite): 5173 by default (Vite may auto-switch to 5174+ if busy)
- Windows PowerShell one-liners:
	- Check and kill listeners on 5173/5174/56157/56158
		- Use this pattern in automation before launches
		- Example:
			- `$ports=@(5173,5174,56157,56158); $conns=Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue | Where-Object { $ports -contains $_.LocalPort }; $ids=$conns | Select-Object -ExpandProperty OwningProcess -Unique; if($ids){ foreach($id in $ids){ try{ Stop-Process -Id $id -Force -ErrorAction Stop } catch {} } Start-Sleep -Seconds 1 }`
	- Verify ports are free: `Get-NetTCPConnection -State Listen | Where-Object { $_.LocalPort -in 5173,5174,56157,56158 }`
- Then start:
	- WebApi: `dotnet run --project src/FlowBoard.WebApi/FlowBoard.WebApi.csproj`
	- WebApp: from `src/FlowBoard.WebApp`: `npm run dev`
		- If 5173 is reported busy, fix by killing the listener and starting again rather than letting Vite jump ports.

Adding a slice (TL;DR)
1) Domain behavior (return `Result`) → 2) Extend `IBoardRepository` if needed → 3) Command/Query + Handler returning `Result<Dto>` → 4) EF + InMemory repos + DbContext mapping → 5) Endpoint calling handler and mapping `Result` → HTTP → 6) Tests (domain + handler; EF if mapping changed).
