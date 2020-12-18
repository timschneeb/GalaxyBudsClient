using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Determine the status of the radio, whether the radio is individually
    /// powered-up/down, connectable, and/or discoverable.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum RadioModes
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        //--

        /// <summary>
        /// Remote devices can discover the radio.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If the radio is <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        /// it is undefined how
        /// <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Connectable"/>
        /// and <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Discoverable"/>
        /// are reported.  Different stacks behave differently.
        /// </para>
        /// </remarks>
        Discoverable = 1,

        /// <summary>
        /// Remote devices can connect to the radio.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If the radio is <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        /// it is undefined how
        /// <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Connectable"/>
        /// and <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Discoverable"/>
        /// are reported.  Different stacks behave differently.
        /// </para>
        /// </remarks>
        Connectable = 2,

        //--

        /// <summary>
        /// The radio is powered-down and thus cannot connect to remote devices.
        /// </summary>
        /// -
        /// <remarks>
        /// <note>Not all stacks report whether the radio is powered-up or down.
        /// Thus there are cases where neither <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        /// and <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOn"/> will be set.
        /// Similarly not all stacks allow the program to control powering down
        /// the radio.
        /// </note>
        /// <para>If the radio is <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        /// it is undefined whether
        /// <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Connectable"/>
        /// and <see cref="F:InTheHand.Net.Bluetooth.RadioModes.Discoverable"/>
        /// are reported.  Different stacks behave differently.
        /// </para>
        /// </remarks>
        /// -
        /// <seealso cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOn"/>
        PowerOff = 0x10,

        /// <summary>
        /// The radio is powered-up and thus can connect to remote devices.
        /// </summary>
        /// -
        /// <remarks>
        /// <note>Not all stacks report whether the radio is powered-up or down.
        /// Thus there are cases where neither <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        /// and <see cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOn"/> will be set.
        /// Similarly not all stacks allow the program to control powering down
        /// the radio.
        /// </note>
        /// </remarks>
        /// -
        /// <seealso cref="F:InTheHand.Net.Bluetooth.RadioModes.PowerOff"/>
        PowerOn = 0x20,

        // NotPresent, etc

        //--

    }

}