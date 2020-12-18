using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Factory
{
    internal interface IUsesBluetoothConnectorImplementsServiceLookup
    {
        /// <summary>
        /// When overidden, initiates 
        /// lookup the SDP record with the give Service Class Id
        /// to find the RFCOMM port number (SCN) that the server is listening on.
        /// The process returns a list of port numbers.
        /// </summary>
        /// <param name="address">The remote device.
        /// </param>
        /// <param name="serviceGuid">The Service Class Id.
        /// </param>
        /// <param name="asyncCallback">callback</param>
        /// <param name="state">state</param>
        /// <returns>IAsyncResult</returns>
        IAsyncResult BeginServiceDiscovery(
            BluetoothAddress address, Guid serviceGuid,
            AsyncCallback asyncCallback, Object state);

        /// <summary>
        /// When overidden, 
        /// completes the SDP Record to port number lookup process
        /// </summary>
        /// -
        /// <param name="ar">IAsyncResult from <see cref="BeginServiceDiscovery"/>.
        /// </param>
        /// -
        /// <remarks>
        /// <para>There must be at least one entry in the result list for each
        /// Service Record found for the specified Service Class Id.  This
        /// allows us to know if no records were found, or that records were
        /// found but none of them were for RFCOMM.
        /// If a particular record does not have a RFCOMM port then -1 (negative
        /// one should be added to the list for it).
        /// </para>
        /// <para>The process may throw an exception if an error occurs, e.g.
        /// the remote device did not respond.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>A <see cref="T:System.Collections.Generic.List{System.Int32}"/>
        /// with at least one entry for each Service Record
        /// found for the specified Service Class Id, the item being -1 if the
        /// record has no port. is .
        /// </returns>
        List<int> EndServiceDiscovery(IAsyncResult ar);
    }
}
