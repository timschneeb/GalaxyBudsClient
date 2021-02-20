using System;
using GalaxyBudsClient.Scripting.Hooks;
using GalaxyBudsClient.Utils;

public class HexDumpHook : IRawStreamHook
{
	/* Dump incoming byte stream to console */
    public void OnRawDataAvailable(ref byte[] msg)
    {
        var dump = HexUtils.Dump(msg, showHeader: false, showAscii: false, showOffset: false);
        Console.WriteLine(dump);
    }

    public void OnRawDataSend(ref byte[] msg)
    {
    }
}