using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Utils.Extensions;

public static class FilePickerExtensions
{
    public static async Task<string?> SaveFilePickerAsync(this Window host,
        IReadOnlyList<FilePickerFileType>? filters = null, 
        string? defaultExtension = null,
        string? suggestedFileName = null,
        string? title = null)
    {
        if (!host.StorageProvider.CanSave)
        {
            await host.ShowUnsupportedPlatformDialogAsync();
            return null;
        }
        
        var file = await host.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            DefaultExtension = defaultExtension,
            FileTypeChoices = filters, 
            SuggestedFileName = suggestedFileName,
            Title = title
        });
            
        return file?.TryGetLocalPath();
    }
    
    public static async Task<string?> OpenFilePickerAsync(this Window host,
        IReadOnlyList<FilePickerFileType>? filters = null, string? title = null)
    {
        if (!host.StorageProvider.CanOpen)
        {
            await host.ShowUnsupportedPlatformDialogAsync();
            return null;
        }
        
        var files = await host.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false, 
            FileTypeFilter = filters
        });
        
        var file = files.Count > 0 ? files[0] : null;
        return file?.TryGetLocalPath();
    }
    
    public static async Task<string?> OpenFolderPickerAsync(this Window host, string? title = null)
    {
        if (!host.StorageProvider.CanPickFolder)
        {
            await host.ShowUnsupportedPlatformDialogAsync();
            return null;
        }
        
        var folders = await host.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = title
        });
        
        var folder = folders.Count > 0 ? folders[0] : null;
        return folder?.TryGetLocalPath();
    }
    
    private static async Task ShowUnsupportedPlatformDialogAsync(this Window host)
    {
        await new MessageBox()
        {
            Title = Loc.Resolve("error"),
            Description = "This platform does not support this operation",
            Topmost = true
        }.ShowDialog(host);
    }
}