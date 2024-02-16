using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class WelcomePage : AbstractPage
	{
		public override Pages PageType => Pages.Welcome;

		private readonly SwitchDetailListItem _darkMode;
		private readonly Border _locale;

		private ContextMenu? _localeMenu;

		private bool _officialAppInstalled = false;

		public WelcomePage()
		{   
			AvaloniaXamlLoader.Load(this);

			_darkMode = this.GetControl<SwitchDetailListItem>("DarkMode");
			_locale = this.GetControl<Border>("Locales");
			
		}

		public override void OnPageShown()
		{
			_darkMode.IsChecked = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
			
			var localeMenuActions =
				new Dictionary<string,EventHandler<RoutedEventArgs>?>();
			
#pragma warning disable 8605
			foreach (int value in Enum.GetValues(typeof(Locales)))
#pragma warning restore 8605
			{
				if (value == (int)Locales.custom && !Loc.IsTranslatorModeEnabled())
					continue;

				var locale = (Locales)value;
				localeMenuActions[locale.GetDescription()] = (sender, args) =>
				{
					SettingsProvider.Instance.Locale = locale;
					Loc.Load();
				};
			}
			
			_localeMenu = MenuFactory.BuildContextMenu(localeMenuActions, _locale);
			
			// Only search for the Buds app on Windows 10 and above
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10) 
			{
				try
				{
					ProcessStartInfo si = new ProcessStartInfo {
						FileName = "powershell",
						Arguments = "Get-AppxPackage SAMSUNGELECTRONICSCO.LTD.GalaxyBuds",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
					};
	
					ThreadPool.QueueUserWorkItem(delegate {
						try
						{
							var process = Process.Start(si);
							if(process?.WaitForExit(4000) ?? false) 
							{
								_officialAppInstalled = process?.StandardOutput.ReadToEnd().Contains("SAMSUNGELECTRONICSCO.LTD.GalaxyBuds") ?? false;
							} 
						}
						catch(Exception exception)
						{
						        Log.Warning("WelcomePage.BudsAppDetected.ThreadPool: " + exception);
						}
					});
				}
				catch (Exception exception)
				{
					Log.Warning("WelcomePage.BudsAppDetected: " + exception);
				}
			}
		}
		
		private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			MainWindow.Instance.Pager.SwitchPage(_officialAppInstalled ? Pages.BudsAppDetected : Pages.DeviceSelect);
		}

		private void DarkMode_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.DarkMode = e ? DarkModes.Dark : DarkModes.Light;
			
			if (Application.Current is App app)
			{
				app.RestartApp(Pages.Welcome);
			}
		}

		private void Locales_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_localeMenu?.Open(_locale);
		}
	}
}
