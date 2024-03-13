﻿using FluentValidation;
using System.Text.RegularExpressions;
using UserMicroservices.Models.DTO;

namespace UserMicroservices.Validators
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
            RuleFor(p => p.ManagerName)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotEmpty().WithMessage("{PropertyName} should be not empty")
              .Length(2, 25)
              .Must(IsValid).WithMessage("{PropertyName} should be all letters.");
            RuleFor(p => p.ManagerEmail)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotEmpty().WithMessage("{PropertyName} should be not empty")
              .EmailAddress();
            RuleFor(p => p.Gender)
               .Cascade(CascadeMode.StopOnFirstFailure)
               .NotEmpty().WithMessage("{PropertyName} should be not empty")
               .Length(2, 25)
               .Must(IsValidGender).WithMessage("{PropertyName} should be either Male,Female or Others.");
            RuleFor(p => p.Role)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotEmpty().WithMessage("{PropertyName} should be not empty")
              .Length(2, 25)
              .Must(IsValidRole).WithMessage("{PropertyName} should be either Admin,Manager or Employee.");
            RuleFor(p => p.PhoneNumber)
              .NotEmpty()
              .NotNull().WithMessage("Phone Number is required.")
              .MinimumLength(10).WithMessage("PhoneNumber must not be less than 10 characters.")
              .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.")
              .Matches(new Regex(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")).WithMessage("PhoneNumber not valid");
            RuleFor(p => p.Email)
             .Cascade(CascadeMode.StopOnFirstFailure)
             .NotEmpty().WithMessage("{PropertyName} should be not empty")
             .EmailAddress();
        }

        private bool IsValid(string name)
        {
            var valid = name.All(Char.IsLetter) || name.Contains(" ");
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
