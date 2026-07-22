using FluentValidation;
using ProductService.DTOs;

namespace ProductService.Validators;

public class ProductValidator
    : AbstractValidator<CreateProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}