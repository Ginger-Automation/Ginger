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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.PlugInsWindows;
using GingerCore;
using GingerWPF.PluginsLib.AddPluginWizardLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class PlugInsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly RepositoryFolder<PluginPackage> mPluginsFolder;
        private PlugInsPage mExplorerPlugInsPage;

        public PlugInsFolderTreeItem(RepositoryFolder<PluginPackage> pluginsFolder)
        {
            mPluginsFolder = pluginsFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mPluginsFolder;
        }
        override public string NodePath()
        {
            return mPluginsFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(PluginPackage);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mPluginsFolder);
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<PluginPackage>(mPluginsFolder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is PluginPackage)
            {
                return new PluginPackageTreeItem((PluginPackage)item);
            }
            else if (item is RepositoryFolderBase)
            {
                return new PlugInsFolderTreeItem((RepositoryFolder<PluginPackage>)item);
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error unknown item added to Plugin Packages folder");
                throw new NotImplementedException();
            }
        }

        Page ITreeViewItem.EditPage()
        {
            if (mExplorerPlugInsPage == null)
            {
                mExplorerPlugInsPage = new PlugInsPage();
            }
            return mExplorerPlugInsPage;
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            AddPlugIn();
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();        
            TreeViewUtils.AddMenuItem(mContextMenu, "Add Plugin", AddPlugIn, null, eImageType.Add);            
            TreeViewUtils.AddMenuItem(mContextMenu, "Open Folder in File Explorer", OpenTreeFolderHandler, null, eImageType.OpenFolder);
        }
        

        public void AddPlugIn(object sender, System.Windows.RoutedEventArgs e)
        {
            AddPlugIn();
        }

        public void AddPlugIn()
        {
            WizardWindow.ShowWizard(new AddPluginPackageWizard());
        }
    }
}
