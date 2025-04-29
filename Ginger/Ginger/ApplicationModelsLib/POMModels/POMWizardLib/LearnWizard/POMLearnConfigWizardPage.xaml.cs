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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.WizardLib;
using GingerWPF.WizardLib;
using OpenQA.Selenium;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMLearnConfigWizardPage : Page, IWizardPage
    {
        private AddPOMWizard mWizard;
        private AddPOMFromScreenshotWizard mScreenShotWizard;
        private BasePOMWizard mBasePOMWizard;
        public ePlatformType mAppPlatform;
        public bool isEnableFriendlyLocator = false;
        public bool isCallFromScreenShotPage = false;
        public POMLearnConfigWizardPage(bool isCallFromScreenshot = false)
        {
            isCallFromScreenShotPage = isCallFromScreenshot;
            InitializeComponent();
            xTAlabel.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                    if (isCallFromScreenShotPage)
                    {
                        //mBasePOMWizard = (BasePOMWizard)WizardEventArgs.Wizard;
                        //mBasePOMWizard.mPomLearnUtils = WizardEventArgs.Wizard.mWizardWindow.mPomLearnUtils;
                        //mBasePOMWizard.BindControls(this, WizardEventArgs);
                        mScreenShotWizard = (AddPOMFromScreenshotWizard)WizardEventArgs.Wizard;
                        xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mScreenShotWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                        xLearnOnlyMappedElements.BindControl(mScreenShotWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
                        xLearnScreenshotsOfElements.BindControl(mScreenShotWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnScreenshotsOfElements));
                        xLearnShadowDOMElements.BindControl(mScreenShotWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnShadowDomElements));
                    }
                    else
                    {
                        mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                        xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                        xLearnOnlyMappedElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
                        xLearnScreenshotsOfElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnScreenshotsOfElements));
                        xLearnShadowDOMElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnShadowDomElements));
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
                    break;
                case EventType.LeavingForNextPage:
                    UpdateCustomTemplateList();
                    break;
            }
        }

        private void UpdateCustomTemplateList()
        {
            if(isCallFromScreenShotPage)
            {
                if (xCustomRelativeXpathTemplateFrame.xCustomRelativeXpathCofigChkBox.IsChecked == true
                && (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile)))
                {
                    if (mScreenShotWizard.mPomLearnUtils.POM.PomSetting != null)
                    {
                        mScreenShotWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                    }
                    else
                    {
                        PomSetting pomSetting = new PomSetting();
                        pomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                        mScreenShotWizard.mPomLearnUtils.POM.PomSetting = pomSetting;
                    }
                }
                else
                {
                    if (mScreenShotWizard.mPomLearnUtils.POM.PomSetting != null)
                    {
                        mScreenShotWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList.Clear();
                    }

                }
            }
            else
            {
                if (xCustomRelativeXpathTemplateFrame.xCustomRelativeXpathCofigChkBox.IsChecked == true
                && (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile)))
                {
                    if (mWizard.mPomLearnUtils.POM.PomSetting != null)
                    {
                        mWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                    }
                    else
                    {
                        PomSetting pomSetting = new PomSetting();
                        pomSetting.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
                        mWizard.mPomLearnUtils.POM.PomSetting = pomSetting;
                    }
                }
                else
                {
                    if (mWizard.mPomLearnUtils.POM.PomSetting != null)
                    {
                        mWizard.mPomLearnUtils.POM.PomSetting.RelativeXpathTemplateList.Clear();
                    }

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
                xAgentControlUC.xAgentConfigsExpanderRow.Height = new GridLength(90);
            }
        }

        private void XTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard.mPomLearnUtils.POM.TargetApplicationKey != null)
                {
                    mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mScreenShotWizard.mPomLearnUtils.POM.TargetApplicationKey);
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
                    isEnableFriendlyLocator = true;
                }
                else
                {
                    xLearnScreenshotsOfElements.Visibility = Visibility.Collapsed;
                    isEnableFriendlyLocator = false;
                }
                SetElementLocatorsSettingsGridView();
                mScreenShotWizard.OptionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
                foreach (Agent agent in mScreenShotWizard.OptionalAgentsList)
                {
                    if (agent.AgentOperations == null)
                    {
                        AgentOperations agentOperations = new AgentOperations(agent);
                        agent.AgentOperations = agentOperations;
                    }
                    agent.Tag = string.Empty;
                }
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, mScreenShotWizard.mPomLearnUtils, nameof(mScreenShotWizard.mPomLearnUtils.Agent));
                xAgentControlUC.Init(mScreenShotWizard.OptionalAgentsList);
                string allProperties = string.Empty;
                PropertyChangedEventManager.RemoveHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);

                PlatformSpecificUIManipulations();
                AddValidations();
            }
            else
            {
                if (mWizard.mPomLearnUtils.POM.TargetApplicationKey != null)
                {
                    mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mWizard.mPomLearnUtils.POM.TargetApplicationKey);
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
                    isEnableFriendlyLocator = true;
                }
                else
                {
                    xLearnScreenshotsOfElements.Visibility = Visibility.Collapsed;
                    isEnableFriendlyLocator = false;
                }
                SetElementLocatorsSettingsGridView();
                mWizard.OptionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
                foreach (Agent agent in mWizard.OptionalAgentsList)
                {
                    if (agent.AgentOperations == null)
                    {
                        AgentOperations agentOperations = new AgentOperations(agent);
                        agent.AgentOperations = agentOperations;
                    }
                    agent.Tag = string.Empty;
                }
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, mWizard.mPomLearnUtils, nameof(mWizard.mPomLearnUtils.Agent));
                xAgentControlUC.Init(mWizard.OptionalAgentsList);
                string allProperties = string.Empty;
                PropertyChangedEventManager.RemoveHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: xAgentControlUC, handler: XAgentControlUC_PropertyChanged, propertyName: allProperties);

                PlatformSpecificUIManipulations();
                AddValidations();
            }
            
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
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count == 0 || mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count == 0)
                {
                    var elementList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetUIElementFilterList();
                    mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                    mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
                }
            }
            else
            {
                if (mWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count == 0 || mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count == 0)
                {
                    var elementList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetUIElementFilterList();
                    mWizard.mPomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                    mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
                }
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
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count > 0)
                {
                    bool valueToSet = !mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList[0].Selected;
                    foreach (UIElementFilter elem in mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList)
                    {
                        elem.Selected = valueToSet;
                    }
                }
            }
            else
            {
                if (mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count > 0)
                {
                    bool valueToSet = !mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList[0].Selected;
                    foreach (UIElementFilter elem in mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList)
                    {
                        elem.Selected = valueToSet;
                    }
                }
            }
            
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count > 0)
                {
                    bool valueToSet = !mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList[0].Selected;
                    foreach (UIElementFilter elem in mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList)
                    {
                        elem.Selected = valueToSet;
                    }
                }
            }
            else
            {
                if (mWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count > 0)
                {
                    bool valueToSet = !mWizard.mPomLearnUtils.AutoMapBasicElementTypesList[0].Selected;
                    foreach (UIElementFilter elem in mWizard.mPomLearnUtils.AutoMapBasicElementTypesList)
                    {
                        elem.Selected = valueToSet;
                    }
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
            if (isCallFromScreenShotPage)
            {
                var Agent = sender as ucAgentControl;
                if (Agent.SelectedAgent != null)
                {
                    // Get the row object from the button's DataContext
                    //Agent selectedAgent = 
                    Uri uri = ValidateURL(mScreenShotWizard.HtmlFilePath);
                    if (uri != null)
                    {
                        ((AgentOperations)Agent.SelectedAgent.AgentOperations).Driver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = uri.AbsoluteUri });
                    }

                    if (e.PropertyName == nameof(ucAgentControl.AgentIsRunning))
                    {

                        UpdateConfigsBasedOnAgentStatus();
                    }
                }
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
        }

        private void ClearAutoMapElementTypesSection()
        {
            if(isCallFromScreenShotPage)
            {
                mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList = [];
                xAutoMapBasicElementTypesGrid.DataSourceList = mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList;

                mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = [];
                xAutoMapAdvancedlementTypesGrid.DataSourceList = mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
            }
            else
            {
                mWizard.mPomLearnUtils.AutoMapBasicElementTypesList = [];
                xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapBasicElementTypesList;

                mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = [];
                xAutoMapAdvancedlementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
            }
        }

        private void SetAutoMapElementTypesSection()
        {
            xAgentControlUC.xAgentConfigsExpander.Visibility = Visibility.Visible;
            SetAutoMapElementTypes();
            if(isCallFromScreenShotPage)
            {
                xAutoMapBasicElementTypesGrid.DataSourceList = mScreenShotWizard.mPomLearnUtils.AutoMapBasicElementTypesList;
                xAutoMapAdvancedlementTypesGrid.DataSourceList = mScreenShotWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
            }
            else
            {
                xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapBasicElementTypesList;
                xAutoMapAdvancedlementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
            }
            
        }

        private void SetElementLocatorsSettingsSection()
        {
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
                {
                    mScreenShotWizard.mPomLearnUtils.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetLearningLocators();
                }
                xElementLocatorsSettingsGrid.DataSourceList = mScreenShotWizard.mPomLearnUtils.ElementLocatorsSettingsList;
                foreach (ElementLocator elementLocator in mScreenShotWizard.mPomLearnUtils.ElementLocatorsSettingsList)
                {
                    string allProperties = string.Empty;
                    PropertyChangedEventManager.RemoveHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
                    PropertyChangedEventManager.AddHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
                }
            }
            else
            {
                if (mWizard.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
                {
                    mWizard.mPomLearnUtils.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetLearningLocators();
                }
                xElementLocatorsSettingsGrid.DataSourceList = mWizard.mPomLearnUtils.ElementLocatorsSettingsList;
                foreach (ElementLocator elementLocator in mWizard.mPomLearnUtils.ElementLocatorsSettingsList)
                {
                    string allProperties = string.Empty;
                    PropertyChangedEventManager.RemoveHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
                    PropertyChangedEventManager.AddHandler(source: elementLocator, handler: Item_PropertyChanged, propertyName: allProperties);
                }
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
            if(isCallFromScreenShotPage)
            {
                if (mScreenShotWizard != null)
                {
                    if ((bool)xManualElementConfigurationRadioButton.IsChecked)
                    {
                        mScreenShotWizard.ManualElementConfiguration = true;
                        RemoveValidations();
                        xLearningConfigsPnl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        mScreenShotWizard.ManualElementConfiguration = false;
                        AddValidations();
                        xLearningConfigsPnl.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if (mWizard != null)
                {
                    if ((bool)xManualElementConfigurationRadioButton.IsChecked)
                    {
                        mWizard.ManualElementConfiguration = true;
                        RemoveValidations();
                        xLearningConfigsPnl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        mWizard.ManualElementConfiguration = false;
                        AddValidations();
                        xLearningConfigsPnl.Visibility = Visibility.Visible;
                    }
                }
            }
            
        }

        private void xLearnSpecificFrameChkBox_Click(object sender, RoutedEventArgs e)
        {
            if(isCallFromScreenShotPage)
            {
                if (Convert.ToBoolean(xLearnSpecificFrameChkBox.IsChecked))
                {
                    xFrameListGrid.Visibility = Visibility.Visible;
                    BindWindowFrameCombox();
                }
                else
                {
                    xFrameListGrid.Visibility = Visibility.Collapsed;
                    mScreenShotWizard.mPomLearnUtils.SpecificFramePath = null;
                }
            }
            else
            {
                if (Convert.ToBoolean(xLearnSpecificFrameChkBox.IsChecked))
                {
                    xFrameListGrid.Visibility = Visibility.Visible;
                    BindWindowFrameCombox();
                }
                else
                {
                    xFrameListGrid.Visibility = Visibility.Collapsed;
                    mWizard.mPomLearnUtils.SpecificFramePath = null;
                }
            }
            
        }

        private void BindWindowFrameCombox()
        {
            if(isCallFromScreenShotPage)
            {
                mScreenShotWizard.mPomLearnUtils.SpecificFramePath = null;
                if (mAppPlatform.Equals(ePlatformType.Java))
                {
                    var windowExplorerDriver = ((IWindowExplorer)(((AgentOperations)mScreenShotWizard.mPomLearnUtils.Agent.AgentOperations).Driver));

                    var list = windowExplorerDriver.GetWindowAllFrames();
                    xFrameListCmbBox.ItemsSource = list;
                    xFrameListCmbBox.DisplayMemberPath = nameof(AppWindow.Title);
                }
            }
            else
            {
                mWizard.mPomLearnUtils.SpecificFramePath = null;
                if (mAppPlatform.Equals(ePlatformType.Java))
                {
                    var windowExplorerDriver = ((IWindowExplorer)(((AgentOperations)mWizard.mPomLearnUtils.Agent.AgentOperations).Driver));

                    var list = windowExplorerDriver.GetWindowAllFrames();
                    xFrameListCmbBox.ItemsSource = list;
                    xFrameListCmbBox.DisplayMemberPath = nameof(AppWindow.Title);
                }
            }
            
        }

        private void xFrameListCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(isCallFromScreenShotPage)
            {
                var selectedItem = (AppWindow)xFrameListCmbBox.SelectedItem;
                if (selectedItem != null)
                {
                    mScreenShotWizard.mPomLearnUtils.SpecificFramePath = selectedItem.Path;
                }
            }
            else
            {
                var selectedItem = (AppWindow)xFrameListCmbBox.SelectedItem;
                if (selectedItem != null)
                {
                    mWizard.mPomLearnUtils.SpecificFramePath = selectedItem.Path;
                }
            }
            
        }

        private void xFrameRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            BindWindowFrameCombox();
        }
    }
}
