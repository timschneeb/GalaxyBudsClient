using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using FluentIcons.Common;
using GalaxyBudsClient.Model;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
}

public abstract class PageViewModelBase : ViewModelBase
{
    protected PageViewModelBase()
    {
        EventDispatcher.Instance.EventReceived += OnEventReceived;
    }

    protected virtual void OnEventReceived(EventDispatcher.Event type, object? parameter){}

    public abstract Control CreateView(); 
    public abstract string TitleKey { get; }
    public virtual void OnNavigated() {}
}

public abstract class MainPageViewModelBase : PageViewModelBase
{
    public abstract Symbol IconKey { get; }

    public abstract bool ShowsInFooter { get; }
}

public abstract class SubPageViewModelBase : PageViewModelBase
{
}
