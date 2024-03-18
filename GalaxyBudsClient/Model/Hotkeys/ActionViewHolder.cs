using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Model.Hotkeys
{
    public class ActionViewHolder(Event value)
    {
        public readonly Event Value = value;

        public override string ToString()
        {
            return Value.GetDescription();
        }
    }
}