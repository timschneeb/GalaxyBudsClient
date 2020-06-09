using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Galaxy_Buds_Client.ui.element
{

    public partial class MuteButton : UserControl
    {
        public bool IsMuted { get; set; }

        public event EventHandler<bool> StatusChanged;
        public MuteButton()
        {
            InitializeComponent();

            UpdateStatus(false);
        }

        private void UpdateStatus(bool b)
        {
            if (b)
            {
                Dispatcher.Invoke(() =>
                {
                    Ellipse.Fill = new SolidColorBrush(color: (Color)ColorConverter.ConvertFromString("#FFD0531B"));
                    CenterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/findmygear/muted.png"));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Ellipse.Fill = new SolidColorBrush(color: (Color)ColorConverter.ConvertFromString("#0054AE33"));
                    CenterImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/findmygear/unmuted.png"));
                });
            }
        }

        public void SetMuted(bool b)
        {
            IsMuted = b;
            UpdateStatus(IsMuted);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IsMuted = !IsMuted;
            UpdateStatus(IsMuted);
            StatusChanged?.Invoke(this, IsMuted);
        }
    }
}
