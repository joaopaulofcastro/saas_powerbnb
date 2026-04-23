using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName")!;
var otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:Endpoint")!;
Action<ResourceBuilder> configureResource = r => r.AddService(serviceName);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("Microsoft.AspNetCore.Hosting")             // Requisições de entrada, RPS, Erros 500
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")     // Métricas do servidor web
            .AddMeter("System.Net.Http")                        // Substitui o HttpClient (Requisições de saída)
            .AddRuntimeInstrumentation()                       // CPU e RAM
            .AddMeter("SaaS.PowerBnB.Metrics.Outbox") // O nosso Meter customizado!
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
            }); // Envia para o OTel Collector
    });

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    var resourceBuilder = ResourceBuilder.CreateDefault();
    configureResource(resourceBuilder);
    options.SetResourceBuilder(resourceBuilder);

    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;

    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(otlpEndpoint);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        var validIssuers = builder.Configuration.GetSection("Authentication:Keycloak:ValidIssuers").Get<string[]>();

        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.Audience = builder.Configuration["Authentication:Keycloak:Audience"];
        options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Authentication:Keycloak:RequireHttpsMetadata"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateAudience = true,
            // O Keycloak costuma usar 'account' como audience padrão | Azure api://CLIENT_ID (necessário "expor" a API)
            ValidAudience = builder.Configuration["Authentication:Keycloak:Audience"],

            ValidateIssuer = true,
            ValidIssuers = validIssuers,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("bffPolicy", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/bff/mobile/me", (System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var name = user.FindFirst("preferred_username")?.Value;

    return Results.Ok(new { Id = userId, Email = email, Name = name });
}).RequireAuthorization();

app.Use(async (context, next) =>
{
    Console.WriteLine($"[BFF LOG] Request: {context.Request.Method} {context.Request.Path}");
    await next();
});
app.MapReverseProxy();
app.Run();
