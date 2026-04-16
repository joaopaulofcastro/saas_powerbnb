using FluentValidation;
using MediatR;
using OneOf;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.SharedKernel.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (!failures.Any()) return await next();

        var errors = failures
            .Select(f => new Error(f.PropertyName, f.ErrorMessage))
            .ToArray();

        var validationFailed = new ValidationFailed(errors);

        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(OneOf<,>))
        {
            var secondType = responseType.GetGenericArguments()[1];
            if (secondType == typeof(ValidationFailed))
            {
                var oneOfType = responseType;
                var fromT1Method = oneOfType.GetMethod("FromT1");
                if (fromT1Method != null)
                {
                    var result = fromT1Method.Invoke(null, new object[] { validationFailed });
                    return (TResponse)result!;
                }
            }
        }

        return await next();
    }
}