using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace GalaxyBudsClient.Scripting.Experiment
{
    public class ExperimentClient
    {
        public event EventHandler<IReadOnlyList<ExperimentRequest>>? NewResultsFound;
        
#if UseLocalServer
        const string API_BASE = "http://localhost:5100";
#else
        const string API_BASE = "http://local.timschneeberger.me:5100";
#endif
        const string API_GET_EXPERIMENTS = API_BASE + "/experiments";
        const string API_POST_RESULT = API_BASE + "/result";
        const string API_POST_COREDUMP = API_BASE + "/coredump";

        private readonly HttpClient _client;
        private readonly Timer _timer;
        public ExperimentClient()
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; },
            };
            _client = new HttpClient(handler);
            _timer = new Timer(120 * 60 * 1000)
            {
                AutoReset = true
            };
            _timer.Elapsed += (sender, args) => ScanForExperiments();
            _timer.Start();
        }

        public async Task PostCoredump(byte[] dump, string device)
        {
            Log.Debug($"ExperimentClient: Posting coredump...");
            try
            {
                var item = new CoredumpItem()
                {
                    Content = dump,
                    Side = device
                };
                
                var jsonBody = JsonConvert.SerializeObject(item, new StringEnumConverter());
                var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var httpResponse = await _client.PostAsync(API_POST_COREDUMP, httpContent);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    Log.Warning(
                        $"ExperimentClient: Server returned error code after posting: " +
                        $"{(int)httpResponse.StatusCode} ({httpResponse.ReasonPhrase}); Content: {await httpResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error("ExperimentClient: Post failed due to network issues: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("ExperimentClient: Post failed: " + ex.Message);
            }
        }
        
        public async Task<bool> PostResult(ExperimentResult result)
        {
            Log.Debug($"ExperimentClient: Posting results for experiment #{result.ExperimentId}...");
            try
            {
                var jsonBody = JsonConvert.SerializeObject(result, new StringEnumConverter());
                var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var httpResponse = await _client.PostAsync(API_POST_RESULT, httpContent);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    Log.Warning(
                        $"ExperimentClient: Server returned error code after posting: " +
                        $"{(int)httpResponse.StatusCode} ({httpResponse.ReasonPhrase}); Content: {await httpResponse.Content.ReadAsStringAsync()}");
                }
                else
                {
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error("ExperimentClient: Post failed due to network issues: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("ExperimentClient: Post failed: " + ex.Message);
            }

            return false;
        }

        public async void ScanForExperiments()
        {
            if (!BluetoothImpl.Instance.IsConnected || 
                BluetoothImpl.Instance.ActiveModel == Models.NULL ||
                DeviceMessageCache.Instance.ExtendedStatusUpdate == null)
            {
                return;
            }
            
            Log.Debug("ExperimentClient: Scanning for experiments...");

            if (SettingsProvider.Instance.Experiments.Disabled)
            {
                Log.Error("ExperimentClient: Feature is disabled. You can enable it here: 'Options > Developer options > Crowdsourcing'");
                return;
            }

            ExperimentRequest[]? requests = null;
            try
            {
                HttpResponseMessage response =
                    await _client.GetAsync($"{API_GET_EXPERIMENTS}/{BluetoothImpl.Instance.ActiveModel.ToString()}");
                if (response.IsSuccessStatusCode)
                {
                    MediaTypeFormatterCollection formatters = new MediaTypeFormatterCollection();
                    formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

                    requests = await response.Content.ReadAsAsync<ExperimentRequest[]>(formatters);
                }

                if (requests == null)
                {
                    Log.Warning("ExperimentClient: Scan failed; no data received");
                    return;
                }

                var results = requests
                    .VerifyDecode()
                    .Where(ExperimentRequestFilters.FilterByEnvironment)
                    .Where(ExperimentRequestFilters.FilterByVersion)
                    .Where(ExperimentRequestFilters.IsNotDone)
                    .ToList()
                    .AsReadOnly();

                Log.Debug($"ExperimentClient: {results.Count} experiment(s) found.");
                NewResultsFound?.Invoke(this, results);
            }
            catch (HttpRequestException ex)
            {
                Log.Error("ExperimentClient: Scan failed due to network issues: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("ExperimentClient: Scan failed: " + ex.Message);
            }
        }
    }
}