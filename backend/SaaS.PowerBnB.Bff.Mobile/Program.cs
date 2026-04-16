using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policy => policy.RequireAuthenticatedUser());
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

app.MapReverseProxy();
app.Run();
