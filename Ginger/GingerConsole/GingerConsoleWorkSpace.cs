#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common;
using Ginger.SolutionGeneral;
using Ginger.SourceControl;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;

namespace Amdocs.Ginger.GingerConsole
{
    // GingerConsole Workspace Event Handler

    public class GingerConsoleWorkSpace : IWorkSpaceEventHandler
    {
        public GingerConsoleWorkSpace()
        {
        }

        public void OpenAddAPIModelWizard()
        {
            throw new NotImplementedException();
        }

        public void SetSolutionSourceControl(Solution solution, ref string repositoryRootFolder)
        {            
            SourceControlBase.eSourceControlType type = SourceControlIntegration.CheckForSolutionSourceControlType(solution.Folder, ref repositoryRootFolder);
            if (type == SourceControlBase.eSourceControlType.GIT)
            {
                solution.SourceControl = new GITSourceControl();
            }
            else if (type == SourceControlBase.eSourceControlType.SVN)
            {
                // FIXME after SVN moved to .net core

                // solution.SourceControl = new SVNSourceControl();
                                
                Reporter.ToConsole(eLogLevel.ERROR, "Source Control of type SVN is not yet supported in GingerConsole");
            }
        }

        public void ShowBusinessFlows()
        {
            throw new NotImplementedException();
        }

        public void ShowDebugConsole(bool visible = true)
        {
            // Ignore
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
            Reporter.ToConsole(eLogLevel.INFO, "Solution Closed");
        }
    }
}
