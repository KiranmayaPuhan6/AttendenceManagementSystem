using AMS.Services.Utility;
using FluentValidation;

namespace AMS.Controller.Validators.UserValidators
{
    public class UpdateRoleValidator : AbstractValidator<UpdateRole>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.UserId)
               .NotEmpty().WithMessage("User ID is required.")
               .GreaterThan(0).WithMessage("User ID must be greater than zero.");

            RuleFor(p => p.Role)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotEmpty().WithMessage("{PropertyName} should be not empty")
              .Length(2, 25)
              .Must(IsValidRole).WithMessage("{PropertyName} should be either Admin,Manager or Employee.");
        }

        private bool IsValidRole(string role)
        {
            if (role == "Admin" || role == "Manager" || role == "Employee")
            {
                return true;
            }
            return false;
        }
    }
}
