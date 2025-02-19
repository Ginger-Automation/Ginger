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
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages.AddActionMenu;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for APINavPage.xaml
    /// </summary>
    public partial class APINavPage : Page, INavPanelPage
    {
        Context mContext;
        ITreeViewItem mItemTypeRootNode;
        SingleItemTreeViewSelectionPage mAPIPage;

        public APINavPage(Context context)
        {
            InitializeComponent();

            mContext = context;

            ConfigureAPIPage();
            string allProperties = string.Empty;
            PropertyChangedEventManager.AddHandler(source: mContext, handler: MContext_PropertyChanged, propertyName: allProperties);
        }

        private void ConfigureAPIPage()
        {
            AppApiModelsFolderTreeItem mAPIsRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>(), true);
            mItemTypeRootNode = mAPIsRoot;
            mAPIPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationAPIModel.TargetApplicationKey) + "." + nameof(ApplicationAPIModel.TargetApplicationKey.ItemName), mContext.Activity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals, showAlerts: false);

            mItemTypeRootNode.SetTools(mAPIPage.xTreeView);
            mAPIPage.xTreeView.SetTopToolBarTools(mAPIsRoot.SaveAllTreeFolderItemsHandler, mAPIsRoot.AddAPIModelFromDocument, RefreshTreeItems, AddActionToListHandler);

            xAPIFrame.ClearAndSetContent(mAPIPage);
        }
        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {
                if (e.PropertyName is nameof(mContext.Activity) or nameof(mContext.Target))
                {
                    UpdateAPITree();
                }
            }
        }

        public void RefreshTreeItems(object sender, RoutedEventArgs e)
        {
            UpdateAPITree();
        }

        public void AddActionToListHandler(object sender, RoutedEventArgs e)
        {
            if (mAPIPage.xTreeView.Tree.CurrentSelectedTreeViewItem != null)
            {
                if (mAPIPage.xTreeView.Tree.CurrentSelectedTreeViewItem is GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems.AppApiModelTreeItem selectedItem)
                {
                    BusinessFlowPages.ActionsFactory.AddActionsHandler(selectedItem.mApiModel, mContext);
                }
            }
        }

        private void UpdateAPITree()
        {
            if (mContext.Activity != null)
            {
                ConfigureAPIPage();
            }
        }

        public void ReLoadPageItems()
        {
            UpdateAPITree();
        }
    }
}
