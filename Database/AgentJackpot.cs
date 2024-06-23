using System;
using System.Collections.Generic;

namespace InputPattern.Database;

/// <summary>
/// There is Agent&apos; Jackpot money.
/// If user bet in round, bet money&apos;s *percent is added jackpot money.
/// And then this money is gived to random user in random time.
/// </summary>
public partial class AgentJackpot
{
    public int Id { get; set; }

    public int ApiId { get; set; }

    public string AgentCode { get; set; } = null!;

    public decimal Balance { get; set; }

    /// <summary>
    /// 1:reserving,2:calling
    /// </summary>
    public byte Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Api Api { get; set; } = null!;
}
