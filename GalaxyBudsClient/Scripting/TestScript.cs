using System;
using System.Text;
using System.Text.Json.Serialization;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Scripting.Hooks;
using Newtonsoft.Json;

public class BuildInfoDumper : IExperimentBase, IMessageHook
{
    public event Action<ExperimentRuntimeResult>? Finished;

    public async void OnHooked()
    {
        try
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.MSG_ID_DEBUG_BUILD_INFO);
        }
        catch (Exception ex)
        {
            Finished?.Invoke(new ExperimentRuntimeResult(1, ex.Message));
        }
    }

    public void OnMessageAvailable(ref SPPMessage msg)
    {
        try
        {
            switch (msg.Id)
            {
                case SPPMessage.MessageIds.MSG_ID_DEBUG_BUILD_INFO:
                    var e = new ExperimentRuntimeResult(0, 
                        JsonConvert.SerializeObject(Encoding.UTF8.GetString(msg.Payload), Formatting.Indented));
                    Finished?.Invoke(e);
                    break;
            }
        }
        catch (Exception ex)
        {
            Finished?.Invoke(new ExperimentRuntimeResult(2, ex.Message));
        }
    }

    public void OnMessageSend(ref SPPMessage msg)
    {
    }
}