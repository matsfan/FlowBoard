using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.DependencyInjection;
using FlowBoard.Application.Services;
using FlowBoard.Infrastructure.Services;
using MediatR;

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
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
// MediatR - register handlers from the Application assembly
builder.Services.AddMediatR(typeof(FlowBoard.Application.Services.ServiceRegistration).Assembly);
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

// Seed demo data (boards, columns, cards) only in Development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.EnsureSeededAsync(scope.ServiceProvider);
    }
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// Apply CORS (before endpoints)
app.UseCors(DevCorsPolicy);

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

public sealed class WebAssemblyMarker { }
