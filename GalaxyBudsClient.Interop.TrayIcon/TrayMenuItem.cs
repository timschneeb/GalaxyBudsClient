namespace GalaxyBudsClient.Interop.TrayIcon
{
    public class TrayMenuItem
    {
        public ItemType Id { set; get; }
        public string Title { set; get; }
        public bool IsEnabled { set; get; }
        public virtual bool IsSeparator => false;

        public TrayMenuItem(string title, bool isEnabled = true, ItemType id = ItemType.User)
        {
            Title = title;
            Id = id;
            IsEnabled = isEnabled;
        }

        public TrayMenuItem(string title, ItemType id)
        {
            Title = title;
            Id = id;
            IsEnabled = true;
        }
    }

    public class TrayMenuSeparator : TrayMenuItem
    {
        public override bool IsSeparator => true;

        public TrayMenuSeparator() : base(string.Empty) {}
    }
}
