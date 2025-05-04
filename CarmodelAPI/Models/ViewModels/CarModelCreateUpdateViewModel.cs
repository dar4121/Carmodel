using System;

namespace CarmodelAPI.Models.ViewModels
{
    public class CarModelCreateUpdateViewModel
    {
        public int? ModelId { get; set; } // Null for insert, value for update
        public int BrandId { get; set; }
        public int ClassId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string? ModelCode { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DateofManufacturing { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
    }
}