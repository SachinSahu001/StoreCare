using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Order
{
    public string Id { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string StoreId { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal NetAmount { get; set; }

    public int PaymentModeId { get; set; }

    public int PaymentStatusId { get; set; }

    public int OrderStatusId { get; set; }

    public string? ShippingAddress { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual MasterTable OrderStatus { get; set; } = null!;

    public virtual MasterTable PaymentMode { get; set; } = null!;

    public virtual MasterTable PaymentStatus { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
