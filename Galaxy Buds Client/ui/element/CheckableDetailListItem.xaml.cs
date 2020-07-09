using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Galaxy_Buds_Client.ui.element
{
    /// <summary>
    /// Interaction logic for DetailListItem.xaml
    /// </summary>
    public partial class CheckableDetailListItem : UserControl
    {
        public static readonly DependencyProperty TextDependencyProperty
            = DependencyProperty.Register(
                "Text",
                typeof(String),
                typeof(CheckableDetailListItem)
            );
        public static readonly DependencyProperty TextDetailDependencyProperty
            = DependencyProperty.Register(
                "TextDetail",
                typeof(String),
                typeof(CheckableDetailListItem)
            );

        public event EventHandler<bool> SwitchToggled;

        public ToggleSwitch Switch => Toggle;

        public CheckableDetailListItem()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public String Text
        {
            get { return (String)GetValue(TextDependencyProperty); }
            set { SetValue(TextDependencyProperty, value); }
        }

        public String TextDetail
        {
            get { return (String)GetValue(TextDetailDependencyProperty); }
            set { SetValue(TextDetailDependencyProperty, value); }
        }

        private void OnBorderClicked(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Switch.Toggle();
            SwitchToggled?.Invoke(this, Switch.IsChecked);
        }
    }
}
