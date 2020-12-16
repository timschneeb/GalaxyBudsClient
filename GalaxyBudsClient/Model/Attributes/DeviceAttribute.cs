using System;

namespace GalaxyBudsClient.Model.Attributes
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
            string str = string.Empty;
            int i = 0;
            foreach (var model in Models)
            {
                str += model.ToString();
                if (i < Models.Length - 1)
                    str += ", ";

                i++;
            }
            return str;
        }

        public readonly Constants.Models[] Models;
    }
}
