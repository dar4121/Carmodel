using System;

namespace CarmodelAPI.Models.ViewModels
{
    public class CarModelViewModel
    {
        public int ModelId { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ModelName { get; set; }
        public string? ModelCode { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DateofManufacturing { get; set; }
        public bool? IsActive { get; set; }
        public int? SortOrder { get; set; }
        public string? DefaultImageUrl { get; set; }
    }
}