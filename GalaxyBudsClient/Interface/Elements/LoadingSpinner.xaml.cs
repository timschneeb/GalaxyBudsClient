using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Elements
{
    public class LoadingSpinner : UserControl
    {
        public LoadingSpinner()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
