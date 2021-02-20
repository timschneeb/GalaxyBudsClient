using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Model.Hotkeys
{
    public class ActionViewHolder
    {
        public readonly EventDispatcher.Event Value;

        public ActionViewHolder(EventDispatcher.Event value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.GetDescription();
        }
    }
}