using AMS.DtoLibrary.DTO.LeaveDto;
using FluentValidation;

namespace AMS.Controller.Validators.LeaveValidators
{
    public class LeaveCreationDtoValidator : AbstractValidator<LeaveCreationDto>
    {
        public LeaveCreationDtoValidator() 
        {

            RuleFor(x => x.LeaveStartDate)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Must(BeAValidDate).WithMessage("{PropertyName} must be a valid date.")
                .LessThanOrEqualTo(x => x.LeaveEndDate).WithMessage("{PropertyName} must be before or equal to the end date.");

            RuleFor(x => x.LeaveEndDate)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Must(BeAValidDate).WithMessage("{PropertyName} must be a valid date.")
                .GreaterThanOrEqualTo(x => x.LeaveStartDate).WithMessage("{PropertyName} must be after or equal to the start date.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.");
        }
        private bool BeAValidDate(DateTime date)
        {
            return date != default;
        }
    }
}
