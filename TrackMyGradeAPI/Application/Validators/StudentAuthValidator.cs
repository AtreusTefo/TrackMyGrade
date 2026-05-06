using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>Validates student login credentials.</summary>
    public class StudentLoginValidator : AbstractValidator<StudentLoginDto>
    {
        public StudentLoginValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    /// <summary>Validates the account activation request (sets first password).</summary>
    public class ActivateAccountValidator : AbstractValidator<ActivateAccountDto>
    {
        public ActivateAccountValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(r => r == "Teacher" || r == "Student")
                .WithMessage("Role must be 'Teacher' or 'Student'");

            RuleFor(x => x.ActivationToken)
                .NotEmpty().WithMessage("Activation token is required");

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }
}
