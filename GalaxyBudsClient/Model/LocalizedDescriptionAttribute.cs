using System.ComponentModel;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient.Model
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string key)
            : base(Loc.Resolve(key))
        {
        }
    }
}
