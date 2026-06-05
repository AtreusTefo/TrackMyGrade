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
                .Length(2, 100).WithMessage("First name must be 2–100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name must contain only letters, spaces, hyphens, or apostrophes");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 100).WithMessage("Last name must be 2–100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name must contain only letters, spaces, hyphens, or apostrophes");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits (Botswana format)");

            RuleFor(x => x.OmangOrPassport)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Omang No. or Passport is required")
                .Matches(@"^[a-zA-Z0-9]{9}$").WithMessage("Must be exactly 9 alphanumeric characters (letters and numbers only)");

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
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 100).WithMessage("First name must be 2–100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name must contain only letters, spaces, hyphens, or apostrophes");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 100).WithMessage("Last name must be 2–100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name must contain only letters, spaces, hyphens, or apostrophes");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email is required");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits (Botswana format)");

            RuleFor(x => x.OmangOrPassport)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("OMANG or Passport is required")
                .Matches(@"^[a-zA-Z0-9]{9}$").WithMessage("Must be exactly 9 alphanumeric characters (letters and numbers only)");

            RuleFor(x => x.Grade)
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");

            RuleFor(x => x.TeacherId)
                .GreaterThan(0).WithMessage("A valid teacher must be selected");
        }
    }
}
