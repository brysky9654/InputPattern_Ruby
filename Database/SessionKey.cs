using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class SessionKey
{
    public int Id { get; set; }

    public string SessionKey1 { get; set; } = null!;

    public string SessionKeyV2 { get; set; } = null!;

    public DateTime ExpireDateTime { get; set; }

    public string UserAgent { get; set; } = null!;
}
