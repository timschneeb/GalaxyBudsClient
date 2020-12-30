using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public interface IL2CapClient : IBluetoothClient
    {
        /// <exclude/>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mtu")]
        int GetMtu();
    }
}
