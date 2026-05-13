using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>
    /// Validates admin-created student records.
    /// Password rules removed — students set their own password via activation flow.
    /// Assessment score rules removed — scores come from graded submissions now.
    /// </summary>
    public class AdminCreateStudentValidator : AbstractValidator<AdminCreateStudentDto>
    {
        /// <summary>
        /// Initializes a new instance of the AdminCreateStudentValidator class.
        /// </summary>
        public AdminCreateStudentValidator()
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required")
                .Matches(@"^[a-zA-Z '\-]+$").WithMessage("First name must contain only letters")
                .Length(2, 50).WithMessage("First name must be 2–50 characters");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required")
                .Matches(@"^[a-zA-Z '\-]+$").WithMessage("Last name must contain only letters")
                .Length(2, 50).WithMessage("Last name must be 2–50 characters");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.OmangOrPassport)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Omang No. or Passport is required")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Must contain only letters and digits")
                .Length(9).WithMessage("Must be exactly 9 characters");

            RuleFor(x => x.Grade)
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");

            RuleFor(x => x.TeacherId)
                .GreaterThan(0).WithMessage("A valid teacher must be selected");
        }
    }

    /// <summary>
    /// Validates admin student update records.
    /// </summary>
    public class AdminUpdateStudentValidator : AbstractValidator<AdminUpdateStudentDto>
    {
        /// <summary>
        /// Initializes a new instance of the AdminUpdateStudentValidator class.
        /// </summary>
        public AdminUpdateStudentValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().Length(2, 50).WithMessage("First name must be 2–50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().Length(2, 50).WithMessage("Last name must be 2–50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress().WithMessage("Valid email is required");

            RuleFor(x => x.OmangOrPassport)
                .NotEmpty().Length(9).WithMessage("Must be exactly 9 characters");

            RuleFor(x => x.Grade)
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");
        }
    }
}
