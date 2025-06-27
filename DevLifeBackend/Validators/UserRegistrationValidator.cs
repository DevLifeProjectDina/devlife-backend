
using DevLifeBackend.DTOs;
using FluentValidation;

namespace DevLifeBackend.Validators
{
    public class UserRegistrationValidator : AbstractValidator<UserRegistrationDto>
    {
        private readonly List<string> _allowedStacks = new() { ".NET", "React", "Angular", "Python" };

        public UserRegistrationValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(20).WithMessage("Username cannot be longer than 20 characters.")
                .Matches("[a-zA-Z]").WithMessage("Username must contain at least one letter.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50)
                .Matches(@"^[\p{L}]+$").WithMessage("Name can only contain letters.");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required.")
                .MaximumLength(50)
                .Matches(@"^[\p{L}]+$").WithMessage("Surname can only contain letters.");

            RuleFor(x => x.Stacks)
                .NotEmpty().WithMessage("At least one stack technology is required.");

            RuleForEach(x => x.Stacks)
                .NotEmpty().WithMessage("Stack technology cannot be an empty string.")
                .Must(stack => _allowedStacks.Contains(stack))
                .WithMessage(x => $"Invalid stack detected. Allowed stacks are: {string.Join(", ", _allowedStacks)}");

            RuleFor(x => x.ExperienceLevel)
                .NotEmpty().WithMessage("Experience level is required.")
                .Must(level => new[] { "Junior", "Middle", "Senior" }.Contains(level))
                .WithMessage("Please enter a valid level: Junior, Middle, or Senior.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Now).WithMessage("Date of birth cannot be in the future.")
                .LessThanOrEqualTo(DateTime.Now.AddYears(-16)).WithMessage("You must be at least 16 years old to register.");
        }
    }
}