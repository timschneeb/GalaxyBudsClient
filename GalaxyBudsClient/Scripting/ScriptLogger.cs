using GalaxyBudsClient.Scripting.Hooks;
using Serilog;

namespace GalaxyBudsClient.Scripting
{
    public class ScriptLogger
    {
        private readonly string _tag;
        
        public ScriptLogger(IHook instance)
        {
            _tag = $"[Script] {instance.GetType().Name}: ";
        }
        
        public void Error(string message)
        {
            Log.Error($"{_tag}{message}");
        }
        
        public void Warning(string message)
        {
            Log.Warning($"{_tag}{message}");
        }
        
        public void Info(string message)
        {
            Log.Information($"{_tag}{message}");
        }
        
        public void Debug(string message)
        {
            Log.Debug($"{_tag}{message}");
        }
    }
}