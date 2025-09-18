using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Boards.Commands;

namespace FlowBoard.WebApi.Endpoints.Boards.Update;

public sealed class UpdateBoardRequest { public string Name { get; set; } = string.Empty; }

