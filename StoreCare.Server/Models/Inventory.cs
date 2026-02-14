using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Inventory
{
    public string Id { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public int? CurrentStock { get; set; }

    public int? MinimumStock { get; set; }

    public int? MaximumStock { get; set; }

    public DateTime? LastStockInDate { get; set; }

    public DateTime? LastStockOutDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;
}
