using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Interface.MarkupExtensions;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Dialogs;

public class EnumChoiceBox<T> : ContentDialog where T : struct
{
    private readonly ComboBox _comboBox;
    
    public EnumChoiceBox()
    {
        PrimaryButtonText = Strings.Okay;
        CloseButtonText = Strings.Cancel;
        DefaultButton = ContentDialogButton.Primary;
        Content = _comboBox = new ComboBox
        {
            ItemsSource = (IEnumerable?)new EnumBindingSource(typeof(T)).ProvideValue(null),
            DisplayMemberBinding = new Binding(".")
            {
                Converter = new ModelNameConverter()
            },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    protected override Type StyleKeyOverride => typeof(ContentDialog);
    

    public async Task<T?> OpenDialogAsync(Window? host = null)
    {
        var result = await ShowAsync(host ?? MainWindow.Instance);
        var selection = _comboBox.SelectedItem;
        return result == ContentDialogResult.None ? default : (T?)selection;
    }
}