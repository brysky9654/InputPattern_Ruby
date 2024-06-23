using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class User
{
    public int Id { get; set; }

    public string AgentCode { get; set; } = null!;

    public string UserCode { get; set; } = null!;

    public string Token { get; set; } = null!;

    public int CurrencyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public decimal Balance { get; set; }

    public string NickName { get; set; } = null!;

    public int ApiId { get; set; }

    public virtual ICollection<AgentCall> AgentCalls { get; set; } = new List<AgentCall>();

    public virtual Api Api { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual ICollection<Wager> Wagers { get; set; } = new List<Wager>();
}
