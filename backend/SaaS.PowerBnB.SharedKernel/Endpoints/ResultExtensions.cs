using Microsoft.AspNetCore.Http;
using SaaS.PowerBnB.SharedKernel.Application.Errors;


namespace SaaS.PowerBnB.SharedKernel.Endpoints;


public static class ResultExtensions
{
    /// <summary>
    /// Converte as falhas de validação do domínio para o padrão RFC 7807 (ValidationProblemDetails).
    /// </summary>
    public static IResult ToProblemDetails(this ValidationFailed validationFailed)
    {
        // Agrupa os erros por propriedade (ex: "Title" -> ["Mínimo 5 chars", "Não pode ser vazio"])
        var errorsDictionary = validationFailed.Errors
            .GroupBy(x => x.Code, x => x.Description)
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.ToArray());

        return Results.ValidationProblem(
            errors: errorsDictionary,
            title: "Ocorreram um ou mais erros de validação.",
            statusCode: StatusCodes.Status400BadRequest,
            type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
        );
    }
}