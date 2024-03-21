using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.InterfaceOld.Pages;

public class WelcomePage : AbstractPage
{
	public override Pages PageType => Pages.Welcome;

	private readonly Border _locale;

	private ContextMenu? _localeMenu;

	private bool _officialAppInstalled = false;

	public WelcomePage()
	{   
		AvaloniaXamlLoader.Load(this);
		_locale = this.GetControl<Border>("Locales");
	}

	public override void OnPageShown()
	{
		//_darkMode.IsChecked = Settings.Instance.DarkMode == DarkModes.Dark;
			
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
				Settings.Instance.Locale = locale;
				Loc.Load();
			};
		}

		_localeMenu = null;//MenuFactory.BuildContextMenu(localeMenuActions, _locale);
			
		// Only search for the Buds app on Windows 10 and above
		if (PlatformUtils.IsWindows && Environment.OSVersion.Version.Major >= 10) 
		{
			/* TODO: implement new setup wizard (refactor strings budsapp_text_p1, budsapp_text_p2, budsapp_text_p3) */
			try
			{
				var si = new ProcessStartInfo {
					FileName = "powershell",
					Arguments = "Get-AppxPackage SAMSUNGELECTRONICSCO.LTD.GalaxyBuds",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
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
						Log.Warning(exception, "WelcomePage.BudsAppDetected.ThreadPool");
					}
				});
			}
			catch (Exception exception)
			{
				Log.Warning(exception, "WelcomePage.BudsAppDetected");
			}
		}
	}
		
	private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		//MainWindow.Instance.Pager.SwitchPage(_officialAppInstalled ? Pages.BudsAppDetected : Pages.DeviceSelect);
	}

	private void Locales_OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		_localeMenu?.Open(_locale);
	}
}