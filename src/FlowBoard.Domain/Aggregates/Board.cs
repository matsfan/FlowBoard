using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Entities;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Aggregates;

public sealed class Board
{
    private readonly List<Column> _columns = [];

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

    #region Board lifecycle
    public static Result<Board> Create(string name, IClock clock)
    {
        var nameResult = BoardName.Create(name);
        if (nameResult.IsFailure)
            return Result<Board>.Failure(nameResult.Errors);

        var now = clock.UtcNow;
        var board = new Board(BoardId.New(), nameResult.Value!, now);
        return board;
    }

    public Result Rename(string newName)
    {
        var nameResult = BoardName.Create(newName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Errors);
        if (Name.Value == nameResult.Value!.Value)
            return Result.Success();
        Name = nameResult.Value!;
        return Result.Success();
    }
    #endregion

    #region Columns
    public Result<Column> AddColumn(string name, int? wipLimit = null)
    {
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

    public Result RenameColumn(ColumnId id, string newName)
    {
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

    public Result<Card> AddCard(ColumnId columnId, string title, string? description, IClock clock)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Error.NotFound("Column.NotFound", "Column not found");
        return column.AddCard(title, description, clock.UtcNow);
    }
    #endregion
}

