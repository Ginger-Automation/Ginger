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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.AddActionMenu;
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

        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (xMainElementsGrid.Grid.SelectedItem != null)
                {
                    return (ElementInfo)xMainElementsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public POMNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;

            ApplicationPOMsTreeItem mPOMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            mItemTypeRootNode = mPOMsRoot;
            mPOMPage = new SingleItemTreeViewSelectionPage("Page Object Models", eImageType.ApplicationPOMModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.Activity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals, showAlerts: false);

            mItemTypeRootNode.SetTools(mPOMPage.xTreeView);
            mPOMPage.xTreeView.SetTopToolBarTools(mPOMsRoot.SaveAllTreeFolderItemsHandler, mPOMsRoot.AddPOM, RefreshTreeItems);

            mContext.PropertyChanged += MContext_PropertyChanged;
            mPOMPage.OnSelect += MainTreeView_ItemSelected;
            SetElementsGridView();
            xPOMFrame.Content = mPOMPage;
            xPOMSplitter.Visibility = Visibility.Collapsed;
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
                }
            }
        }

        private void UpdatePOMTree()
        {
            CollapseDetailsGrid();

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
            GridLength POMItemsSelected = new GridLength(1, GridUnitType.Auto);
            GridLength POMDetailsPanelLoaded = new GridLength(100, GridUnitType.Star);

            if (e.SelectedItems != null && e.SelectedItems.Count == 1)
            {
                mPOM = e.SelectedItems[0] as ApplicationPOMModel;
                if (mPOM != null)
                {
                    foreach (ElementInfo elem in mPOM.MappedUIElements)
                    {
                        elem.ParentGuid = mPOM.Guid;
                    }
                    xMainElementsGrid.DataSourceList = mPOM.MappedUIElements;
                    xMainElementsGrid.Visibility = Visibility.Visible;
                    xPOMSplitter.Visibility = Visibility.Visible;

                    if (xPOMDetails.Height != POMDetailsPanelLoaded)
                    {
                        xPOMItems.Height = POMItemsSelected;
                        xPOMDetails.Height = POMDetailsPanelLoaded;
                    }
                }
            }
            else
            {
                CollapseDetailsGrid();
            }
        }

        public void RefreshTreeItems(object sender, RoutedEventArgs e)
        {
            UpdatePOMTree();
        }

        void CollapseDetailsGrid()
        {
            xMainElementsGrid.Visibility = Visibility.Collapsed;
            xPOMSplitter.Visibility = Visibility.Collapsed;
            xPOMItems.Height = new GridLength(100, GridUnitType.Star);
            xPOMDetails.Height = new GridLength(0);
        }

        private void SetElementsGridView()
        {
            xMainElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 25, AllowSorting = true });

            List<GingerCore.GeneralLib.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true });
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            xMainElementsGrid.AddToolbarTool(eImageType.GoBack, "Add to Actions", new RoutedEventHandler(AddFromPOMNavPage));
        }

        private void AddFromPOMNavPage(object sender, RoutedEventArgs e)
        {
            if (xMainElementsGrid.Grid.SelectedItems != null && xMainElementsGrid.Grid.SelectedItems.Count > 0)
            {
                foreach (ElementInfo elemInfo in xMainElementsGrid.Grid.SelectedItems)
                {
                    ActionsFactory.AddActionsHandler(elemInfo, mContext);
                }
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.HighLightElement(mSelectedElement, true);
            }
        }
        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return false;
            }

            return true;
        }
        private bool IsDriverBusy()
        {
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReLoadPageItems()
        {
            UpdatePOMTree();
            mAgent = mContext.Agent;
        }
    }
}
