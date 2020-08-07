using System;
using System.Windows;
using System.Windows.Controls;

namespace Galaxy_Buds_Client.ui.element
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ListItemCenter : UserControl
    {
        public static readonly DependencyProperty SourceDependencyProperty
            = DependencyProperty.Register(
                "SourceImage",
                typeof(Uri),
                typeof(ListItemCenter)
                );
        public static readonly DependencyProperty TextDependencyProperty
            = DependencyProperty.Register(
                "Text",
                typeof(String),
                typeof(ListItemCenter)
            );
        public ListItemCenter()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public Uri SourceImage
        {
            get { return (Uri) GetValue(SourceDependencyProperty); }
            set
            { SetValue(SourceDependencyProperty, value); }
        }
        public String Text
        {
            get { return (String)GetValue(TextDependencyProperty); }
            set { SetValue(TextDependencyProperty, value); }
        }
    }
}
