using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Model.Hotkeys
{
    public class ActionViewHolder(EventDispatcher.Event value)
    {
        public readonly EventDispatcher.Event Value = value;

        public override string ToString()
        {
            return Value.GetDescription();
        }
    }
}