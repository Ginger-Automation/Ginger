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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionAutoSaveAndRecover;

namespace Ginger.Functionalties
{
    public class SolutionRecover
    {
        public string mRecoverFolderPath = null;
        public string RecoverFolderPath
        {
            get
            {
                return mRecoverFolderPath;

            }
            set
            {
                App.RecoverFolderPath = mRecoverFolderPath;
            }
        }

        public object NewrepositorySerializer { get; private set; }

        string RecoverFolderContianerWithTS = null;

        string mSolutionFolderPath;

        ObservableList<SolutionAutoSaveAndRecover.RecoveredItem> mFiles;
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

        public void SolutionRecoverStart(bool showRecoverPageAnyway=false)
        {
            ObservableList<RecoveredItem> recovredItems = new ObservableList<RecoveredItem>();

            if (Directory.Exists(mRecoverFolderPath))
            {                                
                NewRepositorySerializer serializer = new NewRepositorySerializer();
               
                foreach (var directory in new DirectoryInfo(mRecoverFolderPath).GetDirectories())
                {
                    string timestamp = directory.Name.ToString().Replace("AutoSave_", string.Empty);

                    IEnumerable<FileInfo> files = directory.GetFiles("*", SearchOption.AllDirectories);

                    foreach (FileInfo file in files)
                    {
                        RecoveredItem recoveredItem = new RecoveredItem();
                        recoveredItem.RecoveredItemObject = serializer.DeserializeFromFile(file.FullName);
                        recoveredItem.RecoverDate = timestamp;
                        recoveredItem.RecoveredItemObject.FileName = file.FullName;
                        recoveredItem.RecoveredItemObject.ContainingFolder = file.FullName.Replace(directory.FullName, "~");
                        recoveredItem.Status = eRecoveredItemStatus.PendingRecover;
                        recovredItems.Add(recoveredItem);
                    }
                }
              
                if(recovredItems.Count == 0)
                {                                    
                    CleanUp(); //have empty folders
                }
            }

            //show recover page
            if (recovredItems.Count > 0 || showRecoverPageAnyway)
            {
                RecoverPage recoverPage = new SolutionAutoSaveAndRecover.RecoverPage(recovredItems);
                recoverPage.ShowAsWindow(eWindowShowStyle.Dialog);
            }
        }
        public void CleanUp()
        {
            foreach (var directory in new DirectoryInfo(mRecoverFolderPath).GetDirectories())
            {
                long size = directory.GetFiles("*", SearchOption.AllDirectories).Sum(t => t.Length);
                if (size==0)
                {
                    try
                    {
                        directory.Delete(true);
                    }
                    catch
                    {

                    }
                }

            }
        }

    }
}
