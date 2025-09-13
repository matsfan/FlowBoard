# Ubiquitous Language (Kanban Domain)

Living glossary for the core FlowBoard domain. Keep terms consistent across code, tests, docs, and conversations. Update when a new concept or invariant is introduced.

Status legend: (Existing) already implemented – (Planned) not yet implemented.

---
## Aggregates & Entities

**Board** (Existing)
- Root aggregate. Container and consistency boundary for Columns (and initially Cards).
- Attributes: Id (BoardId), Name (BoardName), CreatedUtc.
- Invariants: Name required, <=100 chars, unique per tenant (future multi-tenant concern).
- Behaviors (current): Create, Rename.
- Planned behaviors: AddColumn, ReorderColumn, RemoveColumn (policy TBD), AddCard (may delegate through Column), MoveCard (enforces WIP), ArchiveCard.

**Column** (Planned)
- Entity (owned by Board). Ordered lane that groups Cards and optionally constrains WIP (work-in-progress).
- Attributes: Id (ColumnId), Name (ColumnName), Order (OrderIndex), WipLimit (WipLimit?), Cards (collection of Card entities or CardIds – design decision pending), CreatedUtc.
- Invariants: Name unique within Board (case-insensitive); OrderIndex values must be contiguous starting at 0; WipLimit > 0 if specified; CardCount <= WipLimit when set.
- Behaviors: Rename, SetWipLimit, Reorder (adjust neighbors), AddCard (validate WIP), RemoveCard, AcceptCardMoveIn (validate WIP), Remove (only if empty OR cascade policy).

**Card** (Planned)
- Work item (could later become its own aggregate if cross-board moves or external references become common, else entity within Board/Column early on).
- Attributes: Id (CardId), Title (CardTitle), Description (CardDescription optional), Order (OrderIndex within Column), CreatedUtc, Possibly Archived flag.
- Invariants: Title required, <= 200 chars (proposed); Description <= 5000 chars (proposed); OrderIndex contiguous within Column.
- Behaviors: Rename (change title), ChangeDescription, Move (between Columns – triggers WIP checks), Reorder, Archive / Restore.

---
## Value Objects

**BoardId** (Existing)
- Strongly typed identifier wrapping Guid.

**BoardName** (Existing)
- Normalized (trimmed) board name with length + non-empty validation.

**ColumnId** (Planned)
- Strongly typed identifier for a Column.

**ColumnName** (Planned)
- Trim + length (<= 60?) + non-empty validation. Unique per Board.

**CardId** (Planned)
- Strongly typed identifier for a Card.

**CardTitle** (Planned)
- Trim + length (<= 200) + non-empty validation.

**CardDescription** (Planned)
- Optional; length <= 5000; future sanitization.

**OrderIndex** (Planned)
- Non-negative integer used for stable ordering within a parent scope (Columns within Board, Cards within Column).
- Invariants: Dense sequence (0..n-1). After inserts/moves reorder must normalize.

**WipLimit** (Planned)
- Positive integer (>0). Null/absent indicates unlimited.

---
## Cross-Cutting Domain Concepts

**Work-In-Progress (WIP) Constraint** (Planned)
- Enforced at Column level. Add/move card operations must validate proposed card count <= WipLimit.

**Ordering** (Planned)
- Reordering operations adjust affected entities. Domain responsible for consistent order maintenance (no gaps / duplicates).

**Archival** (Planned)
- Cards (and possibly Columns) may be archived (soft removal) preserving historical context.

---
## Behaviors / Operations (Canonical Names)
Use these names in method signatures and tests.

Board:
- Create(name)
- Rename(newName)
- AddColumn(name, wipLimit?) (Planned)
- ReorderColumn(columnId, newOrder) (Planned)
- RemoveColumn(columnId) (Planned)
- AddCard(columnId, title[, description]) (Planned)
- MoveCard(cardId, targetColumnId, targetOrder) (Planned)

Column:
- Rename(newName) (Planned)
- SetWipLimit(newLimit) (Planned)
- AddCard(title[, description]) (Planned)
- ReorderCard(cardId, newOrder) (Planned)
- RemoveCard(cardId) (Planned)

Card:
- Rename(newTitle) (Planned)
- ChangeDescription(newDescription) (Planned)
- Archive() / Restore() (Planned)

---
## Invariants (Explicit Rules)
Current:
1. BoardName required; length <= 100.

Planned:
2. ColumnName required; unique per Board (case-insensitive); length <= 60.
3. OrderIndex sequences (Columns, Cards) must remain dense starting at 0 after any operation.
4. WipLimit > 0 when specified.
5. Cannot set WipLimit below current Card count.
6. CardTitle required; length <= 200.
7. CardDescription length <= 5000.
8. Move/Add Card must not violate target Column WipLimit.

---
## Error Codes (Namespace Convention: <Area>.<Entity>.<Condition>)
Existing:
- Board.Name.Empty
- Board.Name.TooLong
- Board.Name.Duplicate (enforced outside Board currently)

Planned:
- Column.Name.Empty
- Column.Name.TooLong
- Column.Name.Duplicate
- Column.NotFound
- Column.WipLimit.Invalid
- Column.WipLimit.Violation (when limit < current or add/move exceeds)
- Card.NotFound
- Card.Title.Empty
- Card.Title.TooLong
- Card.Description.TooLong
- Card.Move.InvalidOrder

---
## Domain Events (Deferred)
Potential future events (only add when an external reaction is required):
- BoardCreated, BoardRenamed
- ColumnAdded, ColumnRenamed, ColumnReordered, WipLimitChanged
- CardAdded, CardMoved, CardReordered, CardArchived

---
## Open Design Questions (Track & Resolve Incrementally)
- Should Card become its own aggregate for cross-board moves? (Current assumption: no, keep within Board for transactional invariants.)
- Soft-delete vs archive for Columns? (Leaning: archive; deletion only when empty.)
- Maximum counts (Columns per Board, Cards per Column) – add limits?
- Event sourcing vs state storage – currently simple state storage via EF.

---
## Next Implementation Step
Introduce Column model:
1. Add ColumnId, ColumnName, WipLimit, OrderIndex value objects.
2. Extend Board with private list of Columns and public read-only view.
3. Implement AddColumn + RenameColumn + basic ReorderColumn.
4. Domain tests covering: add first column, duplicate name rejection, order assignment, rename, reorder.

Update this document after each domain capability is added.
