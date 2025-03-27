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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
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

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib.UpdateMultipleWizard
{
    /// <summary>
    /// Interaction logic for UpdateMultiplePomConfigWizardPage.xaml
    /// </summary>
    public partial class UpdateMultiplePomConfigWizardPage : Page, IWizardPage
    {
        private UpdateMultiplePomWizard mWizard;
        private ePlatformType mAppPlatform;
        public bool isEnableFriendlyLocator = false;
        ObservableList<ApplicationPOMModel> POMModels;
        public UpdateMultiplePomConfigWizardPage()
        {
            InitializeComponent();
            xTAlabel.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (UpdateMultiplePomWizard)WizardEventArgs.Wizard;

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList());
                    xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.mMultiPomDeltaUtils.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                    xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

                    if (xTargetApplicationComboBox.Items != null && xTargetApplicationComboBox.Items.Count > 0)
                    {
                        xTargetApplicationComboBox.SelectedIndex = 0;
                    }
                    
                    SetPomSelectionExpanderSection();
                    SetPomSelectionGridView();
                    break;
                case EventType.LeavingForNextPage:
                    //UpdateCustomTemplateList();
                    break;
            }
        }

        private void XTargetApplicationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {
            if (mWizard.mMultiPomDeltaUtils.POM.TargetApplicationKey != null)
            {
                mAppPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mWizard.mMultiPomDeltaUtils.POM.TargetApplicationKey);
            }
            else
            {

                if (xTargetApplicationComboBox.SelectedItem is ApplicationPlatform selectedplatform)
                {
                    mAppPlatform = selectedplatform.Platform;
                }
            }
        }

        private void SetPomSelectionGridView()
        {
            xPomSelectionGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ApplicationPOMModel.Selected), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(ApplicationPOMModel.Name), Header = "POM Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(ApplicationPOMModel.PageURL), Header = "Page URL", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(ApplicationPOMModel.PageURL), Header = "Page URL", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
            ]
            };

            xPomSelectionGrid.SetAllColumnsDefaultView(defView);
            xPomSelectionGrid.InitViewItems();

            xPomSelectionGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void SetPomSelectionExpanderSection()
        {
            xPomSelectionExpander.IsExpanded = true;
            xPomSelectionExpander.IsEnabled = true;
            mWizard.mMultiPomDeltaUtils.mPOMModels = [];
            mWizard.mMultiPomDeltaUtils.mPOMModels = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where WorkSpace.Instance.Solution.GetTargetApplicationPlatform(x.TargetApplicationKey) == mAppPlatform select x).ToList());
            xPomSelectionGrid.DataSourceList = mWizard.mMultiPomDeltaUtils.mPOMModels;
            //foreach (ApplicationPOMModel applicationPOMModel in mWizard.mMultiPomDeltaUtils.mPOMModels)
            //{
            //    string allProperties = string.Empty;
            //    PropertyChangedEventManager.RemoveHandler(source: applicationPOMModel, handler: Item_PropertyChanged, propertyName: allProperties);
            //    PropertyChangedEventManager.AddHandler(source: applicationPOMModel, handler: Item_PropertyChanged, propertyName: allProperties);
            //}
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mMultiPomDeltaUtils.mPOMModels.Count > 0)
            {
                bool valueToSet = !mWizard.mMultiPomDeltaUtils.mPOMModels[0].Selected;
                foreach (ApplicationPOMModel elemPom in mWizard.mMultiPomDeltaUtils.mPOMModels)
                {
                    elemPom.Selected = valueToSet;
                }
            }
        }

        //private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    ApplicationPOMModel solutionItem = (ApplicationPOMModel)sender;
        //    bool valuetoSet = !solutionItem.Selected;
        //    solutionItem.Selected = valuetoSet;
        //}
    }
}
