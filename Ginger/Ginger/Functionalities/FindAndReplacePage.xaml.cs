using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Functionalities;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Actions;
using Ginger.BusinessFlowFolder;
using Ginger.BusinessFlowWindows;
using Ginger.Environments;
using Ginger.Run;
using Ginger.Run.RunSetActions;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GingerCore.General;

namespace Ginger.Functionalities
{
    public delegate void FindItemsFunction();
    public delegate List<FindItemType> GetSubItemsFunction();

    /// <summary>
    /// Interaction logic for FindAndReplacePage.xaml
    /// </summary>
    public partial class FindAndReplacePage : Page
    {
        private GenericWindow _pageGenericWin = null;
        private ObservableList<FoundItem> mFoundItemsList = new ObservableList<FoundItem>();
        private List<FindItemType> mMainItemsTypeList = new List<FindItemType>();
        private List<FindItemType> mSubItemsTypeList = new List<FindItemType>();
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

        public FindAndReplacePage(eContext context)
        {
            InitializeComponent();
            mContext = context;
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
            mReplaceView.GridColsView.Add(new GridColView() { Field = nameof(FoundItem.OriginObjectName), Header = "Item Name", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style= FindResource("@DataGridColumn_Bold") as Style });
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
                foreach(object item in xFoundItemsGrid.GetVisibileGridItems())
                    ((FoundItem)item).IsSelected = Status;
            }
        }

        private SearchConfig mSearchConfig { get; set; }

        RepositoryItemBase mContextSearchedItem = null;
        string mPageTitle = string.Empty;
        public void Init()
        {
            if (mContext == eContext.AutomatePage)
            {               
                HideItemsDropBox();
                mContextSearchedItem = App.BusinessFlow;
                App.PropertyChanged += App_PropertyChanged;
            }
            else if (mContext == eContext.RunsetPage)
            {
                HideItemsDropBox();
                mContextSearchedItem = App.RunsetExecutor.RunSetConfig;
                App.RunsetExecutor.PropertyChanged += RunsetExecutor_PropertyChanged;
            }
            else
            {
                App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;
            }

            FoundItem.SolutionFolder = App.UserProfile.Solution.Folder;

            mSearchConfig = new SearchConfig() { MatchCase = false, MatchAllWord = false };

            App.ObjFieldBinding(xMatchCaseCheckBox, CheckBox.IsCheckedProperty, mSearchConfig, nameof(SearchConfig.MatchCase));
            App.ObjFieldBinding(xMatchWholeWordCheckBox, CheckBox.IsCheckedProperty, mSearchConfig, nameof(SearchConfig.MatchAllWord));

            xFindWhatTextBox.KeyDown += new KeyEventHandler(xFindWhatTextBox_KeyDown);
            xFoundItemsGrid.MouseDoubleClick += LineDoubleClicked;
            mFindAndReplaceUtils.PropertyChanged += MFindAndReplaceUtils_PropertyChanged;

            xMainItemTypeComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xSubItemTypeComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xReplaceValueComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;

            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), Type = typeof(BusinessFlow), FindItems = FindItemsFromAllBusinessFlows });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Activity), Type = typeof(Activity), FindItems = FindItemsFromAllActivities });
            mMainItemsTypeList.Add(new FindItemType { Name = "Action", Type = typeof(Act), HasSubType = true, FindItems = FindItemsFromAllActions, GetSubItems = GetPlatformsActions });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Variable), Type = typeof(VariableBase), HasSubType = true, FindItems = FindItemsFromAllVariables, GetSubItems = GetVariables });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.RunSet), Type = typeof(RunSetActionBase), FindItems = FindItemsFromAllRanSets });
            mMainItemsTypeList.Add(new FindItemType { Name = "Application Model", Type = typeof(ApplicationModelBase), HasSubType = true, FindItems = FindItemsFromAllApplicationModels, GetSubItems = GetApplicationModels });

            xMainItemTypeComboBox.SelectedValuePath = nameof(FindItemType.Type);
            xMainItemTypeComboBox.DisplayMemberPath = nameof(FindItemType.Name);
            xMainItemTypeComboBox.ItemsSource = mMainItemsTypeList;

        }

        private void xFindWhatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Find();
        }

        private void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProfile.Solution))
                ClearUI();

        }

        private void RunsetExecutor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RunsetExecutor.RunSetConfig))
            {
                ClearUI();
                mContextSearchedItem = App.RunsetExecutor.RunSetConfig;
            }  
        }

        private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.BusinessFlow))
            {
                ClearUI();
                mContextSearchedItem = App.BusinessFlow;
            } 
        }

        private void HideItemsDropBox()
        {
            xMainItemTypeComboBox.Visibility = Visibility.Collapsed;
            xSubItemTypeComboBox.Visibility = Visibility.Collapsed;
            xRow0.Height = new GridLength(0);
        }

        private FoundItem mCurrentItem { get { return (FoundItem)xFoundItemsGrid.CurrentItem; } }

        private void LineDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ShowPage();
        }


        private void ShowPage()
        {
            if (mContext == eContext.SolutionPage)
            {
                if (mMainItemType.Type == typeof(Act))
                {
                    viewAction(mCurrentItem);
                }
                if (mMainItemType.Type == typeof(VariableBase))
                {
                    viewVariable(mCurrentItem);
                }
                if (mMainItemType.Type == typeof(Activity))
                {
                    viewActivity(mCurrentItem);
                }
                if (mMainItemType.Type == typeof(ApplicationModelBase))
                {
                    viewApplicationModel((ApplicationModelBase)mCurrentItem.OriginObject);
                }
                if(mMainItemType.Type == typeof(BusinessFlow))
                {
                    ViewBusinessFlow(mCurrentItem);
                }
            }
        }


        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string title;
                switch(mContext)
            {
                case eContext.AutomatePage:
                    title = string.Format("Find & Replace in '{0}' {1}", App.BusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    break;
                case eContext.RunsetPage:
                    title = string.Format("Find & Replace in '{0}' {1}", App.RunsetExecutor.RunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet));
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
                Reporter.ToUser(eUserMsgKeys.FindAndRepalceFieldIsEmpty, xReplaceLabel.Content);
                return;
            }

            if (mFoundItemsList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.FindAndReplaceListIsEmpty);
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
                    Reporter.ToUser(eUserMsgKeys.FindAndReplaceNoItemsToRepalce);
                    EnableDisableButtons(true);
                    return;
                }

                await Task.Run(() => ReplaceItems(FIList, mValueToReplace));
            }
            finally
            {
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
            if (mContext == eContext.SolutionPage && xMainItemTypeComboBox.SelectedItem == null)
            {
                Reporter.ToUser(eUserMsgKeys.FindAndRepalceFieldIsEmpty, xMainItemTypeLabel.Content);
                return;
            }
            if (string.IsNullOrEmpty(xFindWhatTextBox.Text))
            {
                Reporter.ToUser(eUserMsgKeys.FindAndRepalceFieldIsEmpty, xFindWhatLabel.Content);
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
            if (mContext == eContext.AutomatePage || mContext == eContext.RunsetPage)
            {
                FindItemsFromContextSearchedItem();
            }
            else
                mMainItemType.FindItems();
        }

        private void FindItemsFromContextSearchedItem()
        {
            mFindAndReplaceUtils.FindItemsByReflection(mContextSearchedItem, mContextSearchedItem, mFoundItemsList, mFindWhat, mSearchConfig, mContextSearchedItem, string.Empty, string.Empty);
        }

        private void FindItemsFromAllBusinessFlows()
        {
            ObservableList<BusinessFlow> BFs = App.LocalRepository.GetSolutionBusinessFlows();
            foreach (BusinessFlow BF in BFs)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                mFindAndReplaceUtils.FindItemsByReflection(BF, BF, mFoundItemsList, mFindWhat, mSearchConfig, BF, string.Empty, string.Empty);
            }

        }

        private void FindItemsFromAllActivities()
        {
            ObservableList<BusinessFlow> BFs = App.LocalRepository.GetSolutionBusinessFlows();
            foreach (BusinessFlow BF in BFs)
            {
                foreach (Activity activity in BF.Activities)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                    string path = BF.ItemName;
                    mFindAndReplaceUtils.FindItemsByReflection(activity, activity, mFoundItemsList, mFindWhat, mSearchConfig, BF, path, string.Empty);
                }
            }

            ObservableList<Activity> RepoActions = App.LocalRepository.GetSolutionRepoActivities();
            foreach (Activity activity in RepoActions)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                mFindAndReplaceUtils.FindItemsByReflection(activity, activity, mFoundItemsList, mFindWhat, mSearchConfig, activity, string.Empty, string.Empty);
            }
        }


        private void FindItemsFromAllActions()
        {
            ObservableList<BusinessFlow> BFs = App.LocalRepository.GetSolutionBusinessFlows();

            foreach (BusinessFlow BF in BFs)
            {
                foreach (Activity activitiy in BF.Activities)
                {
                    foreach (Act action in activitiy.Acts)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        if (mSubItemType == null || action.GetType() == mSubItemType)
                        {
                            string itemParent = BF.ItemName + @"\" + activitiy.ActivityName;
                            mFindAndReplaceUtils.FindItemsByReflection(action, action, mFoundItemsList, mFindWhat, mSearchConfig, BF, itemParent, string.Empty);
                        }
                    }
                }
            }

            ObservableList<Act> RepoActions = App.LocalRepository.GetSolutionRepoActions();
            foreach (Act action in RepoActions)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                string path = string.Empty;
                if (mSubItemType == null || action.GetType() == mSubItemType)
                    mFindAndReplaceUtils.FindItemsByReflection(action, action, mFoundItemsList, mFindWhat, mSearchConfig, action, path, string.Empty);
            }
        }


        private void FindItemsFromAllVariables()
        {
            ObservableList<VariableBase> SolutionVariables = App.UserProfile.Solution.Variables;

            foreach (VariableBase VB in SolutionVariables)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                string VariablePath = App.UserProfile.Solution.ItemName;
                if (mSubItemType == null || VB.GetType() == mSubItemType)
                    mFindAndReplaceUtils.FindItemsByReflection(VB, VB, mFoundItemsList, mFindWhat, mSearchConfig, App.UserProfile.Solution, VariablePath, string.Empty);
            }

            ObservableList<BusinessFlow> BFs = App.LocalRepository.GetSolutionBusinessFlows();
            foreach (BusinessFlow BF in BFs)
            {
                foreach (VariableBase VB in BF.Variables)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                    string BFVariablePath = BF.ItemName;
                    if (mSubItemType == null || VB.GetType() == mSubItemType)
                        mFindAndReplaceUtils.FindItemsByReflection(VB, VB, mFoundItemsList, mFindWhat, mSearchConfig, BF, BFVariablePath, string.Empty);
                }
                foreach (Activity activitiy in BF.Activities)
                {
                    foreach (VariableBase VB in activitiy.Variables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                        string ActivityVariablePath = BF.ItemName + @"\" + activitiy.ActivityName;
                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                            mFindAndReplaceUtils.FindItemsByReflection(VB, VB, mFoundItemsList, mFindWhat, mSearchConfig, BF, ActivityVariablePath, string.Empty);
                    }
                }
            }

            ObservableList<VariableBase> RepoVariables = App.LocalRepository.GetSolutionRepoVariables();
            foreach (VariableBase VB in RepoVariables)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                string path = @"SharedRepository\" + VB.ItemName;
                if (mSubItemType == null || VB.GetType() == mSubItemType)
                    mFindAndReplaceUtils.FindItemsByReflection(VB, VB, mFoundItemsList, mFindWhat, mSearchConfig, VB, path, string.Empty);
            }

        }


        private void FindItemsFromAllRanSets()
        {
            ObservableList<RunSetConfig> RunSets = App.LocalRepository.GetSolutionRunSets();
            foreach (RunSetConfig RSC in RunSets)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                mFindAndReplaceUtils.FindItemsByReflection(RSC, RSC, mFoundItemsList, mFindWhat, mSearchConfig, RSC, string.Empty, string.Empty);
            }
        }

        private void FindItemsFromAllApplicationModels()
        {
            ObservableList<ApplicationAPIModel> ApplicationModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();
            foreach (ApplicationAPIModel AAM in ApplicationModels)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping) return;
                string path = "Application API Models\\";
                if (mSubItemType == null || AAM.GetType() == mSubItemType)
                    mFindAndReplaceUtils.FindItemsByReflection(AAM, AAM, mFoundItemsList, mFindWhat, mSearchConfig, AAM, path, string.Empty);
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

        public void viewAction(FoundItem actionToView)
        {
            Act act = (Act)actionToView.OriginObject;
            RepositoryItemBase Parent = actionToView.ParentItemToSave;
            ActionEditPage w;
            if (Parent is BusinessFlow)
                w = new ActionEditPage(act, General.RepositoryItemPageViewMode.Child, Parent as BusinessFlow);
            else
                w = new ActionEditPage(act, General.RepositoryItemPageViewMode.SharedReposiotry);

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(actionToView);
        }

        public void viewVariable(FoundItem variableToViewFoundItem)
        {
            VariableBase variableToView = (VariableBase)variableToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = variableToViewFoundItem.ParentItemToSave;
            VariableEditPage w;
            if (Parent != null && (Parent is BusinessFlow || Parent is Solution))
                w = new VariableEditPage(variableToView, true, VariableEditPage.eEditMode.FindAndReplace, Parent);
            else
                w = new VariableEditPage(variableToView, true, VariableEditPage.eEditMode.SharedRepository);
            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(variableToViewFoundItem);
        }

        private void viewActivity(FoundItem activityToViewFoundItem)
        {
            Activity activity = (Activity)activityToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = (RepositoryItemBase)activityToViewFoundItem.ParentItemToSave;
            ActivityEditPage w;
            if (Parent is BusinessFlow)
                w = new ActivityEditPage(activity, General.RepositoryItemPageViewMode.Child, Parent as BusinessFlow);
            else
                w = new ActivityEditPage(activity, General.RepositoryItemPageViewMode.SharedReposiotry);
            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                RefreshFoundItemField(activityToViewFoundItem);
        }

        private void ViewBusinessFlow(FoundItem businessFlowToViewFoundItem)
        {
            BusinessFlow businessFlow = (BusinessFlow)businessFlowToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = (RepositoryItemBase)businessFlowToViewFoundItem.ParentItemToSave;
            BusinessFlowPage w = new BusinessFlowPage(businessFlow, false, General.RepositoryItemPageViewMode.View);
            w.Width = 1000;
            w.Height = 800;
            w.ShowAsWindow(); //Do we want to enable view Business flow window with save option also?
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
        private void viewApplicationModel(ApplicationModelBase applicationModelToView)
        {
            if (applicationModelToView is ApplicationAPIModel)
            {
                ApplicationAPIModel applicationAPIModel = applicationModelToView as ApplicationAPIModel;
                APIModelPage w = new APIModelPage(applicationAPIModel);
                w.ShowAsWindow(eWindowShowStyle.Free, true, APIModelPage.eEditMode.FindAndReplace);
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
                    if (foundItem.ParentItemToSave.UseNewRepositorySerializer)
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(foundItem.ParentItemToSave);
                    }
                    else
                    {
                        if (foundItem.ParentItemToSave is RepositoryItem)
                            ((RepositoryItem)foundItem.ParentItemToSave).Save();
                    }

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





    public class FindItemType
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool HasSubType { get; set; }
        public FindItemsFunction FindItems;
        public List<FindItemType> mSubItemsTypeList { get; set; }
        public GetSubItemsFunction GetSubItems { get; set; }
    }
}
