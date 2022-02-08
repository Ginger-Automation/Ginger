using Amdocs.Ginger.CoreNET.RunLib;
using GingerCore.Actions;
using System.Collections.Generic;

namespace GingerCore
{
    public interface IAgentOperations
    {
        bool IsShowWindowExplorerOnStart { get; }
        bool IsWindowExplorerSupportReady { get; }
        Agent.eStatus Status { get; }
        void Close();
        void HighLightElement(Act act);
        void InitDriverConfigs();
        void ResetAgentStatus(Agent.eStatus status);
        void RunAction(Act act);
        void SetDriverConfiguration();
        void StartDriver();
        void StartPluginService();
        void Test();
        void WaitForAgentToBeReady();
    }
}
