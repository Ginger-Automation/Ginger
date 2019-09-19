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
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class AppApiModelsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<ApplicationAPIModel> mAPIModelFolder;
        private APIModelsPage mAPIModelsPage;
        private ObservableList<ApplicationAPIModel> mChildAPIs = null;

        public AppApiModelsFolderTreeItem(RepositoryFolder<ApplicationAPIModel> apiModelFolder)
        {
            mAPIModelFolder = apiModelFolder;
        }

        Object ITreeViewItem.NodeObject()
        {
            return mAPIModelFolder;//in case of folder we return it RepositoryFolder to allow manipulating it from tree
        }

        override public string NodePath()
        {
            return mAPIModelFolder.FolderFullPath;
        }

        override public Type NodeObjectType()
        {
            return typeof(ApplicationAPIModel);
        }

        StackPanel ITreeViewItem.Header()
        {
            return NewTVItemFolderHeaderStyle(mAPIModelFolder);
        }

        public override ITreeViewItem GetFolderTreeItem(RepositoryFolderBase folder)
        {
            return new AppApiModelsFolderTreeItem((RepositoryFolder<ApplicationAPIModel>)folder);
        }

        public override ITreeViewItem GetTreeItem(object item)
        {
            if (item is ApplicationAPIModel)
            {
                return new AppApiModelTreeItem((ApplicationAPIModel)item);
            }

            if (item is RepositoryFolderBase)
            {
                return new AppApiModelsFolderTreeItem((RepositoryFolder<ApplicationAPIModel>)item);
            }

            throw new Exception("Error unknown item added to envs folder");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            ObservableList<RepositoryFolder<ApplicationAPIModel>> subFolders = mAPIModelFolder.GetSubFolders();
            foreach (RepositoryFolder<ApplicationAPIModel> apiFolder in subFolders)
            {
                AppApiModelsFolderTreeItem apiFTVI = new AppApiModelsFolderTreeItem(apiFolder);
                Childrens.Add(apiFTVI);
            }
            subFolders.CollectionChanged -= TreeFolderItems_CollectionChanged; // untrack sub folders
            subFolders.CollectionChanged += TreeFolderItems_CollectionChanged; // track sub folders

            //Add direct children's        
            mChildAPIs = mAPIModelFolder.GetFolderItems();
            mChildAPIs.CollectionChanged -= TreeFolderItems_CollectionChanged;
            mChildAPIs.CollectionChanged += TreeFolderItems_CollectionChanged;//adding event handler to add/remove tree items automatically based on folder items collection changes
            foreach (ApplicationAPIModel api in mChildAPIs.OrderBy(nameof(ApplicationAPIModel.Name)))
            {
                AppApiModelTreeItem apiTI = new AppApiModelTreeItem(api);
                Childrens.Add(apiTI);
            }

            return Childrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if(mAPIModelsPage == null)
                mAPIModelsPage = new APIModelsPage(mAPIModelFolder);
            return mAPIModelsPage;
        }


        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            MenuItem addMenu = TreeViewUtils.CreateSubMenu(mContextMenu, "Add API Model", eImageType.Add);
            TreeViewUtils.AddSubMenuItem(addMenu, "Import API's", AddAPIModelFromDocument, null, eImageType.Download);
            TreeViewUtils.AddSubMenuItem(addMenu, "SOAP API Model", AddSoapAPIModel, null, eImageType.APIModel);
            TreeViewUtils.AddSubMenuItem(addMenu, "REST API Model", AddRESTAPIModel, null, eImageType.APIModel);
            if (mAPIModelFolder.IsRootFolder)
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "API Model", allowAddNew:false, allowDeleteFolder:false, allowRenameFolder:false, allowRefresh: false, allowDeleteAllItems: true);
            else
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "API Model", allowAddNew: false, allowRefresh: false);

            AddSourceControlOptions(mContextMenu);
        }

        private void AddSoapAPIModel(object sender, RoutedEventArgs e)
        {
            AddSingleAPIModel(ApplicationAPIUtils.eWebApiType.SOAP);
        }

        private void AddRESTAPIModel(object sender, RoutedEventArgs e)
        {
            AddSingleAPIModel(ApplicationAPIUtils.eWebApiType.REST);
        }

        private void AddSingleAPIModel(ApplicationAPIUtils.eWebApiType type)
        {
            string apiName = string.Empty; ;
            if (InputBoxWindow.GetInputWithValidation(string.Format("Add {0} API",type.ToString()), "API Name:", ref apiName))
            {
                ApplicationAPIModel newApi = new ApplicationAPIModel();
                newApi.APIType = type;
                newApi.Name = apiName;
                newApi.ContainingFolder = mAPIModelFolder.FolderFullPath;
                mAPIModelFolder.AddRepositoryItem(newApi);
            }
        }

        public void AddAPIModelFromDocument(object sender, RoutedEventArgs e)
        {
            mTreeView.Tree.ExpandTreeItem((ITreeViewItem)this);
            WizardWindow.ShowWizard(new AddAPIModelWizard(mAPIModelFolder), 1000);
        }

        public override bool PasteCopiedTreeItem(object nodeItemToCopy, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)nodeItemToCopy);
            if (copiedItem != null)
            {
                AppApiModelTreeItem.HandleGlobalModelParameters(nodeItemToCopy, copiedItem);            // avoid generating new GUIDs for Global Model Parameters associated to API Model being copied
                ((RepositoryFolderBase)(((ITreeViewItem)targetFolderNode).NodeObject())).AddRepositoryItem(copiedItem);
                return true;
            }
            return false;
        }
    }
}
