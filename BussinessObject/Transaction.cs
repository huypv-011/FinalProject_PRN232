using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BussinessObject;

public partial class Transaction
{
    [Key]
    public int IdTransactions { get; set; }

    public double Price { get; set; }

    public DateTime Date { get; set; }

    public virtual BookingOnline IdTransactionsNavigation { get; set; } = null!;
}
