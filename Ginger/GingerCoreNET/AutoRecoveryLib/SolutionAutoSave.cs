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
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace Ginger.Functionalties
{

    public class SolutionAutoSave
    {
        Timer AutoSaveTimer;
               
        public string mAutoSaveFolderPath = null;
        public string AutoSaveFolderPath
        {
            get
            {
                return mAutoSaveFolderPath;               
            }
        }

        string mSolutionFolderPath;

        public bool WaitForAutoSave = false;      
        public SolutionAutoSave()
        {
            AutoSaveTimer = new Timer();            
            AutoSaveTimer.Interval = new TimeSpan(0,5,0).TotalMilliseconds;
            AutoSaveTimer.Elapsed += AutoSaveTimer_Tick;
            AutoSaveTimer.AutoReset = true;
            AutoSaveTimer.Enabled = true;
        }

        public void SolutionInit(string solutionFolderPath)
        {
            mSolutionFolderPath = solutionFolderPath;
            mAutoSaveFolderPath = Path.Combine(mSolutionFolderPath, "AutoSave");
        }

        public void SolutionAutoSaveStart()
        {            
            AutoSaveTimer.Stop();

            if (!Directory.Exists(mAutoSaveFolderPath))
            {
                Directory.CreateDirectory(mAutoSaveFolderPath);
            }
            else
            {
                WorkSpace.Instance.AppSolutionRecover.SolutionRecoverNeeded(mAutoSaveFolderPath);
            }

            AutoSaveTimer.Start();
        }

        public void StopSolutionAutoSave()
        {
            AutoSaveTimer.Stop();            
        }
        public void ResumeSolutionAutoSave()
        {
            AutoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (mAutoSaveFolderPath != null)//meaning solution is loaded
            {
                DoAutoSave();
            }
        }
        
        public void DoAutoSave()
        {           
            Task.Run(() =>
            {
                try
                {
                    WaitForAutoSave = true;                  
                    if (Directory.Exists(AutoSaveFolderPath))
                    {
                        try
                        {
                            Directory.Delete(AutoSaveFolderPath, true);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.WARN, "AutoSave: Failed to clear the AutoSave folder before doing new save", ex);
                        }
                    }

                    if (WorkSpace.Instance.SolutionRepository == null)
                    {
                        return;
                    }

                    //get all dirty items for AutoSave
                    //BusinesFlows           
                    foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
                    {
                        if (bf.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                        {
                            DirtyFileAutoSave(bf);
                        }
                    }

                    //Run Sets           
                    ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    foreach (RunSetConfig runSet in RunSets)
                    {
                        if (runSet.AllowAutoSave && runSet.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                        {                      
                            runSet.UpdateRunnersBusinessFlowRunsList();
                            DirtyFileAutoSave(runSet);
                        }                    
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to do Auto Save : ", ex);
                }
                finally
                {
                    WaitForAutoSave = false;
                }
            });           
        }
        public void SolutionAutoSaveEnd()
        {
            AutoSaveTimer.Stop();

            //clear AutoSave folder
            try
            {
                if (Directory.Exists(AutoSaveFolderPath))
                {
                    Directory.Delete(AutoSaveFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "AutoSave: Failed to delete the all AutoSave folder on SolutionAutoSaveEnd", ex);
            }
        }

        private void DirtyFileAutoSave(RepositoryItemBase itemToSave)
        {
            try
            {
                RepositoryItemBase itemCopy = itemToSave.CreateCopy(false);

                //create similar folders structure
                string itemAutoSavePath = itemToSave.FilePath.Replace(WorkSpace.Instance.Solution.Folder, mAutoSaveFolderPath);
                string itemAutoSaveFolder = Path.GetDirectoryName(itemAutoSavePath);
                if (!Directory.Exists(itemAutoSaveFolder))
                {
                    Directory.CreateDirectory(itemAutoSaveFolder);
                }

                //save item
                itemCopy.RepositorySerializer.SaveToFile(itemCopy, itemAutoSavePath);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("AutoSave: Failed to AutoSave the item:'{0}'", itemToSave.ItemName), ex);
            }
        }

        public void CleanAutoSaveFolders()
        {
            //To Clear the AutoSave Directory Folder
            if (Directory.Exists(AutoSaveFolderPath))
            {
                try
                {
                    Directory.Delete(AutoSaveFolderPath, true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to delete Auto Save folder", ex);
                }
            }
            if (Directory.Exists(WorkSpace.Instance.RecoverFolderPath))
            {
                try
                {
                    Directory.Delete(WorkSpace.Instance.RecoverFolderPath, true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to delete Recover folder", ex);
                }
            }
        }

    }
}
