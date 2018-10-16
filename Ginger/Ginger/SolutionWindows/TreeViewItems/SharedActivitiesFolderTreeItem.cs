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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using GingerCore;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public enum eActivitiesItemsShowMode { ReadWrite, ReadOnly }

        private RepositoryFolder<Activity> mActivitiesFolder;
        private ActivitiesRepositoryPage mActivitiesRepositoryPage;               
        private eActivitiesItemsShowMode mShowMode;
        
        public SharedActivitiesFolderTreeItem(RepositoryFolder<Activity> activitiesFolder, eActivitiesItemsShowMode showMode = eActivitiesItemsShowMode.ReadWrite)
        {
            mActivitiesFolder = activitiesFolder;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActivitiesFolder;
        }
        override public string NodePath()
        {
            return mActivitiesFolder.FolderFullPath;
        }

        override public Type NodeObjectType()
        {
            return typeof(Activity);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mActivitiesFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<Activity>(mActivitiesFolder);            
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is Activity)
            {
                return new SharedActivityTreeItem((Activity)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new SharedActivitiesFolderTreeItem((RepositoryFolder<Activity>)item);
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error unknown item added to Activitiess folder");
                throw new NotImplementedException();
            }
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }


        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mActivitiesRepositoryPage == null)
            {
                mActivitiesRepositoryPage = new ActivitiesRepositoryPage(mActivitiesFolder);
            }
            return mActivitiesRepositoryPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eActivitiesItemsShowMode.ReadWrite)
            {
                if (mActivitiesFolder.IsRootFolder)
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false, allowRefresh: false);
                }
                else
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false, allowRefresh: false);
                }
                
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.Activity), false, false, false, false, false, false, false, false, false,false);
            }
        }               
    }
}
