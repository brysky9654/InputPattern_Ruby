using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class Request
    {
        public bool autoplay { get; set; }
        public List<Bet> bets { get; set; }
        public object offerId { get; set; }
        public object promotionId { get; set; }
        public int seq { get; set; }
        public string sessionUuid { get; set; }
    }
}
