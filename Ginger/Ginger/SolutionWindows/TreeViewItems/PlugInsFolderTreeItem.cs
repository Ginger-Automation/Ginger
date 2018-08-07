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
using Ginger.PlugInsWindows;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerPlugIns;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class PlugInsFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private PlugInsPage mExplorerPlugInsPage;
        public string Path { get; set; }
        public string Folder { get; set; }
        override public string NodePath()
        {
            return Path + @"\";
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            //Add Plugins
            //ObservableList<PlugInWrapper> PlugIns = new ObservableList<PlugInWrapper>();
            //PlugIns = App.LocalRepository.GetSolutionPlugIns(specificFolderPath: Path);
            
            //foreach (PlugInWrapper PIW in PlugIns)
            //{
            //    if (PIW.PlugInType == PlugInWrapper.ePluginType.Embedded)
            //    {
            //        PlugInEmbeddedTreeItem EmbeddedTreeItem = new PlugInEmbeddedTreeItem();
            //        EmbeddedTreeItem.PlugInWrapper = PIW;
            //        Childrens.Add(EmbeddedTreeItem);
            //    }
            //    if (PIW.PlugInType == PlugInWrapper.ePluginType.System)
            //    {
            //        PlugInSystemTreeItem SystemTreeItem = new PlugInSystemTreeItem();
            //        SystemTreeItem.PlugInWrapper = PIW;
            //        Childrens.Add(SystemTreeItem);
            //    }
            //}
            return Childrens;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mExplorerPlugInsPage == null)
            {
                mExplorerPlugInsPage = new PlugInsPage();
            }
            return mExplorerPlugInsPage;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName;

            if (IsGingerDefualtFolder)
            {
                ImageFileName = "@Plugin_16x16.png";
            }
            else
            {
                ImageFileName = "@Folder2_16x16.png";
            }

            return TreeViewUtils.CreateItemHeader(Folder, ImageFileName, Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        object ITreeViewItem.NodeObject()
        {
            return null;
        }

        override public Type NodeObjectType()
        {
            return typeof(PlugInWrapper);
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            TreeViewUtils.AddMenuItem(mContextMenu, "Refresh", RefreshItems, null, eImageType.Refresh);
            TreeViewUtils.AddMenuItem(mContextMenu, "Add Embedded Plugin", AddNewEmbeddedPlugIn, null, "@Plugin_16x16.png");
            TreeViewUtils.AddMenuItem(mContextMenu, "Add System Plugin", AddNewSystemPlugIn, null, "@SystemPlugin_16x16.png");
            TreeViewUtils.AddMenuItem(mContextMenu, "Open Folder in File Explorer", OpenTreeFolderHandler, null, "@Folder_16x16.png");
            mTreeView.AddToolbarTool("@Folder_16x16.png", "Open Folder in File Explorer", OpenTreeFolderHandler);
        }
        private void RefreshItems(object sender, System.Windows.RoutedEventArgs e)
        {
            //App.LocalRepository.RefreshSolutionPlugInConfigurationsCache(specificFolderPath: Path);
            //mTreeView.Tree.RefreshSelectedTreeNodeChildrens();
        }

        private void AddNewEmbeddedPlugIn(object sender, System.Windows.RoutedEventArgs e)
        {
            AddNewPlugIn(true);
        }

        private void AddNewSystemPlugIn(object sender, System.Windows.RoutedEventArgs e)
        {
            AddNewPlugIn(false);
        }

        private void AddNewPlugIn(bool IsEmbeddedPlugin)
        {
            //PlugInWrapper PW = Ginger.PlugInsLib.PlugInsIntegration.AddNewPlugIn(IsEmbeddedPlugin, App.UserProfile.Solution.Folder.ToUpper());

            //if (PW != null)
            //{
            //    ITreeViewItem PTI;
            //    if (IsEmbeddedPlugin)
            //    {
            //        PTI = new PlugInEmbeddedTreeItem();
            //        ((PlugInEmbeddedTreeItem)PTI).PlugInWrapper = PW;

            //    }
            //    else
            //    {
            //        PTI = new PlugInSystemTreeItem();
            //        ((PlugInSystemTreeItem)PTI).PlugInWrapper = PW;
            //    }
            //    ITreeViewItem addTreeViewItem = mTreeView.Tree.AddChildItemAndSelect(this, PTI);
            //    App.LocalRepository.AddItemToCache(PW);
            //    mTreeView.Tree.RefreshHeader(addTreeViewItem);
            //}

            //App.AutomateTabGingerRunner.PlugInsList = App.LocalRepository.GetSolutionPlugIns();
        }
    }
}
