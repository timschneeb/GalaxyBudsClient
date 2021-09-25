
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
            TouchGesture,
            TouchCustomAction,
            AmbientSound,
            NoiseControlPro,
            NoiseControlProAmbient,
            Equalizer,
            BixbyRemap,
            System,
            SystemInfo,
            SystemCoredump,
            SpatialTest,
            Credits,
            SelfTest,
            FactoryReset,
            NoConnection,
            Welcome,
            DeviceSelect,
            Settings,
            SettingsPopup,
            SettingsCrowdsourcing,
            Advanced,
            Hotkeys,
            FirmwareSelect,
            FirmwareTransfer,
            UnsupportedFeature,
            UpdateAvailable,
            UpdateProgress,
            BudsAppDetected,
            Undefined
        }

        public abstract Pages PageType { get; }
        public virtual void OnPageShown(){}
        public virtual void OnPageHidden(){}
    }
}