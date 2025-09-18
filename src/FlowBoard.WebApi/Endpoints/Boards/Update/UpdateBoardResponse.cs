using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Boards.Commands;

namespace FlowBoard.WebApi.Endpoints.Boards.Update;

public sealed class UpdateBoardResponse { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public DateTimeOffset CreatedUtc { get; set; } }
