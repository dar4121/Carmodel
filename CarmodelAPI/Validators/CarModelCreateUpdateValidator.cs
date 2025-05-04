using CarmodelAPI.Models.ViewModels;
using FluentValidation;
using System;

namespace CarmodelAPI.Validators
{
    public class CarModelCreateUpdateValidator : AbstractValidator<CarModelCreateUpdateViewModel>
    {
        public CarModelCreateUpdateValidator()
        {
            RuleFor(x => x.BrandId)
                .NotEmpty()
                .WithMessage("Brand is required")
                .GreaterThan(0)
                .WithMessage("Please select a valid brand");

            RuleFor(x => x.ClassId)
                .NotEmpty()
                .WithMessage("Class is required")
                .GreaterThan(0)
                .WithMessage("Please select a valid class");

            RuleFor(x => x.ModelName)
                .NotEmpty()
                .WithMessage("Model name is required")
                .MaximumLength(50)
                .WithMessage("Model name cannot exceed 50 characters");

            RuleFor(x => x.ModelCode)
                .NotEmpty()
                .WithMessage("Model code is required")
                .MaximumLength(50)
                .WithMessage("Model code cannot exceed 50 characters");

            RuleFor(x => x.Price)
                .NotNull()
                .WithMessage("Price is required")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative");

            RuleFor(x => x.DateofManufacturing)
                .LessThanOrEqualTo(DateTime.Now)
                .When(x => x.DateofManufacturing.HasValue)
                .WithMessage("Manufacturing date cannot be in the future");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Features)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Features))
                .WithMessage("Features cannot exceed 500 characters");
        }
    }
}