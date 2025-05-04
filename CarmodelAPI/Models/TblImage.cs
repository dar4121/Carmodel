using System;
using System.Collections.Generic;

namespace CarmodelAPI.Models;

public partial class TblImage
{
    public int ImageId { get; set; }

    public int? ModelId { get; set; }

    public string? ImageName { get; set; }

    public bool? IsDefault { get; set; }
}
