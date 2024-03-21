using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.ViewModels;

public class TouchActionViewModel(TouchOptions key, byte id)
{
    public TouchOptions Key { get; set; } = key;
    public byte Id { get; set; } = id;
        
    public static TouchActionViewModel FromKeyValuePair(KeyValuePair<TouchOptions, byte> pair)
    {
        return new TouchActionViewModel(pair.Key, pair.Value);
    }
}