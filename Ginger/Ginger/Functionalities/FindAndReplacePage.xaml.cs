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
using Amdocs.Ginger.Common.Functionalities;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static Ginger.AutomatePageLib.AddActionMenu.SharedRepositoryLib.BulkUpdateSharedRepositoryActivitiesPage;

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
        private ObservableList<FoundItem> mFoundItemsList = [];
        private List<FindItemType> mMainItemsTypeList = [];
        private List<FindItemType> mSubItemsTypeList = [];
        private List<ItemToSearchIn> mItemsToSearchIn = [];
        private FindItemType mMainItemType;
        private Type mSubItemType;
        private string mFindWhat;
        private string mValueToReplace;
        FindAndReplaceUtils mFindAndReplaceUtils = new FindAndReplaceUtils();

        public enum eContext
        {
            SolutionPage,
            AutomatePage,
            RunsetPage,
            FolderItems
        }

        public eContext mContext;

        public object ItemToSearchOn => mItemToSearchOn;
        List<ITreeViewItem> mTreeViewChildItems;

        public FindAndReplacePage(eContext context, object itemToSearchOn = null, List<ITreeViewItem> childNodes = null)
        {
            InitializeComponent();
            mContext = context;
            mItemToSearchOn = itemToSearchOn;
            mTreeViewChildItems = childNodes;
            SetFoundItemsGridView();
            Init();
            PageButton();



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
            ReplaceView,
            ReplaceAttributeValueView,
            ReplaceAttributeCheckBoxView,
            ReplaceAttributeComboBoxView,
            ReplaceAttributeTextBoxView
        }


        List<ComboEnumItem> valueTypes;
        private void SetFoundItemsGridView()
        {
            try
            {
                // Replace View
                GridViewDef DefaultViewName = new GridViewDef(GridViewDef.DefaultViewName)
                {
                    GridColsView =
                    [
                        new GridColView() { Field = nameof(FoundItem.IsSelected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["IsSelectedTemplate"] },
                new GridColView() { Field = nameof(FoundItem.OriginObjectType), Header = "Item Type", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true },
                new GridColView() { Field = nameof(FoundItem.OriginObjectName), Header = "Item Name", WidthWeight = 20, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style },
                new GridColView() { Field = nameof(FoundItem.ParentItemPath), Header = "Item Path", WidthWeight = 20, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true },
                new GridColView() { Field = nameof(FoundItem.ItemParent), Header = "Item Parent", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true },
                new GridColView() { Field = nameof(FoundItem.FoundField), Header = "Found Field", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style },
                new GridColView() { Field = nameof(FoundItem.FieldValue), Header = "Field Value", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true, Style = FindResource("@DataGridColumn_Bold") as Style },

                new GridColView() { Field = "FieldValueCheckBox", Header = "Field Value", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Visible = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["xAttributeValueCheckBoxTemplate"] },


                  /*  new GridColView() { Field = "FieldValueComboBox", Header = "Field Value", WidthWeight = 15, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Visible = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["xAttributeValueComboBoxTemplate"] , CellValuesList = valueTypes },
*//*
                      new GridColView()
                    {
                        Header ="Field Value" ,
                        Field = "FieldValueComboBox",
                        CellTemplate = (DataTemplate)this.FindAndReplace.Resources["xAttributeValueComboBoxTemplate"],
                        StyleType = GridColView.eGridColStyleType.Template,
                        WidthWeight = 20,
                     
                    },*/

                new GridColView() { Field = "FieldValueComboBox", Header ="Field Value", WidthWeight = 20, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(FoundItem.FieldValueOption), nameof(FoundItem.FieldValue), true,comboSelectionChangedHandler:FieldValueComboBox_SelectionChanged) },

                new GridColView() { Field = "FieldValueTextBox", Header = "Field Value", WidthWeight = 15, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, Visible = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["xAttributeValueTextBoxTemplate"] },

                new GridColView() { Field = nameof(FoundItem.Status), Header = "Status", WidthWeight = 10, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true },

                new GridColView() { Field = "View Details", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.FindAndReplace.Resources["ViewDetailsButton"] },
            ]
                };
                xFoundItemsGrid.SetAllColumnsDefaultView(DefaultViewName);

                // Find View
                GridViewDef ReplaceView = new GridViewDef(eGridView.ReplaceView.ToString())
                {
                    GridColsView = [
                        new GridColView() { Field = "FieldValueCheckBox", Visible = false },
                        new GridColView() { Field = "FieldValueComboBox", Visible = false },
                        new GridColView() { Field = "FieldValueTextBox", Visible = false },]
                };
                xFoundItemsGrid.AddCustomView(ReplaceView);

                GridViewDef mFineView = new GridViewDef(eGridView.FindView.ToString())
                {
                    GridColsView = [
                        new GridColView() { Field = nameof(FoundItem.Status), Visible = false },
                        new GridColView() { Field = "FieldValueCheckBox", Visible = false },
                        new GridColView() { Field = "FieldValueComboBox", Visible = false },
                        new GridColView() { Field = "FieldValueTextBox", Visible = false },]
                };
                xFoundItemsGrid.AddCustomView(mFineView);


                xFoundItemsGrid.ChangeGridView(eGridView.FindView.ToString());

                GridViewDef ReplaceAttributeValueView = new GridViewDef(nameof(eGridView.ReplaceAttributeValueView))
                {
                    GridColsView = [
                       new GridColView() { Field = "View Details", Visible = false },
                       new GridColView() { Field = "FieldValueCheckBox", Visible = false },
                       new GridColView() { Field = "FieldValueComboBox", Visible = false },
                       new GridColView() { Field = "FieldValueTextBox", Visible = false },]
                };
                xFoundItemsGrid.AddCustomView(ReplaceAttributeValueView);

                GridViewDef ReplaceAttributeTextBoxView = new GridViewDef(eGridView.ReplaceAttributeTextBoxView.ToString())
                {
                    GridColsView = [
                       new GridColView() { Field = "View Details", Visible = false },
                       new GridColView() { Field = nameof(FoundItem.FieldValue),Visible=false },
                       new GridColView() { Field = "FieldValueCheckBox", Visible = false },
                       new GridColView() { Field = "FieldValueComboBox", Visible = false }              ]
                };
                xFoundItemsGrid.AddCustomView(ReplaceAttributeTextBoxView);



                GridViewDef ReplaceAttributeCheckBoxView = new GridViewDef(eGridView.ReplaceAttributeCheckBoxView.ToString())
                {
                    GridColsView = [
                       new GridColView() { Field = "View Details", Visible = false },
                       new GridColView() { Field = nameof(FoundItem.FieldValue),Visible=false },
                       new GridColView() { Field = "FieldValueTextBox", Visible = false },
                       new GridColView() { Field = "FieldValueComboBox", Visible = false }              ]
                };
                xFoundItemsGrid.AddCustomView(ReplaceAttributeCheckBoxView);


                GridViewDef ReplaceAttributeComboBoxView = new GridViewDef(eGridView.ReplaceAttributeComboBoxView.ToString())
                {
                    GridColsView = [
                       new GridColView() { Field = "View Details", Visible = false },
                       new GridColView() { Field = nameof(FoundItem.FieldValue),Visible=false },
                       new GridColView() { Field = "FieldValueCheckBox", Visible = false },
                       new GridColView() { Field = "FieldValueTextBox", Visible = false }             ]
                };
                xFoundItemsGrid.AddCustomView(ReplaceAttributeComboBoxView);
                xFoundItemsGrid.InitViewItems();


                xFoundItemsGrid.AddToolbarTool(
                    eImageType.Share,
                    "Set highlighted value for all",
                    BulkUpdateValueForAll);

                xFoundItemsGrid.MarkUnMarkAllActive += MarkUnMarkAllActionsForFindandReplace;
                xFoundItemsGrid.ShowViewCombo = Visibility.Collapsed;
                xFoundItemsGrid.SetTitleLightStyle = true;
                xFoundItemsGrid.DataSourceList = mFoundItemsList;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in SetFoundItemsGridView", ex);
            }
        }

        private void FieldValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BulkUpdateValueForAll(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)xFindReplaceBtn.IsChecked)
                {
                    return;
                }
                IEnumerable<FoundItem> visibleItems = xFoundItemsGrid
                    .GetFilteredItems()
                    .Cast<FoundItem>();

                FoundItem highlightedItem = (FoundItem)xFoundItemsGrid.CurrentItem;

                foreach (FoundItem item in visibleItems)
                {
                    if (item.IsSelected)
                    {
                        item.FieldValue = highlightedItem.FieldValue;
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error: No Helighted row found.", ex);
            }
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

        private void MarkUnMarkAllActionsForFindandReplace(bool Status)
        {
            if (xFoundItemsGrid.DataSourceList?.Count <= 0)
            {
                return;
            }

            if (xFoundItemsGrid.DataSourceList.Count > 0)
            {
                foreach (object item in xFoundItemsGrid.GetSourceItemsAsIList())
                {
                    ((FoundItem)item).IsSelected = Status;
                }
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

            mFindAndReplaceUtils.PropertyChanged += MFindAndReplaceUtils_PropertyChanged;

            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), Type = typeof(BusinessFlow), GetItemsToSearchIn = GetBusinessFlowsToSearchIn });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Activity), Type = typeof(Activity), GetItemsToSearchIn = GetActivitiesToSearchIn });
            mMainItemsTypeList.Add(new FindItemType { Name = "Action", Type = typeof(Act), HasSubType = true, GetItemsToSearchIn = GetActionsToSearchIn, GetSubItems = GetPlatformsActions });
            mMainItemsTypeList.Add(new FindItemType { Name = GingerDicser.GetTermResValue(eTermResKey.Variable), Type = typeof(VariableBase), HasSubType = true, GetItemsToSearchIn = GetVariablesToSearchIn, GetSubItems = GetVariables });
            if (mContext is eContext.RunsetPage or eContext.SolutionPage)
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

            if (mContext is eContext.SolutionPage)
            {
                mMainItemsTypeList.Add(new FindItemType { Name = eTermResKey.Environment.ToString(), Type = typeof(ProjEnvironment), GetItemsToSearchIn = GetEnvironmentToSearchIn });
                mMainItemsTypeList.Add(new FindItemType { Name = eTermResKey.Agents.ToString(), Type = typeof(Agent), GetItemsToSearchIn = GetAgentToSearchIn });
            }
            xMainItemListCB.SelectedValuePath = nameof(FindItemType.Type);
            xMainItemListCB.DisplayMemberPath = nameof(FindItemType.Name);
            xMainItemListCB.ItemsSource = mMainItemsTypeList;


        }

        private void xFindWhatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Find();
            }
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

        ObservableList<Button> winButtons = [];

        Button searchBtn = new Button
        {
            Content = "Replace Selected"
        };
        Button closeBtn = new Button
        {
            Content = "Close"
        };
        private void PageButton()
        {
            searchBtn.Click += xUpdateAttributeValueBtn_Click;
            winButtons.Add(searchBtn);
            closeBtn.Click += CloseBtn_Click;
            winButtons.Add(closeBtn);
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            xFindReplaceBtn.IsChecked = true;


            var title = mContext switch
            {
                eContext.AutomatePage => string.Format("Find & Replace in '{0}' {1}", ((BusinessFlow)mItemToSearchOn).Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)),
                eContext.RunsetPage => string.Format("Find in '{0}' {1}", WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet)),
                eContext.FolderItems => string.Format("Find & Replace in Folder "),
                _ => "Find & Replace",
            };
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, windowBtnsList: winButtons);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
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

                List<FoundItem> FIList = mFoundItemsList.Where(x => x.IsSelected == true && (x.Status == FoundItem.eStatus.Pending || x.Status == FoundItem.eStatus.Failed)).ToList();
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
                {
                    xSaveButton.Visibility = Visibility.Visible;
                }

                EnableDisableButtons(true);
                mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            }
        }

        private void ReplaceItems(List<FoundItem> FIList, string newValue)
        {
            try
            {
                foreach (FoundItem foundItem in FIList)
                {
                    if (mFindAndReplaceUtils.ReplaceItem(mSearchConfig, mFindWhat, foundItem, newValue))
                    {
                        foundItem.Status = FoundItem.eStatus.Updated;
                    }
                    else
                    {
                        foundItem.Status = FoundItem.eStatus.Failed;
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Fail to Replace Items", ex);
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
        private string? mAttributeName;

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
            try
            {
                mItemsToSearchIn.Clear();
                mMainItemType.GetItemsToSearchIn();

                foreach (ItemToSearchIn searchItem in mItemsToSearchIn)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                    {
                        return;
                    }

                    mFindAndReplaceUtils.FindItemsByReflection(searchItem.OriginItemObject, searchItem.Item, mFoundItemsList, mFindWhat, mSearchConfig, searchItem.ParentItemToSave, searchItem.ItemParent, searchItem.FoundField);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Fail to Find Items", ex);
            }
        }

        private void GetBusinessFlowsToSearchIn()
        {
            switch (mContext)
            {
                case eContext.SolutionPage:/*
                    RepositoryFolder rr = new Amdocs.Ginger.Repository.RepositoryFolder();
                    var tt = rr.GetFolderRepositoryItems<BusinessFlow>();*/
                    foreach (BusinessFlow BF in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>())
                    {
                        mItemsToSearchIn.Add(new ItemToSearchIn(BF, BF, BF, string.Empty, string.Empty));
                    }

                    break;
                case eContext.AutomatePage:
                    mItemsToSearchIn.Add(new ItemToSearchIn(((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn), string.Empty, string.Empty));
                    break;
                case eContext.RunsetPage:
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                    {
                        foreach (BusinessFlow bf in runner.Executor.BusinessFlows)
                        {
                            mItemsToSearchIn.Add(new ItemToSearchIn(bf, bf, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + bf.Name, string.Empty));
                        }
                    }
                    break;
                case eContext.FolderItems:
                    foreach (ITreeViewItem node in mTreeViewChildItems)
                    {
                        if (node != null && node.NodeObject() is RepositoryItemBase)
                        {
                            RepositoryItemBase BF = (RepositoryItemBase)node.NodeObject();
                            if (BF is BusinessFlow)
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(BF, BF, BF, string.Empty, string.Empty));
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void GetAgentToSearchIn()
        {
            foreach (var item in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>())
            {
                mItemsToSearchIn.Add(new ItemToSearchIn(item, item, item, string.Empty, string.Empty));
            }
        }
        private void GetEnvironmentToSearchIn()
        {
            foreach (var item in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
            {
                mItemsToSearchIn.Add(new ItemToSearchIn(item, item, item, string.Empty, string.Empty));
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
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                            {
                                return;
                            }

                            mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, bf, bf.Name, string.Empty));
                        }
                    }

                    //Pull Activities from shared repository
                    ObservableList<Activity> RepoActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    foreach (Activity activity in RepoActions)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, activity, string.Empty, string.Empty));
                    }
                    break;

                case eContext.AutomatePage:
                    //Pull Activities from current businessflows
                    foreach (Activity Activity in ((BusinessFlow)mItemToSearchOn).Activities)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        mItemsToSearchIn.Add(new ItemToSearchIn(Activity, Activity, ((BusinessFlow)mItemToSearchOn), ((BusinessFlow)mItemToSearchOn).Name, string.Empty));
                    }
                    break;

                case eContext.RunsetPage:
                    //Pull Activities from runsets businessflows
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                    {
                        foreach (BusinessFlow BF in runner.Executor.BusinessFlows)
                        {
                            foreach (Activity activity in BF.Activities)
                            {
                                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                {
                                    return;
                                }

                                mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + BF.Name, string.Empty));
                            }
                        }
                    }
                    break;
                case eContext.FolderItems:
                    foreach (ITreeViewItem node in mTreeViewChildItems)
                    {
                        if (node?.NodeObject() is RepositoryItemBase repositoryItem)
                        {
                            if (repositoryItem is BusinessFlow businessFlow)
                            {
                                if (businessFlow == null)
                                {
                                    continue;
                                }
                                foreach (Activity activity in businessFlow.Activities)
                                {
                                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                    {
                                        return;
                                    }

                                    mItemsToSearchIn.Add(new ItemToSearchIn(activity, activity, businessFlow, businessFlow.Name, string.Empty));
                                }
                            }
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
                                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                {
                                    return;
                                }

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
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                            {
                                return;
                            }

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
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        if (mSubItemType == null || action.GetType() == mSubItemType)
                        {
                            mItemsToSearchIn.Add(new ItemToSearchIn(action, action, action, string.Empty, string.Empty));
                        }
                    }
                    break;

                case eContext.AutomatePage:
                    //Pull Activities from current businessflow
                    foreach (Activity activity in ((BusinessFlow)mItemToSearchOn).Activities)
                    {
                        string itemParent = ((BusinessFlow)mItemToSearchOn).Name + @"\" + activity.ActivityName;
                        foreach (Act action in activity.Acts)
                        {
                            if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                            {
                                return;
                            }

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
                    {
                        foreach (BusinessFlow bf in runner.Executor.BusinessFlows)
                        {
                            foreach (Activity activity in bf.Activities)
                            {
                                string itemParent = bf.Name + @"\" + activity.ActivityName;
                                foreach (Act action in activity.Acts)
                                {
                                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                    {
                                        return;
                                    }

                                    if (mSubItemType == null || action.GetType() == mSubItemType)
                                    {
                                        mItemsToSearchIn.Add(new ItemToSearchIn(action, action, WorkSpace.Instance.RunsetExecutor.RunSetConfig, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\" + itemParent, string.Empty));
                                    }
                                }
                            }
                        }
                    }

                    break;
                case eContext.FolderItems:
                    foreach (ITreeViewItem node in mTreeViewChildItems)
                    {
                        if (node?.NodeObject() is RepositoryItemBase repositoryItem)
                        {
                            if (repositoryItem is BusinessFlow businessFlow)
                            {
                                if (businessFlow == null)
                                {
                                    continue;
                                }
                                foreach (Activity activity in businessFlow.Activities)
                                {
                                    string itemParent = businessFlow.Name + @"\" + activity.ActivityName;
                                    foreach (Act action in activity.Acts)
                                    {
                                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                        {
                                            return;
                                        }

                                        if (mSubItemType == null || action.GetType() == mSubItemType)
                                        {
                                            mItemsToSearchIn.Add(new ItemToSearchIn(action, action, businessFlow, itemParent, string.Empty));
                                        }
                                    }
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
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        string VariablePath = WorkSpace.Instance.Solution.Name + "\\Global Variables";
                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                        {
                            mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, WorkSpace.Instance.Solution, VariablePath, string.Empty));
                        }
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
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, activity, ActivityVariablePath, string.Empty));
                            }
                        }
                    }
                    ObservableList<VariableBase> RepoVariables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                    foreach (VariableBase VB in RepoVariables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                        {
                            mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, VB, string.Empty, string.Empty));
                        }
                    }
                    break;

                case eContext.AutomatePage:
                    AddVariableFromBusinessFlowList([((BusinessFlow)mItemToSearchOn)]);
                    break;

                case eContext.RunsetPage:
                    foreach (GingerRunner runner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                    {
                        AddVariableFromBusinessFlowList(runner.Executor.BusinessFlows, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "\\" + runner.Name + "\\", WorkSpace.Instance.RunsetExecutor.RunSetConfig);
                    }

                    break;
                case eContext.FolderItems:
                    foreach (ITreeViewItem node in mTreeViewChildItems)
                    {
                        if (node?.NodeObject() is RepositoryItemBase repositoryItem)
                        {
                            if (repositoryItem is BusinessFlow businessFlow)
                            {
                                if (businessFlow == null)
                                {
                                    continue;
                                }

                                foreach (VariableBase VB in businessFlow.Variables)
                                {
                                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                                    {
                                        return;
                                    }

                                    if (mSubItemType == null || VB.GetType() == mSubItemType)
                                    {
                                        mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, businessFlow, businessFlow.Name, string.Empty));
                                    }
                                }

                                foreach (Activity activity in businessFlow.Activities)
                                {
                                    foreach (VariableBase VB in activity.Variables)
                                    {
                                        string ActivityVariablePath = $"{businessFlow.Name}\\{activity.ItemName}";
                                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                                        {
                                            mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, activity, ActivityVariablePath, string.Empty));
                                        }
                                    }
                                }
                            }
                        }
                    }
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
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                    {
                        return;
                    }

                    if (mSubItemType == null || VB.GetType() == mSubItemType)
                    {
                        mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, BF, BFVariableParent, string.Empty));
                    }
                }
                foreach (Activity activitiy in BF.Activities)
                {
                    string ActivityVariableParent = itemPathPrefix + BF.Name + @"\" + activitiy.ActivityName;
                    foreach (VariableBase VB in activitiy.Variables)
                    {
                        if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                        {
                            return;
                        }

                        if (mSubItemType == null || VB.GetType() == mSubItemType)
                        {
                            if (parent == null)
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, BF, ActivityVariableParent, string.Empty));
                            }
                            else
                            {
                                mItemsToSearchIn.Add(new ItemToSearchIn(VB, VB, parent, ActivityVariableParent, string.Empty));
                            }
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
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                {
                    return;
                }

                mItemsToSearchIn.Add(new ItemToSearchIn(RSC, RSC, RSC, string.Empty, string.Empty));
            }
        }

        private void GetApplicationModelsToSearchIn()
        {
            ObservableList<ApplicationAPIModel> ApplicationModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();
            foreach (ApplicationAPIModel AAM in ApplicationModels)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                {
                    return;
                }

                if (mSubItemType == null || AAM.GetType() == mSubItemType)
                {
                    mItemsToSearchIn.Add(new ItemToSearchIn(AAM, AAM, AAM, "Application API Models\\", string.Empty));
                }
            }
            ObservableList<ApplicationPOMModel> ApplicationPomModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
            foreach (ApplicationPOMModel AAM in ApplicationPomModels)
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                {
                    return;
                }

                if (mSubItemType == null || AAM.GetType() == mSubItemType)
                {
                    mItemsToSearchIn.Add(new ItemToSearchIn(AAM, AAM, AAM, "Application Page Object Models\\", string.Empty));
                }
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
            ObservableList<Act> Acts = [];
            List<FindItemType> ActsSubItemList = [];

            Assembly[] assemblies = { AppDomain.CurrentDomain.Load("GingerCore"), AppDomain.CurrentDomain.Load("GingerCoreCommon"), AppDomain.CurrentDomain.Load("GingerCoreNet") };

            //!!!!!!!!!!!! FIXME see add action page
            var ActTypes =
                from assembly in assemblies
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Act)) && !type.IsAbstract
                select type;

            foreach (Type t in ActTypes)
            {
                Act a = (Act)Activator.CreateInstance(t);
                Acts.Add(a);
            }

            foreach (Act a in Acts)
            {
                if (!string.IsNullOrEmpty(a.ActionDescription))
                {
                    ActsSubItemList.Add(new FindItemType { Name = a.ActionDescription, Type = a.GetType() });
                }
            }

            return ActsSubItemList;
        }

        private List<FindItemType> GetApplicationModels()
        {
            ObservableList<ApplicationModelBase> ApplicationModels = [];
            List<FindItemType> APMsSubItemList = [];

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
                {
                    APMsSubItemList.Add(new FindItemType { Name = a.ObjFolderName, Type = a.GetType() });
                }
            }

            return APMsSubItemList;
        }

        private List<FindItemType> GetVariables()
        {
            ObservableList<VariableBase> Variables = [];
            List<FindItemType> VariablesSubItemList = [];

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
                {
                    VariablesSubItemList.Add(new FindItemType { Name = a.VariableUIType, Type = a.GetType() });
                }
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
            if (actionToView != null)
            {
                Act act = (Act)actionToView.OriginObject;
                RepositoryItemBase Parent = actionToView.ParentItemToSave;
                if (Parent is BusinessFlow)
                {
                    act.Context = new Context() { BusinessFlow = (BusinessFlow)Parent };
                }
                ActionEditPage w;
                if (mContext == eContext.RunsetPage)
                {
                    w = new ActionEditPage(act, General.eRIPageViewMode.View);
                }
                else if (mContext == eContext.AutomatePage)
                {
                    w = new ActionEditPage(act, General.eRIPageViewMode.Automation);
                }
                else if (Parent is BusinessFlow)
                {
                    w = new ActionEditPage(act, General.eRIPageViewMode.ChildWithSave, Parent as BusinessFlow);
                }
                else if (Parent is Activity)
                {
                    w = new ActionEditPage(act, General.eRIPageViewMode.ChildWithSave, actParentActivity: Parent as Activity);
                }
                else
                {
                    w = new ActionEditPage(act, General.eRIPageViewMode.SharedReposiotry);
                }

                if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
                {
                    RefreshFoundItemField(actionToView);
                }
            }
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
            {
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.View);
            }

            if (mContext == eContext.AutomatePage)
            {
                if (Parent is not null and BusinessFlow)
                {
                    w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.Default, Parent as BusinessFlow);
                }
                else if (Parent is not null and Activity)
                {
                    w = new VariableEditPage(variableToView, null, true, VariableEditPage.eEditMode.Default, Parent as Activity);
                }
                else
                {
                    w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.SharedRepository, Parent);
                }
            }
            else if (Parent is not null and (Solution or BusinessFlow or Activity))
            {
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.FindAndReplace, Parent);
            }
            else
            {
                w = new VariableEditPage(variableToView, context, true, VariableEditPage.eEditMode.SharedRepository);
            }

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
            {
                RefreshFoundItemField(variableToViewFoundItem);
            }
        }

        private void ViewActivity(FoundItem activityToViewFoundItem)
        {
            Activity activity = (Activity)activityToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = activityToViewFoundItem.ParentItemToSave;
            GingerWPF.BusinessFlowsLib.ActivityPage w;
            if (mContext == eContext.SolutionPage)
            {
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context() { BusinessFlow = (BusinessFlow)Parent }, General.eRIPageViewMode.ChildWithSave);
            }
            else if (mContext == eContext.AutomatePage)
            {
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context(), General.eRIPageViewMode.Automation);
            }
            else
            {
                w = new GingerWPF.BusinessFlowsLib.ActivityPage(activity, new Context(), General.eRIPageViewMode.View);
            }

            if (w.ShowAsWindow(eWindowShowStyle.Dialog) == true)
            {
                RefreshFoundItemField(activityToViewFoundItem);
            }
        }

        private void ViewBusinessFlow(FoundItem businessFlowToViewFoundItem)
        {
            BusinessFlow businessFlow = (BusinessFlow)businessFlowToViewFoundItem.OriginObject;
            RepositoryItemBase Parent = businessFlowToViewFoundItem.ParentItemToSave;
            GingerWPF.BusinessFlowsLib.BusinessFlowViewPage w = null;
            if (mContext == eContext.RunsetPage)
            {
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.View);
            }
            else if (mContext == eContext.AutomatePage)
            {
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.Automation);
            }
            else
            {
                w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.Standalone);
            }

            w.Width = 1000;
            w.Height = 800;
            if (w.ShowAsWindow() == true)
            {
                RefreshFoundItemField(businessFlowToViewFoundItem);
            }
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
                {
                    RefreshFoundItemField(applicationModelToViewFoundItem);
                }
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
                List<FoundItem> FIList = mFoundItemsList.Where(x => x.IsSelected == true && (x.Status == FoundItem.eStatus.Updated || x.Status == FoundItem.eStatus.Failed)).ToList();

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
            try
            {
                foreach (FoundItem foundItem in FIList)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                    {
                        return;
                    }

                    try
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(foundItem.ParentItemToSave);
                        foundItem.Status = FoundItem.eStatus.Saved;
                    }
                    catch
                    {
                        foundItem.Status = FoundItem.eStatus.Failed;
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Fail to Save", ex);
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
            {
                xReplaceLabel.Visibility = Visibility.Collapsed;
            }

            if (xReplaceButton != null)
            {
                xReplaceButton.Visibility = Visibility.Collapsed;
            }

            if (xRow3 != null)
            {
                xRow3.Height = new GridLength(0);
            }

            if (xFoundItemsGrid != null)
            {
                xFoundItemsGrid.ChangeGridView(eGridView.FindView.ToString());
                xFoundItemsGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            }

            EnableDisableReplaceControlsAndFillReplaceComboBox();
        }

        private void ReplaceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EnableDisableReplaceControlsAndFillReplaceComboBox();
            xReplaceLabel.Visibility = Visibility.Visible;
            xReplaceButton.Visibility = Visibility.Visible;
            xFoundItemsGrid.btnMarkAll.Visibility = Visibility.Visible;
            xRow3.Height = new GridLength(30);
            xFoundItemsGrid.ChangeGridView(eGridView.ReplaceView.ToString());

        }

        private void SubItemTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearUI();
            mSubItemType = (Type)xSubItemTypeComboBox.SelectedValue;
        }

        private void ReplaceValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xReplaceValueComboBox != null && xReplaceValueComboBox.SelectedItem != null)
            {
                mValueToReplace = xReplaceValueComboBox.SelectedItem.ToString();
            }
        }

        private void ValueToReplaceTextBoxTextChanges(object sender, TextChangedEventArgs e)
        {
            mValueToReplace = xReplaceValueTextBox.Text;
        }
        private void xFindReplaceBtn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                mFoundItemsList.Clear();
                xItemSelectionPanel.Visibility = Visibility.Collapsed;
                xFindAttributePanel.Visibility = Visibility.Collapsed;
                xAttributeValueUpdatePnl.Visibility = Visibility.Collapsed;
                xButtonPanel.Visibility = Visibility.Visible;
                xMatchCasePanel.Visibility = Visibility.Visible;
                xReplaceValuePanel.Visibility = Visibility.Visible;
                xFindValuePanel.Visibility = Visibility.Visible;
                xFindAndReplanceBtnPanel.Visibility = Visibility.Visible;
                xItemMainComboPanel.Visibility = Visibility.Visible;
                xFoundItemsGrid.ChangeGridView(eGridView.FindView.ToString());
                xFoundItemsGrid.MouseDoubleClick += LineDoubleClicked;
                xFoundItemsGrid.btnMarkAll.Visibility = Visibility.Collapsed;
                xFoundItemsGrid.RowChangedEvent += RowChangedHandler;

                searchBtn.Visibility = Visibility.Collapsed;

                xRow3.Height = new GridLength(0);
                xRow4.Height = new GridLength(50);
                xRow5.Height = new GridLength(40);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occoured while find and replace", ex);
            }
        }

        private void xAttributeValueBtn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                mFoundItemsList.Clear();
                xButtonPanel.Visibility = Visibility.Collapsed;
                xMatchCasePanel.Visibility = Visibility.Collapsed;
                xReplaceValuePanel.Visibility = Visibility.Collapsed;
                xFindValuePanel.Visibility = Visibility.Collapsed;
                xFindAndReplanceBtnPanel.Visibility = Visibility.Collapsed;
                xItemMainComboPanel.Visibility = Visibility.Collapsed;
                xAttributeValueUpdatePnl.Visibility = Visibility.Visible;
                xItemSelectionPanel.Visibility = Visibility.Visible;
                xFindAttributePanel.Visibility = Visibility.Visible;
                xFoundItemsGrid.ChangeGridView(eGridView.ReplaceAttributeValueView.ToString());
                xFoundItemsGrid.MouseDoubleClick -= LineDoubleClicked;
                xFoundItemsGrid.btnMarkAll.Visibility = Visibility.Visible;
                xFoundItemsGrid.RowChangedEvent -= RowChangedHandler;

                searchBtn.Visibility = Visibility.Visible;

                xRow3.Height = new GridLength(0);
                xRow4.Height = new GridLength(0);
                xRow5.Height = new GridLength(0);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to switch to Change Attribute Value mode", ex);
            }
        }

        private void xMainItemListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearUI();
                mMainItemType = (FindItemType)xMainItemListCB.SelectedItem;
                mItemsToSearchIn.Clear();
                mMainItemType.GetItemsToSearchIn();
                xAttributeNameComboBox.Items.Clear();
                xAttributeNameComboBox.Text = "Select the Attribute...";
                xFoundItemsGrid.Visibility = Visibility.Collapsed;
                xAttributeValueUpdatePnl.Visibility = Visibility.Collapsed;
                var searchItem = mItemsToSearchIn.FirstOrDefault();
                if (searchItem != null)
                {
                    var attributeNameList = mFindAndReplaceUtils.GetSerializableEditableMemberNames(searchItem.OriginItemObject);
                    GingerCore.General.FillComboFromList(xAttributeNameComboBox, attributeNameList);
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error:", ex);
            }
        }

        void AttributeValueControlVisibility()
        {
            try
            {
                searchBtn.Visibility = Visibility.Visible;

                var searchItem = mItemsToSearchIn.FirstOrDefault();
                if (searchItem == null)
                {
                    return;
                }
                mFindAndReplaceUtils.FindTopLevelAttributeNames(searchItem.Item, mFoundItemsList, mAttributeName, mSearchConfig, searchItem.ParentItemToSave, searchItem.ItemParent);

                var item = mFoundItemsList.FirstOrDefault();
                mFoundItemsList.Clear();
                if (item == null || item.FieldType == null)
                {
                    return;
                }
                if (item.FieldType.Name == "Boolean")
                {
                    xFoundItemsGrid.ChangeGridView(eGridView.ReplaceAttributeCheckBoxView.ToString());
                }
                else if (item.FieldType.Name == "String")
                {
                    xFoundItemsGrid.ChangeGridView(eGridView.ReplaceAttributeTextBoxView.ToString());
                }
                else if (item.FieldType.BaseType?.Name == "Enum")
                {
                    string enumTypeName = item.FieldType.FullName;
                    Type enumType = item.FieldType.Assembly.GetType(enumTypeName);
                    if (enumType != null && enumType.IsEnum)
                    {
                        List<ComboEnumItem> comboItems = GingerCore.General.GetEnumValuesForCombo(enumType);
                        FoundItem.FieldValueOption = comboItems.Select(item => item.text).ToList();

                        xFoundItemsGrid.ChangeGridView(eGridView.ReplaceAttributeComboBoxView.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error:", ex);
            }
        }

        private async void FindItemsAsync_item()
        {
            mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Running;

            try
            {
                mFoundItemsList.Clear();
                xFoundItemsGrid.Visibility = Visibility.Visible;
                xProcessingImage2.Visibility = Visibility.Visible;
                searchBtn.IsEnabled = false;

                await Task.Run(() => FindItems_item());
                MarkUnMarkAllActionsForFindandReplace(true);
                searchBtn.IsEnabled = true;
                xProcessingImage2.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error:", ex);
            }
            finally
            {
                mFindAndReplaceUtils.ProcessingState = FindAndReplaceUtils.eProcessingState.Pending;
            }
        }

        private void AddRepositoryItems<T>(Func<IEnumerable<T>> getItems) where T : RepositoryItemBase
        {
            foreach (var item in getItems())
            {
                if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                    return;

                mItemsToSearchIn.Add(new ItemToSearchIn(item, item, item, string.Empty, string.Empty));
            }
        }

        private void FindItems_item()
        {
            try
            {
                foreach (ItemToSearchIn searchItem in mItemsToSearchIn)
                {
                    if (mFindAndReplaceUtils.ProcessingState == FindAndReplaceUtils.eProcessingState.Stopping)
                    {
                        return;
                    }

                    mFindAndReplaceUtils.FindTopLevelAttributeNames(
                        searchItem.Item,
                        mFoundItemsList,
                        mAttributeName,
                        mSearchConfig,
                        searchItem.ParentItemToSave,
                        searchItem.ItemParent
                    );
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Find Items", ex);
            }
        }


        private void xUpdateAttributeValueBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IEnumerable<FoundItem> ModifiedAndSelectedItemList = mFoundItemsList.Where(item => item.Status == FoundItem.eStatus.Modified && item.IsSelected);
                foreach (FoundItem foundItem in ModifiedAndSelectedItemList)
                {
                    bool success = mFindAndReplaceUtils.ReplaceItemEnhanced(foundItem);

                    if (success)
                    {
                        foundItem.Status = FoundItem.eStatus.Updated;
                        foundItem.IsModified = FoundItem.eStatus.Pending;
                    }
                    else
                    {
                        foundItem.Status = FoundItem.eStatus.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update selected attribute values", ex);
            }
        }






        private void xAttributeNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                mAttributeName = xAttributeNameComboBox.SelectedValue?.ToString();
                xAttributeValueUpdatePnl.Visibility = Visibility.Visible;
                if (string.IsNullOrEmpty(mAttributeName))
                {
                    return;
                }
                ClearUI();
                AttributeValueControlVisibility();
                FindItemsAsync_item();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error:", ex);
            }
        }

    }
}
