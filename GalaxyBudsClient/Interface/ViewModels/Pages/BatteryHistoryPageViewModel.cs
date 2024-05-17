using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using GalaxyBudsClient.Generated.Enums;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Microsoft.EntityFrameworkCore;
using ReactiveUI.Fody.Helpers;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class BatteryHistoryPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new BatteryHistoryPage { DataContext = this };
    public override string TitleKey => Keys.SystemBatteryStatistics;
    public IPlotControl? PlotControl { set; get; }
    public Plot? Plot => PlotControl?.Plot;
    public Rectangle? SelectionRect { set; get; }
    public Annotation? MeasureAnnotation { set; get; }
    
    [Reactive] public bool IsPlotLoading { set; get; }
    [Reactive] public Cursor PlotCursor { set; get; } = new(StandardCursorType.Arrow);
    [Reactive] public bool IsLegendVisible { set; get; } = true;
    [Reactive] public bool IsNoDataHintVisible { set; get; }

    [Reactive] public BatteryHistoryOverlays SelectedOverlay { set; get; } = BatteryHistoryOverlays.None;
    [Reactive] public BatteryHistoryTimeSpans SelectedTimeSpan { set; get; } = BatteryHistoryTimeSpans.Last12Hours;
    [Reactive] public BatteryHistoryTools SelectedTool { set; get; } = BatteryHistoryTools.PanAndZoom;

    // TODO show warning if not enough data is available
    public BatteryHistoryPageViewModel()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedTimeSpan):
            case nameof(SelectedOverlay):
                Task.Run(UpdatePlotAsync);
                break;
            case nameof(SelectedTool):
                PlotCursor = SelectedTool switch
                {
                    BatteryHistoryTools.PanAndZoom => new Cursor(StandardCursorType.Arrow),
                    BatteryHistoryTools.MeasureTime => new Cursor(StandardCursorType.RightSide),
                    BatteryHistoryTools.MeasureBattery => new Cursor(StandardCursorType.TopSide),
                    _ => throw new ArgumentOutOfRangeException()
                };
                if (MeasureAnnotation != null) 
                    MeasureAnnotation.IsVisible = false;
                break;
            case nameof(IsLegendVisible):
                UpdateLegendVisibility();
                break;
        }
    }
    
    public void DoReloadCommand() => Task.Run(UpdatePlotAsync);

    public override void OnNavigatedTo()
    {
        Task.Run(UpdatePlotAsync);
    }
    
    private void UpdateLegendVisibility()
    {
        if(Plot == null)
            return;
        
        Plot.Legend.IsVisible = IsLegendVisible;
        Plot.Legend.Alignment = Alignment.LowerLeft;
        PlotControl?.Refresh();
    } 
    
    private async Task UpdatePlotAsync()
    {
        if(Plot == null)
            return;
        
        IsPlotLoading = true;
        IsNoDataHintVisible = false;
        
        Plot.Clear();

        Plot.Add.Palette = new ScottPlot.Palettes.Category10();
        
        var timeLimit = TimeSpan.FromHours((int)SelectedTimeSpan);
        
        await using var disposableQuery = await BatteryHistoryManager.BeginDisposableQueryAsync();
        var cutOffDate = DateTime.Now - timeLimit;
        
        var query = disposableQuery.Queryable
            .Where(record => record.Timestamp > cutOffDate);

        var batteryL = new List<float>();
        var batteryR = new List<float>();
        var timestamp = new List<double>();
        
        IOverlay? overlay = SelectedOverlay switch
        {
            BatteryHistoryOverlays.None => null,
            BatteryHistoryOverlays.HostDevice => new HorizontalBarOverlay<DevicesInverted>(),
            BatteryHistoryOverlays.NoiseControls => new HorizontalBarOverlay<NoiseControlModes>(),
            BatteryHistoryOverlays.Wearing => new HorizontalBarOverlay<LegacyWearStates>(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var nonNullRecordCount = 0;
        await foreach (var record in query.AsAsyncEnumerable())
        {
            var date = record.Timestamp.ToOADate();

            batteryL.Add(record.BatteryL > 0 ? record.BatteryL ?? float.NaN : float.NaN);
            batteryR.Add(record.BatteryR > 0 ? record.BatteryR ?? float.NaN : float.NaN);
            timestamp.Add(date);

            if (record.BatteryL == null && record.BatteryR == null)
            {
                overlay?.AddNullFrame(date);
            }
            else
            {
                nonNullRecordCount++;
            }
            
            switch (overlay)
            {
                case HorizontalBarOverlay<DevicesInverted> singleBarOverlay:
                    singleBarOverlay.Add(date, record.HostDevice);
                    break;
                case HorizontalBarOverlay<NoiseControlModes> singleBarOverlay:
                    singleBarOverlay.Add(date, record.NoiseControlMode);
                    break;
                case HorizontalBarOverlay<LegacyWearStates> singleBarOverlay:
                    if(record is { PlacementL: PlacementStates.Wearing, PlacementR: PlacementStates.Wearing })
                        singleBarOverlay.Add(date, LegacyWearStates.Both);
                    else if(record.PlacementL == PlacementStates.Wearing)
                        singleBarOverlay.Add(date, LegacyWearStates.L);
                    else if(record.PlacementR == PlacementStates.Wearing)
                        singleBarOverlay.Add(date, LegacyWearStates.R);
                    else
                        singleBarOverlay.Add(date, null);
                    break;
            }
        }
        
        overlay?.AddNullFrame(DateTimeOffset.Now.DateTime.ToOADate());
        
        var plotBatteryL = Plot.Add.Scatter(timestamp, batteryL);
        plotBatteryL.MarkerShape = MarkerShape.None;
        plotBatteryL.LineWidth = 2;
        plotBatteryL.LegendText = Strings.Left;

        var plotBatteryR = Plot.Add.Scatter(timestamp, batteryR);
        plotBatteryR.MarkerShape = MarkerShape.None;
        plotBatteryR.LineWidth = 2;
        plotBatteryR.LegendText = Strings.Right;

        overlay?.ApplyOverlay(Plot);

        MeasureAnnotation = Plot.Add.Annotation("");
        MeasureAnnotation.IsVisible = false;
        
        SelectionRect = Plot.Add.Rectangle(0, 0, 0, 0);
        SelectionRect.FillStyle = new FillStyle { Color = Colors.Blue.WithAlpha(.2) };
        SelectionRect.LineStyle = new LineStyle { Color = Colors.Blue };
        
        Plot.Axes.Left.TickGenerator = new NumericAutomatic
        {
            LabelFormatter = value => value is < 0 or > 100 ? string.Empty : NumericAutomatic.DefaultLabelFormatter(value)
        };
        
        Plot.YLabel(Strings.BattHistYAxis);
        UpdateLegendVisibility();
        
        Plot.Axes.DateTimeTicksBottom();
        
        Plot.Axes.SetLimits((DateTimeOffset.Now - timeLimit).DateTime.ToOADate(), DateTimeOffset.Now.DateTime.ToOADate(), 0, 105);
        
        Plot.Axes.Rules.Clear();
        Plot.Axes.Rules.Add(new MaximumBoundary(Plot.Axes.Bottom, Plot.Axes.Left, new AxisLimits(
            (DateTimeOffset.Now - timeLimit).DateTime.ToOADate(), 
            DateTimeOffset.Now.DateTime.ToOADate(), 0, 105)));
        
        IsNoDataHintVisible = nonNullRecordCount <= 10;
        IsPlotLoading = false;
    }
    
    private class HorizontalBarOverlay<T> : IOverlay where T : struct
    {
        private record Region(double Timestamp, double TimestampEnd, T Value);
        
        private readonly List<Region> _regions = [];
        private T? _previousValue;
        private Region? _currentRegion;
        
        public void ApplyOverlay(Plot plot)
        {
            // Clean up the last region if it's still open
            if (_currentRegion != null)
            {
                _regions.Add(_currentRegion);
                _currentRegion = null;
            }
            
            var legendItems = new Dictionary<string, (LineStyle Line, FillStyle Fill)>();
            
            foreach (var region in _regions)
            {
                var description = CompiledEnums.GetDescriptionByType(typeof(T), region.Value);
                
                var rect = plot.Add.Rectangle(region.Timestamp, region.TimestampEnd, 0, 100);
                
                rect.FillColor = rect.FillColor.WithAlpha(.4);
                rect.LineColor = rect.LineColor.WithAlpha(.2);
                
                if (!legendItems.TryGetValue(description, out var value))
                {
                    legendItems.Add(description, (rect.LineStyle, rect.FillStyle));
                    rect.LegendText = string.Format(Strings.BattHistOverlayLegend, description);
                }
                else
                {
                    rect.LineStyle = value.Line;
                    rect.FillStyle = value.Fill;
                }
            }
        }

        public void AddNullFrame(double timestamp) => Add(timestamp, null);

        public void Add(double timestamp, T? value)
        {
            if (_currentRegion == null)
            {
                if(value != null)
                    _currentRegion = new Region(timestamp, timestamp, value.Value);
            }
            else if(!Nullable.Equals(value, _previousValue))
            {
                _currentRegion = _currentRegion with { TimestampEnd = timestamp };
                _regions.Add(_currentRegion);
                
                _currentRegion = null;
                Add(timestamp, value);
            }
            
            _previousValue = value;
        }
    }
    
    private interface IOverlay
    {
        public void ApplyOverlay(Plot plot);
        public void AddNullFrame(double timestamp);
    }
}


