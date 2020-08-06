using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.model
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string key)
            : base(Loc.GetString(key))
        {
        }
    }
}
