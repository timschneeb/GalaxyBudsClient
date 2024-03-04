using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace GalaxyBudsClient.InterfaceOld
{
    public class CustomTitleBar : UserControl
    {
        private readonly Button minimizeButton;
        private readonly Button closeButton;

        private readonly DockPanel titleBar;
        private readonly Control? titleBarBackground;
        private readonly TextBlock? systemChromeTitle;
        private readonly NativeMenuBar? seamlessMenuBar;
        private readonly NativeMenuBar? defaultMenuBar;

        public static readonly StyledProperty<bool> IsSeamlessProperty =
            AvaloniaProperty.Register<CustomTitleBar, bool>(nameof(IsSeamless));

        public bool IsSeamless
        {
            get { return GetValue(IsSeamlessProperty); }
            set
            {
                SetValue(IsSeamlessProperty, value);
                if (titleBarBackground != null && 
                    systemChromeTitle != null &&
                    seamlessMenuBar != null &&
                    defaultMenuBar != null)
                {
                    titleBarBackground.IsVisible = !IsSeamless;
                    systemChromeTitle.IsVisible = !IsSeamless;
                    seamlessMenuBar.IsVisible = IsSeamless;
                    defaultMenuBar.IsVisible = !IsSeamless;

                    if (IsSeamless == false)
                    {
                        titleBar.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
                    }
                }
            }
        }
        
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<CustomTitleBar, string>(nameof(Title));
        
        public event EventHandler? OptionsPressed;
        public event EventHandler? ClosePressed;
        
        public Button OptionsButton { set; get; }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public CustomTitleBar()
        {
            AvaloniaXamlLoader.Load(this);

            minimizeButton = this.GetControl<Button>("MinimizeButton");
            closeButton = this.GetControl<Button>("CloseButton");
            OptionsButton = this.GetControl<Button>("OptionsButton");
            
            minimizeButton.Click += MinimizeWindow;
            closeButton.Click += (sender, args) => ClosePressed?.Invoke(sender, EventArgs.Empty);
            OptionsButton.Click += (sender, args) => OptionsPressed?.Invoke(sender, EventArgs.Empty);

            titleBar = this.GetControl<DockPanel>("TitleBar");
            titleBarBackground = this.FindControl<Control>("TitleBarBackground");
            systemChromeTitle = this.FindControl<TextBlock>("SystemChromeTitle");
            seamlessMenuBar = this.FindControl<NativeMenuBar>("SeamlessMenuBar");
            defaultMenuBar = this.FindControl<NativeMenuBar>("DefaultMenuBar");
            
            SubscribeToWindowState();
        }
        
        private void MinimizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var hostWindow = (Window?)VisualRoot;
            if (hostWindow != null)
            {
                hostWindow.WindowState = WindowState.Minimized;
            }
        }

        private async void SubscribeToWindowState()
        {
            var hostWindow = (Window?)VisualRoot;

            while (hostWindow == null)
            {
                hostWindow = (Window?)VisualRoot;
                await Task.Delay(50);
            }
        }
    }
}
