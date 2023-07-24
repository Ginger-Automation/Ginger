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

using GingerCore.ALM.ZephyrEnt.Bll;
using GingerWPF.UserControlsLib.UCTreeView;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    class TestRepositoryFolderTreeItem : ZephyrEntTreeItem, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public TestRepositoryFolderTreeItem()
        {
        }
        public TestRepositoryFolderTreeItem(BaseResponseItem node) : base(node)
        {
            entityType = EntityFolderType.Phase;
            if (((JArray)node.TryGetItem("categories")).Count > 0)
            {
                CurrentChildrens = new List<ITreeViewItem>();
            }
        }
        public static bool IsCreateBusinessFlowFolder { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            if (Folder == "Subject")
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@WorkFlow_16x16.png");
            }
            else
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@Folder_16x16.png");
            }
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
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //Set Context Menu
            mContextMenu = new ContextMenu();
        }
    }
}

