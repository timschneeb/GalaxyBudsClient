using System;

namespace GalaxyBudsClient.Model
{
    class DeviceAttribute : Attribute
    {
        public DeviceAttribute(Constants.Models[] models)
        {
            Models = models;
        }
        public DeviceAttribute(Constants.Models model)
        {
            Models = new Constants.Models[1];
            Models[0] = model;
        }

        public override string ToString()
        {
            String str = null;
            int i = 0;
            foreach (Constants.Models model in Models)
            {
                str += model.ToString();
                if (i < Models.Length - 1)
                    str += ", ";

                i++;
            }
            return str;
        }

        public Constants.Models[] Models;
    }
}
