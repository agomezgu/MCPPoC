using AG.MCP.Application.DTOs;
using FluentValidation;

namespace AG.MCP.Application.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required");

        RuleFor(x => x.Reference)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
