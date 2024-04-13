using Avalonia.Controls;
using Avalonia.Input;
using GalaxyBudsClient.Interface.ViewModels.Controls;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;

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
        LegacySettings.Instance.TemperatureUnit = LegacySettings.Instance.TemperatureUnit == TemperatureUnits.Celsius
            ? TemperatureUnits.Fahrenheit : TemperatureUnits.Celsius;
    }
}