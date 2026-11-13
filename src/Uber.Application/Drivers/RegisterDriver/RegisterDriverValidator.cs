using FluentValidation;

namespace Uber.Application.Drivers.RegisterDriver;

public sealed class RegisterDriverValidator : AbstractValidator<RegisterDriverCommand>
{
    public RegisterDriverValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(6);
        RuleFor(command => command.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(command => command.DateOfBirth).LessThan(DateTime.UtcNow.Date);
        RuleFor(command => command.Gender).IsInEnum();

        RuleFor(command => command.Brand).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Model).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Plate).NotEmpty().MaximumLength(30);
        RuleFor(command => command.Color).NotEmpty().MaximumLength(50);
        RuleFor(command => command.YearMade).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(command => command.SeatingCapacity).InclusiveBetween(1, 100);
        RuleFor(command => command.VehicleType).IsInEnum();
        RuleFor(command => command.Transmission).IsInEnum();
        RuleFor(command => command.VehicleEngine).IsInEnum();
    }
}
