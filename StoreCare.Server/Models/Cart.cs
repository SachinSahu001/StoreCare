using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Cart
{
    public int Id { get; set; }

    public string CartSessionId { get; set; } = null!;

    public int UserId { get; set; }

    public int ItemId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? AddedDate { get; set; }

    public bool? IsCheckedOut { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
