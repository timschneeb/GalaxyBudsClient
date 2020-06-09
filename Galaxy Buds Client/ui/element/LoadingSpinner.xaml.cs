using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Galaxy_Buds_Client.ui.element
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class LoadingSpinner : UserControl
    {

        public LoadingSpinner()
        {
            InitializeComponent();
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(0.35));
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = storyboard.Duration
            };
            rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTarget(rotateAnimation, Spinner);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            storyboard.Children.Add(rotateAnimation);

            Resources.Add("Storyboard", storyboard);
        }

        public void Start()
        {
            ((Storyboard)Resources["Storyboard"]).Begin();
        }

        public void Stop()
        {
            ((Storyboard)Resources["Storyboard"]).Stop();
        }
    }
}
