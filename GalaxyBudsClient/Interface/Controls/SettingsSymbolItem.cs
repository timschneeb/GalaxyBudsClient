using System;
using Avalonia;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsSymbolItem : SettingsExpanderItem
{
    protected override Type StyleKeyOverride => typeof(SettingsExpanderItem);

    public static readonly StyledProperty<Symbol> SymbolProperty = 
        SymbolIcon.SymbolProperty.AddOwner<SettingsSymbolItem>();
    
    public static readonly StyledProperty<Symbol> ActionSymbolProperty = 
        AvaloniaProperty.Register<SettingsSymbolItem, Symbol>(nameof(ActionSymbol));

    public Symbol? Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }
    
    public Symbol? ActionSymbol
    {
        get => GetValue(ActionSymbolProperty);
        set => SetValue(ActionSymbolProperty, value);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SymbolProperty)
        {
            IconSource = Symbol == null ? null : new SymbolIconSource { Symbol = Symbol.Value };
        }
        else if (change.Property == ActionSymbolProperty)
        {
            ActionIconSource = ActionSymbol == null ? null : new SymbolIconSource { Symbol = ActionSymbol.Value };
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
