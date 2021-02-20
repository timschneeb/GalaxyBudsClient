using Serilog;

namespace GalaxyBudsClient.Scripting.Hooks
{
    public interface IHook
    {
        ///<summary>Called when the hook has been hooked.</summary>
        void OnHooked(){}

        ///<summary>Called when the hook has been unhooked. You can use this to cleanup after your script, if necessary.</summary>
        void OnUnhooked(){}
    }
}