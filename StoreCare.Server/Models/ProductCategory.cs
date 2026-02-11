using System;
using System.Collections.Generic;

namespace StoreCare.Server.Models;

public partial class ProductCategory
{
    public int Id { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string? CategoryDescription { get; set; }

    public string? CategoryImage { get; set; }

    public int? DisplayOrder { get; set; }

    public int? StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual MasterTable? Status { get; set; }
}
