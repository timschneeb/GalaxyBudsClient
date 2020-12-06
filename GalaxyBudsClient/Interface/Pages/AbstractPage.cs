
using Avalonia.Controls;

namespace GalaxyBudsClient.Interface.Pages
{
    public abstract class AbstractPage : UserControl
    {
        public enum Pages
        {
            Dummy,
            Home,
            FindMyGear,
            Touch,
            TouchCustomAction,
            AmbientSound,
            Equalizer,
            System,
            Credits,
            SelfTest,
            FactoryReset,
            NoConnection,
            Welcome,
            DeviceSelect,
            Settings,
            SettingsPopup,
            Advanced,
            UnsupportedFeature,
            UpdateAvailable
        }

        public abstract Pages PageType { get; }
        public virtual void OnPageShown(){}
        public virtual void OnPageHidden(){}
    }
}