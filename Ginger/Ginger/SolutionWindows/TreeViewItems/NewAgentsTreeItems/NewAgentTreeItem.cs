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


//using Ginger.Agents;
//using Ginger.SolutionWindows.TreeViewItems;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerWPF.UserControlsLib.UCTreeView;
//using System;
//using System.Collections.Generic;
//using System.Windows.Controls;

//namespace GingerWPF.TreeViewItemsLib
//{
//    class NewAgentTreeItem : TreeViewItemBase, ITreeViewItem
//    {
//        private NewAgentEditPage mAgentEditPage;
//        public NewAgent Agent { get; set; }        

//        Object ITreeViewItem.NodeObject()
//        {
//            return Agent;
//        }
//        override public string NodePath()
//        {
//            return Agent.FilePath;
//        }
//        override public Type NodeObjectType()
//        {
//            return typeof(NewAgent);
//        }

//        StackPanel ITreeViewItem.Header()
//        {
//            return TreeViewUtils.CreateItemHeader(Agent.Name, "@Agent_16x16.png", null, Agent, nameof( Agent.Name));
//        }

//        List<ITreeViewItem> ITreeViewItem.Childrens()
//        {
//            return null;
//        }

//        bool ITreeViewItem.IsExpandable()
//        {
//            return false;
//        }

//        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
//        {
//            if (mAgentEditPage == null)
//            {
//                mAgentEditPage = new NewAgentEditPage(Agent);
//            }
//            return mAgentEditPage;
//        }

//        ContextMenu ITreeViewItem.Menu()
//        {
//            return mContextMenu;
//        }

//        void ITreeViewItem.SetTools(ITreeView TV)
//        {
//            mTreeView = TV;
//            mContextMenu = new ContextMenu();

//            AddItemNodeBasicManipulationsOptions(mContextMenu, allowCopy: false, allowCut: false);
//            AddSourceControlOptions(mContextMenu);
//        }

//        public override void PostSaveTreeItemHandler()
//        {
//        }

//        public override void PostDeleteTreeItemHandler()
//        {
//        }
//    }
//}