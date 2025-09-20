namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Interface for sending requests to their respective handlers
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}