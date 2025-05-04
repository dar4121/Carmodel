namespace CarmodelAPI.Models.ViewModels
{
    public class ImageViewModel
    {
        public int ImageId { get; set; }
        public int ModelId { get; set; }
        public string ImageName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}