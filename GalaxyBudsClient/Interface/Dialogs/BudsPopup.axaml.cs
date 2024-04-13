using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.StyledWindow;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class BudsPopup : Window
{
    public EventHandler? ClickedEventHandler { get; set; }
    
    private readonly Timer _timer = new(3000){ AutoReset = false };
     
    public BudsPopup() 
    {
        InitializeComponent();

        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Hide, DispatcherPriority.Render);
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Data.Theme) or nameof(Settings.Data.BlurStrength))
        {
            RequestedThemeVariant = IStyledWindow.GetThemeVariant();
        }
    }
    
    public void RearmTimer()
    {
        _timer.Stop();
        _timer.Start();
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
        Header.Content = BluetoothImpl.Instance.DeviceName;

        /* Header */
        if (Settings.Data.PopupCompact)
        {
            MaxHeight = Height = 205 - 35;
            Grid.RowDefinitions[0].Height = new GridLength(0);
        }
        else
        {
            MaxHeight = Height = 205;
            Grid.RowDefinitions[0].Height = new GridLength(35);
        }
            
        /* Window positioning */
        var workArea = (Screens.Primary ?? Screens.All[0]).WorkingArea;
        var scaling = PlatformImpl?.GetPropertyValue<double>("DesktopScaling") ?? 1.0;
        
        var padding = (int)(20 * scaling);

        Position = Settings.Data.PopupPlacement switch
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