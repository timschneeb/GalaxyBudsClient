using Config.Net;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model
{
    public interface ICustomAction
    {
        CustomAction.Actions Action { set; get; }
        string Parameter { set; get; }
    }
    
    public interface IDevice
    {
        [Option(DefaultValue = Models.NULL)]
        Models Model { set; get; }
        string MacAddress { set; get; }
    }
    
    public interface ISettings
    {

        [Option(DefaultValue = DarkModes.Light)]
        DarkModes DarkMode { set; get; }
        
        [Option(DefaultValue = Locales.en)]
        Locales Locale { set; get; }
        
        [Option(DefaultValue = TemperatureUnits.Celsius)]
        TemperatureUnits TemperatureUnit { set; get; }
        
        IDevice RegisteredDevice { set; get; }
        
        bool MinimizeToTray { set; get; }
        
        ICustomAction CustomActionLeft { set; get; }
        ICustomAction CustomActionRight { set; get; }
        
        string UpdateSkippedVersion { set; get; }
        
    }
}