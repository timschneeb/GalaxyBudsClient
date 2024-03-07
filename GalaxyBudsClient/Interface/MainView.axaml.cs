using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.ViewModels;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        ClipboardService.Owner = TopLevel.GetTopLevel(this);
        // Simple check - all desktop versions of this app will have a window as the TopLevel
        // Mobile and WASM will have something else
        _isDesktop = TopLevel.GetTopLevel(this) is Window;
        var vm = new MainViewViewModel();
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.SetFrame(FrameView);

        InitializeNavigationPages();

        FrameView.Navigated += OnFrameViewNavigated;
        NavView.ItemInvoked += OnNavigationViewItemInvoked;
        NavView.BackRequested += OnNavigationViewBackRequested;        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    public void InitializeNavigationPages()
    {
        var coreControls = ""; //GetControlsList("avares://FAControlsGallery/Assets/CoreControlsGroups.json");
        var faControls = ""; //GetControlsList("avares://FAControlsGallery/Assets/FAControlsGroups.json");

        var mainPages = new IMainPageViewModel[]
        {
            new HomePageViewModel(),
            /*new HomePageViewModel
            {
                //NavHeader = "Find My Buds",
                //IconKey = Symbol.LocationLive
            },
            new CoreControlsPageViewModel(coreControls)
            {
                //NavHeader = "Ambient Sound",
                //IconKey = Symbol.HeadphonesSoundWave
            },
            new FAControlsOverviewPageViewModel(faControls)
            {
                //NavHeader = "Touchpad",
                //IconKey = Symbol.HandDraw
            },*/
            new EqualizerPageViewModel()
            {
                //NavHeader = "Equalizer",
                //IconKey = Symbol.DeviceEq
            },/*
            new FAControlsOverviewPageViewModel(faControls)
            {
                //NavHeader = "Advanced settings",
                //IconKey = Symbol.WrenchScrewdriver
            },
            new FAControlsOverviewPageViewModel(faControls)
            {
                //NavHeader = "System",
                //IconKey = Symbol.Apps
            }*/
            new SettingsPageViewModel()
        };

        var menuItems = new List<NavigationViewItemBase>(4);
        var footerItems = new List<NavigationViewItemBase>(2);

        bool inDesign = Design.IsDesignMode;
        
        Dispatcher.UIThread.Post(() =>
        {
            for (int i = 0; i < mainPages.Length; i++)
            {
                var pg = mainPages[i];
                var nvi = new NavigationViewItem
                {
                    Content = pg.NavHeader,
                    Tag = pg,
                    IconSource = new SymbolIconSource { Symbol = pg.IconKey }
                };

                //ToolTip.SetTip(nvi, pg.NavHeader);

                if (_isDesktop || OperatingSystem.IsBrowser())
                {
                    nvi.Classes.Add("AppNav");
                }

                if (pg.ShowsInFooter)
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

            FrameView.NavigateFromObject((IEnumerableExtensions.ElementAt(NavView.MenuItemsSource, 0) as Control).Tag);
        });
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);

        // Frame handles X1 -> BackRequested automatically, we can handle X2
        // here to enable forward navigation
        if (pt.Properties.PointerUpdateKind == PointerUpdateKind.XButton2Released)
        {
            if (FrameView.CanGoForward)
            {
                FrameView.GoForward();
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
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

        ViewModelBase? mainPage = null;

        if (dc is IMainPageViewModel mpvmb)
        {
            mainPage = (ViewModelBase)mpvmb;
        }
       /* else if (dc is PageBaseViewModel pbvm)
        {
            mainPage = pbvm.Parent;
        }
        else if (page is ControlsPageBase cpb)
        {
            mainPage = cpb.CreationContext.Parent;
        }*/

        foreach (NavigationViewItem nvi in NavView.MenuItemsSource)
        {
            if (nvi.Tag == mainPage)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItemsSource)
        {
            if (nvi.Tag == mainPage)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
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

    private void SetNVIIcon(NavigationViewItem? item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        var t = item.Tag;

        if (t is ViewModelBase)
        {
            (item.IconSource as SymbolIconSource).IsFilled = selected;
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
