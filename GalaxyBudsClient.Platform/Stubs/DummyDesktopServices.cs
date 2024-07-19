namespace GalaxyBudsClient.Platform.Stubs;

public class DummyDesktopServices : BaseDesktopServices
{
    public override bool IsAutoStartEnabled { get; set; } = false;
}