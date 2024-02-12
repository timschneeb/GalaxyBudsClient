using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Interface.Elements;
using GalaxyBudsClient.Interface.Pages;
using Serilog;

namespace GalaxyBudsClient.Interface.Transition
{
 	public class PageContainer : Grid
    {
	    private AbstractPage? _lastPageCache;
	    private AbstractPage? _currentPage;
	    private CancellationTokenSource? source;
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
				source = null;
			};
			_pageTransition = fadeTransition;

			// Add placeholder page
			RegisterPages(new DummyPage(), new DummyPage2());
			_currentPage = new DummyPage();
			Children.Add(_currentPage);
		}

		public AbstractPage.Pages CurrentPage
		{
			get
			{
				if (_currentPage == null)
				{
					return AbstractPage.Pages.Undefined;
				}
				return _currentPage.PageType;
			}
		}

		public void RegisterPage(AbstractPage page)
		{
			if (HasPageType(page.PageType))
			{
				Log.Warning($"Page type '${page.PageType}' is already assigned. Disposing old page.");
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
				Log.Warning($"Page '${page.PageType}' to be unregistered is currently loaded");
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
						source!.Cancel();
						if (!_lastWasSuspended)
						{
							_lastPageCache.OnPageHidden();
						}
						Children.Remove(_lastPageCache);
						_lastPageCache = null;
						_lastWasSuspended = false;
						source = null;
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
						source = new CancellationTokenSource();
						Children.Add(_currentPage);
						if (!_suspended)
						{
							_currentPage.OnPageShown();
						}
						// don't await here, we don't want to wait until animation finished
						_pageTransition.Start(_lastPageCache, _currentPage, true, source.Token);
						PageSwitched?.Invoke(this, page);
					}
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
            AbstractPage[] matches = _pageViewModel.Items.Where(abstractPage => abstractPage.PageType == page).ToArray();
            if (matches.Length < 1)
            {
                if (!nullAware)
                {
                    Log.Error($"Page '{page}' is not assigned");
                }

                return null;
            }
            if (matches.Length > 1)
            {
                Log.Warning($"Page '{page}' has multiple assignments. Choosing first one.");
            }

            return matches[0];
        }
        public bool HasPageType(AbstractPage.Pages page)
        {
            return FindPage(page, nullAware: true) != null;
        }

		public class ViewModel
		{
			public ViewModel()
			{
				Items = new ObservableCollection<AbstractPage>();
			}

			public ObservableCollection<AbstractPage> Items { get; }
        }
    }
}
