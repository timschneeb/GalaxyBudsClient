using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentAvalonia.UI.Controls;

namespace GalaxyBudsClient.Interface.Controls;

/// <summary>
/// Custom InfoBar with smaller text and less opacity
/// </summary>
public class CustomInfoBar : InfoBar
{
    protected override Type StyleKeyOverride => typeof(InfoBar);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find("Message") is TextBlock tb)
        {
            tb.Opacity = 0.8;
            tb.FontSize = 13;
        }

        base.OnApplyTemplate(e);
    }
}
