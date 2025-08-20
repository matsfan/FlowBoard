using FlowBoard.Domain.Abstractions;

namespace FlowBoard.Domain;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
