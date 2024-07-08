using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Response
    {
        public string roundId { get; set; }
        public Wager wager { get; set; }
        public float balance { get; set; }
    }

    public class Wager
    {
        public float win { get; set; }
        public Dictionary<string, object> state { get; set; }
        public List<Dictionary<string, object>> data { get; set; }
        public List<string> next { get; set; }
    }
}
