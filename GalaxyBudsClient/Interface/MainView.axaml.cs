using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
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

    private PageViewModelBase? ResolveViewModelByType(Type arg)
    {
        return mainPages.FirstOrDefault(p => p.GetType() == arg);
    }

    private void OnBreadcrumbBarItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is BreadcrumbViewModel vm)
        {
            NavigationService.Instance.Navigate(vm.PageType);
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
        /*new HomePageViewModel()
        {
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
        // Change the current selected item back to normal
        // SetNVIIcon(sender as NavigationViewItem, false);

        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            NavigationTransitionInfo info;

            // Keep the frame navigation when not using connected animation but suppress it
            // if we have a connected animation binding two pages
           /* TODO if (FrameView.Content is ControlsPageBase cpb &&
                ((cpb.TargetType == null && nvi.Tag is CoreControlsPageViewModel) ||
                (cpb.TargetType != null && nvi.Tag is FAControlsOverviewPageViewModel)))
            {
                info = new SuppressNavigationTransitionInfo();
            }
            else*/
            {
                info = e.RecommendedNavigationTransitionInfo;
            }

            NavigationService.Instance.NavigateFromContext(nvi.Tag, info);
        }
    }

    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        var dc = page?.DataContext;
        
        
        PageViewModelBase? mainPage = null;
        if (dc is MainPageViewModelBase mpvmb)
        {
            FrameView.BackStack.Clear();
            ViewModel?.BreadcrumbItems.Clear();
            mainPage = mpvmb;
        }
        /* else if (dc is PageBaseViewModel pbvm)
        {
            mainPage = pbvm.Parent;
        }
        else if (page is ControlsPageBase cpb)
        {
            mainPage = cpb.CreationContext.Parent;
        }*/

        if (mainPage != null)
        {
            switch (e.NavigationMode)
            {
                case NavigationMode.Back when ViewModel?.BreadcrumbItems.Count > 0:
                    ViewModel?.BreadcrumbItems.RemoveAt(ViewModel.BreadcrumbItems.Count - 1);
                    break;
                case NavigationMode.New:
                    ViewModel?.BreadcrumbItems.Add(new BreadcrumbViewModel(mainPage.TitleKey, mainPage.GetType()));
                    break;
            }
        }


        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag == mainPage)
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
            if (nvi.Tag == mainPage)
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
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

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
