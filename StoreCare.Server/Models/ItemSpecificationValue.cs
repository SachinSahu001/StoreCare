using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class ItemSpecificationValue
{
    public string Id { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public string SpecificationId { get; set; } = null!;

    public string? SpecValue { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Specification Specification { get; set; } = null!;
}
