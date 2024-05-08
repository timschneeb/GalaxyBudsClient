using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using Microsoft.EntityFrameworkCore;
using ReactiveUI.Fody.Helpers;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Control;
using ScottPlot.TickGenerators;
using Serilog;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class BatteryHistoryPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new BatteryHistoryPage { DataContext = this };
    public override string TitleKey => Keys.SystemBatteryStatistics;
    public Plot? Plot { set; get; }
    
    [Reactive] public bool IsPlotLoading { set;get; }
    [Reactive] public BatteryHistoryTimeSpans SelectedTimeSpan { set; get; } = BatteryHistoryTimeSpans.Last3Days;

    public BatteryHistoryPageViewModel()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedTimeSpan):
                Task.Run(UpdatePlotAsync);
                break;
        }
    }

    public override void OnNavigatedTo()
    {
        Task.Run(UpdatePlotAsync);
    }

    private async Task UpdatePlotAsync()
    {
        if(Plot == null)
            return;
        
        IsPlotLoading = true;
        
        Plot.Clear();
        Plot.Add.Palette = new ScottPlot.Palettes.Nord();

        var timeLimit = TimeSpan.FromHours((int)SelectedTimeSpan);
        
        await using var disposableQuery = await BatteryHistoryManager.BeginDisposableQueryAsync();
        var cutOffDate = DateTime.Now - timeLimit;
        
        var query = disposableQuery.Queryable
            .Where(record => record.Timestamp > cutOffDate);

        var batteryL = new List<float>();
        var batteryR = new List<float>();
        var timestampL = new List<DateTime>();
        var timestampR = new List<DateTime>();

        await foreach (var record in query.AsAsyncEnumerable())
        {
            batteryL.Add(record.BatteryL > 0 ? record.BatteryL ?? float.NaN : float.NaN);
            timestampL.Add(record.Timestamp);
            batteryR.Add(record.BatteryR > 0 ? record.BatteryR ?? float.NaN : float.NaN);
            timestampR.Add(record.Timestamp);
        }
        
        var plotBatteryL = Plot.Add.Scatter(timestampL, batteryL);
        plotBatteryL.MarkerShape = MarkerShape.None;
        plotBatteryL.LegendText = Strings.Left;

        var plotBatteryR = Plot.Add.Scatter(timestampR, batteryR);
        plotBatteryR.MarkerShape = MarkerShape.None;
        plotBatteryR.LegendText = Strings.Right;
        
        /*Plot.Axes.Rules.Add(new MaximumBoundary(Plot.Axes.Bottom, Plot.Axes.Left, new AxisLimits(new CoordinateRect
        {
            Right = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Left = (DateTimeOffset.Now - timeLimit).ToUnixTimeMilliseconds(),
            Top = 105,
            Bottom = 0
        })));*/
        
        Plot.Axes.Left.TickGenerator = new NumericAutomatic
        {
            LabelFormatter = value => value is < 0 or > 100 ? string.Empty : NumericAutomatic.DefaultLabelFormatter(value), 
        };
        
        Plot.YLabel("Charge (%)");
        Plot.ShowLegend();
        
        Plot?.Axes.DateTimeTicksBottom();

        IsPlotLoading = false;
    }
}


