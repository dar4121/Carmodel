using System;
using System.Collections.Generic;

namespace CarmodelAPI.Models;

public partial class TblCarModel
{
    public int ModelId { get; set; }

    public int? BrandId { get; set; }

    public int? ClassId { get; set; }

    public string? ModelName { get; set; }

    public string? ModelCode { get; set; }

    public string? Description { get; set; }

    public string? Features { get; set; }

    public decimal? Price { get; set; }

    public DateTime? DateofManufacturing { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public int? Sortorder { get; set; }
}
