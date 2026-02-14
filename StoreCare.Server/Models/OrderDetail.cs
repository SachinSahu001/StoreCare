using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class OrderDetail
{
    public string Id { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal TotalPrice { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
