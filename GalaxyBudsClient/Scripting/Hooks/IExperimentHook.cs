using System;
using GalaxyBudsClient.Scripting.Experiment;

namespace GalaxyBudsClient.Scripting.Hooks;

public interface IExperimentBase : IHook
{
    event Action<ExperimentRuntimeResult>? Finished;
}