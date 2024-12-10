namespace GalaxyBudsClient.Platform.Interfaces;

public interface IDesktopServices
{
    public bool IsAutoStartEnabled { set; get; }
    
    public void OpenUri(string uri);
}