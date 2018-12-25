#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.Repository;
using GingerWPF.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ginger.Functionalties
{
    
    public class SolutionAutoSave
    {
        DispatcherTimer AutoSaveTimer;
               
        public string mAutoSaveFolderPath = null;
        public string AutoSaveFolderPath
        {
            get
            {
                return mAutoSaveFolderPath;
               
            }
        }

        string mSolutionFolderPath;
        
        public SolutionAutoSave()
        {
            AutoSaveTimer = new DispatcherTimer();
            AutoSaveTimer.Interval = new TimeSpan(0,5,0);

            AutoSaveTimer.Tick += AutoSaveTimer_Tick;
        }

        public void SolutionInit(string solutionFolderPath)
        {
            mSolutionFolderPath = solutionFolderPath;
            mAutoSaveFolderPath = Path.Combine(mSolutionFolderPath, "AutoSave");
        }

        public void SolutionAutoSaveStart()
        {
            AutoSaveTimer.Stop();

            if (!System.IO.Directory.Exists(mAutoSaveFolderPath))
            {
                System.IO.Directory.CreateDirectory(mAutoSaveFolderPath);
            }
            else
            {
                App.AppSolutionRecover.SolutionRecoverNeeded(mAutoSaveFolderPath);
            }

            AutoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            DoAutoSave();
        }

        public void DoAutoSave()
        {
            Task.Run(() =>
            {
                try
                {
                    //Clear previously saved items (so if user already saved them we won't have them)
                    DirectoryInfo di = new DirectoryInfo(mAutoSaveFolderPath);
                    foreach (FileInfo file in di.GetFiles())
                        file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories())
                        dir.Delete(true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "AutoSave: Failed to clear the AutoSave folder before doing new save", ex);
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
                    if (runSet.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                    {
                        DirtyFileAutoSave(runSet);
                    }
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "AutoSave: Failed to delete the all AutoSave folder on SolutionAutoSaveEnd", ex);
            }
        }

        private void DirtyFileAutoSave(RepositoryItemBase itemToSave)
        {
            try
            {
            RepositoryItemBase itemCopy = itemToSave.CreateCopy(false);
            
                //create similar folders structure
                string ItemOriginalpath = itemToSave.ContainingFolderFullPath;
                string ItemContainingfolder = itemToSave.ContainingFolder;
                string itemAutoSavePath = ItemOriginalpath.Replace(ItemOriginalpath, mAutoSaveFolderPath);
                itemAutoSavePath = Path.Combine(itemAutoSavePath, ItemContainingfolder);
                if(!Directory.Exists(itemAutoSavePath))
                {
                    Directory.CreateDirectory(itemAutoSavePath);
                }

                //save item
                itemCopy.RepositorySerializer.SaveToFile(itemCopy, Path.Combine(itemAutoSavePath, itemCopy.FileName.ToString()));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, string.Format("AutoSave: Failed to AutoSave the item:'{0}'", itemToSave.ItemName), ex);
            }
        }

    }
}
