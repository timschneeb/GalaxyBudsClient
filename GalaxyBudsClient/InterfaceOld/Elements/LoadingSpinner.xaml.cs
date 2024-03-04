using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Elements
{
    public class LoadingSpinner : UserControl
    {
        public LoadingSpinner()
        {
            AvaloniaXamlLoader.Load(this);

            PropertyChanged += (o, e) =>
            {
                if (e.Property.Name != nameof(IsVisible) || e.NewValue == null) 
                    return;
                this.GetControl<Image>("Spinner").IsVisible = ((bool)e.NewValue);
            };
        }
    }
}
