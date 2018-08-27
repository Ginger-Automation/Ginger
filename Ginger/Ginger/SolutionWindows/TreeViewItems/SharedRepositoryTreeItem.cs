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
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using GingerCore.Variables;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedRepositoryTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private SharedRepositorySummaryPage mSharedRepositoryPage;

        string Path = App.UserProfile.Solution.Folder + @"\SharedRepository";

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            
            return TreeViewUtils.CreateItemHeader("Shared Repository", "@SharedRepository_16x16.png", Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
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
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            //Add Activities Groups
            SharedActivitiesGroupsFolderTreeItem SAGFTI = new SharedActivitiesGroupsFolderTreeItem();
            SAGFTI.Folder = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups);
            SAGFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\ActivitiesGroups\";
            SAGFTI.IsGingerDefualtFolder = true;
            Childrens.Add(SAGFTI);

            //Add Activities
            SharedActivitiesFolderTreeItem SAFTI = new SharedActivitiesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>());
            SAFTI.Folder = GingerDicser.GetTermResValue(eTermResKey.Activities);
            SAFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\Activities\";
            SAFTI.IsGingerDefualtFolder = true;
            Childrens.Add(SAFTI);

            // Add Actions
            SharedActionsFolderTreeItem SAcFTI = new SharedActionsFolderTreeItem();
            SAcFTI.Folder = "Actions";
            SAcFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\Actions\";
            SAcFTI.IsGingerDefualtFolder = true;
            Childrens.Add(SAcFTI);

            //Add Variables
            SharedVariablesFolderTreeItem SVFTI = new SharedVariablesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>());
            SVFTI.Folder = GingerDicser.GetTermResValue(eTermResKey.Variables);
            SVFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\Variables\";
            SVFTI.IsGingerDefualtFolder = true;
            Childrens.Add(SVFTI);

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
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
            AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName:null,allowAddNew:false,allowPaste:false,allowCutItems:false,allowCopyItems:false,allowRenameFolder:false,allowAddSubFolder:false,allowDeleteFolder:false);
            AddSourceControlOptions(mContextMenu, false, false);
        }
    }
}
