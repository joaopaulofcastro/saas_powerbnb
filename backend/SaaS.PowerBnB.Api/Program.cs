using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SaaS.PowerBnB.Api.Infrastructure.Auth;
using SaaS.PowerBnB.Modules.Charging;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Behaviors;
using SaaS.PowerBnB.SharedKernel.Endpoints;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Interceptadores
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddScoped<AuditHistoryInterceptor>();
builder.Services.AddScoped<InsertOutboxMessagesInterceptor>();
builder.Services.AddScoped<AuditableEntityInterceptor>();

//Auth
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 1. OpenTelemetry
var serviceName = builder.Configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "SaaS.PowerBnB.Api";
var otlpEndpoint = builder.Configuration.GetValue<string>("OpenTelemetry:Endpoint") ?? "http://127.0.0.1:4317";

Action<ResourceBuilder> configureResource = r => r.AddService(serviceName);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsql()
        .AddRedisInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        })); // Exporta para Jaeger/Grafana

builder.Logging.ClearProviders();
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

// 2. Auth com Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.Audience = builder.Configuration["Authentication:Keycloak:Audience"];
        options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Authentication:Keycloak:RequireHttpsMetadata"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Keycloak:Authority"],

            ValidateAudience = true,
            // O Keycloak costuma usar 'account' como audience padrão | Azure api://CLIENT_ID (necessário "expor" a API)
            ValidAudience = builder.Configuration["Authentication:Keycloak:Audience"],

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// 3. Redis Cache (Abstração)
builder.Services.AddStackExchangeRedisCache(opt => {
    opt.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 4. MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

    // Behaviors Globais (Validam e Cacheiam tudo)
    cfg.AddOpenBehavior(typeof(QueryCachingBehavior<,>)); // 1. Tenta o Cache
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));   // 2. Valida (se for Command ou Cache MISS)
});

// 5. Configuração dos módulos
builder.Services.AddChargingModule(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
//app.UseHttpsRedirection();

//Utilização dos módulos
app.UseChargingModule();

//Método de extensão que mapeia os endpoints registrados nos módulos
app.MapEndpoints();

app.Run();
