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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using GingerCore;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private ActivitiesRepositoryPage mActivitiesRepositoryPage;
        public string Folder { get; set; }
        public string Path { get; set; }
        public enum eActivitiesItemsShowMode { ReadWrite, ReadOnly }
        private eActivitiesItemsShowMode mShowMode;

        public RepositoryFolder<Activity> mActivitiesFolder;

        public SharedActivitiesFolderTreeItem(RepositoryFolder<Activity> activitiesFolder, eActivitiesItemsShowMode showMode = eActivitiesItemsShowMode.ReadWrite)
        {
            mActivitiesFolder = activitiesFolder;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        //override public string NodePath()
        //{
        //    return Path + @"\";
        //}
        override public Type NodeObjectType()
        {
            return typeof(Activity);
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.NewRepositoryItemTreeHeader(mActivitiesFolder, nameof(RepositoryFolder<Activity>.DisplayName), eImageType.Folder, GetSourceControlImage(mActivitiesFolder), false);            
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {

            return GetChildrentGeneric<Activity>(mActivitiesFolder, nameof(Activity.ActivityName));            
        }

       
        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mActivitiesRepositoryPage == null)
            {
                mActivitiesRepositoryPage = new ActivitiesRepositoryPage(mActivitiesFolder.FolderFullPath);
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
                if (IsGingerDefualtFolder)
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false);
                else
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false);
                
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), true, false, false, false, false, false, false, false, false,false);
            }
        }               
    }
}
