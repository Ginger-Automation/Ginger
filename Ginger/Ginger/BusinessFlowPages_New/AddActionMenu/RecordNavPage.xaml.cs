using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages_New;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
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
    public partial class RecordNavPage : Page
    {
        public bool IsRecording = false;
        IWindowExplorer mWindowExplorerDriver;
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

            SetControlsVisibility();
            AgentBasedManipulations();
            SetRecordingButtonText();

            SetMultiplePropertiesGridView();
        }

        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && e.PropertyName == nameof(Context.Activity) ||
                              e.PropertyName == nameof(Context.Target) ||
                              e.PropertyName == nameof(Context.Platform))
            {
                if (IsRecording)
                {
                    IsRecording = !IsRecording;
                    StopRecording();
                }
                SetControlsVisibility();
                SetRecordingButtonText();
            }

            if(e.PropertyName == nameof(Context.Agent) || e.PropertyName == nameof(Context.AgentStatus))
            {
                AgentBasedManipulations();
            }
        }

        private void SetMultiplePropertiesGridView()
        {
            gridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 250, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            gridPOMListItems.btnAdd.Click -= BtnAdd_Click;
            gridPOMListItems.btnAdd.Click += BtnAdd_Click;
            gridPOMListItems.SetAllColumnsDefaultView(view);
            gridPOMListItems.InitViewItems();
            PomModels = new ObservableList<POMBindingObjectHelper>();
            gridPOMListItems.DataSourceList = PomModels;
        }
        
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {            
            if (mApplicationPOMSelectionPage == null)
            {
                ApplicationPOMsTreeItem appModelFolder;
                RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
                appModelFolder = new ApplicationPOMsTreeItem(repositoryFolder);
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                        SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
                mApplicationPOMSelectionPage.SelectionDone += MAppModelSelectionPage_SelectionDone; 
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            AddSelectedPOM(selectedPOMs);
        }

        private void SetControlsVisibility()
        {
            this.Dispatcher.Invoke(() =>
            {
                gridPOMListItems.Visibility = Visibility.Hidden;
                xWinGridUC.IsEnabled = false;
                xRecordingButton.IsEnabled = false;
                xStartAgentMessage.Visibility = Visibility.Visible;
                xPOMPanel.Visibility = Visibility.Hidden;
            });
        }

        private void AgentBasedManipulations()
        {
            if (mContext.Agent != null && (mContext.Agent.IsSupportRecording() || mContext.Agent.Driver is IRecord))
            {
                bool isAgentRunning = mContext.Agent.Status == GingerCore.Agent.eStatus.Running;         //AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
                if (isAgentRunning)
                {
                    xStartAgentMessage.Visibility = Visibility.Collapsed;
                    xWinGridUC.IsEnabled = true;
                }

                mWindowExplorerDriver = mContext.Agent.Driver as IWindowExplorer;

                if ((xWinGridUC.mWindowExplorerDriver == null && mWindowExplorerDriver != null) || xWinGridUC.mWindowExplorerDriver != mWindowExplorerDriver)
                {
                    if (mWindowExplorerDriver != null)
                    {
                        xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;

                        if (xWinGridUC.WindowsComboBox != null)
                        {
                            xWinGridUC.WindowsComboBox.SelectionChanged -= WindowsComboBox_SelectionChanged;
                            xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;
                        }
                    }
                    else
                    {
                        xWinGridUC.WindowsComboBox.ItemsSource = null;
                    }
                }
                else if (xWinGridUC.WindowsComboBox.ItemsSource == null)
                {
                    xWinGridUC.UpdateWindowsList();
                }

                if (isAgentRunning && (AppWindow)xWinGridUC.WindowsComboBox.SelectedItem != null
                    && !string.IsNullOrEmpty(((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem).Title))
                {
                    xPOMPanel.Visibility = Visibility.Visible;
                    xRecordingButton.IsEnabled = true;

                    if (((bool)xIntegratePOM.IsChecked))
                    {
                        gridPOMListItems.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            bool isAgentRunning = mContext.Agent.Status == GingerCore.Agent.eStatus.Running;           // AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);

            if (mContext.Agent != null)
                mWindowExplorerDriver = mContext.Agent.Driver as IWindowExplorer;

            if (isAgentRunning)
            {
                IsRecording = !IsRecording;
                if (xWinGridUC.comboBoxSelectedValue != null)
                {
                    if (IsRecording)
                    {
                        StartRecording();
                    }
                    else
                    {
                        StopRecording();
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
                }
                SetRecordingButtonText();
            }
            else
            {
                SetControlsVisibility();
                SetRecordingButtonText();
            }
        }
        
        private void StartRecording()
        {
            IRecord record = (IRecord)mWindowExplorerDriver;            
            IPlatformInfo platformInfo = PlatformInfoBase.GetPlatformImpl(mContext.Platform);

            List<ApplicationPOMModel> applicationPOMs = null;
            if (Convert.ToBoolean(xIntegratePOM.IsChecked))
            {
                applicationPOMs = new List<ApplicationPOMModel>();
                foreach (var pom in PomModels)
                {
                    if (pom.IsChecked)
                    {
                        applicationPOMs.Add(pom.ItemObject);
                    }
                } 
            }

            mRecordingMngr = new RecordingManager(applicationPOMs, mContext.BusinessFlow, mContext, record, platformInfo);
            mRecordingMngr.StartRecording();
        }

        public void StopRecording()
        {
            if (mRecordingMngr != null)
            {
                mRecordingMngr.StopRecording();
            }
        }

        private void SetRecordingButtonText()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (IsRecording)
                {
                    xRecordingButton.ButtonText = "Stop Recording";
                    xRecordingButton.ToolTip = "Stop Recording";
                    xRecordingButton.ButtonImageType = eImageType.Stop;
                }
                else
                {
                    xRecordingButton.ButtonText = "Start Recording";
                    xRecordingButton.ToolTip = "Start Recording";
                    xRecordingButton.ButtonImageType = eImageType.Run;
                }
                xRecordingButton.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle");
            });
        }

        ObservableList<POMBindingObjectHelper> PomModels = new ObservableList<POMBindingObjectHelper>();
        
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
                    if (!IsPOMAlreadyAdded(pom.ItemName))
                    {
                        ApplicationPOMModel pomToAdd = (ApplicationPOMModel)pom.CreateCopy(false);
                        PomModels.Add(new POMBindingObjectHelper() { IsChecked = true, ItemName = pomToAdd.ItemName, ContainingFolder = pom.ContainingFolder, ItemObject = pom }); 
                    }
                    else
                    {
                        MessageBox.Show(@"""" + pom.ItemName + @""" POM is already added!", "Alert Message");
                    }
                }
                gridPOMListItems.DataSourceList = PomModels;
            }
        }

        private bool IsPOMAlreadyAdded(string itemName)
        {
            bool isPresent = false;
            if(PomModels != null && PomModels.Count > 0)
            {
                foreach (var item in PomModels)
                {
                    if(item.ItemName == itemName)
                    {
                        isPresent = true;
                        break;
                    }
                }
            }
            return isPresent;
        }

        private void XIntegratePOM_Checked(object sender, RoutedEventArgs e)
        {
            gridPOMListItems.Visibility = (bool)xIntegratePOM.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void XIntegratePOM_Unchecked(object sender, RoutedEventArgs e)
        {
            gridPOMListItems.Visibility = (bool)xIntegratePOM.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetControlsVisibility();
            AgentBasedManipulations();
            SetRecordingButtonText();
        }
    }    
}
