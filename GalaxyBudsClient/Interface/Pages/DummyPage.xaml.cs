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
	}
}
