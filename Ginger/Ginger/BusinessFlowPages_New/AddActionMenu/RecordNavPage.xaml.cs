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
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
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
        RecordingManager RecordingMngr;

        public RecordNavPage(Context context, IWindowExplorer windowExplorerDriver)
        {
            InitializeComponent();
            mContext = context;
            mWindowExplorerDriver = windowExplorerDriver;
            xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
            xWinGridUC.mContext = mContext;
            xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;

            SetPOMControlsVisibility(false);
            SetControlsDefault();
            SetMultiplePropertiesGridView();
        }

        public void SetMultiplePropertiesGridView()
        {
            gridPOMListItems.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ItemName), Header = "Name", WidthWeight = 120, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(POMBindingObjectHelper.ContainingFolder), Header = "ContainingFolder", WidthWeight = 150, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            gridPOMListItems.btnAdd.Click += BtnAdd_Click;
            gridPOMListItems.SetAllColumnsDefaultView(view);
            gridPOMListItems.InitViewItems();
        }
        
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem appModelFolder;

            RepositoryFolder<ApplicationPOMModel> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
            appModelFolder = new ApplicationPOMsTreeItem(repositoryFolder);
            mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, appModelFolder,
                                                                                SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
            mApplicationPOMSelectionPage.SelectionDone += MAppModelSelectionPage_SelectionDone;

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            AddSelectedBuinessFlows(selectedPOMs);
        }

        private void SetControlsDefault()
        {
            if ((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem != null && !string.IsNullOrEmpty(((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem).Title))
            {
                UpdateUI(true);
            }
            else
            {
                UpdateUI(false);
            }

            if (mContext.BusinessFlow.Applications.Contains("Web"))
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
            IsRecording = !IsRecording;
            UpdateUI(true);

            if (xWinGridUC.comboBoxSelectedValue != null)
            {
                if (IsRecording)
                {                   
                    StartRecording();
                }
                else
                {
                    StopRecording();
                    UpdateUI(false);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
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

            RecordingMngr = new RecordingManager(applicationPOMs, bFlow, mContext, record, platformInfo);
            RecordingMngr.StartRecording();
        }

        public void StopRecording()
        {
            if (RecordingMngr != null)
            {
                RecordingMngr.StopRecording();
                UpdateUI(false);
            }
        }

        private void UpdateUI(bool isEnabled)
        {
            xRecordingButton.IsEnabled = isEnabled;
            if (isEnabled)
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
                    xRecordingButton.ButtonImageType = eImageType.Play;
                }
                xRecordingButton.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Execution");
            }
        }

        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        ObservableList<POMBindingObjectHelper> PomModels = new ObservableList<POMBindingObjectHelper>();
        
        private void MAppModelSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            AddSelectedBuinessFlows(e.SelectedItems);
        }

        private void AddSelectedBuinessFlows(List<object> selectedPOMs)
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
            AppWindow AW = (AppWindow)xWinGridUC.WindowsComboBox.SelectedItem;
            if (AW == null)
            {
                UpdateUI(false);
            }
            else
            {
                UpdateUI(true);
            }
        }
    }    
}
