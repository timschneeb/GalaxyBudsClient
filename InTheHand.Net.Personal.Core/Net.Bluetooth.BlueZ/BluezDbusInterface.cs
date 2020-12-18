// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezDbusInterface
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License
#if BlueZ
using System;
using System.Collections.Generic;
using System.Text;
using NDesk.DBus;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    static class BluezDbusInterface
    {
#if !FX3_5
        internal delegate void Action();
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "1")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "2")]
        internal delegate void Action<T1, T2>(T1 arg1, T2 arg2);
#endif


        #region DBus Interfaces
        [Interface("org.bluez.Manager")]
        internal interface Manager
        {
            // -- Methods ----
            /// <summary>
            /// "Returns all global properties." See the
            /// remarks below for available properties.
            /// </summary>
            /// -
            /// <remarks>
            /// <list type="definition">
            /// <item><term>array{object} Adapters [readonly]</term>
            /// <description>List of adapter object paths.</description>
            /// </item>
            /// </list>
            /// </remarks>
            /// -
            /// <returns></returns>
            /// -
            /// <exception>
            /// Possible Errors: org.bluez.Error.DoesNotExist,
            /// org.bluez.Error.InvalidArguments
            /// </exception>
            IDictionary<string, object> GetProperties();

            /// <summary>
            /// Returns object path for the default adapter.
            /// </summary>
            /// -
            /// <returns></returns>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.InvalidArguments
            ///     org.bluez.Error.NoSuchAdapter
            /// </exception>
            ObjectPath DefaultAdapter();

            /// <summary>
            /// Returns object path for the specified adapter."
            /// </summary>
            /// -
            /// <param name="pattern"> "Valid
            /// patterns are "hci0" or "00:11:22:33:44:55".
            /// </param>
            /// -
            /// <returns>ObjectPath</returns>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.InvalidArguments
            ///     org.bluez.Error.NoSuchAdapter
            /// </exception>
            ObjectPath FindAdapter(string pattern);

            //ObjectPath[] ListAdapters(); {deprecated}

            // -- Signals ----
            event Action<string, object> PropertyChanged;
            event Action<ObjectPath> AdapterAdded;
            event Action<ObjectPath> AdapterRemoved;
            event Action<ObjectPath> DefaultAdapterChanged;
        }



        [Interface("org.bluez.Adapter")]
        internal interface Adapter
        {
            // -- Methods ----

            /// <summary>
            /// "Returns all properties for the adapter." See the
            /// remarks below for available properties.
            /// </summary>
            /// -
            /// <remarks>
            /// <list type="definition">
            /// <item><term>
            /// string Address [readonly]
            /// </term><description> The Bluetooth device address.
            /// </description></item>
            /// 
            /// <item><term>string Name [readwrite]
            /// </term><description> The Bluetooth friendly name. This value can be
            ///    changed and a PropertyChanged signal will be emitted.
            /// </description></item>
            /// 
            /// <item><term>uint32 Class [readonly]
            /// </term><description> The Bluetooth class of device.
            /// 
            /// </description></item>
            /// 
            /// <item><term>boolean Powered [readwrite]
            /// </term><description> 
            ///    Switch an adapter on or off. This will also set the
            ///    appropiate connectable state.
            /// 
            /// </description></item>
            /// <item><term>boolean Discoverable [readwrite]
            /// </term><description> 
            ///    Switch an adapter to discoverable or non-discoverable
            ///    to either make it visible or hide it. This is a global
            ///    setting and should only be used by the settings
            ///    application.
            /// 
            ///    If the DiscoverableTimeout is set to a non-zero
            ///    value then the system will set this value back to
            ///    false after the timer expired.
            /// 
            ///    In case the adapter is switched off, setting this
            ///    value will fail.
            /// 
            ///    When changing the Powered property the new state of
            ///    this property will be updated via a PropertyChanged
            ///    signal.
            /// 
            /// </description></item>
            /// <item><term>boolean Pairable [readwrite]
            /// </term><description>Switch an adapter to pairable or non-pairable. This is
            ///    a global setting and should only be used by the
            ///    settings application.
            /// 
            ///    Note that this property only affects incoming pairing
            ///    requests.
            /// </description></item>
            /// <item><term>uint32 PaireableTimeout [readwrite]
            /// </term><description> The pairable timeout in seconds. A value of zero
            ///    means that the timeout is disabled and it will stay in
            ///    pareable mode forever.
            /// </description></item>
            /// <item><term>uint32 DiscoverableTimeout [readwrite]
            /// </term><description> The discoverable timeout in seconds. A value of zero
            ///    means that the timeout is disabled and it will stay in
            ///    discoverable/limited mode forever.
            /// 
            ///    The default value for the discoverable timeout should
            ///    be 180 seconds (3 minutes).
            /// </description></item>
            /// <item><term>boolean Discovering [readonly]
            /// </term><description> Indicates that a device discovery procedure is active.
            /// </description></item>
            /// <item><term>array{object} Devices [readonly]
            /// </term><description> List of device object paths.
            /// </description></item>
            /// 
            /// <item><term>array{string} UUIDs [readonly]
            /// </term><description> List of 128-bit UUIDs that represents the available
            ///    local services.
            /// </description></item>
            /// </list>
            /// </remarks>
            /// -
            /// <exception>
            /// Possible Errors: org.bluez.Error.NotReady
            /// </exception>
            /// 
            /// <returns>As above......
            /// </returns>
            IDictionary<string, object> GetProperties();

            void SetProperty(string propertyName, object value);

            // -- Methods ----
            void RequestSession();
            void ReleaseSession();
            void StartDiscovery();
            void StopDiscovery();

            ObjectPath[] ListDevices();

            ObjectPath CreateDevice(string address);

            // CPD here

            void CancelDeviceCreation(string address);

            //next go below:

            /// <summary>
            /// Returns the object path of device for given address.
            /// The device object needs to be first created via
            /// CreateDevice or CreatePairedDevice.
            /// </summary>
            /// -
            /// <param name="address">address string</param>
            /// -
            /// <returns>ObjectPath</returns>
            /// -
            /// <exception>
            /// Possible Errors: org.bluez.Error.DoesNotExist,
            ///     org.bluez.Error.InvalidArguments
            /// </exception>
            ObjectPath FindDevice(string address);

            /// <summary>
            /// This removes the remote device object at the given
            /// path. It will remove also the pairing information.
            /// </summary>
            /// -
            /// <param name="device"></param>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.InvalidArguments,
            ///     org.bluez.Error.Failed
            /// </exception>
            void RemoveDevice(ObjectPath device);

            // FD here

            //next go above:
            /// <summary>
            /// Creates a new object path for a remote device. This
            /// method will connect to the remote device and retrieve
            /// all SDP records and then initiate the pairing.
            /// </summary>
            /// <remarks>
            /// <para>If previously CreateDevice was used successfully,
            /// this method will only initiate the pairing.
            /// </para><para>Compared to CreateDevice this method will fail if
            /// the pairing already exists, but not if the object
            /// path already has been created. This allows applications
            /// to use CreateDevice first and the if needed use
            /// CreatePairedDevice to initiate pairing.
            /// </para><para>The agent object path is assumed to reside within the
            /// process (D-Bus connection instance) that calls this
            /// method. No separate registration procedure is needed
            /// for it and it gets automatically released once the
            /// pairing operation is complete.
            /// </para><para>The capability parameter is the same as for the
            /// RegisterAgent method.
            /// </para>
            /// </remarks>
            ///<exception>
            /// Possible errors: org.bluez.Error.InvalidArguments
            ///          org.bluez.Error.Failed
            /// </exception>
            ObjectPath CreatePairedDevice(string address, ObjectPath agent,
                string capability);

            /// <summary>
            /// "This registers the adapter wide agent."
            /// </summary>
            /// -
            /// <remarks>
            /// </remarks>
            /// -
            /// <param name="agent">
            /// "The object path defines the path the of the agent
            /// that will be called when user input is needed.
            /// If an application disconnects from the bus all
            /// of its registered agents will be removed."
            /// </param>
            /// <param name="capability">
            /// "The capability parameter can have the values
            /// "DisplayOnly", "DisplayYesNo", "KeyboardOnly" and
            /// "NoInputNoOutput" which reflects the input and output
            /// capabilities of the agent. If an empty string is
            /// used it will fallback to "DisplayYesNo"."
            /// </param>
            /// -
            /// <exception>
            /// </exception>
            void RegisterAgent(ObjectPath agent, string capability);
            void UnregisterAgent(ObjectPath agent);

            // -- Signals ----
            /// <summary>
            /// This signal indicates a changed value of the given
            /// property.
            /// </summary>
            /// -
            /// <remarks>
            /// <para><c>PropertyChanged(string name, variant value)</c>
            /// </para>
            /// </remarks>
            event Action<string, object> PropertyChanged;
            /// <summary>
            /// Parameter is object path of created device.
            /// </summary>
            event Action<ObjectPath> DeviceCreated;
            /// <summary>
            /// Parameter is object path of removed device.
            /// </summary>
            event Action<ObjectPath> DeviceRemoved;
            /// <summary>
            /// This signal will be sent every time an inquiry result
            /// has been found by the service daemon. In general they
            /// only appear during a device discovery.
            /// </summary>
            /// -
            /// <remarks>
            /// <para><c>DeviceFound(string address, dict values)</c>
            /// </para>
            /// <para>The dictionary can contain basically the same values
            /// that are returned by the GetProperties method
            /// from the org.bluez.Device interface. In addition there
            /// can be values for the RSSI and the TX power level.
            /// </para>
            /// </remarks>
            event Action<string, IDictionary<string, object>> DeviceFound;
            /// <summary>
            /// This signal will be sent when an inquiry session for
            /// a periodic discovery finishes and previously found
            /// devices are no longer in range or visible.
            /// </summary>
            /// -
            /// <remarks>
            /// <para><c>DeviceDisappeared(string address)</c>
            /// </para>
            /// </remarks>
            event Action<string> DeviceDisappeared;
        }

        internal enum AgentCapability
        {
            DisplayOnly,
            DisplayYesNo,
            KeyboardOnly,
            NoInputNoOutput
        }


        [Interface("org.bluez.Device")]
        internal interface Device
        {
            // -- Methods ----

            /// <summary>
            /// "Returns all properties for the adapter. See the
            /// properties section for available properties."
            /// </summary>
            /// -
            /// <remarks>
            /// <list type="definition">
            /// <item><term>string Address [readonly]
            /// </term><description> The Bluetooth device address of the remote device.
            /// </description></item>
            /// <item><term>string Name [readonly]
            /// </term><description> The Bluetooth remote name. This value can not be
            /// changed. Use the Alias property instead.
            /// </description></item>
            /// <item><term>string Icon [readonly]
            /// </term><description>Proposed icon name according to the freedesktop.org
            /// icon naming specification.
            /// </description></item>
            /// <item><term>uint32 Class [readonly]
            /// </term><description>The Bluetooth class of device of the remote device.
            /// </description></item>
            /// <item><term>array{string} UUIDs [readonly]
            /// </term><description>List of 128-bit UUIDs that represents the available
            /// remote services.
            /// </description></item>
            /// <item><term>array{object} Services [readonly]
            /// </term><description>List of characteristics based services.
            /// </description></item>
            /// <item><term>boolean Paired [readonly]
            /// </term><description>Indicates if the remote device is paired.
            /// </description></item>
            /// <item><term>boolean Connected [readonly]
            /// </term><description>Indicates if the remote device is currently connected.
            /// A PropertyChanged signal indicate changes to this
            /// status.
            /// </description></item>
            /// <item><term>boolean Trusted [readwrite]
            /// </term><description>Indicates if the remote is seen as trusted. This
            /// setting can be changed by the application.
            /// </description></item>
            /// <item><term>boolean Blocked [readwrite]
            /// </term><description> If set to true any incoming connections from the
            /// device will be immediately rejected. Any device
            /// drivers will also be removed and no new ones will
            /// be probed as long as the device is blocked.
            /// </description></item>
            /// <item><term>string Alias [readwrite]
            /// </term><description> The name alias for the remote device. The alias can
            /// be used to have a different friendly name for the
            /// remote device.
            /// <para>In case no alias is set, it will return the remote
            /// device name. Setting an empty string as alias will
            /// convert it back to the remote device name.
            /// </para>
            /// <para>When reseting the alias with an empty string, the
            /// emitted PropertyChanged signal will show the remote
            /// name again.
            /// </para>
            /// </description></item>
            /// <item><term>array{object} Nodes [readonly]
            /// </term><description> List of device node object paths.
            /// </description></item>
            /// <item><term>object Adapter [readonly]
            /// </term><description> The object path of the adapter the device belongs to.
            /// </description></item>
            /// <item><term>boolean LegacyPairing [readonly]
            /// </term><description> Set to true if the device only supports the pre-2.1
            /// pairing mechanism. This property is useful in the
            /// Adapter.DeviceFound signal to anticipate whether
            /// legacy or simple pairing will occur.
            /// <para>Note that this property can exhibit false-positives
            /// in the case of Bluetooth 2.1 (or newer) devices that
            /// have disabled Extended Inquiry Response support.
            /// </para>
            /// </description></item>
            /// </list>
            /// </remarks>
            /// -
            /// <exception>
            /// Possible Errors: org.bluez.Error.NotReady
            /// </exception>
            /// -
            /// <returns>As above......
            /// </returns>
            IDictionary<string, object> GetProperties();

            void SetProperty(string propertyName, object value);

            IDictionary<uint, string> DiscoverServices(string x);
            void CancelDiscovery();
            void Disconnect();

            // ---- Signals ----
            event Action<string, object> PropertyChanged;
            event Action DisconnectRequested;
        }

        [Interface("org.bluez.Agent")]
        internal interface Agent
        {
            /// <summary>
            /// This method gets called when the service daemon
            /// unregisters the agent. An agent can use it to do
            /// cleanup tasks. There is no need to unregister the
            /// agent, because when this method gets called it has
            /// already been unregistered.
            /// </summary>
            void Release();

            /// <summary>
            /// This method gets called when the service daemon
            /// needs to get the passkey for an authentication.
            /// </summary>
            /// -
            /// <param name="device"></param>
            /// -
            /// <returns>
            /// The return value should be a string of 1-16 characters
            /// length. The string can be alphanumeric.
            /// </returns>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.Rejected,
            /// org.bluez.Error.Canceled
            /// </exception>
            string RequestPinCode(ObjectPath device);

            /// <summary>
            /// This method gets called when the service daemon
            /// needs to get the passkey for an authentication.
            /// </summary>
            /// -
            /// <param name="device"></param>
            /// -
            /// <returns>
            /// The return value should be a numeric value
            /// between 0-999999.
            /// </returns>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.Rejected,
            /// org.bluez.Error.Canceled
            /// </exception>
            UInt32 RequestPasskey(object device);

            /// <summary>
            /// This method gets called when the service daemon
            /// needs to display a passkey for an authentication.
            /// </summary>
            /// -
            /// <remarks>
            /// <para>An empty reply should be returned. When the passkey
            /// needs no longer to be displayed, the Cancel method
            /// of the agent will be called.
            /// </para>
            /// <para>During the pairing process this method might be
            /// called multiple times to update the entered value.
            /// </para>
            /// </remarks>
            /// -
            /// <param name="device"></param>
            /// <param name="passkey"></param>
            /// <param name="entered">
            /// The entered parameter indicates the number of already
            /// typed keys on the remote side.
            /// </param>
            void DisplayPasskey(ObjectPath device, UInt32 passkey, byte entered);

            /// <summary>
            /// This method gets called when the service daemon
            /// needs to confirm a passkey for an authentication.
            /// </summary>
            /// -
            /// <remarks>
            /// To confirm the value it should return an empty reply
            /// or an error in case the passkey is invalid.
            /// </remarks>
            /// -
            /// <param name="device"></param>
            /// <param name="passkey"></param>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.Rejected,
            /// org.bluez.Error.Canceled
            /// </exception>
            void RequestConfirmation(object device, UInt32 passkey);

            /// <summary>
            /// This method gets called when the service daemon
            /// needs to authorize a connection/service request.
            /// </summary>
            /// -
            /// <param name="device"></param>
            /// <param name="uuid"></param>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.Rejected,
            /// org.bluez.Error.Canceled
            /// </exception>
            void Authorize(object device, string uuid);

            /// <summary>
            /// This method gets called if a mode change is requested
            /// that needs to be confirmed by the user. An example
            /// would be leaving flight mode.
            /// </summary>
            /// -
            /// <param name="mode"></param>
            /// -
            /// <exception>
            /// Possible errors: org.bluez.Error.Rejected,
            /// org.bluez.Error.Canceled
            /// </exception>
            void ConfirmModeChange(string mode);

            /// <summary>
            /// This method gets called to indicate that the agent
            /// request failed before a reply was returned.
            /// </summary>
            void Cancel();
        }
        #endregion
    }
}
#endif