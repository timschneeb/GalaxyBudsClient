using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.StyledWindow;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Extensions;
using Application = Avalonia.Application;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class BudsPopup : Window
{
    public EventHandler? ClickedEventHandler { get; set; }
    
    private readonly Timer _timer = new(3000){ AutoReset = false };
        
    public BudsPopup() 
    {
        InitializeComponent();
        
        var cachedStatus = DeviceMessageCache.Instance.BasicStatusUpdate;
        if(cachedStatus != null)
            UpdateContent(cachedStatus);
            
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
        SppMessageHandler.Instance.BaseUpdate += InstanceOnBaseUpdate;
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Hide, DispatcherPriority.Render);
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Instance.Theme) or nameof(Settings.Instance.BlurStrength))
        {
            RequestedThemeVariant = IStyledWindow.GetThemeVariant();
        }
    }

    private void InstanceOnBaseUpdate(object? sender, IBasicStatusUpdate e)
    {
        UpdateContent(e);
    }

    public void RearmTimer()
    {
        _timer.Stop();
        _timer.Start();
    }
        
    private void UpdateContent(IBasicStatusUpdate e)
    {
        var bl = e.BatteryL;
        var br = e.BatteryR;
        var bc = e.BatteryCase;

        BatteryL.Content = $"{bl}%"; 
        BatteryR.Content = $"{br}%";
        BatteryC.Content = $"{bc}%";
            
        var connected = BluetoothService.Instance.IsConnected;
        var isLeftOnline = connected && bl > 0 && e.PlacementL != PlacementStates.Disconnected;
        var isRightOnline = connected && br > 0 && e.PlacementR != PlacementStates.Disconnected;
        var isCaseOnline = connected && bc is > 0 and <= 100 && BluetoothService.Instance.DeviceSpec.Supports(Features.CaseBattery);
            
        BatteryL.IsVisible = isLeftOnline;
        BatteryR.IsVisible = isRightOnline;
        BatteryC.IsVisible = isCaseOnline;
        CaseLabel.IsVisible = isCaseOnline;
     
        var type = BluetoothService.Instance.DeviceSpec.IconResourceKey;
        ImageLeft.Source = (IImage?)Application.Current?.FindResource($"Left{type}Connected");
        ImageRight.Source = (IImage?)Application.Current?.FindResource($"Right{type}Connected");
    }

    public override void Hide()
    {  
        /* Close window instead */
        _timer.Stop();
        
        OuterBorder.Tag = "hiding";
        Task.Delay(1000).ContinueWith(_ => { Dispatcher.UIThread.InvokeAsync(Close); });
    }

    public override void Show()
    {
        base.Show();
            
        UpdateSettings();
        OuterBorder.Tag = "showing";
            
        _timer.Start();
    }

    protected override void OnOpened(EventArgs e)
    {
        UpdateSettings();
        base.OnOpened(e);
    }

    private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ClickedEventHandler?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    public void UpdateSettings()
    {
        RequestedThemeVariant = IStyledWindow.GetThemeVariant();
        Header.Content = BluetoothService.Instance.DeviceName;

        /* Header */
        if (Settings.Instance.Popup.Compact)
        {
            MaxHeight = 205 - 35;
            Height = 205 - 35;
            Grid.RowDefinitions[1].Height = new GridLength(0);
        }
        else
        {
            MaxHeight = 205;
            Height = 205;
            Grid.RowDefinitions[1].Height = new GridLength(35);
        }
            
        /* Window positioning */
        var workArea = (Screens.Primary ?? Screens.All[0]).WorkingArea;
        var scaling = PlatformImpl?.GetPropertyValue<double>("DesktopScaling") ?? 1.0;
        
        var padding = (int)(20 * scaling);

        Position = Settings.Instance.Popup.Placement switch
        {
            PopupPlacement.TopLeft => new PixelPoint(workArea.X + padding, workArea.Y + padding),
            PopupPlacement.TopCenter => new PixelPoint(
                (int)(workArea.Width / 2f - Width * scaling / 2 + workArea.X), workArea.Y + padding),
            PopupPlacement.TopRight => new PixelPoint(
                (int)(workArea.Width - Width * scaling + workArea.X - padding), workArea.Y + padding),
            PopupPlacement.BottomLeft => new PixelPoint(workArea.X + padding,
                (int)(workArea.Height - Height * scaling + workArea.Y - padding)),
            PopupPlacement.BottomCenter => new PixelPoint(
                (int)(workArea.Width / 2f - Width * scaling / 2 + workArea.X),
                (int)(workArea.Height - Height * scaling + workArea.Y - padding)),
            PopupPlacement.BottomRight => new PixelPoint(
                (int)(workArea.Width - Width * scaling + workArea.X - padding),
                (int)(workArea.Height - Height * scaling + workArea.Y - padding)),
            _ => Position
        };
    }
}