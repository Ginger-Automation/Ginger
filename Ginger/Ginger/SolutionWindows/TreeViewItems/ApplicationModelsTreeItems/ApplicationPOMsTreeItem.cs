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
        POMEditPage mPOMEditPage;

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
            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@Documents_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }
            return TreeViewUtils.CreateItemHeader("POMs", ImageFile, SourceControlIntegration.GetItemSourceControlImage(mPOMModelFolder.DisplayName, ref ItemSourceControlStatus));
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            foreach (RepositoryFolder<ApplicationPOMModel> POMFolder in mPOMModelFolder.GetSubFolders())
            {
                ApplicationPOMsTreeItem  apiFTVI = new ApplicationPOMsTreeItem(POMFolder);
                Childrens.Add(apiFTVI);
            }

            //Add direct childrens             
            foreach (ApplicationPOMModel api in mPOMModelFolder.GetFolderItems())
            {
                ApplicationPOMTreeItem apiTI = new ApplicationPOMTreeItem(api);
                Childrens.Add(apiTI);
            }

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {

            if (mPOMEditPage == null)
            {
                mPOMEditPage = new POMEditPage(new ApplicationPOMModel());
            }
            return mPOMEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();
            AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: "Document", allowSaveAll: false, allowAddNew: false, allowCopyItems: false, allowCutItems: false, allowPaste: false);
            TreeViewUtils.AddMenuItem(mContextMenu, "Add POM", AddPOM, null, "@Save_16x16.png");
            mTreeView.AddToolbarTool("@Save_16x16.png", "Save", AddPOM);
            AddSourceControlOptions(mContextMenu, false, false);
        }

        internal void AddPOM(object sender, RoutedEventArgs e)
        {            
            WizardWindow.ShowWizard(new AddPOMWizard());            
        }
    }
}
