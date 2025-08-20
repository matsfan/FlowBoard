namespace FlowBoard.Domain.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
