using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsDescriptionItem : SettingsSymbolItem
{
    public SettingsDescriptionItem()
    {
        _title = new TextBlock();
        _subtitle = new TextBlock
        {
            [!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextFillColorSecondaryBrush"),
            TextWrapping = TextWrapping.Wrap
        };
        
        Content = new StackPanel
        {
            Spacing = 4,
            Children = { _title, _subtitle }
        };
    }

    private readonly TextBlock _title;
    private readonly TextBlock _subtitle;

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SettingsSwitchItem, string?>(nameof(Title));
    
    public static readonly StyledProperty<string?> SubtitleProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, string?>(nameof(Subtitle));
    
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TitleProperty)
        {
            _title.Text = change.GetNewValue<string?>();
        }
        else if (change.Property == SubtitleProperty)
        {
            _subtitle.Text = change.GetNewValue<string?>();
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
