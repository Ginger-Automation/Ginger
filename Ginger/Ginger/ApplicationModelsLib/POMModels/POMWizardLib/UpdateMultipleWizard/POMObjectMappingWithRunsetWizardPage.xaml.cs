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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Agents;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib.UpdateMultipleWizard
{
    /// <summary>
    /// Interaction logic for POMObjectMappingWithRunsetWizardPage.xaml
    /// </summary>
    public partial class POMObjectMappingWithRunsetWizardPage : Page, IWizardPage
    {
        public UpdateMultiplePomWizard mWizard;
        PomAllElementsPage mPomAllElementsPage = null;
        SingleItemTreeViewSelectionPage mRunSetsSelectionPage = null;
        // Define a dictionary with string keys and int values
        Dictionary<RunSetConfig, List<ApplicationPOMModel>> runsetConfigMapping;
        public POMObjectMappingWithRunsetWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (UpdateMultiplePomWizard)WizardEventArgs.Wizard;
                    break;

                case EventType.Active:
                    runsetConfigMapping = new Dictionary<RunSetConfig, List<ApplicationPOMModel>>();
                    ObservableList<RunSetConfig> RunSetConfigList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                    //foreach (ApplicationPOMModel applicationPOMModel in mWizard.mMultiPomDeltaUtils.mPOMModels.Where(x => x.Selected))
                    //{
                    //    foreach (RunSetConfig runsetConfig in RunSetConfigList)
                    //    {
                    //        //runsetConfig.GingerRunners.Any(x=>x.Executor.BusinessFlows.Any(y=>y.Activities.Any(k=> k.Acts.Any(p=> p.ActionType is ActUIElement && (ActUIElement)p.E == eLocateBy.POMElement)))))

                    //        foreach (GingerRunner gingerRunner in runsetConfig.GingerRunners)
                    //        {
                    //            foreach (BusinessFlow businessFlow in businessFlows.Where(x => gingerRunner.BusinessFlowsRunList.Select(Y => Y.BusinessFlowGuid).Contains(x.Guid)))
                    //            {
                    //                foreach (Activity activity in businessFlow.Activities)
                    //                {
                    //                    foreach (IAct act in activity.Acts)
                    //                    {
                    //                        if (act is ActUIElement)
                    //                        {
                    //                            ActUIElement actUIElement = (ActUIElement)act;
                    //                            if (actUIElement.ElementLocateBy == eLocateBy.POMElement)
                    //                            {
                    //                                if (actUIElement.ElementLocateValue.Split("_")[0] == applicationPOMModel.Guid.ToString())
                    //                                {
                    //                                    if (!POMWithRunsetMapping.ContainsKey(applicationPOMModel))
                    //                                    {
                    //                                        POMWithRunsetMapping.Add(applicationPOMModel, new ObservableList<RunSetConfig>());
                    //                                    }
                    //                                    POMWithRunsetMapping[applicationPOMModel].Add(runsetConfig);
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    var selectedPOMModels = mWizard.mMultiPomDeltaUtils.mPOMModels.Where(x => x.Selected);

                    foreach (var applicationPOMModel in selectedPOMModels)
                    {
                        var matchingRunSetConfigs = RunSetConfigList
                            .Where(runsetConfig => runsetConfig.GingerRunners
                                .Any(gingerRunner => businessFlows
                                    .Where(businessFlow => gingerRunner.BusinessFlowsRunList
                                        .Select(y => y.BusinessFlowGuid)
                                        .Contains(businessFlow.Guid))
                                    .Any(businessFlow => businessFlow.Activities
                                        .Any(activity => activity.Acts
                                            .Any(act =>
                                                    (act is ActUIElement actUIElement && actUIElement.ElementLocateBy == eLocateBy.POMElement &&
                                                        actUIElement.ElementLocateValue.Split("_")[0] == applicationPOMModel.Guid.ToString()) ||
                                                    (act is ActBrowserElement actBrowserElement && actBrowserElement.LocateBy == eLocateBy.POMElement &&
                                                        actBrowserElement.LocateValue.Split("_")[0] == applicationPOMModel.Guid.ToString()))))));

                        foreach (var runsetConfig in matchingRunSetConfigs)
                        {
                            if (!runsetConfigMapping.ContainsKey(runsetConfig))
                            {
                                runsetConfigMapping[runsetConfig] = new List<ApplicationPOMModel>();
                            }
                            runsetConfigMapping[runsetConfig].Add(applicationPOMModel);
                        }
                    }
                    

                    ObservableList<MultiPomRunSetMapping> multiPomRunSetMappingsList = new ObservableList<MultiPomRunSetMapping>();

                    foreach (var kvp in runsetConfigMapping)
                    {
                        MultiPomRunSetMapping multiPomRunSetMappingItem = new MultiPomRunSetMapping();
                        multiPomRunSetMappingItem.runSetConfig = kvp.Key;
                        multiPomRunSetMappingItem.runsetName = multiPomRunSetMappingItem.runSetConfig.Name;
                        multiPomRunSetMappingItem.applicationPOMModels = kvp.Value;

                        

                        multiPomRunSetMappingItem.commaSeparatedApplicationPOMModels = string.Join(", ", multiPomRunSetMappingItem.applicationPOMModels.Select(apm => apm.ToString()));
                        
                        multiPomRunSetMappingsList.Add(multiPomRunSetMappingItem);
                    }

                    foreach (var item in multiPomRunSetMappingsList)
                    {
                        var itemWithLargestApplicationPOMModels = multiPomRunSetMappingsList
                        .Where(mapping => item.applicationPOMModels.Any(model => selectedPOMModels.Contains(model)))
                        .OrderByDescending(mapping => mapping.applicationPOMModels.Count)
                        .FirstOrDefault();
                        itemWithLargestApplicationPOMModels.Selected = true;
                    }
                    // Find the item with the largest number of ApplicationPOMModel values
                    

                    //foreach (var applicationPOMModel in selectedPOMModels)
                    //{
                    //    if (multiPomRunSetMappingsList.Any(x => x.applicationPOMModels.Select(x=>x.Guid).Contains(applicationPOMModel.Guid)))
                    //    {
                    //        MultiPomRunSetMapping multiPomRunSetMappingData = multiPomRunSetMappingsList.FirstOrDefault(x => x.applicationPOMModels != null && x.applicationPOMModels.Select(x => x.Guid).Contains(applicationPOMModel.Guid));
                    //        multiPomRunSetMappingData.Selected = true;
                    //    }
                    //}

                    mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList = multiPomRunSetMappingsList;
                    SetPomWithRunsetSelectionSection();
                    SetPomWithRunsetSelectionView();
                    break;

                case EventType.LeavingForNextPage:
                case EventType.Finish:
                    //mPomAllElementsPage.FinishEditInAllGrids();
                    //if (mPomAllElementsPage != null)
                    //{
                    //    mPomAllElementsPage.StopSpy();
                    //}
                    //mWizard.mMultiPomDeltaUtils.ClearStopLearning();
                    break;
                case EventType.Cancel:
                    //if (mPomAllElementsPage != null)
                    //{
                    //    mPomAllElementsPage.StopSpy();
                    //}
                    //mWizard.mMultiPomDeltaUtils.ClearStopLearning();
                    break;
            }
        }

        private void SetPomWithRunsetSelectionView()
        {
            xPomWithRunsetSelectionGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllBasicElements));

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(MultiPomRunSetMapping.Selected), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.runsetName), Header = "Run set Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.commaSeparatedApplicationPOMModels), Header = "POM Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xPomWithRunsetSelectionGrid.Resources["xTestElementButtonTemplate"] },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.runSetConfig.DirtyStatusImage), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xPomWithRunsetSelectionGrid.Resources["xTestStatusIconTemplate"] }
            ]
            };

            xPomWithRunsetSelectionGrid.SetAllColumnsDefaultView(defView);
            xPomWithRunsetSelectionGrid.InitViewItems();

            xPomWithRunsetSelectionGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xPomWithRunsetSelectionGrid.AddToolbarTool(eImageType.Run, "Test All Run Set", new RoutedEventHandler(TestAllRunSet));
        }

        private void SetPomWithRunsetSelectionSection()
        {
            xPomWithRunsetSelectionGrid.DataSourceList = mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList;
            
        }

        private void CheckUnCheckAllBasicElements(object sender, RoutedEventArgs e)
        {
            if (mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList.Count > 0)
            {
                bool valueToSet = !mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList[0].Selected;
                foreach (MultiPomRunSetMapping elem in mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList)
                {
                    elem.Selected = valueToSet;
                }
            }
        }

        private void TestAllRunSet(object sender, RoutedEventArgs e)
        {
            //if (!ValidateDriverAvalability())
            //{
            //    return;
            //}

            //if (SelectedElement != null)
            //{
            //    WindowExplorerDriver.TestElementLocators(SelectedElement, mPOM: SelectedPOM);
            //}
        }
    }
}
