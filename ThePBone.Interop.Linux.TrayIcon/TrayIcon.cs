using System;
using System.Collections.Generic;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using GalaxyBudsClient.Interop.TrayIcon;
using Serilog;
using Action = System.Action;
using Application = Eto.Forms.Application;
using Thread = System.Threading.Thread;

namespace ThePBone.Interop.Linux.TrayIcon
{
    public class TrayIcon : ITrayIcon, IDisposable
    {
        #region Properties
        public event EventHandler<TrayMenuItem>? TrayMenuItemSelected;
        public event EventHandler? LeftClicked;
        public event EventHandler RightClicked;
        public List<TrayMenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                UpdateMenu();
            }
        }

        /* PreferDarkMode is ignored here */
        public bool PreferDarkMode { get; set; }

        public void Show()
        {
            BeginInvoke(new Action(() =>
            {
                _tray?.Show();
            }));
        }

        public void Hide()
        {
            BeginInvoke(new Action(() =>
            {
                _tray?.Hide();
            }));
        }
        #endregion

        #region Threading
        public void BeginInvoke(Delegate dlg, params object[] args)
        {
            if (ctx == null) throw new ObjectDisposedException("STAThread");
            ctx.Post((_) => dlg.DynamicInvoke(args), null);
        }

        public object? Invoke(Delegate dlg, params object[] args)
        {
            if (ctx == null) throw new ObjectDisposedException("STAThread");
            object? result = null;
            ctx.Send((_) => result = dlg.DynamicInvoke(args), null);
            return result;
        }

        protected virtual void Initialize(object? sender, EventArgs e)
        {
            ctx = SynchronizationContext.Current;
            mre.Set();
            Application.Instance.Initialized -= Initialize;
        }
        #endregion

        #region STA-owned objects
        private TrayIndicator? _tray;
        #endregion

        #region Threading objects
        private readonly Thread thread;
        private SynchronizationContext? ctx;
        private readonly ManualResetEvent mre;
        private List<TrayMenuItem> _menuItems = new List<TrayMenuItem>();
        #endregion

        public TrayIcon()
        {
            using (mre = new ManualResetEvent(false))
            {
                thread = new Thread(() =>
                {
                    var app = new Application();
                    app.Initialized += Initialize;

                    _tray = new TrayIndicator
                    {
                        Title = "Galaxy Buds Manager",
                        Image = Icon.FromResource("GalaxyBudsClient.Resources.icon_white_small.png")
                    };
                    _tray.Activated += (sender, args) => LeftClicked?.Invoke(this, EventArgs.Empty);
                    _tray.Show();
                    
                    UpdateMenu();
                    
                    app.Run();
                })
                {
                    IsBackground = true
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                mre.WaitOne();
            }
        }
        
        public void UpdateMenu()
        {
            Application.Instance.AsyncInvoke(() =>
            {
                var menu = new ContextMenu();

                foreach (var item in MenuItems)
                {
                    if (item.IsSeparator)
                    {
                        menu.Items.Add(new SeparatorMenuItem());
                    }
                    else
                    {
                        var entry = new ButtonMenuItem();
                        entry.Click += (sender, args) => TrayMenuItemSelected?.Invoke(this, item);
                        entry.Enabled = item.IsEnabled;
                        entry.Text = item.Title;
                        menu.Items.Add(entry);
                    }
;
                }
                
                if(_tray == null)
                {
                    Log.Warning("Linux.TrayIcon: _tray not yet initialized. Cannot update menu.");
                }
                else
                {
                    _tray.Menu = menu;
                }
            });
        }

        public void Dispose()
        {
            if (ctx != null)
            {
                ctx.Send((_) => Application.Instance.Quit(), null);
                ctx = null;
            }
        }
    }
}

