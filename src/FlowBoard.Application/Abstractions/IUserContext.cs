using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Provides access to the current user context.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the ID of the currently authenticated user.
    /// </summary>
    UserId CurrentUserId { get; }
}