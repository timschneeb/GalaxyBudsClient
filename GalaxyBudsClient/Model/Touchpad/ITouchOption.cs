using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Touchpad
{
    public interface ITouchOption
    {
        Dictionary<TouchOptions, byte> LookupTable { get; }

        byte ToByte(TouchOptions id)
        {
            return LookupTable[id];
        }

        TouchOptions FromByte(byte b)
        {
            return LookupTable.FirstOrDefault(x => x.Value == b).Key;
        }
    }
}