using CarmodelAPI.Models.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace CarmodelAPI.Validators
{
    public class ImageUploadValidator : AbstractValidator<ImageUploadViewModel>
    {
        public ImageUploadValidator()
        {
            RuleFor(x => x.ModelId)
                .GreaterThan(0)
                .WithMessage("Car model ID is required");

            RuleFor(x => x.Images)
                .NotEmpty()
                .WithMessage("At least one image is required");

            RuleForEach(x => x.Images)
                .Must(BeValidImage)
                .WithMessage("Invalid image format. Only .jpg, .jpeg, .png, .gif are allowed.")
                .Must(BeValidSize)
                .WithMessage("Image size must be less than 5MB");
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null)
                return false;

            // Valid image extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return file.Length > 0 && allowedExtensions.Contains(extension);
        }

        private bool BeValidSize(IFormFile file)
        {
            if (file == null)
                return false;

            // 5MB max size
            return file.Length <= 5 * 1024 * 1024;
        }
    }
}