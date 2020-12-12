using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class PopupSettingsPage : AbstractPage
	{
		public override Pages PageType => Pages.SettingsPopup;
		
		private readonly SwitchDetailListItem _popupToggle;
		private readonly SwitchDetailListItem _compactToggle;
		private readonly DetailListItem _overrideTitle;
		private readonly MenuListItem _placement;
		
		public PopupSettingsPage()
		{   
			AvaloniaXamlLoader.Load(this);
			_popupToggle = this.FindControl<SwitchDetailListItem>("PopupToggle");
			_compactToggle = this.FindControl<SwitchDetailListItem>("CompactPopup");
			_overrideTitle = this.FindControl<DetailListItem>("OverrideTitle");
			_placement = this.FindControl<MenuListItem>("PositionPopup");

			Loc.LanguageUpdated += UpdateMenuDescriptions;
			Loc.LanguageUpdated += UpdateMenus;
		}

		public override void OnPageShown()
		{
			_popupToggle.IsChecked = SettingsProvider.Instance.Popup.Enabled;
			_compactToggle.IsChecked = SettingsProvider.Instance.Popup.Compact;
			UpdateMenuDescriptions();
			UpdateMenus();
		}
		
		private void UpdateMenuDescriptions()
		{
			_overrideTitle.Description = SettingsProvider.Instance.Popup.CustomTitle.Length < 1 ?
				Loc.Resolve("notset") : SettingsProvider.Instance.Popup.CustomTitle;
			_placement.Description = SettingsProvider.Instance.Popup.Placement.GetDescription();
		}
		
		private void UpdateMenus()
		{
			var menuActions = new Dictionary<string,EventHandler<RoutedEventArgs>?>();
#pragma warning disable 8605
			foreach (int value in Enum.GetValues(typeof(PopupPlacement)))
#pragma warning restore 8605
			{
				var placement = (PopupPlacement)value;
				menuActions[placement.GetDescription()] = (sender, args) =>
				{
					SettingsProvider.Instance.Popup.Placement = placement;
					UpdateMenuDescriptions();
				};
			}
			_placement.Items = menuActions;
		}
		
		private void BackButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(Pages.Settings);
		}

		private void Demo_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
			new BudsPopup(Models.BudsPlus,cache!.BatteryL, cache.BatteryR, cache.BatteryCase).Show();
		}

		private async void OverrideTitle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			var dlg = new InputDialog();
			dlg.Input.Text = SettingsProvider.Instance.Popup.CustomTitle;
			dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var result = await dlg.ShowDialog<string?>(MainWindow.Instance);
			if (result != null)
			{
				SettingsProvider.Instance.Popup.CustomTitle = result;
				UpdateMenuDescriptions();
			}
		}

		private void CompactPopup_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.Popup.Compact = e;
		}

		private void PopupToggle_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.Popup.Enabled = e;
		}
	}
}
