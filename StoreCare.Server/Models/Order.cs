using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int UserId { get; set; }

    public int StoreId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal NetAmount { get; set; }

    public int PaymentModeId { get; set; }

    public int? PaymentStatusId { get; set; }

    public int? OrderStatusId { get; set; }

    public string? ShippingAddress { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual MasterTable? OrderStatus { get; set; }

    public virtual MasterTable PaymentMode { get; set; } = null!;

    public virtual MasterTable? PaymentStatus { get; set; }

    public virtual Store Store { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
