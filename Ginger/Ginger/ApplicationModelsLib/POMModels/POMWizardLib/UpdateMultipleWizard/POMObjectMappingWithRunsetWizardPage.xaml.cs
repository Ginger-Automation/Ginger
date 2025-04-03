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
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
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
using GingerTest.WizardLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Foundation.Collections;

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
        NewRunSetPage NewRunSetPageItem;
        CLIHelper mCLIHelper = new();
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
                    SetPomWithRunsetSelectionView();
                    break;

                case EventType.Active:
                    try
                    {
                        runsetConfigMapping = new Dictionary<RunSetConfig, List<ApplicationPOMModel>>();
                        ObservableList<RunSetConfig> RunSetConfigList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

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
                            multiPomRunSetMappingItem.RunSetConfig = kvp.Key;
                            multiPomRunSetMappingItem.RunsetName = multiPomRunSetMappingItem.RunSetConfig.Name;
                            multiPomRunSetMappingItem.applicationPOMModels = kvp.Value;
                            multiPomRunSetMappingItem.RunSetStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;


                            multiPomRunSetMappingItem.commaSeparatedApplicationPOMModels = string.Join(", ", multiPomRunSetMappingItem.applicationPOMModels.Select(apm => apm.ToString()));

                            multiPomRunSetMappingsList.Add(multiPomRunSetMappingItem);
                        }
                        // Select the item with the largest intersection of applicationPOMModels.
                        // The rationale is that a larger intersection indicates greater relevance or compatibility
                        // with the given set, ensuring the best possible match.
                        foreach (var item in multiPomRunSetMappingsList)
                        {
                            var itemWithLargestApplicationPOMModels = multiPomRunSetMappingsList
                            .Where(mapping => item.applicationPOMModels.Any(model => selectedPOMModels.Contains(model)))
                            .OrderByDescending(mapping => mapping.applicationPOMModels.Count)
                            .FirstOrDefault();
                            itemWithLargestApplicationPOMModels.Selected = true;
                        }

                        mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList = multiPomRunSetMappingsList;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR,$"Failed to auto select runset { ex.ToString()}");
                    }
                    finally
                    {
                        SetPomWithRunsetSelectionSection();
                    }

                    
                    break;

                case EventType.LeavingForNextPage:
                case EventType.Finish:
                    break;
                case EventType.Cancel:
                    break;
                default: break;
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
                new GridColView() { Field = nameof(MultiPomRunSetMapping.RunsetName), Header = "Runset Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.commaSeparatedApplicationPOMModels), Header = "POM Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = "Run", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedPOMObjectMappingWithRunsetGrid.Resources["xTestElementButtonTemplate"] },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.StatusIcon), Header = "Status", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedPOMObjectMappingWithRunsetGrid.Resources["xTestStatusIconTemplate"] },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.PomUpdateStatus), Header = "Comment", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true }
            ]
            };

            xPomWithRunsetSelectionGrid.SetAllColumnsDefaultView(defView);
            xPomWithRunsetSelectionGrid.InitViewItems();

            xPomWithRunsetSelectionGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xPomWithRunsetSelectionGrid.AddToolbarTool(eImageType.Run, "Run All Run Set", new RoutedEventHandler(TestAllRunSet));
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

        MultiPomRunSetMapping mSelectedPomWithRunset
        {
            get
            {
                if (xPomWithRunsetSelectionGrid.Grid.SelectedItem != null)
                {
                    return (MultiPomRunSetMapping)xPomWithRunsetSelectionGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        private async void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            
            mWizard.ProcessStarted();
            mWizard.mWizardWindow.SetFinishButtonEnabled(false);
            if (mSelectedPomWithRunset != null && mSelectedPomWithRunset.Selected)
            {
                mSelectedPomWithRunset.RunSetConfig.AutoUpdatedPOMList = new();
                WorkSpace.Instance.RunningInExecutionMode = true;
                LoadRunsetConfigToRunsetExecutor(runsetExecutor: WorkSpace.Instance.RunsetExecutor, runSetConfig: mSelectedPomWithRunset.RunSetConfig, mCLIHelper: mCLIHelper);
                try
                {
                    await ExecuteRunSet();

                    foreach (MultiPomRunSetMapping elem in mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList)
                    {
                        if (mSelectedPomWithRunset.RunSetConfig.Guid.Equals(elem.RunSetConfig.Guid))
                        {
                            List<string> PassedPomUpdateList = new List<string>();
                            List<string> FailedPomUpdateList = new List<string>();
                            string PassedPomUpdate = string.Empty;
                            string FailedPomUpdate = string.Empty;
                            foreach (ApplicationPOMModel appmodel in mSelectedPomWithRunset.applicationPOMModels)
                            {
                                if (mSelectedPomWithRunset.RunSetConfig.AutoUpdatedPOMList.Contains(appmodel.Guid))
                                {
                                    PassedPomUpdateList.Add(appmodel.Name);
                                }
                                else
                                {
                                    FailedPomUpdateList.Add(appmodel.Name);
                                }
                            }
                            mSelectedPomWithRunset.RunSetStatus = mSelectedPomWithRunset.RunSetConfig.RunSetExecutionStatus;


                            // Add "Updated" after each element in PassedPomUpdateList
                            PassedPomUpdate = string.Join(", ", PassedPomUpdateList.Select(item => $"{item} Updated"));

                            // Add "Not Updated" after each element in FailedPomUpdateList
                            FailedPomUpdate = string.Join(", ", FailedPomUpdateList.Select(item => $"{item} Not Updated"));

                            elem.PomUpdateStatus = $"{PassedPomUpdate}{Environment.NewLine}{FailedPomUpdate}";
                        }
                    }
                    SetPomWithRunsetSelectionSection();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while Execute RunSet", ex);
                }
                finally
                {
                    WorkSpace.Instance.RunningInExecutionMode = false;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
            mWizard.mWizardWindow.SetFinishButtonEnabled(true);
            mWizard.ProcessEnded();
        }
        private void TestAllRunSet(object sender, RoutedEventArgs e)
        {
            //To Do
        }

        public void LoadRunsetConfigToRunsetExecutor(RunSetConfig runSetConfig, RunsetExecutor runsetExecutor, CLIHelper mCLIHelper)
        {
            runsetExecutor.RunSetConfig = runSetConfig;
            

            if (!mCLIHelper.LoadRunset(runsetExecutor))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load Runset ");
                return;
            }

            if (!mCLIHelper.PrepareRunsetForExecution())
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Prepare Runset for execution");
                return;
            }

        }

        async Task ExecuteRunSet()
        {
            Reporter.ToLog(eLogLevel.INFO, string.Format("Executing {0}... ", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            try
            {
                await Execute(WorkSpace.Instance.RunsetExecutor);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while Execute RunSet", ex);
            }
        }

        public async Task Execute(RunsetExecutor runsetExecutor)
        {
            await runsetExecutor.RunRunset();
        }
    }
}
