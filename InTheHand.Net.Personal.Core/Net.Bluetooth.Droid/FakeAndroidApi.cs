// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if FAKE_ANDROID_BTH_API
using System;
using System.Collections.Generic;
using System.IO;
using Java.Util;

namespace Java.Util
{
    [global::System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class RegisterAttribute : Attribute
    {
        public RegisterAttribute(object x)
        {
        }

        public RegisterAttribute(object x, object y, object z)
        {
        }

        public bool DoNotGenerateAcw { get; set; }

    }

    interface IJavaObject { }
    interface ISerializable { }
    interface ICloseable { }
    interface IParcelable { }
    public interface IParcelableCreator { }

    public class ParcelUuid
    {
        public UUID Uuid { get; private set; }
    }

    //---------------------------------------------

#if !ANDROID
    // Summary:
    //     There are multiple, variant layouts of UUIDs, but this class is based upon
    //     variant 2 of , the Leach-Salz variant.
    //
    // Remarks:
    //     UUID is an immutable representation of a 128-bit universally unique identifier
    //     (UUID).
    //     There are multiple, variant layouts of UUIDs, but this class is based upon
    //     variant 2 of , the Leach-Salz variant. This class can be used to model alternate
    //     variants, but most of the methods will be unsupported in those cases; see
    //     each method for details.
    //     [Android Documentation]
    [Register("java/util/UUID", DoNotGenerateAcw = true)]
    public class UUID : Object, IDisposable, IJavaObject, ISerializable, IComparable
    {
        public void Dispose() { throw new NotImplementedException("MOCKy"); }
        public int CompareTo(object other) { throw new NotImplementedException("MOCKy"); }

#if DEBUG
        public UUID(Guid fake) { }
#endif

        [Register(".ctor", "(JJ)V", "")]
        public UUID(long mostSigBits, long leastSigBits) { throw new NotImplementedException("MOCKy"); }

        // Summary:
        //     The 64 least significant bits of the UUID.
        //
        // Remarks:
        //      The 64 least significant bits of the UUID.
        //     [Android Documentation]
        public long LeastSignificantBits { get { throw new NotImplementedException("MOCKy"); } }
        //
        // Summary:
        //     The 64 most significant bits of the UUID.
        //
        // Remarks:
        //      The 64 most significant bits of the UUID.
        //     [Android Documentation]
        public long MostSignificantBits { get { throw new NotImplementedException("MOCKy"); } }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        /*abstract protected override IntPtr ThresholdClass { get; }*/
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        /*abstract protected override Type ThresholdType { get; }*/

        // Summary:
        //     The clock sequence value of the version 1, variant 2 UUID as per .
        //
        // Returns:
        //      a long value.
        //
        // Exceptions:
        //   Java.Lang.UnsupportedOperationException:
        //     if Java.Util.UUID.Version() is not 1.
        //
        // Remarks:
        //      The clock sequence value of the version 1, variant 2 UUID as per .
        //     [Android Documentation]
        [Register("clockSequence", "()I", "")]
        public int ClockSequence() { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     Compares this UUID to the specified UUID.
        //
        // Parameters:
        //   uuid:
        //     the UUID to compare to.
        //
        // Returns:
        //      a value of -1, 0 or 1 if this UUID is less than, equal to or greater than
        //     uuid.
        //
        // Remarks:
        //      Compares this UUID to the specified UUID. The natural ordering of UUIDs
        //     is based upon the value of the bits from most significant to least significant.
        //     [Android Documentation]
        [Register("compareTo", "(Ljava/util/UUID;)I", "")]
        public int CompareTo(UUID uuid) { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     Parses a UUID string with the format defined by Java.Util.UUID.toString().
        //
        // Parameters:
        //   uuid:
        //     the UUID string to parse.
        //
        // Returns:
        //      an UUID instance.
        //
        // Exceptions:
        //   Java.Lang.NullPointerException:
        //     if uuid is null.
        //
        //   Java.Lang.IllegalArgumentException:
        //     if uuid is not formatted correctly.
        //
        // Remarks:
        //      Parses a UUID string with the format defined by Java.Util.UUID.toString().
        //     [Android Documentation]
        [Register("fromString", "(Ljava/lang/String;)Ljava/util/UUID;", "")]
        public static UUID FromString(string uuid) { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     Generates a variant 2, version 3 (name-based, MD5-hashed) UUID as per .
        //
        // Parameters:
        //   name:
        //     the name used as byte array to create an UUID.
        //
        // Returns:
        //      an UUID instance.
        //
        // Remarks:
        //      Generates a variant 2, version 3 (name-based, MD5-hashed) UUID as per .
        //     [Android Documentation]
        [Register("nameUUIDFromBytes", "([B)Ljava/util/UUID;", "")]
        public static UUID NameUUIDFromBytes(byte[] name) { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     The node value of the version 1, variant 2 UUID as per .
        //
        // Returns:
        //      a long value.
        //
        // Exceptions:
        //   Java.Lang.UnsupportedOperationException:
        //     if Java.Util.UUID.Version() is not 1.
        //
        // Remarks:
        //      The node value of the version 1, variant 2 UUID as per .
        //     [Android Documentation]
        [Register("node", "()J", "")]
        public long Node() { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     Generates a variant 2, version 4 (randomly generated number) UUID as per
        //     .
        //
        // Returns:
        //      an UUID instance.
        //
        // Remarks:
        //      Generates a variant 2, version 4 (randomly generated number) UUID as per
        //     .
        //     [Android Documentation]
        [Register("randomUUID", "()Ljava/util/UUID;", "")]
        public static UUID RandomUUID() { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     The timestamp value of the version 1, variant 2 UUID as per .
        //
        // Returns:
        //      a long value.
        //
        // Exceptions:
        //   Java.Lang.UnsupportedOperationException:
        //     if Java.Util.UUID.Version() is not 1.
        //
        // Remarks:
        //      The timestamp value of the version 1, variant 2 UUID as per .
        //     [Android Documentation]
        [Register("timestamp", "()J", "")]
        public long Timestamp() { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     The variant of the UUID as per .
        //
        // Returns:
        //      an int value.
        //
        // Remarks:
        //      The variant of the UUID as per .  0 - Reserved for NCS compatibility2 -
        //     RFC 4122/Leach-Salz6 - Reserved for Microsoft Corporation compatibility7
        //     - Reserved for future use
        //     [Android Documentation]
        [Register("variant", "()I", "")]
        public int Variant() { throw new NotImplementedException("MOCKy"); }
        //
        // Summary:
        //     The version of the variant 2 UUID as per .
        //
        // Returns:
        //      an int value.
        //
        // Remarks:
        //      The version of the variant 2 UUID as per . If the variant is not 2, then
        //     the version will be 0.  1 - Time-based UUID2 - DCE Security UUID3 - Name-based
        //     with MD5 hashing UUID (Java.Util.UUID.NameUUIDFromBytes(System.Byte[]))4
        //     - Randomly generated UUID (Java.Util.UUID.RandomUUID())5 - Name-based with
        //     SHA-1 hashing UUID
        //     [Android Documentation]
        [Register("version", "()I", "")]
        public int Version() { throw new NotImplementedException("MOCKy"); }
    }
#endif
}


namespace Android.Runtime
{
    class ThisIsADummyClassToKeepVsIntellisenseHappy
    {
    }
}

namespace Android.App
{
    public class Activity
    { }
}

namespace Android.Content
{
    public abstract class Context
    { }
}


namespace Android.Bluetooth
{

    // Summary:
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //
    // Remarks:
    //     Represents the local device Bluetooth adapter. The Android.Bluetooth.BluetoothAdapter
    //     lets you perform fundamental Bluetooth tasks, such as initiate device discovery,
    //     query a list of bonded (paired) devices, instantiate a Android.Bluetooth.BluetoothDevice
    //     using a known MAC address, and create a Android.Bluetooth.BluetoothServerSocket
    //     to listen for connection requests from other devices.
    //     To get a Android.Bluetooth.BluetoothAdapter representing the local Bluetooth
    //     adapter, call the static Android.Bluetooth.BluetoothAdapter.get_DefaultAdapter()
    //     method.  Fundamentally, this is your starting point for all Bluetooth actions.
    //     Once you have the local adapter, you can get a set of Android.Bluetooth.BluetoothDevice
    //     objects representing all paired devices with Android.Bluetooth.BluetoothAdapter.get_BondedDevices();
    //     start device discovery with Android.Bluetooth.BluetoothAdapter.StartDiscovery();
    //     or create a Android.Bluetooth.BluetoothServerSocket to listen for incoming
    //     connection requests with Android.Bluetooth.BluetoothAdapter.ListenUsingRfcommWithServiceRecord(System.String,
    //     Java.Util.UUID).
    //     Note: Most methods require the Android.Manifest.Permission.Bluetooth permission
    //     and some also require the Android.Manifest.Permission.BluetoothAdmin permission.
    //      Developer Guides
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //     See AlsoAndroid.Bluetooth.BluetoothDeviceAndroid.Bluetooth.BluetoothServerSocket
    //     [Android Documentation]
    [Register("android/bluetooth/BluetoothAdapter", DoNotGenerateAcw = true)]
    public abstract class BluetoothAdapter : MarshalByRefObject
    {
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Intent used to broadcast the change in connection state of the local Bluetooth
        //     adapter to a profile of the remote device. When the adapter is not connected
        //     to any profiles of any remote devices and it attempts a connection to a profile
        //     this intent will sent. Once connected, this intent will not be sent for any
        //     more connection attempts to any profiles of any remote device. When the adapter
        //     disconnects from the last profile its connected to of any remote device,
        //     this intent will be sent.
        //     This intent is useful for applications that are only concerned about whether
        //     the local adapter is connected to any profile of any device and are not really
        //     concerned about which profile. For example, an application which displays
        //     an icon to display whether Bluetooth is connected or not can use this intent.
        //     This intent will have 3 extras: Android.Bluetooth.BluetoothAdapter.ExtraConnectionState
        //     - The current connection state.  Android.Bluetooth.BluetoothAdapter.ExtraPreviousConnectionState-
        //     The previous connection state.  Android.Bluetooth.BluetoothDevice.ExtraDevice
        //     - The remote device.  Android.Bluetooth.BluetoothAdapter.ExtraConnectionState
        //     or Android.Bluetooth.BluetoothAdapter.ExtraPreviousConnectionState can be
        //     any of Android.Bluetooth.State.Disconnected, Android.Bluetooth.State.Connecting,
        //     Android.Bluetooth.State.Connected, Android.Bluetooth.State.Disconnecting.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_CONNECTION_STATE_CHANGED")]
        public const string ActionConnectionStateChanged = "android.bluetooth.adapter.action.CONNECTION_STATE_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: The local Bluetooth adapter has finished the device discovery
        //     process.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_DISCOVERY_FINISHED")]
        public const string ActionDiscoveryFinished = "android.bluetooth.adapter.action.DISCOVERY_FINISHED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: The local Bluetooth adapter has started the remote device
        //     discovery process.
        //     This usually involves an inquiry scan of about 12 seconds, followed by a
        //     page scan of each new device to retrieve its Bluetooth name.
        //     Register for Android.Bluetooth.BluetoothDevice.ActionFound to be notified
        //     as remote Bluetooth devices are found.
        //     Device discovery is a heavyweight procedure. New connections to remote Bluetooth
        //     devices should not be attempted while discovery is in progress, and existing
        //     connections will experience limited bandwidth and high latency. Use Android.Bluetooth.BluetoothAdapter.CancelDiscovery()
        //     to cancel an ongoing discovery.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_DISCOVERY_STARTED")]
        public const string ActionDiscoveryStarted = "android.bluetooth.adapter.action.DISCOVERY_STARTED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: The local Bluetooth adapter has changed its friendly Bluetooth
        //     name.
        //     This name is visible to remote Bluetooth devices.
        //     Always contains the extra field Android.Bluetooth.BluetoothAdapter.ExtraLocalName
        //     containing the name.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_LOCAL_NAME_CHANGED")]
        public const string ActionLocalNameChanged = "android.bluetooth.adapter.action.LOCAL_NAME_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Activity Action: Show a system activity that requests discoverable mode.
        //      This activity will also request the user to turn on Bluetooth if it is not
        //     currently enabled.
        //     Discoverable mode is equivalent to Android.Bluetooth.ScanMode.ConnectableDiscoverable.
        //     It allows remote devices to see this Bluetooth adapter when they perform
        //     a discovery.
        //     For privacy, Android is not discoverable by default.
        //     The sender of this Intent can optionally use extra field Android.Bluetooth.BluetoothAdapter.ExtraDiscoverableDuration
        //     to request the duration of discoverability. Currently the default duration
        //     is 120 seconds, and maximum duration is capped at 300 seconds for each request.
        //     Notification of the result of this activity is posted using the Android.App.Activity.OnActivityResult(System.Int32,
        //     Android.App.Result, Android.App.Result) callback. The resultCode will be
        //     the duration (in seconds) of discoverability or Android.App.Result.Canceled
        //     if the user rejected discoverability or an error has occurred.
        //     Applications can also listen for Android.Bluetooth.BluetoothAdapter.ActionScanModeChanged
        //     for global notification whenever the scan mode changes. For example, an application
        //     can be notified when the device has ended discoverability.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("ACTION_REQUEST_DISCOVERABLE")]
        public const string ActionRequestDiscoverable = "android.bluetooth.adapter.action.REQUEST_DISCOVERABLE";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Activity Action: Show a system activity that allows the user to turn on Bluetooth.
        //     This system activity will return once Bluetooth has completed turning on,
        //     or the user has decided not to turn Bluetooth on.
        //     Notification of the result of this activity is posted using the Android.App.Activity.OnActivityResult(System.Int32,
        //     Android.App.Result, Android.App.Result) callback. The resultCode will be
        //     Android.App.Result.Ok if Bluetooth has been turned on or Android.App.Result.Canceled
        //     if the user has rejected the request or an error has occurred.
        //     Applications can also listen for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     for global notification whenever Bluetooth is turned on or off.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("ACTION_REQUEST_ENABLE")]
        public const string ActionRequestEnable = "android.bluetooth.adapter.action.REQUEST_ENABLE";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Broadcast Action: Indicates the Bluetooth scan mode of the local Adapter
        //     has changed.
        //     Always contains the extra fields Android.Bluetooth.BluetoothAdapter.ExtraScanMode
        //     and Android.Bluetooth.BluetoothAdapter.ExtraPreviousScanMode containing the
        //     new and old scan modes respectively.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("ACTION_SCAN_MODE_CHANGED")]
        public const string ActionScanModeChanged = "android.bluetooth.adapter.action.SCAN_MODE_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: The state of the local Bluetooth adapter has been changed.
        //     For example, Bluetooth has been turned on or off.
        //     Always contains the extra fields Android.Bluetooth.BluetoothAdapter.ExtraState
        //     and Android.Bluetooth.BluetoothAdapter.ExtraPreviousState containing the
        //     new and old states respectively.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_STATE_CHANGED")]
        public const string ActionStateChanged = "android.bluetooth.adapter.action.STATE_CHANGED";
        //
        // Summary:
        //     Intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR)
        //
        // Remarks:
        //     Sentinel error value for this class. Guaranteed to not equal any other integer
        //     constant in this class. Provided as a convenience for functions that require
        //     a sentinel error value, for example:
        //     Intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.ERROR)
        //     [Android Documentation]
        [Register("ERROR")]
        public const int Error = -2147483648;
        //
        // Summary:
        //     Extra used by Android.Bluetooth.BluetoothAdapter.ActionConnectionStateChanged
        //     This extra represents the current connection state.
        //
        // Remarks:
        //     Extra used by Android.Bluetooth.BluetoothAdapter.ActionConnectionStateChanged
        //     This extra represents the current connection state.
        //     [Android Documentation]
        [Register("EXTRA_CONNECTION_STATE")]
        public const string ExtraConnectionState = "android.bluetooth.adapter.extra.CONNECTION_STATE";
        //
        // Summary:
        //     Used as an optional int extra field in Android.Bluetooth.BluetoothAdapter.ActionRequestDiscoverable
        //     intents to request a specific duration for discoverability in seconds.
        //
        // Remarks:
        //     Used as an optional int extra field in Android.Bluetooth.BluetoothAdapter.ActionRequestDiscoverable
        //     intents to request a specific duration for discoverability in seconds. The
        //     current default is 120 seconds, and requests over 300 seconds will be capped.
        //     These values could change.
        //     [Android Documentation]
        [Register("EXTRA_DISCOVERABLE_DURATION")]
        public const string ExtraDiscoverableDuration = "android.bluetooth.adapter.extra.DISCOVERABLE_DURATION";
        //
        // Summary:
        //     Used as a String extra field in Android.Bluetooth.BluetoothAdapter.ActionLocalNameChanged
        //     intents to request the local Bluetooth name.
        //
        // Remarks:
        //     Used as a String extra field in Android.Bluetooth.BluetoothAdapter.ActionLocalNameChanged
        //     intents to request the local Bluetooth name.
        //     [Android Documentation]
        [Register("EXTRA_LOCAL_NAME")]
        public const string ExtraLocalName = "android.bluetooth.adapter.extra.LOCAL_NAME";
        //
        // Summary:
        //     Extra used by Android.Bluetooth.BluetoothAdapter.ActionConnectionStateChanged
        //     This extra represents the previous connection state.
        //
        // Remarks:
        //     Extra used by Android.Bluetooth.BluetoothAdapter.ActionConnectionStateChanged
        //     This extra represents the previous connection state.
        //     [Android Documentation]
        [Register("EXTRA_PREVIOUS_CONNECTION_STATE")]
        public const string ExtraPreviousConnectionState = "android.bluetooth.adapter.extra.PREVIOUS_CONNECTION_STATE";
        //
        // Summary:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionScanModeChanged
        //     intents to request the previous scan mode.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionScanModeChanged
        //     intents to request the previous scan mode. Possible values are: Android.Bluetooth.ScanMode.None,
        //     Android.Bluetooth.ScanMode.Connectable, Android.Bluetooth.ScanMode.ConnectableDiscoverable,
        //     [Android Documentation]
        [Register("EXTRA_PREVIOUS_SCAN_MODE")]
        public const string ExtraPreviousScanMode = "android.bluetooth.adapter.extra.PREVIOUS_SCAN_MODE";
        //
        // Summary:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     intents to request the previous power state.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     intents to request the previous power state. Possible values are: Android.Bluetooth.State.Off,
        //     Android.Bluetooth.State.TurningOn, Android.Bluetooth.State.On, Android.Bluetooth.State.TurningOff,
        //     [Android Documentation]
        [Register("EXTRA_PREVIOUS_STATE")]
        public const string ExtraPreviousState = "android.bluetooth.adapter.extra.PREVIOUS_STATE";
        //
        // Summary:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionScanModeChanged
        //     intents to request the current scan mode.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionScanModeChanged
        //     intents to request the current scan mode. Possible values are: Android.Bluetooth.ScanMode.None,
        //     Android.Bluetooth.ScanMode.Connectable, Android.Bluetooth.ScanMode.ConnectableDiscoverable,
        //     [Android Documentation]
        [Register("EXTRA_SCAN_MODE")]
        public const string ExtraScanMode = "android.bluetooth.adapter.extra.SCAN_MODE";
        //
        // Summary:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     intents to request the current power state.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     intents to request the current power state. Possible values are: Android.Bluetooth.State.Off,
        //     Android.Bluetooth.State.TurningOn, Android.Bluetooth.State.On, Android.Bluetooth.State.TurningOff,
        //     [Android Documentation]
        [Register("EXTRA_STATE")]
        public const string ExtraState = "android.bluetooth.adapter.extra.STATE";

        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Returns the hardware address of the local Bluetooth adapter.
        //     For example, "00:11:22:AA:BB:CC".
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public string Address { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Remarks:
        //     Return the set of Android.Bluetooth.BluetoothDevice objects that are bonded
        //     (paired) to the local adapter.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     an empty set. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        abstract public ICollection<BluetoothDevice> BondedDevices { get; }
        //
        // Summary:
        //     Currently Android only supports one Bluetooth adapter, but the API could
        //     be extended to support more.
        //
        // Remarks:
        //     Get a handle to the default local Bluetooth adapter.
        //     Currently Android only supports one Bluetooth adapter, but the API could
        //     be extended to support more. This will always return the default adapter.
        //     [Android Documentation]
        public static BluetoothAdapter DefaultAdapter { get { throw new NotImplementedException("MOCKy"); } }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Remarks:
        //     Return true if the local Bluetooth adapter is currently in the device discovery
        //     process.
        //     Device discovery is a heavyweight procedure. New connections to remote Bluetooth
        //     devices should not be attempted while discovery is in progress, and existing
        //     connections will experience limited bandwidth and high latency. Use Android.Bluetooth.BluetoothAdapter.CancelDiscovery()
        //     to cancel an ongoing discovery.
        //     Applications can also register for Android.Bluetooth.BluetoothAdapter.ActionDiscoveryStarted
        //     or Android.Bluetooth.BluetoothAdapter.ActionDiscoveryFinished to be notified
        //     when discovery starts or completes.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     false. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        abstract public bool IsDiscovering { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Return true if Bluetooth is currently enabled and ready for use.
        //     Equivalent to: getBluetoothState() == STATE_ON
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public bool IsEnabled { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Get the friendly Bluetooth name of the local Bluetooth adapter.
        //     This name is visible to remote Bluetooth devices.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public string Name { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Get the current Bluetooth scan mode of the local Bluetooth adapter.
        //     The Bluetooth scan mode determines if the local adapter is connectable and/or
        //     discoverable from remote Bluetooth devices.
        //     Possible values are: Android.Bluetooth.ScanMode.None, Android.Bluetooth.ScanMode.Connectable,
        //     Android.Bluetooth.ScanMode.ConnectableDiscoverable.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     Android.Bluetooth.ScanMode.None. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public ScanMode ScanMode { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Get the current state of the local Bluetooth adapter.
        //     Possible return values are Android.Bluetooth.State.Off, Android.Bluetooth.State.TurningOn,
        //     Android.Bluetooth.State.On, Android.Bluetooth.State.TurningOff.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public State State { get; }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        /*abstract protected override IntPtr ThresholdClass { get; }*/
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        /*abstract protected override Type ThresholdType { get; }*/

        // Summary:
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     false.
        //
        // Returns:
        //      true on success, false on error
        //
        // Remarks:
        //     Cancel the current device discovery process.
        //     Requires Android.Manifest.Permission.BluetoothAdmin.
        //     Because discovery is a heavyweight procedure for the Bluetooth adapter, this
        //     method should always be called before attempting to connect to a remote device
        //     with Android.Bluetooth.BluetoothSocket.Connect(). Discovery is not managed
        //     by the Activity, but is run as a system service, so an application should
        //     always call cancel discovery even if it did not directly request a discovery,
        //     just to be sure.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     false. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     [Android Documentation]
        [Register("cancelDiscovery", "()Z", "")]
        abstract public bool CancelDiscovery();
        //
        // Summary:
        //     Alphabetic characters must be uppercase to be valid.
        //
        // Parameters:
        //   address:
        //     Bluetooth address as string
        //
        // Returns:
        //      true if the address is valid, false otherwise
        //
        // Remarks:
        //     Validate a String Bluetooth address, such as "00:43:A8:23:10:F0"
        //     Alphabetic characters must be uppercase to be valid.
        //     [Android Documentation]
        [Register("checkBluetoothAddress", "(Ljava/lang/String;)Z", "")]
        public static bool CheckBluetoothAddress(string address)
        {
            InTheHand.Net.BluetoothAddress addr;
            if (!InTheHand.Net.BluetoothAddress.TryParse(address, out addr))
                return false;
            if (address != address.ToUpperInvariant()) {
                // Address not in upper case
                return false;
            }
            return true;
        }
        //
        // Summary:
        //     Clients should call this when they are no longer using the proxy obtained
        //     from Android.Bluetooth.BluetoothAdapter.GetProfileProxy(Android.Content.Context,
        //     Android.Bluetooth.IBluetoothProfileServiceListener, Android.Bluetooth.IBluetoothProfileServiceListener).
        //
        // Parameters:
        //   profile:
        //     To be added.
        //
        //   proxy:
        //     Profile proxy object
        //
        // Remarks:
        //     Close the connection of the profile proxy to the Service.
        //     Clients should call this when they are no longer using the proxy obtained
        //     from Android.Bluetooth.BluetoothAdapter.GetProfileProxy(Android.Content.Context,
        //     Android.Bluetooth.IBluetoothProfileServiceListener, Android.Bluetooth.IBluetoothProfileServiceListener).
        //      Profile can be one of Android.Bluetooth.BluetoothProfile.Health, Android.Bluetooth.BluetoothProfile.Headset
        //     or Android.Bluetooth.BluetoothProfile.A2dp
        //     [Android Documentation]
        /*[Register("closeProfileProxy", "(ILandroid/bluetooth/BluetoothProfile;)V", "")]
        abstract abstract public void CloseProfileProxy(ProfileType profile, IBluetoothProfile proxy);*/
        //
        // Summary:
        //     Bluetooth should never be disabled without direct user consent.
        //
        // Returns:
        //      true to indicate adapter shutdown has begun, or false on immediate error
        //
        // Remarks:
        //     Turn off the local Bluetooth adapter&mdash;do not use without explicit user
        //     action to turn off Bluetooth.
        //     This gracefully shuts down all Bluetooth connections, stops Bluetooth system
        //     services, and powers down the underlying Bluetooth hardware.
        //     Bluetooth should never be disabled without direct user consent. The Android.Bluetooth.BluetoothAdapter.Disable()
        //     method is provided only for applications that include a user interface for
        //     changing system settings, such as a "power manager" app.
        //     This is an asynchronous call: it will return immediately, and clients should
        //     listen for Android.Bluetooth.BluetoothAdapter.ActionStateChanged to be notified
        //     of subsequent adapter state changes. If this call returns true, then the
        //     adapter state will immediately transition from Android.Bluetooth.State.On
        //     to Android.Bluetooth.State.TurningOff, and some time later transition to
        //     either Android.Bluetooth.State.Off or Android.Bluetooth.State.On. If this
        //     call returns false then there was an immediate problem that will prevent
        //     the adapter from being turned off - such as the adapter already being turned
        //     off.
        //     Requires the Android.Manifest.Permission.BluetoothAdmin permission
        //     [Android Documentation]
        [Register("disable", "()Z", "")]
        abstract public bool Disable();
        //
        // Summary:
        //     Bluetooth should never be enabled without direct user consent.
        //
        // Returns:
        //      true to indicate adapter startup has begun, or false on immediate error
        //
        // Remarks:
        //     Turn on the local Bluetooth adapter&mdash;do not use without explicit user
        //     action to turn on Bluetooth.
        //     This powers on the underlying Bluetooth hardware, and starts all Bluetooth
        //     system services.
        //     Bluetooth should never be enabled without direct user consent. If you want
        //     to turn on Bluetooth in order to create a wireless connection, you should
        //     use the Android.Bluetooth.BluetoothAdapter.ActionRequestEnable Intent, which
        //     will raise a dialog that requests user permission to turn on Bluetooth. The
        //     Android.Bluetooth.BluetoothAdapter.Enable() method is provided only for applications
        //     that include a user interface for changing system settings, such as a "power
        //     manager" app.
        //     This is an asynchronous call: it will return immediately, and clients should
        //     listen for Android.Bluetooth.BluetoothAdapter.ActionStateChanged to be notified
        //     of subsequent adapter state changes. If this call returns true, then the
        //     adapter state will immediately transition from Android.Bluetooth.State.Off
        //     to Android.Bluetooth.State.TurningOn, and some time later transition to either
        //     Android.Bluetooth.State.Off or Android.Bluetooth.State.On. If this call returns
        //     false then there was an immediate problem that will prevent the adapter from
        //     being turned on - such as Airplane mode, or the adapter is already turned
        //     on.
        //     Requires the Android.Manifest.Permission.BluetoothAdmin permission
        //     [Android Documentation]
        [Register("enable", "()Z", "")]
        abstract public bool Enable();
        //
        // Summary:
        //     Return value can be one of Android.Bluetooth.BluetoothProfile.STATE_DISCONNECTED,
        //     Android.Bluetooth.BluetoothProfile.STATE_CONNECTING, Android.Bluetooth.BluetoothProfile.STATE_CONNECTED,
        //     Android.Bluetooth.BluetoothProfile.STATE_DISCONNECTING
        //
        // Parameters:
        //   profile:
        //     To be added.
        //
        // Returns:
        //     To be added.
        //
        // Remarks:
        //     Get the current connection state of a profile.  This function can be used
        //     to check whether the local Bluetooth adapter is connected to any remote device
        //     for a specific profile.  Profile can be one of Android.Bluetooth.BluetoothProfile.Health,
        //     Android.Bluetooth.BluetoothProfile.Headset, Android.Bluetooth.BluetoothProfile.A2dp.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     Return value can be one of Android.Bluetooth.BluetoothProfile.STATE_DISCONNECTED,
        //     Android.Bluetooth.BluetoothProfile.STATE_CONNECTING, Android.Bluetooth.BluetoothProfile.STATE_CONNECTED,
        //     Android.Bluetooth.BluetoothProfile.STATE_DISCONNECTING
        //     [Android Documentation]
        /*[Register("getProfileConnectionState", "(I)I", "")]
        abstract public ProfileState GetProfileConnectionState(ProfileType profile);*/
        //
        // Summary:
        //     Profile can be one of Android.Bluetooth.BluetoothProfile.Health, Android.Bluetooth.BluetoothProfile.Headset
        //     or Android.Bluetooth.BluetoothProfile.A2dp.
        //
        // Parameters:
        //   context:
        //     Context of the application
        //
        //   listener:
        //     The service Listener for connection callbacks.
        //
        //   profile:
        //     The Bluetooth profile; either Android.Bluetooth.BluetoothProfile.Health,
        //     Android.Bluetooth.BluetoothProfile.Headset or Android.Bluetooth.BluetoothProfile.A2dp.
        //
        // Returns:
        //      true on success, false on error
        //
        // Remarks:
        //     Get the profile proxy object associated with the profile.
        //     Profile can be one of Android.Bluetooth.BluetoothProfile.Health, Android.Bluetooth.BluetoothProfile.Headset
        //     or Android.Bluetooth.BluetoothProfile.A2dp. Clients must implements Android.Bluetooth.IBluetoothProfileServiceListener
        //     to get notified of the connection status and to get the proxy object.
        //     [Android Documentation]
        /*[Register("getProfileProxy", "(Landroid/content/Context;Landroid/bluetooth/BluetoothProfile$ServiceListener;I)Z", "")]
        public bool GetProfileProxy(Context context, IBluetoothProfileServiceListener listener, ProfileType profile);*/
        //
        // Summary:
        //     A Android.Bluetooth.BluetoothDevice will always be returned for a valid hardware
        //     address, even if this adapter has never seen that device.
        //
        // Parameters:
        //   address:
        //     Bluetooth MAC address (6 bytes)
        //
        // Returns:
        //     To be added.
        //
        // Exceptions:
        //   Java.Lang.IllegalArgumentException:
        //     if address is invalid
        //
        // Remarks:
        //     Get a Android.Bluetooth.BluetoothDevice object for the given Bluetooth hardware
        //     address.
        //     Valid Bluetooth hardware addresses must be 6 bytes. This method expects the
        //     address in network byte order (MSB first).
        //     A Android.Bluetooth.BluetoothDevice will always be returned for a valid hardware
        //     address, even if this adapter has never seen that device.
        //     [Android Documentation]
        [Register("getRemoteDevice", "([B)Landroid/bluetooth/BluetoothDevice;", "")]
        abstract public BluetoothDevice GetRemoteDevice(byte[] address);
        //
        // Summary:
        //     A Android.Bluetooth.BluetoothDevice will always be returned for a valid hardware
        //     address, even if this adapter has never seen that device.
        //
        // Parameters:
        //   address:
        //     valid Bluetooth MAC address
        //
        // Returns:
        //     To be added.
        //
        // Exceptions:
        //   Java.Lang.IllegalArgumentException:
        //     if address is invalid
        //
        // Remarks:
        //     Get a Android.Bluetooth.BluetoothDevice object for the given Bluetooth hardware
        //     address.
        //     Valid Bluetooth hardware addresses must be upper case, in a format such as
        //     "00:11:22:33:AA:BB". The helper Android.Bluetooth.BluetoothAdapter.CheckBluetoothAddress(System.String)
        //     is available to validate a Bluetooth address.
        //     A Android.Bluetooth.BluetoothDevice will always be returned for a valid hardware
        //     address, even if this adapter has never seen that device.
        //     [Android Documentation]
        [Register("getRemoteDevice", "(Ljava/lang/String;)Landroid/bluetooth/BluetoothDevice;", "")]
        abstract public BluetoothDevice GetRemoteDevice(string address);
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Parameters:
        //   p0:
        //     To be added.
        //
        //   p1:
        //     To be added.
        //
        // Returns:
        //      a listening RFCOMM BluetoothServerSocket
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example Bluetooth not available, or insufficient permissions,
        //     or channel in use.
        //
        // Remarks:
        //     Create a listening, insecure RFCOMM Bluetooth socket with Service Record.
        //     The link key is not required to be authenticated, i.e the communication may
        //     be vulnerable to Man In the Middle attacks. For Bluetooth 2.1 devices, the
        //     link will be encrypted, as encryption is mandartory.  For legacy devices
        //     (pre Bluetooth 2.1 devices) the link will not be encrypted. Use Android.Bluetooth.BluetoothAdapter.ListenUsingRfcommWithServiceRecord(System.String,
        //     Java.Util.UUID), if an encrypted and authenticated communication channel
        //     is desired.
        //     Use Android.Bluetooth.BluetoothServerSocket.Accept() to retrieve incoming
        //     connections from a listening Android.Bluetooth.BluetoothServerSocket.
        //     The system will assign an unused RFCOMM channel to listen on.
        //     The system will also register a Service Discovery Protocol (SDP) record with
        //     the local SDP server containing the specified UUID, service name, and auto-assigned
        //     channel. Remote Bluetooth devices can use the same UUID to query our SDP
        //     server and discover which channel to connect to. This SDP record will be
        //     removed when this socket is closed, or if this application closes unexpectedly.
        //     Use Android.Bluetooth.BluetoothDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID)
        //     to connect to this socket from another device using the same Java.Util.UUID.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("listenUsingInsecureRfcommWithServiceRecord", "(Ljava/lang/String;Ljava/util/UUID;)Landroid/bluetooth/BluetoothServerSocket;", "")]
        abstract public BluetoothServerSocket ListenUsingInsecureRfcommWithServiceRecord(string name, UUID uuid);
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Parameters:
        //   name:
        //     service name for SDP record
        //
        //   uuid:
        //     uuid for SDP record
        //
        // Returns:
        //      a listening RFCOMM BluetoothServerSocket
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example Bluetooth not available, or insufficient permissions,
        //     or channel in use.
        //
        // Remarks:
        //     Create a listening, secure RFCOMM Bluetooth socket with Service Record.
        //     A remote device connecting to this socket will be authenticated and communication
        //     on this socket will be encrypted.
        //     Use Android.Bluetooth.BluetoothServerSocket.Accept() to retrieve incoming
        //     connections from a listening Android.Bluetooth.BluetoothServerSocket.
        //     The system will assign an unused RFCOMM channel to listen on.
        //     The system will also register a Service Discovery Protocol (SDP) record with
        //     the local SDP server containing the specified UUID, service name, and auto-assigned
        //     channel. Remote Bluetooth devices can use the same UUID to query our SDP
        //     server and discover which channel to connect to. This SDP record will be
        //     removed when this socket is closed, or if this application closes unexpectedly.
        //     Use Android.Bluetooth.BluetoothDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID)
        //     to connect to this socket from another device using the same Java.Util.UUID.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("listenUsingRfcommWithServiceRecord", "(Ljava/lang/String;Ljava/util/UUID;)Landroid/bluetooth/BluetoothServerSocket;", "")]
        abstract public BluetoothServerSocket ListenUsingRfcommWithServiceRecord(string name, UUID uuid);
        //
        // Summary:
        //     Requires Android.Manifest.Permission.BluetoothAdmin
        //
        // Parameters:
        //   name:
        //     a valid Bluetooth name
        //
        // Returns:
        //      true if the name was set, false otherwise
        //
        // Remarks:
        //     Set the friendly Bluetooth name of the local Bluetooth adapter.
        //     This name is visible to remote Bluetooth devices.
        //     Valid Bluetooth names are a maximum of 248 bytes using UTF-8 encoding, although
        //     many remote devices can only display the first 40 characters, and some may
        //     be limited to just 20.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     false. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     Requires Android.Manifest.Permission.BluetoothAdmin
        //     [Android Documentation]
        [Register("setName", "(Ljava/lang/String;)Z", "")]
        abstract public bool SetName(string name);
        //
        // Summary:
        //     Requires Android.Manifest.Permission.BluetoothAdmin.
        //
        // Returns:
        //      true on success, false on error
        //
        // Remarks:
        //     Start the remote device discovery process.
        //     The discovery process usually involves an inquiry scan of about 12 seconds,
        //     followed by a page scan of each new device to retrieve its Bluetooth name.
        //     This is an asynchronous call, it will return immediately. Register for Android.Bluetooth.BluetoothAdapter.ActionDiscoveryStarted
        //     and Android.Bluetooth.BluetoothAdapter.ActionDiscoveryFinished intents to
        //     determine exactly when the discovery starts and completes. Register for Android.Bluetooth.BluetoothDevice.ActionFound
        //     to be notified as remote Bluetooth devices are found.
        //     Device discovery is a heavyweight procedure. New connections to remote Bluetooth
        //     devices should not be attempted while discovery is in progress, and existing
        //     connections will experience limited bandwidth and high latency. Use Android.Bluetooth.BluetoothAdapter.CancelDiscovery()
        //     to cancel an ongoing discovery. Discovery is not managed by the Activity,
        //     but is run as a system service, so an application should always call Android.Bluetooth.BluetoothAdapter.CancelDiscovery()
        //     even if it did not directly request a discovery, just to be sure.
        //     Device discovery will only find remote devices that are currently discoverable
        //     (inquiry scan enabled). Many Bluetooth devices are not discoverable by default,
        //     and need to be entered into a special mode.
        //     If Bluetooth state is not Android.Bluetooth.State.On, this API will return
        //     false. After turning on Bluetooth, wait for Android.Bluetooth.BluetoothAdapter.ActionStateChanged
        //     with Android.Bluetooth.State.On to get the updated value.
        //     Requires Android.Manifest.Permission.BluetoothAdmin.
        //     [Android Documentation]
        [Register("startDiscovery", "()Z", "")]
        abstract public bool StartDiscovery();
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum ScanMode
    {
        // Summary:
        //     To be added.
        None = 20,
        //
        // Summary:
        //     To be added.
        Connectable = 21,
        //
        // Summary:
        //     To be added.
        ConnectableDiscoverable = 23,
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum State
    {
#if false // API Level 14
                    // Thise four are not Radio state enums they are Profile state enums.
                    // Mono has combined the two into one enum.
        // Summary:
        //     To be added.
        Disconnected = 0,
        //
        // Summary:
        //     To be added.
        Connecting = 1,
        //
        // Summary:
        //     To be added.
        Connected = 2,
        //
        // Summary:
        //     To be added.
        Disconnecting = 3,
#endif
        //
        // Summary:
        //     To be added.
        Off = 10,
        //
        // Summary:
        //     To be added.
        TurningOn = 11,
        //
        // Summary:
        //     To be added.
        On = 12,
        //
        // Summary:
        //     To be added.
        TurningOff = 13,
    }

    //----------------

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    [Register("android/bluetooth/BluetoothDevice", DoNotGenerateAcw = true)]
    public abstract class BluetoothDevice : MarshalByRefObject, IDisposable, IJavaObject, IParcelable
    {
        abstract public void Dispose();

        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Indicates a low level (ACL) connection has been established
        //     with a remote device.
        //     Always contains the extra field Android.Bluetooth.BluetoothDevice.ExtraDevice.
        //     ACL connections are managed automatically by the Android Bluetooth stack.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_ACL_CONNECTED")]
        public const string ActionAclConnected = "android.bluetooth.device.action.ACL_CONNECTED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Indicates a low level (ACL) disconnection from a remote
        //     device.
        //     Always contains the extra field Android.Bluetooth.BluetoothDevice.ExtraDevice.
        //     ACL connections are managed automatically by the Android Bluetooth stack.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_ACL_DISCONNECTED")]
        public const string ActionAclDisconnected = "android.bluetooth.device.action.ACL_DISCONNECTED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Indicates that a low level (ACL) disconnection has been
        //     requested for a remote device, and it will soon be disconnected.
        //     This is useful for graceful disconnection. Applications should use this intent
        //     as a hint to immediately terminate higher level connections (RFCOMM, L2CAP,
        //     or profile connections) to the remote device.
        //     Always contains the extra field Android.Bluetooth.BluetoothDevice.ExtraDevice.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_ACL_DISCONNECT_REQUESTED")]
        public const string ActionAclDisconnectRequested = "android.bluetooth.device.action.ACL_DISCONNECT_REQUESTED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Indicates a change in the bond state of a remote device.
        //     For example, if a device is bonded (paired).
        //     Always contains the extra fields Android.Bluetooth.BluetoothDevice.ExtraDevice,
        //     Android.Bluetooth.BluetoothDevice.ExtraBondState and Android.Bluetooth.BluetoothDevice.ExtraPreviousBondState.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_BOND_STATE_CHANGED")]
        public const string ActionBondStateChanged = "android.bluetooth.device.action.BOND_STATE_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Bluetooth class of a remote device has changed.
        //     Always contains the extra fields Android.Bluetooth.BluetoothDevice.ExtraDevice
        //     and Android.Bluetooth.BluetoothDevice.ExtraClass.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_CLASS_CHANGED")]
        public const string ActionClassChanged = "android.bluetooth.device.action.CLASS_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Remote device discovered.
        //     Sent when a remote device is found during discovery.
        //     Always contains the extra fields Android.Bluetooth.BluetoothDevice.ExtraDevice
        //     and Android.Bluetooth.BluetoothDevice.ExtraClass. Can contain the extra fields
        //     Android.Bluetooth.BluetoothDevice.ExtraName and/or Android.Bluetooth.BluetoothDevice.ExtraRssi
        //     if they are available.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_FOUND")]
        public const string ActionFound = "android.bluetooth.device.action.FOUND";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: Indicates the friendly name of a remote device has been
        //     retrieved for the first time, or changed since the last retrieval.
        //     Always contains the extra fields Android.Bluetooth.BluetoothDevice.ExtraDevice
        //     and Android.Bluetooth.BluetoothDevice.ExtraName.
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_NAME_CHANGED")]
        public const string ActionNameChanged = "android.bluetooth.device.action.NAME_CHANGED";
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //
        // Remarks:
        //     Broadcast Action: This intent is used to broadcast the Java.Util.UUID wrapped
        //     as a Android.OS.ParcelUuid of the remote device after it has been fetched.
        //     This intent is sent only when the UUIDs of the remote device are requested
        //     to be fetched using Service Discovery Protocol
        //     Always contains the extra field Android.Bluetooth.BluetoothDevice.ExtraDevice
        //     Always contains the extra field Android.Bluetooth.BluetoothDevice.ExtraUuid
        //     Requires Android.Manifest.Permission.Bluetooth to receive.
        //     [Android Documentation]
        [Register("ACTION_UUID")]
        public const string ActionUuid = "android.bluetooth.device.action.UUID";
        //
        // Summary:
        //     Intent.getIntExtra(BluetoothDevice.EXTRA_BOND_STATE, BluetoothDevice.ERROR)
        //
        // Remarks:
        //     Sentinel error value for this class. Guaranteed to not equal any other integer
        //     constant in this class. Provided as a convenience for functions that require
        //     a sentinel error value, for example:
        //     Intent.getIntExtra(BluetoothDevice.EXTRA_BOND_STATE, BluetoothDevice.ERROR)
        //     [Android Documentation]
        [Register("ERROR")]
        public const int Error = -2147483648;
        //
        // Summary:
        //     Possible values are: Android.Bluetooth.Bond.None, Android.Bluetooth.Bond.Bonding,
        //     Android.Bluetooth.Bond.Bonded.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothDevice.ActionBondStateChanged
        //     intents.  Contains the bond state of the remote device.
        //     Possible values are: Android.Bluetooth.Bond.None, Android.Bluetooth.Bond.Bonding,
        //     Android.Bluetooth.Bond.Bonded.
        //     [Android Documentation]
        [Register("EXTRA_BOND_STATE")]
        public const string ExtraBondState = "android.bluetooth.device.extra.BOND_STATE";
        //
        // Summary:
        //     Used as a Parcelable Android.Bluetooth.BluetoothClass extra field in Android.Bluetooth.BluetoothDevice.ActionFound
        //     and Android.Bluetooth.BluetoothDevice.ActionClassChanged intents.
        //
        // Remarks:
        //     Used as a Parcelable Android.Bluetooth.BluetoothClass extra field in Android.Bluetooth.BluetoothDevice.ActionFound
        //     and Android.Bluetooth.BluetoothDevice.ActionClassChanged intents.
        //     [Android Documentation]
        [Register("EXTRA_CLASS")]
        public const string ExtraClass = "android.bluetooth.device.extra.CLASS";
        //
        // Summary:
        //     Used as a Parcelable Android.Bluetooth.BluetoothDevice extra field in every
        //     intent broadcast by this class.
        //
        // Remarks:
        //     Used as a Parcelable Android.Bluetooth.BluetoothDevice extra field in every
        //     intent broadcast by this class. It contains the Android.Bluetooth.BluetoothDevice
        //     that the intent applies to.
        //     [Android Documentation]
        [Register("EXTRA_DEVICE")]
        public const string ExtraDevice = "android.bluetooth.device.extra.DEVICE";
        //
        // Summary:
        //     Used as a String extra field in Android.Bluetooth.BluetoothDevice.ActionNameChanged
        //     and Android.Bluetooth.BluetoothDevice.ActionFound intents.
        //
        // Remarks:
        //     Used as a String extra field in Android.Bluetooth.BluetoothDevice.ActionNameChanged
        //     and Android.Bluetooth.BluetoothDevice.ActionFound intents. It contains the
        //     friendly Bluetooth name.
        //     [Android Documentation]
        [Register("EXTRA_NAME")]
        public const string ExtraName = "android.bluetooth.device.extra.NAME";
        //
        // Summary:
        //     Possible values are: Android.Bluetooth.Bond.None, Android.Bluetooth.Bond.Bonding,
        //     Android.Bluetooth.Bond.Bonded.
        //
        // Remarks:
        //     Used as an int extra field in Android.Bluetooth.BluetoothDevice.ActionBondStateChanged
        //     intents.  Contains the previous bond state of the remote device.
        //     Possible values are: Android.Bluetooth.Bond.None, Android.Bluetooth.Bond.Bonding,
        //     Android.Bluetooth.Bond.Bonded.
        //     [Android Documentation]
        [Register("EXTRA_PREVIOUS_BOND_STATE")]
        public const string ExtraPreviousBondState = "android.bluetooth.device.extra.PREVIOUS_BOND_STATE";
        //
        // Summary:
        //     Used as an optional short extra field in Android.Bluetooth.BluetoothDevice.ActionFound
        //     intents.
        //
        // Remarks:
        //     Used as an optional short extra field in Android.Bluetooth.BluetoothDevice.ActionFound
        //     intents.  Contains the RSSI value of the remote device as reported by the
        //     Bluetooth hardware.
        //     [Android Documentation]
        [Register("EXTRA_RSSI")]
        public const string ExtraRssi = "android.bluetooth.device.extra.RSSI";
        //
        // Summary:
        //     Used as an extra field in Android.Bluetooth.BluetoothDevice.ActionUuid intents,
        //     Contains the Android.OS.ParcelUuids of the remote device which is a parcelable
        //     version of Java.Util.UUID.
        //
        // Remarks:
        //     Used as an extra field in Android.Bluetooth.BluetoothDevice.ActionUuid intents,
        //     Contains the Android.OS.ParcelUuids of the remote device which is a parcelable
        //     version of Java.Util.UUID.
        //     [Android Documentation]
        [Register("EXTRA_UUID")]
        public const string ExtraUuid = "android.bluetooth.device.extra.UUID";

        // Summary:
        //     For example, "00:11:22:AA:BB:CC".
        //
        // Remarks:
        //     Returns the hardware address of this BluetoothDevice.
        //     For example, "00:11:22:AA:BB:CC".
        //     [Android Documentation]
        abstract public string Address { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Remarks:
        //     Get the Bluetooth class of the remote device.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        abstract public BluetoothClass BluetoothClass { get; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Remarks:
        //     Get the bond state of the remote device.
        //     Possible values for the bond state are: Android.Bluetooth.Bond.None, Android.Bluetooth.Bond.Bonding,
        //     Android.Bluetooth.Bond.Bonded.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        abstract public Bond BondState { get; }
        //
        // Summary:
        //      [Android Documentation]
        //
        // Remarks:
        //      [Android Documentation]
        [Register("CREATOR")]
        public static IParcelableCreator Creator { get; set; }
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Remarks:
        //     Get the friendly Bluetooth name of the remote device.
        //     The local adapter will automatically retrieve remote names when performing
        //     a device scan, and will cache them. This method just returns the name for
        //     this device from the cache.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        abstract public string Name { get; }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        /*abstract protected override IntPtr ThresholdClass { get; }*/
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        /*abstract protected override Type ThresholdType { get; }*/

        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Parameters:
        //   p0:
        //     To be added.
        //
        // Returns:
        //      a RFCOMM BluetoothServerSocket ready for an outgoing connection
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example Bluetooth not available, or insufficient permissions
        //
        // Remarks:
        //     Create an RFCOMM Android.Bluetooth.BluetoothSocket socket ready to start
        //     an insecure outgoing connection to this remote device using SDP lookup of
        //     uuid.
        //     The communication channel will not have an authenticated link key i.e it
        //     will be subject to man-in-the-middle attacks. For Bluetooth 2.1 devices,
        //     the link key will be encrypted, as encryption is mandatory.  For legacy devices
        //     (pre Bluetooth 2.1 devices) the link key will be not be encrypted. Use Android.Bluetooth.BluetoothDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID)
        //     if an encrypted and authenticated communication channel is desired.
        //     This is designed to be used with Android.Bluetooth.BluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord(System.String,
        //     Java.Util.UUID) for peer-peer Bluetooth applications.
        //     Use Android.Bluetooth.BluetoothSocket.Connect() to initiate the outgoing
        //     connection. This will also perform an SDP lookup of the given uuid to determine
        //     which channel to connect to.
        //     The remote device will be authenticated and communication on this socket
        //     will be encrypted.
        //     Hint: If you are connecting to a Bluetooth serial board then try using the
        //     well-known SPP UUID 00001101-0000-1000-8000-00805F9B34FB.  However if you
        //     are connecting to an Android peer then please generate your own unique UUID.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("createInsecureRfcommSocketToServiceRecord", "(Ljava/util/UUID;)Landroid/bluetooth/BluetoothSocket;", "")]
        abstract public BluetoothSocket CreateInsecureRfcommSocketToServiceRecord(UUID uuid);
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth
        //
        // Parameters:
        //   uuid:
        //     service record uuid to lookup RFCOMM channel
        //
        // Returns:
        //      a RFCOMM BluetoothServerSocket ready for an outgoing connection
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example Bluetooth not available, or insufficient permissions
        //
        // Remarks:
        //     Create an RFCOMM Android.Bluetooth.BluetoothSocket ready to start a secure
        //     outgoing connection to this remote device using SDP lookup of uuid.
        //     This is designed to be used with Android.Bluetooth.BluetoothAdapter.ListenUsingRfcommWithServiceRecord(System.String,
        //     Java.Util.UUID) for peer-peer Bluetooth applications.
        //     Use Android.Bluetooth.BluetoothSocket.Connect() to initiate the outgoing
        //     connection. This will also perform an SDP lookup of the given uuid to determine
        //     which channel to connect to.
        //     The remote device will be authenticated and communication on this socket
        //     will be encrypted.
        //     Use this socket only if an authenticated socket link is possible.  Authentication
        //     refers to the authentication of the link key to prevent man-in-the-middle
        //     type of attacks.  For example, for Bluetooth 2.1 devices, if any of the devices
        //     does not have an input and output capability or just has the ability to display
        //     a numeric key, a secure socket connection is not possible.  In such a case,
        //     use {#link createInsecureRfcommSocketToServiceRecord}.  For more details,
        //     refer to the Security Model section 5.2 (vol 3) of Bluetooth Core Specification
        //     version 2.1 + EDR.
        //     Hint: If you are connecting to a Bluetooth serial board then try using the
        //     well-known SPP UUID 00001101-0000-1000-8000-00805F9B34FB.  However if you
        //     are connecting to an Android peer then please generate your own unique UUID.
        //     Requires Android.Manifest.Permission.Bluetooth
        //     [Android Documentation]
        [Register("createRfcommSocketToServiceRecord", "(Ljava/util/UUID;)Landroid/bluetooth/BluetoothSocket;", "")]
        abstract public BluetoothSocket CreateRfcommSocketToServiceRecord(UUID uuid);
        //
        // Summary:
        //     Describe the kinds of special objects contained in this Parcelable's marshalled
        //     representation.
        //
        // Returns:
        //      a bitmask indicating the set of special object types marshalled by the Parcelable.
        //
        // Remarks:
        //     Describe the kinds of special objects contained in this Parcelable's marshalled
        //     representation.
        //     [Android Documentation]
        [Register("describeContents", "()I", "")]
        abstract public int DescribeContents();
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Returns:
        //      False if the sanity check fails, True if the process of initiating an ACL
        //     connection to the remote device was started.
        //
        // Remarks:
        //     Perform a service discovery on the remote device to get the UUIDs supported.
        //     This API is asynchronous and Android.Bluetooth.BluetoothDevice.ActionUuid
        //     intent is sent, with the UUIDs supported by the remote end. If there is an
        //     error in getting the SDP records or if the process takes a long time, Android.Bluetooth.BluetoothDevice.ActionUuid
        //     intent is sent with the UUIDs that is currently present in the cache. Clients
        //     should use the Android.Bluetooth.BluetoothDevice.GetUuids() to get UUIDs
        //     if service discovery is not to be performed.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        [Register("fetchUuidsWithSdp", "()Z", "")]
        abstract public bool FetchUuidsWithSdp();
        //
        // Summary:
        //     Requires Android.Manifest.Permission.Bluetooth.
        //
        // Returns:
        //      the supported features (UUIDs) of the remote device, or null on error
        //
        // Remarks:
        //     Returns the supported features (UUIDs) of the remote device.
        //     This method does not start a service discovery procedure to retrieve the
        //     UUIDs from the remote device. Instead, the local cached copy of the service
        //     UUIDs are returned.
        //     Use Android.Bluetooth.BluetoothDevice.FetchUuidsWithSdp() if fresh UUIDs
        //     are desired.
        //     Requires Android.Manifest.Permission.Bluetooth.
        //     [Android Documentation]
        [Register("getUuids", "()[Landroid/os/ParcelUuid;", "")]
        abstract public ParcelUuid[] GetUuids();
        //
        // Summary:
        //     Flatten this object in to a Parcel.
        //
        // Parameters:
        //   out:
        //     The Parcel in which the object should be written.
        //
        //   flags:
        //     Additional flags about how the object should be written.  May be 0 or Android.OS.Parcelable.ParcelableWriteReturnValue.
        //
        // Remarks:
        //     Flatten this object in to a Parcel.
        //     [Android Documentation]
        /*[Register("writeToParcel", "(Landroid/os/Parcel;I)V", "")]
        abstract public void WriteToParcel(Parcel @out, ParcelableWriteFlags flags);

        public static class InterfaceConsts
        {
            [Register("CONTENTS_FILE_DESCRIPTOR")]
            public const int ContentsFileDescriptor = 1;
            [Obsolete("This constant will be removed in the future version. Use Android.OS.ParcelableWriteFlags enum directly instead of this field.")]
            [Register("PARCELABLE_WRITE_RETURN_VALUE")]
            public const ParcelableWriteFlags ParcelableWriteReturnValue = 1;
        }*/
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum Bond
    {
        // Summary:
        //     To be added.
        None = 10,
        //
        // Summary:
        //     To be added.
        Bonding = 11,
        //
        // Summary:
        //     To be added.
        Bonded = 12,
    }

    // Summary:
    //     Each Android.Bluetooth.BluetoothClass encodes zero or more service classes.
    //
    // Remarks:
    //     Defines all service class constants.
    //     Each Android.Bluetooth.BluetoothClass encodes zero or more service classes.
    //     [Android Documentation]
    [Register("android/bluetooth/BluetoothClass", DoNotGenerateAcw = true)]
    public abstract class BluetoothClass : MarshalByRefObject, IDisposable, IJavaObject, IParcelable
    {
        abstract public void Dispose();

        // Summary:
        //      [Android Documentation]
        //
        // Remarks:
        //      [Android Documentation]
        [Register("CREATOR")]
        public static IParcelableCreator Creator { get; set; }
        //
        // Summary:
        //     Values returned from this function can be compared with the public constants
        //     in Android.Bluetooth.BluetoothClass+Device to determine which device class
        //     is encoded in this Bluetooth class.
        //
        // Remarks:
        //     Return the (major and minor) device class component of this Android.Bluetooth.BluetoothClass.
        //     Values returned from this function can be compared with the public constants
        //     in Android.Bluetooth.BluetoothClass+Device to determine which device class
        //     is encoded in this Bluetooth class.
        //     [Android Documentation]
        abstract public DeviceClass DeviceClass { get; }
        //
        // Summary:
        //     Values returned from this function can be compared with the public constants
        //     in Android.Bluetooth.BluetoothClass+Device+Major to determine which major
        //     class is encoded in this Bluetooth class.
        //
        // Remarks:
        //     Return the major device class component of this Android.Bluetooth.BluetoothClass.
        //     Values returned from this function can be compared with the public constants
        //     in Android.Bluetooth.BluetoothClass+Device+Major to determine which major
        //     class is encoded in this Bluetooth class.
        //     [Android Documentation]
        abstract public MajorDeviceClass MajorDeviceClass { get; }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        /*abstract protected override IntPtr ThresholdClass { get; }*/
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        /*abstract protected override Type ThresholdType { get; }*/

        // Summary:
        //     Describe the kinds of special objects contained in this Parcelable's marshalled
        //     representation.
        //
        // Returns:
        //      a bitmask indicating the set of special object types marshalled by the Parcelable.
        //
        // Remarks:
        //     Describe the kinds of special objects contained in this Parcelable's marshalled
        //     representation.
        //     [Android Documentation]
        [Register("describeContents", "()I", "")]
        abstract public int DescribeContents();
        //
        // Summary:
        //     Valid service classes are the public constants in Android.Bluetooth.BluetoothClass+Service.
        //
        // Parameters:
        //   service:
        //     valid service class
        //
        // Returns:
        //      true if the service class is supported
        //
        // Remarks:
        //     Return true if the specified service class is supported by this Android.Bluetooth.BluetoothClass.
        //     Valid service classes are the public constants in Android.Bluetooth.BluetoothClass+Service.
        //     For example, Android.Bluetooth.ServiceClass.Audio.
        //     [Android Documentation]
        [Register("hasService", "(I)Z", "")]
        abstract public bool HasService(ServiceClass service);
        //
        // Summary:
        //     Flatten this object in to a Parcel.
        //
        // Parameters:
        //   out:
        //     The Parcel in which the object should be written.
        //
        //   flags:
        //     Additional flags about how the object should be written.  May be 0 or Android.OS.Parcelable.ParcelableWriteReturnValue.
        //
        // Remarks:
        //     Flatten this object in to a Parcel.
        //     [Android Documentation]
        /*[Register("writeToParcel", "(Landroid/os/Parcel;I)V", "")]
        abstract public void WriteToParcel(Parcel @out, ParcelableWriteFlags flags);

        [Register("android/bluetooth/BluetoothClass$Device", DoNotGenerateAcw = true)]
        public class Device : Object
        {
            [Register(".ctor", "()V", "")]
            public Device();
            protected Device(IntPtr javaReference, JniHandleOwnership transfer);

            protected override IntPtr ThresholdClass { get; }
            protected override Type ThresholdType { get; }

            [Register("android/bluetooth/BluetoothClass$Device$Major", DoNotGenerateAcw = true)]
            public class Major : Object
            {
                [Register(".ctor", "()V", "")]
                public Major();
                protected Major(IntPtr javaReference, JniHandleOwnership transfer);

                protected override IntPtr ThresholdClass { get; }
                protected override Type ThresholdType { get; }
            }
        }*/

        /*public static class InterfaceConsts
        {
            [Register("CONTENTS_FILE_DESCRIPTOR")]
            public const int ContentsFileDescriptor = 1;
            [Register("PARCELABLE_WRITE_RETURN_VALUE")]
            [Obsolete("This constant will be removed in the future version. Use Android.OS.ParcelableWriteFlags enum directly instead of this field.")]
            public const ParcelableWriteFlags ParcelableWriteReturnValue = 1;
        }*/

        /*[Register("android/bluetooth/BluetoothClass$Service", DoNotGenerateAcw = true)]
        public sealed class Service : Object
        {
            [Register(".ctor", "()V", "")]
            public Service();

            protected override IntPtr ThresholdClass { get; }
            protected override Type ThresholdType { get; }
        }*/
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum MajorDeviceClass
    {
        // Summary:
        //     To be added.
        Misc = 0,
        //
        // Summary:
        //     To be added.
        Computer = 256,
        //
        // Summary:
        //     To be added.
        Phone = 512,
        //
        // Summary:
        //     To be added.
        Networking = 768,
        //
        // Summary:
        //     To be added.
        AudioVideo = 1024,
        //
        // Summary:
        //     To be added.
        Peripheral = 1280,
        //
        // Summary:
        //     To be added.
        Imaging = 1536,
        //
        // Summary:
        //     To be added.
        Wearable = 1792,
        //
        // Summary:
        //     To be added.
        Toy = 2048,
        //
        // Summary:
        //     To be added.
        Health = 2304,
        //
        // Summary:
        //     To be added.
        Uncategorized = 7936,
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum DeviceClass
    {
        // Summary:
        //     To be added.
        ComputerUncategorized = 256,
        //
        // Summary:
        //     To be added.
        ComputerDesktop = 260,
        //
        // Summary:
        //     To be added.
        ComputerServer = 264,
        //
        // Summary:
        //     To be added.
        ComputerLaptop = 268,
        //
        // Summary:
        //     To be added.
        ComputerHandheldPcPda = 272,
        //
        // Summary:
        //     To be added.
        ComputerPalmSizePcPda = 276,
        //
        // Summary:
        //     To be added.
        ComputerWearable = 280,
        //
        // Summary:
        //     To be added.
        PhoneUncategorized = 512,
        //
        // Summary:
        //     To be added.
        PhoneCellular = 516,
        //
        // Summary:
        //     To be added.
        PhoneCordless = 520,
        //
        // Summary:
        //     To be added.
        PhoneSmart = 524,
        //
        // Summary:
        //     To be added.
        PhoneModemOrGateway = 528,
        //
        // Summary:
        //     To be added.
        PhoneIsdn = 532,
        //
        // Summary:
        //     To be added.
        AudioVideoUncategorized = 1024,
        //
        // Summary:
        //     To be added.
        AudioVideoWearableHeadset = 1028,
        //
        // Summary:
        //     To be added.
        AudioVideoHandsfree = 1032,
        //
        // Summary:
        //     To be added.
        AudioVideoMicrophone = 1040,
        //
        // Summary:
        //     To be added.
        AudioVideoLoudspeaker = 1044,
        //
        // Summary:
        //     To be added.
        AudioVideoHeadphones = 1048,
        //
        // Summary:
        //     To be added.
        AudioVideoPortableAudio = 1052,
        //
        // Summary:
        //     To be added.
        AudioVideoCarAudio = 1056,
        //
        // Summary:
        //     To be added.
        AudioVideoSetTopBox = 1060,
        //
        // Summary:
        //     To be added.
        AudioVideoHifiAudio = 1064,
        //
        // Summary:
        //     To be added.
        AudioVideoVcr = 1068,
        //
        // Summary:
        //     To be added.
        AudioVideoVideoCamera = 1072,
        //
        // Summary:
        //     To be added.
        AudioVideoCamcorder = 1076,
        //
        // Summary:
        //     To be added.
        AudioVideoVideoMonitor = 1080,
        //
        // Summary:
        //     To be added.
        AudioVideoVideoDisplayAndLoudspeaker = 1084,
        //
        // Summary:
        //     To be added.
        AudioVideoVideoConferencing = 1088,
        //
        // Summary:
        //     To be added.
        AudioVideoVideoGamingToy = 1096,
        //
        // Summary:
        //     To be added.
        WearableUncategorized = 1792,
        //
        // Summary:
        //     To be added.
        WearableWristWatch = 1796,
        //
        // Summary:
        //     To be added.
        WearablePager = 1800,
        //
        // Summary:
        //     To be added.
        WearableJacket = 1804,
        //
        // Summary:
        //     To be added.
        WearableHelmet = 1808,
        //
        // Summary:
        //     To be added.
        WearableGlasses = 1812,
        //
        // Summary:
        //     To be added.
        ToyUncategorized = 2048,
        //
        // Summary:
        //     To be added.
        ToyRobot = 2052,
        //
        // Summary:
        //     To be added.
        ToyVehicle = 2056,
        //
        // Summary:
        //     To be added.
        ToyDollActionFigure = 2060,
        //
        // Summary:
        //     To be added.
        ToyController = 2064,
        //
        // Summary:
        //     To be added.
        ToyGame = 2068,
        //
        // Summary:
        //     To be added.
        HealthUncategorized = 2304,
        //
        // Summary:
        //     To be added.
        HealthBloodPressure = 2308,
        //
        // Summary:
        //     To be added.
        HealthThermometer = 2312,
        //
        // Summary:
        //     To be added.
        HealthWeighing = 2316,
        //
        // Summary:
        //     To be added.
        HealthGlucose = 2320,
        //
        // Summary:
        //     To be added.
        HealthPulseOximeter = 2324,
        //
        // Summary:
        //     To be added.
        HealthPulseRate = 2328,
        //
        // Summary:
        //     To be added.
        HealthDataDisplay = 2332,
    }

    // Summary:
    //     To be added.
    //
    // Remarks:
    //     To be added.
    public enum ServiceClass
    {
        // Summary:
        //     To be added.
        LimitedDiscoverability = 8192,
        //
        // Summary:
        //     To be added.
        Positioning = 65536,
        //
        // Summary:
        //     To be added.
        Networking = 131072,
        //
        // Summary:
        //     To be added.
        Render = 262144,
        //
        // Summary:
        //     To be added.
        Capture = 524288,
        //
        // Summary:
        //     To be added.
        ObjectTransfer = 1048576,
        //
        // Summary:
        //     To be added.
        Audio = 2097152,
        //
        // Summary:
        //     To be added.
        Telephony = 4194304,
        //
        // Summary:
        //     To be added.
        Information = 8388608,
    }

    //--------------

    // Summary:
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //
    // Remarks:
    //     A connected or connecting Bluetooth socket.
    //     The interface for Bluetooth Sockets is similar to that of TCP sockets: Java.Net.Socket
    //     and Java.Net.ServerSocket. On the server side, use a Android.Bluetooth.BluetoothServerSocket
    //     to create a listening server socket. When a connection is accepted by the
    //     Android.Bluetooth.BluetoothServerSocket, it will return a new Android.Bluetooth.BluetoothSocket
    //     to manage the connection.  On the client side, use a single Android.Bluetooth.BluetoothSocket
    //     to both initiate an outgoing connection and to manage the connection.
    //     The most common type of Bluetooth socket is RFCOMM, which is the type supported
    //     by the Android APIs. RFCOMM is a connection-oriented, streaming transport
    //     over Bluetooth. It is also known as the Serial Port Profile (SPP).
    //     To create a Android.Bluetooth.BluetoothSocket for connecting to a known device,
    //     use Android.Bluetooth.BluetoothDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID).
    //      Then call Android.Bluetooth.BluetoothSocket.Connect() to attempt a connection
    //     to the remote device.  This call will block until a connection is established
    //     or the connection fails.
    //     To create a Android.Bluetooth.BluetoothSocket as a server (or "host"), see
    //     the Android.Bluetooth.BluetoothServerSocket documentation.
    //     Once the socket is connected, whether initiated as a client or accepted as
    //     a server, open the IO streams by calling Android.Bluetooth.BluetoothSocket.get_InputStream()
    //     and Android.Bluetooth.BluetoothSocket.get_OutputStream() in order to retrieve
    //     Java.IO.InputStream and Java.IO.OutputStream objects, respectively, which
    //     are automatically connected to the socket.
    //     Android.Bluetooth.BluetoothSocket is thread safe. In particular, Android.Bluetooth.BluetoothSocket.Close()
    //     will always immediately abort ongoing operations and close the socket.
    //     Note: Requires the Android.Manifest.Permission.Bluetooth permission.  Developer
    //     Guides
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //     See AlsoAndroid.Bluetooth.BluetoothServerSocketJava.IO.InputStreamJava.IO.OutputStream
    //     [Android Documentation]
    [Register("android/bluetooth/BluetoothSocket", DoNotGenerateAcw = true)]
    public abstract class BluetoothSocket : Object, IDisposable, IJavaObject, ICloseable
    {
        abstract public void Dispose();

        // Summary:
        //     The input stream will be returned even if the socket is not yet connected,
        //     but operations on that stream will throw IOException until the associated
        //     socket is connected.
        //
        // Exceptions:
        //   Java.IO.IOException:
        //
        // Remarks:
        //     Get the input stream associated with this socket.
        //     The input stream will be returned even if the socket is not yet connected,
        //     but operations on that stream will throw IOException until the associated
        //     socket is connected.
        //     [Android Documentation]
        abstract public Stream InputStream { get; }
#if ANDROID_API_LEVEL_14
        //
        // Summary:
        //     Get the connection status of this socket, ie, whether there is an active
        //     connection with remote device.
        //
        // Remarks:
        //     Get the connection status of this socket, ie, whether there is an active
        //     connection with remote device.
        //     [Android Documentation]
        abstract public bool IsConnected { get; }
#endif
        //
        // Summary:
        //     The output stream will be returned even if the socket is not yet connected,
        //     but operations on that stream will throw IOException until the associated
        //     socket is connected.
        //
        // Exceptions:
        //   Java.IO.IOException:
        //
        // Remarks:
        //     Get the output stream associated with this socket.
        //     The output stream will be returned even if the socket is not yet connected,
        //     but operations on that stream will throw IOException until the associated
        //     socket is connected.
        //     [Android Documentation]
        abstract public Stream OutputStream { get; }
        //
        // Summary:
        //     Get the remote device this socket is connecting, or connected, to.
        //
        // Remarks:
        //     Get the remote device this socket is connecting, or connected, to.
        //     [Android Documentation]
        abstract public BluetoothDevice RemoteDevice { get; }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        /*protected override IntPtr ThresholdClass { get; }*/
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        /*protected override Type ThresholdType { get; }*/

        // Summary:
        //     Although only the first call has any effect, it is safe to call close multiple
        //     times on the same object.
        //
        // Exceptions:
        //   Java.IO.IOException:
        //
        // Remarks:
        //     Closes the object and release any system resources it holds.
        //     Although only the first call has any effect, it is safe to call close multiple
        //     times on the same object. This is more lenient than the overridden AutoCloseable.close(),
        //     which may be called at most once.
        //     [Android Documentation]
        [Register("close", "()V", "")]
        abstract public void Close();
        //
        // Summary:
        //      Android.Bluetooth.BluetoothSocket.Close() can be used to abort this call
        //     from another thread.
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example connection failure
        //
        // Remarks:
        //     Attempt to connect to a remote device.
        //     This method will block until a connection is made or the connection fails.
        //     If this method returns without an exception then this socket is now connected.
        //     Creating new connections to remote Bluetooth devices should not be attempted
        //     while device discovery is in progress. Device discovery is a heavyweight
        //     procedure on the Bluetooth adapter and will significantly slow a device connection.
        //      Use Android.Bluetooth.BluetoothAdapter.CancelDiscovery() to cancel an ongoing
        //     discovery. Discovery is not managed by the Activity, but is run as a system
        //     service, so an application should always call Android.Bluetooth.BluetoothAdapter.CancelDiscovery()
        //     even if it did not directly request a discovery, just to be sure.
        //     Android.Bluetooth.BluetoothSocket.Close() can be used to abort this call
        //     from another thread.
        //     [Android Documentation]
        [Register("connect", "()V", "")]
        abstract public void Connect();
    }


    // Summary:
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //
    // Remarks:
    //     A listening Bluetooth socket.
    //     The interface for Bluetooth Sockets is similar to that of TCP sockets: Java.Net.Socket
    //     and Java.Net.ServerSocket. On the server side, use a Android.Bluetooth.BluetoothServerSocket
    //     to create a listening server socket. When a connection is accepted by the
    //     Android.Bluetooth.BluetoothServerSocket, it will return a new Android.Bluetooth.BluetoothSocket
    //     to manage the connection.  On the client side, use a single Android.Bluetooth.BluetoothSocket
    //     to both initiate an outgoing connection and to manage the connection.
    //     The most common type of Bluetooth socket is RFCOMM, which is the type supported
    //     by the Android APIs. RFCOMM is a connection-oriented, streaming transport
    //     over Bluetooth. It is also known as the Serial Port Profile (SPP).
    //     To create a listening Android.Bluetooth.BluetoothServerSocket that's ready
    //     for incoming connections, use Android.Bluetooth.BluetoothAdapter.ListenUsingRfcommWithServiceRecord(System.String,
    //     Java.Util.UUID). Then call Android.Bluetooth.BluetoothServerSocket.Accept()
    //     to listen for incoming connection requests. This call will block until a
    //     connection is established, at which point, it will return a Android.Bluetooth.BluetoothSocket
    //     to manage the connection. Once the Android.Bluetooth.BluetoothSocket is acquired,
    //     it's a good idea to call Android.Bluetooth.BluetoothServerSocket.Close()
    //     on the Android.Bluetooth.BluetoothServerSocket when it's no longer needed
    //     for accepting connections. Closing the Android.Bluetooth.BluetoothServerSocket
    //     will not close the returned Android.Bluetooth.BluetoothSocket.
    //     Android.Bluetooth.BluetoothServerSocket is thread safe. In particular, Android.Bluetooth.BluetoothServerSocket.Close()
    //     will always immediately abort ongoing operations and close the server socket.
    //     Note: Requires the Android.Manifest.Permission.Bluetooth permission.  Developer
    //     Guides
    //     For more information about using Bluetooth, read the Bluetooth developer
    //     guide.
    //     See AlsoAndroid.Bluetooth.BluetoothSocket
    //     [Android Documentation]
    [Register("android/bluetooth/BluetoothServerSocket", DoNotGenerateAcw = true)]
    public abstract class BluetoothServerSocket : Object, IDisposable, IJavaObject, ICloseable
    {
        public void Dispose() { throw new NotImplementedException("MOCKy"); }

        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control which jclass is provided to methods like
        //     Android.Runtime.JNIEnv.CallNonVirtualVoidMethod().
        //protected override IntPtr ThresholdClass { get; }
        //
        // Summary:
        //     This API supports the Mono for Android infrastructure and is not intended
        //     to be used directly from your code.
        //
        // Remarks:
        //     This property is used to control virtual vs. non virtual method dispatch
        //     against the underlying JNI object. When this property is equal to the declaring
        //     type, then virtual method invocation against the JNI object is performed;
        //     otherwise, we assume that the method was overridden by a derived type, and
        //     perform non-virtual methdo invocation.
        //protected override Type ThresholdType { get; }

        // Summary:
        //      Android.Bluetooth.BluetoothServerSocket.Close() can be used to abort this
        //     call from another thread.
        //
        // Returns:
        //      a connected Android.Bluetooth.BluetoothSocket
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example this call was aborted, or timeout
        //
        // Remarks:
        //     Block until a connection is established.
        //     Returns a connected Android.Bluetooth.BluetoothSocket on successful connection.
        //     Once this call returns, it can be called again to accept subsequent incoming
        //     connections.
        //     Android.Bluetooth.BluetoothServerSocket.Close() can be used to abort this
        //     call from another thread.
        //     [Android Documentation]
        [Register("accept", "()Landroid/bluetooth/BluetoothSocket;", "")]
        public abstract BluetoothSocket Accept();
        //
        // Summary:
        //      Android.Bluetooth.BluetoothServerSocket.Close() can be used to abort this
        //     call from another thread.
        //
        // Parameters:
        //   timeout:
        //     To be added.
        //
        // Returns:
        //      a connected Android.Bluetooth.BluetoothSocket
        //
        // Exceptions:
        //   Java.IO.IOException:
        //     on error, for example this call was aborted, or timeout
        //
        // Remarks:
        //     Block until a connection is established, with timeout.
        //     Returns a connected Android.Bluetooth.BluetoothSocket on successful connection.
        //     Once this call returns, it can be called again to accept subsequent incoming
        //     connections.
        //     Android.Bluetooth.BluetoothServerSocket.Close() can be used to abort this
        //     call from another thread.
        //     [Android Documentation]
        [Register("accept", "(I)Landroid/bluetooth/BluetoothSocket;", "")]
        public abstract BluetoothSocket Accept(int timeout);
        //
        // Summary:
        //     Closing the Android.Bluetooth.BluetoothServerSocket will not close any Android.Bluetooth.BluetoothSocket
        //     received from Android.Bluetooth.BluetoothServerSocket.Accept().
        //
        // Exceptions:
        //   Java.IO.IOException:
        //
        // Remarks:
        //     Immediately close this socket, and release all associated resources.
        //     Causes blocked calls on this socket in other threads to immediately throw
        //     an IOException.
        //     Closing the Android.Bluetooth.BluetoothServerSocket will not close any Android.Bluetooth.BluetoothSocket
        //     received from Android.Bluetooth.BluetoothServerSocket.Accept().
        //     [Android Documentation]
        [Register("close", "()V", "")]
        public abstract void Close();
    }
}
#endif