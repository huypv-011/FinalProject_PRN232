using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BussinessObject;

public partial class Discount
{
    [Key]
    public int IdDiscount { get; set; }

    public string? Code { get; set; }

    public decimal? Percents { get; set; }

    public bool? Status { get; set; }

    public int? IdAccount { get; set; }

    public virtual Account? IdAccountNavigation { get; set; }
}
