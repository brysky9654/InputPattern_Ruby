using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class Game
{
    public int Id { get; set; }

    public string? GameCode { get; set; }

    public string? GameName { get; set; }

    public string? Slug { get; set; }

    public int? RtpDes { get; set; }

    /// <summary>
    /// /// 0: blocked
    /// /// 1: normal
    /// /// 2: special
    /// /// 3: option
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// /// 0: testing
    /// /// 1: new
    /// /// 2: normal
    /// /// 3: maintence
    /// 
    /// </summary>
    public int Status { get; set; }

    public string? Thumbnail { get; set; }

    public int PayLines { get; set; }

    public string? InitPattern { get; set; }

    public string? Memo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Version { get; set; }

    public virtual ICollection<AgentCall> AgentCalls { get; set; } = new List<AgentCall>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}
