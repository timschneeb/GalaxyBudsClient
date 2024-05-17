using System.Collections.Generic;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform.Interfaces;

public interface IHotkey
{
    IEnumerable<ModifierKeys> Modifier { set; get; }
    IEnumerable<Keys> Keys { set; get; }
}