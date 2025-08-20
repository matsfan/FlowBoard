namespace FlowBoard.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
