using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.InterfaceOld.Pages;
using Serilog;

namespace GalaxyBudsClient.InterfaceOld.Transition
{
 	public class PageContainer : Grid
    {
	    private AbstractPage? _lastPageCache;
	    private AbstractPage? _currentPage;
	    private CancellationTokenSource? _source;
	    private readonly IPageTransition _pageTransition;
		private readonly ViewModel _pageViewModel = new ViewModel();
		private readonly SemaphoreSlim _pageSemaphore = new SemaphoreSlim(1, 1);
		private bool _suspended;
		private bool _lastWasSuspended;

		public bool Suspended
		{
			set
			{
				if (_suspended == value) return;
				_suspended = value;
				if (_suspended)
				{
					_currentPage?.OnPageHidden();
				}
				else
				{
					_currentPage?.OnPageShown();
				}
			}
		}
		public event EventHandler<AbstractPage.Pages>? PageSwitched;
		
		public PageContainer()
		{   
			InitializeComponent();
			
			DataContext = _pageViewModel;
			
			var fadeTransition = new FadeTransition();
			fadeTransition.FadeOutComplete += (_, _) =>
			{
				if (_lastPageCache == null)
				{
					return;
				}
				if (!_lastWasSuspended)
				{
					_lastPageCache.OnPageHidden();
				}
				Children.Remove(_lastPageCache);
				_lastPageCache = null;
				_lastWasSuspended = false;
				_source = null;
			};
			_pageTransition = fadeTransition;

			// Add placeholder page
			RegisterPages(new DummyPage(), new DummyPage2());
			_currentPage = new DummyPage();
			Children.Add(_currentPage);
		}

		public AbstractPage.Pages CurrentPage => _currentPage?.PageType ?? AbstractPage.Pages.Undefined;

		public void RegisterPage(AbstractPage page)
		{
			if (HasPageType(page.PageType))
			{
				Log.Warning("Page type \'${PagePageType}\' is already assigned. Disposing old page", page.PageType);
				UnregisterPage(page);
			}
			_pageViewModel.Items.Add(page);
		}

		public void RegisterPages(params AbstractPage[] pages)
		{
			foreach (var page in pages)
			{
				RegisterPage(page);	
			}
		}
        
		public bool UnregisterPage(AbstractPage page)
		{
			if (Equals(_currentPage, page))
			{
				Log.Warning("Page \'${PagePageType}\' to be unregistered is currently loaded", page.PageType);
			}

			return _pageViewModel.Items.Remove(page);
		}
		
		
		public void UnregisterAll()
		{
			_pageViewModel.Items.Clear();
		}


		public bool SwitchPage(AbstractPage.Pages page)
		{
			var target = FindPage(page);
			Dispatcher.UIThread.InvokeAsync(async () =>
			{
				// This uses a Semaphore to ensure requests are processed in order, which is required to avoid
				// "Control already has a visual parent" and pages not opening due to a race condition
				if (!await _pageSemaphore.WaitAsync(1000))
				{
					Log.Error($"Timed out while waiting for page to show?");
					return;
				}

				try
				{
					if (CurrentPage == page) return;
					if (_lastPageCache != null)
					{
						_source!.Cancel();
						if (!_lastWasSuspended)
						{
							_lastPageCache.OnPageHidden();
						}

						Children.Remove(_lastPageCache);
						_lastPageCache = null;
						_lastWasSuspended = false;
						_source = null;
					}

					if (Children.Count != 1)
					{
						throw new InvalidOperationException();
					}

					if (target != null)
					{
						_lastPageCache = _currentPage;
						_lastWasSuspended = _suspended;
						_currentPage = target;
						_source = new CancellationTokenSource();
						Children.Add(_currentPage);
						if (!_suspended)
						{
							_currentPage.OnPageShown();
						}

						// don't await here, we don't want to wait until animation finished
						_ = _pageTransition.Start(_lastPageCache, _currentPage, true, _source.Token);
						PageSwitched?.Invoke(this, page);
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to switch page");
				}
				finally
				{
					_pageSemaphore.Release();
				}
			}, DispatcherPriority.Render);
			return target != null;
		}
		
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public AbstractPage? FindPage(AbstractPage.Pages page, bool nullAware = false)
        {
            var matches = _pageViewModel.Items.Where(abstractPage => abstractPage.PageType == page).ToArray();
            if (matches.Length < 1)
            {
                if (!nullAware)
                {
                    Log.Error("Page \'{Page}\' is not assigned", page);
                }

                return null;
            }
            if (matches.Length > 1)
            {
                Log.Warning("Page \'{Page}\' has multiple assignments. Choosing first one", page);
            }

            return matches[0];
        }

        private bool HasPageType(AbstractPage.Pages page)
        {
            return FindPage(page, nullAware: true) != null;
        }

        private class ViewModel
		{
			public ObservableCollection<AbstractPage> Items { get; } = new();
		}
    }
}
