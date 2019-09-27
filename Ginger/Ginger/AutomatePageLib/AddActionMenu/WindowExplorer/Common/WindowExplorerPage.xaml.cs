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
using Ginger.Actions.Locators.ASCF;
using Ginger.Drivers.Common;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
using Ginger.WindowExplorer.Android;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.Common;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Mainframe;
using Ginger.WindowExplorer.Windows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.UserControls;
using GingerCore.Platforms.PlatformsInfo;
using System.Linq;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions.Common;

namespace Ginger.WindowExplorer
{
    //Generic Class for Window Explorer to be used by different drivers and explore the window of the Application Under Testing, 
    // for PBDriver, ASCF, Windows and Selenium etc...

    public partial class WindowExplorerPage : Page
    {                
        GenericWindow _GenWin;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;
        IWindowExplorer mWindowExplorerDriver;
        ITreeViewItem mCurrentControlTreeViewItem;
        TreeViewItem mTreeRootItem;
        ITreeViewItem mRootItem;
        ElementInfo mSpyElement;
        ApplicationAgent mApplicationAgent;

        PlatformInfoBase mPlatform;

        //If we come from ActionEditPage keep the Action
        private Act mAction;
        ObservableList<ElementInfo> VisibleElementsInfoList = new ObservableList<ElementInfo>();
        bool mSyncControlsViewWithLiveSpy = false;
        bool mFirstElementSelectionDone = false;
        Page mControlFrameContentPage = null;
        WindowExplorerPOMPage mWindowExplorerPOMPage;

        Context mContext;

        // We can open it from agents grid, or from Action Edit page with Action 
        // If we open from ActionEdit Page then we update the act with Locator
        public WindowExplorerPage(ApplicationAgent ApplicationAgent, Context context, Act Act = null)
        {           
            InitializeComponent();
            WindowControlsTreeView.TreeGrid.RowDefinitions[0].Height = new GridLength(0);

            WindowControlsTreeView.SearchStarted += WindowControlsTreeView_SearchStarted;
            WindowControlsTreeView.SearchCancelled += WindowControlsTreeView_SearchCancelled;
            WindowControlsTreeView.SearchCompleted += WindowControlsTreeView_SearchCompleted;

            mContext = context;
            
            mPlatform = PlatformInfoBase.GetPlatformImpl(((Agent)ApplicationAgent.Agent).Platform);

            //Instead of check make it disabled ?
            if (((Agent)ApplicationAgent.Agent).Driver != null && (((Agent)ApplicationAgent.Agent).Driver is IWindowExplorer) == false)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Control selection is not available yet for driver - " + ((Agent)ApplicationAgent.Agent).Driver.GetType().ToString());
                _GenWin.Close();
                return;                
            }
            
            IWindowExplorer WindowExplorerDriver = (IWindowExplorer)((Agent)ApplicationAgent.Agent).Driver;
                
            mWindowExplorerDriver = WindowExplorerDriver;
            mAction = Act;
            mApplicationAgent = ApplicationAgent;

            if (WorkSpace.Instance.BetaFeatures.ShowPOMInWindowExplorer)
            {
                if (mWindowExplorerDriver is IPOM)
                {
                    POMFrameRow.Height = new GridLength(100, GridUnitType.Star);
                    mWindowExplorerPOMPage = new WindowExplorerPOMPage(mApplicationAgent);
                    POMFrame.Content = mWindowExplorerPOMPage;
                    ((IPOM)mWindowExplorerDriver).ActionRecordedCallback(mWindowExplorerPOMPage.ActionRecorded);
                }
            }
            else
            {
                POMFrameRow.Height = new GridLength(0);
                POMButton.Visibility = Visibility.Collapsed;
            }
            
            WindowControlsTreeView.TreeTitleStyle = (Style)TryFindResource("@NoTitle");
            WindowControlsTreeView.Tree.ItemSelected += WindowControlsTreeView_ItemSelected;

            SetControlsGridView();

            InitControlPropertiesGridView();
            ControlPropertiesGrid.btnRefresh.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(RefreshControlProperties));

            UpdateWindowsList();

            SetDetailsExpanderDesign(false, null);
            SetActionsTabDesign(false);

            if(mAction==null)
            {
                StepstoUpdateActionRow.Height = new GridLength(0);
            }

            ((ImageMakerControl)(ControlsRefreshButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_White");            
        }

        /// <summary>
        /// This method will set the explorer page to be fit in new right panel
        /// </summary>
        /// <param name="windowExplorerDriver"></param>
        public void SetDriver(IWindowExplorer windowExplorerDriver)
        {
            this.Dispatcher.Invoke(() =>
            {
                RecordingButton.Visibility = Visibility.Collapsed;
                mWindowExplorerDriver = windowExplorerDriver;
                UpdateWindowsList();
            });
        }

        private void RefreshControlProperties(object sender, RoutedEventArgs e)
        {
            //TODO: fix me for cached properties like ASCFBrowserElements, it will not work
            if (mCurrentControlTreeViewItem != null)
            ControlPropertiesGrid.DataSourceList = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();            
        }

        private void UpdateWindowsList()
        {
            try
            {
                if (mWindowExplorerDriver != null)
                {
                    List<AppWindow> list = mWindowExplorerDriver.GetAppWindows();
                    WindowsComboBox.ItemsSource = list;
                    WindowsComboBox.DisplayMemberPath = "WinInfo";

                    AppWindow ActiveWindow = mWindowExplorerDriver.GetActiveWindow();

                    if (ActiveWindow != null)
                    {
                        foreach (AppWindow w in list)
                        {
                            if (w.Title == ActiveWindow.Title && w.Path == ActiveWindow.Path)
                            {
                                WindowsComboBox.SelectedValue = w;
                                return;
                            }
                        }
                    }

                    //TODO: If no selection then select the first if only one window exist in list
                    if (!(mWindowExplorerDriver is SeleniumAppiumDriver))//FIXME: need to work for all drivers and from some reason failing for Appium!!
                    {
                        if (WindowsComboBox.Items.Count == 1)
                        {
                            WindowsComboBox.SelectedValue = WindowsComboBox.Items[0];
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while performing Update Window Explorer List", ex);
            }
            
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = "Ginger Window Explorer";

            //if we come from ActionEditPage we need to give the user to update only Action selector + show him some message how to do it
            if (mAction != null)
            {
                ActionsTab.Header = "Action Locator Selection";
                SelectedControlDetailsExpander.IsExpanded = true;
                ActionsTab.IsSelected = true;
            } 
            else
            {
                StepstoUpdateActionRow.Height = new GridLength(0);
            }
            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this, closeEventHandler: CloseWindow);            
        }

        private async void CloseWindow(object sender, EventArgs e)
        {
            //stop Live Spy
            if (LiveSpyButton.IsChecked == true)
                if (dispatcherTimer != null)
                    dispatcherTimer.IsEnabled = false;

            //stop Recording
            if (RecordingButton.IsChecked == true)
                StopRecording();

            if(WindowControlsTreeView.IsSearchRunning())
            {
                await WindowControlsTreeView.CancelSearchAsync();
            }

            _GenWin.Close();
        }

        void InitTree(ITreeViewItem RootItem)
        {
            WindowControlsTreeView.Tree.ClearTreeItems();
            mRootItem = RootItem;

            mTreeRootItem = WindowControlsTreeView.Tree.AddItem(RootItem);
            mTreeRootItem.IsExpanded = false;            
        }
        
        private void RefreshTreeControls()
        {
            WindowControlsTreeView.Tree.ClearTreeItems();
            if (WindowsComboBox.SelectedValue != null && mRootItem != null)
            {
            TreeViewItem TVI = WindowControlsTreeView.Tree.AddItem(mRootItem);
            TVI.IsExpanded = false;
        }
        }
        
        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            //stop Recording
            if (RecordingButton.IsChecked == true)
                StopRecording();

            if (WindowsComboBox.SelectedValue != null)
            {
                if (LiveSpyButton.IsChecked == true)
                {
                    if (mSyncControlsViewWithLiveSpy == false)
                        ControlsViewsExpander.IsExpanded = false;

                    SetPageFunctionalityEnableDisable(false,true,false,false,false,true,true,true,true,true,false);
                    
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
                    SetPageFunctionalityEnableDisable(true, true, true, true, true,true,true,true,true,true,true);
                }               
            }
            else
            {
                LiveSpyButton.IsChecked = false;
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);                
            }   
        }

        private void SetPageFunctionalityEnableDisable(bool RecordingButtonFlag, bool ControlsRefreshButtonFlag, bool RefreshWindowsButtonFlag, bool AddSwitchWindowActionButtonFlag, bool WindowsComboBoxFlag,bool ControlsViewsExpanderFlag,bool SelectedControlDetailsExpanderFlag,bool LiveSpyButtonFlag, bool SyncWithLiveSpyButtonFlag,bool GridTreeViewButtonFlag, bool DORButtonFlag)
        {
            RecordingButton.IsEnabled = RecordingButtonFlag;
            if (RecordingButtonFlag)
            {
                if (RecordingButton.IsChecked == true)
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Record_24x24.png"));
                    RecordingButton.Content = image;
                    RecordingButton.ToolTip = "Stop Recording";
                }
                else
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordOff_24x24.png"));
                    RecordingButton.Content = image;
                    RecordingButton.ToolTip = "Start Recording";
                }
            }
            else
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordDisable_24x24.png"));
                RecordingButton.Content = image;
            }
            
            ControlsRefreshButton.IsEnabled = ControlsRefreshButtonFlag;

            if (ControlsRefreshButtonFlag)
                ((ImageMakerControl)(ControlsRefreshButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_White");            
            else
                ((ImageMakerControl)(ControlsRefreshButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_Gray");

            RefreshWindowsButton.IsEnabled = RefreshWindowsButtonFlag;
                        
            //if (RefreshWindowsButtonFlag)
            //    ((ImageMakerControl)(RefreshWindowsButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");                            
            //else
            //    ((ImageMakerControl)(RefreshWindowsButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_Gray");


            AddSwitchWindowActionButton.IsEnabled = AddSwitchWindowActionButtonFlag;
            //if (AddSwitchWindowActionButtonFlag)
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@AddToList_16x16.png"));
            //    AddSwitchWindowActionButton.Content = image;
            //}
            //else
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@AddToList_Gray_16x16.png"));
            //    AddSwitchWindowActionButton.Content = image;
            //}

            WindowsComboBox.IsEnabled = WindowsComboBoxFlag;
            //if (WindowsComboBoxFlag)
            //{
            //    WindowsComboBox.Foreground = Brushes.Orange;
            //}
            //else
            //{
            //    WindowsComboBox.Foreground = Brushes.Gray;
            //}

            ControlsViewsExpander.IsEnabled = ControlsViewsExpanderFlag;
            if (ControlsViewsExpanderFlag)
                ControlsViewsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString());
            else
                ControlsViewsExpanderLable.Foreground = Brushes.Gray;


            SelectedControlDetailsExpander.IsEnabled = SelectedControlDetailsExpanderFlag;
            if (SelectedControlDetailsExpanderFlag)
                SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString());
            else
                SelectedControlDetailsExpanderLable.Foreground = Brushes.Gray;

            LiveSpyButton.IsEnabled = LiveSpyButtonFlag;
            if (LiveSpyButtonFlag)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Spy_24x24.png"));
                LiveSpyButton.Content = image;
            }
            else
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Spy_Gray_24x24.png"));
                LiveSpyButton.Content = image;
            }

            SyncWithLiveSpyButton.IsEnabled = SyncWithLiveSpyButtonFlag;
            if (SyncWithLiveSpyButtonFlag)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyWhite_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
            }
            else
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyGray_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
            }

            GridTreeViewButton.IsEnabled = GridTreeViewButtonFlag;
            if (GridTreeViewButton.ToolTip.ToString().Contains("Tree") == true)
            {
                if (GridTreeViewButtonFlag)
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
                    GridTreeViewButton.Content = image;
                }
                else
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_Gray_24x24.png"));
                    GridTreeViewButton.Content = image;
                }
            }
            else
            {
                if (GridTreeViewButtonFlag)
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
                    GridTreeViewButton.Content = image;
                }
                else
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_Gray_24x24.png"));
                    GridTreeViewButton.Content = image;
                }

            }

            POMButton.IsEnabled = DORButtonFlag;
            if (DORButtonFlag)
            {
                //TODO: change to gray icon
            }
            else
            {
            }
        }

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))            
            {
                StatusTextBlock.Text = "Spying Element, Please Wait...";
                GingerCore.General.DoEvents();
                mSpyElement = mWindowExplorerDriver.GetControlFromMousePosition();
                if (mSpyElement != null)
                {
                    mWindowExplorerDriver.LearnElementInfoDetails(mSpyElement);
                    StatusTextBlock.Text = mSpyElement.XPath;
                    if (mSyncControlsViewWithLiveSpy)
                    {
                        //TODO: Check Why its here
                        FocusSpyItemOnControlTree();
                    }
                    else
                    {
                        mCurrentControlTreeViewItem = GetTreeViewItemForElementInfo(mSpyElement);
                        ShowCurrentControlInfo();
                    }
                }
                else
                {
                    StatusTextBlock.Text = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void FocusSpyItemOnControlTree()
        {
            //TODO: run the search on background worker so will work fast without user impact 
            if (mSpyElement == null) return;

            StatusTextBlock.Text = mSpyElement.XPath;
            if (WindowControlsGridView.Visibility == System.Windows.Visibility.Visible)
            {
                foreach (ElementInfo EI in VisibleElementsInfoList)
                {
                    if (EI.XPath == mSpyElement.XPath && EI.Path == mSpyElement.Path)
                    {
                        VisibleElementsInfoList.CurrentItem = EI;
                        WindowControlsGridView.ScrollToViewCurrentItem();
                        ShowCurrentControlInfo();
                        break;
                    }
                }
            }
            else
            {
                TreeViewItem spyViemItem = FindMatchingTreeItemByElementXPath(mTreeRootItem, mSpyElement.XPath, mSpyElement.Path);
                if (spyViemItem != null)
                {
                    ITreeViewItem spyItem = (ITreeViewItem)spyViemItem.Tag;
                    WindowControlsTreeView.Tree.SelectItem(spyItem);
                }
                else
                {
                    //TODO:If item not found in a tree and user confirms add it to control tree                        
                    if ((Reporter.ToUser(eUserMsgKey.ConfirmToAddTreeItem)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        //TODO: Need to move this to IWindowExplorer and each driver will implement this and return matching ITreeViewItem for Element.
                        if(mSpyElement is UIAElementInfo)
                        {
                            UIAElementInfo UEI=(UIAElementInfo)mSpyElement;
                            UIAElementInfo rootEI = ((UIAElementInfo)mRootItem.NodeObject());

                            if (UEI.WindowExplorer.GetType() == typeof(PBDriver)) 
                            {
                                 PBControlTreeItemBase PbBase;
                                 PbBase = PBControlTreeItemBase.GetMatchingPBTreeItem(mSpyElement);
                                double XOffset =
                                    double.Parse(((UIAutomationDriverBase) mWindowExplorerDriver).mUIAutomationHelper
                                        .GetControlPropertyValue(mSpyElement.ElementObject, "XOffset"));
                                double YOffset =
                                    double.Parse(((UIAutomationDriverBase)mWindowExplorerDriver).mUIAutomationHelper
                                        .GetControlPropertyValue(mSpyElement.ElementObject, "YOffset"));
                                PbBase.UIAElementInfo.XCordinate = XOffset - rootEI.XCordinate;
                                 PbBase.UIAElementInfo.YCordinate = YOffset - rootEI.YCordinate;
                                 WindowControlsTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)((TreeViewItem)mTreeRootItem.Items[0]).Tag, PbBase);
                            }
                            else
                            {
                                  ITreeViewItem TVI;
                                  TVI = WindowsElementConverter.GetWindowsElementTreeItem(UEI);
                                  WindowControlsTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)((TreeViewItem)mTreeRootItem.Items[0]).Tag, TVI);
                            }                           
                        }
                        else if(mSpyElement is JavaElementInfo)
                        {
                            //TODO: Fix me to add if spy element is not present on tree.
                            Reporter.ToUser(eUserMsgKey.FailedToAddTreeItem, "", "Adding spy element dynamically is not yet supported for this driver");
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.FailedToAddTreeItem, "Adding spy element dynamically is not yet supported  for this driver");
                        }
                    }
                }
            }
        }
        
        public TreeViewItem FindMatchingTreeItemByElementXPath(TreeViewItem root, string searchItemXPath, string searchItemPath)
        {
            if (root == null) return null;
            ElementInfo currentItem = null;

            foreach (TreeViewItem TVI in root.Items)
            {
                ITreeViewItem o = (ITreeViewItem)TVI.Tag;
                if (o != null)
                {
                    currentItem = (ElementInfo)o.NodeObject();
                    currentItem.WindowExplorer = mWindowExplorerDriver;

                    if (currentItem.XPath == searchItemXPath && currentItem.Path == searchItemPath)
                    {
                        return TVI;
                    }
                }
                if (TVI.Items.Count > 0)
                {
                    TVI.IsExpanded = true;
                    TreeViewItem vv = FindMatchingTreeItemByElementXPath(TVI, searchItemXPath, searchItemPath);
                    if (vv != null) return vv;
                }
            }
            return null;
        }
             
        private void WindowControlsTreeView_ItemSelected(object sender, EventArgs e)
        {
            TreeViewItem TVI = (TreeViewItem)sender;
            if (TVI != null)
            {
                ITreeViewItem iv = (ITreeViewItem)TVI.Tag;
                mCurrentControlTreeViewItem = iv;
                                                
                ShowCurrentControlInfo();

            }
        }
        private void WindowControlsTreeView_SearchStarted(object sender, EventArgs e)
        {
            StatusTextBlock.Text = "Searching...";
        }

        private void WindowControlsTreeView_SearchCancelled(object sender, EventArgs e)
        {
            StatusTextBlock.Text = "Ready";
        }

        private void WindowControlsTreeView_SearchCompleted(object sender, EventArgs e)
        {
            StatusTextBlock.Text = "Ready";
        }

        private void ShowCurrentControlInfo()
        {
            if (mCurrentControlTreeViewItem == null) return;
            ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
            try
            {
                if (mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
                {
                    EI.WindowExplorer = mWindowExplorerDriver;
                    mWindowExplorerDriver.HighLightElement(EI);
                    
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
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);                
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
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
                        if (mPlatform.PlatformType().Equals(ePlatformType.Web) || (mPlatform.PlatformType().Equals(ePlatformType.Java) && !EI.ElementType.Contains("JEditor")))
                        {
                            //TODO: J.G: Remove check for element type editor and handle it in generic way in all places
                            list = mPlatform.GetPlatformElementActions(EI);
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
                            CAP = new ControlActionsPage(mWindowExplorerDriver, EI, list, DataPage, actInputValuelist, mContext);
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

        //TODO: fix to be OO style
        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            AppWindow AW = (AppWindow)WindowsComboBox.SelectedItem;    
            if (AW == null) return;
            mWindowExplorerDriver.SwitchWindow(AW.Title);
            RecordingButton.IsEnabled = true;
           
            switch (AW.WindowType)
            {
                case AppWindow.eWindowType.Windows:
                    WindowsWindowTreeItem WWTI = new WindowsWindowTreeItem();
                        UIAElementInfo WEI = (UIAElementInfo)AW.RefObject;
                        WEI.WindowExplorer = mWindowExplorerDriver;
                        WWTI.UIAElementInfo = WEI;
                        WWTI.UIAElementInfo.ElementObject = WEI.ElementObject;

                    InitTree(WWTI);
                    break;
                case AppWindow.eWindowType.PowerBuilder:
                    PBWindowTreeItem WTI = new PBWindowTreeItem();
                    UIAElementInfo PBEI = (UIAElementInfo)AW.RefObject;
                    PBEI.WindowExplorer = mWindowExplorerDriver;
                    WTI.UIAElementInfo = PBEI;
                    InitTree(WTI);            
                    break;
                case AppWindow.eWindowType.ASCFForm:
                    ASCFFormTreeItem AFTI = new ASCFFormTreeItem();
                    AFTI.Name = AW.Title;
                    AFTI.Path = AW.Path;                    
                    InitTree(AFTI);            
                    break;
                case AppWindow.eWindowType.SeleniumWebPage:
                    HTMLPageTreeItem HPTI = new HTMLPageTreeItem();
                    HTMLElementInfo EI = new HTMLElementInfo();
                    EI.ElementTitle = AW.Title;
                    EI.XPath = "/html";
                    EI.WindowExplorer = mWindowExplorerDriver;
                    HPTI.ElementInfo = EI;                                                                
                    InitTree(HPTI);
                    break;
                case AppWindow.eWindowType.JFrmae:
                    JavaWindowTreeItem JWTI = new JavaWindowTreeItem();
                    JavaElementInfo JEI = new JavaElementInfo();
                    JEI.ElementTitle = AW.Title;
                    JEI.Path = AW.Title;
                    JEI.XPath = "/";
                    JEI.IsExpandable = true;
                    JWTI.JavaElementInfo = JEI;
                    JEI.WindowExplorer = mWindowExplorerDriver;
                    InitTree(JWTI);
                    break;
                case AppWindow.eWindowType.Appium:
                    AppiumWindowTreeItem AWTI = new AppiumWindowTreeItem();

                    AppiumElementInfo AEI = new AppiumElementInfo();
                    AEI.WindowExplorer = mWindowExplorerDriver;
                    AEI.XPath = "/";
                    SeleniumAppiumDriver SAD = ((SeleniumAppiumDriver)mWindowExplorerDriver);


                    string pageSourceString = SAD.GetPageSource().Result;
                    XmlDocument pageSourceXml = new XmlDocument();
                    pageSourceXml.LoadXml(pageSourceString);
                    AEI.XmlDoc = pageSourceXml;
                    AEI.XmlNode = pageSourceXml.SelectSingleNode("/");

                    AWTI.AppiumElementInfo = AEI;                    
                    
                    // AWTI.UIAElementInfo = AEI;
                    InitTree(AWTI);
                    break;
                //case AppWindow.eWindowType.AndroidDevice:
                //    AndroidWindowTreeItem ADTI = new AndroidWindowTreeItem();

                //    AndroidElementInfo AWI = new AndroidElementInfo();
                //    AWI.WindowExplorer = mWindowExplorerDriver;
                //    AWI.XPath = "/";
                //    string pageSourceString2 = ((AndroidADBDriver)mWindowExplorerDriver).GetPageSource();
                //    XmlDocument pageSourceXml2 = new XmlDocument();
                //    pageSourceXml2.LoadXml(pageSourceString2);
                //    AWI.XmlDoc = pageSourceXml2;
                //    AWI.XmlNode = pageSourceXml2.SelectSingleNode("/hierarchy");

                //    ADTI.AndroidElementInfo = AWI;
                //    InitTree(ADTI);
                //    break;
                case AppWindow.eWindowType.Mainframe:
                    MainframeTreeItemBase MFTI = new MainframeTreeItemBase ();
                    MFTI.Name = AW.Title;
                    MFTI.Path = AW.Path;
                    InitTree (MFTI);
                    break;
                default:                    
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unknown Window type:" + AW.WindowType);
                    break;
            }            
      
              if (WindowControlsGridView.Visibility == System.Windows.Visibility.Visible)
              {
                  RefreshControlsGrid();
              }
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

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowsList();
        }

        private void StartRecording()
        {
            SetPageFunctionalityEnableDisable(false,false,false,false,false,false,false,false,false,false,false);

            mWindowExplorerDriver.SwitchWindow(((AppWindow)WindowsComboBox.SelectedValue).Title);

            ((DriverBase)mWindowExplorerDriver).StartRecording();

            SetPageFunctionalityEnableDisable(true, false, false, false, false, false, false, false, false, false, false);
        }
        
        private void StopRecording()
        {
            SetPageFunctionalityEnableDisable(false, false, false, false, false, false, false, false, false, false, false);

            ((DriverBase)mWindowExplorerDriver).StopRecording();

            SetPageFunctionalityEnableDisable(true, true, true, true, true, true, true, true, true, true, true);
        }
        
        private void SetControlsGridView()
        {            
            //Set the Tool Bar look
            WindowControlsGridView.ShowAdd = Visibility.Collapsed;
            WindowControlsGridView.ShowClearAll = Visibility.Collapsed;
            WindowControlsGridView.ShowUpDown = Visibility.Collapsed;
            WindowControlsGridView.ShowRefresh = Visibility.Collapsed;

            
             WindowControlsGridView.AddToolbarTool("@Filter16x16.png", "Filter Elements to show", new RoutedEventHandler(FilterElementButtonClicked));
            //TODO: enable refresh to do refresh

            WindowControlsGridView.ShowEdit = System.Windows.Visibility.Collapsed;
            WindowControlsGridView.ShowDelete = System.Windows.Visibility.Collapsed;            

            //TODO: add button to show all...        

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60 });            
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100 });            
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150 });            
            
            WindowControlsGridView.SetAllColumnsDefaultView(view);
            WindowControlsGridView.InitViewItems();
        }

        private async void RefreshControlsGrid()
        {
            if (WindowsComboBox.SelectedValue != null && mWindowExplorerDriver != null)
            {
                List<ElementInfo> list = await Task.Run(() => mWindowExplorerDriver.GetVisibleControls(CheckedFilteringCreteriaList.Select(x => x.ElementType).ToList()));

                StatusTextBlock.Text = "Ready";
                // Convert to obserable for the grid
                VisibleElementsInfoList.Clear();
                foreach (ElementInfo EI in list)
                {
                    VisibleElementsInfoList.Add(EI);
                }

                WindowControlsGridView.DataSourceList = VisibleElementsInfoList;

            }
        }

        private void ControlsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            ElementInfo EI = (ElementInfo)WindowControlsGridView.CurrentItem;
            mCurrentControlTreeViewItem = GetTreeViewItemForElementInfo(EI);            
            ShowCurrentControlInfo();                    
        }

        private ITreeViewItem GetTreeViewItemForElementInfo(Amdocs.Ginger.Common.UIElement.ElementInfo EI)
        {            
            if (EI == null) return null; // can happen when grid is filtered
            
            //TODO: make it OO style avoid the if else if
            ITreeViewItem TVI = null;
            if (EI is JavaElementInfo)
            {
                TVI = JavaElementInfoConverter.GetTreeViewItemFor(EI);
            }
            else if (EI is UIAElementInfo) 
            {
                UIAElementInfo UEI = (UIAElementInfo)EI;
                if (UEI.WindowExplorer.GetType() == typeof(PBDriver))
                { 
                    //TODO:  Below will work for now. But need to Implement element info
                    TVI = PBControlTreeItemBase.GetMatchingPBTreeItem(UEI);
                }
                else
                {
                    TVI = WindowsElementConverter.GetWindowsElementTreeItem(EI);
                }
            }            
            else if (EI is AppiumElementInfo)
            {
                TVI = AppiumElementInfoConverter.GetTreeViewItemFor(EI);
            }
            else if (EI is HTMLElementInfo)
            {
                TVI = HTMLElementInfoConverter.GetHTMLElementTreeItem(((HTMLElementInfo)EI));
            }
            else
            {
                //TODO: err?
               return null;
            }

            return TVI;
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            //stop Live Spy
            if (LiveSpyButton.IsChecked == true)
                if (dispatcherTimer != null)
                    dispatcherTimer.IsEnabled = false;

            if (WindowsComboBox.SelectedValue!=null)
            {
                if (RecordingButton.IsChecked == true)
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Record_24x24.png"));
                    RecordingButton.Content = image;

                    RecordingButton.ToolTip = "Stop Recording";
                   
                    //collapse expanders & stop LiveSpy
                    ControlsViewsExpander.IsExpanded = false;
                    SelectedControlDetailsExpander.IsExpanded = false;
                    LiveSpyButton.IsChecked = false;

                    StartRecording();                    
                }
                else
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordOff_24x24.png"));
                    RecordingButton.Content = image;
                    RecordingButton.ToolTip = "Start Recording";                    
                    StopRecording();                    
                }
            }
            else
            {
                RecordingButton.IsChecked = false;
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);                
            }
        }

        private void GridTreeViewButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            if (((Button)sender).ToolTip.ToString().Contains("Tree") == true)
            {
                //switch to tree view
                WindowControlsTreeView.Visibility = System.Windows.Visibility.Visible;
                WindowControlsGridView.Visibility = System.Windows.Visibility.Collapsed;

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Grid View";
            }
            else
            {
                //switch to grid view
                WindowControlsTreeView.Visibility = System.Windows.Visibility.Collapsed;
                WindowControlsGridView.Visibility = System.Windows.Visibility.Visible;
                if (WindowControlsGridView.DataSourceList == null || WindowControlsGridView.DataSourceList.Count == 0)
                        ShowFilterElementsPage();

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Tree View";
            }           
        }

        private void SyncWithLiveSpyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            if (((Button)sender).ToolTip.ToString().Contains("Cancel") == true)
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyWhite_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
                SyncWithLiveSpyButton.ToolTip = "Click to Locate Live Spy Found Element";

                mSyncControlsViewWithLiveSpy = false;
            }
            else
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithSpyWhite_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
                SyncWithLiveSpyButton.ToolTip = "Click to Cancel Locate Live Spy Found Element";

                mSyncControlsViewWithLiveSpy = true;
                FocusSpyItemOnControlTree();//try to locate last find element by live spy
            }
        }

        private bool RefreshFilteredElements()
        {
            if (FilteringCreteriaList.Count != 0)
            {
                if (CheckedFilteringCreteriaList.Count == 0)
                {
                    Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.FilterNotBeenSet);
                    if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                    {
                        RefreshControlsGrid();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    RefreshControlsGrid();
                    return true;
                }
            }
            else
            {
                Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.RetreivingAllElements);
                if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                {
                    RefreshControlsGrid();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void ControlsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Loading Elements...";
            GingerCore.General.DoEvents();

            if (WindowControlsTreeView.Visibility == System.Windows.Visibility.Visible)
                RefreshTreeControls();
            else
                RefreshFilteredElements();            
            StatusTextBlock.Text = "Ready";
        }
               
        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowsComboBox.SelectedValue != null)
            {
                if (mWindowExplorerDriver.AddSwitchWindowAction(((AppWindow)WindowsComboBox.SelectedValue).Title) == false)
                    StatusTextBlock.Text = "Not implemented yet or not needed for this driver type";
            }
        }
        
        private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(200, GridUnitType.Star);
            ControlsViewRow.MaxHeight = Double.PositiveInfinity;
            if (Row2Splitter != null)
                Row2Splitter.IsEnabled = true;
        }

        private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(35);
            ControlsViewRow.MaxHeight = 35;
            if (Row2Splitter != null)
                Row2Splitter.IsEnabled = false;
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

        private void SelectedControlDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlDetailsRow.Height = new GridLength(35);
            ControlDetailsRow.MaxHeight = 35;
        }

        private void SetDetailsExpanderDesign(bool detailsExist, ElementInfo selectedElementInfo)
        {
            if (detailsExist == false)
            {
                SelectedControlDetailsExpanderLable.Content = "Selected Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = Brushes.Gray;
                SelectedControlDetailsExpander.IsEnabled = false;
                SelectedControlDetailsExpander.IsExpanded = false;                              
            }
            else
            {
                SelectedControlDetailsExpanderLable.Content = "'" + selectedElementInfo.ElementTitle + "' Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString()); ;
                SelectedControlDetailsExpander.IsEnabled = true;
                if (mFirstElementSelectionDone == false)
                {
                    SelectedControlDetailsExpander.IsExpanded = true;
                    mFirstElementSelectionDone = true;
                }
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

        ObservableList<UIElementFilter> CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();
        ObservableList<UIElementFilter> FilteringCreteriaList = new ObservableList<UIElementFilter>();

        private void FilterElementButtonClicked(object sender, RoutedEventArgs e)
        {
            ShowFilterElementsPage();
        }

        public bool DoSearchControls()
        {
            bool isSearched = false;
            foreach (UIElementFilter filter in FilteringCreteriaList)
                if (filter.Selected)
                {
                    if (!CheckedFilteringCreteriaList.Contains(filter))
                        CheckedFilteringCreteriaList.Add(filter);
                }

            StatusTextBlock.Text = "Searching Elements...";
            GingerCore.General.DoEvents();
            
            isSearched = RefreshFilteredElements();

            

            return isSearched;
        }

        private void SetAutoMapElementTypes()
        {
            List<eElementType> UIElementsTypeList = null;
            switch (((Agent)mApplicationAgent.Agent).Platform)
            {
                case ePlatformType.Web:
                    WebPlatform webPlatformInfo = new WebPlatform();
                    UIElementsTypeList = webPlatformInfo.GetPlatformUIElementsType();
                    break;
            }
            if (UIElementsTypeList != null)
            {
                foreach (eElementType eET in UIElementsTypeList)
                {
                    FilteringCreteriaList.Add(new UIElementFilter(eET, string.Empty));
                }
            }
        }

        private void ShowFilterElementsPage()
        {
            if (FilteringCreteriaList.Count == 0)
                SetAutoMapElementTypes();
            if (FilteringCreteriaList.Count != 0)
            {
                CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();
                FilterElementsPage FEW = new FilterElementsPage(FilteringCreteriaList, CheckedFilteringCreteriaList, /*ControlsSearchButton_Click,*/ this);
                FEW.ShowAsWindow(eWindowShowStyle.Dialog);

                foreach (UIElementFilter filter in FilteringCreteriaList)
                    if (filter.Selected)
                    {
                        if (!CheckedFilteringCreteriaList.Contains(filter))
                             CheckedFilteringCreteriaList.Add(filter);
                    }
            }
        }

       

        bool POMOn = false;

        private void POMButton_Click(object sender, RoutedEventArgs e)
        {
            POMOn = !POMOn;
            ShowPOMOrControlsViewsExpander();
        }

        private void ShowPOMOrControlsViewsExpander()
        {
            if (POMOn)
            {
                ControlsViewsExpander.Visibility = Visibility.Collapsed;
                POMFrame.Visibility = Visibility.Visible;
                ControlDetailsRow.Height = new GridLength(200, GridUnitType.Star);
                ControlDetailsRow.MaxHeight = Double.PositiveInfinity;
            }
            else
            {
                ControlsViewsExpander.Visibility = Visibility.Visible;
                POMFrame.Visibility = Visibility.Collapsed;
            }
        }
    }
}
