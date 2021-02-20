using System;

namespace InTheHand.Net.Bluetooth.Msft
{
    struct BTH_RADIO_INFO
    {
        internal readonly LmpFeatures _lmpSupportedFeatures;
        internal readonly Manufacturer _mfg;
        internal readonly UInt16 _lmpSubversion;
        internal readonly LmpVersion _lmpVersion;

        internal BTH_RADIO_INFO(LmpVersion lmpVersion, UInt16 lmpSubversion, Manufacturer mfg, UInt64 lmpSupportedFeatures)
        {
            _lmpSupportedFeatures = unchecked((LmpFeatures)lmpSupportedFeatures);
            _mfg = mfg;
            _lmpSubversion = lmpSubversion;
            _lmpVersion = lmpVersion;
        }

        internal BTH_RADIO_INFO(Version fakeSetAllUnknown)
        {
            _lmpSupportedFeatures = 0;
            _mfg = Manufacturer.Unknown;
            _lmpSubversion = 0;
            _lmpVersion = LmpVersion.Unknown;
        }

        public override string ToString()
        {
            var txt = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "BTH_RADIO_INFO {0}, {1}, {2}, {3:X16} '{4}'",
                _lmpVersion, _lmpSubversion, _mfg,
                (UInt64)_lmpSupportedFeatures, _lmpSupportedFeatures);
            return txt;
        }

        public RadioVersions ConvertToRadioVersions()
        {
            return new RadioVersions(_lmpVersion, _lmpSubversion,
                _lmpSupportedFeatures, _mfg);
        }

    }
}
