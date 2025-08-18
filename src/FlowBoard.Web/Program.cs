using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.DependencyInjection;

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

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
