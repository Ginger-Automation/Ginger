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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions.ApiActionsConversion;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems
{
    public class AppApiModelsFolderTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        public RepositoryFolder<ApplicationAPIModel> mAPIModelFolder;
        private APIModelsPage mAPIModelsPage;
        private ObservableList<ApplicationAPIModel> mChildAPIs = null;
        bool mShowEditInMenu = false;

        public AppApiModelsFolderTreeItem(RepositoryFolder<ApplicationAPIModel> apiModelFolder, bool ShowEditInMenu = false)
        {
            mAPIModelFolder = apiModelFolder;
            mShowEditInMenu = ShowEditInMenu;
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
                return new AppApiModelTreeItem((ApplicationAPIModel)item, mShowEditInMenu);
            }

            if (item is RepositoryFolderBase)
            {
                return new AppApiModelsFolderTreeItem((RepositoryFolder<ApplicationAPIModel>)item);
            }

            throw new Exception("Error unknown item added to envs folder");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return GetChildrentGeneric<ApplicationAPIModel>(mAPIModelFolder);
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            if (mAPIModelsPage == null)
            {
                mAPIModelsPage = new APIModelsPage(mAPIModelFolder);
            }

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
            TreeViewUtils.AddSubMenuItem(addMenu, "Convert Web services Actions", WebServiceActionsConversionHandler, null, eImageType.Convert);

            // for wiremock related changes
            MenuItem addWireMock = TreeViewUtils.CreateSubMenu(mContextMenu, "API Mode", eImageType.MapALM);
            TreeViewUtils.AddSubMenuItem(addWireMock, "Use Mock API", UseMockedAPIURL, null, eImageType.WireMockLogo);
            TreeViewUtils.AddSubMenuItem(addWireMock, "Use Live API", UseRealAPIURL, null, eImageType.APIModel);

            if (mAPIModelFolder.IsRootFolder)
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "API Model", allowAddNew: false, allowDeleteFolder: false, allowRenameFolder: false, allowRefresh: false, allowDeleteAllItems: true);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, "API Model", allowAddNew: false, allowRefresh: false, allowWireMockMapping: true);
            }

            AddSourceControlOptions(mContextMenu);
        }

        /// <summary>
        /// This event is used to handle the WebService Conversion Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebServiceActionsConversionHandler(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new ApiActionsConversionWizard(mAPIModelFolder), 900, 700);
        }

        private void AddSoapAPIModel(object sender, RoutedEventArgs e)
        {
            AddSingleAPIModel(ApplicationAPIUtils.eWebApiType.SOAP);
        }

        private void AddRESTAPIModel(object sender, RoutedEventArgs e)
        {
            AddSingleAPIModel(ApplicationAPIUtils.eWebApiType.REST);
        }

        private void UseMockedAPIURL(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationAPIModel apiModel in mAPIModelFolder.GetFolderItems())
            {
                apiModel.UseLiveAPI = false;
            }
            SaveModifiedActivities();
        }

        private void UseRealAPIURL(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationAPIModel apiModel in mAPIModelFolder.GetFolderItems())
            {
                apiModel.UseLiveAPI = true;
            }
            SaveModifiedActivities();
        }

        private void SaveModifiedActivities()
        {
            IEnumerable<ApplicationAPIModel> modifiedAPIModelListItems =
                mAPIModelFolder.GetFolderItems();

            foreach (ApplicationAPIModel item in modifiedAPIModelListItems)
            {
                SaveHandler.Save(item);
            }
        }

        private void AddSingleAPIModel(ApplicationAPIUtils.eWebApiType type)
        {
            string apiName = string.Empty; ;
            if (InputBoxWindow.GetInputWithValidation(string.Format("Add {0} API", type.ToString()), "API Name:", ref apiName))
            {
                ApplicationAPIModel newApi = new ApplicationAPIModel
                {
                    APIType = type,
                    Name = apiName,
                    ContainingFolder = mAPIModelFolder.FolderFullPath
                };
                mAPIModelFolder.AddRepositoryItem(newApi);
            }
        }

        public void AddAPIModelFromDocument(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.Solution.ApplicationPlatforms.Any(p => p.Platform == ePlatformType.WebServices))
            {
                mTreeView.Tree.ExpandTreeItem(this);
                WizardWindow.ShowWizard(new AddAPIModelWizard(mAPIModelFolder), 1000);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"Please Add at-least one Web Service platform based {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} to continue adding API Models");
            }
        }

        public override bool PasteCopiedTreeItem(object nodeItemToCopy, TreeViewItemGenericBase targetFolderNode, bool toRefreshFolder = true)
        {
            RepositoryItemBase copiedItem = CopyTreeItemWithNewName((RepositoryItemBase)nodeItemToCopy);
            if (copiedItem != null)
            {
                copiedItem.DirtyStatus = eDirtyStatus.NoTracked;
                AppApiModelTreeItem.HandleGlobalModelParameters(nodeItemToCopy, copiedItem);            // avoid generating new GUIDs for Global Model Parameters associated to API Model being copied
                ((RepositoryFolderBase)(((ITreeViewItem)targetFolderNode).NodeObject())).AddRepositoryItem(copiedItem);
                return true;
            }
            return false;
        }
    }
}
