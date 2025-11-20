using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BussinessObject;

public partial class Service
{
    [Key]
    public int IdService { get; set; }

    public string? Name { get; set; }

    public string? Describe { get; set; }

    public string? Image { get; set; }

    public double Price { get; set; }

    public virtual ICollection<AddService> AddServices { get; set; } = new List<AddService>();
}
