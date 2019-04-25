using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions.Locators.ASCF;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.WindowExplorer.Android;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Mainframe;
using GingerCore;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerWPF.UserControlsLib.UCTreeView;
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
using System.Xml;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for RecordNavAction.xaml
    /// </summary>
    public partial class RecordNavPage : Page
    {
        bool isRecording = false;
        IWindowExplorer mWindowExplorerDriver;
        private Activity mActParentActivity = null;
        Context mContext;
        public RecordNavPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            xWinGridUC.mContext = mContext;
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            isRecording = !isRecording;
            UpdateUI();
            //stop Live Spy
            //if (LiveSpyButton.IsChecked == true)
            //    if (dispatcherTimer != null)
            //        dispatcherTimer.IsEnabled = false;

            if (xWinGridUC.comboBoxSelectedValue != null)
            {
                if (mWindowExplorerDriver == null)
                    mWindowExplorerDriver = xWinGridUC.mWindowExplorerDriver;
                //if (RecordingButton.IsChecked == true)
                if (isRecording)
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
                //isRecording = false;
                Reporter.ToUser(eUserMsgKey.TargetWindowNotSelected);
            }
        }

        private void StartRecording()
        {
            mWindowExplorerDriver.SwitchWindow(((AppWindow)xWinGridUC.comboBoxSelectedValue).Title);
            ((DriverBase)mWindowExplorerDriver).StartRecording();
        }

        private void StopRecording()
        {
            ((DriverBase)mWindowExplorerDriver).StopRecording();
        }

        private void UpdateUI()
        {
            if (isRecording)
            {
                startRecord.Visibility = Visibility.Collapsed;
                stopRecord.Visibility = Visibility.Visible;
                lblRecording.Content = RecordingButton.ToolTip = "Stop Recording";
                RecordingButton.Background = Brushes.White;
                lblRecording.Foreground = Brushes.DeepSkyBlue;
            }
            else
            {
                stopRecord.Visibility = Visibility.Collapsed;
                startRecord.Visibility = Visibility.Visible;
                lblRecording.Content = RecordingButton.ToolTip = "Start Recording";
                RecordingButton.Background = Brushes.DeepSkyBlue;
                lblRecording.Foreground = Brushes.White;
            }
        }

        //private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    AppWindow AW = (AppWindow)WindowsComboBox.SelectedItem;
        //    if (AW == null) return;
        //    mWindowExplorerDriver.SwitchWindow(AW.Title);
        //    RecordingButton.IsEnabled = true;

        //    switch (AW.WindowType)
        //    {
        //        case AppWindow.eWindowType.Windows:
        //            WindowsWindowTreeItem WWTI = new WindowsWindowTreeItem();
        //            UIAElementInfo WEI = (UIAElementInfo)AW.RefObject;
        //            WEI.WindowExplorer = mWindowExplorerDriver;
        //            WWTI.UIAElementInfo = WEI;
        //            WWTI.UIAElementInfo.ElementObject = WEI.ElementObject;

        //            InitTree(WWTI);
        //            break;
        //        case AppWindow.eWindowType.PowerBuilder:
        //            PBWindowTreeItem WTI = new PBWindowTreeItem();
        //            UIAElementInfo PBEI = (UIAElementInfo)AW.RefObject;
        //            PBEI.WindowExplorer = mWindowExplorerDriver;
        //            WTI.UIAElementInfo = PBEI;
        //            InitTree(WTI);
        //            break;
        //        case AppWindow.eWindowType.ASCFForm:
        //            ASCFFormTreeItem AFTI = new ASCFFormTreeItem();
        //            AFTI.Name = AW.Title;
        //            AFTI.Path = AW.Path;
        //            InitTree(AFTI);
        //            break;
        //        case AppWindow.eWindowType.SeleniumWebPage:
        //            HTMLPageTreeItem HPTI = new HTMLPageTreeItem();
        //            HTMLElementInfo EI = new HTMLElementInfo();
        //            EI.ElementTitle = AW.Title;
        //            EI.XPath = "/html";
        //            EI.WindowExplorer = mWindowExplorerDriver;
        //            HPTI.ElementInfo = EI;
        //            InitTree(HPTI);
        //            break;
        //        case AppWindow.eWindowType.JFrmae:
        //            JavaWindowTreeItem JWTI = new JavaWindowTreeItem();
        //            JavaElementInfo JEI = new JavaElementInfo();
        //            JEI.ElementTitle = AW.Title;
        //            JEI.Path = AW.Title;
        //            JEI.XPath = "/";
        //            JEI.IsExpandable = true;
        //            JWTI.JavaElementInfo = JEI;
        //            JEI.WindowExplorer = mWindowExplorerDriver;
        //            InitTree(JWTI);
        //            break;
        //        case AppWindow.eWindowType.Appium:
        //            AppiumWindowTreeItem AWTI = new AppiumWindowTreeItem();

        //            AppiumElementInfo AEI = new AppiumElementInfo();
        //            AEI.WindowExplorer = mWindowExplorerDriver;
        //            AEI.XPath = "/";
        //            SeleniumAppiumDriver SAD = ((SeleniumAppiumDriver)mWindowExplorerDriver);


        //            string pageSourceString = SAD.GetPageSource().Result;
        //            XmlDocument pageSourceXml = new XmlDocument();
        //            pageSourceXml.LoadXml(pageSourceString);
        //            AEI.XmlDoc = pageSourceXml;
        //            AEI.XmlNode = pageSourceXml.SelectSingleNode("/");

        //            AWTI.AppiumElementInfo = AEI;

        //            // AWTI.UIAElementInfo = AEI;
        //            InitTree(AWTI);
        //            break;
        //        case AppWindow.eWindowType.AndroidDevice:
        //            AndroidWindowTreeItem ADTI = new AndroidWindowTreeItem();

        //            AndroidElementInfo AWI = new AndroidElementInfo();
        //            AWI.WindowExplorer = mWindowExplorerDriver;
        //            AWI.XPath = "/";
        //            string pageSourceString2 = ((AndroidADBDriver)mWindowExplorerDriver).GetPageSource();
        //            XmlDocument pageSourceXml2 = new XmlDocument();
        //            pageSourceXml2.LoadXml(pageSourceString2);
        //            AWI.XmlDoc = pageSourceXml2;
        //            AWI.XmlNode = pageSourceXml2.SelectSingleNode("/hierarchy");

        //            ADTI.AndroidElementInfo = AWI;
        //            InitTree(ADTI);
        //            break;
        //        case AppWindow.eWindowType.Mainframe:
        //            MainframeTreeItemBase MFTI = new MainframeTreeItemBase();
        //            MFTI.Name = AW.Title;
        //            MFTI.Path = AW.Path;
        //            InitTree(MFTI);
        //            break;
        //        default:
        //            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unknown Window type:" + AW.WindowType);
        //            break;
        //    }

        //    //if (WindowControlsGridView.Visibility == System.Windows.Visibility.Visible)
        //    //{
        //    //    RefreshControlsGrid();
        //    //}
        //}
        //void InitTree(ITreeViewItem RootItem)
        //{
        //    //WindowControlsTreeView.Tree.ClearTreeItems();
        //    //mRootItem = RootItem;

        //    //mTreeRootItem = WindowControlsTreeView.Tree.AddItem(RootItem);
        //    //mTreeRootItem.IsExpanded = false;
        //}

        //private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
