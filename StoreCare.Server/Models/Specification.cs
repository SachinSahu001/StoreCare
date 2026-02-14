using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Specification
{
    public string Id { get; set; } = null!;

    public string SpecCode { get; set; } = null!;

    public string SpecName { get; set; } = null!;

    public string? SpecDescription { get; set; }

    public string? DataType { get; set; }

    public bool? IsRequired { get; set; }

    public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public int StatusId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<ItemSpecificationValue> ItemSpecificationValues { get; set; } = new List<ItemSpecificationValue>();

    public virtual Product Product { get; set; } = null!;

    public virtual MasterTable Status { get; set; } = null!;
}
