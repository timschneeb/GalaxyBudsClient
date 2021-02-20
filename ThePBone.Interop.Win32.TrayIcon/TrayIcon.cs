using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GalaxyBudsClient.Interop.TrayIcon;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace ThePBone.Interop.Win32.TrayIcon
{
    public class TrayIcon : ITrayIcon
    {
        #region Properties
        public event EventHandler<TrayMenuItem> TrayMenuItemSelected;
        public event EventHandler LeftClicked;
        public event EventHandler RightClicked;

        public List<TrayMenuItem> MenuItems
        {
            set
            {
                _menuItems = value;
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (_win != null)
                    {
                        _win.MenuItems = _menuItems;
                    }
                });
            }
            get => _menuItems;
        }

        public bool PreferDarkMode
        {
            set
            {
                _preferDarkMode = value;
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (_win != null)
                    {
                        _win.PreferDarkMode = value;
                    }
                });
            }
            get => _preferDarkMode;
        }

        #endregion

        #region Threading
        public void Cleanup()
        {
            ResourceLoader.ClearCache();
        }
        
        public bool BeginInvoke(Delegate dlg, params object[] args)
        {
            if (_ctx == null)
            {
                return false;
            }
            _ctx.Post((_) => dlg.DynamicInvoke(args), null);
            return true;
        }

        public object Invoke(Delegate dlg, params object[] args)
        {
            if (_ctx == null)
            {
                return null;
            }
            object result = null;
            _ctx.Send((_) => result = dlg.DynamicInvoke(args), null);
            return result;
        }
        #endregion

        #region STA-owned objects
        private Application _application;
        private ContainerWindow _win;
        #endregion

        #region Threading objects
        private readonly Thread _thread;
        private SynchronizationContext _ctx;
        private bool _preferDarkMode;
        private List<TrayMenuItem> _menuItems;
        #endregion

        #region Initialization
        public TrayIcon()
        {
            var parentCtx = this;
                _thread = new Thread(() =>
            {
                _ctx = SynchronizationContext.Current;
                
                if (Application.Current == null)
                {
                    _application = new Application();
                }

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _win = new ContainerWindow
                    {
                        ShowInTaskbar = false,
                        Visibility = Visibility.Hidden,
                        WindowState = WindowState.Minimized,
                        PreferDarkMode = PreferDarkMode,
                        MenuItems = MenuItems,
                        ParentClass = parentCtx
                    };

                    Application.Current.Run();
                });
            })
            {
                IsBackground = true
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }
        #endregion
        
        private class ContainerWindow : Window
        {
            public readonly TaskbarIcon TaskbarIcon;

            private bool? _lastRedrawDark = null;
            public TrayIcon ParentClass = null;

            private List<TrayMenuItem> _menuItems = new List<TrayMenuItem>();
            public List<TrayMenuItem> MenuItems
            {
                set
                {
                    _menuItems = value;
                    CreateMenu();
                }
                get => _menuItems;
            }

            private bool _preferDarkMode = false;

            public bool PreferDarkMode
            {
                set
                {
                    _preferDarkMode = value;
                    CreateMenu();
                }
                get => _preferDarkMode;
            }

            public ContainerWindow()
            {
                TaskbarIcon = new TaskbarIcon();

                var iconResource = Assembly.GetEntryAssembly()
                    ?.GetManifestResourceStream("GalaxyBudsClient.Resources.icon_white.ico");
                if (iconResource != null)
                {
                    TaskbarIcon.Icon = new Icon(iconResource);
                }
                TaskbarIcon.ToolTipText = "Galaxy Buds Manager";
                TaskbarIcon.TrayLeftMouseUp += (sender, args) => ParentClass.LeftClicked?.Invoke(this, EventArgs.Empty);
                TaskbarIcon.TrayRightMouseDown += (sender, args) => ParentClass.RightClicked?.Invoke(this, EventArgs.Empty);
                ReloadStyles(false);
                CreateMenu();
            }

            private static void ReloadStyles(bool dark)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(
                    ResourceLoader.LoadXamlFromManifest<ResourceDictionary>("ThePBone.Interop.Win32.TrayIcon.Styles.Styles.xaml"));
                Application.Current.Resources.MergedDictionaries.Add(
                    ResourceLoader.LoadXamlFromManifest<ResourceDictionary>($"ThePBone.Interop.Win32.TrayIcon.Styles.Brushes{(dark ? "Dark" : string.Empty)}.xaml"));
            }

            private void CreateMenu()
            {    
                if (MenuItems == null)
                {
                    return;
                }
                
                if (_lastRedrawDark != PreferDarkMode)
                {
                    ReloadStyles(PreferDarkMode);
                }

                ContextMenu ctxMenu = new ContextMenu
                {
                    Style = ResourceLoader.FindResource<Style>("SmallContextMenuStyle")
                };

                foreach (var item in MenuItems)
                {
                    if (item.IsSeparator)
                    {
                        // TODO: Fix style
                        // ctxMenu.Items.Add(new Separator());
                    }
                    else
                    {
                        var i = new MenuItem
                        {
                            Header = item.Title,
                            IsEnabled = item.IsEnabled,
                            Style = ResourceLoader.FindResource<Style>(item.IsEnabled
                                ? "SmallMenuItemStyle"
                                : "SmallMenuItemStyle-Static")
                        };
                        i.Click += (sender, args) => ParentClass.TrayMenuItemSelected?.Invoke(this, item);
                        ctxMenu.Items.Add(i);
                    }

                }
                
                TaskbarIcon.ContextMenu = ctxMenu;
                _lastRedrawDark = PreferDarkMode;
            }
        }

        public void Dispose()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _win?.TaskbarIcon?.Dispose();
            });
        }
    }
}
