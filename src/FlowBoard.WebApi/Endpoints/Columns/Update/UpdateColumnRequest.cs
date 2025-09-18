using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Columns.Commands;

namespace FlowBoard.WebApi.Endpoints.Columns.Update;

public sealed class UpdateColumnRequest { public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }