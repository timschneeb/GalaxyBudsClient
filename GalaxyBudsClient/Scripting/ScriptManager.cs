using System;
using System.Collections.Generic;
using System.Linq;
using CSScriptLib;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Scripting.Hooks;
using GLib;
using Gtk;
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
    }
}