using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class Api
{
    public int Id { get; set; }

    public string ApiCode { get; set; } = null!;

    public string CallbackUrl { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string SecretKey { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AgentJackpot> AgentJackpots { get; set; } = new List<AgentJackpot>();

    public virtual ICollection<AgentRtp> AgentRtps { get; set; } = new List<AgentRtp>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
