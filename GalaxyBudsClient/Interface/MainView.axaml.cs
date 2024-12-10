using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Utils.Interface;
using Serilog;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface;

public partial class MainView : UserControl
{
    public static MainView? Instance { get; private set; }

    private MainViewViewModel? ViewModel => DataContext as MainViewViewModel;
    private PageViewModelBase? CurrentPageViewModel { set; get; }

    private readonly MainPageViewModelBase[] _mainPages = [
        new WelcomePageViewModel(),
        new HomePageViewModel(),
        new NoiseControlPageViewModel(),
        new EqualizerPageViewModel(),
        new FindMyBudsPageViewModel(),
        new TouchpadPageViewModel(),
        new AdvancedPageViewModel(),
        new SystemPageViewModel(),
        new DevicesPageViewModel(),
        new SettingsPageViewModel()
    ];

    private readonly SubPageViewModelBase[] _subPages =
    [
        new AmbientCustomizePageViewModel(),
        new BixbyRemapPageViewModel(),
        new FirmwarePageViewModel(),
        new FitTestPageViewModel(),
        new HotkeyPageViewModel(),
        new SystemInfoPageViewModel(),
        new RenamePageViewModel(),
        new FmmConfigPageViewModel(),
        new UsageReportPageViewModel(),
        new BatteryHistoryPageViewModel(),
        new HiddenModePageViewModel()
    ];
    
    public MainView()
    {
        InitializeComponent();
        Instance = this;
        
        Loc.LanguageUpdated += OnLanguageUpdated;
        
        var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;
        if (insetsManager != null)
        {
            insetsManager.DisplayEdgeToEdge = true;
        }
        
        var vm = new MainViewViewModel
        {
            VmResolver = ResolveViewModelByType
        };
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewViewModel.IsInSetupWizard))
                CheckSetupWizardState();
        };
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.Frame = FrameView;

        InitializeNavigationPages();
        
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        BreadcrumbBar.ItemClicked += OnBreadcrumbBarItemClicked;
        FrameView.Navigated += OnFrameViewNavigated;
        NavView.ItemInvoked += OnNavigationViewItemInvoked;
        NavView.BackRequested += OnNavigationViewBackRequested;
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsData.ShowSidebar))
        {
            RefreshSidebarState();
            RefreshSidebarItemStates();
        }
    }

    private void OnLanguageUpdated()
    {
        FlowDirection = Loc.ResolveFlowDirection();
    }

    public T? ResolveViewModelByType<T>() where T : PageViewModelBase
    {
        return ResolveViewModelByType(typeof(T)) as T;
    }

    private PageViewModelBase? ResolveViewModelByType(Type arg)
    {
        return _mainPages.Concat<PageViewModelBase>(_subPages).FirstOrDefault(p => p.GetType() == arg);
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if(ViewModel == null)
            return;
        
        // Ignore the last item, as it's the current page
        if (args.Item is BreadcrumbViewModel vm && args.Index != ViewModel.BreadcrumbItems.Count - 1)
        {
            // Remove BreadcrumbItems from the end until the clicked item is the last one
            for(var i = ViewModel.BreadcrumbItems.Count - 1; i >= 0; i--)
            {
                if (ViewModel.BreadcrumbItems[i] == vm)
                {
                    break;
                }
                
                ViewModel.BreadcrumbItems.RemoveAt(i);
            }
            NavigationService.Instance.Navigate(vm.PageType);
            
            // Also remove the items from the FrameView's back stack
            for(var i = FrameView.BackStack.Count - 1; i >= 0; i--)
            {
                var stop = FrameView.BackStack[i].SourcePageType == vm.PageType;
                
                FrameView.BackStack.RemoveAt(i);
                
                if (stop)
                {
                    break;
                }
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[1].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }
    
    private void CheckSetupWizardState()
    {
        foreach (var item in NavView.MenuItemsSource)
        {
            if(item is not NavigationViewItem nvi)
                continue;

            if(nvi.Tag is WelcomePageViewModel)
                nvi.IsVisible = ViewModel?.IsInSetupWizard == true;
            else
                nvi.IsVisible = ViewModel?.IsInSetupWizard == false;
        }
        
        foreach (var item in NavView.FooterMenuItemsSource)
        {
            if(item is not NavigationViewItem nvi)
                continue;

            if (nvi.Tag is not SettingsPageViewModel)
                nvi.IsEnabled = ViewModel?.IsInSetupWizard == false;
        }
        
        if (ViewModel?.IsInSetupWizard == true && CurrentPageViewModel is not WelcomePageViewModel)
        {
            NavigationService.Instance.Navigate(typeof(WelcomePageViewModel));
        }
        else if (ViewModel?.IsInSetupWizard == false && CurrentPageViewModel is WelcomePageViewModel)
        {
            NavigationService.Instance.Navigate(typeof(HomePageViewModel));
        }
    }
    
    private void RefreshSidebarState()
    {
        NavView.Classes.Set("AppNav", Settings.Data.ShowSidebar);
        NavView.Classes.Set("MobileContentContainer", true);
        NavView.PaneDisplayMode = Settings.Data.ShowSidebar ? 
            NavigationViewPaneDisplayMode.Left : NavigationViewPaneDisplayMode.LeftMinimal;
    }

    private void RefreshSidebarItemStates()
    {
        foreach(var nvi in NavView.MenuItemsSource.Cast<NavigationViewItem>()
                    .Concat(NavView.FooterMenuItemsSource.Cast<NavigationViewItem>()))
        {
            nvi.Classes.Set("AppNav", Settings.Data.ShowSidebar);
        }
    }
    
    private void InitializeNavigationPages()
    {
        RefreshSidebarState();
            
        Dispatcher.UIThread.Post(() =>
        {   
            var menuItems = new List<NavigationViewItem>(14);
            var footerItems = new List<NavigationViewItem>(2);
            
            foreach (var page in _mainPages)
            {
                var nvi = new NavigationViewItem
                {
                    Content = Loc.ResolveOrDefault(page.TitleKey) ?? page.FallbackTitle,
                    Tag = page,
                    IconSource = new SymbolIconSource { Symbol = page.IconKey }
                };

                nvi.Classes.Set("AppNav", Settings.Data.ShowSidebar);

                if (page.ShowsInFooter)
                    footerItems.Add(nvi);
                else
                    menuItems.Add(nvi);
            }

            NavView.MenuItemsSource = menuItems;
            NavView.FooterMenuItemsSource = footerItems;
            CheckSetupWizardState();
            
            // Go to Home if not in setup mode
            if(ViewModel?.IsInSetupWizard == false)
                NavigationService.Instance.Navigate(typeof(HomePageViewModel));
        }, DispatcherPriority.Render);
    }

    private void OnNavigationViewBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        /* Pressing back on a main page: remove all items until a MainPageViewModel is found
           We don't want to return to a sub page. The back button mostly behaves like an up button. */

        if (CurrentPageViewModel is MainPageViewModelBase)
        {
            for (var i = FrameView.BackStack.Count - 1; i >= 0; i--)
            {
                if (FrameView.BackStack[i].SourcePageType.IsSubclassOf(typeof(MainPageViewModelBase)))
                    break;

                Log.Error("Removed {Page}", FrameView.BackStack[i].SourcePageType);
                FrameView.BackStack.RemoveAt(i);
            }
        }

        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is not NavigationViewItem nvi) 
            return;

        if (nvi.Tag == null)
            throw new InvalidOperationException("Tag is null");
            
        NavigationService.Instance.NavigateFromContext(nvi.Tag, e.RecommendedNavigationTransitionInfo);
    }
    
    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        CurrentPageViewModel?.OnNavigatedFrom();

        var control = e.Content as Control;
        var page = control?.DataContext as PageViewModelBase;
        
        if (page is MainPageViewModelBase)
        {
            if (page is HomePageViewModel or WelcomePageViewModel)
            {
                FrameView.BackStack.Clear();
            }

            ViewModel?.BreadcrumbItems.Clear();
        }
        
        if (page != null)
        {
            if (e.NavigationMode == NavigationMode.New || page is MainPageViewModelBase) 
                ViewModel?.BreadcrumbItems.Add(new BreadcrumbViewModel(page.TitleKey, page.GetType()));
            else if (e.NavigationMode == NavigationMode.Back && ViewModel?.BreadcrumbItems.Count > 0)
                ViewModel?.BreadcrumbItems.RemoveAt(ViewModel.BreadcrumbItems.Count - 1);
        }
        
        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag == page)
            {
                NavView.SelectedItem = nvi;
                SetNvIcon(nvi, true);
            }
            else
            {
                SetNvIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItemsSource)
        {
            if (nvi.Tag == page)
            {
                NavView.SelectedItem = nvi;
                SetNvIcon(nvi, true);
            }
            else
            {
                SetNvIcon(nvi, false);
            }
        }

        CurrentPageViewModel = page;
        page?.OnNavigatedTo();
        
        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            NavView.IsBackButtonVisible = true;
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            NavView.IsBackButtonVisible = false;
        }
    }

    private static void SetNvIcon(NavigationViewItem? item, bool selected)
    {
        var t = item?.Tag;

        if (t is ViewModelBase && item?.IconSource is SymbolIconSource source)
        {
            source.IsFilled = selected;
        }
    }

    private async void OnTroubleshootConnectionClicked(object? sender, RoutedEventArgs e)
    {
        await new MessageBox
        {
            Title = Strings.ConnlostTroubleshootTitle,
            Description = Strings.ConnlostTroubleshootContent
        }.ShowAsync();
    }
}
