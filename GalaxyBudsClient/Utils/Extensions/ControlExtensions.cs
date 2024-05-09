using Avalonia;
using Avalonia.Input;
using ScottPlot;

namespace GalaxyBudsClient.Utils.Extensions;

public static class ControlExtensions
{
    public static Pixel ToPixel(this PointerEventArgs e, Visual visual)
    {
        var x = (float)e.GetPosition(visual).X;
        var y = (float)e.GetPosition(visual).Y;
        return new Pixel(x, y);
    }
}