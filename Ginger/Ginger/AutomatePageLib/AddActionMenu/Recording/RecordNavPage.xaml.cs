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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.AddActionMenu;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for RecordNavAction.xaml
    /// </summary>
    public partial class RecordNavPage : Page, INavPanelPage
    {
        public bool IsRecording = false;
        IWindowExplorer mDriver;
        Context mContext;
        RecordingManager mRecordingMngr;
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public RecordNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;
            context.PropertyChanged += Context_PropertyChanged;

            SetDriver();
            SetRecordingControls();
            SetSelectedPOMsGridView();
            xWindowSelectionUC.mContext = context;
        }

        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {

                if (e != null && e.PropertyName == nameof(Context.Agent) || e.PropertyName == nameof(Context.AgentStatus))
                {
                    if (IsRecording)
                    {
                        IsRecording = false;
                        StopRecording();
                    }
                    SetDriver();
                    SetRecordingControls();
                }
                else if (e != null && e.PropertyName == nameof(Context.Target))
                {
                    SetSelectedPOMsGridView();
                    mApplicationPOMSelectionPage = null;
                }
            }
        }

        private void SetSelectedPOMsGridView()
        {
            xSelectedPOMsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationPOMModel.NameWithRelativePath), Header = "POM", AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            xSelectedPOMsGrid.SetAllColumnsDefaultView(view);
            xSelectedPOMsGrid.InitViewItems();

            xSelectedPOMsGrid.btnAdd.Click -= BtnAdd_Click;
            xSelectedPOMsGrid.btnAdd.Click += BtnAdd_Click;

            xSelectedPOMsGrid.DataSourceList = mPomModels;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem appModelFolder;
            RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
            appModelFolder = new ApplicationPOMsTreeItem(repositoryFolder);
            mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                    SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, true,
                                                                                            new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." +
                                                                                            nameof(ApplicationPOMModel.TargetApplicationKey.ItemName),
                                                                                            mContext.Activity.TargetApplication));
            mApplicationPOMSelectionPage.SelectionDone += MAppModelSelectionPage_SelectionDone;


            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            AddSelectedPOM(selectedPOMs);
        }

        private void SetDriver()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mContext.Agent != null && (mContext.Agent.IsSupportRecording() || mContext.Agent.Driver is IRecord))
                {
                    mDriver = mContext.Agent.Driver as IWindowExplorer;

                    xWindowSelectionUC.mWindowExplorerDriver = mDriver;
                    xWindowSelectionUC.mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
                    if (mDriver == null)
                    {
                        xWindowSelectionUC.WindowsComboBox.ItemsSource = null;
                    }

                    if (mDriver != null && xWindowSelectionUC.WindowsComboBox.ItemsSource == null)
                    {
                        xWindowSelectionUC.UpdateWindowsList();
                    }

                    if (PlatformInfoBase.GetPlatformImpl(mContext.Platform) != null
                        && PlatformInfoBase.GetPlatformImpl(mContext.Platform).IsPlatformSupportPOM())
                    {
                        xPOMPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        xPOMPanel.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (xWindowSelectionUC.SelectedWindow != null)
            {
                IsRecording = true;
                StartRecording();
                SetRecordingControls();
                return;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
                IsRecording = false;
                SetRecordingControls();
            }
        }

        private void xStopRecordingBtn_Click(object sender, RoutedEventArgs e)
        {
            IsRecording = false;
            StopRecording();
            SetRecordingControls();
        }

        private void StartRecording()
        {
            IRecord record = (IRecord)mDriver;
            IPlatformInfo platformInfo = PlatformInfoBase.GetPlatformImpl(mContext.Platform);

            if (xIntegratePOM.IsChecked == true)
            {
                mRecordingMngr = new RecordingManager(mPomModels, mContext.BusinessFlow, mContext, record, platformInfo);
            }
            else
            {
                mRecordingMngr = new RecordingManager(null, mContext.BusinessFlow, mContext, record, platformInfo);
            }

            mRecordingMngr.RecordingNotificationEvent += RecordingMngr_RecordingNotificationEvent;
            mRecordingMngr.StartRecording();
        }

        /// <summary>
        /// This event is used to notify from recordingmanager class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordingMngr_RecordingNotificationEvent(object sender, RecordingEventArgs e)
        {
            switch (e.EventType)
            {                
                case eRecordingEvent.StopRecording:
                    Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, mContext.Agent.Name, mContext.Environment.Name);
                    mContext.AgentStatus = Agent.eStatus.NotStarted.ToString();
                    break;
            }
        }

        public void StopRecording()
        {
            if (mRecordingMngr != null)
            {
                mRecordingMngr.StopRecording();
            }
        }

        private void SetRecordingControls()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (IsRecording)
                {
                    xRecordingButton.ButtonText = "Recording...";
                    xRecordingButton.ToolTip = "Recording Window Operations";
                    xRecordingButton.ButtonImageType = eImageType.Processing;
                    xRecordingButton.IsEnabled = false;
                    xPOMPanel.IsEnabled = false;

                    xStopRecordingBtn.Visibility = Visibility.Visible;

                    xPOMPanel.IsEnabled = false;
                }
                else
                {
                    xRecordingButton.ButtonText = "Record";
                    xRecordingButton.ToolTip = "Start Recording";
                    xRecordingButton.ButtonImageType = eImageType.Camera;
                    xRecordingButton.IsEnabled = true;

                    xStopRecordingBtn.Visibility = Visibility.Collapsed;

                    xPOMPanel.IsEnabled = true;
                }
                xRecordingButton.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Highlighted");
            });
        }

        ObservableList<ApplicationPOMModel> mPomModels = new ObservableList<ApplicationPOMModel>();

        private void MAppModelSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            AddSelectedPOM(e.SelectedItems);
        }

        private void AddSelectedPOM(List<object> selectedPOMs)
        {
            if (selectedPOMs != null && selectedPOMs.Count > 0)
            {
                foreach (ApplicationPOMModel pom in selectedPOMs)
                {
                    if (mPomModels.Contains(pom) == false)
                    {
                        mPomModels.Add(pom);
                    }
                }
            }
        }



        private void XIntegratePOM_Checked(object sender, RoutedEventArgs e)
        {
            xSelectedPOMsGrid.Visibility = (bool)xIntegratePOM.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void XIntegratePOM_Unchecked(object sender, RoutedEventArgs e)
        {
            xSelectedPOMsGrid.Visibility = (bool)xIntegratePOM.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ReLoadPageItems()
        {
            if (IsRecording)
            {
                IsRecording = false;
                StopRecording();
            }
            SetDriver();
            SetRecordingControls();

            SetSelectedPOMsGridView();
            mApplicationPOMSelectionPage = null;
        }
    }
}
