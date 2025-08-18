using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.DependencyInjection;
using FlowBoard.Application;
using FlowBoard.Infrastructure.Boards;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// FastEndpoints & Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
	o.DocumentSettings = s =>
	{
		s.Title = "FlowBoard API";
	};
});

// Layer registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

public sealed class WebAssemblyMarker { }
