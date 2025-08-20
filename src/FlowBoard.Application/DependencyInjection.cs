using FlowBoard.Application.Boards.Handlers;
using FlowBoard.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
    services.AddScoped<CreateBoardHandler>();
    services.AddScoped<ListBoardsHandler>();
        return services;
    }
}
