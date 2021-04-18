using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting.Experiment;
using Newtonsoft.Json.Converters;
using Serilog;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareRemoteClient
    {
#if UseLocalServer
        const string API_BASE = "http://localhost:5101";
#else
        const string API_BASE = "https://fw.timschneeberger.me";
#endif
        const string API_GET_FIRMWARE = API_BASE + "/firmware";
        const string API_DOWNLOAD_FIRMWARE = API_BASE + "/firmware/download";

        private readonly HttpClient _client;
        public FirmwareRemoteClient()
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            };
            _client = new HttpClient(handler);
        }
        
        public async Task<FirmwareRemoteBinary[]> SearchForFirmware(bool allowDowngrade = false)
        {
            Log.Debug("FirmwareRemoteClient: Searching for firmware binaries...");

            try
            {
                FirmwareRemoteBinary[] firmwares;
                HttpResponseMessage response = await _client.GetAsync(API_GET_FIRMWARE);
                if (response.IsSuccessStatusCode)
                {
                    MediaTypeFormatterCollection formatters = new MediaTypeFormatterCollection();
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
                Log.Debug($"FirmwareRemoteClient: {results.Count()} firmware found.");
                return results.ToArray();
            }
            catch (HttpRequestException ex)
            {
                Log.Error("FirmwareRemoteClient: Search failed due to network issues: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("FirmwareRemoteClient: Search failed: " + ex.Message);
                throw;
            }
        }
        
        public async Task<byte[]> DownloadFirmware(FirmwareRemoteBinary target)
        {
            Log.Debug($"FirmwareRemoteClient: Downloading firmware '{target.BuildName}'...");

            try
            {
                byte[] binary;
                HttpResponseMessage response = await _client.GetAsync($"{API_DOWNLOAD_FIRMWARE}/{target.BuildName}");
                if (response.IsSuccessStatusCode)
                {
                    MediaTypeFormatterCollection formatters = new MediaTypeFormatterCollection();
                    formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

                    binary = await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    Log.Debug($"FirmwareRemoteClient: Error code: " + response.StatusCode);
                    throw new NetworkInformationException((int)response.StatusCode);
                }
                
                return binary;
            }
            catch (HttpRequestException ex)
            {
                Log.Error("FirmwareRemoteClient: Search failed due to network issues: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("FirmwareRemoteClient: Search failed: " + ex.Message);
                throw;
            }
        }
    }
}