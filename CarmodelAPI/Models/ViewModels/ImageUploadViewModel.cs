using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CarmodelAPI.Models.ViewModels
{
    public class ImageUploadViewModel
    {
        public int ModelId { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public bool IsDefault { get; set; }

        // For backward compatibility with single image upload
        public IFormFile? Image
        {
            get => Images?.Count > 0 ? Images[0] : null;
            set { if (value != null) { if (Images == null) Images = new List<IFormFile>(); Images.Add(value); } }
        }
    }
}