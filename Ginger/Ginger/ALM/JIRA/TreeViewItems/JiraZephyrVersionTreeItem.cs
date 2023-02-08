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

using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using JiraRepositoryStd.Data_Contracts;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.ALM.JIRA.TreeViewItems
{
    public class JiraZephyrVersionTreeItem : JiraZephyrTreeItem, ITreeViewItem
    {
        public bool AlreadyImported { get; set; }
        public BusinessFlow MappedBusinessFlow { get; set; }
        public string MappedBusinessFlowPath { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();
        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        public JiraZephyrVersionTreeItem(List<JiraZephyrCycle> childrenCycles = null)
        {
            CurrentChildrens = new List<ITreeViewItem>();

            if (childrenCycles != null)
            {
                foreach (JiraZephyrCycle cycle in childrenCycles)
                {
                    JiraZephyrCycleTreeItem tvi = new JiraZephyrCycleTreeItem(cycle.FoldersList);
                    tvi.Name = cycle.name.ToString();
                    tvi.Id = cycle.id.ToString();
                    tvi.VersionId = cycle.versionId.ToString();
                    CurrentChildrens.Add(tvi);
                }
            }
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@ExecutionRes_16x16.png");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return CurrentChildrens;
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
            return null;
        }
        
        void ITreeViewItem.SetTools(ITreeView TV)
        {
            // there is not tools needed at this stage
        }
    }
}
