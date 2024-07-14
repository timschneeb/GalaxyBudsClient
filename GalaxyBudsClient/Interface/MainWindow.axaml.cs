using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.StyledWindow;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;

namespace GalaxyBudsClient.Interface;

public partial class MainWindow : StyledAppWindow
{
   
    private bool _firstShow = true;

    private static App App => Application.Current as App ?? throw new InvalidOperationException();
        
    private static MainWindow? _instance;
    public static MainWindow Instance => _instance ??= new MainWindow();
    
    public MainWindow()
    {
        Log.Error(new StackTrace().ToString());
        
        InitializeComponent();
        this.AttachDevTools();
            
        BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
        Loc.LanguageUpdated += OnLanguageUpdated;
            
        if (App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon)
            {
                WindowState = WindowState.Minimized;
            }
        }
    }

    private void OnLanguageUpdated()
    {
        FlowDirection = Loc.ResolveFlowDirection();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (Settings.Data.MinimizeToTray && PlatformUtils.SupportsTrayIcon)
        {
            // check if the cause of the termination is due to shutdown or application close request
            if (e.CloseReason is not (WindowCloseReason.OSShutdown or WindowCloseReason.ApplicationShutdown))
            {
                BringToTray();
                e.Cancel = true;
                Log.Debug("MainWindow.OnClosing: Termination cancelled");
                base.OnClosing(e);
                return;
            }

            Log.Debug("MainWindow.OnClosing: closing event, continuing termination");
        }
        else
        {
            Log.Debug("MainWindow.OnClosing: Now closing session");
        }
            
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.FIND_MY_EARBUDS_STOP);
        await BluetoothImpl.Instance.DisconnectAsync();
        base.OnClosing(e);
    }
        
    protected override void OnOpened(EventArgs e)
    {
        if (_firstShow)
        {
            if (Settings.Data.OpenDevToolsOnStartup)
            {
                WindowLauncher.ShowDevTools(this);
            }
            
            HotkeyReceiverManager.Reset();
            HotkeyReceiverManager.Instance.Update(true);
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon && _firstShow)
            {
                Log.Debug("MainWindow: Launched minimized");
                BringToTray();
            }
        }

        if(_firstShow)
            Log.Information("Startup time: {Time}",  Stopwatch.GetElapsedTime(Program.StartedAt));
            
        _firstShow = false;
        base.OnOpened(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        BluetoothImpl.Instance.BluetoothError -= OnBluetoothError;
        Loc.LanguageUpdated -= OnLanguageUpdated;

        if(App.ApplicationLifetime is IControlledApplicationLifetime lifetime)
        {
            Log.Information("MainWindow: Shutting down normally");
            lifetime.Shutdown();
        }
        else
        {
            Log.Information("MainWindow: Shutting down using Environment.Exit");
            Environment.Exit(0);
        }
    }

    public void BringToFront()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {  
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
                
            if (PlatformUtils.IsLinux)
            {
                Hide(); // Workaround for some Linux DMs
            }

#if OSX
            GalaxyBudsClient.Platform.OSX.AppUtils.setHideInDock(false);
#endif
            Show();
                
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        });
    }

    public void ToggleVisibility()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!IsVisible)
            {
                BringToFront();
            }
            else
            {
                BringToTray();
            }
        });
    }
    
    private void BringToTray()
    {
#if OSX
        GalaxyBudsClient.Platform.OSX.AppUtils.setHideInDock(true);
#endif
        Hide();
    }

    private void OnBluetoothError(object? sender, BluetoothException e)
    {
        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            switch (e.ErrorCode)
            {
                case BluetoothException.ErrorCodes.NoAdaptersAvailable:
                    await new MessageBox
                    {
                        Title = Strings.Error,
                        Description = Strings.Nobluetoothdev
                    }.ShowAsync();
                    break;
            }
        });
    }
}