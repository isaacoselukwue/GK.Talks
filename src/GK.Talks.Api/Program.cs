using GK.Talks.Api.Contracts;
using GK.Talks.Api.Middlewares;
using GK.Talks.Application;
using GK.Talks.Application.Commands;
using GK.Talks.Application.Queries;
using GK.Talks.Core.Interfaces;
using GK.Talks.Infrastructure;
using GK.Talks.Infrastructure.Data;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.Enrich.FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(context.Configuration);

});

string? connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddHealthChecks()
    .AddSqlite(
        connectionString ?? "Data Source=:memory:",
        name: "sqlite",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: ["db", "sqlite"],
        timeout: TimeSpan.FromSeconds(5)
    );

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

ActivitySource activitySource = new("GK.Talks.Application");
builder.Services.AddSingleton(activitySource);
string serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "GK.Talks";
string otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddMeter("Microsoft.AspNetCore.Hosting")
               .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
               .AddMeter("System.Net.Http");
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("GK.Talks.Application")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .SetSampler(new AlwaysOnSampler())
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            });
    });

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference(x =>
{
    x.WithTitle("GK Talks Api");
    x.WithTheme(ScalarTheme.Moon);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}



app.MapPost("/speakers", async (RegisterSpeakerDto req, IMediator mediator, HttpContext ctx, IBrowserDetector detector) =>
{
    string? ua = ctx.Request.Headers.UserAgent.FirstOrDefault();
    var browser = detector.Parse(ua);
    RegisterSpeakerCommand command = new(
        req.FirstName,
        req.LastName,
        req.Email,
        req.Experience,
        req.HasBlog,
        req.BlogUrl,
        req.Certifications ?? [],
        req.Employer,
        req.Sessions?.Select(s => new GK.Talks.Application.Commands.SessionDto(s.Title, s.Description)).ToList() ?? [],
        browser.Name.ToString(),
        browser.MajorVersion
    );

    var result = await mediator.Send(command);
    if (result.IsSuccess) return Results.Created($"/speakers/{result.Value}", result);
    else return Results.BadRequest(result);
});

app.MapGet("/speakers/{id:int}", async (int id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetSpeakerByIdQuery(id));

    if (!result.IsSuccess || result.Value is null)
        return Results.NotFound();
    return Results.Ok(result.Value.ToDto());
});

app.MapGet("/speakers", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetAllSpeakersQuery());
    if(result.Value is null) return Results.NoContent();

    return Results.Ok(result.Value.ToDtos());
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
