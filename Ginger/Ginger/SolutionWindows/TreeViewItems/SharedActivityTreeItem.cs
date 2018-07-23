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

using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivityTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private ActivityEditPage mActivityEditPage;
        public Activity Activity { get; set; }
        private SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode mShowMode;

        public SharedActivityTreeItem(SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode showMode = SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode.ReadWrite)
        {
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return Activity;
        }
        override public string NodePath()
        {
            return Activity.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(Activity);
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Activity.ActivityName, "@Activities_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Activity.FileName, ref ItemSourceControlStatus));
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
            if (mActivityEditPage == null)
            {
                mActivityEditPage = new ActivityEditPage(Activity);
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
            RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage(Activity);
            usagePage.ShowAsWindow();
        }

        public override bool DeleteTreeItem(object item, bool deleteWithoutAsking = false, bool refreshTreeAfterDelete = true)
        {
            if (LocalRepository.CheckIfSureDoingChange(Activity, "delete") == true)
            {
                return (base.DeleteTreeItem(Activity, deleteWithoutAsking, refreshTreeAfterDelete));                
            }

            return false;
        }      
    }
}