using AG.MCP.Application.DTOs;
using FluentValidation;

namespace AG.MCP.Application.Validators;

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .GreaterThanOrEqualTo(x => x.IssueDate).WithMessage("Due date must be on or after issue date");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Invoice must have at least one item");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemValidator());
    }
}
