#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.UserControls;
using GingerCore;
using GingerCore.ALM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DefectSuggestion.ToOpenDefectFlag).ToString(), Header = "To Open Defect", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ToOpenDefectFlag"] },
                new GridColView() { Field = nameof(DefectSuggestion.ALMDefectID).ToString(), Header = "ALM Defect ID", WidthWeight = 10, ReadOnly = true, HorizontalAlignment = System.Windows.HorizontalAlignment.Center },
                new GridColView() { Field = nameof(DefectSuggestion.RunnerName).ToString(), Header = "Runner Name", WidthWeight = 8, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.BusinessFlowName).ToString(), Header = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name", WidthWeight = 12, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ActivitiesGroupName).ToString(), Header = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " Name", WidthWeight = 14, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ActivitySequence).ToString(), Header = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Sequence", WidthWeight = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ActivityName).ToString(), Header = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Name", WidthWeight = 10, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ActionSequence).ToString(), Header = "Action Sequence", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ActionDescription).ToString(), Header = "Action Description", WidthWeight = 16, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.RetryIteration).ToString(), Header = "Retry Iteration", WidthWeight = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ErrorDetails).ToString(), Header = "Error Details", WidthWeight = 18, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ExtraDetails).ToString(), Header = "Extra Details", WidthWeight = 16, ReadOnly = true },
                new GridColView() { Field = nameof(DefectSuggestion.ScreenshotFileNames).ToString(), Header = "Screenshot", WidthWeight = 7, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ScreenShotButton"] },
            ]
            };

            grdDefectSuggestions.SetAllColumnsDefaultView(view);
            grdDefectSuggestions.InitViewItems();

            grdDefectSuggestions.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));

            grdDefectSuggestions.DataSourceList = WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList;
            grdDefectSuggestions.Visibility = Visibility.Visible;

            DefectProfilesCombo_Binding();
        }

        public void DefectProfilesCombo_Binding()
        {
            DefectProfiles_cbx.ItemsSource = null;

            if (WorkSpace.Instance.Solution != null)
            {
                DefectProfiles_cbx.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                DefectProfiles_cbx.DisplayMemberPath = nameof(ALMDefectProfile.Name).ToString();
                DefectProfiles_cbx.SelectedValuePath = nameof(ALMDefectProfile.ID).ToString();
                DefectProfiles_cbx.SelectedIndex = DefectProfiles_cbx.Items.IndexOf(((ObservableList<ALMDefectProfile>)DefectProfiles_cbx.ItemsSource).FirstOrDefault(x => (x.IsDefault == true)));
            }
        }

        public void ReloadData()
        {
            grdDefectSuggestions.DataSourceList = WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList;
            DefectProfilesCombo_Binding();
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            grdDefectSuggestions.DataSourceList = WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList;
        }

        private void ScreenShotClicked(object sender, RoutedEventArgs e)
        {
            if (grdDefectSuggestions.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            else
            {
                if (mFailedActionsScreenshotsPage == null)
                {
                    mFailedActionsScreenshotsPage = new FailedActionsScreenshotsPage(((DefectSuggestion)grdDefectSuggestions.CurrentItem).ScreenshotFileNames);
                }

                mFailedActionsScreenshotsPage.ShowAsWindow();
            }
        }

        private void OpenDefectForSelectedSuggestions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null &&
                    WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Any(x => x.ToOpenDefectFlag == true && string.IsNullOrEmpty(x.ALMDefectID)))
                {
                    //if selected ALM is QC And UseRest=False return
                    GingerCoreNET.ALMLib.ALMConfig almConfig = ALMCore.GetCurrentAlmConfig(((ALMDefectProfile)DefectProfiles_cbx.SelectedItem).AlmType);
                    if (!almConfig.UseRest && almConfig.AlmType == GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.QC)
                    {
                        Reporter.ToUser(eUserMsgKey.ALMDefectsUserInOtaAPI);
                        return;
                    }

                    if (Reporter.ToUser(eUserMsgKey.AskALMDefectsOpening, WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count(x => x.ToOpenDefectFlag == true && string.IsNullOrEmpty(x.ALMDefectID))) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {

                        Dictionary<Guid, Dictionary<string, string>> defectsForOpening = [];
                        foreach (DefectSuggestion defectSuggestion in WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Where(x => x.ToOpenDefectFlag == true && string.IsNullOrEmpty(x.ALMDefectID)))
                        {
                            Dictionary<string, string> currentALMDefectFieldsValues = [];
                            try
                            {
                                currentALMDefectFieldsValues = ((ALMDefectProfile)DefectProfiles_cbx.SelectedItem).ALMDefectProfileFields.Where(z => (z.SelectedValue != null && z.SelectedValue != string.Empty) ||
                                                                                                                                                                                   z.ExternalID == "description" || z.ExternalID == "Summary" || z.ExternalID == "name").ToDictionary(x => x.ExternalID, x => x.SelectedValue != null ? x.SelectedValue.Replace("&", "&amp;") : x.SelectedValue = string.Empty)
                                                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "description" ? defectSuggestion.ErrorDetails : w.Value)
                                                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "Summary" ? defectSuggestion.Summary : w.Value)
                                                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "name" ? defectSuggestion.Summary : w.Value);
                            }
                            catch (Exception ex)
                            {
                                currentALMDefectFieldsValues.Add("Summary", defectSuggestion.Summary);
                                currentALMDefectFieldsValues.Add("description", defectSuggestion.ErrorDetails != null ? defectSuggestion.ErrorDetails : "There is no error description");
                            }
                            currentALMDefectFieldsValues.Add("screenshots", string.Join(",", defectSuggestion.ScreenshotFileNames));
                            currentALMDefectFieldsValues.Add("ActivityGroupExternalID", defectSuggestion.ActivityGroupExternalID);
                            currentALMDefectFieldsValues.Add("ActivityExternalID", defectSuggestion.ActivityExternalID);
                            currentALMDefectFieldsValues.Add("BFExternalID1", defectSuggestion.BFExternalID.Item1);
                            currentALMDefectFieldsValues.Add("BFExternalID2", defectSuggestion.BFExternalID.Item2);

                            defectsForOpening.Add(defectSuggestion.DefectSuggestionGuid, currentALMDefectFieldsValues);
                        }
                        var defectFields = ((ALMDefectProfile)DefectProfiles_cbx.SelectedItem).ALMDefectProfileFields.ToList();
                        //Update alm type to open defect
                        ALMIntegration.Instance.UpdateALMType(((ALMDefectProfile)DefectProfiles_cbx.SelectedItem).AlmType, true);
                        Dictionary<Guid, string> defectsOpeningResults = ALMIntegration.Instance.CreateNewALMDefects(defectsForOpening, defectFields);

                        if (defectsOpeningResults != null && defectsOpeningResults.Any())
                        {
                            foreach (KeyValuePair<Guid, string> defectOpeningResult in defectsOpeningResults.Where(d => !string.IsNullOrEmpty(d.Value) && d.Value != "0"))
                            {
                                foreach (var suggestedDefect in WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Where(x => x.DefectSuggestionGuid == defectOpeningResult.Key))
                                {
                                    suggestedDefect.ALMDefectID = defectOpeningResult.Value;
                                    suggestedDefect.IsOpenDefectFlagEnabled = false;
                                    suggestedDefect.ToOpenDefectFlag = false;
                                }
                            }
                            grdDefectSuggestions.DataSourceList = WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList;
                            Reporter.ToUser(eUserMsgKey.ALMDefectsWereOpened, defectsOpeningResults.Where(x => x.Value != null && x.Value != string.Empty && x.Value != "0").ToList().Count);
                        }
                    }
                }
                else if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null && WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Any() && !WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Any(x => string.IsNullOrEmpty(x.ALMDefectID)))
                {
                    Reporter.ToUser(eUserMsgKey.AllSelectedDefectAlreadyCreatedInAlm);
                    return;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoSelectedDefect);
                    return;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to create Defect - {ex.InnerException}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}