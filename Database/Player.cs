using System;
using System.Collections.Generic;

namespace InputPattern.Database;

public partial class Player
{
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// 0:disconnected,1:connected
    /// </summary>
    public byte Connected { get; set; }

    public byte GameDone { get; set; }

    public int GameId { get; set; }

    public int Big { get; set; }

    public int Small { get; set; }

    public int CurIndex { get; set; }

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public string Machine { get; set; } = null!;

    /// <summary>
    /// Previous bets stored to calculate multi in free pattern
    /// </summary>
    public decimal VirtualBet { get; set; }

    /// <summary>
    /// Added because the original Free first spin win value is forgotten while looking for another pattern in FSOption.
    /// </summary>
    public decimal TotalWin { get; set; }

    public decimal LastBet { get; set; }

    public decimal LastWin { get; set; }

    public string LastPattern { get; set; } = null!;

    public string Settings { get; set; } = null!;

    public string ReplayLogList { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public int CurWager { get; set; }

    public decimal TotalJackpot { get; set; }

    public decimal TotalCall { get; set; }

    public int CallId { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
