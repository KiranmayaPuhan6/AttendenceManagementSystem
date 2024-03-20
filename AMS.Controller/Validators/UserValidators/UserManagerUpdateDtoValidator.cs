using AMS.DtoLibrary.DTO.UserDto;
using FluentValidation;

namespace AMS.Controller.Validators.UserValidators
{
    public class UserManagerUpdateDtoValidator : AbstractValidator<UserManagerUpdateDto>
    {
        public UserManagerUpdateDtoValidator()
        {
            RuleFor(x => x.UserId)
               .NotEmpty().WithMessage("{PropertyName} is required.")
               .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");

            RuleFor(p => p.ManagerEmail)
             .Cascade(CascadeMode.StopOnFirstFailure)
             .NotEmpty().WithMessage("{PropertyName} should be not empty")
             .EmailAddress();
        }
    }
}
