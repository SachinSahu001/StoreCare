using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class LoginHistory
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime? LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Browser { get; set; }

    public string? Platform { get; set; }

    public string? DeviceType { get; set; }

    public string? Status { get; set; }

    public string? FailureReason { get; set; }

    public string? SessionId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? Active { get; set; }

    public virtual User User { get; set; } = null!;
}
