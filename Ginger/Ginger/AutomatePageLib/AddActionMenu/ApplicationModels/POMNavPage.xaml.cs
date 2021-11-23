#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.AddActionMenu;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ApplicationModelsLib.POMModels.PomElementsPage;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for POMNavAction.xaml
    /// </summary>
    public partial class POMNavPage : Page, INavPanelPage
    {
        public PomElementsPage mappedUIElementsPage;
        ApplicationPOMModel mPOM;
        Context mContext;
        ITreeViewItem mItemTypeRootNode;
        SingleItemTreeViewSelectionPage mPOMPage;
        Ginger.General.eRIPageViewMode mPageViewMode;
        ActionsPOMListViewHelper mPOMListHelper;

        private Agent mAgent;

        public POMNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;

            xMainElementsListView.ListTitleVisibility = Visibility.Hidden;
            mPOMListHelper = new ActionsPOMListViewHelper(mContext, mPageViewMode);
            xMainElementsListView.SetDefaultListDataTemplate(mPOMListHelper);
            xMainElementsListView.ListSelectionMode = SelectionMode.Extended;
            mPOMListHelper.ListView = xMainElementsListView;

            ApplicationPOMsTreeItem mPOMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            mItemTypeRootNode = mPOMsRoot;
            mPOMPage = new SingleItemTreeViewSelectionPage("Page Object Models", eImageType.ApplicationPOMModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.Activity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals, showAlerts: false);
            mItemTypeRootNode.SetTools(mPOMPage.xTreeView);
            mPOMPage.xTreeView.SetTopToolBarTools(mPOMsRoot.SaveAllTreeFolderItemsHandler, mPOMsRoot.AddPOM, RefreshTreeItems);
            mContext.PropertyChanged += MContext_PropertyChanged;
            mPOMPage.OnSelect += MainTreeView_ItemSelected;
            //SetElementsGridView();
            mPOMPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            mPOMPage.xTreeView.HorizontalAlignment = HorizontalAlignment.Stretch;
            mPOMPage.Width = Double.NaN;
            xPOMFrame.Content = mPOMPage;
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {
                if (e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
                {
                    UpdatePOMTree();
                }
                if (e.PropertyName is nameof(mContext.Agent) || e.PropertyName is nameof(mContext.AgentStatus))
                {
                    mAgent = mContext.Agent;
                    mPOMListHelper.mContext.Agent = mContext.Agent;
                }
            }
        }

        private void UpdatePOMTree()
        {
            if (mContext.Activity != null)
            {
                mPOMPage.xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.Activity.TargetApplication);
                mPOMPage.xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
                mPOMPage.xTreeView.Tree.SelectItem(mItemTypeRootNode);
                mPOMPage.xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
            }
        }
        
        private void MainTreeView_ItemSelected(object sender, SelectionTreeEventArgs e)
        {
            if (e.SelectedItems != null && e.SelectedItems.Count == 1)
            {
                mPOM = e.SelectedItems[0] as ApplicationPOMModel;
                if (mPOM != null)
                {
                    foreach (ElementInfo elem in mPOM.MappedUIElements)
                    {
                        elem.ParentGuid = mPOM.Guid;
                    }
                    mPOM.StartDirtyTracking();
                    xPOMDetails.Height = xPOMItems.Height;
                    xMainElementsListView.DataSourceList = mPOM.MappedUIElements;
                    xMainElementsListView.Visibility = Visibility.Visible;
                    xPOMSplitter.IsEnabled = true;
                }
            }
            else
            {
                xPOMDetails.Height = new GridLength(0, GridUnitType.Star);
                xMainElementsListView.DataSourceList = null;
                xMainElementsListView.Visibility = Visibility.Hidden;
                xPOMSplitter.IsEnabled = false;
            }
        }        

        public void RefreshTreeItems(object sender, RoutedEventArgs e)
        {
            UpdatePOMTree();
        }

        public void ReLoadPageItems()
        {
            UpdatePOMTree();
            mAgent = mContext.Agent;
        }
    }
}
