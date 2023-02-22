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
#endregion License
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Collections;
using GingerCore.GeneralLib;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for RepositoryItemErroHandlerPublishInfoPage.xaml
    /// </summary>
    public partial class RepositoryItemPublishInfoPage : Page
    {
        private GenericWindow _pageGenericWin = null;
        private RepositoryItemBase mRepoItem;
        public ObservableList<RepositoryItemUsage> mRepoItemUsages = new ObservableList<RepositoryItemUsage>();
        private readonly object mAddUsageLock = new object();

        public RepositoryItemPublishInfoPage(RepositoryItemBase repoItem)
        {
            InitializeComponent();
            mRepoItem = repoItem;
            xRepoItemPublisIngoGrid.DataSourceList = mRepoItemUsages;
            SetErrorHandlerUsagesGridView();
        }

        private void SetErrorHandlerUsagesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.Selected, Header = "Selected", StyleType = GridColView.eGridColStyleType.CheckBox, WidthWeight = 5 });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.UsageItemName, Header = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name", WidthWeight = 15, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.HostBizFlowPath, Header = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Path", WidthWeight = 20, ReadOnly = true });

            view.GridColsView.Add(new GridColView()
            {
                Field = RepositoryItemUsage.Fields.RepositoryItemPublishType,
                Header = "Publish Type",
                WidthWeight = 10,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(RepositoryItemUsage.eRepositoryItemPublishType)), nameof(RepositoryItemUsage.RepositoryItemPublishType),false,true)
            });


            view.GridColsView.Add(new GridColView()
            {
                Field = RepositoryItemUsage.Fields.InsertRepositoryInsatncePosition,
                Header = "Insert At",
                WidthWeight = 10,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = ucGrid.GetGridComboBoxTemplate(GingerCore.General.GetEnumValuesForCombo(typeof(RepositoryItemUsage.eInsertRepositoryInsatncePosition)), nameof(RepositoryItemUsage.InsertRepositoryInsatncePosition), comboSelectionChangedHandler: InsertPositionCobmo_SelectionChanged)
            });

            view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemUsage.IndexActivityName), Header = "Index Activity", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(RepositoryItemUsage.ActivityNameList), nameof(RepositoryItemUsage.IndexActivityName), true) });
            view.GridColsView.Add(new GridColView() { Field = RepositoryItemUsage.Fields.PublishStatus, Header = "Status", WidthWeight = 20, ReadOnly = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = GingerCore.General.GetEnumValuesForCombo(typeof(RepositoryItemUsage.ePublishStatus)) });

            xRepoItemPublisIngoGrid.SetAllColumnsDefaultView(view);
            xRepoItemPublisIngoGrid.InitViewItems();

            xRepoItemPublisIngoGrid.AddToolbarTool("@Checkbox_16x16.png", "Select / Un-Select All", new RoutedEventHandler(SelectUnSelectAll));
            xRepoItemPublisIngoGrid.AddToolbarTool("@DropDownList_16x16.png", "Set Same Selected Part to All", new RoutedEventHandler(SetSamePartToAll));
        }

        private void InsertPositionCobmo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = (RepositoryItemUsage)xRepoItemPublisIngoGrid.CurrentItem;

            if (currentItem != null && currentItem.InsertRepositoryInsatncePosition == RepositoryItemUsage.eInsertRepositoryInsatncePosition.AfterSpecificActivity)
            {
                SetActivityList(currentItem);
            }
        }

        private static void SetActivityList(RepositoryItemUsage currentItem)
        {
            var activityIndex = 0;
            foreach (var item in currentItem.HostBusinessFlow.Activities)
            {
                currentItem.ActivityNameList.Add(activityIndex + "-" + item.ActivityName);
                activityIndex++;
            }
        }

        private void SetSamePartToAll(object sender, RoutedEventArgs e)
        {
            if (xRepoItemPublisIngoGrid.CurrentItem != null)
            {
                try
                {
                    StartProcessingIcon();
                    RepositoryItemUsage repositoryItemUsage = (RepositoryItemUsage)xRepoItemPublisIngoGrid.CurrentItem;
                    foreach (RepositoryItemUsage usage in mRepoItemUsages)
                    {
                        usage.InsertRepositoryInsatncePosition = repositoryItemUsage.InsertRepositoryInsatncePosition;

                        if (usage.InsertRepositoryInsatncePosition == RepositoryItemUsage.eInsertRepositoryInsatncePosition.AfterSpecificActivity)
                        {
                            usage.ActivityNameList.ClearAll();
                            SetActivityList(usage);
                        }
                    }
                }
                finally
                {
                    StopProcessingIcon();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void SelectUnSelectAll(object sender, RoutedEventArgs e)
        {
            bool selectValue = false;
            if (mRepoItemUsages.Count > 0)
            {
                selectValue = !mRepoItemUsages[0].Selected;//decide if to take or not
                foreach (RepositoryItemUsage usage in mRepoItemUsages)
                {
                    usage.Selected = selectValue;
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button publishErrorHandlerButton = new Button();
            publishErrorHandlerButton.Content = "Publish All Selected";
            publishErrorHandlerButton.Click += new RoutedEventHandler(PublishErrorHandlerButton_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(publishErrorHandlerButton);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Repository Item Publish Page", this, winButtons, true, "Close");
        }

        private async void PublishErrorHandlerButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                StartProcessingIcon();
                var errorOccured = false;
                foreach (var repositoryItem in mRepoItemUsages)
                {
                    try
                    {
                        if (repositoryItem.Selected && repositoryItem.PublishStatus != RepositoryItemUsage.ePublishStatus.Published)
                        {
                            Activity activityCopy = (Activity)mRepoItem.CreateInstance(true);
                            activityCopy.Active = true;

                            if (repositoryItem.InsertRepositoryInsatncePosition == RepositoryItemUsage.eInsertRepositoryInsatncePosition.AtEnd)
                            {
                                repositoryItem.HostBusinessFlow.AddActivity(activityCopy, repositoryItem.HostBusinessFlow.ActivitiesGroups.Last());
                            }
                            else if (repositoryItem.InsertRepositoryInsatncePosition == RepositoryItemUsage.eInsertRepositoryInsatncePosition.Beginning)
                            {
                                repositoryItem.HostBusinessFlow.AddActivity(activityCopy, repositoryItem.HostBusinessFlow.ActivitiesGroups.FirstOrDefault(), insertIndex: 0);
                            }
                            else if (repositoryItem.InsertRepositoryInsatncePosition == RepositoryItemUsage.eInsertRepositoryInsatncePosition.AfterSpecificActivity)
                            {
                                if (repositoryItem.IndexActivityName == null)
                                {
                                    errorOccured = true;
                                }
                                else
                                {
                                    var indexToAdd = Convert.ToInt32(repositoryItem.IndexActivityName[0].ToString());
                                    repositoryItem.HostBusinessFlow.AddActivity(activityCopy, repositoryItem.HostBusinessFlow.ActivitiesGroups.FirstOrDefault(), insertIndex: indexToAdd + 1);
                                }
                            }
                            if (!errorOccured)
                            {
                                if (repositoryItem.RepositoryItemPublishType == RepositoryItemUsage.eRepositoryItemPublishType.LinkInstance)
                                {
                                    repositoryItem.HostBusinessFlow.MarkActivityAsLink(activityCopy.Guid, activityCopy.ParentGuid);
                                }
                                repositoryItem.PublishStatus = RepositoryItemUsage.ePublishStatus.Published;
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(repositoryItem.HostBusinessFlow);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        errorOccured = true;
                        repositoryItem.PublishStatus = RepositoryItemUsage.ePublishStatus.FailedToPublish;
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {System.Reflection.MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                }
                StopProcessingIcon();
                if (errorOccured)
                {
                    Reporter.ToUser(eUserMsgKey.FailedToPublishRepositoryInfo);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.PublishRepositoryInfo);
                }
            }
             );
        }

        private async Task GetBusinessFlowPublishedInfo()
        {
            try
            {
                await Task.Run(() =>
                {
                    StartProcessingIcon();
                    ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                    Parallel.ForEach(businessFlows, BF =>
                    {
                        RepositoryItemUsage itemUsage;
                        string businessFlowName = Amdocs.Ginger.Common.GeneralLib.General.RemoveInvalidFileNameChars(BF.Name);
                        RepositoryItemUsage.eUsageTypes usageType = RepositoryItemUsage.eUsageTypes.None;
                        bool isPublishedInBF = false;

                        if (mRepoItem is Activity)
                        {
                            foreach (Activity errorHandler in BF.Activities)
                            {
                                if (errorHandler.ParentGuid == mRepoItem.Guid || errorHandler.Guid == mRepoItem.Guid ||
                                            (mRepoItem.ExternalID != null && mRepoItem.ExternalID != string.Empty && mRepoItem.ExternalID != "0" && errorHandler.ExternalID == mRepoItem.ExternalID))
                                {
                                    isPublishedInBF = true;
                                }
                            }
                            if (!isPublishedInBF)
                            {
                                itemUsage = new() { HostBusinessFlow = BF, HostBizFlowPath = System.IO.Path.Combine(BF.ContainingFolder, businessFlowName), UsageItemName = businessFlowName, UsageItemType = usageType, Selected = false, RepositoryItemPublishType = RepositoryItemUsage.eRepositoryItemPublishType.LinkInstance, InsertRepositoryInsatncePosition = RepositoryItemUsage.eInsertRepositoryInsatncePosition.AtEnd };
                                AddBFUsageInList(itemUsage);
                            }
                        }


                    });
                    StopProcessingIcon();
                });
            }
            catch (Exception ex)
            {
                StopProcessingIcon();
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during GetBusinessFlowPublishedInfo", ex);
            }
        }

        private void AddBFUsageInList(RepositoryItemUsage itemUsage)
        {
            lock (mAddUsageLock)
            {
                mRepoItemUsages.Add(itemUsage);
            }
        }

        private void StartProcessingIcon()
        {
            xProcessingIcon.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
                   new Action(
                       delegate ()
                       {
                           xProcessingIcon.Visibility = Visibility.Visible;
                       }
           ));
        }
        private void StopProcessingIcon()
        {
            xProcessingIcon.Dispatcher.BeginInvoke(
               System.Windows.Threading.DispatcherPriority.Normal,
                   new Action(
                       delegate ()
                       {
                           xProcessingIcon.Visibility = Visibility.Collapsed;
                       }
           ));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetBusinessFlowPublishedInfo().ConfigureAwait(false);
        }
    }
}
