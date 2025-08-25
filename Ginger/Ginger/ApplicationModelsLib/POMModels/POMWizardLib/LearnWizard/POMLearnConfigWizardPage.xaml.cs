#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Agents;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using OpenQA.Selenium;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMLearnConfigWizardPage : Page, IWizardPage
    {
        private BasePOMWizard mBasePOMWizard;
        public ePlatformType mAppPlatform;
        public bool isEnableFriendlyLocator = false;
        private const double AGENT_CONFIGS_ROW_HEIGHT = 90;

        public eImageType IconType { get; set; } = eImageType.GingerPlayLogo;



        public POMLearnConfigWizardPage()
        {
            InitializeComponent();
            this.DataContext = this;
            xTAlabel.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                    mBasePOMWizard = (BasePOMWizard)WizardEventArgs.Wizard;
                    xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mBasePOMWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                    xLearnOnlyMappedElements.BindControl(mBasePOMWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
                    xLearnScreenshotsOfElements.BindControl(mBasePOMWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnScreenshotsOfElements));
                    xLearnShadowDOMElements.BindControl(mBasePOMWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnShadowDomElements));
                    if(WorkSpace.Instance.BetaFeatures.ShowPOMForAI)
                    {
                        xLearnPOMByAI.BindControl(mBasePOMWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnPOMByAI));
                    }
                    xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

                    if (xTargetApplicationComboBox.Items != null && xTargetApplicationComboBox.Items.Count > 0)
                    {
                        xTargetApplicationComboBox.SelectedIndex = 0;
                    }
                    AddValidations();

                    ClearAutoMapElementTypesSection();
                    SetAutoMapElementTypesGridView();
                    
                    if (mAppPlatform == ePlatformType.Web)
                    {
                        isEnableFriendlyLocator = true;
                    }
                    SetElementLocatorsSettingsGridView();
                    UpdateConfigsBasedOnAgentStatus();
                    PlatformSpecificUIManipulations();
                    // Subscribe to SeleniumDriver events for AI processing
                    SubscribeToCurrentSeleniumDriver();
                    break;
                case EventType.LeavingForNextPage:
                    UpdateCustomTemplateList();
                    break;
            }
        }

        private void SubscribeToCurrentSeleniumDriver()
        {
            try
            {
                // Get the agent from the wizard's PomLearnUtils
                Agent currentAgent = mBasePOMWizard?.mPomLearnUtils?.Agent;

                // Alternative: Get from the agent control if available
                if (currentAgent == null && xAgentControlUC != null)
                {
                    currentAgent = xAgentControlUC.SelectedAgent;
                }

                // Check if agent is running and has a driver
                if (currentAgent?.AgentOperations is AgentOperations agentOps && agentOps.Driver is SeleniumDriver seleniumDriver)
                {
                    // Get the wizard window and subscribe to driver events
                    if (mBasePOMWizard?.mWizardWindow is WizardWindow wizardWindow)
                    {
                        wizardWindow.SubscribeToSeleniumDriver(seleniumDriver);
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully subscribed to SeleniumDriver AI processing events");
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "SeleniumDriver not available for AI processing subscription");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error subscribing to SeleniumDriver events", ex);
            }
        }

        private void UpdateCustomTemplateList()
        {
            if (xCustomRelativeXpathTemplateFrame.xCustomRelativeXpathCofigChkBox.IsChecked == true
            && (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile)))
            {
                if (mBasePOMWizard.mPomLearnUtils.POM.PomSetting != null)
                {
                    mBasePOMWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                }
                else
                {
                    PomSetting pomSetting = new PomSetting();
                    pomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                    mBasePOMWizard.mPomLearnUtils.POM.PomSetting = pomSetting;
                }
            }
            else
            {
                if (mBasePOMWizard.mPomLearnUtils.POM.PomSetting != null)
                {
                    mBasePOMWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList.Clear();
                }

            }
        }

        private void PlatformSpecificUIManipulations()
        {
            if (mAppPlatform.Equals(ePlatformType.Java))
            {
                xSpecificFrameConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xSpecificFrameConfigPanel.Visibility = Visibility.Collapsed;
            }

            xCustomRelativeXpathTemplateFrame.Visibility = (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile)) ? Visibility.Visible : Visibility.Collapsed;

            if (mAppPlatform == ePlatformType.Mobile)
            {
                xAgentControlUC.xAgentConfigsExpanderRow.Height = new GridLength(0);
                xCustomRelativeXpathTemplateFrame.UpdateDefaultTemplate();
            }
            else
            {
                xAgentControlUC.xAgentConfigsExpanderRow.Height = new GridLength(AGENT_CONFIGS_ROW_HEIGHT);
            }
        }

        private void XTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            if (mBasePOMWizard.mPomLearnUtils.POM.TargetApplicationKey != null)
            {
                mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mBasePOMWizard.mPomLearnUtils.POM.TargetApplicationKey);
            }
            else
            {

                if (xTargetApplicationComboBox.SelectedItem is ApplicationPlatform selectedplatform)
                {
                    mAppPlatform = selectedplatform.Platform;
                }
            }
            if (mAppPlatform == ePlatformType.Web)
            {
                xLearnScreenshotsOfElements.Visibility = Visibility.Visible;
                if(WorkSpace.Instance.BetaFeatures.ShowPOMForAI)
                {
                    xLearnPOMByAI.Visibility = Visibility.Visible;
                }
                isEnableFriendlyLocator = true;
            }
            else
            {
                xLearnScreenshotsOfElements.Visibility = Visibility.Collapsed;
                xLearnPOMByAI.Visibility = Visibility.Collapsed;
                isEnableFriendlyLocator = false;
            }
            SetElementLocatorsSettingsGridView();
            mBasePOMWizard.OptionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
            foreach (Agent agent in mBasePOMWizard.OptionalAgentsList)
            {
                if (agent.AgentOperations == null)
                {
                    AgentOperations agentOperations = new AgentOperations(agent);
                    agent.AgentOperations = agentOperations;
                }
                agent.Tag = string.Empty;
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, mBasePOMWizard.mPomLearnUtils, nameof(mBasePOMWizard.mPomLearnUtils.Agent));
            xAgentControlUC.Init(mBasePOMWizard.OptionalAgentsList);
            string allProperties = string.Empty;
            PropertyChangedEventManager.RemoveHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);

            PlatformSpecificUIManipulations();
            AddValidations();
        }

        private void AddValidations()
        {
            xAgentControlUC.AddValidationRule(new AgentControlValidationRule(AgentControlValidationRule.eAgentControlValidationRuleType.AgentIsMappedAndRunning));
        }

        private void RemoveValidations()
        {
            xAgentControlUC.RemoveValidations(ucAgentControl.SelectedAgentProperty);
        }

        private void SetAutoMapElementTypes()
        {
            if (mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count == 0 || mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count == 0)
            {
                var elementList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetUIElementFilterList();
                mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
            }
            
        }



        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapBasicElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));
            xAutoMapAdvancedlementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllAdvancedElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly = true },
                new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true },
            ]
            };

            xAutoMapBasicElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapBasicElementTypesGrid.InitViewItems();
            xAutoMapAdvancedlementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapAdvancedlementTypesGrid.InitViewItems();
        }

        private void CheckUnCheckAllAdvancedElements(object sender, RoutedEventArgs e)
        {
            CheckUnCheckAllElements(mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList);
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            CheckUnCheckAllElements(mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList);
        }

        private void CheckUnCheckAllElements(ObservableList<UIElementFilter> elementsList)
        {
            if (elementsList.Count > 0)
            {
                bool valueToSet = !elementsList.All(elem => elem.Selected);
                foreach (UIElementFilter elem in elementsList)
                {
                    elem.Selected = valueToSet;
                }
            }
        }

        private void SetElementLocatorsSettingsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator), Visible = isEnableFriendlyLocator, Header = "Friendly Locator", WidthWeight = 25, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true },
            ]
            };

            xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            xElementLocatorsSettingsGrid.InitViewItems();

            xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        public Uri ValidateURL(String sURL)
        {
            Uri myurl;
            if (Uri.TryCreate(sURL, UriKind.Absolute, out myurl))
            {
                return myurl;
            }
            return null;
        }

        private void XAgentControlUC_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (mBasePOMWizard is AddPOMFromScreenshotWizard)
            {
                var ucAgentControl = sender as ucAgentControl;

                if (ucAgentControl.SelectedAgent != null)
                {
                    Uri uri = ValidateURL(General.GetFullFilePath(mBasePOMWizard.HtmlFilePath));
                    if (uri != null)
                    {
                        NavigateAgentToHtml(ucAgentControl.SelectedAgent, uri);
                    }
                }
            }
            if (e.PropertyName == nameof(ucAgentControl.AgentIsRunning))
            {
                UpdateConfigsBasedOnAgentStatus();

                // Subscribe to SeleniumDriver when agent becomes available
                if (xAgentControlUC.AgentIsRunning)
                {
                    SubscribeToCurrentSeleniumDriver();
                }
            }

            // Also subscribe when the selected agent changes
            if (e.PropertyName == nameof(ucAgentControl.SelectedAgent) && xAgentControlUC.SelectedAgent != null)
            {
                SubscribeToCurrentSeleniumDriver();
            }
        }
        
        private async void NavigateAgentToHtml(Agent agent, Uri uri)
        {
            try
            {
                if(((AgentOperations)agent.AgentOperations).Driver != null)
                {
                    Act act = new ActBrowserElement
                    {
                        ControlAction = ActBrowserElement.eControlAction.GotoURL,
                        ValueForDriver = uri.AbsoluteUri,
                    };

                    act.GetOrCreateInputParam(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

                    await Task.Run(() =>
                        ((AgentOperations)agent.AgentOperations)
                            .Driver.RunAction(act));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error to load url: {ex.Message}", ex);
            }
        }


        private void UpdateConfigsBasedOnAgentStatus()
        {
            if (xAgentControlUC.AgentIsRunning)
            {
                SetAutoMapElementTypesSection();
                SetElementLocatorsSettingsSection();
            }
            else
            {
                ClearAutoMapElementTypesSection();
            }
            xLearnOnlyMappedElements.IsEnabled = xAgentControlUC.AgentIsRunning;
            xLearnScreenshotsOfElements.IsEnabled = xAgentControlUC.AgentIsRunning;
            xLearnShadowDOMElements.IsEnabled = xAgentControlUC.AgentIsRunning;
            xAutoMapElementTypesExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xAutoMapElementTypesExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
            xSpecificFrameConfigPanel.IsEnabled = xAgentControlUC.AgentIsRunning;
            xLearnPOMByAI.IsEnabled = xAgentControlUC.AgentIsRunning;

        }

        private void ClearAutoMapElementTypesSection()
        {
            mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList = [];
            xAutoMapBasicElementTypesGrid.DataSourceList = mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList;

            mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = [];
            xAutoMapAdvancedlementTypesGrid.DataSourceList = mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
        }

        private void SetAutoMapElementTypesSection()
        {
            xAgentControlUC.xAgentConfigsExpander.Visibility = Visibility.Visible;
            SetAutoMapElementTypes();
            xAutoMapBasicElementTypesGrid.DataSourceList = mBasePOMWizard.mPomLearnUtils.AutoMapBasicElementTypesList;
            xAutoMapAdvancedlementTypesGrid.DataSourceList = mBasePOMWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;

        }

        private void SetElementLocatorsSettingsSection()
        {
            if (mBasePOMWizard.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
            {
                mBasePOMWizard.mPomLearnUtils.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetLearningLocators();
            }
            xElementLocatorsSettingsGrid.DataSourceList = mBasePOMWizard.mPomLearnUtils.ElementLocatorsSettingsList;
            foreach (ElementLocator elementLocator in mBasePOMWizard.mPomLearnUtils.ElementLocatorsSettingsList)
            {
                string allProperties = string.Empty;
                PropertyChangedEventManager.RemoveHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ElementLocator solutionItem = (ElementLocator)sender;
            if (!solutionItem.Active && solutionItem.EnableFriendlyLocator)
            {
                solutionItem.EnableFriendlyLocator = false;
            }
        }


        private void xAutomaticElementConfigurationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mBasePOMWizard != null)
            {
                if ((bool)xManualElementConfigurationRadioButton.IsChecked)
                {
                    mBasePOMWizard.ManualElementConfiguration = true;
                    RemoveValidations();
                    xLearningConfigsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mBasePOMWizard.ManualElementConfiguration = false;
                    AddValidations();
                    xLearningConfigsPnl.Visibility = Visibility.Visible;
                }
            }
        }

        private void xLearnSpecificFrameChkBox_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(xLearnSpecificFrameChkBox.IsChecked))
            {
                xFrameListGrid.Visibility = Visibility.Visible;
                BindWindowFrameCombox();
            }
            else
            {
                xFrameListGrid.Visibility = Visibility.Collapsed;
                mBasePOMWizard.mPomLearnUtils.SpecificFramePath = null;
            }

        }

        private void BindWindowFrameCombox()
        {
            mBasePOMWizard.mPomLearnUtils.SpecificFramePath = null;
            if (mAppPlatform.Equals(ePlatformType.Java))
            {
                var windowExplorerDriver = ((IWindowExplorer)(((AgentOperations)mBasePOMWizard.mPomLearnUtils.Agent.AgentOperations).Driver));

                var list = windowExplorerDriver.GetWindowAllFrames();
                xFrameListCmbBox.ItemsSource = list;
                xFrameListCmbBox.DisplayMemberPath = nameof(AppWindow.Title);
            }
        }

        private void xFrameListCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (AppWindow)xFrameListCmbBox.SelectedItem;
            if (selectedItem != null)
            {
                mBasePOMWizard.mPomLearnUtils.SpecificFramePath = selectedItem.Path;
            }

        }

        private void xFrameRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            BindWindowFrameCombox();
        }
    }
}
