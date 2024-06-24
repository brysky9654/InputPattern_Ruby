using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Round
    {
        public string status { get; set; }
        public object jackpotWin { get; set; }
        public string roundId { get; set; }
        public List<object> possibleActions { get; set; }
        public List<Event> events { get; set; }
    }
}
