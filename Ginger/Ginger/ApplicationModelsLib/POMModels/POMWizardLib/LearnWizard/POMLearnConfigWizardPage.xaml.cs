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

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList( WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                    xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.mPomLearnUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                    xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

                    if (xTargetApplicationComboBox.Items != null && xTargetApplicationComboBox.Items.Count > 0)
                    {
                        xTargetApplicationComboBox.SelectedIndex = 0;
                    }

                    if (mWizard.mPomLearnUtils.POM.TargetApplicationKey != null)
                    {
                        mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mWizard.mPomLearnUtils.POM.TargetApplicationKey);
                    }
                    mWizard.OptionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == mAppPlatform select x).ToList());
                    foreach (Agent agent in mWizard.OptionalAgentsList)
                    {
                        agent.Tag = string.Empty;
                    }
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, mWizard.mPomLearnUtils, nameof(mWizard.mPomLearnUtils.Agent));
                    xAgentControlUC.Init(mWizard.OptionalAgentsList);                   
                    xAgentControlUC.PropertyChanged += XAgentControlUC_PropertyChanged;

                    AddValidations();

                    ClearAutoMapElementTypesSection();
                    SetAutoMapElementTypesGridView();
                    xLearnOnlyMappedElements.BindControl(mWizard.mPomLearnUtils, nameof(PomLearnUtils.LearnOnlyMappedElements));
                    SetElementLocatorsSettingsGridView();
                    UpdateConfigsBasedOnAgentStatus();
                    break;
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
            if (mWizard.mPomLearnUtils.AutoMapElementTypesList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in new WebPlatform().GetPlatformElementTypesData().ToList())
                        {
                            mWizard.mPomLearnUtils.AutoMapElementTypesList.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, elementTypeOperation.IsCommonElementType));
                        }
                        break;
                }
            }
        }

        private void SetAutoMapElementTypesGridView()
        {
            //tool bar
            xAutoMapElementTypesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllElements));

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "To Map", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox});           
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100, ReadOnly=true });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100, ReadOnly = true });

            xAutoMapElementTypesGrid.SetAllColumnsDefaultView(view);
            xAutoMapElementTypesGrid.InitViewItems();
        }

        private void SetElementLocatorsSettingsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly=true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true });

            xElementLocatorsSettingsGrid.SetAllColumnsDefaultView(defView);
            xElementLocatorsSettingsGrid.InitViewItems();

            xElementLocatorsSettingsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void CheckUnCheckAllElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mPomLearnUtils.AutoMapElementTypesList.Count > 0)
            {
                bool valueToSet = !mWizard.mPomLearnUtils.AutoMapElementTypesList[0].Selected;
                foreach (UIElementFilter elem in mWizard.mPomLearnUtils.AutoMapElementTypesList)
                {
                    elem.Selected = valueToSet;
                }
            }
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
            xAutoMapElementTypesExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xAutoMapElementTypesExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsExpanded = xAgentControlUC.AgentIsRunning;
            xElementLocatorsSettingsExpander.IsEnabled = xAgentControlUC.AgentIsRunning;
        }

        private void ClearAutoMapElementTypesSection()
        {
            mWizard.mPomLearnUtils.AutoMapElementTypesList = new ObservableList<UIElementFilter>();
            xAutoMapElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapElementTypesList;
        }

        private void SetAutoMapElementTypesSection()
        {
            xAgentControlUC.xAgentConfigsExpander.IsExpanded = false;

            SetAutoMapElementTypes();
            xAutoMapElementTypesGrid.DataSourceList = mWizard.mPomLearnUtils.AutoMapElementTypesList;
        }

        private void SetElementLocatorsSettingsSection()
        {
            if (mWizard.mPomLearnUtils.ElementLocatorsSettingsList.Count == 0)
            {
                switch (mAppPlatform)
                {
                    case ePlatformType.Web:
                        mWizard.mPomLearnUtils.ElementLocatorsSettingsList = new WebPlatform().GetLearningLocators();
                        break;
                }
            }
            xElementLocatorsSettingsGrid.DataSourceList = mWizard.mPomLearnUtils.ElementLocatorsSettingsList;
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
    }
}
