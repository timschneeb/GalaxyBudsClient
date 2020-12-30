namespace GalaxyBudsClient.Scripting.Experiment
{
    public class ExperimentRuntimeResult
    {
        public int ResultCode;
        public string? ResultCodeString;
        public string Result;

        public ExperimentRuntimeResult(int resultCode, string result, string? resultCodeString = null)
        {
            ResultCode = resultCode;
            ResultCodeString = resultCodeString;
            Result = result;
        }
    }
}