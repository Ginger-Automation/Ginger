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
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.AgentsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib
{

    // Will be used for the new agents built for .net core and plugin

    class NewAgentsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        //public RepositoryFolder<NewAgent> mAgentsFolder;

        //TODO: fixme change name of eBusinessFlowsTreeViewMode
        public eBusinessFlowsTreeViewMode mViewMode;
        
        //public NewAgentsFolderTreeItem(RepositoryFolder<NewAgent> AgentsFolder, eBusinessFlowsTreeViewMode viewMode = eBusinessFlowsTreeViewMode.ReadWrite)
        //{
        //    mAgentsFolder = AgentsFolder;
        //    mViewMode = viewMode;
        //}

        public string Folder { get; set; }
        public string Path {get; set;}

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            // return typeof(NewAgent);
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            // return TreeViewUtils.CreateItemHeader(mAgentsFolder, nameof(RepositoryFolder<Agent>.DisplayName), eImageType.Folder, GetSourceControlImage(mAgentsFolder), false);
            return null;
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            
            //foreach (RepositoryFolder<NewAgent> RF in mAgentsFolder.GetSubFolders())
            //{
            //    // TODO: move it from here
            //    NewAgentsFolderTreeItem BFFTVI = new NewAgentsFolderTreeItem(RF, mViewMode);
            //    Childrens.Add(BFFTVI);
            //}

            //var Agents = mAgentsFolder.GetFolderItems().OrderBy(nameof(NewAgent.Name)); 

            //foreach (NewAgent agent in Agents)
            //{
            //    NewAgentTreeItem BFTI = new NewAgentTreeItem();   // TODO: add View mode
            //    BFTI.Agent = agent;
            //    Childrens.Add(BFTI);
            //}
            return Childrens;            
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            return null;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //mTreeView = TV;
            //mContextMenu = new ContextMenu();
          
            //if (mAgentsFolder.IsRootFolder)
            //    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Agent", allowCopyItems: false, allowCutItems: false, allowPaste: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false);            
            //else
            //    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Agent", allowCopyItems: false, allowCutItems: false, allowPaste: false, allowRefresh: false);

            ////AddSourceControlOptions(mContextMenu,false,false);
            //AddSourceControlOptions(mContextMenu);      
        }

        public override void AddTreeItem()
        {
            NewAgentPage NAP = new NewAgentPage();
            NAP.ShowAsWindow();            
        }
    }
}