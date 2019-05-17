using amdocs.ginger.GingerCoreNET;
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
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
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

        TreeViewItem mTreeRootItem;
        ITreeViewItem mRootItem;
        TreeView2 WindowControlsTreeView;
        ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("mContext", typeof(Context), typeof(UCWindowsGrid));
        public object comboBoxSelectedValue = null;
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
                    AddSwitchWindowActionButton.Visibility = Visibility.Visible;
                }
                else if (value == false)
                {
                    AddSwitchWindowActionButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public UCWindowsGrid()
        {
            InitializeComponent();
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBoxSelectedValue = WindowsComboBox.SelectedItem;
            AppWindow AW = (AppWindow)WindowsComboBox.SelectedItem;
            if (AW == null)
                return;

            //Reporter.ToUser(eUserMsgKey.)
            mWindowExplorerDriver.SwitchWindow(AW.Title);
            //RecordingButton.IsEnabled = true;

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
                case AppWindow.eWindowType.AndroidDevice:
                    AndroidWindowTreeItem ADTI = new AndroidWindowTreeItem();

                    AndroidElementInfo AWI = new AndroidElementInfo();
                    AWI.WindowExplorer = mWindowExplorerDriver;
                    AWI.XPath = "/";
                    string pageSourceString2 = ((AndroidADBDriver)mWindowExplorerDriver).GetPageSource();
                    XmlDocument pageSourceXml2 = new XmlDocument();
                    pageSourceXml2.LoadXml(pageSourceString2);
                    AWI.XmlDoc = pageSourceXml2;
                    AWI.XmlNode = pageSourceXml2.SelectSingleNode("/hierarchy");

                    ADTI.AndroidElementInfo = AWI;
                    InitTree(ADTI);
                    break;
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
        }

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowsList();
        }

        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowsComboBox.SelectedValue != null)
            {
                mWindowExplorerDriver.AddSwitchWindowAction((WindowsComboBox.SelectedValue as AppWindow).Title);
            }
        }

        public void UpdateWindowsList()
        {
            //if (mContext == null)
            //    return;

            //@AppAgentAct:
            //Activity mActParentActivity = mContext.BusinessFlow.CurrentActivity;
            //ApplicationAgent appAgent = (ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();

            //if(appAgent == null)
            //{
            //    mContext.Runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            //    mContext.Runner.UpdateApplicationAgents();
            //    goto AppAgentAct;
            //}

            //mPlatform = PlatformInfoBase.GetPlatformImpl(appAgent.Agent.Platform);

            //mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            //if (appAgent != null)
            //{
            //    if (appAgent.Agent.Driver == null)
            //    {
            //        appAgent.Agent.DSList = mDSList;
            //        appAgent.Agent.StartDriver();
            //    }
            //    else if(!appAgent.Agent.Driver.IsRunning())
            //    {
            //        if (Reporter.ToUser(eUserMsgKey.PleaseStartAgent, eUserMsgOption.OKCancel, eUserMsgSelection.OK) == eUserMsgSelection.OK)
            //        {
            //            appAgent.Agent.StartDriver();
            //        }
            //        else
            //            return;
            //    }
            //    DriverBase driver = appAgent.Agent.Driver;
            //    if (driver is IWindowExplorer)
            //    {
            //        mWindowExplorerDriver = (IWindowExplorer)appAgent.Agent.Driver;
            //    }
            //}
            try
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
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occured while performing Update Window Explorer List", ex);
            }
        }

        public void StopStartedAgent()
        {
            if (mContext == null)
                return;

            @AppAgentAct:
            Activity mActParentActivity = mContext.BusinessFlow.CurrentActivity;
            ApplicationAgent appAgent = (ApplicationAgent)mContext.Runner.ApplicationAgents.Where(x => x.AppName == mActParentActivity.TargetApplication).FirstOrDefault();
            
            mPlatform = PlatformInfoBase.GetPlatformImpl(appAgent.Agent.Platform);

            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (appAgent != null)
            {
                if (appAgent.Agent.Driver != null && appAgent.Agent.Driver.IsRunning())
                {
                    appAgent.Agent.Close();
                    WindowsComboBox.ItemsSource = new List<Window>();
                    return;
                }
            }
        }

        public void InitTree(ITreeViewItem RootItem)
        {
            if(WindowControlsTreeView != null)
                WindowControlsTreeView.Tree.ClearTreeItems();
            mRootItem = RootItem;

            mTreeRootItem = WindowControlsTreeView != null ? WindowControlsTreeView.Tree.AddItem(RootItem) : null;
            if(mTreeRootItem != null)
                mTreeRootItem.IsExpanded = false;
        }
    }
}
