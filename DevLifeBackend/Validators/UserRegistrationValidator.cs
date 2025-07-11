using DevLifeBackend.DTOs;
using DevLifeBackend.Enums;
using FluentValidation;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DevLifeBackend.Validators
{
    public class UserRegistrationValidator : AbstractValidator<UserRegistrationDto>
    {
        private static readonly HashSet<TechStack> _allowedIndividualStacks = new HashSet<TechStack>(
            Enum.GetValues(typeof(TechStack))
                .Cast<TechStack>()
                .Where(ts => (int)(object)ts != 0 && ts != TechStack.Unknown)
        );

        private static readonly HashSet<ExperienceLevel> _allowedExperienceLevels = new HashSet<ExperienceLevel>(
            Enum.GetValues(typeof(ExperienceLevel))
                .Cast<ExperienceLevel>()
                .Where(el => el != ExperienceLevel.Unknown)
        );

        public UserRegistrationValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
                .MaximumLength(20).WithMessage("Username cannot be longer than 20 characters.")
                .Matches("[a-zA-Z]").WithMessage("Username must contain at least one letter.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name cannot be longer than 50 characters.")
                .Matches(@"^[\p{L}]+$").WithMessage("Name can only contain letters.");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Surname is required.")
                .MaximumLength(50).WithMessage("Surname cannot be longer than 50 characters.")
                .Matches(@"^[\p{L}]+$").WithMessage("Surname can only contain letters.");

            RuleFor(x => x.Stacks)
                .Must((dto, userStacks) =>
                {
                    if ((int)(object)userStacks == 0 || userStacks == TechStack.Unknown)
                    {
                        return false; 
                    }

                    var selectedIndividualFlags = Enum.GetValues(typeof(TechStack))
                                                      .Cast<TechStack>()
                                                      .Where(f => (int)(object)f != 0 && f != TechStack.Unknown && userStacks.HasFlag(f))
                                                      .ToList();

                    if (!selectedIndividualFlags.Any())
                    {
                        return false;
                    }

                    foreach (var selectedFlag in selectedIndividualFlags)
                    {
                        if (!_allowedIndividualStacks.Contains(selectedFlag))
                        {
                            return false; 
                        }
                    }

                    return true; 
                })
                .WithMessage($"You must select at least one technology. Allowed technologies are: {string.Join(", ", _allowedIndividualStacks.Select(s => s.ToString()))}");

            RuleFor(x => x.ExperienceLevel)
                .Must((dto, userExperienceLevel) =>
                {
                    if (userExperienceLevel == ExperienceLevel.Unknown)
                    {
                        return false;
                    }

                    return _allowedExperienceLevels.Contains(userExperienceLevel);
                })
                .WithMessage($"You must select an experience level. Allowed levels are: {string.Join(", ", _allowedExperienceLevels.Select(el => el.ToString()))}");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Now).WithMessage("Date of birth cannot be in the future.")
                .LessThanOrEqualTo(DateTime.Now.AddYears(-16)).WithMessage("You must be at least 16 years old to register.");
        }
    }
}