using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class PatCruiseRoyale
{
    public int Id { get; set; }

    public string? GameCode { get; set; }

    public string PType { get; set; } = null!;

    public string Type { get; set; } = null!;

    /// <summary>
    /// If Free, Bonus true
    /// </summary>
    public byte GameDone { get; set; }

    /// <summary>
    /// pur, fsp, ind
    /// </summary>
    public string? Idx { get; set; }

    public int? Big { get; set; }

    public int? Small { get; set; }

    public double Win { get; set; }

    public double TotalWin { get; set; }

    public double TotalBet { get; set; }

    /// <summary>
    /// Previous bets stored to calculate multi in freepattern
    /// </summary>
    public double VirtualBet { get; set; }

    public double? Rtp { get; set; }

    public string? Balance { get; set; }

    public string? Pattern { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
