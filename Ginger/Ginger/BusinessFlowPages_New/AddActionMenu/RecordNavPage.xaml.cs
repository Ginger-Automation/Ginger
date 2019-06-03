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
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
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
        private Activity mActParentActivity = null;
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
            xWinGridUC.mContext = mContext;
            xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;

            InitMethods();
            context.PropertyChanged += Context_PropertyChanged;
        }

        private void InitMethods()
        {
            SetPOMControlsVisibility(false);
            SetControlsDefault();
            SetMultiplePropertiesGridView();
            SetDefault(mContext);
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e != null && e.PropertyName == nameof(BusinessFlow))
            {
                mContext = (Context)sender;
                xWinGridUC.mContext = mContext;
                InitMethods();
                if (IsRecording)
                {
                    IsRecording = !IsRecording;
                    StopRecording();
                    SetRecordingButtonText();
                }
            }
        }

        /// <summary>
        /// This method will set the page to default
        /// </summary>
        /// <param name="context"></param>
        public void SetDefault(Context context)
        {
            if(mActParentActivity != context.BusinessFlow.CurrentActivity)
            {                
                xWinGridUC.UpdateWindowsList();
                xIntegratePOM.IsChecked = false;
            }
            mActParentActivity = context.BusinessFlow.CurrentActivity;

            if (AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, context, out mWindowExplorerDriver))
            {
                xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
                xStartAgentButton.IsEnabled = false;
            }
            else
            {
                xStartAgentButton.IsEnabled = true;
            }
        }

        public void SetMultiplePropertiesGridView()
        {
            gridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 250, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            gridPOMListItems.btnAdd.Click += BtnAdd_Click;
            gridPOMListItems.SetAllColumnsDefaultView(view);
            gridPOMListItems.InitViewItems();
        }
        
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem appModelFolder;

            RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
            appModelFolder = new ApplicationPOMsTreeItem(repositoryFolder);
            if (mApplicationPOMSelectionPage == null)
            {
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                        SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
                mApplicationPOMSelectionPage.SelectionDone += MAppModelSelectionPage_SelectionDone; 
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            AddSelectedPOM(selectedPOMs);
        }

        private void SetControlsDefault()
        {
            SetRecordingButtonText();

            if (mContext.BusinessFlow.Applications.Contains(ePlatformType.Web.ToString()))
            {
                xPOMPanel.Visibility = Visibility.Visible; 
            }
            else
            {
                xPOMPanel.Visibility = Visibility.Hidden;
            }
        }

        private void SetPOMControlsVisibility(bool isVisible)
        {
            if (isVisible)
            {
                gridPOMListItems.Visibility = Visibility.Visible;
                gridPOMListItems.Visibility = Visibility.Visible;
            }
            else
            {
                gridPOMListItems.Visibility = Visibility.Hidden;
                gridPOMListItems.Visibility = Visibility.Hidden;
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

        private void StartAgentButton_Click(object sender, RoutedEventArgs e)
        {
            bool isStarted = AgentHelper.StartAgent(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
            if(isStarted)
            {
                xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
                xStartAgentButton.IsEnabled = false;
                SetRecordingButtonText();
            }
        }

        private void StartRecording()
        {
            BusinessFlow bFlow = mContext.BusinessFlow;
            IRecord record = (IRecord)mWindowExplorerDriver;
            IPlatformInfo platformInfo = PlatformInfoBase.GetPlatformTargetApplication(mContext.Activity.TargetApplication);

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

            mRecordingMngr = new RecordingManager(applicationPOMs, bFlow, mContext, record, platformInfo);
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
            if (AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver) &&
                (AppWindow)xWinGridUC.WindowsComboBox.SelectedItem != null && !string.IsNullOrEmpty(((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem).Title))
            {
                xRecordingButton.IsEnabled = true;
                xStartAgentButton.IsEnabled = false;
            }
            else
            {
                xRecordingButton.IsEnabled = false;
                xStartAgentButton.IsEnabled = true;
            }

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
                xRecordingButton.ButtonImageType = eImageType.Play;
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
            SetPOMControlsVisibility(true);
        }

        private void XIntegratePOM_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPOMControlsVisibility(false);
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRecordingButtonText();
        }
    }    
}
