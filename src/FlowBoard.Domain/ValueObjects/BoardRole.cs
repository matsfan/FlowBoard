namespace FlowBoard.Domain.ValueObjects;

/// <summary>
/// Roles that a user can have on a board.
/// </summary>
public enum BoardRole
{
    /// <summary>
    /// Member can work on columns and cards but cannot change board governance.
    /// </summary>
    Member = 1,
    
    /// <summary>
    /// Owner has full control of board settings and membership.
    /// </summary>
    Owner = 2
}