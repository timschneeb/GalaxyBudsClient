using System;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Extensions;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Control;
using Strings = GalaxyBudsClient.Generated.I18N.Strings;

namespace GalaxyBudsClient.Interface.Pages;

public partial class BatteryHistoryPage : BasePage<BatteryHistoryPageViewModel>
{
    private bool _mouseIsDown;
    private AxisLimits _axisLimitCache;
    private Coordinates _mouseDownCoordinates;
    private Coordinates _mouseNowCoordinates;
    private readonly PlotActions _customActions;
    private CoordinateRect MouseSelectionRect => new(_mouseDownCoordinates, _mouseNowCoordinates);
    
    public BatteryHistoryPage()
    {
        InitializeComponent();
        
        // Disable double-click benchmark action
        _customActions = PlotActions.Standard();
        _customActions.ToggleBenchmark = delegate { };
        PlotControl.Interaction.Enable(_customActions);
    }

    protected override void OnInitialized()
    {
        ViewModel!.PlotControl = PlotControl;
        base.OnInitialized();
    }

    private void OnHintClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        Settings.Data.IsBatteryHistoryHintHidden = true;
    }

    private void OnOverlayHelpItemClicked(object? sender, RoutedEventArgs e)
    {
        OverlayTip.IsOpen = true;
    }

    private void OnControlsHelpItemClicked(object? sender, RoutedEventArgs e)
    {
        ControlsTip.IsOpen = true;
    }

    private void OnToolsHelpItemClicked(object? sender, RoutedEventArgs e)
    {
        ToolsTip.IsOpen = true;
    }
    
    public void OnPlotPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseIsDown || ViewModel?.SelectedTool == BatteryHistoryTools.PanAndZoom)
            return;
        
        var pixel = e.ToPixel(PlotControl);
        switch (ViewModel?.SelectedTool)
        {
            case BatteryHistoryTools.MeasureTime:
                _mouseNowCoordinates = PlotControl.Plot.GetCoordinates(pixel.X, 0) with { Y = _axisLimitCache.Top };
                break;
            case BatteryHistoryTools.MeasureBattery:
                _mouseNowCoordinates = PlotControl.Plot.GetCoordinates(0, pixel.Y) with { X = _axisLimitCache.Right };
                break;
            default:
                return;
        }
        
        if (ViewModel?.SelectionRect is { } rect)
        {
            rect.CoordinateRect = MouseSelectionRect;
            rect.IsVisible = true;
        }
        
        if (ViewModel?.MeasureAnnotation is { } annotation)
        {
            var timeSpan = TimeSpan.FromTicks(DateTime.FromOADate(_mouseNowCoordinates.X).Ticks - DateTime.FromOADate(_mouseDownCoordinates.X).Ticks);
            annotation.Text = ViewModel.SelectedTool switch
            {
                BatteryHistoryTools.MeasureTime => 
                    string.Format(Strings.BattHistMeasureTimespanDisplay, timeSpan.ToString(timeSpan.Days > 0 ? Strings.BattHistMeasureTimespanUnitLong : "hh'h:'mm':'ss")),
                BatteryHistoryTools.MeasureBattery => 
                    string.Format(Strings.BattHistMeasureDifferenceDisplay, $"{_mouseNowCoordinates.Y - _mouseDownCoordinates.Y:N0}%"),
                _ => string.Empty
            };
            annotation.IsVisible = true;
        }

        PlotControl.Refresh();
    } 
    
    public void OnPlotPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if(ViewModel?.SelectedTool == BatteryHistoryTools.PanAndZoom)
            return;
        
        var pixel = e.ToPixel(PlotControl);
        _axisLimitCache = PlotControl.Plot.Axes.Rules.FirstOrDefault(r => r is MaximumBoundary) is MaximumBoundary maxBoundary
            ? maxBoundary.Limits
            : new AxisLimits();
        
        switch (ViewModel?.SelectedTool)
        {
            case BatteryHistoryTools.MeasureTime:
                _mouseDownCoordinates = PlotControl.Plot.GetCoordinates(pixel.X, 0) with { Y = _axisLimitCache.Bottom };
                break;
            case BatteryHistoryTools.MeasureBattery:
                _mouseDownCoordinates = PlotControl.Plot.GetCoordinates(0, pixel.Y) with { X = _axisLimitCache.Left };
                break;
            default:
                return;
        }
        
        _mouseIsDown = true;
        PlotControl.Interaction.Disable();
    } 
    
    public void OnPlotPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseIsDown = false;
        if (ViewModel?.SelectionRect is { } rect)
        {
            rect.IsVisible = false;
        }
        
        // Reset the mouse positions
        _mouseDownCoordinates = Coordinates.NaN;
        _mouseNowCoordinates = Coordinates.NaN;

        // Update the plot
        PlotControl.Refresh();
        PlotControl.Interaction.Enable(_customActions);
    }
}