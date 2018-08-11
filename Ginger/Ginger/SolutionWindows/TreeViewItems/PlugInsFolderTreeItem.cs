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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.PlugInsWindows;
using GingerWPF.PluginsLib.AddPluginWizardLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

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

            ObservableList<PluginPackage> PlugIns = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            
            foreach (PluginPackage PIW in PlugIns)
            {                
                PluginPackageTreeItem EmbeddedTreeItem = new PluginPackageTreeItem();
                EmbeddedTreeItem.PluginPackage = PIW;
                Childrens.Add(EmbeddedTreeItem);                                
            }
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
            return typeof(PluginPackage);
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();        
            TreeViewUtils.AddMenuItem(mContextMenu, "Add Plugin", AddPlugIn, null, "@Plugin_16x16.png");            
            TreeViewUtils.AddMenuItem(mContextMenu, "Open Folder in File Explorer", OpenTreeFolderHandler, null, "@Folder_16x16.png");
            mTreeView.AddToolbarTool("@Folder_16x16.png", "Open Folder in File Explorer", OpenTreeFolderHandler);
        }
        

        private void AddPlugIn(object sender, System.Windows.RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddPluginPackageWizard());
        }

        
    }
}
