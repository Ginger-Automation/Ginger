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
using System.Globalization;
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

                        mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList = GingerCoreNET.GeneralLib.General.GetSelectedRunsetList(RunSetConfigList, businessFlows, selectedPOMModels, ApplicationPOMModelrunsetConfigMapping);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to auto select runset {ex.ToString()}");
                    }
                    finally
                    {
                        RefreshGridDataSourceList();
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
                new GridColView() { Field = nameof(MultiPomRunSetMapping.PomUpdateStatus), Header = "Comment", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
                new GridColView() { Field = nameof(MultiPomRunSetMapping.LastUpdatedTime), Header = "Last Updated", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.Text, ReadOnly = true },
            ]
            };

            xPomWithRunsetSelectionGrid.SetAllColumnsDefaultView(defView);
            xPomWithRunsetSelectionGrid.InitViewItems();

            xPomWithRunsetSelectionGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            //// TODO: For next release
            xPomWithRunsetSelectionGrid.AddToolbarTool(eImageType.Run, "Run All Run Set", new RoutedEventHandler(TestAllRunSet));
        }

        private void RefreshGridDataSourceList()
        {
            foreach (MultiPomRunSetMapping pom in mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList)
            {
                pom.LastUpdatedTime = pom.ApplicationAPIModel.RepositoryItemHeader.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            xPomWithRunsetSelectionGrid.DataSourceList = mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList;
        }
        MultiPomRunSetMapping mSelectedPomWithRunset { get; set; }



        private async void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            var button = sender as Button;
            if (button != null)
            {
                // Get the row object from the button's DataContext
                mSelectedPomWithRunset = button.DataContext as MultiPomRunSetMapping;

                if (mSelectedPomWithRunset != null)
                {
                    mWizard.ProcessStarted();
                    mWizard.DisableBackBtnOnLastPage = true;
                    mWizard.mWizardWindow.SetFinishButtonEnabled(false);
                    mWizard.mWizardWindow.SetPrevButtonEnabled(false);
                    xPomWithRunsetSelectionGrid.DisableGridColoumns();

                    await RunSelectedRunset(mSelectedPomWithRunset);

                    mWizard.DisableBackBtnOnLastPage = false;
                    mWizard.mWizardWindow.SetFinishButtonEnabled(true);
                    mWizard.mWizardWindow.SetPrevButtonEnabled(true);
                    xPomWithRunsetSelectionGrid.EnableGridColumns();
                    mWizard.ProcessEnded();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                }
            }
        }

        private async Task RunSelectedRunset(MultiPomRunSetMapping mSelectedPomWithRunset)
        {
            await GingerCoreNET.GeneralLib.General.RunSelectedRunset(mSelectedPomWithRunset, mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList, mCLIHelper);
            RefreshGridDataSourceList();
        }


        private async void TestAllRunSet(object sender, RoutedEventArgs e)
        {
            // Iterate through each item in the MultiPomRunSetMappingList
            foreach (MultiPomRunSetMapping item in mWizard.mMultiPomDeltaUtils.MultiPomRunSetMappingList)
            {
                await RunSelectedRunset(item);
            }
            // Refresh the UI or perform any necessary updates after all RunSets have been executed
            // Update the PomWithRunsetSelectionSection
            RefreshGridDataSourceList();
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
