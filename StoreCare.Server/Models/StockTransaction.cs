using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class StockTransaction
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public string? TransactionType { get; set; }

    public int Quantity { get; set; }

    public int PreviousStock { get; set; }

    public int NewStock { get; set; }

    public int? ReferenceId { get; set; }

    public string? Remarks { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;
}
