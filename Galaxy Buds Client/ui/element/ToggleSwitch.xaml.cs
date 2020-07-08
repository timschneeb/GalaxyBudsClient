using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Galaxy_Buds_Client.ui.element
{
    /// <summary>
    /// Interaction logic for ToggleSwitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        public static readonly DependencyProperty ColorOnDependencyProperty
            = DependencyProperty.Register(
                "ColorOn",
                typeof(SolidColorBrush),
                typeof(ToggleSwitch),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(130, 190, 125)), ColorOn_PropertyChangedCallback)
            );

        public static readonly DependencyProperty ColorOffDependencyProperty
            = DependencyProperty.Register(
                "ColorOff",
                typeof(SolidColorBrush),
                typeof(ToggleSwitch),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(160, 160, 160)), ColorOff_PropertyChangedCallback)
            );

        public static readonly DependencyProperty DisableButtonDependencyProperty
            = DependencyProperty.Register(
                "DisableButton",
                typeof(bool),
                typeof(ToggleSwitch),
                new PropertyMetadata(false)
            );

        private static void ColorOff_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ToggleSwitch originator = d as ToggleSwitch;
            if (originator != null)
            {
                if (!originator.IsChecked)
                    originator.Dispatcher.Invoke(() => { originator.Back.Fill = originator.ColorOff; });
            }
        }

        private static void ColorOn_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch originator = d as ToggleSwitch;
            if (originator != null)
            {
                if (originator.IsChecked)
                    originator.Dispatcher.Invoke(() => { originator.Back.Fill = originator.ColorOn; });
            }
        }


        public SolidColorBrush ColorOn
        {
            get { return (SolidColorBrush)GetValue(ColorOnDependencyProperty); }
            set
            {
                SetValue(ColorOnDependencyProperty, value);
                if (IsChecked)
                    Dispatcher.Invoke(() => { Back.Fill = ColorOn; });
            }
        }
        public SolidColorBrush ColorOff
        {
            get { return (SolidColorBrush)GetValue(ColorOffDependencyProperty); }
            set
            {
                SetValue(ColorOffDependencyProperty, value);
            }
        }
        public bool DisableButton
        {
            get { return (bool)GetValue(DisableButtonDependencyProperty); }
            set { SetValue(DisableButtonDependencyProperty, value); }
        }

        public event EventHandler<bool> CheckedChanged;

        public ToggleSwitch()
        {
            InitializeComponent();
            Back.Fill = ColorOff;
            IsChecked = false;
            Dot.Margin = new Thickness(-39, 0, 0, 0);

            NewStoryboard("StoryboardCheck", 0, 39);
            NewStoryboard("StoryboardUncheck", 39, 0);
        }

        public bool IsChecked { get; set; }

        private void NewStoryboard(String name, int from, int to)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(0.1));

            DoubleAnimation translateAnim = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = storyboard.Duration
            };

            Storyboard.SetTarget(translateAnim, Dot);
            Storyboard.SetTargetProperty(translateAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            storyboard.Children.Add(translateAnim);

            Resources.Add(name, storyboard);
        }

        private void PlayAnimation(bool state)
        {
            ((Storyboard)Resources["StoryboardCheck"]).Stop();
            ((Storyboard)Resources["StoryboardUncheck"]).Stop();
            ((Storyboard)Resources["StoryboardCheck"]).Seek(TimeSpan.Zero);
            ((Storyboard)Resources["StoryboardUncheck"]).Seek(TimeSpan.Zero);

            if (state)
            {
                ((Storyboard)Resources["StoryboardCheck"]).Begin();
            }
            else
            {
                ((Storyboard)Resources["StoryboardUncheck"]).Begin();
            }
        }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DisableButton)
                return;

            Toggle();
            CheckedChanged?.Invoke(this, IsChecked);
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DisableButton)
                return;

            Toggle();
            CheckedChanged?.Invoke(this, IsChecked);
        }

        public void Toggle()
        {
            IsChecked = !IsChecked;
            Dispatcher.Invoke(() =>
            {
                if (IsChecked)
                {
                    Back.Fill = ColorOn;
                    IsChecked = true;
                }
                else
                {
                    Back.Fill = ColorOff;
                    IsChecked = false;
                }
                PlayAnimation(IsChecked);
            });

        }

        public void SetChecked(bool check)
        {
            if (check == IsChecked)
                return;

            IsChecked = !check;
            Toggle();
        }
    }
}
