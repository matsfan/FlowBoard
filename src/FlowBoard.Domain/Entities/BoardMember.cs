using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Entities;

/// <summary>
/// Represents a user's membership in a board with their role and join date.
/// </summary>
public sealed class BoardMember
{
    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private BoardMember() { }

    internal BoardMember(UserId userId, BoardRole role, DateTimeOffset joinedAt)
    {
        Id = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public UserId Id { get; private set; }
    public BoardRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    internal void ChangeRole(BoardRole newRole)
    {
        Role = newRole;
    }
}