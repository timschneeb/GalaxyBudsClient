using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyAutoStartHelper : IAutoStartHelper
{
    public bool Enabled { get; set; } = false;
}