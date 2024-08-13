using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;

namespace GalaxyBudsClient.Interface.Dialogs;

public class InputDialog : ContentDialog
{
    private readonly TextBox _textBox;

    public string? Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    public InputDialog(Control? headerControl = null)
    {
        PrimaryButtonText = Strings.Okay;
        CloseButtonText = Strings.Cancel;
        DefaultButton = ContentDialogButton.Primary;

        _textBox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        var panel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                _textBox,
            }
        };
        
        if(headerControl != null)
            panel.Children.Insert(0, headerControl);

        Content = panel;
    }

    protected override Type StyleKeyOverride => typeof(ContentDialog);
    
    public async Task<string?> OpenDialogAsync(TopLevel? host = null)
    {
        var result = await ShowAsync(host);
        return result == ContentDialogResult.None ? null : Text;
    }
}