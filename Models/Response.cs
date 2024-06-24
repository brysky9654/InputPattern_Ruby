using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Response
    {
        public Round round { get; set; }
        public bool promotionNoLongerAvailable { get; set; }
        public object promotionWin { get; set; }
        public object offer { get; set; }
        public object freeRoundOffer { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public AccountBalance accountBalance { get; set; }
        public object statusData { get; set; }
        public object dialog { get; set; }
        public object customData { get; set; }
        public DateTime serverTime { get; set; }
    }
}
