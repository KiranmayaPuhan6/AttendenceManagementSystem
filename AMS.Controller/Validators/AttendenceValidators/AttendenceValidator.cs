using AMS.Entities.Models.Domain.Entities;
using FluentValidation;

namespace AMS.Controller.Validators.AttendenceValidators
{
    public class AttendenceValidator : AbstractValidator<Attendence>
    {
        public AttendenceValidator()
        {
            RuleFor(p => p.AttendenceType)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotEmpty().WithMessage("{PropertyName} should be not empty")
              .Length(2, 25)
              .Must(IsValid).WithMessage("{PropertyName} should be all letters.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .GreaterThan(0).WithMessage("User ID must be greater than zero.");
        }

        private bool IsValid(string attendenceType)
        {
            if (attendenceType == "Regular" || attendenceType == "Leave" || attendenceType == "Holiday")
            {
                return true;
            }
            return false;
        }
    }
}
