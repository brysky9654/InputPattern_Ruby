using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class Wager
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string GameCode { get; set; } = null!;

    public long RoundId { get; set; }

    public decimal BetAmount { get; set; }

    public decimal PayoutAmount { get; set; }

    public decimal Balance { get; set; }

    public string Detail { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 0: debit 1: credit 2:cancel
    /// </summary>
    public byte Status { get; set; }

    public virtual User User { get; set; } = null!;
}
