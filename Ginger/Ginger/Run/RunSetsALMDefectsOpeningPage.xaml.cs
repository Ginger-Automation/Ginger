#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common;
using Ginger.Environments;
using Ginger.Reports;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Ginger.ALM;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunSetsExecutionsPage.xaml
    /// </summary>
    public partial class RunSetsALMDefectsOpeningPage : Page
    {
        FailedActionsScreenshotsPage mFailedActionsScreenshotsPage;

        public RunSetsALMDefectsOpeningPage()
        {
            InitializeComponent();
            SetGridView();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ToOpenDefectFlag).ToString(), Header = "To Open Defect", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ToOpenDefectFlag"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ALMDefectID).ToString(), Header = "ALM Defect ID", WidthWeight = 10, ReadOnly = true, HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.RunnerName).ToString(), Header = "Runner Name", WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.BusinessFlowName).ToString(), Header = "Business Flow Name", WidthWeight = 12, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ActivitiesGroupName).ToString(), Header = "Activities Group Name", WidthWeight = 14, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ActivitySequence).ToString(), Header = "Activity Sequence", WidthWeight = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ActivityName).ToString(), Header = "Activity Name", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ActionSequence).ToString(), Header = "Action Sequence", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ActionDescription).ToString(), Header = "Action Description", WidthWeight = 16, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.RetryIteration).ToString(), Header = "Retry Iteration", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ErrorDetails).ToString(), Header = "Error Details", WidthWeight = 18, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ExtraDetails).ToString(), Header = "Extra Details", WidthWeight = 16, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(DefectSuggestion.ScreenshotFileNames).ToString(), Header = "Screenshot", WidthWeight = 7, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ScreenShotButton"] });

            grdDefectSuggestions.SetAllColumnsDefaultView(view);
            grdDefectSuggestions.InitViewItems();

            grdDefectSuggestions.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));

            grdDefectSuggestions.DataSourceList = App.RunsetExecutor.DefectSuggestionsList;
            grdDefectSuggestions.Visibility = Visibility.Visible;

            DefectProfilesCombo_Binding();
        }

        public void DefectProfilesCombo_Binding()
        {
            DefectProfiles_cbx.ItemsSource = null;

            if (App.UserProfile.Solution != null)
            {
                DefectProfiles_cbx.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                DefectProfiles_cbx.DisplayMemberPath = nameof(ALMDefectProfile.Name).ToString();
                DefectProfiles_cbx.SelectedValuePath = nameof(ALMDefectProfile.ID).ToString();
                DefectProfiles_cbx.SelectedIndex = DefectProfiles_cbx.Items.IndexOf(((ObservableList<ALMDefectProfile>)DefectProfiles_cbx.ItemsSource).Where(x => (x.IsDefault == true)).FirstOrDefault());
            }
        }

        public void ReloadData()
        {
            grdDefectSuggestions.DataSourceList = App.RunsetExecutor.DefectSuggestionsList;
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            grdDefectSuggestions.DataSourceList = App.RunsetExecutor.DefectSuggestionsList;
        }

        private void ScreenShotClicked(object sender, RoutedEventArgs e)
        {
            if (grdDefectSuggestions.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                return;
            }
            else
            {
                if (mFailedActionsScreenshotsPage == null)
                    mFailedActionsScreenshotsPage = new FailedActionsScreenshotsPage(((DefectSuggestion)grdDefectSuggestions.CurrentItem).ScreenshotFileNames);
                mFailedActionsScreenshotsPage.ShowAsWindow();
            }
        }

        private void OpenDefectForSelectedSuggestions_Click(object sender, RoutedEventArgs e)
        {
            if (!ALMIntegration.Instance.AlmConfigurations.UseRest)
            {
                Reporter.ToUser(eUserMsgKeys.ALMDefectsUserInOtaAPI, "");
                return;
            }
            if ((App.RunsetExecutor.DefectSuggestionsList != null) && (App.RunsetExecutor.DefectSuggestionsList.Count > 0) &&
                (App.RunsetExecutor.DefectSuggestionsList.Where(x => x.ToOpenDefectFlag == true && (x.ALMDefectID == null || x.ALMDefectID == string.Empty)).ToList().Count > 0))
            {
                if (Reporter.ToUser(eUserMsgKeys.AskALMDefectsOpening, App.RunsetExecutor.DefectSuggestionsList.Where(x => x.ToOpenDefectFlag == true && (x.ALMDefectID == null || x.ALMDefectID == string.Empty)).ToList().Count) == MessageBoxResult.Yes)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    Dictionary<Guid, Dictionary<string, string>> defectsForOpening = new Dictionary<Guid, Dictionary<string, string>>();
                    foreach (DefectSuggestion defectSuggestion in App.RunsetExecutor.DefectSuggestionsList.Where(x => x.ToOpenDefectFlag == true && (x.ALMDefectID == null || x.ALMDefectID == string.Empty)).ToList())
                    {
                        Dictionary<string, string> currentALMDefectFieldsValues = ((ALMDefectProfile)DefectProfiles_cbx.SelectedItem).ALMDefectProfileFields.Where(z => (z.SelectedValue != null && z.SelectedValue != string.Empty) ||
                                                                                                                                                                         z.ExternalID == "description" || z.ExternalID == "Summary" || z.ExternalID == "name").ToDictionary(x => x.ExternalID, x => x.SelectedValue != null ? x.SelectedValue.Replace("&", "&amp;") : x.SelectedValue = string.Empty)
                                                                                                                                     .ToDictionary(w => w.Key, w => w.Key == "description" ? defectSuggestion.Description : w.Value)
                                                                                                                                     .ToDictionary(w => w.Key, w => w.Key == "Summary" ? defectSuggestion.Summary : w.Value)
                                                                                                                                     .ToDictionary(w => w.Key, w => w.Key == "name" ? defectSuggestion.Summary : w.Value);
                        defectsForOpening.Add(defectSuggestion.DefectSuggestionGuid, currentALMDefectFieldsValues);
                    }

                    Dictionary<Guid, string> defectsOpeningResults = ALMIntegration.Instance.CreateNewALMDefects(defectsForOpening);

                    if ((defectsOpeningResults != null) && (defectsOpeningResults.Count > 0))
                    {
                        foreach (KeyValuePair<Guid, string> defectOpeningResult in defectsOpeningResults)
                        {
                            if ((defectOpeningResult.Value != null) && (defectOpeningResult.Value != "0"))
                            {
                                App.RunsetExecutor.DefectSuggestionsList.Where(x => x.DefectSuggestionGuid == defectOpeningResult.Key).ToList().ForEach(z => { z.ALMDefectID = defectOpeningResult.Value; z.IsOpenDefectFlagEnabled = false; z.ToOpenDefectFlag = false; });
                            }
                        }
                        grdDefectSuggestions.DataSourceList = App.RunsetExecutor.DefectSuggestionsList;
                        Mouse.OverrideCursor = null;
                        Reporter.ToUser(eUserMsgKeys.ALMDefectsWereOpened, defectsOpeningResults.Where(x => x.Value != null && x.Value != string.Empty && x.Value != "0").ToList().Count);
                    }
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}