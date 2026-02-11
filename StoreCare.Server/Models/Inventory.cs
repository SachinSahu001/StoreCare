using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int? CurrentStock { get; set; }

    public int? MinimumStock { get; set; }

    public int? MaximumStock { get; set; }

    public DateTime? LastStockInDate { get; set; }

    public DateTime? LastStockOutDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;
}
