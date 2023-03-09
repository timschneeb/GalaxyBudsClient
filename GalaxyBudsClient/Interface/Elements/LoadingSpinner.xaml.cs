using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Elements
{
    public class LoadingSpinner : UserControl
    {
        private readonly Image _spinnerImage;

        public LoadingSpinner()
        {
            AvaloniaXamlLoader.Load(this);

            _spinnerImage = this.FindControl<Image>("Spinner");

            PropertyChanged += (o, e) =>
            {
                if (e.Property.Name == nameof(IsVisible))
                {
                    if (e.NewValue != null)
                    {
                        if ((bool)e.NewValue)
                            _spinnerImage.IsVisible = true;
                        else
                            _spinnerImage.IsVisible = false;
                    }
                }
            };
        }
    }
}
