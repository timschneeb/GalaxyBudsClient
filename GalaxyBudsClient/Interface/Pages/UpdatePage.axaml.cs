using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class UpdatePage : AbstractPage
	{
		public override Pages PageType => Pages.UpdateAvailable;
		
		private readonly TextBlock _versionTitle;
		
		public UpdatePage()
		{   
			AvaloniaXamlLoader.Load(this);
			_versionTitle = this.FindControl<TextBlock>("VersionTitle");
		}

		public override void OnPageShown()
		{
			Log.Debug(this.GetType().Name + " shown");
		}

		public override void OnPageHidden()
		{
			Log.Debug(this.GetType().Name + " hidden");
		}
	
		private void ViewChangelog_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient/releases");
		}
		
		private void OpenWebsite(String url)
		{
			var psi = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}
	}
}
