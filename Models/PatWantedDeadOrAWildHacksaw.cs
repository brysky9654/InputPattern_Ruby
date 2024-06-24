using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class PatWantedDeadOrAWildHacksaw
    {
        public int id { get; set; }
        public string gameCode { get; set; }
        public string gameName { get; set; }
        public string pType { get; set; }
        public string type { get; set; }
        public byte gameDone { get; set; }
        public string idx { get; set; }
        public int big { get; set; }
        public int small { get; set; }
        public int win { get; set; }
        public int totalWin { get; set; }
        public int totalBet { get; set; }
        public int virtualBet { get; set; }
        public int rtp { get; set; }
        public int? balance { get; set; }
        public string pattern { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
