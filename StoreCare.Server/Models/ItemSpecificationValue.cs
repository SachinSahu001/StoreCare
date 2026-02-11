using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class ItemSpecificationValue
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int SpecificationId { get; set; }

    public string? SpecValue { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Specification Specification { get; set; } = null!;
}
