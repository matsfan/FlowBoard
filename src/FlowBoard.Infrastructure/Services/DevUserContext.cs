using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Infrastructure.Services;

/// <summary>
/// Development stub implementation of IUserContext that returns a fixed user ID.
/// In production, this would integrate with the authentication system.
/// </summary>
public sealed class DevUserContext : IUserContext
{
    // Fixed dev user ID for testing - in production this would come from auth context
    public UserId CurrentUserId => new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
}