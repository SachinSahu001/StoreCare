using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class Cart
{
    public string Id { get; set; } = null!;

    public string CartSessionId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public int? Quantity { get; set; }

    public DateTime? AddedDate { get; set; }

    public bool? IsCheckedOut { get; set; }

    public bool? Active { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
