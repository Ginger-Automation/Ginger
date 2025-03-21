#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Ginger.SolutionWindows.TreeViewItems;
using GingerCore.ALM.QC;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Ginger.ALM.QC.TreeViewItems
{
    public class QCTestLabFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public string TestSetName { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

        public List<ITreeViewItem> CurrentChildrens = null;

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            //if root item
            if (Folder == "Root")
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@WorkFlow_16x16.png");
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(TestSetName))
                { return TreeViewUtils.CreateItemHeader(TestSetName, "@TestSet_16x16.png"); }
                else { return TreeViewUtils.CreateItemHeader(Folder, "@Folder_16x16.png"); }
            }

        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            CurrentChildrens = [];

            // get the sub items for the root here and return list of Childrens
            // Step #1 add sub folder of current folder
            List<string> strParentFolders = ALMIntegration.Instance.GetTestLabExplorer(Path);
            //Add QC folders to tree children    

            foreach (string sFolder in strParentFolders)
            {
                QCTestLabFolderTreeItem pfn = new QCTestLabFolderTreeItem
                {
                    Folder = sFolder,
                    Path = Path + @"\" + sFolder
                };
                CurrentChildrens.Add(pfn);
            }

            // Step #2 add folder Test Set list
            List<ALMTestSetSummary> sTestSets = (List<ALMTestSetSummary>)ALMIntegration.Instance.GetTestSetExplorer(Path);

            foreach (ALMTestSetSummary tsItem in sTestSets)
            {
                tsItem.TestSetStatuses = new List<string[]>();
                QCTestSetTreeItem pfn = new QCTestSetTreeItem
                {
                    TestSetID = tsItem.TestSetID.ToString(),
                    TestSetName = tsItem.TestSetName,
                    Path = Path + @"\" + tsItem.TestSetName
                };
                ALMTestSetSummary tsItemStatus = ALMIntegration.Instance.GetTSRunStatus(tsItem);
                pfn.TestSetStatuses = tsItem.TestSetStatuses;
                pfn.IsTestSetAlreadyImported();
                CurrentChildrens.Add(pfn);
            }

            return CurrentChildrens;
        }


        private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(Path))
                {
                    QCTestLabFolderTreeItem BFFTI = new QCTestLabFolderTreeItem();
                    string FolderName = System.IO.Path.GetFileName(d);

                    BFFTI.Folder = FolderName;
                    BFFTI.Path = d;

                    Childrens.Add(BFFTI);
                }

            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return null;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //Set Tools
            //TV.AddToolbarTool("@Refresh_16x16.png", "Refresh Business Flows", TV.RefreshTreeNodeChildren);

            //Set Context Menu
            mContextMenu = new ContextMenu();
        }
    }
}
