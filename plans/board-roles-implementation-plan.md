# Board Roles Implementation Plan

Purpose: Introduce first-class board membership and roles (Owner, Member) across Domain → Application → Infrastructure → WebApi, preserving Clean Architecture, DDD, and CQRS conventions used in FlowBoard.

## Goals & Non-Goals

- Goals
  - Add membership to Board aggregate with two roles: Owner and Member.
  - Enforce invariants in domain: ≥1 Owner at all times; no duplicate members.
  - Guard board-level operations (rename, archive/unarchive, delete, membership changes) to Owners.
  - Allow Members to perform column/card operations.
  - Provide a minimal way to identify the current user in handlers.
  - Return Result/Result\<T\> for expected failures; map Forbidden → 403.
- Non-Goals (deferred)
  - External authentication/identity provider integration.
  - UI for managing members (basic API only in this phase).
  - Additional roles (Viewer/Guest) or board-level sharing links.

## Architecture Fit

- Domain remains persistence-agnostic; value objects for UserId/BoardRole, BoardMember entity/value, membership held inside Board aggregate.
- Application layer receives current user via IUserContext abstraction; checks are performed by aggregate methods, not endpoints.
- Repositories unchanged in contract for existing features; add membership-aware methods only if required.
- Both EF and InMemory repositories must persist membership consistently.

## Definitions

- BoardRole (enum): Owner = 1, Member = 2 (reserve 0 for Unknown if needed).
- UserId (value object): wraps Guid; consistent with existing *Id types.
- BoardMember (entity/value inside Board): UserId Id, BoardRole Role, DateTime JoinedAt (via IClock).

## Domain Changes

- New value objects
  - `ValueObjects/UserId.cs`
  - `ValueObjects/BoardRole.cs` (enum)
  - `Entities/BoardMember.cs` (or nested type under Board)
- Board aggregate
  - Add private collection: `List<BoardMember> _members` and read-only `IReadOnlyCollection<BoardMember> Members`.
  - Invariants
    - Must always have ≥1 Owner.
    - A `UserId` can appear at most once.
  - Behaviors (return Result/Result\<T\>)
    - `AddMember(UserId userId, BoardRole role)`
    - `RemoveMember(UserId userId)` (blocked if removing last Owner)
    - `ChangeRole(UserId userId, BoardRole role)` (idempotent)
    - `IsOwner(UserId userId)` / `HasMember(UserId userId)` helpers
  - Guard methods (used internally by other Board behaviors)
    - `EnsureOwner(UserId actor)` for board-level operations
    - `EnsureCollaborator(UserId actor)` for column/card operations
  - Apply guards to existing mutations: rename, archive/unarchive, delete; column/card mutations check collaborator.

## Application Layer

- Abstractions
  - `IUserContext` with `UserId CurrentUserId { get; }`
- Command/Query handlers
  - Inject `IUserContext` where mutating existing board state.
  - Load board via `IBoardRepository`.
  - Delegate to aggregate methods which enforce permissions.
  - Map Result → HTTP, extend endpoints to return 403 on `Forbidden`.
- DTO changes (optional in Phase 1)
  - Add `CurrentUserRole` to `BoardDto` or expose a lightweight membership summary.

## Infrastructure

- EF Core
  - Add mapping for `BoardMember` as owned collection on `Board`.
  - Table: `BoardMembers` with columns `(BoardId, UserId, Role, JoinedAt)`.
  - ValueConverters for `UserId` similar to other *Id types; enum mapping for `BoardRole`.
  - Migration: create new table and foreign key.
- InMemory
  - Persist members within stored Board; no extra contract needed.

## WebApi Layer

- Introduce 403 mapping in endpoints for `Result.Forbidden`.
- Membership endpoints (Phase 2)
  - Keep RESTful resource shape. Suggested minimal surface for now:
    - `PUT /boards/{boardId}/members/{userId}` with body `{ role: "Owner|Member" }` to add or change.
    - `DELETE /boards/{boardId}/members/{userId}` to remove.
  - Existing board settings endpoints implicitly require Owner via domain guards.
- For dev, a simple stub `IUserContext` that reads a fixed user id from header `X-Demo-UserId` (or appsettings) is acceptable.

## Testing Strategy

- Domain tests
  - AddMember happy path and duplicate prevention.
  - RemoveMember blocked when last Owner.
  - ChangeRole idempotency and Owner→Member edge cases (respect ≥1 owner).
  - Guards: EnsureOwner forbids non-owners.
- Application tests
  - Handlers enforce owner for rename/archive/delete.
  - 403 mapping verification.
- Infrastructure tests
  - EF mapping round-trip for members; InMemory parity.
- WebApi tests (optional Phase 2)
  - Membership endpoints CRUD and guard responses using stub `IUserContext`.

## Migration & Backfill

- Boards created before roles: on load, if membership is empty, treat the creator (unknown historically) as Owner or assign a system Owner. For dev simplicity, Phase 1 can allow empty membership and skip guards until explicitly managed; Phase 2 introduces a backfill strategy.

## Phases & Milestones

- Phase 1: Domain + App plumbing
  - Domain types and invariants
  - IUserContext abstraction + dev stub implementation
  - Apply guards to a subset of operations (rename, archive)
  - 403 mapping
  - Unit tests green
- Phase 2: Persistence + Endpoints
  - EF/InMemory mapping and migrations
  - Minimal membership endpoints (PUT/DELETE on members)
  - Tests for repositories + WebApi
- Phase 3: UX and Enhancements (optional)
  - Show current user role on board
  - Invite flow, transfer ownership, Viewer role (if needed)

## Risks & Mitigations

- Risk: Role checks accidentally in endpoints or repos → keep all checks in aggregate.
- Risk: Last-owner removal edge case → explicit invariant and tests.
- Risk: Dev stub user context leaking to prod → wire DI so prod must implement `IUserContext`.

## Acceptance Criteria

- Domain: Board supports membership with Owner/Member; invariants enforced.
- Application: Mutations enforce Owner/Member via domain guards; 403 mapping added.
- Infrastructure: EF + InMemory persist and retrieve members.
- WebApi: Membership endpoints (if Phase 2 implemented) behave as specified.
- Tests: New tests passing across Domain, Application, and Infrastructure (and WebApi if Phase 2).
