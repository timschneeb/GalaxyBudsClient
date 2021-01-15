using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Utils.DynamicLocalization;

namespace GalaxyBudsClient.Message.Decoder
{
    public class SelfTestParser : BaseMessageParser
    {
        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.SELF_TEST;

        public bool AllChecks { set; get; }
        public bool AllLeftAccelerator { set; get; }
        public bool AllRightAccelerator { set; get; }
        public bool HardwareVersion { set; get; }
        public bool SoftwareVersion { set; get; }
        public bool TouchFirmwareVersion { set; get; }
        public bool LeftSerialNo { set; get; }
        public bool RightSerialNo { set; get; }
        public bool LeftBluetoothAddress { set; get; }
        public bool RightBluetoothAddress { set; get; }
        public bool LeftAcceleratorX { set; get; }
        public bool LeftAcceleratorY { set; get; }
        public bool LeftAcceleratorZ { set; get; }
        public bool RightAcceleratorX { set; get; }
        public bool RightAcceleratorY { set; get; }
        public bool RightAcceleratorZ { set; get; }
        public bool LeftProximity { set; get; }
        public bool RightProximity { set; get; }
        public bool LeftThermistor { set; get; }
        public bool RightThermistor { set; get; }
        public bool LeftAdcSOC { set; get; }
        public bool LeftAdcVCell { set; get; }
        public bool LeftAdcCurrent { set; get; }
        public bool RightAdcSOC { set; get; }
        public bool RightAdcVCell { set; get; }
        public bool RightAdcCurrent { set; get; }
        public bool LeftHall { set; get; }
        public bool RightHall { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;
            var data = BitConverter.ToInt32(msg.Payload, 0);

            HardwareVersion = (data & 1) == 0;
            SoftwareVersion = (data & 2) == 0;
            TouchFirmwareVersion = (data & 4) == 0;
            LeftSerialNo = (data & 8) == 0;
            RightSerialNo = (data & 16) == 0;
            LeftBluetoothAddress = (data & 32) == 0;
            RightBluetoothAddress = (data & 64) == 0;
            LeftAcceleratorX = (data & 128) == 0;
            LeftAcceleratorY = (data & 256) == 0;
            LeftAcceleratorZ = (data & 512) == 0;
            RightAcceleratorX = (data & 1024) == 0;
            RightAcceleratorY = (data & 2048) == 0;
            RightAcceleratorZ = (data & 4096) == 0;
            LeftProximity = (data & 8192) == 0;
            RightProximity = (data & 16384) == 0;
            LeftThermistor = (data & 32768) == 0;
            RightThermistor = (data & 65536) == 0;
            LeftAdcSOC = (data & 131072) == 0;
            LeftAdcVCell = (data & 262144) == 0;
            LeftAdcCurrent = (data & 524288) == 0;
            RightAdcSOC = (data & 1048576) == 0;
            RightAdcVCell = (data & 2097152) == 0;
            RightAdcCurrent = (data & 4194304) == 0;
            LeftHall = (data & 8388608) == 0;
            RightHall = (data & 16777216) == 0;


            bool result = true;
            foreach (PropertyInfo pi in GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(bool) && pi.Name != "AllChecks" 
                    && pi.Name != "AllLeftAccelerator" && pi.Name != "AllRightAccelerator")
                {
                    if (!(bool)(pi.GetValue(this) ?? false))
                    {
                        result = false;
                    }
                }
            }

            AllLeftAccelerator = LeftAcceleratorX && LeftAcceleratorY && LeftAcceleratorZ;
            AllRightAccelerator = RightAcceleratorX && RightAcceleratorY && RightAcceleratorZ;
            AllChecks = result;
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                map.Add(property.Name, ((bool)(property.GetValue(this) ?? "null")) ?
                    Loc.Resolve("selftest_pass") : Loc.Resolve("selftest_fail"));
            }

            return map;
        }
    }
}
