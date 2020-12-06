using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Pages;
using Serilog;

namespace GalaxyBudsClient.Interface.Transition
{
 	public class PageContainer : UserControl
	{
		public Carousel Pager { get; }

		private object? _lastPageCache = null;
		
		private ViewModel _vm = new ViewModel();
		public PageContainer()
		{   
			InitializeComponent();
			
			DataContext = _vm;
			
			Pager = this.FindControl<Carousel>("Pager");
			var fadeTransition = new FadeTransition();
			fadeTransition.FadeInBegin += (sender, args) =>
			{
				if (Pager.SelectedItem is AbstractPage page)
				{
					page.OnPageShown();
				}
			};
			fadeTransition.FadeOutComplete += (sender, args) =>
			{		
				if (_lastPageCache is AbstractPage page)
				{
					page.OnPageHidden();
				}
			};
			Pager.PageTransition = fadeTransition;

			// Add placeholder page
			RegisterPage(new DummyPage());
			SwitchPage(AbstractPage.Pages.Dummy);
		}

		public void RegisterPage(AbstractPage page)
		{
			if (_vm.HasPageType(page.PageType))
			{
				Log.Warning($"Page type '${page.PageType}' is already assigned. Disposing old page.");
				UnregisterPage(page);
			}
			_vm.Items.Add(page);
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
			if (Equals(Pager.SelectedItem, page))
			{
				Log.Warning($"Page '${page.PageType}' to be unregistered is currently loaded");
			}

			return _vm.Items.Remove(page);
		}

		public bool SwitchPage(AbstractPage.Pages page)
		{
			var target = _vm.FindPage(page);

			if (target != null)
			{
				_lastPageCache = Pager.SelectedItem;
				Pager.SelectedItem = target;
			}

			return target != null;
		}
		
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private class ViewModel
		{
			public ViewModel()
			{
				Items = new ObservableCollection<AbstractPage>();
			}

			public ObservableCollection<AbstractPage> Items { get; }
			
			public AbstractPage? FindPage(AbstractPage.Pages page, bool nullAware = false)
			{
				AbstractPage[] matches = Items.Where(abstractPage => abstractPage.PageType == page).ToArray();
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
		}

	}
}
