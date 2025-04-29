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
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.WizardLib;
using GingerWPF.WizardLib;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public abstract class BasePOMWizard : WizardBase
    {
        public PomLearnUtils mPomLearnUtils;
        public ObservableList<Agent> OptionalAgentsList = null;
        public bool IsLearningWasDone { get; set; }

        private bool mManualElementConfiguration;
        public bool ManualElementConfiguration { get { return mManualElementConfiguration; } set { mManualElementConfiguration = value; } }


        public void BindControls(POMLearnConfigWizardPage pOMLearnConfigWizardPage, WizardEventArgs WizardEventArgs)
        {
            ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
            pOMLearnConfigWizardPage.xTargetApplicationComboBox.BindControl<ApplicationPlatform>(this.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
            pOMLearnConfigWizardPage.xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

            if (pOMLearnConfigWizardPage.xTargetApplicationComboBox.Items != null && pOMLearnConfigWizardPage.xTargetApplicationComboBox.Items.Count > 0)
            {
                pOMLearnConfigWizardPage.xTargetApplicationComboBox.SelectedIndex = 0;
            }
            AddValidations(pOMLearnConfigWizardPage);
            ClearAutoMapElementTypesSection(pOMLearnConfigWizardPage);
            SetAutoMapElementTypesGridView(pOMLearnConfigWizardPage);
            
            if (pOMLearnConfigWizardPage.mAppPlatform == ePlatformType.Web)
            {
                pOMLearnConfigWizardPage.isEnableFriendlyLocator = true;
            }
            SetElementLocatorsSettingsGridView(pOMLearnConfigWizardPage);
            UpdateConfigsBasedOnAgentStatus(pOMLearnConfigWizardPage);
            PlatformSpecificUIManipulations(pOMLearnConfigWizardPage);
            pOMLearnConfigWizardPage.xLearnOnlyMappedElements.BindControl(this.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
            pOMLearnConfigWizardPage.xLearnScreenshotsOfElements.BindControl(this.mPomLearnUtils, nameof(PomLearnUtils.LearnScreenshotsOfElements));
            pOMLearnConfigWizardPage.xLearnShadowDOMElements.BindControl(this.mPomLearnUtils, nameof(PomLearnUtils.LearnShadowDomElements));
            
        }

        public void AddValidations(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            pOMLearnConfigWizardPage.xAgentControlUC.AddValidationRule(new AgentControlValidationRule(AgentControlValidationRule.eAgentControlValidationRuleType.AgentIsMappedAndRunning));
        }

        public void ClearAutoMapElementTypesSection(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            this.mPomLearnUtils.AutoMapBasicElementTypesList = [];
            pOMLearnConfigWizardPage.xAutoMapBasicElementTypesGrid.DataSourceList = this.mPomLearnUtils.AutoMapBasicElementTypesList;

            this.mPomLearnUtils.AutoMapAdvanceElementTypesList = [];
            pOMLearnConfigWizardPage.xAutoMapAdvancedlementTypesGrid.DataSourceList = this.mPomLearnUtils.AutoMapAdvanceElementTypesList;
        }


        private void SetAutoMapElementTypesGridView(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            //tool bar
            pOMLearnConfigWizardPage.xAutoMapBasicElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));
            pOMLearnConfigWizardPage.xAutoMapAdvancedlementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllAdvancedElements));

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

            pOMLearnConfigWizardPage.xAutoMapBasicElementTypesGrid.SetAllColumnsDefaultView(view);
            pOMLearnConfigWizardPage.xAutoMapBasicElementTypesGrid.InitViewItems();
            pOMLearnConfigWizardPage.xAutoMapAdvancedlementTypesGrid.SetAllColumnsDefaultView(view);
            pOMLearnConfigWizardPage.xAutoMapAdvancedlementTypesGrid.InitViewItems();
        }

        private void SetElementLocatorsSettingsGridView(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(ElementLocator.EnableFriendlyLocator), Visible = pOMLearnConfigWizardPage.isEnableFriendlyLocator, Header = "Friendly Locator", WidthWeight = 25, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true },
            ]
            };

            pOMLearnConfigWizardPage.xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            pOMLearnConfigWizardPage.xElementLocatorsSettingsGrid.InitViewItems();

            //pOMLearnConfigWizardPage.xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void UpdateConfigsBasedOnAgentStatus(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            if (pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning)
            {
                SetAutoMapElementTypesSection(pOMLearnConfigWizardPage);
                SetElementLocatorsSettingsSection(pOMLearnConfigWizardPage);
            }
            else
            {
                ClearAutoMapElementTypesSection(pOMLearnConfigWizardPage);
            }
            pOMLearnConfigWizardPage.xLearnOnlyMappedElements.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xLearnScreenshotsOfElements.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xLearnShadowDOMElements.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xAutoMapElementTypesExpander.IsExpanded = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xAutoMapElementTypesExpander.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xElementLocatorsSettingsExpander.IsExpanded = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
            pOMLearnConfigWizardPage.xElementLocatorsSettingsExpander.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;

            pOMLearnConfigWizardPage.xSpecificFrameConfigPanel.IsEnabled = pOMLearnConfigWizardPage.xAgentControlUC.AgentIsRunning;
        }

        private void PlatformSpecificUIManipulations(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            if (pOMLearnConfigWizardPage.mAppPlatform.Equals(ePlatformType.Java))
            {
                pOMLearnConfigWizardPage.xSpecificFrameConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                pOMLearnConfigWizardPage.xSpecificFrameConfigPanel.Visibility = Visibility.Collapsed;
            }

            pOMLearnConfigWizardPage.xCustomRelativeXpathTemplateFrame.Visibility = (pOMLearnConfigWizardPage.mAppPlatform.Equals(ePlatformType.Web) || pOMLearnConfigWizardPage.mAppPlatform.Equals(ePlatformType.Mobile)) ? Visibility.Visible : Visibility.Collapsed;

            if (pOMLearnConfigWizardPage.mAppPlatform == ePlatformType.Mobile)
            {
                pOMLearnConfigWizardPage.xAgentControlUC.xAgentConfigsExpanderRow.Height = new GridLength(0);
                pOMLearnConfigWizardPage.xCustomRelativeXpathTemplateFrame.UpdateDefaultTemplate();
            }
            else
            {
                pOMLearnConfigWizardPage.xAgentControlUC.xAgentConfigsExpanderRow.Height = new GridLength(90);
            }
        }

        private void SetAutoMapElementTypesSection(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            pOMLearnConfigWizardPage.xAgentControlUC.xAgentConfigsExpander.Visibility = Visibility.Visible;
            SetAutoMapElementTypes(pOMLearnConfigWizardPage);

            pOMLearnConfigWizardPage.xAutoMapBasicElementTypesGrid.DataSourceList = this.mPomLearnUtils.AutoMapBasicElementTypesList;
            pOMLearnConfigWizardPage.xAutoMapAdvancedlementTypesGrid.DataSourceList = this.mPomLearnUtils.AutoMapAdvanceElementTypesList;

        }

        private void SetAutoMapElementTypes(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
            if (this.mPomLearnUtils.AutoMapBasicElementTypesList.Count == 0 || this.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count == 0)
            {
                var elementList = PlatformInfoBase.GetPlatformImpl(pOMLearnConfigWizardPage.mAppPlatform).GetUIElementFilterList();
                this.mPomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                this.mPomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
            }

        }

        private void SetElementLocatorsSettingsSection(POMLearnConfigWizardPage pOMLearnConfigWizardPage)
        {
                if (this.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
                {
                    this.mPomLearnUtils.ElementLocatorsSettingsList = PlatformInfoBase.GetPlatformImpl(pOMLearnConfigWizardPage.mAppPlatform).GetLearningLocators();
                }
            pOMLearnConfigWizardPage.xElementLocatorsSettingsGrid.DataSourceList = this.mPomLearnUtils.ElementLocatorsSettingsList;
                foreach (ElementLocator elementLocator in this.mPomLearnUtils.ElementLocatorsSettingsList)
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

        private void CheckUnCheckAllAdvancedElements(object sender, RoutedEventArgs e)
        {
            if (this.mPomLearnUtils.AutoMapAdvanceElementTypesList.Count > 0)
            {
                bool valueToSet = !this.mPomLearnUtils.AutoMapAdvanceElementTypesList[0].Selected;
                foreach (UIElementFilter elem in this.mPomLearnUtils.AutoMapAdvanceElementTypesList)
                {
                    elem.Selected = valueToSet;
                }
            }
            
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if (this.mPomLearnUtils.AutoMapBasicElementTypesList.Count > 0)
            {
                bool valueToSet = !this.mPomLearnUtils.AutoMapBasicElementTypesList[0].Selected;
                foreach (UIElementFilter elem in this.mPomLearnUtils.AutoMapBasicElementTypesList)
                {
                    elem.Selected = valueToSet;
                }
            }
            
        }

    }
}
