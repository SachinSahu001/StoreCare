using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Store
{
    public string Id { get; set; } = null!;

    public string StoreCode { get; set; } = null!;

    public string StoreName { get; set; } = null!;

    public string? Address { get; set; }

    public string? ContactNumber { get; set; }

    public string? Email { get; set; }

    public string? StoreLogo { get; set; }

    public int StatusId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual MasterTable Status { get; set; } = null!;

    public virtual ICollection<StoreProductAssignment> StoreProductAssignments { get; set; } = new List<StoreProductAssignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
