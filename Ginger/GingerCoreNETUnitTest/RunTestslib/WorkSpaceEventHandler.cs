#region License
/*
Copyright © 2014-2022 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Ginger.SolutionGeneral;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GingerCoreNETUnitTest.RunTestslib
{

    // GingerCoreNETUnitTest Workspace Event Handler
    public class WorkSpaceEventHandler : IWorkSpaceEventHandler
    {
        
        public void OpenAddAPIModelWizard()
        {
            throw new System.NotImplementedException();
        }

        public void SetSolutionSourceControl(Solution solution, ref string repositoryRootFolder)
        {
            Console.WriteLine("SetSolutionSourceControl");
        }

        public void ShowBusinessFlows()
        {
        }

        public void ShowDebugConsole(bool visible = true)
        {
            Console.WriteLine("ShowDebugConsole visible=" + visible);
        }

        public void ShowUpgradeGinger(string solutionFolder, List<string> higherVersionFiles)
        {
            throw new NotImplementedException();
        }

        public void ShowUpgradeSolutionItems(SolutionUpgradePageViewMode upgradeSolution, string solutionFolder, string solutionName, List<string> list)
        {
            throw new NotImplementedException();
        }

        public void SolutionClosed()
        {
        }

        public bool OpenEncryptionKeyHandler(Solution solution)
        {
            throw new NotImplementedException();
        }

        public string GetClipboardText()
        {
            throw new NotImplementedException();
        }
    }
}
