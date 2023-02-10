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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Environments;
using Ginger.Environments.AddEnvironmentWizardLib;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore.Environments;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib.NewEnvironmentsTreeItems
{
    public class EnvironmentsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<ProjEnvironment> mProjEnvironmentFolder;        
     
        private EnvsListPage mEnvsListPage;

        public EnvironmentsFolderTreeItem(RepositoryFolder<ProjEnvironment> projEnvironmentFolder)
        {
            mProjEnvironmentFolder = projEnvironmentFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mProjEnvironmentFolder;
        }

        override public string NodePath()
        {           
            return mProjEnvironmentFolder.FolderFullPath;
        }

        override public Type NodeObjectType()
        {
            return typeof(ProjEnvironment);
        }

        StackPanel ITreeViewItem.Header()
        {            
            return NewTVItemFolderHeaderStyle(mProjEnvironmentFolder);
        }        

        public override ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            return null;
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is ProjEnvironment)
            {
                return new EnvironmentTreeItem() { ProjEnvironment = (ProjEnvironment)item };
            }

            if (item is RepositoryFolderBase)
            {
                return new EnvironmentsFolderTreeItem((RepositoryFolder<ProjEnvironment>)item);
            }

            throw new Exception("Error unknown item added to envs folder");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ProjEnvironment>(mProjEnvironmentFolder);            
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mEnvsListPage == null)
                mEnvsListPage = new EnvsListPage(mProjEnvironmentFolder);
            return mEnvsListPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }
        
        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Add New Environment", AddItemHandler, null, eImageType.Add);
            if (mProjEnvironmentFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Environment", allowAddNew: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Environment", allowAddNew: false, allowRefresh: false);

            AddSourceControlOptions(mContextMenu);
        }

        public override void PostSaveTreeItemHandler()
        {                 
        }

        internal void AddItemHandler(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddEnvironmentWizard(mProjEnvironmentFolder));            
        }
    }
}
