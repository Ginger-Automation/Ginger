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

using Ginger.Activities;
using Ginger.ALM;
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.Activities;
using GingerCore.SourceControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesGroupTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly ActivitiesGroup mActivitiesGroup;
        private ActivitiesGroupPage mActivitiesGroupPage;        
        private SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode mShowMode;

        public SharedActivitiesGroupTreeItem(ActivitiesGroup activitiesGroup, SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode showMode = SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode.ReadWrite)
        {
            mActivitiesGroup = activitiesGroup;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActivitiesGroup;
        }
        override public string NodePath()
        {
            return mActivitiesGroup.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(ActivitiesGroup);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mActivitiesGroup);
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
            if (mActivitiesGroupPage == null)
            {
                mActivitiesGroupPage = new ActivitiesGroupPage(mActivitiesGroup, null,  ActivitiesGroupPage.eEditMode.SharedRepository);
            }
            return mActivitiesGroupPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;            
        }


        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mShowMode == SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode.ReadWrite)
            {
                AddItemNodeBasicManipulationsOptions(mContextMenu);
               
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);

                MenuItem exportMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Export");
                TreeViewUtils.AddSubMenuItem(exportMenu, "Export All to ALM", ExportToALM, null, "@ALM_16x16.png");

                AddSourceControlOptions(mContextMenu);
            }
            else
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);
            }                    
        }

        private void ShowUsage(object sender, RoutedEventArgs e)
        {
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mActivitiesGroup);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivitiesGroup, "delete") == true)
            {
                return (base.DeleteTreeItem(mActivitiesGroup, deleteWithoutAsking, refreshTreeAfterDelete));                
            }
            return false;
        }

        private void ExportToALM(object sender, System.Windows.RoutedEventArgs e)
        {
            ALMIntegration.Instance.ExportActivitiesGroupToALM(mActivitiesGroup, true);
        }
    }
}
