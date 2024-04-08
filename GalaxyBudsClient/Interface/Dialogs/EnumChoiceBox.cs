using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Interface.MarkupExtensions;

namespace GalaxyBudsClient.Interface.Dialogs;

public class EnumChoiceBox : ContentDialog
{
    private readonly ComboBox _comboBox;
    
    public EnumChoiceBox(MarkupExtension enumBindingSource)
    {
        PrimaryButtonText = Strings.Okay;
        CloseButtonText = Strings.Cancel;
        DefaultButton = ContentDialogButton.Primary;
        Content = _comboBox = new ComboBox
        {
            ItemsSource = (IEnumerable?)enumBindingSource.ProvideValue(null!),
            DisplayMemberBinding = new Binding(".")
            {
                Converter = new ModelNameConverter()
            },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    protected override Type StyleKeyOverride => typeof(ContentDialog);
    

    public async Task<object?> OpenDialogAsync(Window? host = null)
    {
        var result = await ShowAsync(host ?? MainWindow.Instance);
        var selection = _comboBox.SelectedItem;
        return result == ContentDialogResult.None ? default : selection;
    }
}