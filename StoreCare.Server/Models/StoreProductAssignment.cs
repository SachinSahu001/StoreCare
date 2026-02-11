using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class StoreProductAssignment
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int ProductId { get; set; }

    public bool? CanManage { get; set; }

    public int? StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual MasterTable? Status { get; set; }

    public virtual Store Store { get; set; } = null!;
}
