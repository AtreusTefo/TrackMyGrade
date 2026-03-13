using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>
    /// Centralizes all student validation rules. StudentCreateValidator and
    /// StudentUpdateValidator inherit this so rules are never duplicated.
    /// </summary>
    public abstract class StudentBaseValidator<T> : AbstractValidator<T>
        where T : StudentDtoBase
    {
        protected StudentBaseValidator()
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required")
                .Matches(@"^[a-zA-Z '\-]+$").WithMessage("First name must contain only letters")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required")
                .Matches(@"^[a-zA-Z '\-]+$").WithMessage("Last name must contain only letters")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.OmangOrPassport)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Omang No. or Passport is required")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Omang No. or Passport must contain only letters and digits")
                .Length(9).WithMessage("Omang No. or Passport must be exactly 9 characters");

            RuleFor(x => x.Grade)
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");

            RuleFor(x => x.Assessment1)
                .InclusiveBetween(0, 20).WithMessage("Assessment 1 must be between 0 and 20");

            RuleFor(x => x.Assessment2)
                .InclusiveBetween(0, 20).WithMessage("Assessment 2 must be between 0 and 20");

            RuleFor(x => x.Assessment3)
                .InclusiveBetween(0, 20).WithMessage("Assessment 3 must be between 0 and 20");
        }
    }

    public class StudentCreateValidator : StudentBaseValidator<StudentCreateDto> { }

    public class StudentUpdateValidator : StudentBaseValidator<StudentUpdateDto> { }
}
