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

using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.IO;

namespace Ginger.ALM.QC.TreeViewItems
{
    public class QCTestSetTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public string TestSetName { get; set; }
        public string TestSetID { get; set; }
        public List<string[]> TestSetStatuses { get; set; }
        public bool AlreadyImported { get; set; }
        public BusinessFlow MappedBusinessFlow { get; set; }
        public string MappedBusinessFlowPath { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

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
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
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
        }

        public void IsTestSetAlreadyImported()
        {
            AlreadyImported = false;
            MappedBusinessFlow = null;
            MappedBusinessFlowPath = "None";
            foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
            {
                if (bf.ExternalID != null)
                {
                    if (bf.ExternalID == this.TestSetID)
                    {
                        AlreadyImported = true;
                        MappedBusinessFlow = bf;
                        MappedBusinessFlowPath = bf.ContainingFolder +'\\'+ bf.Name;                       
                        break;
                    }
                }
            }
        }
    }
}
