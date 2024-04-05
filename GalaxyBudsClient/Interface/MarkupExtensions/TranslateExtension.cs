using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

// Used as a workaround for XamlUIExtension which doesn't support DynamicResources for text bindings properly
public class TranslateExtension(string key) : MarkupExtension, IObservable<string>
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ToBinding();
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        return Loc.AddObserverForKey(key, observer);
    }
}