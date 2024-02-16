using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class UnsupportedFeaturePage : AbstractPage
	{
		public override Pages PageType => Pages.UnsupportedFeature;

		public string RequiredVersion
		{
			set => _requiredFw.Text = value;
			get => _requiredFw.Text ?? string.Empty;
		}

		private readonly TextBlock _requiredFw;
		private readonly TextBlock _currentFw;
		
		public UnsupportedFeaturePage()
		{   
			AvaloniaXamlLoader.Load(this);
			_requiredFw = this.GetControl<TextBlock>("RequiredFw");
			_currentFw = this.GetControl<TextBlock>("CurrentFw");
		}

		/*private void InstanceOnSwVersionResponse(object sender, string e)
		{
			if (e == null)
				return;
			Dispatcher.Invoke(() => { CurrentFw.Text = $"{Loc.GetString("unsupported_feature_current_fwver")} {e.Remove(0, 1)}"; });
		}*/
		
		private void OnBackPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Home);
		}
	}
}
