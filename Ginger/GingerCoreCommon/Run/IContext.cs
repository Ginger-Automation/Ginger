using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
//using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.ComponentModel;

namespace Amdocs.Ginger.Common
{
    public interface IContext
    {
        Activity Activity { get; set; }
        //IAgent Agent { get; set; }
        string AgentStatus { get; set; }
        BusinessFlow BusinessFlow { get; set; }
        ProjEnvironment Environment { get; set; }
        eExecutedFrom ExecutedFrom { get; set; }
        ePlatformType Platform { get; set; }
        IGingerExecutionEngine Runner { get; set; }
        //RunSetActionBase RunsetAction { get; set; }
        TargetBase Target { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name);
    }
}
