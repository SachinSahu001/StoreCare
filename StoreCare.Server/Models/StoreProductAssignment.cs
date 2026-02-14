using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class StoreProductAssignment
{
    public string Id { get; set; } = null!;

    public string StoreId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public bool? CanManage { get; set; }

    public int StatusId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual MasterTable Status { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
