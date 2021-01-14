using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSScriptLib;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting.Hooks;
using Log = Serilog.Log;

namespace GalaxyBudsClient.Scripting
{
    public class ScriptManager
    {
        private static readonly object Padlock = new object();
        private static ScriptManager? _instance;
        public static ScriptManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new ScriptManager();
                }
            }
        }
        public static void Init()
        {
            lock (Padlock)
            { 
                _instance ??= new ScriptManager();
            }
        }

        public IList<IHook> Hooks { get; }

        public IReadOnlyList<IMessageHook> MessageHooks
        {
            get
            {
                return Hooks
                    .Where(x => x is IMessageHook)
                    .Cast<IMessageHook>()
                    .ToList()
                    .AsReadOnly();
            }
        }
        
        public IReadOnlyList<IRawStreamHook> RawStreamHooks
        {
            get
            {
                return Hooks
                    .Where(x => x is IRawStreamHook)
                    .Cast<IRawStreamHook>()
                    .ToList()
                    .AsReadOnly();
            }
        }
        
        public IReadOnlyList<IDecoderHook> DecoderHooks
        {
            get
            {
                return Hooks
                    .Where(x => x is IDecoderHook)
                    .Cast<IDecoderHook>()
                    .ToList()
                    .AsReadOnly();
            }
        }

        public ScriptManager()
        {
            Hooks = new List<IHook>();
        }  

        public void RegisterHook(IHook hook)
        {
            Hooks.Add(hook);
            hook.OnHooked();
        }

        public void UnregisterHook(IHook hook)
        {
            Hooks.Remove(hook);
            hook.OnUnhooked();
        }

        public void RegisterUserHooks()
        {
            var directory = PlatformUtils.CombineDataPath("scripts");
            Log.Information($"User script directory: {directory}");
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);
            Log.Information($"ScriptManager: {files.Length} user script(s) found");

            foreach (var file in files.Where(x => x != null))
            {
                try
                {
                    var script = File.ReadAllText(file);
                    RegisterHook((IHook)CSScript.Evaluator.LoadCode(script));
                    Log.Debug($"ScriptManager: {Path.GetFileName(file)} hooked successfully");
                }
                catch (CompilerException ex)
                {
                    Log.Error($"[{Path.GetFileName(file)}]: Compiler error in user script : {ex.Message}");
                }
                catch (Exception ex)
                {
                    Log.Error($"[{Path.GetFileName(file)}]: Failed to hook user script: {ex.Message}");
                }
            }
        }
    }
}