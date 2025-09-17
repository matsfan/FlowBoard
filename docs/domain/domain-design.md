# Domain Design (FlowBoard Kanban Domain)

Purpose
- Define the domain model, invariants, and behaviors for the FlowBoard Kanban system.
- Provide a stable reference for developers, tests, and reviewers. Keep this doc as the single source of truth for domain decisions.

Scope
- Core Kanban domain within the FlowBoard bounded context.
- Aligns with Clean Architecture: Domain (pure), Application (use cases, DTOs), Infrastructure (EF, storage), WebApi (endpoints).

Status Legend
- Existing: implemented
- Planned: not yet implemented

---
Bounded Context: FlowBoard (Core Kanban)
- Primary concepts: Board, Column, Card.
- Consistency boundary: Board is the root aggregate.
- Time source: `IClock` (no direct `DateTime.UtcNow`).
- Error model: `Result` / `Result<T>` with domain error codes (see Error Codes).

---
Aggregates & Entities

Board (Existing)
- Role: Root aggregate; consistency boundary for Columns (and initially Cards via Columns).
- State
  - Id: BoardId (VO)
  - Name: BoardName (VO)
  - CreatedUtc
  - Columns: ordered collection of Column (owned entity) [Planned]
- Invariants
  - Name is required; length <= 100; unique per tenant (future concern; currently enforced outside aggregate).
- Behaviors (Canonical Names)
  - Create(name)
  - Rename(newName)
  - AddColumn(name, wipLimit?) (Planned)
  - ReorderColumn(columnId, newOrder) (Planned)
  - RemoveColumn(columnId) (policy TBD) (Planned)
  - AddCard(columnId, title[, description]) (Planned)
  - MoveCard(cardId, targetColumnId, targetOrder) (Planned)

Column (Planned)
- Role: Owned entity within Board; ordered lane grouping Cards; optionally constrains WIP.
- State
  - Id: ColumnId (VO)
  - Name: ColumnName (VO)
  - Order: OrderIndex (VO)
  - WipLimit: WipLimit? (VO; null = unlimited)
  - Cards: ordered collection of Card entities or CardIds (design TBD)
  - CreatedUtc
- Invariants
  - Name unique within a Board (case-insensitive).
  - OrderIndex values for Columns are contiguous starting at 0 with no gaps.
  - WipLimit > 0 when specified.
  - CardCount <= WipLimit when set.
- Behaviors (Canonical Names)
  - Rename(newName)
  - SetWipLimit(newLimit)
  - Reorder(newOrder) – adjusts affected neighbors via Board orchestration
  - AddCard(title[, description]) – validates WIP
  - RemoveCard(cardId)
  - AcceptCardMoveIn(cardId, atOrder) – validates WIP
  - Remove – allowed only if empty OR per cascade policy

Card (Planned)
- Role: Work item within a Column (may later become its own aggregate if cross-board moves or external references are required).
- State
  - Id: CardId (VO)
  - Title: CardTitle (VO)
  - Description: CardDescription? (VO)
  - Order: OrderIndex (VO) within Column
  - CreatedUtc
  - Archived: bool (optional, planned)
- Invariants
  - Title required; length <= 200.
  - Description length <= 5000.
  - OrderIndex contiguous within its Column.
- Behaviors (Canonical Names)
  - Rename(newTitle)
  - ChangeDescription(newDescription)
  - Move(targetColumnId, targetOrder)
  - Reorder(newOrder)
  - Archive() / Restore()

---
Value Objects
- BoardId (Existing): strongly typed identifier wrapping Guid.
- BoardName (Existing): normalized/trimmmed, non-empty, length <= 100.
- ColumnId (Planned): strongly typed identifier.
- ColumnName (Planned): trimmed, non-empty, length <= 60, unique per Board.
- CardId (Planned): strongly typed identifier.
- CardTitle (Planned): trimmed, non-empty, length <= 200.
- CardDescription (Planned): optional, length <= 5000.
- OrderIndex (Planned): non-negative integer; used for stable ordering within a parent scope (Columns within Board, Cards within Column). Sequences must be dense (0..n-1) after any mutation.
- WipLimit (Planned): positive integer (>0). Null indicates unlimited.

---
Invariants (Explicit Rules)
Current
1. BoardName required; length <= 100.

Planned
2. ColumnName required; unique per Board (case-insensitive); length <= 60.
3. OrderIndex sequences (Columns, Cards) remain dense starting at 0 after any operation.
4. WipLimit > 0 when specified.
5. Cannot set WipLimit below current Card count.
6. CardTitle required; length <= 200.
7. CardDescription length <= 5000.
8. Add/Move Card must not violate target Column WipLimit.

---
Error Model & Codes (Namespace: <Area>.<Entity>.<Condition>)
Existing
- Board.Name.Empty
- Board.Name.TooLong
- Board.Name.Duplicate (currently enforced outside Board)

Planned
- Column.Name.Empty
- Column.Name.TooLong
- Column.Name.Duplicate
- Column.NotFound
- Column.WipLimit.Invalid
- Column.WipLimit.Violation (limit < current or add/move exceeds)
- Card.NotFound
- Card.Title.Empty
- Card.Title.TooLong
- Card.Description.TooLong
- Card.Move.InvalidOrder

Guidance
- Behaviors return Result/Result<T> with these codes for expected validation failures.
- Avoid throwing for flow control; use exceptions only for truly exceptional cases.

---
Ordering & WIP
- Ordering: Board ensures Columns have a dense OrderIndex; Column ensures Cards have a dense OrderIndex.
- Insertions/moves trigger normalization to remove gaps/duplicates.
- WIP: Column enforces max card count when WipLimit is set. Board-level operations that add/move cards must delegate to Column validations.

---
Repositories & Contracts
- `IBoardRepository` (Domain) provides access to boards for Application use cases.
- Operations (indicative): GetById, Add, Update, ExistsByName (for uniqueness) – actual contract defined in code.
- Implementations live in Infrastructure (EF + InMemory). Domain and Application must not reference EF types.

---
Domain Events (Deferred)
- Only introduce when an external reaction is required.
- Potential: BoardCreated, BoardRenamed, ColumnAdded, ColumnRenamed, ColumnReordered, WipLimitChanged, CardAdded, CardMoved, CardReordered, CardArchived.

---
Consistency & Transactions
- Aggregate boundary: Board ensures local invariants for Columns and Cards.
- Transactions: Operations that modify Board/Columns/Cards should be persisted atomically via the repository.

---
Application Integration
- Use cases live in Application as handlers (async) returning Result<Dto>.
- Handlers orchestrate repository calls and map Domain to DTOs.
- WebApi endpoints remain thin: parse ? handler ? HTTP mapping.

---
Persistence Notes (EF Oriented)
- Simple state storage (no event sourcing at present).
- EF mappings in Infrastructure (`FlowBoardDbContext`) should preserve value objects and owned entities.
- Migrations handled in Infrastructure, started by WebApi project.

---
Non-Functional Notes
- Deterministic time via `IClock`.
- Keep Domain free of external libraries.
- Maintain clear dependency direction: WebApi ? Application ? Domain; Infrastructure used by WebApi for DI.

---
Open Design Questions
- Should Card become its own aggregate for cross-board moves? Current assumption: keep within Board for transactional invariants.
- Soft-delete vs archive for Columns? Leaning: archive; deletion only when empty.
- Maximum counts (Columns per Board, Cards per Column)? Consider introducing limits.
- Event sourcing vs state storage – currently EF state storage.

---
Roadmap / Next Steps
1. Introduce Column model:
   - Add ColumnId, ColumnName, WipLimit, OrderIndex value objects.
   - Extend Board with private list of Columns and public read-only view.
   - Implement AddColumn + RenameColumn + basic ReorderColumn.
   - Tests: add first column, duplicate name rejection, order assignment, rename, reorder.
2. Introduce Card model with basic add/move respecting WIP.
3. Wire repository updates and EF mappings; update Application handlers and endpoints.
4. Add error code coverage and map to HTTP responses.

---
Ubiquitous Language (Glossary)
- Board: Kanban board; aggregate root.
- Column: Lane within a board; orders cards; optional WIP.
- Card: Work item represented within a column.
- OrderIndex: Dense 0-based order within a parent scope.
- WipLimit: Positive integer cap on cards in a column.
