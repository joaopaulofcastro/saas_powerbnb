namespace SaaS.PowerBnB.SharedKernel.Application.Errors;

public readonly record struct ValidationFailed(IEnumerable<Error> Errors)
{
    // Construtor de conveniência para um erro só
    public ValidationFailed(Error error) : this(new[] { error }) { }
}
