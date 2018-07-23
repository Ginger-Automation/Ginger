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

using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.PlugIns;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class PlugInEmbeddedTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public PlugInWrapper PlugInWrapper { get; set; }
        private PlugInsWindows.PlugInWraperPage mPlugInPage;
        
        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mPlugInPage == null)
            {
                mPlugInPage = new PlugInsWindows.PlugInWraperPage(PlugInWrapper);
            }
            return mPlugInPage;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(PlugInWrapper.Name, "@Plugin_16x16.png");
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        object ITreeViewItem.NodeObject()
        {
            return PlugInWrapper;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            AddItemNodeBasicManipulationsOptions(mContextMenu, allowSave: false, allowCopy: false, allowCut: false, allowDuplicate: false, allowDelete: true);
        }

        public override void PostDeleteTreeItemHandler()
        {
            string PathToDelete = PlugInWrapper.PlugInRootPath;
        }
    }
}
