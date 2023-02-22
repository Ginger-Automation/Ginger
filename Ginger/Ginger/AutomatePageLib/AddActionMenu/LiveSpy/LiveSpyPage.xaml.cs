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
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyPage.xaml
    /// </summary>
    public partial class LiveSpyPage : Page
    {
        bool isSpying = false;
        Context mContext;
        IWindowExplorer mWindowExplorerDriver;

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

            xUCElementDetails.Context = mContext;

            SetDriver(driver);
            SetSpyingControls();

            //xUCElementDetails.xPropertiesGrid.btnRefresh.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(RefreshControlProperties));

            InitUCElementDetailsLocatorsGrid();

            xUCElementDetails.PropertyChanged += XUCElementDetails_PropertyChanged;

            if (mContext.Platform == ePlatformType.Web)
            {
                xUCElementDetails.xElementScreenShotFrameTop.Visibility = Visibility.Visible;
            }
            else
            {
                xUCElementDetails.xElementScreenShotFrameTop.Visibility = Visibility.Collapsed;
            }
            xUCElementDetails.xElementScreenShotFrame.Visibility = Visibility.Collapsed;
            xUCElementDetails.xRightImageSection.Width = new GridLength(0, GridUnitType.Pixel);

        }

        private void XUCElementDetails_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(xUCElementDetails.SelectedElement))
            {
                HandleUCElementDetails();
            }
        }

        bool ElementDetailsNotNullHandled = false;
        bool ElementDetailsNullHandled = false;
        public void HandleUCElementDetails()
        {
            if (xUCElementDetails.SelectedElement == null && !ElementDetailsNullHandled)
            {
                ControlDetailsRow.Height = new GridLength(0);

                ElementDetailsNullHandled = true;
                ElementDetailsNotNullHandled = false;
            }
            else if (xUCElementDetails.SelectedElement != null && !ElementDetailsNotNullHandled)
            {
                ControlDetailsRow.Height = new GridLength(150, GridUnitType.Star);

                ElementDetailsNotNullHandled = true;
                ElementDetailsNullHandled = false;
            }
        }

        public void InitUCElementDetailsLocatorsGrid()
        {
            if (mWindowExplorerDriver != null)
            {
                if (mWindowExplorerDriver.IsPOMSupported())
                {
                    xUCElementDetails.InitLocatorsGridView();
                }
                else
                {
                    xUCElementDetails.InitLegacyLocatorsGridView();
                }
            }
        }

        public void SetDriver(IWindowExplorer windowExplorerDriver)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mWindowExplorerDriver != windowExplorerDriver)
                {
                    mWindowExplorerDriver = windowExplorerDriver;
                    xWindowSelectionUC.mWindowExplorerDriver = mWindowExplorerDriver;
                    xWindowSelectionUC.Platform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
                    xUCElementDetails.WindowExplorerDriver = windowExplorerDriver;

                    InitUCElementDetailsLocatorsGrid();
                }

                if (windowExplorerDriver == null)
                {
                    xWindowSelectionUC.WindowsComboBox.ItemsSource = null;
                    xUCElementDetails.SelectedElement = null;
                }

                if (windowExplorerDriver != null && xWindowSelectionUC.WindowsComboBox.ItemsSource == null)
                {
                    xWindowSelectionUC.UpdateWindowsList();
                    xUCElementDetails.SelectedElement = null;
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

            //if (xControlPropertiesGrid.DataSourceList != null)
            //{
            //    xControlPropertiesGrid.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    xControlPropertiesGrid.Visibility = Visibility.Collapsed;
            //}
        }

        private void SpyTimerHandler(object sender, EventArgs e)
        {
            if (mWindowExplorerDriver != null && ((AgentOperations)mContext.Agent.AgentOperations).Status == Agent.eStatus.Running)    //((GingerCore.Drivers.DriverBase)mWindowExplorerDriver).IsDriverRunning)
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
                    mSpyElement = mWindowExplorerDriver.GetControlFromMousePosition();

                    if (mSpyElement != null)
                    {
                        mWindowExplorerDriver.UnHighLightElements();
                        mSpyElement.WindowExplorer = mWindowExplorerDriver;
                        xWindowSelectionUC.mWindowExplorerDriver.LearnElementInfoDetails(mSpyElement);
                        xStatusTextBlock.Text = "Element was identified, see details below.";//string.Format("The element '{0}' was identified", mSpyElement.ElementName);                    
                        xStatusTextBlock.Foreground = (Brush)FindResource("$PassedStatusColor");
                        GingerCore.General.DoEvents();
                        mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(mSpyElement);
                        mWindowExplorerDriver.HighLightElement(mSpyElement);
                        xUCElementDetails.SelectedElement = mSpyElement;
                        //update screenshot
                        BitmapSource source = null;
                        if (mSpyElement.ScreenShotImage != null)
                        {
                            source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mSpyElement.ScreenShotImage.ToString()));
                        }
                        xUCElementDetails.xElementScreenShotFrameTop.Content = new ScreenShotViewPage(mSpyElement?.ElementName, source, false);
                    }
                    else
                    {
                        xUCElementDetails.SelectedElement = null;
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

        //private void ShowCurrentControlInfo()
        //{
        //    if (mCurrentControlTreeViewItem == null) return;
        //    ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
        //    try
        //    {
        //        //if (xWindowSelectionUC.mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
        //        //{
        //        //    EI.WindowExplorer = xWindowSelectionUC.mWindowExplorerDriver;
        //        //    xWindowSelectionUC.mWindowExplorerDriver.HighLightElement(EI);

        //        //    //General tab will show the generic element info page, customized page will be in Data tab
        //        //    mControlFrameContentPage = new ElementInfoPage(EI);
        //        //    ControlFrame.Content = mControlFrameContentPage;
        //        //    SetDetailsExpanderDesign(true, EI);
        //        //    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
        //        //    {
        //        //        xControlPropertiesGrid.DataSourceList = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();
        //        //        xControlPropertiesGrid.Visibility = System.Windows.Visibility.Visible;
        //        //    }
        //        //    else
        //        //    {
        //        //        xControlPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
        //        //    }
        //        //    ShowControlActions(mCurrentControlTreeViewItem);
        //        //}
        //        //else
        //        //{
        //        //    Reporter.ToUser(eUserMsgKey.ObjectUnavailable);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Reporter.ToLog(eLogLevel.ERROR, "Exception in LiveSpy page ShowCurrentControlInfo", ex);
        //        Reporter.ToUser(eUserMsgKey.ObjectLoad);
        //    }
        //}
    }
}
