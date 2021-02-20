using System;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    interface IBluetopiaSecurity : IBluetoothSecurity
    {
        void InitStack();
    }
}
