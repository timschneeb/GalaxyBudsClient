using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.Dummy
{
    public class AutoStartHelper : IAutoStartHelper
    {
        public bool Enabled { get; set; } = false;
    }
}