using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.DependencyInjection;
using FlowBoard.Application;
using FlowBoard.Infrastructure;

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
// CORS for local WebApp (Vite dev server)
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
	options.AddPolicy(DevCorsPolicy, policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Apply CORS (before endpoints)
app.UseCors(DevCorsPolicy);

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

public sealed class WebAssemblyMarker { }
