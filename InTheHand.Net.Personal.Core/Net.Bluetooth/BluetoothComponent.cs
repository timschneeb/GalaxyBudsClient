//#define PRE_V2_4
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using InTheHand.Net.Sockets;
using System.Diagnostics;
#if NETCF
using SendOrPostCallback = System.Threading.WaitCallback;
#endif

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Provides simple access to asynchronous methods on Bluetooth features, for
    /// instance to background device discovery.
    /// </summary>
    /// -
    /// <example>
    /// <code lang="VB.NET">
    /// Public Sub DiscoDevicesAsync()
    ///     Dim bco As New BluetoothComponent()
    ///     AddHandler bco.DiscoverDevicesProgress, AddressOf HandleDiscoDevicesProgress
    ///     AddHandler bco.DiscoverDevicesComplete, AddressOf HandleDiscoDevicesComplete
    ///     bco.DiscoverDevicesAsync(255, True, True, True, False, 99)
    /// End Sub
    ///
    /// Private Sub HandleDiscoDevicesProgress(ByVal sender As Object, ByVal e As DiscoverDevicesEventArgs)
    ///     Console.WriteLine("DiscoDevicesAsync Progress found {0} devices.", e.Devices.Length)
    /// End Sub
    ///
    /// Private Sub HandleDiscoDevicesComplete(ByVal sender As Object, ByVal e As DiscoverDevicesEventArgs)
    ///     Debug.Assert(CInt(e.UserState) = 99)
    ///     If e.Cancelled Then
    ///         Console.WriteLine("DiscoDevicesAsync cancelled.")
    ///     ElseIf e.Error IsNot Nothing Then
    ///         Console.WriteLine("DiscoDevicesAsync error: {0}.", e.Error.Message)
    ///     Else
    ///         Console.WriteLine("DiscoDevicesAsync complete found {0} devices.", e.Devices.Length)
    ///     End If
    /// End Sub
    /// </code>
    /// </example>
    public class BluetoothComponent : Component
    {
        readonly BluetoothClient m_cli;

        //----
        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothComponent"/> class.
        /// </summary>
        public BluetoothComponent()
            : this(new BluetoothClient())
        {
        }

        //TODO !!!! Add to public stack factory

        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.Net.Bluetooth.BluetoothComponent"/> class.
        /// </summary>
        /// -
        /// <param name="cli">A <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/> 
        /// instance to use to run discovery on.  Must be non-null.
        /// </param>
        public BluetoothComponent(BluetoothClient cli)
        {
            if (cli == null) throw new ArgumentNullException("cli");
            m_cli = cli;
        }

        /// <summary>
        /// Optionally disposes of the managed resources used by the
        /// <see cref="T:InTheHand.Net.Bluetooth.BluetoothComponent"/> class.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged
        /// resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) {
                    m_cli.Dispose();
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        //----
        /// <summary>
        /// Occurs when an device discovery operation completes.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This event is raised at the end of the discovery process
        /// and lists all the discovered devices.
        /// </para>
        /// </remarks>
        /// -
        /// <seealso cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesProgress"/>
        public event EventHandler<DiscoverDevicesEventArgs> DiscoverDevicesComplete;

        /// <summary>
        /// Raises the <see cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesComplete"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:InTheHand.Net.Bluetooth.DiscoverDevicesEventArgs"/>
        /// object that contains event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            Justification = "It it /not/ visible!")]
        protected void OnDiscoveryComplete(DiscoverDevicesEventArgs e)
        {
            EventHandler<DiscoverDevicesEventArgs> eh = DiscoverDevicesComplete;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <summary>
        /// Occurs during an device discovery operation
        /// to show one or more new devices.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This event is raised for all discovered devices, both the
        /// known devices which are presented first, if requested,
        /// as well as newly discovery device found by the inquiry process,
        /// again if requested.
        /// </para>
        /// <para>Note that any event instance may include one or more devices.  Note
        /// also that a particular device may be presented more than one time;
        /// including once from the &#x2018;known&#x2019; list, once when a
        /// device is dicovered, and possibly another time when the discovery
        /// process retrieves the new device&#x2019;s Device Name.
        /// </para>
        /// </remarks>
        /// -
        /// <seealso cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesComplete"/>
        public event EventHandler<DiscoverDevicesEventArgs> DiscoverDevicesProgress;

        /// <summary>
        /// Raises the <see cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesProgress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:InTheHand.Net.Bluetooth.DiscoverDevicesEventArgs"/>
        /// object that contains event data.
        /// </param>
        protected void OnDiscoveryProgress(DiscoverDevicesEventArgs e)
        {
            var eh = DiscoverDevicesProgress;
            if (eh != null) {
                eh(this, e);
            }
        }

        /// <summary>
        /// Discovers accessible Bluetooth devices and returns their names and addresses.
        /// This method does not block the calling thread.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>See <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Boolean,System.Boolean,System.Boolean,System.Boolean)"/>
        /// for more information.
        /// </para>
        /// <para>The devices are presented in the <see cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesComplete"/>
        /// and <see cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesProgress"/> events.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="maxDevices">The maximum number of devices to get information about.
        /// </param>
        /// <param name="authenticated">True to return previously authenticated/paired devices.
        /// </param>
        /// <param name="remembered">True to return remembered devices.
        /// </param>
        /// <param name="unknown">True to return previously unknown devices.
        /// </param>
        /// <param name="discoverableOnly">True to return only the devices that 
        /// are in range, and in    discoverable mode.  See the remarks section.
        /// </param>
        /// <param name="state">A user-defined object that is passed to the method
        /// invoked when the asynchronous operation completes.
        /// </param>
        /// -
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public void DiscoverDevicesAsync(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            object state)
        {
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(state);
            // Provide the remembered devices immediately
            DoRemembered(authenticated, remembered, discoverableOnly, asyncOp);
            //
#if PRE_V2_4
            if (discoverableOnly)
                throw new ArgumentException("Flag 'discoverableOnly' is not supported in this version.", "discoverableOnly");
            FuncDiscoDevs dlgt = new FuncDiscoDevs(m_cli.DiscoverDevices);
            FuncDiscoDevs exist = Interlocked.CompareExchange(ref m_dlgt, dlgt, null);
            if (exist != null)
                throw new InvalidOperationException("Only support one concurrent operation.");
            dlgt.BeginInvoke(maxDevices,
                authenticated, remembered, unknown, //discoverableOnly,
                HandleDiscoComplete, asyncOp);
#else
            m_cli.BeginDiscoverDevices(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                HandleDiscoComplete, asyncOp,
                HandleDiscoNewDevice, asyncOp);
#endif
        }

        private void DoRemembered(bool authenticated, bool remembered, bool discoverableOnly, AsyncOperation asyncOp)
        {
            if ((authenticated || remembered) && !discoverableOnly) {
                var rmbd = m_cli.DiscoverDevices(255, authenticated, remembered, false, false);
                if (rmbd.Length != 0) {
                    var e = new DiscoverDevicesEventArgs(rmbd, asyncOp.UserSuppliedState);
                    SendOrPostCallback cb = delegate(object args) {
                        OnDiscoveryProgress((DiscoverDevicesEventArgs)args);
                    };
                    asyncOp.Post(cb, e);
                }
            }
        }

#if PRE_V2_4
        delegate BluetoothDeviceInfo[] FuncDiscoDevs(int maxDevices,
            bool authenticated, bool remembered, bool unknown/*, bool discoverableOnly*/);
        FuncDiscoDevs m_dlgt;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void HandleDiscoComplete(IAsyncResult ar)
        {
            AsyncOperation asyncOp = (AsyncOperation)ar.AsyncState;
            DiscoverDevicesEventArgs e;
            try {
#if PRE_V2_4
                FuncDiscoDevs dlgt = Interlocked.Exchange(ref m_dlgt, null);
                BluetoothDeviceInfo[] arr = dlgt.EndInvoke(ar);
#else
                BluetoothDeviceInfo[] arr = m_cli.EndDiscoverDevices(ar);
#endif
                e = new DiscoverDevicesEventArgs(arr, asyncOp.UserSuppliedState);
            } catch (Exception ex) {
                e = new DiscoverDevicesEventArgs(ex, asyncOp.UserSuppliedState);
                // TO-DO ?? Set Cancelled if disposed?
            }
            //
            SendOrPostCallback cb = delegate(object args) {
                OnDiscoveryComplete((DiscoverDevicesEventArgs)args);
            };
            asyncOp.PostOperationCompleted(cb, e);
        }

        private void HandleDiscoNewDevice(InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo newDevice, object state)
        {
            Debug.WriteLine(DateTime.UtcNow.TimeOfDay.ToString() + ": HandleDiscoNewDevice.");
            AsyncOperation asyncOp = (AsyncOperation)state;
            Debug.Assert(newDevice != null);
            var rslt = new BluetoothDeviceInfo[] { new BluetoothDeviceInfo(newDevice) };
            Debug.Assert(rslt.Length > 0, "NOT rslt.Length > 0");
            var e = new DiscoverDevicesEventArgs(rslt, asyncOp.UserSuppliedState);
            SendOrPostCallback cb = delegate(object args) {
                OnDiscoveryProgress((DiscoverDevicesEventArgs)args);
            };
            asyncOp.Post(cb, e);
        }

    }

}
