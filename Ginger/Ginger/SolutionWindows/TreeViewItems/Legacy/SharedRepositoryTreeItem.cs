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

using amdocs.ginger.GingerCoreNET;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedRepositoryTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private SharedRepositorySummaryPage mSharedRepositoryPage;

        string Path = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"SharedRepository\");

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {

            return TreeViewUtils.CreateItemHeader("Shared Repository", "@SharedRepository_16x16.png", Ginger.SourceControl.SourceControlUI.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return typeof(SharedRepositorySummaryPage);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens =
            [
                //Add Activities Groups
                new SharedActivitiesGroupsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>()),
                //Add Activities
                new SharedActivitiesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>()),
                // Add Actions
                new SharedActionsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()),
                //Add Variables
                new SharedVariablesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>()),
            ];

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mSharedRepositoryPage == null)
            {
                mSharedRepositoryPage = new SharedRepositorySummaryPage();
            }

            return mSharedRepositoryPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;

        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: null, allowAddNew: false, allowPaste: false, allowCutItems: false, allowCopyItems: false, allowRenameFolder: false, allowAddSubFolder: false, allowDeleteFolder: false);
            AddSourceControlOptions(mContextMenu, false, false);
        }
    }
}
