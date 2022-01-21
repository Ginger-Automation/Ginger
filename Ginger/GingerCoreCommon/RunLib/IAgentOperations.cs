using Amdocs.Ginger.CoreNET.RunLib;
using GingerCore.Actions;
using System.Collections.Generic;

namespace GingerCore
{
    public interface IAgentOperations
    {
        bool IsShowWindowExplorerOnStart { get; }
        bool IsWindowExplorerSupportReady { get; }

        void Close();
        void HighLightElement(Act act);
        void InitDriverConfigs();
        void ResetAgentStatus(Agent.eStatus status);
        void RunAction(Act act);
        void SetDriverConfiguration();
        void StartDriver();
        void StartPluginService();
        bool SupportVirtualAgent();
        void Test();
        void WaitForAgentToBeReady();
    }
}
