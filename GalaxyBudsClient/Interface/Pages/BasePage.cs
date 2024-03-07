using System;
using Avalonia.Controls;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Pages;

public class BasePage<T> : ContentControl, IDisposable where T : ViewModelBase
{  
    public BasePage()
    {
        Loc.LanguageUpdated += OnLanguageUpdated;
    }
    
    public void Dispose()
    {
        Loc.LanguageUpdated -= OnLanguageUpdated;
    }
    
    protected T? ViewModel => DataContext as T;

    protected virtual void OnLanguageUpdated() {}
}