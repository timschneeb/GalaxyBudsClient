using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Hooks;
using GalaxyBudsClient.Utils;
using Serilog;

public class DumpSKU : IMessageHook
{
    public async void OnHooked()
    {
        if (BluetoothImpl.Instance.ActiveModel == Models.Buds ||
            BluetoothImpl.Instance.ActiveModel == Models.BudsPlus)
        {
            Log.Error("[Script] DumpSKU: Unsupported device");
            ScriptManager.Instance.UnregisterHook(this);
            return;
        }

        if (BluetoothImpl.Instance.IsConnected)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_DEBUG_SKU);
        }
        else
        {
            BluetoothImpl.Instance.Connected += OnConnected;
        }
    }

    public void OnUnhooked()
    {
        BluetoothImpl.Instance.Connected -= OnConnected;
    }
    
    private async void OnConnected(object? sender, EventArgs e)
    {
        await Task.Delay(100);
        await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_DEBUG_SKU);
    }
    
    public void OnMessageAvailable(ref SPPMessage msg)
    {
        if (msg.Id == SPPMessage.MessageIds.MSG_ID_DEBUG_SKU)
        {
            Log.Information("[Script] DumpSKU: " + HexUtils.Dump(msg.Payload, showAscii: true, showHeader: false, showOffset: false));
            ScriptManager.Instance.UnregisterHook(this);
        }
    }

    public void OnMessageSend(ref SPPMessage msg)
    {
    }
}