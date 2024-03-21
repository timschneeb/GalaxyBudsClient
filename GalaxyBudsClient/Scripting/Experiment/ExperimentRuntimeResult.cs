namespace GalaxyBudsClient.Scripting.Experiment;

public class ExperimentRuntimeResult(int resultCode, string result, string? resultCodeString = null)
{
    public readonly int ResultCode = resultCode;
    public readonly string? ResultCodeString = resultCodeString;
    public readonly string Result = result;
}