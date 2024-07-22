using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using Serilog;

namespace GalaxyBudsClient.Utils.Extensions;

public static class FilePickerExtensions
{
    public static async Task<IStorageFile?> SaveFilePickerAsync(this TopLevel host,
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
        
        var file = await host.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = defaultExtension,
            FileTypeChoices = filters, 
            SuggestedFileName = suggestedFileName,
            Title = title
        });
        
        return file;
    }
    
    public static async Task<IStorageFile?> OpenFilePickerAsync(this TopLevel host,
        IReadOnlyList<FilePickerFileType>? filters = null, string? title = null)
    {
        if (!host.StorageProvider.CanOpen)
        {
            await host.ShowUnsupportedPlatformDialogAsync();
            return null;
        }
        
        var files = await host.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false, 
            FileTypeFilter = filters,
            Title = title
        });
        
        var file = files.Count > 0 ? files[0] : null;
        return file;
    }
    
    public static async Task<IStorageFolder?> OpenFolderPickerAsync(this TopLevel host, string? title = null)
    {
        if (!host.StorageProvider.CanPickFolder)
        {
            await host.ShowUnsupportedPlatformDialogAsync();
            return null;
        }
        
        var folders = await host.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = title
        });
        
        var folder = folders.Count > 0 ? folders[0] : null;
        return folder;
    }
    
    public static async Task<byte[]?> TryReadAllBytes(this IStorageFile? file)
    {
        if (file == null)
            return null;
        
        using var memoryStream = new MemoryStream();
        try
        {
            await using var stream = await file.OpenReadAsync();
            await stream.CopyToAsync(memoryStream);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error(ex, "Failed to read file");
            return null;
        }

        return memoryStream.ToArray();
    }
    
    private static async Task ShowUnsupportedPlatformDialogAsync(this Visual host)
    {
        await new MessageBox
        {
            Title = Strings.Error,
            Description = "This platform does not support this operation"
        }.ShowAsync(host);
    }
}