using System;
using System.Collections.Generic;

namespace InputPattern.Database;

/// <summary>
/// There is Agent&apos;s Rtp.
/// this is setted by admin page.
/// </summary>
public partial class AgentRtp
{
    public int Id { get; set; }

    public int ApiId { get; set; }

    public string AgentCode { get; set; } = null!;

    public int Rtp { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Api Api { get; set; } = null!;
}
