using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.ui.devmode;
using Galaxy_Buds_Client.util.DynamicLocalization;
using Sentry;

namespace Galaxy_Buds_Client
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;

        public App()
        {
            SentrySdk.Init(o =>
            {
                o.Dsn = new Dsn("https://4591394c5fd747b0ab7f5e81297c094d@o456940.ingest.sentry.io/5462682");
                o.MaxBreadcrumbs = 100;
#if DEBUG
                o.Environment = "staging";
#else
                o.Environment = "beta"; //"production";
#endif
                o.BeforeSend = sentryEvent =>
                {
                    sentryEvent.SetTag("bluetooth-mac", Galaxy_Buds_Client.Properties.Settings.Default.RegisteredDevice);
                    sentryEvent.SetTag("bluetooth-model", Galaxy_Buds_Client.Properties.Settings.Default.RegisteredDeviceModel.ToString());
                    sentryEvent.SetTag("sw-version", Galaxy_Buds_Client.Properties.Settings.Default.LastSwVersion);
                    
                    sentryEvent.SetExtra("bluetooth-mac", Galaxy_Buds_Client.Properties.Settings.Default.RegisteredDevice);
                    sentryEvent.SetExtra("bluetooth-model", Galaxy_Buds_Client.Properties.Settings.Default.RegisteredDeviceModel.ToString());
                    sentryEvent.SetExtra("bluetooth-connected", BluetoothService.Instance.IsConnected);
                    sentryEvent.SetExtra("custom-locale", Galaxy_Buds_Client.Properties.Settings.Default.CurrentLocale.GetDescription());
                    sentryEvent.SetExtra("sw-version", Galaxy_Buds_Client.Properties.Settings.Default.LastSwVersion);

                    if(_mainWindow != null)
                    {
                        sentryEvent.SetExtra("current-page", _mainWindow.PageControl.CurrentPageName);
                    }
                    else
                    {
                        sentryEvent.SetExtra("current-page", "MainWindow is NULL");
                    }
                    return sentryEvent;
                };
            });
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SingleInstanceWatcher();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (Loc.IsTranslatorModeEnabled())
            {
                Galaxy_Buds_Client.Properties.Settings.Default.CurrentLocale = Locale.custom;
                Galaxy_Buds_Client.Properties.Settings.Default.Save();
            }
            Loc.Load();

            bool startMinimized = false;
            bool startDevMode = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/StartMinimized")
                {
                    startMinimized = true;
                }

                if (e.Args[i] == "/DeveloperMode")
                {
                    startDevMode = true;
                }
            }

            MainWindow mainWindow = new MainWindow();
            if (!startMinimized)
            {
                mainWindow.Show();
            }

            if (startDevMode)
            {
                new DevWindow().Show();
            }

            _mainWindow = mainWindow;
        }
        
        private const string UniqueEventName = "{e6bc50e8-e3ed-487e-bbed-d57cc8ec76c1}";
        private EventWaitHandle eventWaitHandle;

        /// <summary>prevent a second instance and signal it to bring its mainwindow to foreground</summary>
        /// <seealso cref="https://stackoverflow.com/a/23730146/1644202"/>
        private void SingleInstanceWatcher()
        {
            // check if it is already open.
            try
            {
                // try to open it - if another instance is running, it will exist , if not it will throw
                this.eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                // Notify other instance so it could bring itself to foreground.
                this.eventWaitHandle.Set();

                // Terminate this instance.
                this.Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (this.eventWaitHandle.WaitOne())
                {
                    Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // could be set or removed anytime
                        if (!_mainWindow.Equals(null))
                        {
                            var mw = _mainWindow;

                            if (mw.WindowState == WindowState.Minimized || mw.Visibility != Visibility.Visible)
                            {
                                mw.Show();
                                mw.WindowState = WindowState.Normal;
                            }

                            // According to some sources these steps are required to be sure it went to foreground.
                            mw.Activate();
                            mw.Topmost = true;
                            mw.Topmost = false;
                            mw.Focus();
                        }
                    }));
                }
            })
            .Start();
        }
    }
}
