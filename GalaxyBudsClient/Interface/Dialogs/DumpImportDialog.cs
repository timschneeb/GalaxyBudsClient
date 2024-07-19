using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Interface.MarkupExtensions;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Dialogs;

public class DumpImportDialog : ContentDialog
{
    private readonly ComboBox _comboBox;
    private readonly CheckBox _replayCheckBox;
    
    public record DumpImportDialogResult(Models Model, bool Replay);
    
    public DumpImportDialog()
    {
        Title = "Choose model for dump...";
        PrimaryButtonText = Strings.Okay;
        CloseButtonText = Strings.Cancel;
        DefaultButton = ContentDialogButton.Primary;
        
        _comboBox = new ComboBox
        {
            ItemsSource = (IEnumerable?)new ModelsBindingSource().ProvideValue(null!),
            DisplayMemberBinding = new CompiledBindingExtension(new CompiledBindingPathBuilder().TypeCast<object>().Build())
            {
                Converter = new ModelNameConverter()
            },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        _replayCheckBox = new CheckBox
        {
            Content = "Replay dump (instant)"
        };

        Content = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                _comboBox,
                _replayCheckBox
            }
        };
    }

    protected override Type StyleKeyOverride => typeof(ContentDialog);
    

    public async Task<DumpImportDialogResult?> OpenDialogAsync(TopLevel? host = null)
    {
        var result = await ShowAsync(host);
        return result == ContentDialogResult.None || 
               _comboBox.SelectedItem is not Models selection || 
               selection == Models.NULL ? null : 
            new DumpImportDialogResult(selection, _replayCheckBox.IsChecked ?? false);
    }
}