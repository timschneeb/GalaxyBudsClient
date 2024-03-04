using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Pages
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
