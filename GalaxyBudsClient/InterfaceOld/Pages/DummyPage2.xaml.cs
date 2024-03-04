using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.InterfaceOld.Pages
{
 	public class DummyPage2 : AbstractPage
	{
		public override Pages PageType => Pages.Dummy2;

		private readonly Grid _grid; 
		
		public DummyPage2()
		{   
			AvaloniaXamlLoader.Load(this);
			_grid = this.GetControl<Grid>("Grid");
		}

		public override void OnPageShown()
		{
			_grid.Tag = "active";
		}

		public override void OnPageHidden()
		{
			_grid.Tag = "inactive";
		}
	}
}
