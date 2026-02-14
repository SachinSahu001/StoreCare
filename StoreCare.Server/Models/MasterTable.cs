using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class MasterTable
{
    public int Id { get; set; }

    public string TableName { get; set; } = null!;

    public string TableValue { get; set; } = null!;

    public int? TableSequence { get; set; }

    public string? ItemDescription { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Order> OrderOrderStatuses { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderPaymentModes { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderPaymentStatuses { get; set; } = new List<Order>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();

    public virtual ICollection<StoreProductAssignment> StoreProductAssignments { get; set; } = new List<StoreProductAssignment>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();

    public virtual ICollection<User> UserRoles { get; set; } = new List<User>();

    public virtual ICollection<User> UserStatuses { get; set; } = new List<User>();
}
