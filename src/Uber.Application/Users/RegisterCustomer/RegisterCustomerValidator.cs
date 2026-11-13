using FluentValidation;

namespace Uber.Application.Users.RegisterCustomer;

public sealed class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(6);
        RuleFor(command => command.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(command => command.DateOfBirth).LessThan(DateTime.UtcNow.Date);
        RuleFor(command => command.Gender).IsInEnum();
    }
}
