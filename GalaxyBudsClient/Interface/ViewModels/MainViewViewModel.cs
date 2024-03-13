using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.ViewModels.Pages;

namespace GalaxyBudsClient.Interface.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public MainViewViewModel()
    {
        NavigationFactory = new NavigationFactory(this);
    }

    public NavigationFactory NavigationFactory { get; }
    public required Func<Type, PageViewModelBase?> VmResolver { get; set; }
    public ObservableCollection<BreadcrumbViewModel> BreadcrumbItems { get; } = [
        // Workaround: The Breadcrumb library crashes if there are no items when it tries to measure its width 
        new BreadcrumbViewModel("", typeof(HomePageViewModel)) 
    ];
}

public class NavigationFactory(MainViewViewModel owner) : INavigationPageFactory
{
    private MainViewViewModel Owner { get; } = owner;

    public Control GetPage(Type srcType)
    {
        var model = Owner.VmResolver(srcType);
        if (model is null) 
            throw new ArgumentException("No view model found for type");
        
        return GetPageFromObject(model);
    }

    public Control GetPageFromObject(object target)
    {
        if (target is not PageViewModelBase model) 
            throw new ArgumentException("Target must derive from ViewModelBase");
        
        var view = model.CreateView();
        view.DataContext = model;
        return view;
    }
}
