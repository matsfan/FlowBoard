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

## Using MediatR in FlowBoard

This repo uses MediatR for application-level commands and queries. Handlers live in `src/FlowBoard.Application/UseCases/**/Handlers` and implement `IRequestHandler<TRequest, TResponse>`.

- Commands: mutate state and return `Result` or `Result<TDto>` (see `CreateBoardCommand`, `AddCardCommand`).
- Queries: read-only and return `Result<TDto>` collections or single DTOs (see `ListBoardsQuery`).

Registration:

- The Web API composition root registers MediatR once, scanning the Application assembly: see `src/FlowBoard.WebApi/Program.cs`.
- Do not register handlers explicitly in DI; keep `ServiceRegistration` minimal.

Endpoint pattern:

- Inject `IMediator` into FastEndpoints endpoints and call `await mediator.Send(new YourCommand(...), ct)`.
- Translate `Result` to HTTP in the endpoint (400 for domain/validation errors, 201/200 for success).

Example (rename a card):

```csharp
public sealed class RenameCardEndpoint(IMediator mediator) : Endpoint<RenameCardRequest>
{
	public override void Configure()
	{
		Post("/boards/{boardId:guid}/columns/{columnId:guid}/cards/{cardId:guid}/rename");
		Group<CardsGroup>();
	}

	public override async Task HandleAsync(RenameCardRequest req, CancellationToken ct)
	{
		// map route → command
		req.BoardId = Route<Guid>("boardId");
		req.ColumnId = Route<Guid>("columnId");
		req.CardId = Route<Guid>("cardId");

		var result = await mediator.Send(new RenameCardCommand(req.BoardId, req.ColumnId, req.CardId, req.Title), ct);
		if (result.IsFailure)
		{
			AddError(string.Join("; ", result.Errors.Select(e => e.Code + ":" + e.Message)));
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}
		await Send.OkAsync(cancellation: ct);
	}
}
```

Add a new use case by:

1. Creating a command or query record in `UseCases/<Area>/Commands|Queries` implementing `IRequest<Result|Result<T>>`.
2. Implementing a handler in `UseCases/<Area>/Handlers` implementing `IRequestHandler<TRequest, Result|Result<T>>` and orchestrating the Domain.
3. Calling it from a FastEndpoint via `IMediator`.

## API (Board / Column / Card)

Base URL (dev): `http://localhost:5000` (or whatever Kestrel assigns). All routes shown relative.

Boards:

- `POST /boards` – create board `{ name }` → 201 with board dto
- `GET /boards` – list boards → 200 `[ { id, name, createdUtc } ]`

Columns:

- `POST /boards/{boardId}/columns` – body: `{ name, wipLimit? }` → 201 `{ id, boardId, name, order, wipLimit }`
- `POST /boards/{boardId}/columns/{columnId}/rename` – `{ name }` → 200
- `POST /boards/{boardId}/columns/{columnId}/reorder` – `{ newOrder }` → 200
- `POST /boards/{boardId}/columns/{columnId}/wip` – `{ wipLimit? }` (null clears) → 200

Cards:

- `POST /boards/{boardId}/columns/{columnId}/cards` – `{ title, description? }` → 201 `{ id, ... }`
- `POST /boards/{boardId}/cards/{cardId}/move` – `{ fromColumnId, toColumnId, targetOrder }` → 200
- `POST /boards/{boardId}/columns/{columnId}/cards/{cardId}/reorder` – `{ newOrder }` → 200
- `POST /boards/{boardId}/columns/{columnId}/cards/{cardId}/archive` – 200 (idempotent)
- `POST /boards/{boardId}/columns/{columnId}/cards/{cardId}/rename` – `{ title }` → 200
- `POST /boards/{boardId}/columns/{columnId}/cards/{cardId}/description` – `{ description? }` → 200
- `DELETE /boards/{boardId}/columns/{columnId}/cards/{cardId}` – 200

Common failure response shape (FastEndpoints default) is a 400 with an `Errors` array; domain error codes include:

- `Board.NotFound`, `Column.NotFound`, `Card.NotFound`
- Validation: `Card.Title.Empty`, `Card.Title.TooLong`, `Column.WipLimit.Invalid`, `Column.WipLimit.Violation`, `Card.Move.InvalidOrder`
- Conflict: `Column.WipLimit.Violation` when exceeding limit on move/add

Restore (un-archive) card is intentionally NOT implemented yet; planned future slice.

---

Keep README concise; expand only when developer onboarding friction appears.
