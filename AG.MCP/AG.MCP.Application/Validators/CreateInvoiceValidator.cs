using AG.MCP.Application.DTOs;
using FluentValidation;

namespace AG.MCP.Application.Validators;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client is required");

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

public class CreateInvoiceItemValidator : AbstractValidator<CreateInvoiceItemRequest>
{
    public CreateInvoiceItemValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Item description is required")
            .MaximumLength(500);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required")
            .MaximumLength(20);
    }
}
