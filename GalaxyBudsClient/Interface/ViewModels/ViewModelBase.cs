using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using FluentIcons.Common;
using ReactiveUI;

namespace GalaxyBudsClient.Interface.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
}

public abstract class PageViewModelBase : ViewModelBase
{
    public abstract Control CreateView(); 
    public abstract string TitleKey { get; }
}

public abstract class MainPageViewModelBase : PageViewModelBase
{
    public abstract Symbol IconKey { get; }

    public abstract bool ShowsInFooter { get; }
}
