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

using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Variables;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedVariableTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private VariableEditPage mVariableEditPage;
        public VariableBase VariableBase { get; set; }
        private SharedVariablesFolderTreeItem.eVariablesItemsShowMode mShowMode;

        public SharedVariableTreeItem(SharedVariablesFolderTreeItem.eVariablesItemsShowMode showMode = SharedVariablesFolderTreeItem.eVariablesItemsShowMode.ReadWrite)
        {
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return VariableBase;
        }
        override public string NodePath()
        {
            return VariableBase.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(VariableBase);
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(VariableBase.Name, "@Variable_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(VariableBase.FileName, ref ItemSourceControlStatus));
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
            if (mVariableEditPage == null)
            {
                mVariableEditPage = new VariableEditPage(VariableBase);
            }
            return mVariableEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mShowMode == SharedVariablesFolderTreeItem.eVariablesItemsShowMode.ReadWrite)
            {
                AddItemNodeBasicManipulationsOptions(mContextMenu);

                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, "@Link_16x16.png");

                AddSourceControlOptions(mContextMenu);
            }
            else
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, "@Link_16x16.png");
            }
        }

        private void ShowUsage(object sender, RoutedEventArgs e)
        {
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(VariableBase);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (LocalRepository.CheckIfSureDoingChange(VariableBase, "delete") == true)
            {
                return (base.DeleteTreeItem(VariableBase, deleteWithoutAsking, refreshTreeAfterDelete));                
            }
            return false;
        }       
    }
}
