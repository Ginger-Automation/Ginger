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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Drivers.Common;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyPage.xaml
    /// </summary>
    public partial class LiveSpyPage : Page
    {
        bool isSpying = false;
        Context mContext;
        IWindowExplorer mDriver;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;
        ElementInfo mSpyElement;
        ITreeViewItem mCurrentControlTreeViewItem;
        Page mControlFrameContentPage = null;
        bool mFirstElementSelectionDone = false;
        private Act mAction;     
        
        

        public LiveSpyPage(Context context, IWindowExplorer driver)
        {
            InitializeComponent();

            mContext = context;
            xWindowSelectionUC.mContext = mContext;

            SetDriver(driver);
            SetSpyingControls();
            InitControlPropertiesGridView();
        }

        public void SetDriver(IWindowExplorer windowExplorerDriver)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mDriver != windowExplorerDriver)
                {
                    mDriver = windowExplorerDriver;
                    xWindowSelectionUC.mWindowExplorerDriver = mDriver;
                    xWindowSelectionUC.mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
                }

                if (windowExplorerDriver == null)
                {
                    xWindowSelectionUC.WindowsComboBox.ItemsSource = null;
                }

                if (windowExplorerDriver != null && xWindowSelectionUC.WindowsComboBox.ItemsSource == null)
                {
                    xWindowSelectionUC.UpdateWindowsList();
                }
            });
        }

        private void SpyingButton_Click(object sender, RoutedEventArgs e)
        {
            if (xWindowSelectionUC.SelectedWindow != null)
            {
                isSpying = true;
                SetSpyingControls();

                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(SpyTimerHandler);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                dispatcherTimer.IsEnabled = true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
                StopSpying();
            }
        }

        private void xStopSpyingBtn_Click(object sender, RoutedEventArgs e)
        {
            StopSpying();
        }

        private void StopSpying()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.IsEnabled = false;
            }
            isSpying = false;
            SetSpyingControls();
        }

        private void SetSpyingControls()
        {
            if (isSpying)
            {
                xSpyingButton.ButtonText = "Spying...";
                xSpyingButton.ToolTip = "Spying Window Elements";
                xSpyingButton.ButtonImageType = eImageType.Processing;
                xSpyingButton.IsEnabled = false;

                xStatusTextBlock.Text = "Hover with the mouse over the element you want to spy and click/hold down the 'Ctrl' Key";
                xStatusTextBlock.Visibility = Visibility.Visible;
                xStatusTextBlock.Foreground = (Brush)FindResource("$HighlightColor_Purple");
                xStopSpyingBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xSpyingButton.ButtonText = "Spy";
                xSpyingButton.ToolTip = "Start Spying";
                xSpyingButton.ButtonImageType = eImageType.Spy;
                xSpyingButton.IsEnabled = true;

                xStatusTextBlock.Visibility = Visibility.Collapsed;

                xStopSpyingBtn.Visibility = Visibility.Collapsed;
            }
            xSpyingButton.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Highlighted");            

            if (xControlPropertiesGrid.DataSourceList != null)
            {
                xControlPropertiesGrid.Visibility = Visibility.Visible;
            }
            else
            {
                xControlPropertiesGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void SpyTimerHandler(object sender, EventArgs e)
        {
            if (mDriver != null)
            {
                ///?? why we have specific driver handleing?
                //if (xWindowSelectionUC.mWindowExplorerDriver.GetType() == typeof(GingerCore.Drivers.SeleniumDriver) 
                //&& ((GingerCore.Drivers.SeleniumDriver)xWindowSelectionUC.mWindowExplorerDriver).Platform == ePlatformType.Web)
                //{
                //    xWindowSelectionUC.mWindowExplorerDriver.StartSpying();
                //}

                // Get control info only if control key is pressed
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    xStatusTextBlock.Text = "Element been identified, please wait...";
                    xStatusTextBlock.Foreground = (Brush)FindResource("$RunningStatusColor");
                    GingerCore.General.DoEvents();
                    mSpyElement = xWindowSelectionUC.mWindowExplorerDriver.GetControlFromMousePosition();
                    if (mSpyElement != null)
                    {
                        xWindowSelectionUC.mWindowExplorerDriver.LearnElementInfoDetails(mSpyElement);
                        xStatusTextBlock.Text = "Element was identified, see details below.";//string.Format("The element '{0}' was identified", mSpyElement.ElementName);                    
                        xStatusTextBlock.Foreground = (Brush)FindResource("$PassedStatusColor");
                        GingerCore.General.DoEvents();
                        mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(mSpyElement);
                        ShowCurrentControlInfo();
                    }
                    else
                    {
                        xStatusTextBlock.Text = "Failed to identify the element.";
                        xStatusTextBlock.Foreground = (Brush)FindResource("$FailedStatusColor");
                        GingerCore.General.DoEvents();
                    }
                }
            }
            else
            {
                StopSpying();
            }
        }

        private void InitControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = "Name", WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "Value", WidthWeight = 20, ReadOnly = true });

            xControlPropertiesGrid.SetAllColumnsDefaultView(view);
            xControlPropertiesGrid.InitViewItems();
        }

        private void ShowCurrentControlInfo()
        {
            if (mCurrentControlTreeViewItem == null) return;
            ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
            try
            {
                if (xWindowSelectionUC.mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
                {
                    EI.WindowExplorer = xWindowSelectionUC.mWindowExplorerDriver;
                    xWindowSelectionUC.mWindowExplorerDriver.HighLightElement(EI);

                    //General tab will show the generic element info page, customized page will be in Data tab
                    mControlFrameContentPage = new ElementInfoPage(EI);
                    ControlFrame.Content = mControlFrameContentPage;
                    SetDetailsExpanderDesign(true, EI);
                    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
                    {
                        xControlPropertiesGrid.DataSourceList = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();
                        xControlPropertiesGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        xControlPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception in LiveSpy page ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
            }
        }

        private void SetDetailsExpanderDesign(bool detailsExist, ElementInfo selectedElementInfo)
        {
            if (detailsExist == false)
            {
                xSelectedControlDetailsExpander.Visibility = Visibility.Collapsed;
                xSelectedControlDetailsExpanderLable.Content = "Selected Element Details & Actions";
                xSelectedControlDetailsExpanderLable.Foreground = Brushes.Gray;
                xSelectedControlDetailsExpander.IsEnabled = false;
                xSelectedControlDetailsExpander.IsExpanded = false;
            }
            else
            {
                xSelectedControlDetailsExpander.Visibility = Visibility.Visible;
                xSelectedControlDetailsExpanderLable.Content = "'" + selectedElementInfo.ElementType + " : " + selectedElementInfo.ElementName + "' Element Details & Actions";
                xSelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_DarkBlue")).ToString()); ;
                xSelectedControlDetailsExpander.IsEnabled = true;
                if (mFirstElementSelectionDone == false)
                {
                    xSelectedControlDetailsExpander.IsExpanded = true;
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

                    ePlatformType mPlatformType = xWindowSelectionUC.mPlatform.PlatformType();

                    // if platform is web or
                    // platform is java and element info type is javaelementinfo 
                    if (mPlatformType == ePlatformType.Web || mPlatformType == ePlatformType.Java)
                    {
                        list = xWindowSelectionUC.mPlatform.GetPlatformElementActions(EI);
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

                        Page DataPage = mCurrentControlTreeViewItem.EditPage(mContext);
                        actInputValuelist = ((IWindowExplorerTreeItem)iv).GetItemSpecificActionInputValues();
                        CAP = new ControlActionsPage(xWindowSelectionUC.mWindowExplorerDriver, EI, list, DataPage, actInputValuelist, mContext);
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
                xActionsTab.IsEnabled = false;
                ControlActionsFrame.Visibility = System.Windows.Visibility.Collapsed;
                xGeneralTab.IsSelected = true;
            }
            else
            {
                xActionsTab.IsEnabled = true;
                xActionsTab.IsSelected = true;
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
                xSelectedControlDetailsExpander.IsExpanded = false;
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
