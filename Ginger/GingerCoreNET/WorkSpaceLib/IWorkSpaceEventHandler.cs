#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion



using System.Collections.Generic;
using Ginger.SolutionGeneral;

namespace amdocs.ginger.GingerCoreNET
{
    public interface IWorkSpaceEventHandler
    {             
        void ShowBusinessFlows();

        void OpenAddAPIModelWizard();
        

        // Occur when Solution is closed
        void SolutionClosed();

        // occur when useprofile contains show debug window
        void ShowDebugConsole(bool visible = true);
        void ShowUpgradeGinger(string solutionFolder, List<string> higherVersionFiles); // when openning solution we check if the solution is older or newer, and popup an upgrade question for ginger or for the solution
        void ShowUpgradeSolutionItems(SolutionUpgradePageViewMode upgradeSolution, string solutionFolder, string solutionName, List<string> list);
        void SetSolutionSourceControl(Solution solution, ref string RepositoryRootFolder);
        bool OpenEncryptionKeyHandler(Solution solution);

        string GetClipboardText();
    }
}
