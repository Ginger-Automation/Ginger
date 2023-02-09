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

using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.Variables;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedVariableTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private VariableEditPage mVariableEditPage;
        readonly VariableBase mVariableBase;
        private SharedVariablesFolderTreeItem.eVariablesItemsShowMode mShowMode;

        public SharedVariableTreeItem(VariableBase variableBase, SharedVariablesFolderTreeItem.eVariablesItemsShowMode showMode = SharedVariablesFolderTreeItem.eVariablesItemsShowMode.ReadWrite)
        {
            mVariableBase = variableBase;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mVariableBase;
        }
        override public string NodePath()
        {
            return mVariableBase.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(VariableBase);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mVariableBase);
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
            if (mVariableEditPage == null)
            {
                mVariableEditPage = new VariableEditPage(mVariableBase, null, false, VariableEditPage.eEditMode.SharedRepository);
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

                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);

                AddSourceControlOptions(mContextMenu);
            }
            else
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);
            }
        }

        private void ShowUsage(object sender, RoutedEventArgs e)
        {
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mVariableBase);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mVariableBase, "delete") == true)
            {
                return (base.DeleteTreeItem(mVariableBase, deleteWithoutAsking, refreshTreeAfterDelete));                
            }
            return false;
        }       
    }
}
