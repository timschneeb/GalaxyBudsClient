using System;

namespace GalaxyBudsClient.Model
{
    public class PropertyViewModel
    {
        public String Key { get; set; }
        public String Value { get; set; }

        public PropertyViewModel(String key, String value)
        {
            Key = key;
            Value = value;
        }
    }
}
