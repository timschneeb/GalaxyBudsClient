using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SELF_TEST)]
public class SelfTestDecoder : BaseMessageDecoder
{
    public bool AllChecks { get; }
    public bool AllLeftAccelerator { get; }
    public bool AllRightAccelerator { get; }
    public bool HardwareVersion { get; }
    public bool SoftwareVersion { get; }
    public bool TouchFirmwareVersion { get; }
    public bool LeftSerialNo { get; }
    public bool RightSerialNo { get; }
    public bool LeftBluetoothAddress { get; }
    public bool RightBluetoothAddress { get; }
    public bool LeftAcceleratorX { get; }
    public bool LeftAcceleratorY { get; }
    public bool LeftAcceleratorZ { get; }
    public bool RightAcceleratorX { get; }
    public bool RightAcceleratorY { get; }
    public bool RightAcceleratorZ { get; }
    public bool LeftProximity { get; }
    public bool RightProximity { get; }
    public bool LeftThermistor { get; }
    public bool RightThermistor { get; }
    public bool LeftAdcSoc { get; }
    public bool LeftAdcVCell { get; }
    public bool LeftAdcCurrent { get; }
    public bool RightAdcSoc { get; }
    public bool RightAdcVCell { get; }
    public bool RightAdcCurrent { get; }
    public bool LeftHall { get; }
    public bool RightHall { get; }

    public SelfTestDecoder(SppMessage msg) : base(msg)
    {
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
        LeftAdcSoc = (data & 131072) == 0;
        LeftAdcVCell = (data & 262144) == 0;
        LeftAdcCurrent = (data & 524288) == 0;
        RightAdcSoc = (data & 1048576) == 0;
        RightAdcVCell = (data & 2097152) == 0;
        RightAdcCurrent = (data & 4194304) == 0;
        LeftHall = (data & 8388608) == 0;
        RightHall = (data & 16777216) == 0;
        
        var result = true;
        foreach (var pi in GetType().GetProperties())
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
}