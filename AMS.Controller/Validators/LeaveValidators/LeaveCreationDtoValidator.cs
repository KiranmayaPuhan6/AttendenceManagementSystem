using AMS.DtoLibrary.DTO.LeaveDto;
using FluentValidation;

namespace AMS.Controller.Validators.LeaveValidators
{
    public class LeaveCreationDtoValidator : AbstractValidator<LeaveCreationDto>
    {
        public LeaveCreationDtoValidator() 
        {

            RuleFor(x => x.LeaveStartDate)
                .NotEmpty().WithMessage("Leave start date is required.")
                .Must(BeAValidDate).WithMessage("Leave start date must be a valid date.")
                .LessThanOrEqualTo(x => x.LeaveEndDate).WithMessage("Leave start date must be before or equal to the end date.");

            RuleFor(x => x.LeaveEndDate)
                .NotEmpty().WithMessage("Leave end date is required.")
                .Must(BeAValidDate).WithMessage("Leave end date must be a valid date.")
                .GreaterThanOrEqualTo(x => x.LeaveStartDate).WithMessage("Leave end date must be after or equal to the start date.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .GreaterThan(0).WithMessage("User ID must be greater than zero.");
        }
        private bool BeAValidDate(DateTime date)
        {
            return date != default;
        }
    }
}
