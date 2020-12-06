using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace GalaxyBudsClient.Interface.Elements
{
    public class ToggleSwitch : UserControl
    {
        public static readonly StyledProperty<Color> ColorOnProperty =
            AvaloniaProperty.Register<ToggleSwitch, Color>(nameof(ColorOn), Colors.Orange);
        
        public static readonly StyledProperty<Color> ColorOffProperty =
            AvaloniaProperty.Register<ToggleSwitch, Color>(nameof(ColorOff), Color.FromRgb(160, 160, 160));

        public static readonly StyledProperty<bool> DisableProperty =
            AvaloniaProperty.Register<ToggleSwitch, bool>(nameof(ColorOff), false);

        public Color ColorOn
        {
            get => GetValue(ColorOnProperty);
            set
            {
                SetValue(ColorOnProperty, value);
                if (_isChecked)
                {
                    _back.Background = new SolidColorBrush(value);
                }
            }
        }

        public Color ColorOff
        {
            get => GetValue(ColorOffProperty);
            set
            {
                SetValue(ColorOffProperty, value);
                if (!_isChecked)
                {
                    _back.Background = new SolidColorBrush(value);
                }
            }
        }

        public bool DisableButton
        {
            get => GetValue(DisableProperty);
            set => SetValue(DisableProperty, value);
        }

        public event EventHandler<bool>? CheckedChanged;

        private readonly Border _back;
        private readonly Ellipse _dot;
        private readonly Animation _marginAnimation;
        private bool _isChecked;

        public ToggleSwitch()
        {
            AvaloniaXamlLoader.Load(this);
            
            _back = this.FindControl<Border>("Back");
            _dot = this.FindControl<Ellipse>("Dot");
            
            _isChecked = false;
            
            _marginAnimation = new Animation
            {
                Duration = TimeSpan.Parse("0:0:.04"),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Layoutable.MarginProperty,
                                Value = new Thickness(-39, 0, 0, 0)
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Layoutable.MarginProperty,
                                Value = new Thickness(39, 0, 0, 0)
                            }
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
            
            _dot.Margin = new Thickness(-39, 0, 0, 0);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetChecked(value);
        }

        public void Toggle()
        {
            _isChecked = !_isChecked;
            
            _back.Background = _isChecked ? new SolidColorBrush(ColorOn) : new SolidColorBrush(ColorOff);

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (_isChecked)
                {
                    _marginAnimation.PlaybackDirection = PlaybackDirection.Normal;
                    await _marginAnimation.RunAsync(_dot);
                    _dot.Margin = new Thickness(39, 0, 0, 0);
                }
                else
                {
                    _marginAnimation.PlaybackDirection = PlaybackDirection.Reverse;
                    await _marginAnimation.RunAsync(_dot);
                    _dot.Margin = new Thickness(-39, 0, 0, 0);
                }
            });
        }

        public void SetChecked(bool check)
        {
            if (check == _isChecked)
                return;

            _isChecked = !check;
            Toggle();
        }

        private void Dot_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DisableButton)
                return;
            
            Toggle();
            CheckedChanged?.Invoke(this, _isChecked);
        }

        private void Back_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DisableButton)
                return;

            Toggle();
            CheckedChanged?.Invoke(this, _isChecked);
        }
    }
}
