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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
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
        private ePlatformType mAppPlatform;
        public bool isEnableFriendlyLocator = false;
        public POMLearnConfigWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                    xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                    xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

                    if (xTargetApplicationComboBox.Items != null && xTargetApplicationComboBox.Items.Count > 0)
                    {
                        xTargetApplicationComboBox.SelectedIndex = 0;
                    }

                    AddValidations();

                    ClearAutoMapElementTypesSection();
                    SetAutoMapElementTypesGridView();
                    xLearnOnlyMappedElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
                    xLearnScreenshotsOfElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnScreenshotsOfElements));
                    if(mAppPlatform == ePlatformType.Web)
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
            if (xCustomRelativeXpathTemplateFrame.xCustomRelativeXpathCofigChkBox.IsChecked == true
                && (mAppPlatform.Equals(ePlatformType.Web) || mAppPlatform.Equals(ePlatformType.Mobile)))
            {
                mWizard.mPomLearnUtils.POM.RelativeXpathTemplateList = new ObservableList<CustomRelativeXpathTemplate>(xCustomRelativeXpathTemplateFrame.RelativeXpathTemplateList.Where(x => x.Status == CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed));
            }
            else
            {
                mWizard.mPomLearnUtils.POM.RelativeXpathTemplateList.Clear();
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
            xAgentControlUC.PropertyChanged -= XAgentControlUC_PropertyChanged;
            xAgentControlUC.PropertyChanged += XAgentControlUC_PropertyChanged;

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
            if (mWizard.mPomLearnUtils.AutoMapBasicElementTypesList.Count == 0 || mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count == 0)
            {
                var elementList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetUIElementFilterList();
                mWizard.mPomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
            }
        }



        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapBasicElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));
            xAutoMapAdvancedlementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllAdvancedElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true });

            xAutoMapBasicElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapBasicElementTypesGrid.InitViewItems();
            xAutoMapAdvancedlementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapAdvancedlementTypesGrid.InitViewItems();
        }

        private void CheckUnCheckAllAdvancedElements(object sender, RoutedEventArgs e)
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

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
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

        private void SetElementLocatorsSettingsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator),Visible = isEnableFriendlyLocator , Header = "Friendly Locator", WidthWeight = 25, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true });

            xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            xElementLocatorsSettingsGrid.InitViewItems();

            xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void XAgentControlUC_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ucAgentControl.AgentIsRunning))
            {
                UpdateConfigsBasedOnAgentStatus();
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
            xAutoMapElementTypesExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xAutoMapElementTypesExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsEnabled = xAgentControlUC.AgentIsRunning;

            xSpecificFrameConfigPanel.IsEnabled = xAgentControlUC.AgentIsRunning;
        }

        private void ClearAutoMapElementTypesSection()
        {
            mWizard.mPomLearnUtils.AutoMapBasicElementTypesList = new ObservableList<UIElementFilter>();
            xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapBasicElementTypesList;

            mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList = new ObservableList<UIElementFilter>();
            xAutoMapAdvancedlementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
        }

        private void SetAutoMapElementTypesSection()
        {
            xAgentControlUC.xAgentConfigsExpander.Visibility = Visibility.Visible;

            SetAutoMapElementTypes();
            xAutoMapBasicElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapBasicElementTypesList;
            xAutoMapAdvancedlementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapAdvanceElementTypesList;
        }

        private void SetElementLocatorsSettingsSection()
        {
            if (mWizard.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
            {
                mWizard.mPomLearnUtils.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(mAppPlatform).GetLearningLocators();
            }
            xElementLocatorsSettingsGrid.DataSourceList = mWizard.mPomLearnUtils.ElementLocatorsSettingsList;
            foreach (ElementLocator elementLocator in mWizard.mPomLearnUtils.ElementLocatorsSettingsList)
            {
                elementLocator.PropertyChanged -= Item_PropertyChanged;
                elementLocator.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ElementLocator solutionItem = (ElementLocator)sender;
            if(!solutionItem.Active && solutionItem.EnableFriendlyLocator)
            {
                solutionItem.EnableFriendlyLocator = false;
            }
        }


        private void xAutomaticElementConfigurationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                if ((bool)xManualElementConfigurationRadioButton.IsChecked)
                {
                    mWizard.ManualElementConfiguration = true;
                    RemoveValidations();
                    //xAgentControlUC.Visibility = Visibility.Hidden;
                    //xAutoMapElementTypesExpander.Visibility = Visibility.Hidden;
                    //xElementLocatorsSettingsExpander.Visibility = Visibility.Collapsed;
                    xLearningConfigsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mWizard.ManualElementConfiguration = false;
                    AddValidations();
                    //xAgentControlUC.Visibility = Visibility.Visible;
                    //xAutoMapElementTypesExpander.Visibility = Visibility.Visible;
                    //xElementLocatorsSettingsExpander.Visibility = Visibility.Visible;
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
                mWizard.mPomLearnUtils.SpecificFramePath = null;
            }
        }

        private void BindWindowFrameCombox()
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

        private void xFrameListCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (AppWindow)xFrameListCmbBox.SelectedItem;
            if (selectedItem != null)
            {
                mWizard.mPomLearnUtils.SpecificFramePath = selectedItem.Path;
            }
        }

        private void xFrameRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            BindWindowFrameCombox();
        }
    }
}
