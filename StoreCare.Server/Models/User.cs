using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string? StoreId { get; set; }

    public string? ProfilePicture { get; set; }

    public int? StatusId { get; set; }

    public DateTime? LastLogin { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual MasterTable Role { get; set; } = null!;

    public virtual MasterTable? Status { get; set; }

    public virtual Store? Store { get; set; }
}
