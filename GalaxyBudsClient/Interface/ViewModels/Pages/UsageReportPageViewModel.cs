using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class UsageReportPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new UsageReportPage();
    public override string TitleKey => Keys.UsageReportsHeader;
    
    [Reactive] public string? MeteringDescription { private set; get; } = Strings.NotAvailable;
    public ObservableCollection<ItemViewHolder> MeteringItems { get; } = [];
    [Reactive] public string? UsageDescription { private set; get; } = Strings.NotAvailable;
    public ObservableCollection<ItemViewHolder> UsageItems { get; } = [];
    
    private Usage2ReportDecoder? _usageReport;
    private MeteringReportDecoder? _meteringReport;
    
    public UsageReportPageViewModel()
    {
        SppMessageReceiver.Instance.MeteringReportResponse += OnMeteringReportResponse;
        SppMessageReceiver.Instance.UsageReportResponse += OnUsageReportResponse;
        SppMessageReceiver.Instance.Usage2ReportResponse += OnUsage2ReportResponse;
        
        Loc.LanguageUpdated += OnLanguageUpdated;
        
        BluetoothImpl.Instance.Device.DeviceChanged += (_, _) =>
        {
            // Device changed, clear the data
            _usageReport = null;
            _meteringReport = null;
            MeteringItems.Clear();
            UsageItems.Clear();
        };
        
        MeteringItems.CollectionChanged += (_, _) =>
        {
            MeteringDescription = MeteringItems.Count > 0 ? string.Format(Strings.LastUpdatedAtN, DateTime.Now.ToShortTimeString()) : Strings.NotAvailable;
        };
        
        UsageItems.CollectionChanged += (_, _) =>
        {
            UsageDescription = UsageItems.Count > 0 ? string.Format(Strings.LastUpdatedAtN, DateTime.Now.ToShortTimeString()) : Strings.NotAvailable;
        };
    }

    private void OnLanguageUpdated()
    {
        if (_usageReport != null)
            ProcessUsageReport(_usageReport);
        if (_meteringReport != null)
            OnMeteringReportResponse(this, _meteringReport);
    }

    private void ProcessUsageReport(Usage2ReportDecoder report)
    {
        _usageReport = _usageReport == null ? report : report.Merge(_usageReport);
        
        UsageItems.Clear();
        foreach (var (key, value) in _usageReport.ToStringMap())
        {
            UsageItems.Add(new ItemViewHolder(key, value));
        }
    }

    private void OnUsage2ReportResponse(object? sender, Usage2ReportDecoder e) => ProcessUsageReport(e);
    private void OnUsageReportResponse(object? sender, UsageReportDecoder e) => ProcessUsageReport(new Usage2ReportDecoder(e));

    private void OnMeteringReportResponse(object? sender, MeteringReportDecoder e)
    {
        _meteringReport = e;
        
        MeteringItems.Clear();
        if(e.TotalBatteryCapacity != null)
            MeteringItems.Add(new ItemViewHolder(Strings.MeteringTotalBattCapacity, string.Format(Strings.MilliampHoursUnit, e.TotalBatteryCapacity.Value.ToString())));
        MeteringItems.Add(new ItemViewHolder(Strings.MeteringA2dpTime, FormatValue(e.A2dpUsingTimeL, e.A2dpUsingTimeR, AsTimeString)));
        MeteringItems.Add(new ItemViewHolder(Strings.MeteringEscoTime, FormatValue(e.EscoUsingTimeL, e.EscoUsingTimeR, AsTimeString)));
    }
    
    private static object? AsTimeString(int? time)
    {
        return time == null ? null : TimeSpan.FromMilliseconds(time.Value).ToString(@"hh\:mm\:ss");
    }
    
    private static string FormatValue<T>(T? left, T? right, Func<T?, object?>? formatter = null)
    {
        if (left == null && right == null)
            return Strings.NoDataStored;

        formatter ??= o => o;
        
        return string.Format(Strings.ValueLeftRightInline, 
            formatter(left) ?? Strings.PlacementDisconnected, 
            formatter(right) ?? Strings.PlacementDisconnected);
    }
}


