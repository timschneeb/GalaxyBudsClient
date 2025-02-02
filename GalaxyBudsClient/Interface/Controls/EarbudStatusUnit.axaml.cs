using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.ViewModels.Controls;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Controls;

public partial class EarbudStatusUnit : Panel
{
    public EarbudStatusUnit()
    {
        InitializeComponent();
        var vm = new EarbudStatusUnitViewModel();
        vm.Changed.Subscribe(new Action<object>(_ =>
        {
            // FIXME: Avalonia bug: After upgrading to Avalonia 11.2.2 from 11.2.0-beta, the label bounds are initially too small
            Dispatcher.UIThread.Post(() =>
            {
                LeftBatteryText.InvalidateMeasure();
                RightBatteryText.InvalidateMeasure();
            });
        }));

        DataContext = vm;
    }

    private void OnTemperaturePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Settings.Data.TemperatureUnit = Settings.Data.TemperatureUnit == TemperatureUnits.Celsius
                ? TemperatureUnits.Fahrenheit : TemperatureUnits.Celsius;
        });
    }
}
