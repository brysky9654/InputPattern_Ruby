using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputPattern
{
    public class HashCodeManager
    {
        private static HashCodeManager _instance = null;

        public static HashCodeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HashCodeManager();
                return _instance;
            }
        }


    }
}
