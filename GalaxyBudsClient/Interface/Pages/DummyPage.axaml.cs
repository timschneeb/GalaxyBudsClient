using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class DummyPage : AbstractPage
	{
		public override Pages PageType => Pages.Dummy;
		
		public DummyPage()
		{   
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnPageShown()
		{
			Log.Debug(this.GetType().Name + " shown");
		}

		public override void OnPageHidden()
		{
			Log.Debug(this.GetType().Name + " hidden");
		}
	}
}
