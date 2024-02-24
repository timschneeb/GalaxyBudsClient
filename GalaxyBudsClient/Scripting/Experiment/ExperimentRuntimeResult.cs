namespace GalaxyBudsClient.Scripting.Experiment
{
    public class ExperimentRuntimeResult
    {
        public readonly int ResultCode;
        public readonly string? ResultCodeString;
        public readonly string Result;

        public ExperimentRuntimeResult(int resultCode, string result, string? resultCodeString = null)
        {
            ResultCode = resultCode;
            ResultCodeString = resultCodeString;
            Result = result;
        }
    }
}