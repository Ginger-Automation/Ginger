#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActionsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public enum eActionsItemsShowMode { ReadWrite, ReadOnly }

        RepositoryFolder<Act> mActionsFolder;
        private ActionsRepositoryPage mActionsRepositoryPage;
        private eActionsItemsShowMode mShowMode;


        public SharedActionsFolderTreeItem(RepositoryFolder<Act> actionsFolder, eActionsItemsShowMode showMode = eActionsItemsShowMode.ReadWrite)
        {
            mActionsFolder = actionsFolder;
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mActionsFolder;
        }
        override public string NodePath()
        {
            return mActionsFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(Act);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mActionsFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<Act>(mActionsFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is Act)
            {
                return new SharedActionTreeItem((Act)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new SharedActionsFolderTreeItem((RepositoryFolder<Act>)item);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error unknown item added to Actions folder");
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

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mActionsRepositoryPage == null)
            {
                mActionsRepositoryPage = new ActionsRepositoryPage(mActionsFolder, new Context());
            }
            return mActionsRepositoryPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eActionsItemsShowMode.ReadWrite)
            {
                if (mActionsFolder.IsRootFolder)
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Action", allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false, allowRefresh: false, allowDeleteAllItems: true);
                }
                else
                {
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Action", allowAddNew: false, allowRefresh: false);
                }

                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), false, false, false, false, false, false, false, false, false, false);
            }
        }
    }
}