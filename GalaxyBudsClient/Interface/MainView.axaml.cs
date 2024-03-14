using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using CommandLine;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private MainViewViewModel? ViewModel => DataContext as MainViewViewModel;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        ClipboardService.Owner = TopLevel.GetTopLevel(this);
        // Simple check - all desktop versions of this app will have a window as the TopLevel
        // Mobile and WASM will have something else
        _isDesktop = TopLevel.GetTopLevel(this) is Window;
        var vm = new MainViewViewModel
        {
            VmResolver = ResolveViewModelByType
        };
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.Frame = FrameView;

        InitializeNavigationPages();

        BreadcrumbBar.ItemClicked += OnBreadcrumbBarItemClicked;
        FrameView.Navigated += OnFrameViewNavigated;
        NavView.ItemInvoked += OnNavigationViewItemInvoked;
        NavView.BackRequested += OnNavigationViewBackRequested;
    }

    public T? ResolveViewModelByType<T>() where T : PageViewModelBase
    {
        return mainPages.Concat<PageViewModelBase>(subPages).FirstOrDefault(p => p.GetType() == typeof(T)).Cast<T?>();
    }
    
    public PageViewModelBase? ResolveViewModelByType(Type arg)
    {
        return mainPages.Concat<PageViewModelBase>(subPages).FirstOrDefault(p => p.GetType() == arg);
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
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    private readonly MainPageViewModelBase[] mainPages = [
        new HomePageViewModel(),
        new NoiseControlPageViewModel(),
        /*{
            //NavHeader = "Find My Buds",
            //IconKey = Symbol.LocationLive
        },
        new CoreControlsPageViewModel()
        {
            //NavHeader = "Ambient Sound",
            //IconKey = Symbol.HeadphonesSoundWave
        },
        new FAControlsOverviewPageViewModel()
        {
            //NavHeader = "Touchpad",
            //IconKey = Symbol.HandDraw
        },*/
        new EqualizerPageViewModel(),/*
        new FAControlsOverviewPageViewModel()
        {
            //NavHeader = "Advanced settings",
            //IconKey = Symbol.WrenchScrewdriver
        },
        new FAControlsOverviewPageViewModel()
        {
            //NavHeader = "System",
            //IconKey = Symbol.Apps
        }*/
        new SettingsPageViewModel()
    ];

    private readonly SubPageViewModelBase[] subPages =
    [
        new AmbientCustomizePageViewModel()
    ];

    
    private void InitializeNavigationPages()
    {
        
        var menuItems = new List<NavigationViewItemBase>(6);
        var footerItems = new List<NavigationViewItemBase>(1);

        
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var page in mainPages)
            {
                var nvi = new NavigationViewItem
                {
                    Content = page.TitleKey,
                    Tag = page,
                    IconSource = new SymbolIconSource { Symbol = page.IconKey }
                };

                if (_isDesktop || OperatingSystem.IsBrowser())
                {
                    nvi.Classes.Add("AppNav");
                }

                if (page.ShowsInFooter)
                    footerItems.Add(nvi);
                else
                    menuItems.Add(nvi);
            }

            NavView.MenuItemsSource = menuItems;
            NavView.FooterMenuItemsSource = footerItems;

            if (_isDesktop || OperatingSystem.IsBrowser())
            {
                NavView.Classes.Add("AppNav");
            }
            else
            {
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            }

            FrameView.NavigateFromObject((NavView.MenuItemsSource.ElementAt(0) as Control)!.Tag);
        });
    }

    private void OnNavigationViewBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }

    private void OnNavigationViewItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is not NavigationViewItem nvi) 
            return;

        if (nvi.Tag == null)
            throw new InvalidOperationException("Tag is null");
            
        // TODO: maybe customize transitions? (up/down for NVIs and left/right for subpages)
        NavigationService.Instance.NavigateFromContext(nvi.Tag, e.RecommendedNavigationTransitionInfo);
    }

    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        var control = e.Content as Control;
        var page = control?.DataContext as PageViewModelBase;

        if (page is MainPageViewModelBase)
        {
            if (page is HomePageViewModel)
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

        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
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

    private async void AnimateContentForBackButton(bool show)
    {
        if (!IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0,0,0,1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48,4,12,4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0,0,0,1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12,4,12,4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);
        }
    }

    private bool _isDesktop;
}
