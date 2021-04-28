#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using System.Drawing;
using Ginger.Actions.UserControls;
using GingerCore.Actions.VisualTesting;
using HtmlAgilityPack;
using Ginger.ApplicationModelsLib.POMModels;
using GingerCoreNET.Application_Models;

namespace Ginger.WindowExplorer
{
    public class PageSrcParser
    {
        //public PageSrcParser(TreeView tView, HtmlAgilityPack.HtmlDocument htmlDoc)
        //{
        //    htmlDoc.LoadHtml(PageSrcTxt);

            
        //}



        //System.Windows.Forms.TreeView tView;
        //public System.Windows.Forms.TreeView GetSrcTree(System.Windows.Forms.HtmlDocument htmlDoc)
        //{
        //    tView.Nodes.Add(LoadTreeNodes(htmlDoc.Body));

        //    return tView;
        //}

        //public TreeNode LoadTreeNodes(HtmlElement htmlElement)
        //{
        //    TreeNode tNode = new TreeNode(htmlElement.TagName);
        //    for(int i = 0; i < htmlElement.Children.Count; i++)
        //    {
        //        tNode.Nodes.Add(LoadTreeNodes(htmlElement.Children[i]));
        //    }

        //    return tNode;
        //}
    }

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
        public PlatformInfoBase Platform
        {
            get
            {
                return mPlatform;
            }
        }

        //If we come from ActionEditPage keep the Action
        private Act mAction;
        ObservableList<ElementInfo> VisibleElementsInfoList = new ObservableList<ElementInfo>();
        bool mSyncControlsViewWithLiveSpy = false;
        bool mFirstElementSelectionDone = false;
        //Page mControlFrameContentPage = null;
        //WindowExplorerPOMPage mWindowExplorerPOMPage;
        //PomAllElementsPage pomAllElementsPage = null;
        bool IsWebMobJavaPlatform = false;

        Context mContext;

        // We can open it from agents grid, or from Action Edit page with Action 
        // If we open from ActionEdit Page then we update the act with Locator
        public WindowExplorerPage(ApplicationAgent ApplicationAgent, Context context, Act Act = null)
        {
            InitializeComponent();
            xWindowControlsTreeView.TreeGrid.RowDefinitions[0].Height = new GridLength(0);

            mContext = context;
            mContext.PropertyChanged += MContext_PropertyChanged;
            mPlatform = PlatformInfoBase.GetPlatformImpl(((Agent)ApplicationAgent.Agent).Platform);

            IsWebMobJavaPlatform = (mPlatform.PlatformType() == ePlatformType.Web || mPlatform.PlatformType() == ePlatformType.Mobile || mPlatform.PlatformType() == ePlatformType.Java);

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

            /// Beta Feature based section - will look into this later
            //if (WorkSpace.Instance.BetaFeatures.ShowPOMInWindowExplorer)
            //{
            //    if (mWindowExplorerDriver is IPOM)
            //    {
            //        xWindowSelection.xIntegratePOMChkBox.Visibility = Visibility.Visible;
            //        xWindowSelection.POMsComboBox.Visibility = Visibility.Visible;

            //        mWindowExplorerPOMPage = new WindowExplorerPOMPage(mApplicationAgent);
            //        ((IPOM)mWindowExplorerDriver).ActionRecordedCallback(mWindowExplorerPOMPage.ActionRecorded);

            //        //POMFrameRow.Height = new GridLength(100, GridUnitType.Star);
            //        //POMFrame.Content = mWindowExplorerPOMPage;
            //    }
            //}
            //else
            //{
            //    xWindowSelection.xIntegratePOMChkBox.Visibility = Visibility.Collapsed;
            //    xWindowSelection.POMsComboBox.Visibility = Visibility.Collapsed;
            //    //POMFrameRow.Height = new GridLength(0);
            //    //POMButton.Visibility = Visibility.Collapsed;
            //}

            UpdateWindowsList();

            xWindowControlsTreeView.SearchStarted += WindowControlsTreeView_SearchStarted;
            xWindowControlsTreeView.SearchCancelled += WindowControlsTreeView_SearchCancelled;
            xWindowControlsTreeView.SearchCompleted += WindowControlsTreeView_SearchCompleted;
            xWindowControlsTreeView.TreeTitleStyle = (Style)TryFindResource("@NoTitle");
            xWindowControlsTreeView.Tree.ItemSelected += WindowControlsTreeView_ItemSelected;

            xWindowSelection.RefreshWindowsButton.Click += RefreshWindowsButton_Click;
            xWindowSelection.AddSwitchWindowActionButton.Click += AddSwitchWindowActionButton_Click;
            xWindowSelection.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;

            SetControlsGridView();

            SetPlatformBasedUIUpdates();

            InitControlPropertiesGridView();

            //xUCElementDetails.AppAgent = mApplicationAgent;
            xWindowSelection.context = context;
            xUCElementDetails.Context = context;
            xUCElementDetails.WindowExplorerDriver = mWindowExplorerDriver;
            xUCElementDetails.Platform = mPlatform;
            xUCElementDetails.xPropertiesGrid.btnRefresh.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(RefreshControlProperties));

            if (mWindowExplorerDriver is IPOM)
            {
                xUCElementDetails.xIntegratePOMChkBox.Visibility = Visibility.Visible;
                xUCElementDetails.xIntegratePOMChkBox.IsChecked = true;

                //mWindowExplorerPOMPage = new WindowExplorerPOMPage(mApplicationAgent);
                //((IPOM)mWindowExplorerDriver).ActionRecordedCallback(mWindowExplorerPOMPage.ActionRecorded);
                xUCElementDetails.InitLocatorsGridView();
            }
            else
            {
                xUCElementDetails.xIntegratePOMChkBox.Visibility = Visibility.Collapsed;
                //xWindowSelection.POMsComboBox.Visibility = Visibility.Collapsed;
                xUCElementDetails.InitLegacyLocatorsGridView();
            }

            RefreshTabsContent();

            xUCElementDetails.PropertyChanged += XUCElementDetails_PropertyChanged;
        }

        bool ElementDetailsNotNullHandled = false;
        bool ElementDetailsNullHandled = false;
        private void XUCElementDetails_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HandleUCElementDetails();
        }

        public void HandleUCElementDetails()
        {
            if (xUCElementDetails.SelectedElement == null && !ElementDetailsNullHandled)
            {
                xElementDetailGridRow.Height = new GridLength(0);
                xElementDetailGridRow.MinHeight = 0;
                xElementDetailGridColumn.Width = new GridLength(0);

                xRowSplitter.Visibility = Visibility.Collapsed;
                xColumnSplitter.Visibility = Visibility.Collapsed;

                ElementDetailsNullHandled = true;
                ElementDetailsNotNullHandled = false;
            }
            else if (!ElementDetailsNotNullHandled)
            {
                xElementDetailGridRow.Height = new GridLength(200, GridUnitType.Star);
                xElementDetailGridRow.MinHeight = 200;
                xElementDetailGridColumn.Width = new GridLength(91, GridUnitType.Star);

                if (ActualWidth > 700)
                {
                    xRowSplitter.Visibility = Visibility.Collapsed;
                    xColumnSplitter.Visibility = Visibility.Visible;
                }
                else
                {
                    xRowSplitter.Visibility = Visibility.Visible;
                    xColumnSplitter.Visibility = Visibility.Collapsed;
                }

                ElementDetailsNotNullHandled = true;
                ElementDetailsNullHandled = false;
            }
        }

        private void SetPlatformBasedUIUpdates()
        {
            if (mPlatform.PlatformType() == ePlatformType.Web || mPlatform.PlatformType() == ePlatformType.Mobile)
            {
                xPageSrcTab.Visibility = Visibility.Visible;
            }
            else
            {
                xPageSrcTab.Visibility = Visibility.Collapsed;
            }
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {
                if (e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
                {

                }
            }
        }

        /// <summary>
        /// This method will set the explorer page to be fit in new right panel
        /// </summary>
        /// <param name="windowExplorerDriver"></param>
        public void SetDriver(IWindowExplorer windowExplorerDriver)
        {
            this.Dispatcher.Invoke(() =>
            {
                //RecordingButton.Visibility = Visibility.Collapsed;
                mWindowExplorerDriver = windowExplorerDriver;
                UpdateWindowsList();
            });
        }

        private void RefreshControlProperties(object sender, RoutedEventArgs e)
        {
            //TODO: fix me for cached properties like ASCFBrowserElements, it will not work
            if (mCurrentControlTreeViewItem != null)
            {
                ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
                xUCElementDetails.SelectedElement = EI;
            }
        }

        private void UpdateWindowsList()
        {
            try
            {
                if (mWindowExplorerDriver != null)
                {
                    List<AppWindow> list = mWindowExplorerDriver.GetAppWindows();
                    xWindowSelection.WindowsComboBox.ItemsSource = list;
                    xWindowSelection.WindowsComboBox.DisplayMemberPath = "WinInfo";

                    AppWindow ActiveWindow = mWindowExplorerDriver.GetActiveWindow();

                    if (ActiveWindow != null)
                    {
                        foreach (AppWindow w in list)
                        {
                            if (w.Title == ActiveWindow.Title && w.Path == ActiveWindow.Path)
                            {
                                //xWindowSelection.WindowsComboBox.SelectedValue = w;
                                xWindowSelection.WindowsComboBox.SelectedItem = w;

                                mWindowExplorerDriver.SwitchWindow(w.Title);

                                return;
                            }
                        }
                    }

                    //TODO: If no selection then select the first if only one window exist in list
                    if (mWindowExplorerDriver is GenericAppiumDriver)//FIXME: need to work for all drivers and from some reason failing for Appium!!
                    {
                        if (xWindowSelection.WindowsComboBox.Items.Count == 1)
                        {
                            xWindowSelection.WindowsComboBox.SelectedItem = xWindowSelection.WindowsComboBox.Items[0];
                            mWindowExplorerDriver.SwitchWindow((xWindowSelection.WindowsComboBox.SelectedItem as AppWindow).Title);
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
                //ActionsTab.Header = "Action Locator Selection";
                //SelectedControlDetailsExpander.IsExpanded = true;
                //ActionsTab.IsSelected = true;
            }
            else
            {
                //StepstoUpdateActionRow.Height = new GridLength(0);
            }
            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this, closeEventHandler: CloseWindow);
        }

        private async void CloseWindow(object sender, EventArgs e)
        {
            //stop Live Spy
            //if (LiveSpyButton.IsChecked == true)
            //    if (dispatcherTimer != null)
            //        dispatcherTimer.IsEnabled = false;

            //stop Recording
            //if (RecordingButton.IsChecked == true)
            //    StopRecording();

            if (xWindowControlsTreeView.IsSearchRunning())
            {
                await xWindowControlsTreeView.CancelSearchAsync();
            }

            _GenWin.Close();
        }

        void InitTree(ITreeViewItem RootItem)
        {
            xWindowControlsTreeView.Tree.ClearTreeItems();
            mRootItem = RootItem;

            mTreeRootItem = xWindowControlsTreeView.Tree.AddItem(RootItem);
            mTreeRootItem.IsExpanded = false;
        }

        private void RefreshPageSrcContent()
        {
            //xHTMLTree.Items.Add(new PageSrcParser(((SeleniumDriver)mContext.Agent.Driver).GetPageHTML())
        }

        private async Task RefreshTreeContent()
        {
            xLoadingTreeViewBanner.Visibility = Visibility.Visible;
            xWindowControlsTreeView.Visibility = Visibility.Collapsed;

            GingerCore.General.DoEvents();

            AppWindow AW = (AppWindow)xWindowSelection.WindowsComboBox.SelectedItem;
            if (AW == null) return;
            mWindowExplorerDriver.SwitchWindow(AW.Title);

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
                    //ElementInfo AEI = new ElementInfo();
                    AEI.WindowExplorer = mWindowExplorerDriver;
                    AEI.XPath = "/";
                    GenericAppiumDriver GAD = ((GenericAppiumDriver)mWindowExplorerDriver);

                    string pageSourceString = await GAD.GetPageSource();
                    XmlDocument pageSourceXml = new XmlDocument();
                    pageSourceXml.LoadXml(pageSourceString);
                    AEI.XmlDoc = pageSourceXml;
                    AEI.ElementObject = pageSourceXml.SelectSingleNode("/");

                    AWTI.ElementInfo = AEI;

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
                    MainframeTreeItemBase MFTI = new MainframeTreeItemBase();
                    MFTI.Name = AW.Title;
                    MFTI.Path = AW.Path;
                    InitTree(MFTI);
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unknown Window type:" + AW.WindowType);
                    break;
            }

            xLoadingTreeViewBanner.Visibility = Visibility.Collapsed;
            xWindowControlsTreeView.Visibility = Visibility.Visible;
        }

        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            //stop Recording
            //if (RecordingButton.IsChecked == true)
            //    StopRecording();

            if (xWindowSelection.WindowsComboBox.SelectedValue != null)
            {
                //if (LiveSpyButton.IsChecked == true)
                //{
                //    if (mSyncControlsViewWithLiveSpy == false)
                //        ControlsViewsExpander.IsExpanded = false;

                //    SetPageFunctionalityEnableDisable(false,true,false,false,false,true,true,true,true,true,false);

                //    if (dispatcherTimer == null)
                //    {
                //        dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                //        dispatcherTimer.Tick += new EventHandler(timenow);
                //        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                //    }

                //    dispatcherTimer.IsEnabled = true;
                //}
                //else
                //{
                dispatcherTimer.IsEnabled = false;
                SetPageFunctionalityEnableDisable(true, true, true, true, true, true, true, true, true, true, true);
                //}               
            }
            else
            {
                //LiveSpyButton.IsChecked = false;
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
            }
        }

        private void SetPageFunctionalityEnableDisable(bool RecordingButtonFlag, bool ControlsRefreshButtonFlag, bool RefreshWindowsButtonFlag, bool AddSwitchWindowActionButtonFlag, bool WindowsComboBoxFlag, bool ControlsViewsExpanderFlag, bool SelectedControlDetailsExpanderFlag, bool LiveSpyButtonFlag, bool SyncWithLiveSpyButtonFlag, bool GridTreeViewButtonFlag, bool DORButtonFlag)
        {
            //RecordingButton.IsEnabled = RecordingButtonFlag;
            //if (RecordingButtonFlag)
            //{
            //    if (RecordingButton.IsChecked == true)
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Record_24x24.png"));
            //        RecordingButton.Content = image;
            //        RecordingButton.ToolTip = "Stop Recording";
            //    }
            //    else
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordOff_24x24.png"));
            //        RecordingButton.Content = image;
            //        RecordingButton.ToolTip = "Start Recording";
            //    }
            //}
            //else
            //{
            //System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordDisable_24x24.png"));
            //RecordingButton.Content = image;
            //}

            //ControlsRefreshButton.IsEnabled = ControlsRefreshButtonFlag;

            //if (ControlsRefreshButtonFlag)
            //    ((ImageMakerControl)(ControlsRefreshButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_White");
            //else
            //    ((ImageMakerControl)(ControlsRefreshButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_Gray");

            xWindowSelection.RefreshWindowsButton.IsEnabled = RefreshWindowsButtonFlag;

            //if (RefreshWindowsButtonFlag)
            //    ((ImageMakerControl)(RefreshWindowsButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");                            
            //else
            //    ((ImageMakerControl)(RefreshWindowsButton.Content)).ImageForeground = (SolidColorBrush)FindResource("$BackgroundColor_Gray");


            xWindowSelection.AddSwitchWindowActionButton.IsEnabled = AddSwitchWindowActionButtonFlag;
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

            xWindowSelection.WindowsComboBox.IsEnabled = WindowsComboBoxFlag;
            //if (WindowsComboBoxFlag)
            //{
            //    WindowsComboBox.Foreground = Brushes.Orange;
            //}
            //else
            //{
            //    WindowsComboBox.Foreground = Brushes.Gray;
            //}

            //ControlsViewsExpander.IsEnabled = ControlsViewsExpanderFlag;
            //if (ControlsViewsExpanderFlag)
            //    ControlsViewsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString());
            //else
            //    ControlsViewsExpanderLable.Foreground = Brushes.Gray;


            //SelectedControlDetailsExpander.IsEnabled = SelectedControlDetailsExpanderFlag;
            //if (SelectedControlDetailsExpanderFlag)
            //    SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString());
            //else
            //    SelectedControlDetailsExpanderLable.Foreground = System.Windows.Media.Brushes.Gray;

            //LiveSpyButton.IsEnabled = LiveSpyButtonFlag;
            //if (LiveSpyButtonFlag)
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Spy_24x24.png"));
            //    LiveSpyButton.Content = image;
            //}
            //else
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Spy_Gray_24x24.png"));
            //    LiveSpyButton.Content = image;
            //}

            //SyncWithLiveSpyButton.IsEnabled = SyncWithLiveSpyButtonFlag;
            //if (SyncWithLiveSpyButtonFlag)
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyWhite_24x24.png"));
            //    SyncWithLiveSpyButton.Content = image;
            //}
            //else
            //{
            //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyGray_24x24.png"));
            //    SyncWithLiveSpyButton.Content = image;
            //}

            //GridTreeViewButton.IsEnabled = GridTreeViewButtonFlag;
            //if (GridTreeViewButton.ToolTip.ToString().Contains("Tree") == true)
            //{
            //    if (GridTreeViewButtonFlag)
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
            //        GridTreeViewButton.Content = image;
            //    }
            //    else
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_Gray_24x24.png"));
            //        GridTreeViewButton.Content = image;
            //    }
            //}
            //else
            //{
            //    if (GridTreeViewButtonFlag)
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
            //        GridTreeViewButton.Content = image;
            //    }
            //    else
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_Gray_24x24.png"));
            //        GridTreeViewButton.Content = image;
            //    }

            //}

            //POMButton.IsEnabled = DORButtonFlag;
            //if (DORButtonFlag)
            //{
            //    //TODO: change to gray icon
            //}
            //else
            //{
            //}
        }

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //StatusTextBlock.Text = "Spying Element, Please Wait...";
                GingerCore.General.DoEvents();
                mSpyElement = mWindowExplorerDriver.GetControlFromMousePosition();
                if (mSpyElement != null)
                {
                    mWindowExplorerDriver.LearnElementInfoDetails(mSpyElement);
                    //StatusTextBlock.Text = mSpyElement.XPath;
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
                    //StatusTextBlock.Text = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void FocusSpyItemOnControlTree()
        {
            //TODO: run the search on background worker so will work fast without user impact 
            if (mSpyElement == null) return;

            //StatusTextBlock.Text = mSpyElement.XPath;
            if (xWindowControlsGridView.Visibility == System.Windows.Visibility.Visible)
            {
                foreach (ElementInfo EI in VisibleElementsInfoList)
                {
                    if (EI.XPath == mSpyElement.XPath && EI.Path == mSpyElement.Path)
                    {
                        VisibleElementsInfoList.CurrentItem = EI;
                        xWindowControlsGridView.ScrollToViewCurrentItem();
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
                    xWindowControlsTreeView.Tree.SelectItem(spyItem);
                }
                else
                {
                    //TODO:If item not found in a tree and user confirms add it to control tree                        
                    if ((Reporter.ToUser(eUserMsgKey.ConfirmToAddTreeItem)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        //TODO: Need to move this to IWindowExplorer and each driver will implement this and return matching ITreeViewItem for Element.
                        if (mSpyElement is UIAElementInfo)
                        {
                            UIAElementInfo UEI = (UIAElementInfo)mSpyElement;
                            UIAElementInfo rootEI = ((UIAElementInfo)mRootItem.NodeObject());

                            if (UEI.WindowExplorer.GetType() == typeof(PBDriver))
                            {
                                PBControlTreeItemBase PbBase;
                                PbBase = PBControlTreeItemBase.GetMatchingPBTreeItem(mSpyElement);
                                double XOffset =
                                    double.Parse(((UIAutomationDriverBase)mWindowExplorerDriver).mUIAutomationHelper
                                        .GetControlPropertyValue(mSpyElement.ElementObject, "XOffset"));
                                double YOffset =
                                    double.Parse(((UIAutomationDriverBase)mWindowExplorerDriver).mUIAutomationHelper
                                        .GetControlPropertyValue(mSpyElement.ElementObject, "YOffset"));
                                PbBase.UIAElementInfo.XCordinate = XOffset - rootEI.XCordinate;
                                PbBase.UIAElementInfo.YCordinate = YOffset - rootEI.YCordinate;
                                xWindowControlsTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)((TreeViewItem)mTreeRootItem.Items[0]).Tag, PbBase);
                            }
                            else
                            {
                                ITreeViewItem TVI;
                                TVI = WindowsElementConverter.GetWindowsElementTreeItem(UEI);
                                xWindowControlsTreeView.Tree.AddChildItemAndSelect((ITreeViewItem)((TreeViewItem)mTreeRootItem.Items[0]).Tag, TVI);
                            }
                        }
                        else if (mSpyElement is JavaElementInfo)
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
            //StatusTextBlock.Text = "Searching...";
        }

        private void WindowControlsTreeView_SearchCancelled(object sender, EventArgs e)
        {
            //StatusTextBlock.Text = "Ready";
        }

        private void WindowControlsTreeView_SearchCompleted(object sender, EventArgs e)
        {
            //StatusTextBlock.Text = "Ready";
        }
        bool POMBasedAction = false;
        ElementInfo POMElement = null;

        private void ShowCurrentControlInfo()
        {
            if (mCurrentControlTreeViewItem == null)
            {
                return;
            }

            ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();

            try
            {
                if (mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
                {
                    EI.WindowExplorer = mWindowExplorerDriver;

                    mWindowExplorerDriver.HighLightElement(EI);

                    //General tab will show the generic element info page, customized page will be in Data tab
                    //mControlFrameContentPage = new ElementInfoPage(EI);
                    //ControlFrame.Content = mControlFrameContentPage;
                    SetDetailsExpanderDesign(true, EI);
                    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
                    {
                        EI.GetElementProperties();
                        xUCElementDetails.SelectedElement = EI;
                    }
                    else
                    {
                        xUCElementDetails.xPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    /*
                    if(xWindowSelection.xIntegratePOMChkBox.IsChecked == true && xWindowSelection.SelectedPOM != null)
                    {
                        //pomAllElementsPage = new PomAllElementsPage(xWindowSelection.SelectedPOM, PomAllElementsPage.eAllElementsPageContext.POMEditPage);
                        ElementInfo matchingOriginalElement = (ElementInfo)mWindowExplorerDriver.GetMatchingElement(EI, xWindowSelection.SelectedPOM.GetUnifiedElementsList());

                        if (matchingOriginalElement == null)
                        {
                            mWindowExplorerDriver.LearnElementInfoDetails(EI);
                            matchingOriginalElement = (ElementInfo)mWindowExplorerDriver.GetMatchingElement(EI, xWindowSelection.SelectedPOM.GetUnifiedElementsList());
                        }

                        if (xWindowSelection.SelectedPOM.MappedUIElements.Contains(matchingOriginalElement) || xWindowSelection.SelectedPOM.UnMappedUIElements.Contains(matchingOriginalElement))
                        {
                            PomDeltaUtils pomDeltaUtils = new PomDeltaUtils(xWindowSelection.SelectedPOM, mContext.Agent);
                            pomDeltaUtils.KeepOriginalLocatorsOrderAndActivation = true;

                            /// Not Required but 
                            pomDeltaUtils.DeltaViewElements.Clear();

                            /// To Do - POM Delta Run and if Updated Element is found then ask user if they would like to replace existing POM Element with New ?
                            pomDeltaUtils.SetMatchingElementDeltaDetails(matchingOriginalElement, EI);

                            int originalItemIndex = -1;
                            if ((ApplicationPOMModel.eElementGroup)matchingOriginalElement.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                            {
                                originalItemIndex = xWindowSelection.SelectedPOM.MappedUIElements.IndexOf(matchingOriginalElement);
                            }

                            if (pomDeltaUtils.DeltaViewElements[0].DeltaStatus == eDeltaStatus.Changed)
                            {
                                //enter it to POM elements instead of existing one
                                if (Reporter.ToUser(eUserMsgKey.UpdateExistingPOMElement, matchingOriginalElement.ElementName) == eUserMsgSelection.Yes)
                                {
                                    /// Replace existing element with new one
                                    /// Element exists in Mapped Elements list
                                    if (originalItemIndex > -1)
                                    {
                                        xWindowSelection.SelectedPOM.MappedUIElements.RemoveAt(originalItemIndex);
                                        xWindowSelection.SelectedPOM.MappedUIElements.Insert(originalItemIndex, pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                    }
                                    /// Element exists in Un-Mapped Elements list
                                    /// We'll remove Element from Unmapped list and add it as new into Mapped Elements list
                                    else
                                    {
                                        xWindowSelection.SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                        xWindowSelection.SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);
                                    }

                                    POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                                }
                                else
                                {
                                    if(originalItemIndex == -1)
                                    {
                                        xWindowSelection.SelectedPOM.MappedUIElements.Add(pomDeltaUtils.DeltaViewElements[0].ElementInfo);
                                        xWindowSelection.SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);

                                        POMElement = pomDeltaUtils.DeltaViewElements[0].ElementInfo;
                                    }
                                    else
                                    {
                                        POMElement = matchingOriginalElement;
                                    }
                                }
                            }
                            else
                            {
                                /// Element exist in UnMapped Elements List
                                if (originalItemIndex == -1)
                                {
                                    //if (Reporter.ToUser(eUserMsgKey.POMMoveElementFromUnmappedToMapped, matchingOriginalElement.ElementName, xWindowSelection.SelectedPOM.Name) == eUserMsgSelection.Yes)
                                    //{
                                        xWindowSelection.SelectedPOM.MappedUIElements.Add(matchingOriginalElement);
                                        xWindowSelection.SelectedPOM.UnMappedUIElements.Remove(matchingOriginalElement);
                                    //}
                                }

                                POMElement = matchingOriginalElement;
                            }
                            POMBasedAction = true;
                        }
                        else
                        {
                            if (Reporter.ToUser(eUserMsgKey.POMElementNotExist, EI.ElementName, xWindowSelection.SelectedPOM.Name) == eUserMsgSelection.Yes)
                            {
                                POMBasedAction = true;
                                xWindowSelection.SelectedPOM.MappedUIElements.Add(EI);

                                POMElement = EI;
                                POMElement.ParentGuid = xWindowSelection.SelectedPOM.Guid;
                            }
                            else
                            {
                                POMElement = null;
                                POMBasedAction = false;
                            }
                        }
                    }
                    ShowControlActions(mCurrentControlTreeViewItem);
                    */
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

        private void ShowControlActions(ITreeViewItem iv)
        {
            //We show action if we didn't came from Edit page
            if (iv is IWindowExplorerTreeItem)
            {
                ElementInfo EI = (ElementInfo)iv.NodeObject();

                ControlActionsPage_New CAP = null;
                // We came from Action EditPage
                //if (mAction != null)
                //{
                //    CAP = new ControlActionsPage_New(mAction, EI);
                //}
                //else

                ObservableList<Act> list = new ObservableList<Act>();
                ObservableList<ActInputValue> actInputValuelist = new ObservableList<ActInputValue>();

                //var elmentPresentationinfo = mPlatform.GetElementPresentatnInfo(EI);//type A (ActionType to show) || type B

                //if(Type A)
                //        { }
                //else if (Type b)
                //        {
                //    list = ((IWindowExplorerTreeItem)iv).GetElementActions(); 
                //}

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

                    if (EI.Locators.CurrentItem == null)
                    {
                        EI.Locators.CurrentItem = EI.Locators[0];
                    }

                    ElementLocator eiLocator = EI.Locators.CurrentItem as ElementLocator;

                    string elementVal = string.Empty;
                    if (EI.OptionalValuesObjectsList.Count > 0)
                    {
                        elementVal = Convert.ToString(EI.OptionalValuesObjectsList.Where(v => v.IsDefault).FirstOrDefault().Value);
                    }

                    ElementActionCongifuration actConfigurations;
                    if (POMBasedAction)
                    {
                        //ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
                        //{
                        //    LocateBy = eLocateBy.POMElement,
                        //    LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                        //    ElementValue = elementVal,
                        //    AddPOMToAction = true,
                        //    POMGuid = elementInfo.ParentGuid.ToString(),
                        //    ElementGuid = elementInfo.Guid.ToString(),
                        //    LearnedElementInfo = elementInfo,
                        //};
                        //POMElement
                        actConfigurations = new ElementActionCongifuration
                        {
                            LocateBy = eLocateBy.POMElement,
                            LocateValue = POMElement.ParentGuid.ToString() + "_" + POMElement.Guid.ToString(),
                            ElementValue = elementVal,
                            AddPOMToAction = true,
                            POMGuid = POMElement.ParentGuid.ToString(),
                            ElementGuid = POMElement.Guid.ToString(),
                            LearnedElementInfo = POMElement,
                            Type = POMElement.ElementTypeEnum
                        };
                    }
                    else
                    {
                        //check if we have POM in context if yes set the Locate by and value to the specific POM if not so set it to the first active Locator in the list of Locators
                        actConfigurations = new ElementActionCongifuration
                        {
                            LocateBy = eiLocator.LocateBy,
                            LocateValue = eiLocator.LocateValue,
                            Type = EI.ElementTypeEnum,
                            ElementValue = elementVal,
                            ElementGuid = EI.Guid.ToString(),
                            LearnedElementInfo = EI
                        };
                    }

                    CAP = new ControlActionsPage_New(mWindowExplorerDriver, EI, list, DataPage, actInputValuelist, mContext, actConfigurations);
                }

                if (CAP == null)
                {
                    xUCElementDetails.xAddActionTab.Visibility = Visibility.Collapsed;
                    xUCElementDetails.xActUIPageFrame.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xUCElementDetails.xActUIPageFrame.Content = CAP;
                    xUCElementDetails.xAddActionTab.Visibility = Visibility.Visible;
                    xUCElementDetails.xActUIPageFrame.Visibility = Visibility.Visible;
                }
                //ControlActionsFrame.Content = CAP;
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
            if ((sender as ComboBox).SelectedItem != null)
                RefreshTabsContent();
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

            xUCElementDetails.xPropertiesGrid.SetAllColumnsDefaultView(view);
            xUCElementDetails.xPropertiesGrid.InitViewItems();
        }

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowsList();
        }

        private void StartRecording()
        {
            SetPageFunctionalityEnableDisable(false, false, false, false, false, false, false, false, false, false, false);

            mWindowExplorerDriver.SwitchWindow(((AppWindow)xWindowSelection.WindowsComboBox.SelectedValue).Title);

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
            xWindowControlsGridView.ShowAdd = Visibility.Collapsed;
            xWindowControlsGridView.ShowClearAll = Visibility.Collapsed;
            xWindowControlsGridView.ShowUpDown = Visibility.Collapsed;
            xWindowControlsGridView.ShowRefresh = Visibility.Collapsed;

            xWindowControlsGridView.AddToolbarTool("@Filter16x16.png", "Filter Elements to show", new RoutedEventHandler(FilterElementButtonClicked));
            //TODO: enable refresh to do refresh

            xWindowControlsGridView.ShowEdit = System.Windows.Visibility.Collapsed;
            xWindowControlsGridView.ShowDelete = System.Windows.Visibility.Collapsed;

            //TODO: add button to show all...        

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150, ReadOnly = true });

            xWindowControlsGridView.SetAllColumnsDefaultView(view);
            xWindowControlsGridView.InitViewItems();
        }

        private async void RefreshControlsGrid()
        {
            if (xWindowSelection.WindowsComboBox.SelectedValue != null && mWindowExplorerDriver != null)
            {
                try
                {
                    //StatusTextBlock.Text = "Loading";
                    List<ElementInfo> list = await mWindowExplorerDriver.GetVisibleControls(CheckedFilteringCreteriaList.Select(x => x.ElementType).ToList());

                    // Convert to obserable for the grid
                    VisibleElementsInfoList.Clear();
                    foreach (ElementInfo EI in list)
                    {
                        if (EI.WindowExplorer == null)
                            EI.WindowExplorer = mWindowExplorerDriver;

                        VisibleElementsInfoList.Add(EI);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception while loading the Grid data", ex);
                }
                finally
                {
                    //StatusTextBlock.Text = "Ready";
                }

                xWindowControlsGridView.DataSourceList = VisibleElementsInfoList;

            }
        }

        List<string> ListUIElementTypes = null;
        private void ControlsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            ElementInfo EI = (ElementInfo)xWindowControlsGridView.CurrentItem;

            if (EI != null)
            {
                mCurrentControlTreeViewItem = GetTreeViewItemForElementInfo(EI);

                xUCElementDetails.SelectedElement = EI;
            }

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
            //if (LiveSpyButton.IsChecked == true)
            //    if (dispatcherTimer != null)
            //        dispatcherTimer.IsEnabled = false;

            //if (xWindowSelection.WindowsComboBox.SelectedValue!=null)
            //{
            //    if (RecordingButton.IsChecked == true)
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Record_24x24.png"));
            //        RecordingButton.Content = image;

            //        RecordingButton.ToolTip = "Stop Recording";

            //        //collapse expanders & stop LiveSpy
            //        ControlsViewsExpander.IsExpanded = false;
            //        SelectedControlDetailsExpander.IsExpanded = false;
            //        LiveSpyButton.IsChecked = false;

            //        StartRecording();                    
            //    }
            //    else
            //    {
            //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@RecordOff_24x24.png"));
            //        RecordingButton.Content = image;
            //        RecordingButton.ToolTip = "Start Recording";                    
            //        StopRecording();                    
            //    }
            //}
            //else
            //{
            //    RecordingButton.IsChecked = false;
            //    Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);                
            //}
        }

        //private void GridTreeViewButton_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();

        //    if (((Button)sender).ToolTip.ToString().Contains("Tree") == true)
        //    {
        //        //switch to tree view
        //        WindowControlsTreeView.Visibility = System.Windows.Visibility.Visible;
        //        WindowControlsGridView.Visibility = System.Windows.Visibility.Collapsed;

        //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
        //        GridTreeViewButton.Content = image;
        //        GridTreeViewButton.ToolTip = "Switch to Grid View";
        //    }
        //    else
        //    {
        //        //switch to grid view
        //        WindowControlsTreeView.Visibility = System.Windows.Visibility.Collapsed;
        //        WindowControlsGridView.Visibility = System.Windows.Visibility.Visible;
        //        if (WindowControlsGridView.DataSourceList == null || WindowControlsGridView.DataSourceList.Count == 0)
        //            ShowFilterElementsPage();

        //        image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
        //        GridTreeViewButton.Content = image;
        //        GridTreeViewButton.ToolTip = "Switch to Tree View";
        //        RefreshControlsGrid();
        //    }
        //}

        private void SyncWithLiveSpyButton_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            //if (((Button)sender).ToolTip.ToString().Contains("Cancel") == true)
            //{
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyWhite_24x24.png"));
            //    SyncWithLiveSpyButton.Content = image;
            //    SyncWithLiveSpyButton.ToolTip = "Click to Locate Live Spy Found Element";

            //    mSyncControlsViewWithLiveSpy = false;
            //}
            //else
            //{
            //    image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithSpyWhite_24x24.png"));
            //    SyncWithLiveSpyButton.Content = image;
            //    SyncWithLiveSpyButton.ToolTip = "Click to Cancel Locate Live Spy Found Element";

            //    mSyncControlsViewWithLiveSpy = true;
            //    FocusSpyItemOnControlTree();//try to locate last find element by live spy
            //}
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

        //private void ControlsRefreshButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //StatusTextBlock.Text = "Loading Elements...";
        //    GingerCore.General.DoEvents();

        //    if (WindowControlsTreeView.Visibility == System.Windows.Visibility.Visible)
        //        RefreshTreeControls();
        //    else
        //        RefreshFilteredElements();
        //    //StatusTextBlock.Text = "Ready";
        //}

        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (xWindowSelection.WindowsComboBox.SelectedValue != null)
            {
                WindowExplorerCommon.CreateActUISwitchWindowAction(((AppWindow)xWindowSelection.WindowsComboBox.SelectedValue).Title, mContext);
            }
        }

        //private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    ControlsViewRow.Height = new GridLength(200, GridUnitType.Star);
        //    ControlsViewRow.MaxHeight = Double.PositiveInfinity;
        //    //if (Row2Splitter != null)
        //    //    Row2Splitter.IsEnabled = true;
        //}

        //private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        //{
        //    ControlsViewRow.Height = new GridLength(35);
        //    ControlsViewRow.MaxHeight = 35;
        //    //if (Row2Splitter != null)
        //    //    Row2Splitter.IsEnabled = false;
        //}

        private void SelectedControlDetailsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            //if (mControlFrameContentPage == null)
            //{
            //    SelectedControlDetailsExpander.IsExpanded = false;
            //    return;
            //}
            //ControlDetailsRow.Height = new GridLength(200, GridUnitType.Star);
            //ControlDetailsRow.MaxHeight = Double.PositiveInfinity;
        }

        private void SelectedControlDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            //ControlDetailsRow.Height = new GridLength(35);
            //ControlDetailsRow.MaxHeight = 35;
        }

        private void SetDetailsExpanderDesign(bool detailsExist, ElementInfo selectedElementInfo)
        {
            if (detailsExist == false)
            {
                //SelectedControlDetailsExpanderLable.Content = "Selected Element Details & Actions";
                //SelectedControlDetailsExpanderLable.Foreground = System.Windows.Media.Brushes.Gray;
                //SelectedControlDetailsExpander.IsEnabled = false;
                //SelectedControlDetailsExpander.IsExpanded = false;
            }
            else
            {
                //SelectedControlDetailsExpanderLable.Content = "'" + selectedElementInfo.ElementTitle + "' Element Details & Actions";
                //SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString()); ;
                //SelectedControlDetailsExpander.IsEnabled = true;
                if (mFirstElementSelectionDone == false)
                {
                    //SelectedControlDetailsExpander.IsExpanded = true;
                    mFirstElementSelectionDone = true;
                }
            }
        }

        private void SetActionsTabDesign(bool actionsExist)
        {
            if (actionsExist == false)
            {
                //ActionsTab.IsEnabled = false;
                //ControlActionsFrame.Visibility = System.Windows.Visibility.Collapsed;
                //GeneralTab.IsSelected = true;
            }
            else
            {
                //ActionsTab.IsEnabled = true;
                //ActionsTab.IsSelected = true;
                //ControlActionsFrame.Visibility = System.Windows.Visibility.Visible;
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

            //StatusTextBlock.Text = "Searching Elements...";
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
            //POMOn = !POMOn;
            //ShowPOMOrControlsViewsExpander();
        }

        private void ShowPOMOrControlsViewsExpander()
        {
            //if (POMOn)
            //{
            //    ControlsViewsExpander.Visibility = Visibility.Collapsed;
            //    POMFrame.Visibility = Visibility.Visible;
            //    ControlDetailsRow.Height = new GridLength(200, GridUnitType.Star);
            //    ControlDetailsRow.MaxHeight = Double.PositiveInfinity;
            //}
            //else
            //{
            //    ControlsViewsExpander.Visibility = Visibility.Visible;
            //    POMFrame.Visibility = Visibility.Collapsed;
            //}
        }

        ScreenShotViewPage mScreenShotViewPage;

        public void ShowScreenShot()
        {
            if (mWindowExplorerDriver == null || mContext.Agent.Driver.IsRunning() == false)
            {
                return;
            }

            xLoadingScreenShotBanner.Visibility = Visibility.Visible;
            xScreenShotFrame.Visibility = Visibility.Collapsed;

            /// UnComment later after complete Implementation of functionalities over all platforms.
            //if(IsWebMobJavaPlatform)
            //    mWindowExplorerDriver.UnHighLightElements();

            //Bitmap ScreenShotBitmap = ((UIAutomationDriverBase)mApplicationAgent.Agent.Driver).mUIAutomationHelper.GetAppWindowAsBitmap((AppWindow)xWindowSelection.WindowsComboBox.SelectedItem);  // GetScreenShot(new Tuple<int, int>(1000, 1000));   // new Tuple<int, int>(ApplicationPOMModel.cLearnScreenWidth, ApplicationPOMModel.cLearnScreenHeight));

            Bitmap ScreenShotBitmap = ((IVisualTestingDriver)mApplicationAgent.Agent.Driver).GetScreenShot();   // new Tuple<int, int>(ApplicationPOMModel.cLearnScreenWidth, ApplicationPOMModel.cLearnScreenHeight));

            if (mWindowExplorerDriver is GenericAppiumDriver)
            {
                mScreenShotViewPage = new ScreenShotViewPage("", ScreenShotBitmap, 0.25);
            }
            else
            {
                mScreenShotViewPage = new ScreenShotViewPage("", ScreenShotBitmap, 0.5);
            }

            if (mPlatform.PlatformType() == ePlatformType.Web)
            {
                mScreenShotViewPage.xMainImage.MouseMove += XMainImage_MouseMove;
            }
            
            mScreenShotViewPage.xMainImage.MouseLeftButtonDown += XMainImage_MouseLeftButtonDown;
            
            xScreenShotFrame.Content = mScreenShotViewPage;
            //xDeviceImage.Source = General.ToBitmapSource(ScreenShotBitmap);

            xLoadingScreenShotBanner.Visibility = Visibility.Collapsed;
            xScreenShotFrame.Visibility = Visibility.Visible;
        }

        private async void XMainImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RemoveElemntRectangle();

            System.Windows.Point pointOnImg = e.GetPosition((System.Windows.Controls.Image)sender);

            await HighlightSelectedElement(pointOnImg);
        }

        private async void XMainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mContext.Runner.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done during execution.");
            }
            else
            {
                /// Uncomment this logic post successfull implementation of highlight on screenshot image functionality for every platform
                /// Uncomment before final check-in
                //if (xUCElementDetails.SelectedElement != null 
                //    && IsWebMobJavaPlatform)    // this check untill unhilight functionality support isn't implemented for platforms other than Web and Java
                //{
                //    mWindowExplorerDriver.UnHighLightElements();
                //}

                //if(currentHighlightedElement == null)
                //{
                //    RemoveElemntRectangle();

                //    System.Windows.Point pointOnImg = e.GetPosition((System.Windows.Controls.Image)sender);

                //    HighlightSelectedElement(pointOnImg);
                //}

                RemoveElemntRectangle();

                System.Windows.Point pointOnImg = e.GetPosition((System.Windows.Controls.Image)sender);

                await HighlightSelectedElement(pointOnImg);

                xUCElementDetails.SelectedElement = currentHighlightedElement;

                mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(currentHighlightedElement);
                //mActInputValues = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetItemSpecificActionInputValues();

                ShowCurrentControlInfo();
            }
        }

        //private async void StorePageSource()
        //{
        //    int step = 0;
        //    try
        //    {
        //        step = 1;
        //        pageSourceString = await mApplicationAgent.Agent.Driver .GetPageSource();
        //        pageSourceTextViewer.Text = pageSourceString;
        //        step = 2;
        //        pageSourceXml = new XmlDocument();
        //        pageSourceXml.LoadXml(pageSourceString);
        //        pageSourceXMLViewer.xmlDocument = pageSourceXml;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser ||
        //                        AppiumDriver.DriverPlatformType == SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser)
        //            Reporter.ToLog(eLogLevel.ERROR, "Failed to get mobile page source or convert it to XML format", ex);
        //        else
        //            Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, ex.Message);

        //        if (step == 1)
        //        {
        //            pageSourceXml = null;
        //            pageSourceXMLViewer.xmlDocument = null;
        //            pageSourceTextViewer.Text = string.Empty;
        //        }
        //        else
        //        {
        //            pageSourceXml = null;
        //            pageSourceXMLViewer.xmlDocument = null;
        //        }
        //    }

        //    LoadingLabel.Visibility = Visibility.Hidden;
        //    inspectorElementTabsControl.Visibility = Visibility.Visible;
        //}

        private ElementInfo mCurrentHighlightedElement;
        public ElementInfo currentHighlightedElement
        {
            get
            {
                return mCurrentHighlightedElement;
            }
            set
            {
                mCurrentHighlightedElement = value;
            }
        }

        private async Task HighlightSelectedElement(System.Windows.Point pointOnImg)
        {
            try
            {
                //calculate clicked point on mobile
                System.Windows.Point pointOnAppScreen = GetPointOnAppWindow(pointOnImg);
                long pointOnMobile_X = (long)pointOnAppScreen.X;
                long pointOnMobile_Y = (long)pointOnAppScreen.Y;

                //get the clicked element
                ElementInfo inspectElementInfo = await ((IVisualTestingDriver)mApplicationAgent.Agent.Driver).GetElementAtPoint(pointOnMobile_X, pointOnMobile_Y);
                if (inspectElementInfo != null && inspectElementInfo != currentHighlightedElement)
                {
                    //show panel
                    //SetAttributesActionsView(true);

                    //update the attributes details
                    //elementAttributesDetails.Text = string.Empty;
                    //elementAttributesDetails.Text = inspectElementNode.LocalName + ":" + System.Environment.NewLine;
                    //foreach (XmlAttribute attribute in inspectElementNode.Attributes)
                    //    elementAttributesDetails.Text += attribute.Name + "=   '" + attribute.Value + "'" + System.Environment.NewLine;

                    //mark the element bounds on image
                    DrawElementRectangleAsync(inspectElementInfo);

                    //TODO: select the node in the xml

                    currentHighlightedElement = inspectElementInfo;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.MobileShowElementDetailsFailed, ex.Message);
            }
        }

        private void DrawElementRectangleAsync(ElementInfo clickedElementNode)
        {
            try
            {
                //remove previous rectangle
                RemoveElemntRectangle();

                //rectangleXmlNode = clickedElementNode;

                //get the element location
                double ratio_X = 0;
                double ratio_Y = 0;
                double element_Start_X = clickedElementNode.X;
                double element_Start_Y = clickedElementNode.Y;
                double element_Max_X = clickedElementNode.X + clickedElementNode.Width;
                double element_Max_Y = clickedElementNode.Y + clickedElementNode.Height;

                //draw the rectangle
                //mHighightRectangle = new System.Windows.Shapes.Rectangle();
                double rectangleWidth = -1, rectangleHeight = -1;

                /// Mobile specific calculations
                if (mWindowExplorerDriver is GenericAppiumDriver)
                {
                    XmlNode rectangleXmlNode = clickedElementNode.ElementObject as XmlNode;
                    switch (((GenericAppiumDriver)mWindowExplorerDriver).DevicePlatformType)
                    {
                        case Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile.eDevicePlatformType.Android:
                            ratio_X = mScreenShotViewPage.xMainImage.Source.Width / mScreenShotViewPage.xMainImage.ActualWidth;
                            ratio_Y = mScreenShotViewPage.xMainImage.Source.Height / mScreenShotViewPage.xMainImage.ActualHeight;
                            string bounds = rectangleXmlNode.Attributes["bounds"].Value;
                            bounds = bounds.Replace("[", ",");
                            bounds = bounds.Replace("]", ",");
                            string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (boundsXY.Count() == 4)
                            {
                                element_Start_X = (Convert.ToInt64(boundsXY[0])) / ratio_X;
                                //rectangleStartPoint_X = (long)element_Start_X;
                                element_Start_Y = (Convert.ToInt64(boundsXY[1])) / ratio_Y;
                                //rectangleStartPoint_Y = (long)element_Start_Y;
                                element_Max_X = (Convert.ToInt64(boundsXY[2])) / ratio_X;
                                element_Max_Y = (Convert.ToInt64(boundsXY[3])) / ratio_Y;
                            }
                            break;

                        case Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile.eDevicePlatformType.iOS:
                            //if ( AppiumDriver.DriverDeviceType == SeleniumAppiumDriver.eDeviceType.Phone)
                            //{
                            //    ratio_X = (mScreenShotViewPage.xMainImage.Source.Width / 2) / mScreenShotViewPage.xMainImage.ActualWidth;
                            //    ratio_Y = (mScreenShotViewPage.xMainImage.Source.Height / 2) / mScreenShotViewPage.xMainImage.ActualHeight;
                            //}
                            //else
                            //{
                                ratio_X = mScreenShotViewPage.xMainImage.Source.Width / mScreenShotViewPage.xMainImage.ActualWidth;
                                ratio_Y = mScreenShotViewPage.xMainImage.Source.Height / mScreenShotViewPage.xMainImage.ActualHeight;
                            //}
                            element_Start_X = (Convert.ToInt64(rectangleXmlNode.Attributes["x"].Value)) / ratio_X;
                            //rectangleStartPoint_X = (long)element_Start_X;
                            element_Start_Y = (Convert.ToInt64(rectangleXmlNode.Attributes["y"].Value)) / ratio_Y;
                            //rectangleStartPoint_Y = (long)element_Start_Y;
                            element_Max_X = element_Start_X + ((Convert.ToInt64(rectangleXmlNode.Attributes["width"].Value)) / ratio_X);
                            element_Max_Y = element_Start_Y + ((Convert.ToInt64(rectangleXmlNode.Attributes["height"].Value)) / ratio_Y);
                            break;
                    }

                    rectangleWidth = element_Max_X - element_Start_X;
                    rectangleHeight = element_Max_Y - element_Start_Y;
                }
                else
                {
                    rectangleWidth = clickedElementNode.Width;
                    rectangleHeight = clickedElementNode.Height;
                }

                mScreenShotViewPage.xHighlighterBorder.SetValue(Canvas.LeftProperty, element_Start_X + ((mScreenShotViewPage.xMainCanvas.ActualWidth - mScreenShotViewPage.xMainImage.ActualWidth) / 2));
                mScreenShotViewPage.xHighlighterBorder.SetValue(Canvas.TopProperty, element_Start_Y + ((mScreenShotViewPage.xMainCanvas.ActualHeight - mScreenShotViewPage.xMainImage.ActualHeight) / 2));
                mScreenShotViewPage.xHighlighterBorder.Margin = new Thickness(0);

                mScreenShotViewPage.xHighlighterBorder.Width = rectangleWidth;
                mScreenShotViewPage.xHighlighterBorder.Height = rectangleHeight;
                //mScreenShotViewPage.xHighLighterRectangle.Fill = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.5f };
                mScreenShotViewPage.xHighlighterBorder.Visibility = Visibility.Visible;

                //mScreenShotViewPage.xHighLighterRectangle.SetValue(Border.WidthProperty, 2);    // = new SolidColorBrush() { Color = Colors.Red, Opacity = 0.5f };

                //DeviceImageCanvas.Children.Add(mHighightRectangle);

                //bind events to device image events
                //mScreenShotViewPage.xHighLighterRectangle.MouseMove += DeviceImage_MouseMove;
                //mScreenShotViewPage.xHighLighterRectangle.MouseLeftButtonDown += DeviceImage_MouseLeftButtonDown;
                //mScreenShotViewPage.xHighLighterRectangle.MouseLeftButtonUp += DeviceImage_MouseLeftButtonUp;
                //mScreenShotViewPage.xHighLighterRectangle.MouseEnter += DeviceImage_MouseEnter;
                //mScreenShotViewPage.xHighLighterRectangle.MouseLeave += DeviceImage_MouseLeave;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to draw mobile element rectangle", ex);
            }
        }

        private void RemoveElemntRectangle()
        {
            if (mScreenShotViewPage != null)
            {
                //mScreenShotViewPage.xHighLighterRectangle.Visibility = Visibility.Collapsed;
                mScreenShotViewPage.xHighlighterBorder.Visibility = Visibility.Collapsed;
            }
        }

        private System.Windows.Point GetPointOnAppWindow(System.Windows.Point clickedPoint)
        {
            System.Windows.Point pointOnAppScreen = new System.Windows.Point();
            double ratio_X = 1;
            double ratio_Y = 1;

            if (mApplicationAgent.Agent.Driver.Platform == ePlatformType.Mobile)
            {
                //ratio_X = (DeviceImage.Source.Width / 2) / DeviceImage.ActualWidth;
                // ratio_Y = (DeviceImage.Source.Height / 2) / DeviceImage.ActualHeight;
            }
            else
            {
                ratio_X = mScreenShotViewPage.xMainImage.Source.Width / mScreenShotViewPage.xMainImage.ActualWidth;
                ratio_Y = mScreenShotViewPage.xMainImage.Source.Height / mScreenShotViewPage.xMainImage.ActualHeight;
            }

            pointOnAppScreen.X = (long)(clickedPoint.X * ratio_X);
            pointOnAppScreen.Y = (long)(clickedPoint.Y * ratio_Y);

            return pointOnAppScreen;
        }

        //public ElementInfo FindElementXmlNodeByXY(long pointOnMobile_X, long pointOnMobile_Y)
        //{
        //    try
        //    {
        //        //get screen elements nodes

        //        // Do once?
        //        // if XMLSOurce changed we need to refresh
        //        if (pageSourceXml == null)
        //        {
        //            //pageSourceString = (mContext.Agent.Driver as SeleniumDriver).GetPageSourceString();// ((SeleniumDriver)mWindowExplorerDriver).page.GetPageSource();
        //            //pageSourceHtml = new HtmlDocument();
        //            //pageSourceHtml.LoadHtml(pageSourceString);

        //            //pageSourceXml = new XmlDocument();
        //            //pageSourceXml.LoadXml(pageSourceString);
        //            //pageSourceXMLViewer.xmlDocument = pageSourceXml;
        //        }

        //        IEnumerable<ElementInfo> ElmsNodes = (mContext.Agent.Driver as SeleniumDriver).GetAllElementsFromPage("", null);

        //        //ElmsNodes = pageSourceHtml.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#"));

        //        ///get the selected element from screen
        //        if (ElmsNodes != null && ElmsNodes.Count() > 0)
        //        {
        //            //move to collection for getting last node which fits to bounds
        //            ObservableList<ElementInfo> ElmsNodesColc = new ObservableList<ElementInfo>();
        //            foreach (ElementInfo elemNode in ElmsNodes)
        //            {
        //                //if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOS && elemNode.LocalName == "UIAWindow") continue;                        
        //                //try { if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.Android && elemNode.Attributes["focusable"].Value == "false") continue; }catch (Exception ex) { }
        //                bool skipElement = false;
        //                //if (FilterElementsChK.IsChecked == true)
        //                //{
        //                //    string[] filterList = FilterElementsTxtbox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        //                //    try
        //                //    {
        //                //        for (int indx = 0; indx < filterList.Length; indx++)
        //                //            if (elemNode.Name.Contains(filterList[indx].Trim()) ||
        //                //                   elemNode.LocalName.Contains(filterList[indx].Trim()))
        //                //            {
        //                //                skipElement = true;
        //                //                break;
        //                //            }
        //                //    }
        //                //    catch (Exception ex)
        //                //    {
        //                //        //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); 
        //                //    }
        //                //}

        //                if (!skipElement)
        //                    ElmsNodesColc.Add(elemNode);
        //            }

        //            Dictionary<ElementInfo, long> foundElements = new Dictionary<ElementInfo, long>();
        //            foreach (ElementInfo elementNode in ElmsNodesColc.Reverse())
        //            {
        //                //get the element location
        //                long element_Start_X = -1;
        //                long element_Start_Y = -1;
        //                long element_Max_X = -1;
        //                long element_Max_Y = -1;

        //                //switch (AppiumDriver.DriverPlatformType)
        //                //{
        //                //    case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
        //                //        try
        //                //        {
        //                //            if (elementNode.Attributes["bounds"] != null)
        //                //            {
        //                //                string bounds = elementNode.Attributes["bounds"].Value;
        //                //                bounds = bounds.Replace("[", ",");
        //                //                bounds = bounds.Replace("]", ",");
        //                //                string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //                //                if (boundsXY.Count() == 4)
        //                //                {
        //                //                    element_Start_X = Convert.ToInt64(boundsXY[0]);
        //                //                    element_Start_Y = Convert.ToInt64(boundsXY[1]);
        //                //                    element_Max_X = Convert.ToInt64(boundsXY[2]);
        //                //                    element_Max_Y = Convert.ToInt64(boundsXY[3]);
        //                //                }
        //                //            }
        //                //            else
        //                //            {
        //                //                element_Start_X = -1;
        //                //                element_Start_Y = -1;
        //                //                element_Max_X = -1;
        //                //                element_Max_Y = -1;
        //                //            }
        //                //        }
        //                //        catch (Exception ex)
        //                //        {
        //                //            element_Start_X = -1;
        //                //            element_Start_Y = -1;
        //                //            element_Max_X = -1;
        //                //            element_Max_Y = -1;
        //                //            //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
        //                //        }
        //                //        break;

        //                //    case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
        //                //        try
        //                //        {
        //                //            element_Start_X = Convert.ToInt64(elementNode.Attributes["x"].Value);
        //                //            element_Start_Y = Convert.ToInt64(elementNode.Attributes["y"].Value);
        //                //            element_Max_X = element_Start_X + Convert.ToInt64(elementNode.Attributes["width"].Value);
        //                //            element_Max_Y = element_Start_Y + Convert.ToInt64(elementNode.Attributes["height"].Value);
        //                //        }
        //                //        catch (Exception ex)
        //                //        {
        //                //            element_Start_X = -1;
        //                //            element_Start_Y = -1;
        //                //            element_Max_X = -1;
        //                //            element_Max_Y = -1;
        //                //            //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
        //                //        }
        //                //        break;
        //                //}


        //                if (((pointOnMobile_X >= element_Start_X) && (pointOnMobile_X <= element_Max_X))
        //                           && ((pointOnMobile_Y >= element_Start_Y) && (pointOnMobile_Y <= element_Max_Y)))
        //                {
        //                    //object found                                
        //                    //return elementNode;
        //                    foundElements.Add(elementNode, ((element_Max_X - element_Start_X) * (element_Max_Y - element_Start_Y)));
        //                }
        //            }

        //            //getting the small node size found
        //            ElementInfo foundNode = null;
        //            long foundNodeSize = 0;
        //            if (foundElements.Count > 0)
        //            {
        //                foundNode = foundElements.Keys.First();
        //                foundNodeSize = foundElements.Values.First();
        //            }
        //            for (int indx = 0; indx < foundElements.Keys.Count; indx++)
        //            {
        //                if (foundElements.Values.ElementAt(indx) < foundNodeSize)
        //                {
        //                    foundNode = foundElements.Keys.First();
        //                    foundNodeSize = foundElements.Values.ElementAt(indx);
        //                }
        //            }
        //            if (foundNode != null)
        //                return foundNode;
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
        //        return null;
        //    }
        //}

        private void ViewsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (xViewsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xViewsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xViewsTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }

        bool RefreshGrid = false;
        bool RefreshTree = false;
        bool RefreshScreenshot = false;
        bool RefreshPageSrc = false;

        private void xRefreshCurrentTabContentBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshTabsContent();
        }

        public async void RefreshTabsContent()
        {
            RefreshGrid = true;
            RefreshTree = true;
            RefreshScreenshot = true;
            RefreshPageSrc = true;

            if (xViewsTabs.SelectedItem == xScreenShotViewTab)       /// Screenshot View
            {
                ShowScreenShot();
                RefreshScreenshot = false;
            }
            else if (xViewsTabs.SelectedItem == xGridViewTab)       /// Grid View
            {
                RefreshGridContent();

                RefreshGrid = false;
            }
            else if (xViewsTabs.SelectedItem == xTreeViewTab)        /// Tree View
            {
                await RefreshTreeContent();

                RefreshTree = false;
            }
            else if(xViewsTabs.SelectedItem == xPageSrcTab)
            {
                RefreshPageSrcContent();

                RefreshPageSrc = false;
            }
        }

        private void RefreshGridContent()
        {
            if (xWindowControlsGridView.DataSourceList == null || xWindowControlsGridView.DataSourceList.Count == 0)
                ShowFilterElementsPage();

            xLoadingControlsGridBanner.Visibility = Visibility.Visible;
            xWindowControlsGridView.Visibility = Visibility.Collapsed;

            RefreshFilteredElements();

            xLoadingControlsGridBanner.Visibility = Visibility.Collapsed;
            xWindowControlsGridView.Visibility = Visibility.Visible;
        }

        private void xMainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth > 700)
            {
                xRowSplitter.Visibility = Visibility.Collapsed;
                xColumnSplitter.Visibility = Visibility.Visible;

                xViewsTabs.SetValue(Grid.ColumnProperty, 0);
                xViewsTabs.SetValue(Grid.RowSpanProperty, 2);
                xViewsTabs.SetValue(Grid.RowProperty, 0);
                xViewsTabs.ClearValue(Grid.ColumnSpanProperty);

                xUCElementDetails.SetValue(Grid.ColumnProperty, 1);
                xUCElementDetails.SetValue(Grid.RowSpanProperty, 2);
                xUCElementDetails.SetValue(Grid.RowProperty, 0);
                xUCElementDetails.ClearValue(Grid.ColumnSpanProperty);
                xUCElementDetails.Margin = new Thickness(2, 0, 0, 0);
            }
            else
            {
                xRowSplitter.Visibility = Visibility.Visible;
                xColumnSplitter.Visibility = Visibility.Collapsed;

                xViewsTabs.SetValue(Grid.RowProperty, 0);
                xViewsTabs.SetValue(Grid.ColumnProperty, 0);
                xViewsTabs.SetValue(Grid.ColumnSpanProperty, 2);
                xViewsTabs.ClearValue(Grid.RowSpanProperty);

                xUCElementDetails.SetValue(Grid.RowProperty, 1);
                xUCElementDetails.SetValue(Grid.ColumnProperty, 0);
                xUCElementDetails.SetValue(Grid.ColumnSpanProperty, 2);
                xUCElementDetails.ClearValue(Grid.RowSpanProperty);
                xUCElementDetails.ClearValue(MarginProperty);
            }
        }

        private void xGridViewTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (RefreshGrid)
            {
                RefreshGridContent();
                RefreshGrid = false;
            }
        }

        private async void xTreeViewTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (RefreshTree)
            {
                await RefreshTreeContent();
                RefreshTree = false;
            }
        }

        private void ScreenShotViewTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (RefreshScreenshot)
            {
                ShowScreenShot();
                RefreshScreenshot = false;
            }
        }

        private async void xPageSrcTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if(mWindowExplorerDriver is GenericAppiumDriver)
            {
                xXMLPageSrcViewer.Visibility = Visibility.Visible;
                string pageSrcXML = await ((GenericAppiumDriver)mWindowExplorerDriver).GetPageSource();
                XmlDocument pageSrcXmlDoc = new XmlDocument();
                pageSrcXmlDoc.LoadXml(pageSrcXML);
                xXMLPageSrcViewer.xmlDocument = pageSrcXmlDoc;

                //xPageSrcTree.Visibility = Visibility.Collapsed;
                //string pageSrcXML = await ((GenericAppiumDriver)mWindowExplorerDriver).GetPageSource();

                //xUCPageSourceTree.GenerateTree(pageSrcXML);
            }
        }
    }
}
