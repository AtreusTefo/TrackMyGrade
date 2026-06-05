using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>
    /// Teacher login validation.
    /// TeacherRegisterValidator removed — teachers are created by admins now.
    /// </summary>
    public class TeacherLoginValidator : AbstractValidator<TeacherLoginDto>
    {
        /// <summary>
        /// Initializes a new instance of the TeacherLoginValidator class.
        /// </summary>
        public TeacherLoginValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    /// <summary>Validates admin-created teacher accounts.</summary>
    public class AdminCreateTeacherValidator : AbstractValidator<AdminCreateTeacherDto>
    {
        /// <summary>
        /// Initializes a new instance of the AdminCreateTeacherValidator class.
        /// </summary>
        public AdminCreateTeacherValidator()
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

            RuleFor(x => x.Subject)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Subject is required")
                .MaximumLength(100).WithMessage("Subject cannot exceed 100 characters");
        }
    }

    /// <summary>Validates assignment creation by a teacher.</summary>
    public class AssignmentCreateValidator : AbstractValidator<AssignmentCreateDto>
    {
        /// <summary>
        /// Initializes a new instance of the AssignmentCreateValidator class.
        /// </summary>
        public AssignmentCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().MaximumLength(200).WithMessage("Title is required (max 200 chars)");

            RuleFor(x => x.MaxScore)
                .GreaterThan(0).WithMessage("Max score must be greater than 0");

            RuleFor(x => x.ClassGroupId)
                .GreaterThan(0).WithMessage("A valid class must be selected");
        }
    }
}
