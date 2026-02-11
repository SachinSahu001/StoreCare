using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Specification
{
    public int Id { get; set; }

    public string SpecCode { get; set; } = null!;

    public string SpecName { get; set; } = null!;

    public string? SpecDescription { get; set; }

    public string? DataType { get; set; }

    public bool? IsRequired { get; set; }

    public int ProductId { get; set; }

    public int? DisplayOrder { get; set; }

    public int? StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<ItemSpecificationValue> ItemSpecificationValues { get; set; } = new List<ItemSpecificationValue>();

    public virtual Product Product { get; set; } = null!;

    public virtual MasterTable? Status { get; set; }
}
