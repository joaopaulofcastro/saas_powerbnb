using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.SharedKernel.Endpoints;

namespace SaaS.PowerBnB.Modules.Identity.Features.RegisterUser;

internal class KeycloakWebhookEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/webhooks/keycloak", HandleAsync)
           .WithTags("Identity")
           .WithName("KeycloakWebhook");
        // Sem .RequireAuthorization() — autenticação exclusiva via X-Keycloak-Webhook-Secret
    }

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        ISender sender,
        IConfiguration configuration,
        ILogger<KeycloakWebhookEndpoint> logger)
    {
        // 1. Valida o secret com comparação de tempo constante (previne timing attacks)
        var configuredSecret = configuration["Identity:WebhookSecret"] ?? string.Empty;
        var receivedSecret = context.Request.Headers["X-Keycloak-Webhook-Secret"].ToString();

        var configBytes = Encoding.UTF8.GetBytes(configuredSecret);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedSecret);

        // Garante que os arrays têm o mesmo tamanho antes da comparação
        if (configBytes.Length != receivedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(configBytes, receivedBytes))
        {
            logger.LogWarning(
                "Tentativa de acesso ao Webhook com secret inválido. IP: {IpAddress}",
                context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido");
            return Results.Unauthorized();
        }

        // 2. Lê e desserializa o payload
        KeycloakWebhookPayload? payload;
        try
        {
            payload = await context.Request.ReadFromJsonAsync<KeycloakWebhookPayload>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Payload JSON inválido recebido no Webhook do Keycloak.");
            return Results.BadRequest("Payload JSON inválido.");
        }

        if (payload is null)
            return Results.BadRequest("Payload não pode ser nulo.");

        // 3. Filtra apenas eventos REGISTER — retorna 200 para outros tipos (idempotência)
        if (payload.Type != "REGISTER")
            return Results.Ok();

        // 4. Valida que o UserId é um UUID válido (sub do Keycloak)
        if (!Guid.TryParse(payload.UserId, out var userId))
        {
            logger.LogWarning(
                "Webhook recebido com userId inválido: '{UserId}'",
                payload.UserId);
            return Results.BadRequest($"O campo 'userId' não é um UUID válido: '{payload.UserId}'.");
        }

        // 5. Extrai email e nome do payload
        var email = payload.Details?.Email ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest("O campo 'details.email' é obrigatório.");

        var firstName = payload.Details?.FirstName ?? string.Empty;
        var lastName = payload.Details?.LastName ?? string.Empty;
        var fullName = $"{firstName} {lastName}".Trim();

        // 6. Envia o comando via MediatR
        var command = new RegisterUserCommand(userId, email, fullName);
        var result = await sender.Send(command);

        return result.Match(
            id => Results.Ok(new { UserId = id }),
            validationFailed => Results.BadRequest(validationFailed)
        );
    }
}

// DTOs para desserialização do payload do Keycloak
internal record KeycloakWebhookPayload(
    string Type,
    string RealmId,
    string ClientId,
    string UserId,
    string IpAddress,
    KeycloakWebhookDetails? Details
);

internal record KeycloakWebhookDetails(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Username
);
