using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class CrowdsourcingSettingsPage : AbstractPage
	{
		public override Pages PageType => Pages.SettingsCrowdsourcing;
		
		private readonly SwitchDetailListItem _crowdToggle;
		private readonly SwitchDetailListItem _crashToggle;
		
		public CrowdsourcingSettingsPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_crowdToggle = this.FindControl<SwitchDetailListItem>("CrowdsourcingToggle");
			_crashToggle = this.FindControl<SwitchDetailListItem>("CrashToggle");
		}

		public override void OnPageShown()
		{
			_crowdToggle.IsChecked = !SettingsProvider.Instance.Experiments.Disabled;
			_crashToggle.IsChecked = !SettingsProvider.Instance.DisableCrashReporting;
		}
	
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Settings);
		}
		
		private void CrashToggle_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.DisableCrashReporting = !e;
		}

		private void CrowdsourcingToggle_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.Experiments.Disabled = !e;
		}
	}
}
