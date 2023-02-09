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

using GingerWPF.UserControlsLib.UCTreeView;
using JiraRepositoryStd.Data_Contracts;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.ALM.JIRA.TreeViewItems
{
    public class JiraZephyrCycleTreeItem : JiraZephyrTreeItem, ITreeViewItem
    {
        private bool isExpandable = true;

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        public JiraZephyrCycleTreeItem(List<JiraZephyrCycleFolder> childrenCycles = null)
        {
            CurrentChildrens = new List<ITreeViewItem>();

            if ((childrenCycles != null) && (childrenCycles.Count > 0))
            {
                foreach (JiraZephyrCycleFolder folder in childrenCycles)
                {
                    JiraZephyrFolderTreeItem tvf = new JiraZephyrFolderTreeItem();
                    tvf.Name = folder.FolderName.ToString();
                    tvf.Id = folder.FolderId.ToString();
                    tvf.CycleId = folder.CycleId.ToString();
                    tvf.VersionId = folder.VersionId.ToString();
                    tvf.Description = folder.Description;
                    CurrentChildrens.Add(tvf);
                }
            }
            else 
            {
                isExpandable = false;
            }
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@WorkFlow_16x16.png");          
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return CurrentChildrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return isExpandable;
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
