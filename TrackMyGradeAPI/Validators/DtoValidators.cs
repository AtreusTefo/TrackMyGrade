using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    public class TeacherRegisterValidator : AbstractValidator<TeacherRegisterDto>
    {
        public TeacherRegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required")
                .MaximumLength(100).WithMessage("Subject must not exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Length(6, 20).WithMessage("Password must be between 6 and 20 characters");
        }
    }

    public class TeacherLoginValidator : AbstractValidator<TeacherLoginDto>
    {
        public TeacherLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    public class StudentCreateValidator : AbstractValidator<StudentCreateDto>
    {
        public StudentCreateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required")
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");

            RuleFor(x => x.Assessment1)
                .NotEmpty().WithMessage("Assessment1 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment1 must be between 0 and 20");

            RuleFor(x => x.Assessment2)
                .NotEmpty().WithMessage("Assessment2 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment2 must be between 0 and 20");

            RuleFor(x => x.Assessment3)
                .NotEmpty().WithMessage("Assessment3 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment3 must be between 0 and 20");
        }
    }

    public class StudentUpdateValidator : AbstractValidator<StudentUpdateDto>
    {
        public StudentUpdateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\d{8}$").WithMessage("Phone must be exactly 8 digits");

            RuleFor(x => x.Grade)
                .NotEmpty().WithMessage("Grade is required")
                .InclusiveBetween(1, 12).WithMessage("Grade must be between 1 and 12");

            RuleFor(x => x.Assessment1)
                .NotEmpty().WithMessage("Assessment1 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment1 must be between 0 and 20");

            RuleFor(x => x.Assessment2)
                .NotEmpty().WithMessage("Assessment2 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment2 must be between 0 and 20");

            RuleFor(x => x.Assessment3)
                .NotEmpty().WithMessage("Assessment3 is required")
                .InclusiveBetween(0, 20).WithMessage("Assessment3 must be between 0 and 20");
        }
    }
}
