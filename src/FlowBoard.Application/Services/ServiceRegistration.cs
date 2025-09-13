using FlowBoard.Application.UseCases.Boards.Handlers;
using FlowBoard.Application.UseCases.Cards.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CreateBoardHandler>();
        services.AddScoped<ListBoardsHandler>();
        services.AddScoped<AddCardHandler>();
        services.AddScoped<MoveCardHandler>();
        services.AddScoped<ReorderCardHandler>();
        return services;
    }
}
