using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.BusinessFlowPages_New;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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
            InitMethods();
        }
              
        private void InitMethods()
        {
            SetControlsVisibility();
            SetRecordingButtonText();
            SetMultiplePropertiesGridView();            

            if (xWinGridUC.mContext == null)
            {
                xWinGridUC.mContext = mContext; 
            }

            if (xWinGridUC.mWindowExplorerDriver == null || (mWindowExplorerDriver != null && xWinGridUC.mWindowExplorerDriver != mWindowExplorerDriver))
            {
                xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
            }

            if (xWinGridUC.WindowsComboBox != null)
            {
                xWinGridUC.WindowsComboBox.SelectionChanged -= WindowsComboBox_SelectionChanged;
                xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged; 
            }            
        }
        
        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e != null && e.PropertyName == nameof(BusinessFlow))
            {
                if (e.PropertyName == nameof(BusinessFlow))
                {
                    mContext = (Context)sender;
                    xWinGridUC.WindowsComboBox.ItemsSource = new List<AppWindow>();
                    mApplicationPOMSelectionPage = null;                    
                }
                if (IsRecording)
                {
                    IsRecording = !IsRecording;
                    StopRecording();
                }
                InitMethods();
            }
            else if (e.PropertyName == nameof(Context.AgentStatus) || e.PropertyName == nameof(Context.Agent) || e.PropertyName == nameof(Context.TargetApplication))
            {
                InitMethods();
            }
        }

        private void SetMultiplePropertiesGridView()
        {
            gridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 250, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
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
            gridPOMListItems.Visibility = Visibility.Hidden;
            xIntegratePOM.Visibility = Visibility.Hidden;
            xWinGridUC.IsEnabled = false;
            xRecordingButton.IsEnabled = false;
            xStartAgentMessage.Visibility = Visibility.Visible;
            xPOMPanel.Visibility = Visibility.Hidden;

            if (mContext.Agent != null && (mContext.Agent.IsSupportRecording() || mContext.Agent.Driver is IRecord))
            {
                if (PlatformInfoBase.GetPlatformImpl(mContext.ActivityPlatform) != null && PlatformInfoBase.GetPlatformImpl(mContext.ActivityPlatform).IsPlatformSupportPOM())
                {
                    if (((bool)xIntegratePOM.IsChecked))
                    {
                        gridPOMListItems.Visibility = Visibility.Visible; 
                    }                    
                    xPOMPanel.Visibility = Visibility.Visible;
                }

                bool isAgentRunning = AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
                if(isAgentRunning)
                {
                    xStartAgentMessage.Visibility = Visibility.Collapsed;
                    xWinGridUC.IsEnabled = true;
                    if (xWinGridUC.mWindowExplorerDriver == null && mWindowExplorerDriver != null)
                    {
                        xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver; 
                    }
                }
                else
                {
                    if(IsRecording)
                    {
                        StopRecording();
                    }
                }

                if (isAgentRunning && (AppWindow)xWinGridUC.WindowsComboBox.SelectedItem != null
                    && !string.IsNullOrEmpty(((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem).Title))
                {
                    xIntegratePOM.Visibility = Visibility.Visible;
                    xRecordingButton.IsEnabled = true;
                }
            }
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            bool isAgentRunning = AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
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
                InitMethods();
            }
        }
        
        private void StartRecording()
        {
            IRecord record = (IRecord)mWindowExplorerDriver;            
            IPlatformInfo platformInfo = PlatformInfoBase.GetPlatformImpl(mContext.ActivityPlatform);

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
            xRecordingButton.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Execution");            
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
            SetControlsVisibility();
        }

        private void XIntegratePOM_Unchecked(object sender, RoutedEventArgs e)
        {
            SetControlsVisibility();
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetControlsVisibility();
        }
    }    
}
