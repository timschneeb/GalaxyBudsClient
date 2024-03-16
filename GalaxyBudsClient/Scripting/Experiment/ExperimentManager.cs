using System;
using System.Collections.Generic;
using System.Linq;
using CSScriptLib;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting.Hooks;
using GalaxyBudsClient.Utils.Extensions;
using Sentry;
using Log = Serilog.Log;
using Settings = GalaxyBudsClient.Utils.Settings;
using Task = System.Threading.Tasks.Task;
using Timer = System.Timers.Timer;

namespace GalaxyBudsClient.Scripting.Experiment
{
    public class ExperimentManager
    {
        private const int InitialScanTimeout = 5000; 
        private readonly ExperimentClient _client = new();
        private ExperimentRequest? _activeExperiment = null;
        private IExperimentBase? _activeExperimentHook = null;
        private Timer? _experimentTimeLimit = null;

        public ExperimentManager()
        {
            BluetoothImpl.Instance.Connected += OnConnected;
            _client.NewResultsFound += OnNewResultsFound;
        }

        private void LaunchExperiment(ExperimentRequest e)
        {
            if (_activeExperiment != null)
            {
                Log.Warning("ExperimentRuntime: Skipped. Another experiment (#{Id}) is already running", _activeExperiment?.Id);
                return;
            }

            if (string.IsNullOrEmpty(e.Script))
            {
                Log.Warning("ExperimentRuntime: Experiment #{Id} has an empty script", _activeExperiment?.Id);
                return;
            }
            
            _activeExperiment = e;
            
            _experimentTimeLimit = new Timer
            {
                Interval = (e.TimeConstraint ?? 60) * 1000, 
                AutoReset = false
            };
            _experimentTimeLimit.Elapsed += (sender, args) =>
            {
                Log.Warning("ExperimentRuntime: Experiment #{Id} cancelled. Time constraint of {Interval} seconds exceeded", e.Id, _experimentTimeLimit.Interval / 1000);
                ReportResult(new ExperimentRuntimeResult(-2, string.Empty, $"TIMEOUT ({_experimentTimeLimit.Interval / 1000}s)"));
            };
            _experimentTimeLimit.Start();
            
            Log.Debug("ExperimentRuntime: Launching experiment id #{Id} ({Name})", e.Id, e.Name);
            try
            {
                try
                {
                    _activeExperimentHook = (IExperimentBase)CSScript.Evaluator.LoadCode(e.Script);
                    _activeExperimentHook.Finished += ReportResult;
                    
                    Log.Debug("ExperimentRuntime: Experiment #{Id} hooked", e.Id);
                    
                    ScriptManager.Instance.RegisterHook(_activeExperimentHook);
                }
                catch(CompilerException ex)
                {
                    Log.Error("ScriptManager.RegisterHook: Compiler error: {Message}", ex.Message);
                    ReportResult(new ExperimentRuntimeResult(-3, ex.Message, $"COMPILER_ERROR"));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExperimentRuntime: Failed to execute #{Id}. Reason: {Message}; Source: {Source}", e.Id, ex.Message, ex.Source);
                ReportResult(new ExperimentRuntimeResult(-1, $"{ex.Message} (Source: {ex.Source}; Type: {ex.GetType()})", $"GENERIC_LAUNCH_ERROR"));
            }
        }

        private void ExcludeExperiment(long id)
        {
            Settings.Instance.Experiments.FinishedIds =
                Settings.Instance.Experiments.FinishedIds.Add(id);
        }
        
        public static Environment CurrentEnvironment()
        {
#if DEBUG
            return Environment.Internal;
#else
            return Environment.Production;
#endif
        }

        private async void ReportResult(ExperimentRuntimeResult runtimeResult)
        {
            _experimentTimeLimit?.Stop();

            try
            {
                ExperimentResult result;
                if (_activeExperiment != null)
                {
                    result = new ExperimentResult()
                    {
                        Environment = CurrentEnvironment(),
                        ExperimentId = _activeExperiment.Id,
                        Result = runtimeResult.Result,
                        ResultCode = runtimeResult.ResultCode,
                        ResultCodeString = runtimeResult.ResultCodeString ??
                                           (runtimeResult.ResultCode == 0 ? "PASS" : "FAIL")
                    };
                }
                else
                {
                    Log.Warning(
                        "ExperimentRuntime.ReportResult: ActiveExperiment is null. Cannot send report and exclude");
                    return;
                }

                Log.Debug("ExperimentRuntime: Experiment finished with result code {Code}", runtimeResult.ResultCode);

                if (_activeExperimentHook != null)
                {
                    _activeExperimentHook.Finished -= ReportResult;
                    ScriptManager.Instance.UnregisterHook(_activeExperimentHook);
                    Log.Debug("ExperimentRuntime: Experiment unhooked");
                }
                else
                {
                    Log.Warning("ExperimentRuntime: Experiment not unhooked; hook reference is already null");
                }

                await _client.PostResult(result);

                ExcludeExperiment(_activeExperiment.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExperimentManager: Error while posting results");
                SentrySdk.CaptureException(ex);
            } 

            NextExperiment();
        }

        private void NextExperiment()
        {
            _activeExperiment = null;
            _activeExperimentHook = null;
            Task.Delay(InitialScanTimeout).ContinueWith(_ => _client.ScanForExperiments());
        }
        
        private void OnConnected(object? sender, EventArgs e)
        {
            Task.Delay(InitialScanTimeout).ContinueWith(_ => _client.ScanForExperiments());
        }

        private void OnNewResultsFound(object? sender, IReadOnlyList<ExperimentRequest> e)
        {
            if (e.Count > 0)
            {
                LaunchExperiment(e.First());
            }
        }

        #region Singleton
        private static readonly object Padlock = new();
        private static ExperimentManager? _instance;
        public static ExperimentManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new ExperimentManager();
                }
            }
        }
        public static void Init()
        {
            lock (Padlock)
            { 
                _instance ??= new ExperimentManager();
            }
        }
        #endregion
    }
}