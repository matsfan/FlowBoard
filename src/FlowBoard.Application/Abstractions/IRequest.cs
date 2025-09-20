namespace FlowBoard.Application.Abstractions;

/// <summary>
/// Marker interface for requests that return a response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequest<TResponse>;