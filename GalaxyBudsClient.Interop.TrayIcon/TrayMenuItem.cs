namespace GalaxyBudsClient.Interop.TrayIcon
{
    public class TrayMenuItem
    {
        public int Id { set; get; }
        public string Title { set; get; }
        public bool IsEnabled { set; get; }
        public virtual bool IsSeparator => false;

        public TrayMenuItem(string title, bool isEnabled = true)
        {
            Title = title;
            IsEnabled = isEnabled;
        }
    }

    public class TrayMenuSeparator : TrayMenuItem
    {
        public override bool IsSeparator => true;

        public TrayMenuSeparator() : base(string.Empty) {}
    }
}
