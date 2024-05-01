using Avalonia.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class FmmConfigPageViewModel : SubPageViewModelBase
{
    public override Control CreateView() => new FmmConfigPage();
    public override string TitleKey => Keys.SmartThingsFind;
    
    [Reactive] public bool IsLinked { get; set; } = true;
    [Reactive] public string Iv { get; set; } = Strings.NoDataStored;
    [Reactive] public string SecretKey { get; set; } = Strings.NoDataStored;
    [Reactive] public string Token { get; set; } = Strings.NoDataStored;
    [Reactive] public string Region { get; set; } = Strings.NoDataStored;

    private GetFmmConfigDecoder? _fmmConfig;
    
    public FmmConfigPageViewModel()
    {
        SppMessageReceiver.Instance.GetFmmConfigResponse += OnGetFmmConfigResponseReceived;
        BluetoothImpl.Instance.Connected += (_, _) => RequestData();
    }

    private void OnGetFmmConfigResponseReceived(object? sender, GetFmmConfigDecoder e)
    {
        _fmmConfig = e;
        
        IsLinked = e.LeftSecretKey != null || e.RightSecretKey != null || 
                   e.LeftFmmToken != null || e.RightFmmToken != null || 
                   e.LeftIv != null || e.RightIv != null;
        
        Iv = FormatValue(e.LeftIv, e.RightIv);
        SecretKey = FormatValue(e.LeftSecretKey, e.RightSecretKey);
        Token = FormatValue(e.LeftFmmToken, e.RightFmmToken);
        Region = FormatValue(e.LeftRegion.ToString(), e.RightRegion.ToString());
    }
    
    private static string FormatValue(string? left, string? right)
    {
        if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
            return Strings.NoDataStored;
        if (left == right)
            return left!;
        
        var leftFormatted = string.IsNullOrEmpty(left) ? 
            DeviceMessageCache.Instance.BasicStatusUpdate?.PlacementL == PlacementStates.Disconnected ? 
                Strings.PlacementDisconnected : Strings.NoDataStored : left;
        var rightFormatted = string.IsNullOrEmpty(right) ?
            DeviceMessageCache.Instance.BasicStatusUpdate?.PlacementR == PlacementStates.Disconnected ? 
                Strings.PlacementDisconnected : Strings.NoDataStored : right;
        return string.Format(Strings.ValueLeftRight, leftFormatted, rightFormatted);
    }

    public async void DoDataClear()
    {
        await BluetoothImpl.Instance.SendAsync(new SetFmmConfigEncoder
        {
            Revision = _fmmConfig?.Revision ?? 0,
            UpdateLeft = true,
            UpdateRight = true
        });
        
        RequestData();
    }
    
    private static async void RequestData()
    {
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.GET_FMM_CONFIG);
    }
    
    public override void OnNavigatedTo()
    {
        SppMessageReceiver.Instance.BaseUpdate += OnBaseUpdateReceived;
        SppMessageReceiver.Instance.SetFmmConfigResponse += OnSetFmmConfigResponse;
    }
    
    public override void OnNavigatedFrom()
    {
        SppMessageReceiver.Instance.BaseUpdate -= OnBaseUpdateReceived;
        SppMessageReceiver.Instance.SetFmmConfigResponse -= OnSetFmmConfigResponse;
    }
    
    private void OnBaseUpdateReceived(object? sender, IBasicStatusUpdate e) => RequestData();
    
    private async void OnSetFmmConfigResponse(object? sender, byte e)
    {
        await new MessageBox
        {
            Title = e > 0 ? Strings.Error : Strings.SmartThingsFindWriteOk,
            Description = e > 0 ? Strings.SmartThingsFindWriteFail : Strings.SmartThingsFindWriteOkDesc
        }.ShowAsync();
    }
}


