using System.ComponentModel;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Model.Attributes
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public string Key { get; }

        public LocalizedDescriptionAttribute(string key)
            : base(Loc.Resolve(key))
        {
            Key = key;
        }
    }
}
