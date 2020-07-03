using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy_Buds_Client.model
{
    class DeviceAttribute : Attribute
    {
        private Constants.Model buds;

        public DeviceAttribute(Constants.Model buds)
        {
            this.buds = buds;
        }

        public Constants.Model Model
        {
            get => buds;
        }
    }
}
