using FluentValidation;
using TrackMyGradeAPI.DTOs;

namespace TrackMyGradeAPI.Validators
{
    /// <summary>
    /// Validates assignment creation, submission, and grading operations.
    /// Ensures referential integrity, business logic constraints, and data consistency.
    /// </summary>
    public class AssignmentCreateValidator : AbstractValidator<AssignmentCreateDto>
    {
        /// <summary>
        /// Initializes a new instance of the AssignmentCreateValidator class.
        /// </summary>
        public AssignmentCreateValidator()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Title is required")
                .Length(1, 200).WithMessage("Title must be 1–200 characters")
                .Matches(@"^[a-zA-Z0-9\s\-.,!?()]+$").WithMessage("Title must contain only alphanumeric characters and basic punctuation");

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .Length(0, 2000).WithMessage("Description must not exceed 2000 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.DueDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Due date is required")
                .GreaterThan(System.DateTime.UtcNow).WithMessage("Due date must be in the future");

            RuleFor(x => x.MaxScore)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Max score must be greater than zero")
                .LessThanOrEqualTo(100).WithMessage("Max score should not exceed 100 (typical grading scale)");

            RuleFor(x => x.ClassGroupId)
                .GreaterThan(0).WithMessage("A valid class group must be selected");
        }
    }

    /// <summary>
    /// Validates assignment submission creation.
    /// Ensures student submission content is valid and within constraints.
    /// </summary>
    public class SubmissionCreateValidator : AbstractValidator<SubmissionCreateDto>
    {
        /// <summary>
        /// Initializes a new instance of the SubmissionCreateValidator class.
        /// </summary>
        public SubmissionCreateValidator()
        {
            RuleFor(x => x.Content)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Submission content is required")
                .Length(1, 2000).WithMessage("Submission must be 1–2000 characters");
        }
    }

    /// <summary>
    /// Validates grading (scoring) operations.
    /// Ensures score is within valid range and feedback (if provided) is valid.
    /// </summary>
    public class GradingValidator : AbstractValidator<GradingDto>
    {
        /// <summary>
        /// Initializes a new instance of the GradingValidator class.
        /// </summary>
        public GradingValidator()
        {
            RuleFor(x => x.Score)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Score must not exceed 100 (typical grading scale)");

            RuleFor(x => x.Feedback)
                .Cascade(CascadeMode.Stop)
                .Length(0, 2000).WithMessage("Feedback must not exceed 2000 characters")
                .When(x => x.Feedback != null);
        }
    }
}
