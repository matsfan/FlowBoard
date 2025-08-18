using FlowBoard.Application.Boards;
using FlowBoard.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateBoardHandler, CreateBoardHandler>();
        return services;
    }
}
