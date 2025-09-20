using Microsoft.Extensions.DependencyInjection;
using FlowBoard.Application.Abstractions;

namespace FlowBoard.Application.Services;

/// <summary>
/// Simple mediator implementation that uses DI to resolve handlers
/// </summary>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        var handler = serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));

        if (handleMethod == null)
            throw new InvalidOperationException($"Handler for {requestType.Name} does not have Handle method");

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
        
        if (result is Task<TResponse> task)
            return await task;
        
        if (result is TResponse directResult)
            return directResult;
            
        throw new InvalidOperationException($"Handler for {requestType.Name} returned unexpected result type");
    }
}