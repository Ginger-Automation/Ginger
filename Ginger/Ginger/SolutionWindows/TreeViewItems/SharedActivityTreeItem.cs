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

using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.Common.Enums;
//using System.Runtime.Remoting.Contexts;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivityTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly Activity mActivity;
        private GingerWPF.BusinessFlowsLib.ActivityPage mActivityEditPage;
        private SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode mShowMode;

        public SharedActivityTreeItem(Activity activity, SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode showMode = SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode.ReadWrite)
        {
            mActivity = activity;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActivity;
        }
        override public string NodePath()
        {
            return mActivity.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(Activity);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemHeaderStyle(mActivity);
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
            if (mActivityEditPage == null)
            {
                mActivityEditPage = new GingerWPF.BusinessFlowsLib.ActivityPage(mActivity, new Amdocs.Ginger.Common.Context() { Activity = mActivity }, General.eRIPageViewMode.SharedReposiotry);
            }
            return mActivityEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            if (mShowMode == SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode.ReadWrite)
            {
                AddItemNodeBasicManipulationsOptions(mContextMenu);

                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);

                TreeViewUtils.AddMenuItem(mContextMenu, "Publish To Business Flows", ShowErrorHandlerPublishInfo, null, eImageType.Link);

                AddSourceControlOptions(mContextMenu);
            }
            else
            {
                TreeViewUtils.AddMenuItem(mContextMenu, "View Repository Item Usage", ShowUsage, null, eImageType.InstanceLink);
            }
        }

        private void ShowErrorHandlerPublishInfo(object sender, RoutedEventArgs e)
        {
            RepositoryItemPublishInfoPage repositoryItemErroHandlerPublishInfoPage = new RepositoryItemPublishInfoPage(mActivity);
            repositoryItemErroHandlerPublishInfoPage.ShowAsWindow();
        }

        private void ShowUsage(object sender, RoutedEventArgs e)
        {
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(mActivity);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mActivity, "delete") == true)
            {
                return (base.DeleteTreeItem(mActivity, deleteWithoutAsking, refreshTreeAfterDelete));
            }

            return false;
        }
        public async override void PostSaveTreeItemHandler()
        {
            await SharedRepositoryOperations.UpdateLinkedInstances(mActivity);
        }
    }
}