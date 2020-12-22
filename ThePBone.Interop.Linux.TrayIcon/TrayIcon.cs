using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Interop.TrayIcon;
using Gtk;
using Serilog;
using Action = System.Action;
using Thread = System.Threading.Thread;

namespace ThePBone.Interop.Linux.TrayIcon
{
    public class TrayIcon : ITrayIcon, IDisposable
    {
        #region Properties
        public event EventHandler<TrayMenuItem>? TrayMenuItemSelected;
        public event EventHandler? LeftClicked;
        public event EventHandler? RightClicked;
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
        #endregion

        #region Thread-owned objects
        private Menu? _popupMenu;
        private Application _app;
        private StatusIcon _status;
        private List<TrayMenuItem> _menuItems = new List<TrayMenuItem>();
        #endregion

        #region Threading objects
        private readonly Thread thread;
        private SynchronizationContext? ctx;
        #endregion
        
        public TrayIcon()
        {

           /* 
            new Task(() =>
            {
                ctx = SynchronizationContext.Current;
               
                Application.Init();
                
                _app = new Application("GalaxyBudsClient.ThePBone.Interop.Linux.TrayIcon", GLib.ApplicationFlags.None);
                _app.Register(GLib.Cancellable.Current);

                _status = new StatusIcon();//new Gdk.Pixbuf("ScreenlapseIcon.png"));
                
                _status.Activate += (sender, args) => LeftClicked?.Invoke(this, EventArgs.Empty);
                _status.PopupMenu += (o, menuArgs) =>
                {
                    RightClicked?.Invoke(this, EventArgs.Empty);
                    _popupMenu?.ShowAll ();
                    _popupMenu?.Popup ();
                };
                
                _popupMenu = new Menu ();

                ImageMenuItem explore = new ImageMenuItem ("Explore");
                _popupMenu.Add(explore);
                
                Application.Run(); 
            }).Start();*/
        }
        
        public void UpdateMenu()
        {
            /*Application.Instance.AsyncInvoke(() =>
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
                }
                
                if(_tray == null)
                {
                    Log.Warning("Linux.TrayIcon: _tray not yet initialized. Cannot update menu.");
                }
                else
                {
                    _tray.Menu = menu;
                }
            });*/
        }

        public void Dispose()
        {
            if (ctx != null)
            {
                ctx.Send((_) => Application.Quit(), null);
                ctx = null;
            }
        }
    }
}

