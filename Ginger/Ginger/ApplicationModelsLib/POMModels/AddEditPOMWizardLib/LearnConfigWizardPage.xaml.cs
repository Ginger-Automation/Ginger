#region License
/*
Copyright © 2014-2018 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.Locators.ASCF;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.UserControls;
using Ginger.WindowExplorer;
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
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using static GingerCore.Agent;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class LearnConfigWizardPage : Page, IWizardPage
    {
        private AddPOMWizard mWizard;

        

        public LearnConfigWizardPage()
        {
            InitializeComponent();
           
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {

            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    
                    Init();
                    break;
                case EventType.LeavingForNextPage:
                    mWizard.CheckedFilteringCreteriaList = GingerCore.General.ConvertListToObservableList(FilteringCreteriaList.Where(x=>x.Selected).ToList());
                    mWizard.mAgent = mAgent;
                    mWizard.WinExplorer = mWindowExplorerDriver;
                    break;
            }
        }

        private void SetControlsGridView()
        {

            xFilterElementsGridView.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllElements));

            xFilterElementsGridView.ShowTitle = Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            //view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["IsSelectedTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100 });

            xFilterElementsGridView.SetAllColumnsDefaultView(view);
            xFilterElementsGridView.InitViewItems();
        }

        private void CheckUnCheckAllElements(object sender, RoutedEventArgs e)
        {
            IObservableList filteringCriteriaList = xFilterElementsGridView.DataSourceList;


            int selectedItems = CountSelectedItems();
            if (selectedItems < xFilterElementsGridView.DataSourceList.Count)
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)
                    UIEFActual.Selected = true;
            else if (selectedItems == xFilterElementsGridView.DataSourceList.Count)
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)
                    UIEFActual.Selected = false;

            xFilterElementsGridView.DataSourceList = filteringCriteriaList;
        }

        private int CountSelectedItems()
        {
            int counter = 0;
            foreach (UIElementFilter UIEFActual in xFilterElementsGridView.DataSourceList)
            {
                if (UIEFActual.Selected)
                    counter++;
            }
            return counter;
        }

        private void Init()
        {
            SetControlsGridView();
            ObservableList<Agent> optionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web select x).ToList());
            xAgentComboBox.BindControl<Agent>(mWizard.POM, nameof(ApplicationPOMModel.Guid), optionalAgentsList, nameof(mAgent.Name), nameof(mAgent.Key), BindingMode.OneWay);
            xFilterElementsGridView.DataSourceList = FilteringCreteriaList;
        }

        private void StartAgentButton_Click(object sender, RoutedEventArgs e)
        {

            if (mAgent != null && mAgent.Status == Agent.eStatus.NotStarted)
                StartAppAgent();
        }

        private void StartAppAgent()
        {
            AutoLogProxy.UserOperationStart("StartAgentButton_Click");
            Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgent, null, mAgent.Name, "AppName"); //Yuval: change app name to be taken from current app
            if (mAgent.Status != Agent.eStatus.Running)
            {
                mAgent.StartDriver();
                mWindowExplorerDriver = (IWindowExplorer)mAgent.Driver;
                UpdatePagesList();
            }

            Reporter.CloseGingerHelper();
            AutoLogProxy.UserOperationEnd();
            
        }

        ObservableList<UIElementFilter> FilteringCreteriaList = new ObservableList<UIElementFilter>();
        IWindowExplorer mWindowExplorerDriver;

        public Agent mAgent { get; set; }

        private void AgentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mWizard.IsLearningWasDone = false;

            mAgent = (Agent)xAgentComboBox.SelectedItem;
            //TODO: move to Init()
            CloseAgentButton.DataContext = this;
            LoadingAgentButton.DataContext = this;
            xStartAgentButton.DataContext = this;
            if (mAgent != null)
            {
                if (mAgent.Status == eStatus.NotStarted || mAgent.Status == eStatus.FailedToStart)
                {
                    switch (mAgent.DriverType)
                    {
                        case eDriverType.SeleniumChrome:
                            mAgent.Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
                            break;
                    }

                }

                mWindowExplorerDriver = (IWindowExplorer)mAgent.Driver;
                mWizard.WinExplorer = mWindowExplorerDriver;
                ObservableList<UIElementFilter> DriverFilteringCreteriaList = mWindowExplorerDriver.GetFilteringCreteriaDict();
                foreach (UIElementFilter filter in DriverFilteringCreteriaList)
                    FilteringCreteriaList.Add(filter);

                UpdatePagesList();
            }

        }

        private void LoadingAgentButton_Click(object sender, RoutedEventArgs e)
        {
            mAgent.Driver.cancelAgentLoading = true;
        }

        private void CloseAgentButton_Click(object sender, RoutedEventArgs e)
        {
            mAgent.Close();
        }

        private void IsSelected_FieldSelection_Click(object sender, RoutedEventArgs e)
        {
            mWizard.IsLearningWasDone = false;
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppWindow AW = (AppWindow)xAgentPageComboBox.SelectedItem;
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
                    break;
                case AppWindow.eWindowType.PowerBuilder:
                    PBWindowTreeItem WTI = new PBWindowTreeItem();
                    UIAElementInfo PBEI = (UIAElementInfo)AW.RefObject;
                    PBEI.WindowExplorer = mWindowExplorerDriver;
                    WTI.UIAElementInfo = PBEI;
                    break;
                case AppWindow.eWindowType.ASCFForm:
                    ASCFFormTreeItem AFTI = new ASCFFormTreeItem();
                    AFTI.Name = AW.Title;
                    AFTI.Path = AW.Path;
                    break;
                case AppWindow.eWindowType.SeleniumWebPage:
                    HTMLPageTreeItem HPTI = new HTMLPageTreeItem();
                    HTMLElementInfo EI = new HTMLElementInfo();
                    EI.ElementTitle = AW.Title;
                    EI.XPath = "html";
                    EI.WindowExplorer = mWindowExplorerDriver;
                    HPTI.ElementInfo = EI;
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
                    break;
                case AppWindow.eWindowType.Appium:
                    AppiumWindowTreeItem AWTI = new AppiumWindowTreeItem();

                    AppiumElementInfo AEI = new AppiumElementInfo();
                    AEI.WindowExplorer = mWindowExplorerDriver;
                    AEI.XPath = "/";
                    string pageSourceString = ((SeleniumAppiumDriver)mWindowExplorerDriver).GetPageSource();
                    XmlDocument pageSourceXml = new XmlDocument();
                    pageSourceXml.LoadXml(pageSourceString);
                    AEI.XmlDoc = pageSourceXml;
                    AEI.XmlNode = pageSourceXml.SelectSingleNode("/");

                    AWTI.AppiumElementInfo = AEI;
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
                    break;
                case AppWindow.eWindowType.Mainframe:
                    MainframeTreeItemBase MFTI = new MainframeTreeItemBase();
                    MFTI.Name = AW.Title;
                    MFTI.Path = AW.Path;
                    break;
                default:
                    MessageBox.Show("Unknown Window type:" + AW.WindowType);
                    break;
            }
        }

        private void RefreshPagesButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatePagesList();
        }


        private void UpdatePagesList()
        {
            List<AppWindow> list = mWindowExplorerDriver.GetAppWindows();
            xAgentPageComboBox.ItemsSource = list;
            xAgentPageComboBox.DisplayMemberPath = "WinInfo";

            AppWindow ActiveWindow = mWindowExplorerDriver.GetActiveWindow();

            if (list != null && list.Count > 0)
            {
                xAgentPageComboBox.SelectedValue = list[0];
            }

            //if (ActiveWindow != null)
            //{
            //    foreach (AppWindow w in list)
            //    {
            //        if (w.Title == ActiveWindow.Title && w.Path == ActiveWindow.Path)
            //        {
            //            xAgentPageComboBox.SelectedValue = w;
            //            return;
            //        }
            //    }
            //}

            //TODO: If no selection then select the first if only one window exist in list
            if (!(mWindowExplorerDriver is SeleniumAppiumDriver))//FIXME: need to work for all drivers and from some reason failing for Appium!!
            {
                if (xAgentPageComboBox.Items.Count == 1)
                {
                    xAgentPageComboBox.SelectedValue = xAgentPageComboBox.Items[0];
                }
            }
        }
    }
}
