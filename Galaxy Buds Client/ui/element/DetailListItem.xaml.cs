using System;
using System.Windows;
using System.Windows.Controls;

namespace Galaxy_Buds_Client.ui.element
{
    /// <summary>
    /// Interaction logic for DetailListItem.xaml
    /// </summary>
    public partial class DetailListItem : UserControl
    {
        public static readonly DependencyProperty TextDependencyProperty
            = DependencyProperty.Register(
                "Text",
                typeof(String),
                typeof(DetailListItem)
            );
        public static readonly DependencyProperty TextDetailDependencyProperty
            = DependencyProperty.Register(
                "TextDetail",
                typeof(String),
                typeof(DetailListItem)
            );

        public DetailListItem()
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
    }
}
