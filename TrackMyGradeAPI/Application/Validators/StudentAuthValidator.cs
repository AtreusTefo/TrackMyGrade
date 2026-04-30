using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    public class StudentLoginValidator : AbstractValidator<StudentLoginDto>
    {
        public StudentLoginValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    public class StudentSubmitAssessmentsValidator : AbstractValidator<StudentSubmitAssessmentsDto>
    {
        public StudentSubmitAssessmentsValidator()
        {
            RuleFor(x => x.Assessment1)
                .InclusiveBetween(0, 20).WithMessage("Assessment 1 must be between 0 and 20");

            RuleFor(x => x.Assessment2)
                .InclusiveBetween(0, 20).WithMessage("Assessment 2 must be between 0 and 20");

            RuleFor(x => x.Assessment3)
                .InclusiveBetween(0, 20).WithMessage("Assessment 3 must be between 0 and 20");
        }
    }
}
