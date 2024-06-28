using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Data
    {
        public string? position { get; set; }
        public string? winner { get; set; }
        public string? loser { get; set; }
        public string? winAmount { get; set; }
        public string? symbol { get; set; }
        public string? mask { get; set; }
        public string? count { get; set; }
        public string? bfw { get; set; }
        public string? bfc { get; set; }
        public string? multiplier { get; set; }
        public string? wilds { get; set; }
        public string? lives { get; set; }
        public string? baseWinAmount { get; set; }
        public List<string> winMultipliers { get; set; }
    }
}
