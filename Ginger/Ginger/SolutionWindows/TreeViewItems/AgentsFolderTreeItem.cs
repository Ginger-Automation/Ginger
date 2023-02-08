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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.Agents.AddAgentWizardLib;
using GingerCore;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    public class AgentsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly RepositoryFolder<Agent> mAgentsFolder;
        private AgentsPage mAgentsPage;
        
        public AgentsFolderTreeItem(RepositoryFolder<Agent> agentsFolder)
        {
            mAgentsFolder = agentsFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mAgentsFolder;
        }
        override public string NodePath()
        {
            return mAgentsFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(Agent);
        }

        StackPanel ITreeViewItem.Header()
        {           
            return NewTVItemFolderHeaderStyle(mAgentsFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<Agent>(mAgentsFolder);               
        }
        
        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is Agent)
            {
                return new AgentTreeItem((Agent)item);
            }

            if (item is RepositoryFolderBase)
            {
                return new AgentsFolderTreeItem((RepositoryFolder<Agent>)item);
            }

            throw new Exception("Error unknown item added to Agents folder");
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddAgentWizard(mAgentsFolder));
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mAgentsPage == null)
            {
                mAgentsPage = new AgentsPage(mAgentsFolder);
            }
            return mAgentsPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
                                 
            if (mAgentsFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Agent", allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Agent", allowRefresh: false);
            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh Application Agents Lists", RefreshApplicationAgents, null, eImageType.Refresh);

            AddSourceControlOptions(mContextMenu,false,false);          
        }

        public override void AddTreeItem()
        {
            WizardWindow.ShowWizard(new AddAgentWizard(mAgentsFolder));
        }

        private void RefreshApplicationAgents(object sender, System.Windows.RoutedEventArgs e)
        {            
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.UpdateAppAgentsMapping, null);
        }

        public override void PostDeleteTreeItemHandler()
        {            
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.UpdateAppAgentsMapping, null);
        }
    }
}