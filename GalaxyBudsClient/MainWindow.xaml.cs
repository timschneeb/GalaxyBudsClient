using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Pages;
using GalaxyBudsClient.InterfaceOld.Transition;
using Serilog;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient
{
    
    // Dummy class until all old pages are removed
    public sealed class MainWindow : Window
    {
        public readonly HomePage HomePage = new();
        public readonly CustomTouchActionPage CustomTouchActionPage = new();
        public readonly ConnectionLostPage ConnectionLostPage = new();
        public readonly UpdatePage UpdatePage = new();
        public readonly UpdateProgressPage UpdateProgressPage = new();
        public readonly DeviceSelectionPage DeviceSelectionPage = new();
        
        public PageContainer? Pager { set; get; }
        
        public static MainWindow Instance
        {
            get
            {
                Log.Fatal("LegacyMainWindow: Instance requested from {Ctx}", new StackTrace().ToString());
                throw new NotSupportedException();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void _()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
            
            Pager = this.GetControl<PageContainer>("Container");

            // Allocate essential pages immediately
            Pager.RegisterPages(HomePage, ConnectionLostPage, new WelcomePage());
            
            // Defer the rest of the page registration
            Dispatcher.UIThread.Post(() => Pager.RegisterPages(new FindMyGearPage(),
                new TouchpadPage(), new AdvancedPage(),
                CustomTouchActionPage, DeviceSelectionPage,
                UpdatePage, UpdateProgressPage, new HotkeyPage(), new BixbyRemapPage(),
                new BudsAppDetectedPage(), new TouchpadGesturePage(), 
                new GearFitPage()), DispatcherPriority.ApplicationIdle);
            
        }
    }
}