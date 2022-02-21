using FluentValidation;

namespace WeatherStation.DTO;

public class CreateUserValidator : AbstractValidator<CreateUser> {
    public CreateUserValidator () {
        RuleFor (d => d.Email).EmailAddress ();
        RuleFor (d => d.Name).NotEmpty ();
        RuleFor (d => d.Password).NotEmpty ().MinimumLength (4);
    }
}
