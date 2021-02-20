using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class DummyPage2 : AbstractPage
	{
		public override Pages PageType => Pages.Dummy2;

		private readonly Grid _grid; 
		
		public DummyPage2()
		{   
			AvaloniaXamlLoader.Load(this);
			_grid = this.FindControl<Grid>("Grid");
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
