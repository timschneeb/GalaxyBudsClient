using System;


namespace System.Diagnostics
{
#if NETCF

    // Don't use the real class name but drop the "Attribute" suffix to avoid:
    // CS0444: Predefined type 'System.Diagnostics.DebuggerDisplayAttribute' was not found in 'c:\...\WindowsCE\mscorlib.dll' but was found in '...'
    [Conditional("NEVER")]
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class DebuggerDisplay : Attribute
    {
        public DebuggerDisplay(string value) { }
    }

#endif
}