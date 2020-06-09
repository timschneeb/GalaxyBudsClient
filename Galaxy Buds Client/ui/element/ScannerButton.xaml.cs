using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Galaxy_Buds_Client.ui.element
{

    public partial class ScannerButton : UserControl
    {
        private bool _scanning = false;

        public event EventHandler<bool> ScanningStatusChanged;
        public ScannerButton()
        {
            InitializeComponent();
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(4));
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = storyboard.Duration
            };
            rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTarget(rotateAnimation, Radar);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            storyboard.Children.Add(rotateAnimation);

            Resources.Add("Storyboard", storyboard);

            UpdateStatus(false);
        }

        public void Start()
        {
            if (_scanning == true)
                return;

            UpdateStatus(true);
        }

        public void Stop()
        {
            if (_scanning == false)
                return;

            UpdateStatus(false);
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Grid mainGrid)
            {
                var cellHeight = mainGrid.RowDefinitions[Grid.GetRow(Ellipse)].ActualHeight;
                var cellWidth = mainGrid.ColumnDefinitions[Grid.GetColumn(Ellipse)].ActualWidth;
                var newSize = Math.Min(cellHeight, cellWidth);

                Ellipse.Height = newSize;
                Ellipse.Width = newSize;
            }
        }

        private void UpdateStatus(bool scanning)
        {
            if (scanning)
            {
                Dispatcher.Invoke(() =>
                {
                    Radar.Visibility = Visibility.Visible;
                    Ellipse.Fill = new SolidColorBrush(color: (Color)ColorConverter.ConvertFromString("#D0531B"));
                    CenterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/findmygear/stop_white.png"));
                    ((Storyboard)Resources["Storyboard"]).Begin();
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Radar.Visibility = Visibility.Hidden;
                    Ellipse.Fill = new SolidColorBrush(color: (Color)ColorConverter.ConvertFromString("#54AE33"));
                    CenterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/findmygear/start_white.png"));
                    ((Storyboard)Resources["Storyboard"]).Stop();
                });
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _scanning = !_scanning;
            UpdateStatus(_scanning);
            ScanningStatusChanged?.Invoke(this, _scanning);
        }
    }
}
