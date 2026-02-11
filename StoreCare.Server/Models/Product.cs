using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Product
{
    public int Id { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public string? ProductImage { get; set; }

    public int CategoryId { get; set; }

    public string? BrandName { get; set; }

    public int? StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();

    public virtual MasterTable? Status { get; set; }

    public virtual ICollection<StoreProductAssignment> StoreProductAssignments { get; set; } = new List<StoreProductAssignment>();
}
