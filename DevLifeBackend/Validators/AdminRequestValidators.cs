using DevLifeBackend.DTOs;
using FluentValidation;

namespace DevLifeBackend.Validators
{
    public class GenerateSnippetRequestValidator : AbstractValidator<GenerateSnippetRequestDto>
    {
        public GenerateSnippetRequestValidator()
        {
            RuleFor(x => x.Language).NotEmpty();
            RuleFor(x => x.Difficulty)
                .NotEmpty()
                .Must(d => new[] { "Junior", "Middle", "Senior" }.Contains(d))
                .WithMessage("Difficulty must be one of the following: Junior, Middle, Senior.");
        }
    }
}