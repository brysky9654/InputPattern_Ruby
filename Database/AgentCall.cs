using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class AgentCall
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int GameId { get; set; }

    /// <summary>
    /// Reserved Money
    /// </summary>
    public decimal CallMoney { get; set; }

    /// <summary>
    /// 0:reserve,1:apply,2:cancel
    /// </summary>
    public byte Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 1:free,2:buy
    /// </summary>
    public byte Type { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Missed Money
    /// </summary>
    public decimal MissedMoney { get; set; }

    /// <summary>
    /// Bet Money
    /// </summary>
    public decimal BetAmount { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
