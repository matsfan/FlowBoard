using FlowBoard.Application.UseCases.Boards.Handlers;
using FlowBoard.Application.UseCases.Cards.Handlers;
using FlowBoard.Application.UseCases.Columns.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.Application.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Board handlers
        services.AddScoped<CreateBoardHandler>();
        services.AddScoped<ListBoardsHandler>();

        // Column handlers
        services.AddScoped<AddColumnHandler>();
        services.AddScoped<RenameColumnHandler>();
        services.AddScoped<ReorderColumnHandler>();
        services.AddScoped<SetColumnWipLimitHandler>();

        // Card handlers
        services.AddScoped<AddCardHandler>();
        services.AddScoped<MoveCardHandler>();
        services.AddScoped<ReorderCardHandler>();
        services.AddScoped<ArchiveCardHandler>();
        services.AddScoped<RenameCardHandler>();
        services.AddScoped<ChangeCardDescriptionHandler>();
        services.AddScoped<DeleteCardHandler>();
        return services;
    }
}
