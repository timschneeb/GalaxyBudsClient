using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Platform;
using Serilog;
// ReSharper disable MethodSupportsCancellation

namespace GalaxyBudsClient.Utils.Tools;

// Debug utility
public static class HiddenCmdScanner
{
    public static void BeginScan(ushort start = 0, ushort end = ushort.MaxValue)
    {
        Task.Run(async () =>
        {
            // Scan for commands
            foreach (var id in Enumerable
                         .Range(start, end)
                         .Select(x => (ushort)x)
                         .Except(HiddenCmds.Commands.Keys))
            {
                var hexId = Convert.ToHexString(BitConverter.GetBytes(id).Reverse().ToArray());
                var cancelSource = new CancellationTokenSource();
                
                Log.Debug("Testing command {Id}", hexId);
                SppMessageReceiver.Instance.HiddenCmdData += InstanceOnHiddenCmdData;
                
                await BluetoothImpl.Instance.SendAsync(new HiddenCmdDataEncoder
                {
                    CommandId = hexId
                });

                try
                {
                    await Task.Delay(1000, cancelSource.Token);
                }
                catch(TaskCanceledException) {}
    
                SppMessageReceiver.Instance.HiddenCmdData -= InstanceOnHiddenCmdData;
                continue;

                async void InstanceOnHiddenCmdData(object? sender, HiddenCmdDataDecoder e)
                {
                    if (e.Content.Contains("Invalid Command [len="))
                    {
                        await cancelSource.CancelAsync();
                        await Task.Delay(10);
                        return;
                    }
                    
                    Log.Error("Command {Id} responded: {Msg}", hexId, e.Content.ReplaceLineEndings().Replace("\n", "\\n"));
                    await File.AppendAllTextAsync("scan.log", $"{hexId} => {e.Content.ReplaceLineEndings().Replace("\n", "\\n")}\n");
                    await cancelSource.CancelAsync();
                }
            }
        });
    }
}