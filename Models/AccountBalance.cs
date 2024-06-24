using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern.Models
{
    public class AccountBalance
    {
        public string currencyCode { get; set; }
        public string balance { get; set; }
        public object realBalance { get; set; }
        public object bonusBalance { get; set; }
    }
}
