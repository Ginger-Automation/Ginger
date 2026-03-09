#region License
/*
Copyright © 2014-2026 European Support Limited

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
using System;
using System.IO;

namespace Ginger.Functionalties
{
    public class SolutionRecover
    {
        private string mRecoverFolderPath = null;

        public object NewrepositorySerializer { get; private set; }

        string RecoverFolderContianerWithTS = null;

        string mSolutionFolderPath;

        public string RecoverFolderPath
        {
            get { return mRecoverFolderPath; }
        }

        public void SolutionInit(string solutionFolderPath)
        {
            mSolutionFolderPath = solutionFolderPath;
            mRecoverFolderPath = Path.Combine(mSolutionFolderPath, "Recover");
        }

        public void SolutionRecoverNeeded(string AutoSavePath)
        {
            if (!System.IO.Directory.Exists(mRecoverFolderPath))
            {
                System.IO.Directory.CreateDirectory(mRecoverFolderPath);
            }
            //Move all Auto Saved files to Recover folder
            //create folder to hold new files
            RecoverFolderContianerWithTS = "AutoSave" + DateTime.Now.ToString("_dd-MMM-yy_HH-mm");
            RecoverFolderContianerWithTS = Path.Combine(mRecoverFolderPath, RecoverFolderContianerWithTS);
            System.IO.Directory.CreateDirectory(RecoverFolderContianerWithTS);

            //Move files
            if (Directory.Exists(AutoSavePath))
            {
                foreach (var file in new DirectoryInfo(AutoSavePath).GetDirectories())
                {
                    string itemtoSaveToRecover = file.FullName.Replace(AutoSavePath, RecoverFolderContianerWithTS);
                    file.MoveTo(Path.Combine(itemtoSaveToRecover));
                }
            }
        }

        public void SolutionRecoverStart(bool showRecoverPageAnyway = false)
        {
            if (Directory.Exists(WorkSpace.Instance.AppSolutionRecover.RecoverFolderPath)
                && Directory.GetFiles(WorkSpace.Instance.AppSolutionRecover.RecoverFolderPath, "*.xml", SearchOption.AllDirectories).Length > 0)
            {
                TargetFrameworkHelper.Helper.ShowRecoveryItemPage();
            }
        }
        public void CleanUpRecoverFolder()
        {
            if (Directory.Exists(mRecoverFolderPath))
            {
                try
                {
                    Directory.Delete(mRecoverFolderPath, true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to delete Recover folder", ex);
                }
            }
        }


        public void DoSolutionAutoSaveAndRecover()
        {
            //Init
            WorkSpace.Instance.AppSolutionAutoSave.SolutionInit(WorkSpace.Instance.Solution.Folder);
            SolutionInit(WorkSpace.Instance.Solution.Folder);

            //start Auto Save
            WorkSpace.Instance.AppSolutionAutoSave.SolutionAutoSaveStart();

            //check if Recover is needed
            if (!WorkSpace.Instance.UserProfile.DoNotAskToRecoverSolutions)
            {
                SolutionRecoverStart();
            }
        }
    }
}
