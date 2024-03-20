using AMS.DtoLibrary.DTO.UserDto;
using FluentValidation;
using System.Text.RegularExpressions;

namespace AMS.Controller.Validators.UserValidators
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
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
            RuleFor(p => p.Gender)
               .Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().WithMessage("{PropertyName} should be not empty")
               .Length(2, 25)
               .Must(IsValidGender).WithMessage("{PropertyName} should be either Male,Female or Others.");
        }

        private bool IsValid(string name)
        {
            var valid = name.All(char.IsLetter) || name.Contains(" ");
            return valid;
        }
        private bool IsValidGender(string gender)
        {
            if (gender == "Male" || gender == "Female" || gender == "Others")
            {
                return true;
            }
            return false;
        }

    }
}
