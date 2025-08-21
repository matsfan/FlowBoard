using FlowBoard.Application.UseCases.Boards.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CreateBoardHandler>();
        services.AddScoped<ListBoardsHandler>();
        return services;
    }
}
