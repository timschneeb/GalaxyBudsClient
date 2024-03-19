using System;
using System.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Pages;
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
        public readonly DeviceSelectionPage DeviceSelectionPage = new();

        public PageContainer Pager { set; get; } = new();
        
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

            // Allocate essential pages immediately
            Pager.RegisterPages(HomePage, ConnectionLostPage, new WelcomePage());
            
            // Defer the rest of the page registration
            Dispatcher.UIThread.Post(() => 
                    Pager.RegisterPages(new TouchpadPage(), CustomTouchActionPage, DeviceSelectionPage, new BudsAppDetectedPage()
                ), DispatcherPriority.ApplicationIdle);
        }
    }
    
    // Dummy class
    public class PageContainer
    {
        public void RegisterPages(params AbstractPage[] pages) {}
        public bool SwitchPage(AbstractPage.Pages page) => throw new NotSupportedException();
    }
}