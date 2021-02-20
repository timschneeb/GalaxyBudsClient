using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Stores the LMP etc versions.
    /// </summary>
    public sealed class RadioVersions
    {
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// -
        /// <param name="lmpVersion">The LMP Version.
        /// </param>
        /// <param name="lmpSubversion">The LMP Subversion
        /// as a <see cref="T:System.UInt16"/>.
        /// </param>
        /// <param name="lmpSupportedFeatures">The LMP Supported Features.
        /// </param>
        /// <param name="mfg">The Manufacturer.
        /// </param>
        /// <summary>
        /// Get the LMP Subversion value.
        /// </summary>
        [CLSCompliant(false)]
        public RadioVersions(LmpVersion lmpVersion, ushort lmpSubversion,
            LmpFeatures lmpSupportedFeatures, Manufacturer mfg)
        {
            this.LmpVersion = lmpVersion;
            LmpSubversion = lmpSubversion;
            LmpSupportedFeatures = lmpSupportedFeatures;
            Manufacturer = mfg;
        }

        // The rest as automatic properties.
        UInt16 _lmpSubversion;

        /// <summary>
        /// Get the LMP Version.
        /// </summary>
        public LmpVersion LmpVersion { get; private set; }

        /// <summary>
        /// Get the LMP Subversion.
        /// </summary>
        /// -
        /// <remarks>
        /// <note>This is of CLR type <see cref="T:System.Int32"/> for CLS
        /// compliance.  The Bluetooth value is of course of type
        /// <see cref="T:System.UInt16"/>.
        /// </note>
        /// </remarks>
        public int LmpSubversion
        {
            get { return _lmpSubversion; }
            private set { _lmpSubversion = checked((UInt16)value); }
        }

        /// <summary>
        /// Get the LMP Supported Features.
        /// </summary>
        public LmpFeatures LmpSupportedFeatures { get; private set; }

        /// <summary>
        /// Get the Manufacturer.
        /// </summary>
        public Manufacturer Manufacturer { get; private set; }
    }
}
