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

using Ginger.Agents;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.UserControls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class AgentTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly Agent mAgent;
        private AgentEditPage mAgentEditPage;
                
        public AgentTreeItem(Agent agent)
        {
            mAgent = agent;
            if (mAgent.AgentOperations == null)
            {
                mAgent.AgentOperations = new AgentOperations(mAgent);
            }
        }

        Object ITreeViewItem.NodeObject()
        {
            return mAgent;
        }
        override public string NodePath()
        {
            return mAgent.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(Agent);
        }

        StackPanel ITreeViewItem.Header()
        {                        
            return NewTVItemHeaderStyle(mAgent);
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
            if (mAgentEditPage == null)
            {
                mAgentEditPage = new AgentEditPage(mAgent);
            }
            return mAgentEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }
        
        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            AddItemNodeBasicManipulationsOptions(mContextMenu);
            AddSourceControlOptions(mContextMenu);
        }

        public override void PostSaveTreeItemHandler()
        {            
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.UpdateAppAgentsMapping, null);
        }

        public override void PostDeleteTreeItemHandler()
        {            
            App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.UpdateAppAgentsMapping, null);
        }
    }
}