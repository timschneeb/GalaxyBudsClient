
using Avalonia.Controls;

namespace GalaxyBudsClient.Interface.Pages
{
    public abstract class AbstractPage : UserControl
    {
        public enum Pages
        {
            Dummy,
            Dummy2,
            Home,
            FindMyGear,
            Touch,
            TouchCustomAction,
            AmbientSound,
            NoiseControlPro,
            Equalizer,
            System,
            SystemInfo,
            SystemCoredump,
            Credits,
            SelfTest,
            FactoryReset,
            NoConnection,
            Welcome,
            DeviceSelect,
            Settings,
            SettingsPopup,
            Advanced,
            Hotkeys,
            UnsupportedFeature,
            UpdateAvailable,
            UpdateProgress,
            Undefined
        }

        public abstract Pages PageType { get; }
        public virtual void OnPageShown(){}
        public virtual void OnPageHidden(){}
    }
}