using Galaxy_Buds_Client.model.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy_Buds_Client.model
{
    class DeviceAttribute : Attribute
    {
        public DeviceAttribute(Model[] models)
        {
            Models = models;
        }
        public DeviceAttribute(Model model)
        {
            Models = new Model[1];
            Models[0] = model;
        }

        public override string ToString()
        {
            String str = null;
            int i = 0;
            foreach (Model model in Models)
            {
                str += model.ToString();
                if (i < Models.Length - 1)
                    str += ", ";

                i++;
            }
            return str;
        }

        public Model[] Models;
    }
}
