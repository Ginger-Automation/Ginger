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
using Amdocs.Ginger.Repository;
using Ginger.PlugInsWindows;
using GingerWPF.TreeViewItemsLib;
using System;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class PluginPackageTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly PluginPackage mPluginPackage;
        private PlugInsWindows.PluginPackagePage mPlugInPage;
        
        public PluginPackageTreeItem(PluginPackage pluginPackage)
        {
            mPluginPackage = pluginPackage;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mPluginPackage;
        }
        override public string NodePath()
        {
            return mPluginPackage.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(PluginPackage);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mPluginPackage);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mPlugInPage == null)
            {
                 mPlugInPage = new PluginPackagePage(mPluginPackage);
            }
            return mPlugInPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            AddItemNodeBasicManipulationsOptions(mContextMenu, allowSave: false, allowCopy: false, allowCut: false, allowDuplicate: false, allowDelete: true);
        }

        
    }
}
