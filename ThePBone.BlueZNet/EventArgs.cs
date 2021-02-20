using System;

namespace ThePBone.BlueZNet
{
  public class BlueZEventArgs : EventArgs
  {
    public BlueZEventArgs(bool isStateChange = true)
    {
      IsStateChange = isStateChange;
    }

    public bool IsStateChange { get; }
  }

  public class DeviceFoundEventArgs : BlueZEventArgs
  {
    public DeviceFoundEventArgs(Device device, bool isStateChange = true)
      : base(isStateChange)
    {
      Device = device;
    }

    public Device Device { get; }
  }
}