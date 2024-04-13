using Avalonia.Controls;
using Avalonia.Input;
using GalaxyBudsClient.Interface.ViewModels.Controls;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Controls;

public partial class EarbudStatusUnit : Panel
{
    public EarbudStatusUnit()
    {
        InitializeComponent();
        DataContext = new EarbudStatusUnitViewModel();
    }
        
    private void OnTemperaturePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Settings.Data.TemperatureUnit = Settings.Data.TemperatureUnit == TemperatureUnits.Celsius
            ? TemperatureUnits.Fahrenheit : TemperatureUnits.Celsius;
    }
}