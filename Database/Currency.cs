using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class Currency
{
    public int Id { get; set; }

    /// <summary>
    /// USD
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// $
    /// </summary>
    public string Symbol { get; set; } = null!;

    public int Rate { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
