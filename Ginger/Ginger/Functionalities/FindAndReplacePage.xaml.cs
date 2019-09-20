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
using Amdocs.Ginger.Common.Functionalities;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using GingerWPF.ApplicationModelsLib.APIModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.Functionalities
{
    public delegate void GetItemsToSearchInFunction();
    public delegate List<FindItemType> GetSubItemsFunction();

    /// <summary>
    /// Interaction logic for FindAndReplacePage.xaml
    /// </summary>
    public partial class FindAndReplacePage : Page
    {
        private GenericWindow _pageGenericWin = null;
        object mItemToSearchOn;
        private ObservableList<FoundItem> mFoundItemsList = new ObservableList<FoundItem>();
        private List<FindItemType> mMainItemsTypeList = new List<FindItemType>();
        private List<FindItemType> mSubItemsTypeList = new List<FindItemType>();
        private List<ItemToSearchIn> mItemsToSearchIn = new List<ItemToSearchIn>();
        private FindItemType mMainItemType;
        private Type mSubItemType;
        private string mFindWhat;
        private string mValueToReplace;
        FindAndReplaceUtils mFindAndReplaceUtils = new FindAndReplaceUtils();

        public enum eContext
        {
            SolutionPage,
            AutomatePage,
            RunsetPage
        }

        public eContext mContext;


        public FindAndReplacePage(eContext context, object itemToSearchOn = null)
        {
            InitializeComponent();
            mContext = context;
            mItemToSearchOn = itemToSearchOn;
            SetFoundItemsGridView();
            Init();
        }

        private void MFindAndReplaceUtils_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender.GetType() == typeof(FindAndReplaceUtils))
            {
                switch (mFindAndReplaceUtils.ProcessingState)
                {
                    case FindAndReplaceUtils.eProcessingState.Pending:
                        Dispatcher.Invoke(() =>
                        {
                            xStopButton.Visibility = Visibility.Collapsed;
                        });
                        break;

                    case FindAndReplaceUtils.eProcessingState.Running:
                        Dispatcher.Invoke(() =>
                        {
                            xStopButton.ButtonText = "Stop";
                            xStopButton.Visibility = Visibility.Visible;
                        });
                        break;
                }
            }
        }

        public enum eGridView
        {
            FindView,
        }

        private void SetFoundItemsGridView()
        {
            //# Find View 
            //GridViewDef mReplaceView = new GridViewDef(eGridView.ReplaceView.ToString());
            GridViewDef mReplaceView = new GridViewDef(GridViewDef.DefaultViewName);
            mReplaceView.GridColsView = new ObservableList<GridColView>();
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.IsSelected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["IsSelectedTemplate"] });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.OriginObjectType), Header = "Item Type", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.OriginObjectName), Header = "Item Name", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.ParentItemPath), Header = "Item Path", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.ItemParent), Header = "Item Parent", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.FoundField), Header = "Found Field", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.FieldValue), Header = "Field Value", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style });

            //mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.StatusIcon), Header = "", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, AllowSorting = true, MaxWidth = 20 });
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.Status), Header = "Status", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });
            mReplaceView.GridColsView.Add(new GridColView() { Field = "View Details", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["ViewDetailsButton"] });
            xFoundItemsGrid.SetAllColumnsDefaultView(mReplaceView);

            GridViewDef mFineView = new GridViewDef(eGridView.FindView.ToString());
            mFineView.GridColsView = new ObservableList<GridColView>();
            mFineView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.IsSelected), Visible = false });
            mFineView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.Status), Visible = false });
            xFoundItemsGrid.AddCustomView(mFineView);


            xFoundItemsGrid.btnMarkAll.Visibility = Visibility.Visible;
            xFoundItemsGrid.ShowViewCombo = Visibility.Collapsed;

            xFoundItemsGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;

            xFoundItemsGrid.InitViewItems();
            xFoundItemsGrid.SetTitleLightStyle = true;
            xFoundItemsGrid.ChangeGridView(eGridView.FindView.ToString());
            xFoundItemsGrid.RowChangedEvent += RowChangedHandler;
            xFoundItemsGrid.DataSourceList = mFoundItemsList;
        }

        private void ClearUI()
        {
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            mFoundItemsList.Clear();
        }

        private void StopProcess(object sender, RoutedEventArgs e)
        {
            xStopButton.ButtonText = "Stopping...";
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Stopping;
        }

        private void RowChangedHandler(object sender, EventArgs e)
        {
            EnableDisableReplaceControlsAndFillReplaceComboBox();
        }

        private void EnableDisableReplaceControlsAndFillReplaceComboBox()
        {
            if (xFoundItemsGrid != null && xFoundItemsGrid.CurrentItem != null)
            {
                FoundItem FI = (FoundItem)xFoundItemsGrid.CurrentItem;
                PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);

                if (PI.PropertyType.BaseType == typeof(Enum) || PI.PropertyType == typeof(bool))
                {
                    xReplaceValueTextBox.Visibility = Visibility.Collapsed;
                    xReplaceValueComboBox.Visibility = Visibility.Visible;
                    GingerCore.General.FillComboFromList(xReplaceValueComboBox, FI.OptionalValuesToRepalce);
                }
                else
                {
                    xReplaceValueTextBox.Visibility = Visibility.Visible;
                    xReplaceValueComboBox.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (xReplaceValueTextBox != null && xReplaceValueComboBox != null)
                {
                    xReplaceValueTextBox.Visibility = Visibility.Visible;
                    xReplaceValueComboBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MarkUnMarkAllActions(bool Status)
        {
            if (xFoundItemsGrid.DataSourceList.Count <= 0) return;
            if (xFoundItemsGrid.DataSourceList.Count > 0)
            {
                foreach (object item in xFoundItemsGrid.GetVisibileGridItems())
                    ((FoundItem)item).IsSelected = Status;
            }
        }

        private SearchConfig mSearchConfig { get; set; }

        string mPageTitle = string.Empty;
        public void Init()
        {
            //if (mContext == eContext.AutomatePage)
            //    App.PropertyChanged += App_PropertyChanged;
            //else 
            if (mContext == eContext.RunsetPage)
            {
                WorkSpace.Instance.RunsetExecutor.PropertyChanged += RunsetExecutor_PropertyChanged;
                xReplaceRadioButton.Visibility = Visibility.Hidden;
            }

            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;


            FoundItem.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mSearchConfig = new SearchConfig() { MatchCase = false, MatchAllWord = false };
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMatchCaseCheckBox, CheckBox.IsCheckedProperty, mSearchConfig, nameof(SearchConfig.MatchCase));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMatchWholeWordCheckBox, CheckBox.IsCheckedProperty, mSearchConfig, nameof(SearchConfig.MatchAllWord));

            xFindWhatTextBox.KeyDown += new KeyEventHandler(xFindWhatTextBox_KeyDown);
            xFoundItemsGrid.MouseDoubleClick += LineDoubleClicked;
            mFindAndReplaceUtils.PropertyChanged += MFindAndReplaceUtils_PropertyChanged;

            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), Type = typeof(BusinessFlow), GetItemsToSearchIn = GetBusinessFlowsToSearchIn });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Activity), Type = typeof(Activity), GetItemsToSearchIn = GetActivitiesToSearchIn });
            mMainItemsTypeList.Add(new FindItemType { Name = "Action", Type = typeof(Act), HasSubType = true, GetItemsToSearchIn = GetActionsToSearchIn, GetSubItems = GetPlatformsActions });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Variable), Type = typeof(VariableBase), HasSubType = true, GetItemsToSearchIn = GetVariablesToSearchIn, GetSubItems = GetVariables });
            if (mContext == eContext.RunsetPage || mContext == eContext.SolutionPage)
            {
                mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.RunSet), Type = typeof(RunSetActionBase), GetItemsToSearchIn = GetRunSetsToSearchIn });
            }
            if (mContext == eContext.SolutionPage)
            {
                mMainItemsTypeList.Add(new FindItemType { Name = "Application Model", Type = typeof(ApplicationModelBase), HasSubType = true, GetItemsToSearchIn = GetApplicationModelsToSearchIn, GetSubItems = GetApplicationModels });
            }

            xMainItemTypeComboBox.SelectedValuePath = nameof(FindItemType.Type);
            xMainItemTypeComboBox.DisplayMemberPath = nameof(FindItemType.Name);
            xMainItemTypeComboBox.ItemsSource = mMainItemsTypeList;

        }

        private void xFindWhatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Find();
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                ClearUI();
            }
        }

        private void RunsetExecutor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RunsetExecutor.RunSetConfig))
            {
                ClearUI();
            }
        }

        //private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(App.BusinessFlow))
        //    {
        //        ClearUI();
        //    } 
        //}

        private FoundItem mCurrentItem { get { return (FoundItem)xFoundItemsGrid.CurrentItem; } }

        private void LineDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ShowPage();
        }


        private void ShowPage()
        {
            if (mMainItemType.Type == typeof(Act))
            {
                ViewAction(mCurrentItem);
            }
            if (mMainItemType.Type == typeof(VariableBase))
            {
                ViewVariable(mCurrentItem);
            }
            if (mMainItemType.Type == typeof(Activity))
            {
                ViewActivity(mCurrentItem);
            }
            if (mMainItemType.Type == typeof(ApplicationModelBase))
            {
                ViewApplicationModel(mCurrentItem);
            }
            if (mMainItemType.Type == typeof(BusinessFlow))
            {
                ViewBusinessFlow(mCurrentItem);
            }
            if (mMainItemType.Type == typeof(RunSetActionBase))
            {
                ViewRunSet(mCurrentItem);
            }
        }


        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string title;
            switch (mContext)
            {
                case eContext.AutomatePage:
                    title = string.Format("Find & Replace in '{0}' {1}", ((BusinessFlow)mItemToSearchOn).Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    break;
                case eContext.RunsetPage:
                    title = string.Format("Find in '{0}' {1}", WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet));
                    break;
                default:
                    title = "Find & Replace";
                    break;

            }
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this);
        }

        private void ReplaceButtonClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mValueToReplace))
            {
                Reporter.ToUser(eUserMsgKey.FindAndRepalceFieldIsEmpty, xReplaceLabel.Content);
                return;
            }

            if (mFoundItemsList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.FindAndReplaceListIsEmpty);
                return;
            }

            ReplaceItemsAsync();
        }

        private async void ReplaceItemsAsync()
        {
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Running;

            try
            {
                EnableDisableButtons(false);

                List<FoundItem> FIList = mFoundItemsList.Where(x => x.IsSelected == true && (x.Status == FoundItem.eStatus.PendingReplace || x.Status == FoundItem.eStatus.ReplaceFailed)).ToList();
                if (FIList.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.FindAndReplaceNoItemsToRepalce);
                    EnableDisableButtons(true);
                    return;
                }

                await Task.Run(() => ReplaceItems(FIList, mValueToReplace));
            }
            finally
            {
                if (mContext != eContext.AutomatePage)
                    xSaveButton.Visibility = Visibility.Visible;
                EnableDisableButtons(true);
                mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            }
        }

        private void ReplaceItems(List<FoundItem> FIList, string newValue)
        {
            foreach (FoundItem foundItem in FIList)
            {
                if (mFindAndReplaceUtils.ReplaceItem(mSearchConfig, mFindWhat, foundItem, newValue))
                {
                    foundItem.Status = FoundItem.eStatus.Replaced;
                }
                else
                {
                    foundItem.Status = FoundItem.eStatus.ReplaceFailed;
                }

            }
        }

        private void FindButtonClicked(object sender, RoutedEventArgs e)
        {
            Find();
        }

        private void Find()
        {
            if (xMainItemTypeComboBox.SelectedItem == null)
            {
                Reporter.ToUser(eUserMsgKey.FindAndRepalceFieldIsEmpty, xMainItemTypeLabel.Content);
                return;
            }
            if (string.IsNullOrEmpty(xFindWhatTextBox.Text))
            {
                Reporter.ToUser(eUserMsgKey.FindAndRepalceFieldIsEmpty, xFindWhatLabel.Content);
                return;
            }
            FindItemsAsync();
        }

        private async void FindItemsAsync()
        {
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Running;

            try
            {
                EnableDisableButtons(false);
                mFoundItemsList.Clear();

                xFoundItemsGrid.Visibility = Visibility.Visible;
                xProcessingImage.Visibility = Visibility.Visible;
                await Task.Run(() => FindItems());
                xProcessingImage.Visibility = Visibility.Collapsed;
            }
            finally
            {
                EnableDisableButtons(true);
                mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            }
        }

        private bool CurrentlyOnProcessIndicator = false;

        private void EnableDisableButtons(bool Enabled)
        {
            xReplaceButton.IsEnabled = Enabled;
            xFindButton.IsEnabled = Enabled;
            xSaveButton.IsEnabled = Enabled;
            CurrentlyOnProcessIndicator = !Enabled;
            xStopButton.IsEnabled = !Enabled;
        }

        private void FindItems()
        {
            mItemsToSearchIn.Clear();
            mMainItemType.GetItemsToSearchIn();

            foreach (ItemToSearchIn searchItem in mItemsToSearchIn)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                mFindAndReplaceUtils.FindItemsByReflection(searchItem.OriginItemObject, searchItem.Item, mFoundItemsList, mFindWhat, mSearchConfig, searchItem.ParentItemToSave, searchItem.ItemParent, searchItem.FoundField);
            }
        }

        private void GetBusinessFlowsToSearchIn()
        {
            switch (mContext)
            {
                case eContext.SolutionPage:
                    foreach (BusinessFlow BF in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
                        mItemsToSearchIn.Add(new ItemToSearchIn(BF, BF, BF, string.Empty, string.Empty));
                    break;
                case eContext.AutomatePage:
                    mItemsToSearchIn.Add(new ItemToSearchIn(((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn), string.Empty, string.Empty));
                    break;
                case eContext.RunsetPage:
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                        foreach (BusinessFlow bf in runner.BusinessFlows)
                            mItemsToSearchIn.Add(new ItemToSearchIn(bf, bf, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + bf.Name, string.Empty));
                    break;
            }
        }

        private void GetActivitiesToSearchIn()
        {
            switch (mContext)
            {
                case eContext.SolutionPage:
                    //Pull Activities from all businessflows
                    foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
                    {
                        foreach (Activity activity in bf.Activities)
                        {
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                            mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, bf, bf.Name, string.Empty));
                        }
                    }

                    //Pull Activities from shared repository
                    ObservableList<Activity> RepoActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    foreach (Activity activity in RepoActions)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, activity, string.Empty, string.Empty));
                    }
                    break;

                case eContext.AutomatePage:
                    //Pull Activities from current businessflows
                    foreach (Activity Activity in ((BusinessFlow)mItemToSearchOn).Activities)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        mItemsToSearchIn.Add(new ItemToSearchIn(Activity, Activity, ((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn).Name, string.Empty));
                    }
                    break;

                case eContext.RunsetPage:
                    //Pull Activities from runsets businessflows
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                        foreach (BusinessFlow BF in runner.BusinessFlows)
                        {
                            foreach (Activity activity in BF.Activities)
                            {
                                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                                mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + BF.Name, string.Empty));
                            }
                        }
                    break;
            }
        }

        private void GetActionsToSearchIn()
        {
            switch (mContext)
            {
                case eContext.SolutionPage:
                    //Pull actions from all solutions
                    foreach (BusinessFlow bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
                    {
                        foreach (Activity activity in bf.Activities)
                        {
                            string itemParent = bf.Name + @"\" + activity.ActivityName;
                            foreach (Act action in activity.Acts)
                            {
                                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                                if (mSubItemType == null || action.GetType() == mSubItemType)
                                {
                                    mItemsToSearchIn.Add(new ItemToSearchIn(action, action, bf, itemParent, string.Empty));
                                }
                            }
                        }
                    }

                    //Pull all shared repostory actions inside shared repository activities
                    ObservableList<Activity> RepoActivities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    foreach (Activity activity in RepoActivities)
                    {
                        foreach (Act action in activity.Acts)
                        {
                            string itemParent = activity.ItemName;
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                            if (mSubItemType == null || action.GetType() == mSubItemType)
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(action, action, activity, itemParent, string.Empty));
                            }
                        }
                    }

                    //Pull all shared repository actions
                    ObservableList<Act> RepoActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
                    foreach (Act action in RepoActions)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        if (mSubItemType == null || action.GetType() == mSubItemType)
                            mItemsToSearchIn.Add(new ItemToSearchIn(action, action, action, string.Empty, string.Empty));
                    }
                    break;

                case eContext.AutomatePage:
                    //Pull Activities from current businessflow
                    foreach (Activity activity in ((BusinessFlow)mItemToSearchOn).Activities)
                    {
                        string itemParent = ((BusinessFlow)mItemToSearchOn).Name + @"\" + activity.ActivityName;
                        foreach (Act action in activity.Acts)
                        {
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                            if (mSubItemType == null || action.GetType() == mSubItemType)
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(action, action, ((BusinessFlow)mItemToSearchOn), itemParent, string.Empty));
                            }
                        }
                    }
                    break;

                case eContext.RunsetPage:
                    //Pull Activities from businessflows inside runsets
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                        foreach (BusinessFlow bf in runner.BusinessFlows)
                        {
                            foreach (Activity activity in bf.Activities)
                            {
                                string itemParent = bf.Name + @"\" + activity.ActivityName;
                                foreach (Act action in activity.Acts)
                                {
                                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                                    if (mSubItemType == null || action.GetType() == mSubItemType)
                                    {
                                        mItemsToSearchIn.Add(new ItemToSearchIn(action, action, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + itemParent, string.Empty));
                                    }
                                }
                            }
                        }
                    break;
            }
        }


        private void GetVariablesToSearchIn()
        {
            switch (mContext)
            {
                case eContext.SolutionPage:
                    //Pull variables from solution global variables
                    foreach (VariableBase VB in WorkSpace.Instance.Solution.Variables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        string VariablePath = WorkSpace.Instance.Solution.Name + "\\Global Variables";
                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                            mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, WorkSpace.Instance.Solution, VariablePath, string.Empty));
                    }

                    //pull variables from all repository BF's
                    AddVariableFromBusinessFlowList(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>());

                    //pull variables from shared repository activities
                    ObservableList<Activity> RepoActivities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    foreach (Activity activity in RepoActivities)
                    {
                        foreach (VariableBase VB in activity.Variables)
                        {
                            string ActivityVariablePath = activity.ItemName;
                            if (mSubItemType == null || VB.GetType() == mSubItemType)
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, activity, ActivityVariablePath, string.Empty));
                        }
                    }
                    ObservableList<VariableBase> RepoVariables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                    foreach (VariableBase VB in RepoVariables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                            mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, VB, string.Empty, string.Empty));
                    }
                    break;

                case eContext.AutomatePage:
                    AddVariableFromBusinessFlowList(new ObservableList<BusinessFlow>() { ((BusinessFlow)mItemToSearchOn) });
                    break;

                case eContext.RunsetPage:
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                        AddVariableFromBusinessFlowList(runner.BusinessFlows, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\", WorkSpace.Instance.RunsetExecutor.RunSetConfig);
                    break;
            }
        }

        private void AddVariableFromBusinessFlowList(ObservableList<BusinessFlow> businessFlowList, string itemPathPrefix = "", RepositoryItemBase parent = null)
        {
            foreach (BusinessFlow BF in businessFlowList)
            {
                string BFVariableParent = itemPathPrefix + BF.Name;
                foreach (VariableBase VB in BF.Variables)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                    if (mSubItemType == null || VB.GetType() == mSubItemType)
                        mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, BF, BFVariableParent, string.Empty));
                }
                foreach (Activity activitiy in BF.Activities)
                {
                    string ActivityVariableParent = itemPathPrefix + BF.Name + @"\" + activitiy.ActivityName;
                    foreach (VariableBase VB in activitiy.Variables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                        {
                            if (parent == null)
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, BF, ActivityVariableParent, string.Empty));
                            else
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, parent, ActivityVariableParent, string.Empty));
                        }
                    }
                }
            }
        }

        private void GetRunSetsToSearchIn()
        {
            ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
            foreach (RunSetConfig RSC in RunSets)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                mItemsToSearchIn.Add(new ItemToSearchIn(RSC, RSC, RSC, string.Empty, string.Empty));
            }
        }

        private void GetApplicationModelsToSearchIn()
        {
            ObservableList<ApplicationAPIModel> ApplicationModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();
            foreach (ApplicationAPIModel AAM in ApplicationModels)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                if (mSubItemType == null || AAM.GetType() == mSubItemType)
                    mItemsToSearchIn.Add(new ItemToSearchIn(AAM, AAM, AAM, "Application API Models\\", string.Empty));
            }
        }


        private void FillSubItemTypeComboBox()
        {
            mSubItemsTypeList.Clear();
            FindItemType DefaultFindItem = new FindItemType { Name = "<All>", Type = null };


            if (mMainItemType.mSubItemsTypeList == null)
            {
                mSubItemsTypeList = mMainItemType.GetSubItems();
                mSubItemsTypeList.Add(DefaultFindItem);
                mMainItemType.mSubItemsTypeList = new List<FindItemType>(mSubItemsTypeList);

            }
            else
            {
                mSubItemsTypeList = new List<FindItemType>(mMainItemType.mSubItemsTypeList);

            }

            mSubItemsTypeList.Sort((a, b) => a.Name.CompareTo(b.Name));
            xSubItemTypeComboBox.ItemsSource = mSubItemsTypeList;
            xSubItemTypeComboBox.SelectedValuePath = nameof(FindItemType.Type);
            xSubItemTypeComboBox.DisplayMemberPath = nameof(FindItemType.Name);

            xSubItemTypeComboBox.SelectedIndex = 0;
        }

        private List<FindItemType> GetPlatformsActions()
        {
            ObservableList<Act> Acts = new ObservableList<Act>();
            List<FindItemType> ActsSubItemList = new List<FindItemType>();

            AppDomain.CurrentDomain.Load("GingerCore");

            //!!!!!!!!!!!! FIXME see add action page
            var ActTypes =
                // from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in typeof(Act).Assembly.GetTypes()
                where type.IsSubclassOf(typeof(Act))
                && type != typeof(ActWithoutDriver)
                select type;

            foreach (Type t in ActTypes)
            {
                Act a = (Act)Activator.CreateInstance(t);
                Acts.Add(a);
            }

            foreach (Act a in Acts)
            {
                if (!string.IsNullOrEmpty(a.ActionDescription))
                    ActsSubItemList.Add(new FindItemType { Name = a.ActionDescription, Type = a.GetType() });
            }

            return ActsSubItemList;
        }

        private List<FindItemType> GetApplicationModels()
        {
            ObservableList<ApplicationModelBase> ApplicationModels = new ObservableList<ApplicationModelBase>();
            List<FindItemType> APMsSubItemList = new List<FindItemType>();

            // !!! remove hard coded and use typeof
            AppDomain.CurrentDomain.Load("GingerCoreCommon");

            var ApplicationModelTypes =
                // from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in typeof(ApplicationModelBase).Assembly.GetTypes()
                where type.IsSubclassOf(typeof(ApplicationModelBase))
                select type;

            foreach (Type t in ApplicationModelTypes)
            {
                ApplicationModelBase a = (ApplicationModelBase)Activator.CreateInstance(t);
                ApplicationModels.Add(a);
            }

            foreach (ApplicationModelBase a in ApplicationModels)
            {
                if (!string.IsNullOrEmpty(a.ObjFolderName))
                    APMsSubItemList.Add(new FindItemType { Name = a.ObjFolderName, Type = a.GetType() });

            }

            return APMsSubItemList;
        }

        private List<FindItemType> GetVariables()
        {
            ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();
            List<FindItemType> VariablesSubItemList = new List<FindItemType>();

            // !!!! remove hard code and use typeof
            AppDomain.CurrentDomain.Load("GingerCoreCommon");

            var ApplicationModelTypes =
                from type in typeof(VariableBase).Assembly.GetTypes()
                where type.IsSubclassOf(typeof(VariableBase))
                select type;

            foreach (Type t in ApplicationModelTypes)
            {
                VariableBase a = (VariableBase)Activator.CreateInstance(t);
                Variables.Add(a);
            }

            foreach (VariableBase a in Variables)
            {
                if (!string.IsNullOrEmpty(a.VariableUIType))
                    VariablesSubItemList.Add(new FindItemType { Name = a.VariableUIType, Type = a.GetType() });

            }

            return VariablesSubItemList;
        }


        private void ItemTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearUI();
            mMainItemType = (FindItemType)xMainItemTypeComboBox.SelectedItem;

            if (mMainItemType.Type == typeof(Act) || mMainItemType.Type == typeof(ApplicationModelBase) || mMainItemType.Type == typeof(VariableBase))
            {
                xSubItemTypeLabel.Visibility = Visibility.Visible;
                xSubItemTypeComboBox.Visibility = Visibility.Visible;

                FillSubItemTypeComboBox();
            }
            else
            {
                xSubItemTypeLabel.Visibility = Visibility.Collapsed;
                xSubItemTypeComboBox.Visibility = Visibility.Collapsed;
            }
        }


        private void TextToFindTextBoxTextChanges(object sender, TextChangedEventArgs e)
        {
            mFindWhat = xFindWhatTextBox.Text;

            if (CurrentlyOnProcessIndicator && string.IsNullOrEmpty(xFindWhatTextBox.Text))
            {
                xFindButton.IsEnabled = false;
            }
            else
            {
                xFindButton.IsEnabled = true;
            }
        }

        public void ViewAction(FoundItem actionToView)
        {
            Act act = (Act)actionToView.OriginObject;
            RepositoryItemBase Parent = actionToView.ParentItemToSave;
            if (Parent is BusinessFlow)
            {
                act.Context = new Context() { BusinessFlow = (BusinessFlow)Parent };
            }
            ActionEditPage w;
            if (mContext == eContext.RunsetPage)
                w = new ActionEditPage(act, General.eRIPageViewMode.View);
            else if (mContext == eContext.AutomatePage)
                w = new ActionEditPage(act, General.eRIPageViewMode.Automation);
            else if (Parent is BusinessFlow)
                w = new ActionEditPage(act, General.eRIPageViewMode.ChildWithSave, Parent as BusinessFlow);
            else if (Parent is Activity)
                w = new ActionEditPage(act, General.eRIPageViewMode.ChildWithSave, actParentActivity: Parent as Activity);
            else
                w = new ActionEditPage(act, General.eRIPageViewMode.SharedReposiotry);

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(actionToView);
        }

        public void ViewVariable(FoundItem variableToViewFoundItem)
        {
            VariableBase variableToView = (VariableBase)variableToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = variableToViewFoundItem.ParentItemToSave;
            Context context = null;
            if (Parent is BusinessFlow)
            {
                context = new Context() { BusinessFlow = (BusinessFlow)Parent };
            }
            VariableEditPage w;
            if (mContext == eContext.RunsetPage)
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.View);
            if (mContext == eContext.AutomatePage)
            {
                if (Parent != null && Parent is BusinessFlow)
                    w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.Default, Parent as BusinessFlow);
                else if (Parent != null && Parent is Activity)
                    w = new VariableEditPage(variableToView, null, true, VariableEditPage.eEditMode.Default, Parent as Activity);
                else
                    w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.SharedRepository, Parent);
            }
            else if (Parent != null && (Parent is Solution || Parent is BusinessFlow || Parent is Activity))
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.FindAndReplace, Parent);
            else
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.SharedRepository);

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(variableToViewFoundItem);
        }

        private void ViewActivity(FoundItem activityToViewFoundItem)
        {
            Activity activity = (Activity)activityToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = (RepositoryItemBase)activityToViewFoundItem.ParentItemToSave;
            GingerWPF.BusinessFlowsLib.ActivityPage w;
            if (mContext == eContext.SolutionPage)
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context() {BusinessFlow = (BusinessFlow)Parent}, General.eRIPageViewMode.ChildWithSave);
            else if (mContext == eContext.AutomatePage)
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context(), General.eRIPageViewMode.Automation);
            else
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context(), General.eRIPageViewMode.View);

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(activityToViewFoundItem);
        }

        private void ViewBusinessFlow(FoundItem businessFlowToViewFoundItem)
        {
            BusinessFlow businessFlow = (BusinessFlow)businessFlowToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = (RepositoryItemBase)businessFlowToViewFoundItem.ParentItemToSave;
            GingerWPF.BusinessFlowsLib.BusinessFlowViewPage w = null;
            if (mContext == eContext.RunsetPage)
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.View);
            else if (mContext == eContext.AutomatePage)
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.Automation);
            else
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.Standalone);

            w.Width = 1000;
            w.Height = 800;
            if (w.ShowAsWindow() == true)
                RefreshFoundItemField(businessFlowToViewFoundItem);
        }

        private void ViewRunSet(FoundItem runSetToViewFoundItem)
        {
            Reporter.ToUser(eUserMsgKey.FindAndReplaceViewRunSetNotSupported, xReplaceLabel.Content);
            //MessageBox.Show()
            //RunSetConfig runSetConfig = (RunSetConfig)runSetToViewFoundItem.OriginObject;
            //NewRunSetPage w = new NewRunSetPage(runSetConfig, NewRunSetPage.eEditMode.View);
            //w.Width = 1000;
            //w.Height = 800;
            //w.ShowAsWindow();
        }

        private void RefreshFoundItemField(FoundItem actionToView)
        {
            Type PII = actionToView.OriginObject.GetType();
            PropertyInfo PI = actionToView.OriginObject.GetType().GetProperty(actionToView.FoundField);
            if (PI != null)
            {
                dynamic newValue = null;
                newValue = PI.GetValue(actionToView.OriginObject);
                if (newValue.ToString() != actionToView.FieldValue)
                {
                    actionToView.Status = FoundItem.eStatus.Saved;
                    actionToView.OnPropertyChanged(nameof(actionToView.OriginObjectName));
                    actionToView.FieldValue = newValue.ToString();
                }
            }
        }
        private void ViewApplicationModel(FoundItem applicationModelToViewFoundItem)
        {
            ApplicationModelBase applicationModelToView = (ApplicationModelBase)mCurrentItem.OriginObject;
            if (applicationModelToView is ApplicationAPIModel)
            {
                ApplicationAPIModel applicationAPIModel = applicationModelToView as ApplicationAPIModel;
                APIModelPage w = new APIModelPage(applicationAPIModel);

                if (w.ShowAsWindow(eWindowShowStyle.Dialog, false, APIModelPage.eEditMode.FindAndReplace) == true)
                    RefreshFoundItemField(applicationModelToViewFoundItem);
            }
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveAsync();
            }
            catch
            {

            }

        }

        private async void SaveAsync()
        {
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Running;

            try
            {
                EnableDisableButtons(false);
                List<FoundItem> FIList = mFoundItemsList.Where(x => x.IsSelected == true && (x.Status == FoundItem.eStatus.Replaced || x.Status == FoundItem.eStatus.SavedFailed)).ToList();

                await Task.Run(() => Save(FIList));
            }
            finally
            {
                EnableDisableButtons(true);
                mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            }
        }

        private void Save(List<FoundItem> FIList)
        {
            foreach (FoundItem foundItem in FIList)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;

                try
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(foundItem.ParentItemToSave);
                    foundItem.Status = FoundItem.eStatus.Saved;
                }
                catch
                {
                    foundItem.Status = FoundItem.eStatus.SavedFailed;
                }

            }
        }

        private void ViewDetailsClicked(object sender, RoutedEventArgs e)
        {
            ShowPage();
        }

        private void FindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (xReplaceValueTextBox != null)
            //    xReplaceValueTextBox.Visibility = Visibility.Collapsed;
            if (xReplaceLabel != null)
                xReplaceLabel.Visibility = Visibility.Collapsed;
            if (xReplaceButton != null)
                xReplaceButton.Visibility = Visibility.Collapsed;
            if (xRow3 != null)
                xRow3.Height = new GridLength(0);
            if (xFoundItemsGrid != null)
                xFoundItemsGrid.ChangeGridView(eGridView.FindView.ToString());
            EnableDisableReplaceControlsAndFillReplaceComboBox();
        }

        private void ReplaceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableReplaceControlsAndFillReplaceComboBox();
            xReplaceLabel.Visibility = Visibility.Visible;
            xReplaceButton.Visibility = Visibility.Visible;
            xRow3.Height = new GridLength(30);
            xFoundItemsGrid.ChangeGridView(GridViewDef.DefaultViewName);
        }

        private void SubItemTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearUI();
            mSubItemType = (Type)xSubItemTypeComboBox.SelectedValue;
        }

        private void ReplaceValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xReplaceValueComboBox != null && xReplaceValueComboBox.SelectedItem != null)
                mValueToReplace = xReplaceValueComboBox.SelectedItem.ToString();
        }

        private void ValueToReplaceTextBoxTextChanges(object sender, TextChangedEventArgs e)
        {
            mValueToReplace = xReplaceValueTextBox.Text;
        }
    }






}
