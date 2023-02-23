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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.SourceControl;
using Ginger.SolutionGeneral;
using Ginger.SourceControl;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Amdocs.Ginger.GingerRuntime
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
                if ((WorkSpace.Instance!=null && WorkSpace.Instance.UserProfile!=null && WorkSpace.Instance.UserProfile.SourceControlUseShellClient) || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    solution.SourceControl = new GitSourceControlShellWrapper();
                }
                else
                {
                    solution.SourceControl = new GITSourceControl();
                }

            }
            else if (type == SourceControlBase.eSourceControlType.SVN)
            {
                solution.SourceControl = new SVNSourceControlShellWrapper();
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
            Reporter.ToLog(eLogLevel.INFO, "Solution Closed");
        }

        public bool OpenEncryptionKeyHandler(Solution solution)
        {
            throw new NotImplementedException();
        }

        public string GetClipboardText()
        {
            return string.Empty;
        }
    }
}
