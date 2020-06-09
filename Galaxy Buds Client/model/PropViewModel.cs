using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy_Buds_Client.model
{
    class PropViewModel
    {
        public String Key { get; set; }
        public String Value { get; set; }

        public PropViewModel(String key, String value)
        {
            Key = key;
            Value = value;
        }
    }
}
