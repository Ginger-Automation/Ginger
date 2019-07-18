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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Drivers.Common;
using Ginger.Drivers.PowerBuilder;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Windows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyPage.xaml
    /// </summary>
    public partial class LiveSpyPage : Page
    {
        bool isSpying = false;
        Context mContext;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;
        ElementInfo mSpyElement;
        ITreeViewItem mCurrentControlTreeViewItem;
        Page mControlFrameContentPage = null;
        bool mFirstElementSelectionDone = false;
        private Act mAction;        
        IWindowExplorer mWindowExplorerDriver;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context"></param>
        public LiveSpyPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            xWinGridUC.mContext = mContext;
            context.PropertyChanged += Context_PropertyChanged;
            InitMethod();
            AgentBasedManipulations();
            InitWinGridUC();
            InitControlPropertiesGridView();
        }

        /// <summary>
        /// This event is used to initialise the WinGrid
        /// </summary>
        private void InitWinGridUC()
        {
            xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
            xWinGridUC.mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
            xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;
        }

        /// <summary>
        /// This method is used to set controls to initialise or set to default
        /// </summary>
        private void InitMethod()
        {
            this.Dispatcher.Invoke(() => {
                xWinGridUC.IsEnabled = false;
                xSpyingButton.IsEnabled = false;
                xStartAgentMessage.Visibility = Visibility.Visible;
                ControlPropertiesGrid.Visibility = Visibility.Collapsed;
            });
        }

        void AgentBasedManipulations()
        {
            if (mContext.Agent != null)
            {
                bool isAgentRunning = mContext.Agent.Status == Agent.eStatus.Running;           //      AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);

                if (mContext.Agent != null)
                    mWindowExplorerDriver = mContext.Agent.Driver as IWindowExplorer;

                if (isAgentRunning)
                {
                    xStartAgentMessage.Visibility = Visibility.Collapsed;
                    xWinGridUC.IsEnabled = true;

                    if (ControlPropertiesGrid.DataSourceList != null)
                    {
                        ControlPropertiesGrid.Visibility = Visibility.Visible;
                    }

                    if ((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem != null
                        && !string.IsNullOrEmpty(((AppWindow)xWinGridUC.WindowsComboBox.SelectedItem).Title))
                    {
                        xSpyingButton.IsEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// This method will set the explorer page to be fit in new right panel
        /// </summary>
        /// <param name="windowExplorerDriver"></param>
        public void SetWindowExplorerForNewPanel(IWindowExplorer windowExplorerDriver)
        {
            if (mWindowExplorerDriver != windowExplorerDriver)
            {
                mWindowExplorerDriver = windowExplorerDriver;
                xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver; 
            }
            else if(windowExplorerDriver == null)
            {
                xWinGridUC.WindowsComboBox.ItemsSource = null;
            }

            if (windowExplorerDriver != null && xWinGridUC.WindowsComboBox.ItemsSource == null)
            {
                xWinGridUC.UpdateWindowsList();
            }
        }

        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Context.Agent) || e.PropertyName == nameof(Context.AgentStatus))
            {
                InitMethod();
                AgentBasedManipulations();
                if (xWinGridUC.mWindowExplorerDriver == null)
                {
                    xWinGridUC.mWindowExplorerDriver = mWindowExplorerDriver;
                }
            }
        }

        /// <summary>
        /// This event is used to handle the selection event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xStartAgentMessage.Visibility = Visibility.Collapsed;
            xSpyingButton.IsEnabled = true;
        }

        /// <summary>
        /// This is the event to handle the spyclick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpyingButton_Click(object sender, RoutedEventArgs e)
        {
            SpyButtonClick();
        }

        /// <summary>
        /// This method is used to perform the button click opertion
        /// </summary>
        private void SpyButtonClick()
        {
            isSpying = !isSpying;
            UpdateUI();

            if (xWinGridUC.comboBoxSelectedValue != null)
            {
                if (isSpying)
                {
                    if (dispatcherTimer == null)
                    {
                        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                        dispatcherTimer.Tick += new EventHandler(timenow);
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    }

                    dispatcherTimer.IsEnabled = true;
                }
                else
                {
                    dispatcherTimer.IsEnabled = false;
                }
            }
            else
            {
                isSpying = false;
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
            }
        }

        private void UpdateUI()
        {
            if (isSpying)
            {
                xSpyingButton.ToolTip = "Stop Spying";
                xSpyingButton.Background = Brushes.DeepSkyBlue;
                xSpyingButton.BorderThickness = new Thickness(2);
                StatusTextBlock.Text = "Spying...";
            }
            else
            {
                StatusTextBlock.Text = "";
                xSpyingButton.ToolTip = "Start Spying";
                xSpyingButton.Background = Brushes.Transparent;
            }
        }

        private void timenow(object sender, EventArgs e)
        {
            if (xWinGridUC.mWindowExplorerDriver.GetType() == typeof(GingerCore.Drivers.SeleniumDriver) 
                && ((GingerCore.Drivers.SeleniumDriver)xWinGridUC.mWindowExplorerDriver).Platform == ePlatformType.Web)
            {
                xWinGridUC.mWindowExplorerDriver.StartSpying(); 
            }
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                StatusTextBlock.Text = "Spying Element, Please Wait...";
                GingerCore.General.DoEvents();
                mSpyElement = xWinGridUC.mWindowExplorerDriver.GetControlFromMousePosition();
                if (mSpyElement != null)
                {
                    xWinGridUC.mWindowExplorerDriver.LearnElementInfoDetails(mSpyElement);
                    StatusTextBlock.Text = mSpyElement.ElementName;
                    mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(mSpyElement);
                    ShowCurrentControlInfo();
                }
                else
                {
                    StatusTextBlock.Text = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void InitControlPropertiesGridView()
        {
            // Grid View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = "Name", WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "Value", WidthWeight = 20, ReadOnly = true });

            ControlPropertiesGrid.SetAllColumnsDefaultView(view);
            ControlPropertiesGrid.InitViewItems();
        }

        private void ShowCurrentControlInfo()
        {
            if (mCurrentControlTreeViewItem == null) return;
            ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
            try
            {
                if (xWinGridUC.mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
                {
                    EI.WindowExplorer = xWinGridUC.mWindowExplorerDriver;
                    xWinGridUC.mWindowExplorerDriver.HighLightElement(EI);

                    //General tab will show the generic element info page, customized page will be in Data tab
                    mControlFrameContentPage = new ElementInfoPage(EI);
                    ControlFrame.Content = mControlFrameContentPage;
                    SetDetailsExpanderDesign(true, EI);
                    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
                    {
                        ControlPropertiesGrid.DataSourceList = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();
                        ControlPropertiesGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        ControlPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    ShowControlActions(mCurrentControlTreeViewItem);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ObjectUnavailable);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
            }
        }

        private void SetDetailsExpanderDesign(bool detailsExist, ElementInfo selectedElementInfo)
        {
            if (detailsExist == false)
            {
                SelectedControlDetailsExpander.Visibility = Visibility.Collapsed;
                SelectedControlDetailsExpanderLable.Content = "Selected Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = Brushes.Gray;
                SelectedControlDetailsExpander.IsEnabled = false;
                SelectedControlDetailsExpander.IsExpanded = false;
            }
            else
            {
                SelectedControlDetailsExpander.Visibility = Visibility.Visible;
                SelectedControlDetailsExpanderLable.Content = "'" + selectedElementInfo.ElementType + " : " + selectedElementInfo.ElementName + "' Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString()); ;
                SelectedControlDetailsExpander.IsEnabled = true;
                if (mFirstElementSelectionDone == false)
                {
                    SelectedControlDetailsExpander.IsExpanded = true;
                    mFirstElementSelectionDone = true;
                }
            }
        }

        private void ShowControlActions(ITreeViewItem iv)
        {
            //We show action if we didn't came from Edit page
            if (iv is IWindowExplorerTreeItem)
            {
                ElementInfo EI = (ElementInfo)iv.NodeObject();

                ControlActionsPage CAP = null;
                // We came from Action EditPage
                if (mAction != null)
                {
                    CAP = new ControlActionsPage(mAction, EI);
                }
                else
                {
                    ObservableList<Act> list = new ObservableList<Act>();
                    ObservableList<ActInputValue> actInputValuelist = new ObservableList<ActInputValue>();
                    if (xWinGridUC.mPlatform.PlatformType() == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                    {
                        list = xWinGridUC.mPlatform.GetPlatformElementActions(EI);
                    }
                    else
                    {                                                               // this "else" is temporary. Currently only ePlatformType.Web is overided
                        list = ((IWindowExplorerTreeItem)iv).GetElementActions();   // case will be removed once all platforms will be overrided
                    }                                                               //

                    //If no element actions returned then no need to get locator's. 
                    if (list == null || list.Count == 0)
                    {
                        SetActionsTabDesign(false);
                        return;
                    }
                    else
                    {
                        Page DataPage = mCurrentControlTreeViewItem.EditPage();
                        actInputValuelist = ((IWindowExplorerTreeItem)iv).GetItemSpecificActionInputValues();
                        CAP = new ControlActionsPage(xWinGridUC.mWindowExplorerDriver, EI, list, DataPage, actInputValuelist, mContext);
                    }
                }
                ControlActionsFrame.Content = CAP;
                SetActionsTabDesign(true);
            }
            else
            {
                SetActionsTabDesign(false);
            }
        }

        private void SetActionsTabDesign(bool actionsExist)
        {
            if (actionsExist == false)
            {
                ActionsTab.IsEnabled = false;
                ControlActionsFrame.Visibility = System.Windows.Visibility.Collapsed;
                GeneralTab.IsSelected = true;
            }
            else
            {
                ActionsTab.IsEnabled = true;
                ActionsTab.IsSelected = true;
                ControlActionsFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SelectedControlDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlDetailsRow.Height = new GridLength(35);
            ControlDetailsRow.MaxHeight = 35;
        }

        private void SelectedControlDetailsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (mControlFrameContentPage == null)
            {
                SelectedControlDetailsExpander.IsExpanded = false;
                return;
            }
            ControlDetailsRow.Height = new GridLength(200, GridUnitType.Star);
            ControlDetailsRow.MaxHeight = Double.PositiveInfinity;
        }

        private void ControlTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: check tab and do it only on props, only once per control for speed - cache
            // Add in Grid refresh button if needed
            //if (e.Source  == ControlTabs)  // Handle selection change only from the tab, otherwise it might bubble up from grid or alike
            //{
            //    ShowCurrentControlInfo();
            //}
        }
    }
}
