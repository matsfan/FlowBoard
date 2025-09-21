using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Entities;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Aggregates;

public sealed class Board
{
    private readonly List<Column> _columns = [];
    private readonly List<BoardMember> _members = [];

    // EF Core parameterless constructor
    private Board() { }

    private Board(BoardId id, BoardName name, DateTimeOffset createdUtc)
    {
        Id = id;
        Name = name;
        CreatedUtc = createdUtc;
    }

    public BoardId Id { get; }
    public BoardName Name { get; private set; } = null!;
    public DateTimeOffset CreatedUtc { get; }
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    #region Board lifecycle
    public static Result<Board> Create(string name, UserId creatorId, IClock clock)
    {
        var nameResult = BoardName.Create(name);
        if (nameResult.IsFailure)
            return Result<Board>.Failure(nameResult.Errors);

        var now = clock.UtcNow;
        var board = new Board(BoardId.New(), nameResult.Value!, now);
        
        // Add the creator as the initial owner
        var initialMember = new BoardMember(creatorId, BoardRole.Owner, now);
        board._members.Add(initialMember);
        
        return board;
    }

    public Result Rename(string newName, UserId actorUserId)
    {
        var ownerCheck = EnsureOwner(actorUserId);
        if (ownerCheck.IsFailure)
            return ownerCheck;
            
        var nameResult = BoardName.Create(newName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Errors);
        if (Name.Value == nameResult.Value!.Value)
            return Result.Success();
        Name = nameResult.Value!;
        return Result.Success();
    }
    #endregion

    #region Membership
    /// <summary>
    /// Adds a new member to the board with the specified role.
    /// </summary>
    public Result AddMember(UserId userId, BoardRole role, IClock clock)
    {
        if (_members.Any(m => m.Id == userId))
            return Error.Conflict("Board.Member.AlreadyExists", "User is already a member of this board");

        var member = new BoardMember(userId, role, clock.UtcNow);
        _members.Add(member);
        return Result.Success();
    }

    /// <summary>
    /// Removes a member from the board, but prevents removing the last owner.
    /// </summary>
    public Result RemoveMember(UserId userId)
    {
        var member = _members.FirstOrDefault(m => m.Id == userId);
        if (member is null)
            return Error.NotFound("Board.Member.NotFound", "User is not a member of this board");

        // Check if this would remove the last owner
        if (member.Role == BoardRole.Owner)
        {
            var ownerCount = _members.Count(m => m.Role == BoardRole.Owner);
            if (ownerCount <= 1)
                return Error.Validation("Board.Member.LastOwner", "Cannot remove the last owner from the board");
        }

        _members.Remove(member);
        return Result.Success();
    }

    /// <summary>
    /// Changes a member's role. Prevents removing the last owner.
    /// </summary>
    public Result ChangeRole(UserId userId, BoardRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.Id == userId);
        if (member is null)
            return Error.NotFound("Board.Member.NotFound", "User is not a member of this board");

        // If changing from Owner to Member, ensure we're not removing the last owner
        if (member.Role == BoardRole.Owner && newRole == BoardRole.Member)
        {
            var ownerCount = _members.Count(m => m.Role == BoardRole.Owner);
            if (ownerCount <= 1)
                return Error.Validation("Board.Member.LastOwner", "Cannot remove the last owner from the board");
        }

        member.ChangeRole(newRole);
        return Result.Success();
    }

    /// <summary>
    /// Checks if a user is an owner of this board.
    /// </summary>
    public bool IsOwner(UserId userId) => _members.Any(m => m.Id == userId && m.Role == BoardRole.Owner);

    /// <summary>
    /// Checks if a user is a member (any role) of this board.
    /// </summary>
    public bool HasMember(UserId userId) => _members.Any(m => m.Id == userId);

    /// <summary>
    /// Ensures the actor is an owner of this board for governance operations.
    /// </summary>
    private Result EnsureOwner(UserId actorUserId)
    {
        if (!IsOwner(actorUserId))
            return Error.Forbidden("Board.Permission.OwnerRequired", "Only board owners can perform this action");
        return Result.Success();
    }

    /// <summary>
    /// Ensures the actor is a collaborator (member or owner) for content operations.
    /// </summary>
    private Result EnsureCollaborator(UserId actorUserId)
    {
        if (!HasMember(actorUserId))
            return Error.Forbidden("Board.Permission.MemberRequired", "Only board members can perform this action");
        return Result.Success();
    }
    #endregion

    #region Columns
    public Result<Column> AddColumn(string name, UserId actorUserId, int? wipLimit = null)
    {
        var collaboratorCheck = EnsureCollaborator(actorUserId);
        if (collaboratorCheck.IsFailure)
            return Result<Column>.Failure(collaboratorCheck.Errors);
            
        var nameResult = ColumnName.Create(name);
        if (nameResult.IsFailure)
            return Result<Column>.Failure(nameResult.Errors);

        if (_columns.Any(c => string.Equals(c.Name.Value, nameResult.Value!.Value, StringComparison.OrdinalIgnoreCase)))
            return Error.Conflict("Column.Name.Duplicate", "A column with that name already exists on this board");

        WipLimit? limit = null;
        if (wipLimit.HasValue)
        {
            var limitResult = ValueObjects.WipLimit.Create(wipLimit.Value);
            if (limitResult.IsFailure)
                return Result<Column>.Failure(limitResult.Errors);
            limit = limitResult.Value;
        }

        var order = OrderIndex.Create(_columns.Count).Value!; // safe: non-negative sequential
        var column = new Column(ColumnId.New(), nameResult.Value!, order, limit);
        _columns.Add(column);
        return column;
    }

    public Result RenameColumn(ColumnId id, string newName, UserId actorUserId)
    {
        var collaboratorCheck = EnsureCollaborator(actorUserId);
        if (collaboratorCheck.IsFailure)
            return collaboratorCheck;
            
        var column = _columns.FirstOrDefault(c => c.Id == id);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");

        var nameResult = ColumnName.Create(newName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Errors);

        if (_columns.Any(c => c.Id != id && string.Equals(c.Name.Value, nameResult.Value!.Value, StringComparison.OrdinalIgnoreCase)))
            return Error.Conflict("Column.Name.Duplicate", "A column with that name already exists on this board");

        column.Rename(nameResult.Value!);
        return Result.Success();
    }

    public Result ReorderColumn(ColumnId id, int newOrder)
    {
        if (newOrder < 0 || newOrder >= _columns.Count)
            return Error.Validation("Column.Order.Invalid", "New order is out of range");

        var column = _columns.FirstOrDefault(c => c.Id == id);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");

        var currentIndex = _columns.IndexOf(column);
        if (currentIndex == newOrder)
            return Result.Success();

        _columns.RemoveAt(currentIndex);
        _columns.Insert(newOrder, column);
        // normalize order indexes
        for (var i = 0; i < _columns.Count; i++)
        {
            _columns[i].SetOrder(new OrderIndex(i));
        }
        return Result.Success();
    }

    public Result<Card> AddCard(ColumnId columnId, string title, string? description, UserId actorUserId, IClock clock)
    {
        var collaboratorCheck = EnsureCollaborator(actorUserId);
        if (collaboratorCheck.IsFailure)
            return Result<Card>.Failure(collaboratorCheck.Errors);
            
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        return column.AddCard(title, description, clock.UtcNow);
    }

    public Result MoveCard(CardId cardId, ColumnId fromColumnId, ColumnId toColumnId, int targetOrder)
    {
        var from = _columns.FirstOrDefault(c => c.Id == fromColumnId);
        if (from is null)
            return Error.NotFound("Column.NotFound", "Source column not found");
        var to = _columns.FirstOrDefault(c => c.Id == toColumnId);
        if (to is null)
            return Error.NotFound("Column.NotFound", "Target column not found");

        var card = from.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in source column");

        if (targetOrder < 0 || targetOrder > to.Cards.Count)
            return Error.Validation("Card.Move.InvalidOrder", "Target order is out of range");

        if (from.Id != to.Id && to.WipLimit.HasValue && to.Cards.Count >= to.WipLimit.Value.Value)
            return Error.Conflict("Column.WipLimit.Violation", "WIP limit reached for target column");

        // Remove from source and insert into target
        from.RemoveCard(card);
        // Adjust targetOrder if inserting at end
        if (targetOrder > to.Cards.Count)
            targetOrder = to.Cards.Count;
        to.InsertCardAt(card, targetOrder);
        return Result.Success();
    }

    public Result ReorderCardWithinColumn(ColumnId columnId, CardId cardId, int newOrder)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        return column.ReorderCard(cardId, newOrder);
    }

    public Result ArchiveCard(ColumnId columnId, CardId cardId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in column");
        if (card.IsArchived)
            return Result.Success(); // idempotent
        card.Archive();
        return Result.Success();
    }

    public Result UnarchiveCard(ColumnId columnId, CardId cardId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in column");
        if (!card.IsArchived)
            return Result.Success(); // idempotent
        card.Restore();
        return Result.Success();
    }

    public Result RenameCard(ColumnId columnId, CardId cardId, string newTitle)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in column");
        var titleResult = CardTitle.Create(newTitle);
        if (titleResult.IsFailure)
            return Result.Failure(titleResult.Errors);
        if (card.Title.Value == titleResult.Value!.Value)
            return Result.Success();
        card.Rename(titleResult.Value!);
        return Result.Success();
    }

    public Result ChangeCardDescription(ColumnId columnId, CardId cardId, string? newDescription)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in column");
        var descResult = CardDescription.Create(newDescription);
        if (descResult.IsFailure)
            return Result.Failure(descResult.Errors);
        if (card.Description.Value == descResult.Value!.Value)
            return Result.Success();
        card.ChangeDescription(descResult.Value!);
        return Result.Success();
    }

    public Result SetColumnWipLimit(ColumnId columnId, int? newWipLimit)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        return column.SetWipLimit(newWipLimit);
    }

    public Result DeleteCard(ColumnId columnId, CardId cardId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        var card = column.FindCard(cardId);
        if (card is null)
            return Error.NotFound("Card.NotFound", "Card not found in column");
        column.RemoveCard(card);
        return Result.Success();
    }
    #endregion
}

