#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using Ginger.SourceControl;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems
{
    public class ApplicationPOMsTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<ApplicationPOMModel> mPOMModelFolder;        
        private POMModelsPage mPOMModelsPage;
        private ObservableList<ApplicationPOMModel> mChildPoms = null;

        public ApplicationPOMsTreeItem(RepositoryFolder<ApplicationPOMModel> POMModelFolder)
        {
            mPOMModelFolder = POMModelFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mPOMModelFolder;
        }
        override public string NodePath()
        {
            return mPOMModelFolder.FolderFullPath;
        }
        override public Type NodeObjectType()
        {
            return typeof(ApplicationPOMModel);
        }

        StackPanel ITreeViewItem.Header()
        {           
            return NewTVItemFolderHeaderStyle(mPOMModelFolder);
        }

        public override ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            return new ApplicationPOMsTreeItem((RepositoryFolder<ApplicationPOMModel>)folder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is ApplicationPOMModel)
            {
                return new ApplicationPOMTreeItem((ApplicationPOMModel)item);
            }

            if (item is RepositoryFolderBase)
            {
                return new ApplicationPOMsTreeItem((RepositoryFolder<ApplicationPOMModel>)item);
            }

            throw new Exception("Error unknown item added to envs folder");
        }


        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            ObservableList<RepositoryFolder<ApplicationPOMModel>> subFolders = mPOMModelFolder.GetSubFolders();
            foreach (RepositoryFolder<ApplicationPOMModel> pomFolder in subFolders)
            {
                ApplicationPOMsTreeItem pomFTVI = new ApplicationPOMsTreeItem(pomFolder);
                Childrens.Add(pomFTVI);
            }
            subFolders.CollectionChanged -= TreeFolderItems_CollectionChanged; // untrack sub folders
            subFolders.CollectionChanged += TreeFolderItems_CollectionChanged; // track sub folders

            //Add direct children's        
            mChildPoms = mPOMModelFolder.GetFolderItems();
            mChildPoms.CollectionChanged -= TreeFolderItems_CollectionChanged;
            mChildPoms.CollectionChanged += TreeFolderItems_CollectionChanged;//adding event handler to add/remove tree items automatically based on folder items collection changes
            foreach (ApplicationPOMModel pom in mChildPoms.OrderBy(nameof(ApplicationPOMModel.Name)))
            {
                ApplicationPOMTreeItem pomTI = new ApplicationPOMTreeItem(pom);
                Childrens.Add(pomTI);
            }

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {

            if (mPOMModelsPage == null)
            {
                mPOMModelsPage = new POMModelsPage(mPOMModelFolder);
            }
            return mPOMModelsPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Add Page Objects Model", AddPOM, null, eImageType.Add);
            if (mPOMModelFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "Page Objects Model", allowAddNew: false, allowRefresh: false);

            AddSourceControlOptions(mContextMenu);
        }

        internal void AddPOM(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);
            WizardWindow.ShowWizard(new AddPOMWizard(mPOMModelFolder),1000,700, DoNotShowAsDialog:true);            
        }
    }
}
