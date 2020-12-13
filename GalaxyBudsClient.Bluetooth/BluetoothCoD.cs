using System;

namespace GalaxyBudsClient.Bluetooth
{
    public class BluetoothCoD
    {
        public enum Major
        {
            Reserved,
            Miscellaneous = 0, 
            Computer = 1,
            Phone = 2,
            Network = 3,
            AudioVideo = 4, 
            Peripheral = 5, 
            Imaging = 6, 
            Wearable = 7, 
            Toy = 8,
            Health = 9,
            Uncategorized = 31
        }
        
        public Major MajorClass { get; }

        public BluetoothCoD(uint cod)
        {
            var rawMajor = (cod >> 8) & 0x1f;
            var rawMinor = (cod >> 2) & 0x3f;

            if (rawMajor <= 9 || rawMajor == 31)
            {
                MajorClass = (Major) rawMajor;
            }
            else
            {
                MajorClass = Major.Reserved;
            }
        }

        public override string ToString()
        {
            return MajorClass.ToString();
        }
    }
}