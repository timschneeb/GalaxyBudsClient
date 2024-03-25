using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform;
using Newtonsoft.Json.Converters;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware;

public class FirmwareRemoteClient
{
    private const string ApiBase = "https://fw.timschneeberger.me/v3";
    private const string ApiGetFirmware = ApiBase + "/firmware";
    private const string ApiDownloadFirmware = ApiBase + "/firmware/download";

    private readonly HttpClient _client;
    public FirmwareRemoteClient()
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = delegate { return true; }
        };
        _client = new HttpClient(handler);
    }
        
    public async Task<FirmwareRemoteBinary[]> SearchForFirmware(bool allowDowngrade = false)
    {
        Log.Debug("FirmwareRemoteClient: Searching for firmware binaries...");

        try
        {
            FirmwareRemoteBinary[] firmwares;
            var response = await _client.GetAsync(ApiGetFirmware + $"/{BluetoothService.ActiveModel.ToString()}");
            if (response.IsSuccessStatusCode)
            {
                var formatters = new MediaTypeFormatterCollection();
                formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

                firmwares = await response.Content.ReadAsAsync<FirmwareRemoteBinary[]>(formatters);
            }
            else
            {
                throw new NetworkInformationException((int)response.StatusCode);
            }

            var results = firmwares
                .Where(FirmwareRemoteBinaryFilters.FilterByModel);
                
            if (!allowDowngrade)
            {
                results = results.Where(FirmwareRemoteBinaryFilters.FilterByVersion);
            }

            results = results.ToList().AsReadOnly();
            Log.Debug("FirmwareRemoteClient: {Count} firmware found", results.Count());
            return results.ToArray();
        }
        catch (HttpRequestException ex)
        {
            Log.Error("FirmwareRemoteClient: Search failed due to network issues: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("FirmwareRemoteClient: Search failed: {Message}", ex.Message);
            throw;
        }
    }
        
    public async Task<byte[]> DownloadFirmware(FirmwareRemoteBinary target)
    {
        Log.Debug("FirmwareRemoteClient: Downloading firmware \'{Name}\'...", target.BuildName);

        try
        {
            byte[] binary;
            var response = await _client.GetAsync($"{ApiDownloadFirmware}/{target.BuildName}");
            if (response.IsSuccessStatusCode)
            {
                var formatters = new MediaTypeFormatterCollection();
                formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

                binary = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                Log.Debug("FirmwareRemoteClient: Error code: {Code}", response.StatusCode);
                throw new NetworkInformationException((int)response.StatusCode);
            }
                
            return binary;
        }
        catch (HttpRequestException ex)
        {
            Log.Error("FirmwareRemoteClient: Search failed due to network issues: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Log.Error("FirmwareRemoteClient: Search failed: {Message}", ex.Message);
            throw;
        }
    }
}