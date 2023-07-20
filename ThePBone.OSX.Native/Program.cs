using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using Serilog;
using ThePBone.OSX.Native.Unmanaged;

namespace ThePBone.OSX.Native
{
    public static class Program
    {
        private static unsafe void Main(string[] args)
        {
            var result = new Device();
            var arrayPtr = new byte*[2];
            fixed (byte* sUuid = standardUuid, lUuid = legacyUuid)
            {
                arrayPtr[0] = sUuid;
                arrayPtr[1] = lUuid;
                fixed (byte** uuids = arrayPtr)
                {
                    Unmanaged.SystemDialogs.ui_select_bt_device(uuids, 2, ref result);
                }
            }
                
            Console.WriteLine(result.device_name);
            Console.WriteLine(result.mac_address);
            Console.WriteLine(result.is_connected);
            Console.WriteLine(result.is_paired);
                
            Memory.btdev_free(ref result);
        }
        static readonly byte[] standardUuid =
        {
            0x00, 0x00, 0x11, 0x01, 0x00, 0x00, 0x10, 0x00,
            0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb
        };
        
        static readonly byte[] legacyUuid =
        {
            0x00, 0x00, 0x12, 0x01, 0x00, 0x00, 0x10, 0x00,
            0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb
        };
        
        public static async void DoTests()
        {
            Log.Debug("OK!");

            Logger.logger_set_on_event((level, message) =>
            {
                Log.Debug($"{level}: {Marshal.PtrToStringAnsi(message)}");
            });
            
            var bt = new BluetoothService();
            bt.Connected += (sender, args) => Log.Debug("> Connected!");
            bt.Disconnected += (sender, args) => Log.Debug("> Disconnected!");
            bt.Connecting += (sender, args) => Log.Debug("> Connecting!");
            bt.RfcommConnected += (sender, args) => Log.Debug("> RFCOMM connected!");
            bt.BluetoothErrorAsync += (sender, exception) => Log.Debug($"> ERROR! {exception}");
            bt.NewDataAvailable += (sender, bytes) => Log.Debug("<< DATA PACKET");
            
            
            //await bt.ConnectAsync("80:7b:3e:21:79:ec", "00001101-0000-1000-8000-00805F9B34FB");
            
            // Make sure the correct service uuid for the model is set here. 
            await bt.ConnectAsync("64:03:7f:2e:2b:3a", "00001101-0000-1000-8000-00805F9B34FB");
            
            LOOP:
            await Task.Delay(100);
            
            var ambientOff = new byte[]
            {
                0xFD, 0x04, 0x00, 0x80, 0x00, 0x98, 0x1B, 0xDD
            };
            var ambientOn = new byte[]
            {
                0xFD, 0x04, 0x00, 0x80, 0x01, 0xB9, 0x0B, 0xDD
            };
            var fmgStart = new byte[]
            {
                0xFD, 0x03, 0x00, 0xA0, 0xEA, 0xB5, 0xDD
            }; 
            var fmgStop = new byte[]
            {
                0xFD, 0x03, 0x00, 0xA1, 0xCB, 0xA5, 0xDD
            };
            
            // Toggle ambient sound on & off for testing
            await bt.SendAsync(ambientOn);
            await Task.Delay(3000);
            await bt.SendAsync(ambientOff);
            
            while (true)
            {
                await Task.Delay(100);
            }
        }
    }
}