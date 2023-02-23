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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.BusinessFlowPages.AddActionMenu;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.BusinessFlowWindows;
using Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        ElementInfoListViewHelper mPOMListHelper;
        ActivitiesRepositoryPage mActivitiesRepositoryViewPage;
        private Agent mAgent;

        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Status == Agent.eStatus.Running)
                {
                    return ((AgentOperations)mAgent.AgentOperations).Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.AgentOperations.Close();
                    }
                    return null;
                }
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (xPomElementsListView.List.SelectedItem != null)
                {
                    return (ElementInfo)xPomElementsListView.List.SelectedItem;
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

            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEventAsync;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEventAsync;

            mContext = context;

            xPomElementsListView.ListTitleVisibility = Visibility.Hidden;
            mPOMListHelper = new ElementInfoListViewHelper(mContext, General.eRIPageViewMode.AddFromModel);
            xPomElementsListView.SetDefaultListDataTemplate(mPOMListHelper);
            xPomElementsListView.ListSelectionMode = SelectionMode.Extended;
            mPOMListHelper.ListView = xPomElementsListView;

            ConfigurePOMPage();
            mContext.PropertyChanged += MContext_PropertyChanged;
        }

        private void ConfigurePOMPage()
        {
            ApplicationPOMsTreeItem mPOMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            mItemTypeRootNode = mPOMsRoot;
            mPOMPage = new SingleItemTreeViewSelectionPage("Page Object Models", eImageType.ApplicationPOMModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                        new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.Activity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals, showAlerts: false);
            mItemTypeRootNode.SetTools(mPOMPage.xTreeView);
            mPOMPage.xTreeView.SetTopToolBarTools(mPOMsRoot.SaveAllTreeFolderItemsHandler, mPOMsRoot.AddPOM, RefreshTreeItems);
            mPOMPage.OnSelect += MainTreeView_ItemSelected;
            mPOMPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            mPOMPage.xTreeView.HorizontalAlignment = HorizontalAlignment.Stretch;
            mPOMPage.xTreeView.SetAddButtonToArrow();
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
                    mPOMListHelper.Context.Agent = mContext.Agent;
                }
            }
        }

        private void UpdatePOMTree()
        {
            if (mContext.Activity != null)
            {
                ConfigurePOMPage();
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
                    xPomElementsListView.DataSourceList = mPOM.MappedUIElements;
                    xPomElementsListView.Visibility = Visibility.Visible;
                    xPOMSplitter.IsEnabled = true;

                    if (WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mPOM.TargetApplicationKey) == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                    {
                        //Suggested Activities to show
                        ObservableList<Activity> suggestedActivities = AutoGenerateFlows.CreatePOMActivitiesFromMetadata(mPOM);

                        mActivitiesRepositoryViewPage = new ActivitiesRepositoryPage(suggestedActivities, mContext, true);
                        xSharedActivitiesFrame.Content = mActivitiesRepositoryViewPage;
                        xSuggestedActivitiesTabItem.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xSuggestedActivitiesTabItem.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                xPOMDetails.Height = new GridLength(0, GridUnitType.Star);
                xPomElementsListView.DataSourceList = null;
                xPomElementsListView.Visibility = Visibility.Hidden;
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

        private async void App_AutomateBusinessFlowEventAsync(AutomateEventArgs args)
        {
            switch (args.EventType)
            {
                case AutomateEventArgs.eEventType.HighlightElement:
                    HighlightElementClicked();
                    break;
                default:
                    //Avoid other operations
                    break;
            }
        }

        private void HighlightElementClicked()
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
            if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
