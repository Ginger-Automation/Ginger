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
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.Application_Models;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        Dictionary<ApplicationPOMModel, List<RunSetConfig>> ApplicationPOMModelrunsetConfigMapping;
        ObservableList<RunSetConfig> RunsetConfigList = new ObservableList<RunSetConfig>();
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
                        ApplicationPOMModelrunsetConfigMapping = new Dictionary<ApplicationPOMModel, List<RunSetConfig>>();
                        ObservableList<RunSetConfig> RunSetConfigList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        ObservableList<GingerCore.BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.BusinessFlow>();

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
                                if (!ApplicationPOMModelrunsetConfigMapping.ContainsKey(applicationPOMModel))
                                {
                                    ApplicationPOMModelrunsetConfigMapping[applicationPOMModel] = new List<RunSetConfig>();
                                }
                                ApplicationPOMModelrunsetConfigMapping[applicationPOMModel].Add(runsetConfig);
                            }
                        }


                        ObservableList<MultiPomRunSetMapping> multiPomRunSetMappingsList = new ObservableList<MultiPomRunSetMapping>();

                        foreach (var kvp in ApplicationPOMModelrunsetConfigMapping)
                        {
                            MultiPomRunSetMapping multiPomRunSetMappingItem = new MultiPomRunSetMapping();
                            multiPomRunSetMappingItem.ApplicationAPIModel = kvp.Key;
                            multiPomRunSetMappingItem.ApplicationAPIModelName = multiPomRunSetMappingItem.ApplicationAPIModel.Name;
                            multiPomRunSetMappingItem.RunSetConfigList = kvp.Value;
                            multiPomRunSetMappingItem.RunSetStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;

                            multiPomRunSetMappingsList.Add(multiPomRunSetMappingItem);
                        }

                        // Set the most common RunSetConfig for each item, but retain the unique RunSetConfig if it differs
                        foreach (MultiPomRunSetMapping item in multiPomRunSetMappingsList)
                        {
                            if (item.RunSetConfigList != null && item.RunSetConfigList.Any())
                            {
                                //Find the most common RunSetConfig across all items in multiPomRunSetMappingsList
                                var mostCommonRunSetConfig = multiPomRunSetMappingsList
                                    .SelectMany(item => item.RunSetConfigList)
                                    .GroupBy(runsetConfig => runsetConfig)
                                    .OrderByDescending(group => group.Count())
                                    .FirstOrDefault()?.Key;

                                // Check if the most common RunSetConfig is present in the current item's RunSetConfigList
                                if (item.RunSetConfigList.Contains(mostCommonRunSetConfig))
                                {
                                    item.SelectedRunset = mostCommonRunSetConfig;
                                    item.RunsetName = mostCommonRunSetConfig?.Name;
                                }
                                else
                                {
                                    var uniqueRunSetConfig = item.RunSetConfigList
                                        .GroupBy(runsetConfig => runsetConfig)
                                        .OrderByDescending(group => group.Count())
                                        .FirstOrDefault()?.Key;

                                    item.SelectedRunset = uniqueRunSetConfig;
                                    item.RunsetName = uniqueRunSetConfig?.Name;
                                }
                            }
                            else
                            {
                                // Handle case where RunSetConfigList is empty or null
                                item.SelectedRunset = null;
                                item.RunsetName = string.Empty;
                                item.PomUpdateStatus = $"No Runset found";
                            }
                        }

                        mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList = multiPomRunSetMappingsList;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to auto select runset {ex.ToString()}");
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
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(MultiPomRunSetMapping.ApplicationAPIModelName), Header = "POM Name", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.RunsetName), Header = "RunSet", WidthWeight = 50, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(MultiPomRunSetMapping.RunSetConfigList), nameof(MultiPomRunSetMapping.RunsetName), true,comboSelectionChangedHandler:RunSetComboBox_SelectionChanged) },
                new GridColView() { Field = "Run", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedPOMObjectMappingWithRunsetGrid.Resources["xTestElementButtonTemplate"] },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.StatusIcon), Header = "RunSet Status", WidthWeight = 20, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xSelectedPOMObjectMappingWithRunsetGrid.Resources["xTestStatusIconTemplate"] },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.PomUpdateStatus), Header = "Comment", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true }
            ]
            };

            xPomWithRunsetSelectionGrid.SetAllColumnsDefaultView(defView);
            xPomWithRunsetSelectionGrid.InitViewItems();

            xPomWithRunsetSelectionGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            //// TODO: For next release
            //xPomWithRunsetSelectionGrid.AddToolbarTool(eImageType.Run, "Run All Run Set", new RoutedEventHandler(TestAllRunSet));
        }

        private void SetPomWithRunsetSelectionSection()
        {
            xPomWithRunsetSelectionGrid.DataSourceList = mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList;
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
            mWizard.DisableBackBtnOnLastPage = true;
            mWizard.mWizardWindow.SetFinishButtonEnabled(false);
            mWizard.mWizardWindow.SetPrevButtonEnabled(false);
            xPomWithRunsetSelectionGrid.DisableGridColoumns();
            if (mSelectedPomWithRunset != null)
            {
                if (mSelectedPomWithRunset.SelectedRunset != null)
                {
                    mSelectedPomWithRunset.SelectedRunset.AutoUpdatedPOMList = new();
                    if (mSelectedPomWithRunset.SelectedRunset?.SelfHealingConfiguration != null)
                    {
                        if (mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.EnableSelfHealing)
                        {
                            if (mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.AutoUpdateApplicationModel)
                            {
                                if (!mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.ForceUpdateApplicationModel)
                                {
                                    mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.ForceUpdateApplicationModel = true;
                                }
                            }
                            else
                            {
                                mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.AutoUpdateApplicationModel = true;
                                mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.ForceUpdateApplicationModel = true;
                            }
                        }
                        else
                        {
                            mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.EnableSelfHealing = true;
                            mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.AutoUpdateApplicationModel = true;
                            mSelectedPomWithRunset.SelectedRunset.SelfHealingConfiguration.ForceUpdateApplicationModel = true;
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected, "RunSet");
                    return;
                }

                WorkSpace.Instance.RunningInExecutionMode = true;
                LoadRunsetConfigToRunsetExecutor(runsetExecutor: WorkSpace.Instance.RunsetExecutor, runSetConfig: mSelectedPomWithRunset.SelectedRunset, mCLIHelper: mCLIHelper);
                try
                {
                    mSelectedPomWithRunset.RunSetStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
                    await ExecuteRunSet();
                    foreach (MultiPomRunSetMapping elem in mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList)
                    {
                        if (mSelectedPomWithRunset.SelectedRunset.Guid.Equals(elem.SelectedRunset?.Guid))
                        {
                            if (mSelectedPomWithRunset.SelectedRunset.AutoUpdatedPOMList.Contains(elem.ApplicationAPIModel.Guid))
                            {
                                elem.PomUpdateStatus = $"{elem.ApplicationAPIModel.Name} Updated";
                                var aPOMModified = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().First(aPOM => aPOM.Guid == elem.ApplicationAPIModel.Guid);
                                if (aPOMModified != null)
                                {
                                    SaveHandler.Save(aPOMModified);
                                }
                                else
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, $"Cannot find POM with GUID '{elem.ApplicationAPIModel.Guid}' to save");
                                }
                            }
                            else
                            {
                                elem.PomUpdateStatus = $"{elem.ApplicationAPIModel.Name} Not Updated";
                            }

                            elem.RunSetStatus = mSelectedPomWithRunset.SelectedRunset.RunSetExecutionStatus;
                            if (elem.RunSetStatus.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed))
                            {
                                elem.PomUpdateStatus = $"{elem.PomUpdateStatus} and Runset status Failed";
                            }

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
            mWizard.DisableBackBtnOnLastPage = false;
            mWizard.mWizardWindow.SetFinishButtonEnabled(true);
            mWizard.mWizardWindow.SetPrevButtonEnabled(true);
            xPomWithRunsetSelectionGrid.EnableGridColumns();
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

        public async Task ExecuteRunSet()
        {
            Reporter.ToLog(eLogLevel.INFO, string.Format("Executing {0}... ", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            try
            {

                await Task.Run(() =>
                {
                    try
                    {
                        Execute(WorkSpace.Instance.RunsetExecutor);
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception
                        Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while Execute RunSet", ex);
                    }
                });

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

        private void RunSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mSelectedPomWithRunset != null)
            {
                mSelectedPomWithRunset.SelectedRunset = ((RunSetConfig)((ComboBox)sender).SelectedItem);
                mSelectedPomWithRunset.RunsetName = ((RunSetConfig)((ComboBox)sender).SelectedItem).Name;
            }
        }
    }
}
