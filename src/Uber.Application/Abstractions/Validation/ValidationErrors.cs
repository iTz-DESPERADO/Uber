using Uber.Domain.Common;

namespace Uber.Application.Abstractions.Validation;

public static class ValidationErrors
{
    public static Error From(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var message = string.Join(
            "; ",
            failures.Select(failure => failure.ErrorMessage).Distinct());

        return new Error("Validation.Failed", message);
    }
}
