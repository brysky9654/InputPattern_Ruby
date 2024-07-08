using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Request
    {
        public string game { get; set; }
        public string provider { get; set; }
        public string action { get; set; }
        public int bet { get; set; }
        public string roundId { get; set; }
    }
}
