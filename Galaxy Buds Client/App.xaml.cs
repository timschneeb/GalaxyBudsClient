using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Galaxy_Buds_Client
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SingleInstanceWatcher();
        }

        private MainWindow _mainWindow;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            bool startMinimized = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/StartMinimized")
                {
                    startMinimized = true;
                }
            }

            MainWindow mainWindow = new MainWindow();
            if (!startMinimized)
            {
                mainWindow.Show();
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
