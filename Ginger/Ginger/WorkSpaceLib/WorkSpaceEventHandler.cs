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
using Ginger;
using Ginger.SolutionGeneral;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System.Collections.Generic;

namespace GingerWPF.WorkSpaceLib
{
    // Ginger.exe Workspace Event Handler

    public class WorkSpaceEventHandler : IWorkSpaceEventHandler
    {        
        public void AddApplication()
        {
        }

        public void OpenAddAPIModelWizard()
        {
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
                solution.SourceControl = new SVNSourceControl();
            }
        }

        public void ShowBusinessFlows()
        {
            throw new System.NotImplementedException();
        }

        //public void OpenContainingFolder(string folderPath)
        //{
        //    string FullPath = WorkSpace.Instance.SolutionRepository.GetFolderFullPath(folderPath);
        //    if (string.IsNullOrEmpty(FullPath))
        //        return;

        //    if (!Directory.Exists(FullPath))
        //    {
        //        Directory.CreateDirectory(FullPath);
        //    }
        //    Process.Start(FullPath);
        //}



        public void ShowDebugConsole(bool visible = true)
        {
            // TODO: decide if we want both or one and done !!!!!!!!!!!!


            // Ginger WPF window with buttons like clear and we can customize
            DebugConsoleWindow debugConsole = new DebugConsoleWindow();
            debugConsole.ShowAsWindow();

            // windows black console window
            App.ShowConsoleWindow();
        }


        public void ShowUpgradeGinger(string solutionFolder, List<string> higherVersionFiles)
        {
            UpgradePage gingerUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeGinger, solutionFolder, string.Empty, higherVersionFiles);
            gingerUpgradePage.ShowAsWindow();
        }

        public void ShowUpgradeSolutionItems(SolutionUpgradePageViewMode upgradeSolution, string solutionFolder, string solutionName, List<string> list)
        {
            UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, solutionFolder, solutionName, list);
            solutionUpgradePage.ShowAsWindow();
        }

        public void SolutionClosed()
        {
        }

        public bool OpenEncryptionKeyHandler(Solution solution)
        {
            SolutionPage solutionPage = new SolutionPage();
            if(solution==null)
            {
                solutionPage.ShowAsWindow();
                return true;
            }
            else
            {
                return solutionPage.ShowAsWindow(solution);
            }
        }

        public string GetClipboardText()
        {
            return GingerCore.General.GetClipboardText();
        }
    }
}
