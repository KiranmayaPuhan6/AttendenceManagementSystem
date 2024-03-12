using FluentValidation;
using UserMicroservices.Models.Domain.Entities;

namespace UserMicroservices.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator() 
        {
            RuleFor(p => p.Name)
               .Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().WithMessage("{PropertyName} should be not empty")
               .Length(2, 25)
               .Must(IsValid).WithMessage("{PropertyName} should be all letters.");
            RuleFor(p => p.Company)
               .Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().WithMessage("{PropertyName} should be not empty")
               .Length(2, 50);
            RuleFor(p => p.Designation)
               .Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().WithMessage("{PropertyName} should be not empty");
        }

        private bool IsValid(string name)
        {
            return name.All(Char.IsLetter);
        }
    }
}
