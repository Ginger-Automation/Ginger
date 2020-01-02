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
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions.Locators.ASCF;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Mainframe;
using GingerCore.Actions.UIAutomation;
using GingerCore.DataSource;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms.PlatformsInfo;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCWindowsGrid.xaml
    /// </summary>
    public partial class UCWindowsGrid : UserControl
    {
        IWindowExplorer windowExplorerDriver;
        public IWindowExplorer mWindowExplorerDriver {
            get
            {
                return windowExplorerDriver;
            }
            set
            {
                windowExplorerDriver = value;
                UpdateWindowsList();
            }
        }

        //TreeViewItem mTreeRootItem;
        //ITreeViewItem mRootItem;
        //TreeView2 WindowControlsTreeView;
        //ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("mContext", typeof(Context), typeof(UCWindowsGrid));
        public object SelectedWindow = null;
        public PlatformInfoBase mPlatform;

        public Context mContext
        {
            get { return GetValue(ContextProperty) as Context; }
            set { SetValue(ContextProperty, value);  }
        }

        public bool AddSwitchActionRequired
        {
            set
            {
                if (value == true)
                {
                    xAddSwitchWindowActionButton.Visibility = Visibility.Visible;
                }
                else if (value == false)
                {
                    xAddSwitchWindowActionButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public UCWindowsGrid()
        {
            InitializeComponent();
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedWindow = WindowsComboBox.SelectedItem;
            AppWindow AW = (AppWindow)WindowsComboBox.SelectedItem;
            if (AW == null)
                return;

            mWindowExplorerDriver.SwitchWindow(AW.Title);

            //switch (AW.WindowType)
            //{
            //    case AppWindow.eWindowType.Windows:
            //        WindowsWindowTreeItem WWTI = new WindowsWindowTreeItem();
            //        UIAElementInfo WEI = (UIAElementInfo)AW.RefObject;
            //        WEI.WindowExplorer = mWindowExplorerDriver;
            //        WWTI.UIAElementInfo = WEI;
            //        WWTI.UIAElementInfo.ElementObject = WEI.ElementObject;
            //        InitTree(WWTI);
            //        break;

            //    case AppWindow.eWindowType.PowerBuilder:
            //        PBWindowTreeItem WTI = new PBWindowTreeItem();
            //        UIAElementInfo PBEI = (UIAElementInfo)AW.RefObject;
            //        PBEI.WindowExplorer = mWindowExplorerDriver;
            //        WTI.UIAElementInfo = PBEI;
            //        InitTree(WTI);
            //        break;

            //    case AppWindow.eWindowType.ASCFForm:
            //        ASCFFormTreeItem AFTI = new ASCFFormTreeItem();
            //        AFTI.Name = AW.Title;
            //        AFTI.Path = AW.Path;
            //        InitTree(AFTI);
            //        break;

            //    case AppWindow.eWindowType.SeleniumWebPage:
            //        HTMLPageTreeItem HPTI = new HTMLPageTreeItem();
            //        HTMLElementInfo EI = new HTMLElementInfo();
            //        EI.ElementTitle = AW.Title;
            //        EI.XPath = "/html";
            //        EI.WindowExplorer = mWindowExplorerDriver;
            //        HPTI.ElementInfo = EI;
            //        InitTree(HPTI);
            //        break;

            //    case AppWindow.eWindowType.JFrmae:
            //        JavaWindowTreeItem JWTI = new JavaWindowTreeItem();
            //        JavaElementInfo JEI = new JavaElementInfo();
            //        JEI.ElementTitle = AW.Title;
            //        JEI.Path = AW.Title;
            //        JEI.XPath = "/";
            //        JEI.IsExpandable = true;
            //        JWTI.JavaElementInfo = JEI;
            //        JEI.WindowExplorer = mWindowExplorerDriver;
            //        InitTree(JWTI);
            //        break;

            //    case AppWindow.eWindowType.Appium:
            //        AppiumWindowTreeItem AWTI = new AppiumWindowTreeItem();
            //        AppiumElementInfo AEI = new AppiumElementInfo();
            //        AEI.WindowExplorer = mWindowExplorerDriver;
            //        AEI.XPath = "/";
            //        SeleniumAppiumDriver SAD = ((SeleniumAppiumDriver)mWindowExplorerDriver);
            //        string pageSourceString = SAD.GetPageSource().Result;
            //        XmlDocument pageSourceXml = new XmlDocument();
            //        pageSourceXml.LoadXml(pageSourceString);
            //        AEI.XmlDoc = pageSourceXml;
            //        AEI.XmlNode = pageSourceXml.SelectSingleNode("/");
            //        AWTI.AppiumElementInfo = AEI;
            //        InitTree(AWTI);
            //        break;

            //    case AppWindow.eWindowType.Mainframe:
            //        MainframeTreeItemBase MFTI = new MainframeTreeItemBase();
            //        MFTI.Name = AW.Title;
            //        MFTI.Path = AW.Path;
            //        InitTree(MFTI);
            //        break;

            //    default:
            //        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unknown Window type:" + AW.WindowType);
            //        break;
            //}
        }

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowsList();
        }

        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowsComboBox.SelectedValue != null)
            {
                WindowExplorerCommon.CreateActUISwitchWindowAction((WindowsComboBox.SelectedValue as AppWindow).Title,mContext);
            }
        }

        public void UpdateWindowsList()
        {            
            try
            {
                if (mWindowExplorerDriver != null)
                {
                    List<AppWindow> list = mWindowExplorerDriver.GetAppWindows();
                    WindowsComboBox.ItemsSource = list;
                    WindowsComboBox.DisplayMemberPath = "WinInfo";

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


        //public void InitTree(ITreeViewItem RootItem)
        //{
        //    if(WindowControlsTreeView != null)
        //        WindowControlsTreeView.Tree.ClearTreeItems();
        //    mRootItem = RootItem;

        //    mTreeRootItem = WindowControlsTreeView != null ? WindowControlsTreeView.Tree.AddItem(RootItem) : null;
        //    if(mTreeRootItem != null)
        //        mTreeRootItem.IsExpanded = false;
        //}
    }
}
