using FastEndpoints;
using MediatR;
using FlowBoard.Application.UseCases.Columns.Queries;

namespace FlowBoard.WebApi.Endpoints.Columns.GetById;

public sealed class GetColumnByIdResponse { public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public int Order { get; set; } public int? WipLimit { get; set; } }
