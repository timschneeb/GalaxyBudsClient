namespace GalaxyBudsClient.Model.Hotkeys
{
    public class ActionViewHolder
    {
        public readonly string DisplayName;
        public readonly EventDispatcher.Event Value;

        public ActionViewHolder(string displayName, EventDispatcher.Event value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}