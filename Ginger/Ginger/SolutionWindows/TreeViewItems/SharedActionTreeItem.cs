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

using Ginger.Actions;
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActionTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        Act mAct;
        private ActionEditPage mActionEditPage;        
        private SharedActionsFolderTreeItem.eActionsItemsShowMode mShowMode;

        public SharedActionTreeItem(Act act, SharedActionsFolderTreeItem.eActionsItemsShowMode showMode = SharedActionsFolderTreeItem.eActionsItemsShowMode.ReadWrite)
        {
            mAct = act;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mAct;
        }
        override public string NodePath()
        {
            return mAct.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(Act);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mAct);
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
            if (mActionEditPage == null)
            {
                mActionEditPage = new ActionEditPage(mAct, General.eRIPageViewMode.SharedReposiotry);
            }
            return mActionEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mShowMode == SharedActionsFolderTreeItem.eActionsItemsShowMode.ReadWrite)
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
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mAct);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mAct, "delete") == true)
            {
                return (base.DeleteTreeItem(mAct, deleteWithoutAsking, refreshTreeAfterDelete));                
            }
            return false;
        }       
    }
}
