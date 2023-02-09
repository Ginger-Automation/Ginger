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
using GingerCore;
using GingerCore.ALM.ZephyrEnt.Bll;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    public class ZephyrEntTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private BaseResponseItem treeItemData = new BaseResponseItem();
        private bool isExpandable = true;
        public EntityFolderType entityType { get; set; }
        public BusinessFlow QCExplorer { get; set; }
        public List<ITreeViewItem> CurrentChildrens = null;
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionId { get; set; }
        public BaseResponseItem TreeItemData
        {
            get { return treeItemData; }
            set { treeItemData = value; }
        }
        public ZephyrEntTreeItem()
        {

        }
        public ZephyrEntTreeItem(BaseResponseItem node)
        {
            this.treeItemData = node;
            this.Name = node.TryGetItem("name").ToString();
            this.Id = node.TryGetItem("id").ToString();
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
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

