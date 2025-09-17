using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application handlers are registered with MediatR from the Web host (composition root).
        // Keep this method for other application-layer registrations if needed.
        return services;
    }
}
