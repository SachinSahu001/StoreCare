using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Item
{
    public string Id { get; set; } = null!;

    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? ItemDescription { get; set; }

    public string? ItemImage { get; set; }

    public string ProductId { get; set; } = null!;

    public string StoreId { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? TaxPercent { get; set; }

    public int StatusId { get; set; }

    public bool? IsFeatured { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Inventory? Inventory { get; set; }

    public virtual ICollection<ItemSpecificationValue> ItemSpecificationValues { get; set; } = new List<ItemSpecificationValue>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual MasterTable Status { get; set; } = null!;

    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public virtual Store Store { get; set; } = null!;
}
