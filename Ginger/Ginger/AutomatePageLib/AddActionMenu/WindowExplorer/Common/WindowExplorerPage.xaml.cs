#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Ginger.Actions.Locators.ASCF;
using Ginger.Actions.UserControls;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.UserControls;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.Common;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Mainframe;
using Ginger.WindowExplorer.Windows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

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
        public PlatformInfoBase Platform
        {
            get
            {
                return mPlatform;
            }
        }

        //If we come from ActionEditPage keep the Action
        private Act mAction;
        ObservableList<ElementInfo> VisibleElementsInfoList = [];
        bool mSyncControlsViewWithLiveSpy = false;
        bool mFirstElementSelectionDone = false;
        //Page mControlFrameContentPage = null;
        //WindowExplorerPOMPage mWindowExplorerPOMPage;
        //PomAllElementsPage pomAllElementsPage = null;

        Context mContext;

        // We can open it from agents grid, or from Action Edit page with Action 
        // If we open from ActionEdit Page then we update the act with Locator
        public WindowExplorerPage(ApplicationAgent ApplicationAgent, Context context, Act Act = null)
        {
            InitializeComponent();

            xWindowControlsTreeView.TreeGrid.RowDefinitions[0].Height = new GridLength(0);

            mContext = context;
            //mContext.PropertyChanged += MContext_PropertyChanged;
            mPlatform = PlatformInfoBase.GetPlatformImpl(ApplicationAgent.Agent.Platform);

            //Instead of check make it disabled ?
            if (((AgentOperations)ApplicationAgent.Agent.AgentOperations).Driver != null && (((AgentOperations)ApplicationAgent.Agent.AgentOperations).Driver is IWindowExplorer) == false)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Control selection is not available yet for driver - " + ((AgentOperations)ApplicationAgent.Agent.AgentOperations).Driver.GetType().ToString());
                _GenWin.Close();
                return;
            }

            IWindowExplorer WindowExplorerDriver = (IWindowExplorer)((AgentOperations)ApplicationAgent.Agent.AgentOperations).Driver;

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
            xWindowControlsTreeView.TreeTitleVisibility = Visibility.Collapsed;
            xWindowControlsTreeView.Tree.ItemSelected += WindowControlsTreeView_ItemSelected;

            xWindowSelection.RefreshWindowsButton.Click += RefreshWindowsButton_Click;
            xWindowSelection.AddSwitchWindowActionButton.Click += AddSwitchWindowActionButton_Click;
            xWindowSelection.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;

            SetControlsGridView();

            //xUCElementDetails.AppAgent = mApplicationAgent;
            xWindowSelection.context = context;
            xUCElementDetails.Context = context;
            xUCElementDetails.WindowExplorerDriver = mWindowExplorerDriver;
            xUCElementDetails.Platform = mPlatform;
            xUCElementDetails.xPropertiesGrid.btnRefresh.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler(RefreshControlProperties));

            //IsPOMSupportedPlatform = ApplicationPOMModel.PomSupportedPlatforms.Contains(mContext.Platform);

            //RefreshTabsContent();

            xUCElementDetails.PropertyChanged += XUCElementDetails_PropertyChanged;
            SetPlatformBasedUpdates();

            xHTMLPageSrcViewer.HTMLTree.SelectedItemChanged += HTMLPageSourceTree_SelectedItemChanged;
            xXMLPageSrcViewer.XMLTree.SelectedItemChanged += XMLPageSourceTree_SelectedItemChanged;

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

        private void HTMLPageSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            HtmlNode hNode = null;

            if ((sender as TreeView).SelectedItem != null)
            {
                hNode = ((sender as TreeView).SelectedItem as TreeViewItem).Tag as HtmlNode;
            }

            if (hNode != null && hNode.NodeType == HtmlNodeType.Element)
            {
                HTMLElementInfo EI = new HTMLElementInfo()
                {
                    XPath = hNode.XPath,
                    ID = hNode.Id,
                    HTMLElementObject = hNode
                };

                ElementInfo elemInfo = mWindowExplorerDriver.LearnElementInfoDetails(EI);

                if (elemInfo != null)
                {
                    elemInfo.SetLocatorsAndPropertiesCategory(((DriverBase)mWindowExplorerDriver).PomCategory);

                    mCurrentControlTreeViewItem = GetTreeViewItemForElementInfo(elemInfo);
                    ShowCurrentControlInfo();
                }
            }
        }

        private void XMLPageSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            XmlNode xmlNode = null;

            if ((sender as TreeView).SelectedItem != null)
            {
                xmlNode = (sender as TreeView).SelectedItem as XmlNode;
            }

            if (xmlNode != null && xmlNode.Attributes.Count > 0)
            {
                ElementInfo elemInfo = mWindowExplorerDriver.LearnElementInfoDetails(new ElementInfo() { ElementObject = xmlNode });

                if (elemInfo != null)
                {
                    elemInfo.SetLocatorsAndPropertiesCategory(((DriverBase)mWindowExplorerDriver).PomCategory);

                    mCurrentControlTreeViewItem = GetTreeViewItemForElementInfo(elemInfo);
                    ShowCurrentControlInfo();
                }
            }
            else
            {
                xUCElementDetails.SelectedElement = null;
                mCurrentControlTreeViewItem = null;
            }
        }

        bool ElementDetailsNotNullHandled = false;
        bool ElementDetailsNullHandled = false;
        private void XUCElementDetails_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(xUCElementDetails.SelectedElement))
            {
                HandleUCElementDetails();
            }
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
            else if (xUCElementDetails.SelectedElement != null && !ElementDetailsNotNullHandled)
            {
                xElementDetailGridRow.Height = new GridLength(50, GridUnitType.Star);
                xElementDetailGridColumn.Width = new GridLength(50, GridUnitType.Star);

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

        object DefaultSelectedTab()
        {
            return mWindowExplorerDriver.DefaultView() switch
            {
                eTabView.GridView => xGridViewTab,
                eTabView.TreeView => xTreeViewTab,
                eTabView.PageSource => xPageSrcTab,
                eTabView.Screenshot => xScreenShotViewTab,
                _ => null,
            };
        }

        bool ExplorerLoaded = false;
        private void SetPlatformBasedUpdates()
        {
            if (mWindowExplorerDriver != null)
            {
                xWindowSelection.xWindowDropdownLbl.Content = mWindowExplorerDriver.SelectionWindowText();

                xPageSrcTab.Visibility = mWindowExplorerDriver.SupportedViews().Contains(eTabView.PageSource) ? Visibility.Visible : Visibility.Collapsed;
                xTreeViewTab.Visibility = mWindowExplorerDriver.SupportedViews().Contains(eTabView.TreeView) ? Visibility.Visible : Visibility.Collapsed;
                xGridViewTab.Visibility = mWindowExplorerDriver.SupportedViews().Contains(eTabView.GridView) ? Visibility.Visible : Visibility.Collapsed;
                xScreenShotViewTab.Visibility = mWindowExplorerDriver.SupportedViews().Contains(eTabView.Screenshot) ? Visibility.Visible : Visibility.Collapsed;

                xLiveSpyTab.Visibility = mWindowExplorerDriver.IsLiveSpySupported() ? Visibility.Visible : Visibility.Collapsed;

                xWindowSelection.Visibility = mWindowExplorerDriver.IsWinowSelectionRequired() ? Visibility.Visible : Visibility.Collapsed;

                xViewsTabs.SelectedItem = DefaultSelectedTab();

                InitUCElementDetailsLocatorsGrid();

                ExplorerLoaded = true;
            }
            else
            {
                ExplorerLoaded = false;
            }
        }

        //private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
        //    {
        //        if (e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
        //        {

        //        }
        //    }
        //}

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

                if (!ExplorerLoaded)
                {
                    SetPlatformBasedUpdates();
                }

                if (spyPage != null)
                {
                    spyPage.SetDriver(windowExplorerDriver);
                }
            });
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

                    if (xWindowSelection.WindowsComboBox.Items.Count == 1 && ((AgentOperations)mContext.Agent.AgentOperations).Driver.IsRunning())
                    {
                        xWindowSelection.WindowsComboBox.SelectedItem = xWindowSelection.WindowsComboBox.Items[0];
                        mWindowExplorerDriver.SwitchWindow((xWindowSelection.WindowsComboBox.SelectedItem as AppWindow).Title);
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

        private async Task RefreshPageSrcContent()
        {
            xLoadingPageSrcBanner.Visibility = Visibility.Visible;
            xXMLPageSrcViewer.Visibility = Visibility.Collapsed;
            xHTMLPageSrcViewer.Visibility = Visibility.Collapsed;

            try
            {
                var srcDoc = await mWindowExplorerDriver.GetPageSourceDocument(true);

                if (srcDoc is not null and XmlDocument)
                {
                    xXMLPageSrcViewer.Visibility = Visibility.Visible;
                    xXMLPageSrcViewer.xmlDocument = srcDoc as XmlDocument;
                }
                else if (srcDoc is HtmlDocument)
                {
                    xHTMLPageSrcViewer.Visibility = Visibility.Visible;

                    xHTMLPageSrcViewer.htmlDocument = srcDoc as HtmlDocument;
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
            }
            finally
            {
                xLoadingPageSrcBanner.Visibility = Visibility.Collapsed;
            }
        }

        public AppWindow SwitchToCurrentWindow()
        {
            GingerCore.General.DoEvents();

            AppWindow AW = (AppWindow)xWindowSelection.WindowsComboBox.SelectedItem;
            if (AW == null)
            {
                return null;
            }

            mWindowExplorerDriver.SwitchWindow(AW.Title);

            return AW;
        }

        private async Task RefreshTreeContent()
        {
            xLoadingTreeViewBanner.Visibility = Visibility.Visible;
            xWindowControlsTreeView.Visibility = Visibility.Collapsed;

            AppWindow AW = SwitchToCurrentWindow();

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
                    ASCFFormTreeItem AFTI = new ASCFFormTreeItem
                    {
                        Name = AW.Title,
                        Path = AW.Path
                    };
                    InitTree(AFTI);
                    break;
                case AppWindow.eWindowType.WebPage:
                    HTMLPageTreeItem HPTI = new HTMLPageTreeItem();
                    HTMLElementInfo EI = new HTMLElementInfo
                    {
                        ElementTitle = AW.Title,
                        XPath = "/html",
                        WindowExplorer = mWindowExplorerDriver
                    };
                    HPTI.ElementInfo = EI;
                    InitTree(HPTI);
                    break;
                case AppWindow.eWindowType.JFrmae:
                    JavaWindowTreeItem JWTI = new JavaWindowTreeItem();
                    JavaElementInfo JEI = new JavaElementInfo
                    {
                        ElementTitle = AW.Title,
                        Path = AW.Title,
                        XPath = "/",
                        IsExpandable = true
                    };
                    JWTI.JavaElementInfo = JEI;
                    JEI.WindowExplorer = mWindowExplorerDriver;
                    InitTree(JWTI);
                    break;
                ///We're no more working with TreeView for Mobile platform
                //case AppWindow.eWindowType.Appium:
                //    AppiumWindowTreeItem AWTI = new AppiumWindowTreeItem();

                //    AppiumElementInfo AEI = new AppiumElementInfo();
                //    //ElementInfo AEI = new ElementInfo();
                //    AEI.WindowExplorer = mWindowExplorerDriver;
                //    AEI.XPath = "/";
                //    GenericAppiumDriver GAD = ((GenericAppiumDriver)mWindowExplorerDriver);

                //    string pageSourceString = await GAD.GetPageSource();
                //    XmlDocument pageSourceXml = new XmlDocument();
                //    pageSourceXml.LoadXml(pageSourceString);
                //    AEI.XmlDoc = pageSourceXml;
                //    AEI.ElementObject = pageSourceXml.SelectSingleNode("/");

                //    AWTI.ElementInfo = AEI;

                //    InitTree(AWTI);
                //    break;
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
                    MainframeTreeItemBase MFTI = new MainframeTreeItemBase
                    {
                        Name = AW.Title,
                        Path = AW.Path
                    };
                    InitTree(MFTI);
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unknown Window type:" + AW.WindowType);
                    break;
            }

            xLoadingTreeViewBanner.Visibility = Visibility.Collapsed;
            xWindowControlsTreeView.Visibility = Visibility.Visible;
        }

        private void SetPageFunctionalityEnableDisable(bool RecordingButtonFlag, bool ControlsRefreshButtonFlag, bool RefreshWindowsButtonFlag, bool AddSwitchWindowActionButtonFlag, bool WindowsComboBoxFlag, bool ControlsViewsExpanderFlag, bool SelectedControlDetailsExpanderFlag, bool LiveSpyButtonFlag, bool SyncWithLiveSpyButtonFlag, bool GridTreeViewButtonFlag, bool DORButtonFlag)
        {
            xWindowSelection.RefreshWindowsButton.IsEnabled = RefreshWindowsButtonFlag;

            xWindowSelection.AddSwitchWindowActionButton.IsEnabled = AddSwitchWindowActionButtonFlag;

            xWindowSelection.WindowsComboBox.IsEnabled = WindowsComboBoxFlag;
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
            if (mSpyElement == null)
            {
                return;
            }

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
                        if (mSpyElement is UIAElementInfo UEI)
                        {
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
            if (root == null)
            {
                return null;
            }

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
                    if (vv != null)
                    {
                        return vv;
                    }
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
                    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
                    {
                        EI.GetElementProperties();
                        xUCElementDetails.SelectedElement = EI;
                    }
                    else
                    {
                        xUCElementDetails.xPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ObjectUnavailable);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoadTryRefresh);
            }
        }

        //TODO: fix to be OO style
        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem != null)
            {
                RefreshTabsContent();
            }
        }

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowsList();
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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60, ReadOnly = true },
                new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150, ReadOnly = true },
            ]
            };

            xWindowControlsGridView.SetAllColumnsDefaultView(view);
            xWindowControlsGridView.InitViewItems();
        }

        private async Task RefreshControlsGrid()
        {
            if (xWindowSelection.WindowsComboBox.SelectedValue != null && mWindowExplorerDriver != null)
            {
                try
                {
                    //StatusTextBlock.Text = "Loading";
                    PomSetting pomSetting = new PomSetting
                    {
                        FilteredElementType = CheckedFilteringCreteriaList
                    };
                    List<ElementInfo> list = await mWindowExplorerDriver.GetVisibleControls(pomSetting);

                    // Convert to obserable for the grid
                    VisibleElementsInfoList.Clear();
                    foreach (ElementInfo EI in list)
                    {
                        if (EI.WindowExplorer == null)
                        {
                            EI.WindowExplorer = mWindowExplorerDriver;
                        }

                        EI.ElementTitle = EI.ElementName;

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
            }

            ShowCurrentControlInfo();
        }

        private ITreeViewItem GetTreeViewItemForElementInfo(Amdocs.Ginger.Common.UIElement.ElementInfo EI)
        {
            if (EI == null)
            {
                return null; // can happen when grid is filtered
            }

            //TODO: make it OO style avoid the if else if
            ITreeViewItem TVI = null;
            if (EI is JavaElementInfo)
            {
                TVI = JavaElementInfoConverter.GetTreeViewItemFor(EI);
            }
            else if (EI is UIAElementInfo UEI)
            {
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
            else if (EI is AppiumElementInfo || EI.WindowExplorer is GenericAppiumDriver)
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

        private async Task<bool> RefreshFilteredElements()
        {
            xLoadingControlsGridBanner.Visibility = Visibility.Visible;
            xWindowControlsGridView.Visibility = Visibility.Collapsed;

            try
            {
                if (FilteringCreteriaList.Count != 0)
                {
                    if (CheckedFilteringCreteriaList.Count == 0)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.FilterNotBeenSet);
                        if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                        {
                            await RefreshControlsGrid();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        await RefreshControlsGrid();
                        return true;
                    }
                }
                else
                {
                    Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.RetreivingAllElements);
                    if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                    {
                        await RefreshControlsGrid();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            finally
            {
                xLoadingControlsGridBanner.Visibility = Visibility.Collapsed;
                xWindowControlsGridView.Visibility = Visibility.Visible;
            }
        }

        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (xWindowSelection.WindowsComboBox.SelectedValue != null)
            {
                WindowExplorerCommon.CreateActUISwitchWindowAction(((AppWindow)xWindowSelection.WindowsComboBox.SelectedValue).Title, mContext);
            }
        }

        ObservableList<UIElementFilter> CheckedFilteringCreteriaList = [];
        ObservableList<UIElementFilter> FilteringCreteriaList = [];

        private void FilterElementButtonClicked(object sender, RoutedEventArgs e)
        {
            ShowFilterElementsPage();
        }

        public async Task<bool> DoSearchControls()
        {
            bool isSearched;
            foreach (UIElementFilter filter in FilteringCreteriaList)
            {
                if (filter.Selected)
                {
                    if (!CheckedFilteringCreteriaList.Contains(filter))
                    {
                        CheckedFilteringCreteriaList.Add(filter);
                    }
                }
            }

            //StatusTextBlock.Text = "Searching Elements...";
            GingerCore.General.DoEvents();

            isSearched = await RefreshFilteredElements();

            return isSearched;
        }

        private void SetAutoMapElementTypes()
        {
            List<eElementType> UIElementsTypeList = mPlatform.GetPlatformUIElementsType();

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
            {
                SetAutoMapElementTypes();
            }

            if (FilteringCreteriaList.Count != 0)
            {
                CheckedFilteringCreteriaList = [];
                FilterElementsPage FEW = new FilterElementsPage(FilteringCreteriaList, CheckedFilteringCreteriaList, /*ControlsSearchButton_Click,*/ this);
                FEW.ShowAsWindow(eWindowShowStyle.Dialog);

                foreach (UIElementFilter filter in FilteringCreteriaList)
                {
                    if (filter.Selected)
                    {
                        if (!CheckedFilteringCreteriaList.Contains(filter))
                        {
                            CheckedFilteringCreteriaList.Add(filter);
                        }
                    }
                }
            }
        }

        ScreenShotViewPage mScreenShotViewPage;

        public void ShowScreenShot()
        {
            if (mWindowExplorerDriver == null || ((AgentOperations)mContext.Agent.AgentOperations).Driver == null || ((AgentOperations)mContext.Agent.AgentOperations).Driver.IsRunning() == false)
            {
                return;
            }

            xLoadingScreenShotBanner.Visibility = Visibility.Visible;
            xScreenShotFrame.Visibility = Visibility.Collapsed;
            xRetrySSBanner.Visibility = Visibility.Collapsed;

            var runtimeDriver = ((AgentOperations)mApplicationAgent.Agent.AgentOperations).Driver;
            if (runtimeDriver is not IVisualTestingDriver visualDriver)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Screenshot is not supported for this agent driver. Verify the agent's driver implements IVisualTestingDriver (GetScreenShot).");
                xLoadingScreenShotBanner.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                mWindowExplorerDriver.UnHighLightElements(); // keep previous behavior

            }
            catch
            {
                // ignore
            }

            try
            {
                LoadPageSourceDoc = mWindowExplorerDriver.SupportedViews().Contains(eTabView.PageSource);

                if (!mWindowExplorerDriver.SupportedViews().Contains(eTabView.Screenshot) || SwitchToCurrentWindow() == null)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "ShowScreenShot: Screenshot view not supported for current IWindowExplorer implementation or no active window.");
                    return;
                }

                using (Bitmap ScreenShotBitmap = visualDriver.GetScreenShot())
                {
                    if (ScreenShotBitmap == null)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "ShowScreenShot: GetScreenShot returned null.");
                        xRetrySSBanner.Visibility = Visibility.Visible;
                        return;
                    }

                    mScreenShotViewPage = new ScreenShotViewPage("", ScreenShotBitmap, (mWindowExplorerDriver as DriverBase)?.ScreenShotInitialZoom() ?? 0.5)
                    {
                        ImageMouseCursor = Cursors.Hand
                    };

                    // wire up click to try to resolve element under clicked point (if driver supports IVisualTestingDriver)
                    mScreenShotViewPage.xMainImage.MouseLeftButtonDown += XMainImage_MouseLeftButtonDown;

                    xScreenShotFrame.ClearAndSetContent(mScreenShotViewPage);

                    // Re-apply previously selected element rectangle (if any). This preserves highlight across screenshot refreshes.
                    try
                    {
                        if (currentHighlightedElement != null)
                        {
                            // ensure element fields are fresh for drawing
                            try { mWindowExplorerDriver?.UpdateElementInfoFields(currentHighlightedElement); } catch { }
                            DrawElementRectangleAsync(currentHighlightedElement);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "ShowScreenShot: failed to reapply previous highlight: " + ex.Message, ex);
                    }

                    xScreenShotFrame.Visibility = Visibility.Visible;
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
                xRetrySSBanner.Visibility = Visibility.Visible;
            }
            finally
            {
                xLoadingScreenShotBanner.Visibility = Visibility.Collapsed;
            }
        }

        private bool mLastSearchFinished = true;
        bool LastSearchFinished
        {
            get
            {
                return mLastSearchFinished;
            }
            set
            {
                mLastSearchFinished = value;

                if (mLastSearchFinished)
                {
                    xStatusBarIcon.ImageType = Amdocs.Ginger.Common.Enums.eImageType.MousePointer;
                    xStatusBarText.Text = "Click on an element to see its details";
                }
                else
                {
                    xStatusBarIcon.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
                    xStatusBarText.Text = "Loading element details. Please wait.";
                }
            }
        }

        private async void XMainImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                // Nothing to do - keep currentHighlightedElement rectangle visible
                return;
            }

            // Ctrl is pressed -> perform live lookup and update highlight rectangle
            LastSearchFinished = false;

            // We intentionally remove previous rectangle as we'll draw new one for the hovered element
            RemoveElemntRectangle();

            System.Windows.Point pointOnImg = e.GetPosition((System.Windows.Controls.Image)sender);

            await HighlightSelectedElement(new System.Drawing.Point((int)pointOnImg.X, (int)pointOnImg.Y));

            LastSearchFinished = true;
        }

        private async void XMainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mContext.Runner.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done during execution.");
            }
            else if (!LastSearchFinished)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done until previous request is finished.");
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

                LastSearchFinished = false;
                mScreenShotViewPage.ImageMouseCursor = Cursors.Wait;
                try
                {
                    if (LoadPageSourceDoc)
                    {
                        await mWindowExplorerDriver.GetPageSourceDocument(true);
                        LoadPageSourceDoc = false;
                    }

                    RemoveElemntRectangle();

                    System.Windows.Point pointOnImg = e.GetPosition((System.Windows.Controls.Image)sender);

                    await HighlightSelectedElement(new System.Drawing.Point((int)pointOnImg.X, (int)pointOnImg.Y));

                    mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(currentHighlightedElement);

                    ShowCurrentControlInfo();
                }
                catch (Exception exc)
                {
                    Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
                }
                finally
                {
                    LastSearchFinished = true;
                    mScreenShotViewPage.ImageMouseCursor = Cursors.Hand;
                }
            }
        }

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

        private async Task HighlightSelectedElement(System.Drawing.Point ClickedPoint)
        {
            try
            {
                // Convert mouse point on displayed Image control to source (screenshot / device) coordinates
                // This mirrors DrawElementRectangleAsync mapping and handles Image.Stretch (Uniform/UniformToFill/Fill)
                var img = mScreenShotViewPage?.xMainImage;
                int srcX = 0, srcY = 0;
                bool coordsFromImage = false;

                if (img != null)
                {
                    try
                    {
                        double srcW = 0, srcH = 0;
                        if (img.Source is System.Windows.Media.Imaging.BitmapSource bmp && bmp.PixelWidth > 0 && bmp.PixelHeight > 0)
                        {
                            srcW = bmp.PixelWidth;
                            srcH = bmp.PixelHeight;
                        }

                        if (srcW <= 0) srcW = Math.Max(1.0, img.ActualWidth);
                        if (srcH <= 0) srcH = Math.Max(1.0, img.ActualHeight);

                        double dispW = Math.Max(1.0, img.ActualWidth);
                        double dispH = Math.Max(1.0, img.ActualHeight);

                        double scaleX = dispW / srcW;
                        double scaleY = dispH / srcH;
                        double usedScaleX = scaleX;
                        double usedScaleY = scaleY;
                        double offsetX = 0;
                        double offsetY = 0;

                        var stretch = img.Stretch;
                        if (stretch == Stretch.Uniform || stretch == Stretch.UniformToFill)
                        {
                            double uniformScale = (stretch == Stretch.Uniform) ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
                            usedScaleX = usedScaleY = uniformScale;

                            double renderedWidth = srcW * uniformScale;
                            double renderedHeight = srcH * uniformScale;
                            offsetX = (dispW - renderedWidth) / 2.0;
                            offsetY = (dispH - renderedHeight) / 2.0;
                        }

                        // Mouse point passed in is in displayed image coords (ClickedPoint)
                        double calcSrcX = (ClickedPoint.X - offsetX) / Math.Max(1e-6, usedScaleX);
                        double calcSrcY = (ClickedPoint.Y - offsetY) / Math.Max(1e-6, usedScaleY);

                        // Clamp to source bounds
                        calcSrcX = Math.Max(0, Math.Min(calcSrcX, srcW - 1));
                        calcSrcY = Math.Max(0, Math.Min(calcSrcY, srcH - 1));

                        srcX = (int)Math.Round(calcSrcX);
                        srcY = (int)Math.Round(calcSrcY);

                        coordsFromImage = true;
                    }
                    catch (Exception exMap)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "HighlightSelectedElement: failed to convert displayed point to source coords, falling back to driver mapping: " + exMap.Message, exMap);
                        coordsFromImage = false;
                    }
                }

                // If we failed to compute from image, fallback to driver mapping (old behavior)
                System.Drawing.Point pointOnAppScreen;
                if (coordsFromImage)
                {
                    pointOnAppScreen = new System.Drawing.Point(srcX, srcY);
                }
                else
                {
                    // DriverBase mapping - kept as fallback
                    pointOnAppScreen = ((DriverBase)mWindowExplorerDriver).GetPointOnAppWindow(ClickedPoint,
                        mScreenShotViewPage?.xMainImage?.Source?.Width ?? mScreenShotViewPage?.xMainImage?.ActualWidth ?? 0,
                        mScreenShotViewPage?.xMainImage?.Source?.Height ?? mScreenShotViewPage?.xMainImage?.ActualHeight ?? 0,
                        mScreenShotViewPage?.xMainImage?.ActualWidth ?? 0,
                        mScreenShotViewPage?.xMainImage?.ActualHeight ?? 0);
                }

                var runtimeDriver = ((AgentOperations)mApplicationAgent.Agent.AgentOperations).Driver;
                if (runtimeDriver is not IVisualTestingDriver visualDriver)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Element lookup from screenshot is not supported by this driver. Driver must implement IVisualTestingDriver.GetElementAtPoint.");
                    return;
                }

                // get the clicked element from the visual testing driver using precise source coords
                ElementInfo inspectElementInfo = await visualDriver.GetElementAtPoint(pointOnAppScreen.X, pointOnAppScreen.Y);
                if (inspectElementInfo == null)
                {
                    // nothing found at the point - hide previous rectangle / selection
                    RemoveElemntRectangle();
                    xUCElementDetails.SelectedElement = null;
                    return;
                }

                // If this is the same element already highlighted, just return
                if (inspectElementInfo == currentHighlightedElement)
                {
                    return;
                }

                try
                {
                    // Prepare element for explorer usage
                    inspectElementInfo.WindowExplorer ??= mWindowExplorerDriver;
                    inspectElementInfo.SetLocatorsAndPropertiesCategory(((DriverBase)mWindowExplorerDriver).PomCategory);

                    // Ensure coordinates/size are fresh (driver-specific)
                    try
                    {
                        mWindowExplorerDriver?.UpdateElementInfoFields(inspectElementInfo);
                    }
                    catch (Exception exUpd)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "HighlightSelectedElement: UpdateElementInfoFields failed: " + exUpd.Message, exUpd);
                    }

                    // Draw rectangle in screenshot view using the element's (now fresh) bounds
                    DrawElementRectangleAsync(inspectElementInfo);

                    // Keep persistent highlighted element reference for UI flows
                    currentHighlightedElement = inspectElementInfo;

                    // Ask the driver to highlight the same element in the actual app - DO NOT locate by locators,
                    // locating by locators may resolve to a different element than the exact element returned by GetElementAtPoint.
                    try
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                mWindowExplorerDriver?.HighLightElement(inspectElementInfo, locateElementByItLocators: false);
                            }
                            catch (Exception exDriver)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "HighlightSelectedElement: driver HighLightElement failed: " + exDriver.Message, exDriver);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "HighlightSelectedElement: scheduling driver highlight failed: " + ex.Message, ex);
                    }

                    // Update UI details pane
                    mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(currentHighlightedElement);
                    ShowCurrentControlInfo();
                }
                catch (Exception exInner)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "HighlightSelectedElement: processing inspectElementInfo failed", exInner);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.MobileShowElementDetailsFailed, ex.Message);
            }
        }

        private void DrawElementRectangleAsync(ElementInfo clickedElementInfo)
        {
            try
            {
                // remove previous rectangle
                RemoveElemntRectangle();

                // element bounds in source image/device coordinates
                System.Drawing.Point ElementStartPoint = new System.Drawing.Point(clickedElementInfo.X, clickedElementInfo.Y);
                System.Drawing.Point ElementMaxPoint = new System.Drawing.Point(clickedElementInfo.X + clickedElementInfo.Width, clickedElementInfo.Y + clickedElementInfo.Height);

                // try to get source image size (device/screenshot) safely
                double srcW = 0, srcH = 0;
                try
                {
                    var bmpSrc = mScreenShotViewPage.xMainImage.Source as System.Windows.Media.Imaging.BitmapSource;
                    if (bmpSrc != null && bmpSrc.PixelWidth > 0 && bmpSrc.PixelHeight > 0)
                    {
                        srcW = bmpSrc.PixelWidth;
                        srcH = bmpSrc.PixelHeight;
                    }
                }
                catch { /* ignore */ }

                // fallback to image Actual size if source pixels not available
                if (srcW <= 0) srcW = Math.Max(1.0, mScreenShotViewPage.xMainImage.ActualWidth);
                if (srcH <= 0) srcH = Math.Max(1.0, mScreenShotViewPage.xMainImage.ActualHeight);

                double dispW = Math.Max(1.0, mScreenShotViewPage.xMainImage.ActualWidth);
                double dispH = Math.Max(1.0, mScreenShotViewPage.xMainImage.ActualHeight);

                // Allow driver/platform to adjust rectangle (works in source coordinates)
                double rectangleWidth = clickedElementInfo.Width;
                double rectangleHeight = clickedElementInfo.Height;
                try
                {
                    bool driverHandled = ((DriverBase)mWindowExplorerDriver).SetRectangleProperties(
                        ref ElementStartPoint,
                        ref ElementMaxPoint,
                        srcW,
                        srcH,
                        dispW,
                        dispH,
                        clickedElementInfo);

                    if (driverHandled)
                    {
                        rectangleWidth = ElementMaxPoint.X - ElementStartPoint.X;
                        rectangleHeight = ElementMaxPoint.Y - ElementStartPoint.Y;
                    }
                    else
                    {
                        rectangleWidth = clickedElementInfo.Width;
                        rectangleHeight = clickedElementInfo.Height;
                    }
                }
                catch
                {
                    rectangleWidth = clickedElementInfo.Width;
                    rectangleHeight = clickedElementInfo.Height;
                }

                // Compute display scale & offsets depending on Image.Stretch
                double scaleX = dispW / srcW;
                double scaleY = dispH / srcH;
                double usedScaleX = scaleX;
                double usedScaleY = scaleY;
                double offsetX = 0;
                double offsetY = 0;

                var stretch = mScreenShotViewPage.xMainImage.Stretch;
                if (stretch == Stretch.Uniform || stretch == Stretch.UniformToFill)
                {
                    double uniformScale = (stretch == Stretch.Uniform) ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
                    usedScaleX = usedScaleY = uniformScale;

                    double renderedWidth = srcW * uniformScale;
                    double renderedHeight = srcH * uniformScale;
                    offsetX = (dispW - renderedWidth) / 2.0;
                    offsetY = (dispH - renderedHeight) / 2.0;
                }
                // else Fill -> keep independent scales

                // Convert source coords to displayed image coords
                double left = ElementStartPoint.X * usedScaleX + offsetX;
                double top = ElementStartPoint.Y * usedScaleY + offsetY;
                double right = (ElementStartPoint.X + rectangleWidth) * usedScaleX + offsetX;
                double bottom = (ElementStartPoint.Y + rectangleHeight) * usedScaleY + offsetY;

                if (right < left) { var t = left; left = right; right = t; }
                if (bottom < top) { var t = top; top = bottom; bottom = t; }

                double w = Math.Max(2, right - left);
                double h = Math.Max(2, bottom - top);

                // account for Canvas offsets of the Image control
                double canvasLeftForMainImage = Canvas.GetLeft(mScreenShotViewPage.xMainImage);
                if (double.IsNaN(canvasLeftForMainImage))
                    canvasLeftForMainImage = 0;
                double canvasTopForMainImage = Canvas.GetTop(mScreenShotViewPage.xMainImage);
                if (double.IsNaN(canvasTopForMainImage))
                    canvasTopForMainImage = 0;

                // apply to UI on dispatcher
                mScreenShotViewPage.xHighlighterBorder.Dispatcher.BeginInvoke(new Action(() =>
                {
                    mScreenShotViewPage.xHighlighterBorder.SetValue(Canvas.LeftProperty, left + canvasLeftForMainImage);
                    mScreenShotViewPage.xHighlighterBorder.SetValue(Canvas.TopProperty, top + canvasTopForMainImage);
                    mScreenShotViewPage.xHighlighterBorder.Margin = new Thickness(0);

                    mScreenShotViewPage.xHighlighterBorder.Width = w;
                    mScreenShotViewPage.xHighlighterBorder.Height = h;
                    mScreenShotViewPage.xHighlighterBorder.Visibility = Visibility.Visible;
                }), System.Windows.Threading.DispatcherPriority.Render);

                // ask driver to highlight in the actual app as well (non-blocking)
                try
                {
                    var drv = mWindowExplorerDriver;
                    if (drv != null)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                drv.HighLightElement(clickedElementInfo, locateElementByItLocators: true);
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "Driver HighLightElement failed", ex);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Request to driver highlight element failed", ex);
                }
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
                mScreenShotViewPage.xHighlighterBorder.Visibility = Visibility.Collapsed;
            }
        }

        private async void ViewsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }

                if (WindowComboboxRow.Height.Value == 0)
                {
                    WindowComboboxRow.Height = new GridLength(50);
                }

                if (xViewsTabs.SelectedItem == xTreeViewTab)
                {
                    await TreeViewTab_Selected(sender, e);
                }
                else if (xViewsTabs.SelectedItem == xScreenShotViewTab)
                {
                    ScreenShotViewTab_Selected(sender, e);
                }
                else if (xViewsTabs.SelectedItem == xPageSrcTab)
                {
                    await PageSrcTab_Selected(sender, e);
                }
                else if (xViewsTabs.SelectedItem == xGridViewTab)
                {
                    await GridViewTab_Selected(sender, e);
                }
                else if (xViewsTabs.SelectedItem == xLiveSpyTab)
                {
                    WindowComboboxRow.Height = new GridLength(0);
                    LiveSpyTab_Selected(sender, e);
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
        /// <summary>
        /// Flag to identify if the screenshot was taken means page was changed
        /// hence need to update the page source document on the driver
        /// </summary>
        bool LoadPageSourceDoc = true;

        private void xRefreshCurrentTabContentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!LastSearchFinished)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done until previous request is finished.");
                return;
            }

            xUCElementDetails.SelectedElement = null;
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
                await RefreshGridContent();

                RefreshGrid = false;
            }
            else if (xViewsTabs.SelectedItem == xTreeViewTab)        /// Tree View
            {
                await RefreshTreeContent();

                RefreshTree = false;
            }
            else if (xViewsTabs.SelectedItem == xPageSrcTab)
            {
                await RefreshPageSrcContent();
                RefreshPageSrc = false;
            }
        }

        private async Task RefreshGridContent()
        {
            if (xWindowControlsGridView.DataSourceList == null || xWindowControlsGridView.DataSourceList.Count == 0)
            {
                ShowFilterElementsPage();
            }
            else
            {
                await RefreshFilteredElements();
            }
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

        private async Task GridViewTab_Selected(object sender, RoutedEventArgs e)
        {
            if (RefreshGrid)
            {
                await RefreshGridContent();
                RefreshGrid = false;
            }
        }

        private async Task TreeViewTab_Selected(object sender, RoutedEventArgs e)
        {
            if (RefreshTree)
            {
                await RefreshTreeContent();
                RefreshTree = false;
            }
        }

        private void ScreenShotViewTab_Selected(object sender, RoutedEventArgs e)
        {
            if (RefreshScreenshot)
            {
                ShowScreenShot();
                RefreshScreenshot = false;
            }
        }

        private async Task PageSrcTab_Selected(object sender, RoutedEventArgs e)
        {
            if (RefreshPageSrc)
            {
                await RefreshPageSrcContent();
                RefreshPageSrc = false;
            }
        }

        LiveSpyPage spyPage;
        private void LiveSpyTab_Selected(object sender, SelectionChangedEventArgs e)
        {
            xUCElementDetails.SelectedElement = null;
            if (spyPage == null)
            {
                if (mWindowExplorerDriver == null)
                {
                    mWindowExplorerDriver = (IWindowExplorer)((AgentOperations)mContext.Agent.AgentOperations).Driver;
                }

                spyPage = new LiveSpyPage(mContext, mWindowExplorerDriver);
            }

            if (!xLiveSpyTabContentFrame.HasContent || xLiveSpyTabContentFrame.Content == null)
            {
                xLiveSpyTabContentFrame.ClearAndSetContent(spyPage);
            }
        }

        private void xRetryScreenshotBtn_Click(object sender, RoutedEventArgs e)
        {
            xRetrySSBanner.Visibility = Visibility.Collapsed;
            ShowScreenShot();
        }

        private void xScreenShotViewTab_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void xScreenShotViewTab_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void xViewsTabs_MouseEnter(object sender, MouseEventArgs e)
        {
            xStatusBarText.Visibility = Visibility.Visible;
            xStatusBarIcon.Visibility = Visibility.Visible;
        }

        private void xViewsTabs_MouseLeave(object sender, MouseEventArgs e)
        {
            xStatusBarText.Visibility = Visibility.Collapsed;
            xStatusBarIcon.Visibility = Visibility.Collapsed;
        }

        private void xCopyPageSrc_Click(object sender, RoutedEventArgs e)
        {
            GingerCore.General.SetClipboardText(mWindowExplorerDriver.GetCurrentPageSourceString());
        }
    }
}
