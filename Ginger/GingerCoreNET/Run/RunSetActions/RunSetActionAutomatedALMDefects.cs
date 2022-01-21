#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Ginger.Reports;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionAutomatedALMDefects : RunSetActionBase
    {
        public enum eDefectsOpeningMode
        {
            HTMLReport,
            FreeText
        }

        public new static class Fields
        {
            public const string SelectedDefectsProfileID = "SelectedDefectsProfileID";
            public const string DefectsOpeningModeForAll = "DefectsOpeningModeForAll";
            public const string DefectsOpeningModeForMarked = "DefectsOpeningModeForMarked";
            public const string DefectsOpeningModeReviewOnly = "DefectsOpeningModeReviewOnly";
        }

        private int mSelectedDefectsProfileID;
        [IsSerializedForLocalRepository]
        public int SelectedDefectsProfileID { get { return mSelectedDefectsProfileID; } set { if (mSelectedDefectsProfileID != value) { mSelectedDefectsProfileID = value; OnPropertyChanged(Fields.SelectedDefectsProfileID); } } }

        private bool mDefectsOpeningModeForAll;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeForAll { get { return mDefectsOpeningModeForAll; } set { if (mDefectsOpeningModeForAll != value) { mDefectsOpeningModeForAll = value; OnPropertyChanged(Fields.DefectsOpeningModeForAll); } } }

        private bool mDefectsOpeningModeForMarked;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeForMarked { get { return mDefectsOpeningModeForMarked; } set { if (mDefectsOpeningModeForMarked != value) { mDefectsOpeningModeForMarked = value; OnPropertyChanged(Fields.DefectsOpeningModeForMarked); } } }

        private bool mDefectsOpeningModeReviewOnly;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeReviewOnly { get { return mDefectsOpeningModeReviewOnly; } set { if (mDefectsOpeningModeReviewOnly != value) { mDefectsOpeningModeReviewOnly = value; OnPropertyChanged(Fields.DefectsOpeningModeReviewOnly); } } }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return false; }
        }

        public override void Execute(IReportInfo RI)
        {
            if ((WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null) && (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count > 0))
            {
                Dictionary<Guid, Dictionary<string, string>> defectsForOpening = new Dictionary<Guid, Dictionary<string, string>>();

                ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                ALMDefectProfile defaultALMDefectProfile = new ALMDefectProfile();
                if ((ALMDefectProfiles != null) && (ALMDefectProfiles.Count > 0))
                {
                    defaultALMDefectProfile = ALMDefectProfiles.Where(z => z.ID == SelectedDefectsProfileID).ToList().FirstOrDefault();
                    if (defaultALMDefectProfile == null)
                    {
                        defaultALMDefectProfile = ALMDefectProfiles.Where(z => z.IsDefault).ToList().FirstOrDefault();
                        if (defaultALMDefectProfile == null)
                        {
                            defaultALMDefectProfile = ALMDefectProfiles.FirstOrDefault();
                        }
                    }
                }

                if (DefectsOpeningModeForMarked)
                {
                    foreach (DefectSuggestion defectSuggestion in WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Where(x => x.AutomatedOpeningFlag == true).ToList())
                    {
                        Dictionary<string, string> currentALMDefectFieldsValues = new Dictionary<string, string>();
                        try
                        {
                            currentALMDefectFieldsValues = defaultALMDefectProfile.ALMDefectProfileFields.Where(z => (z.SelectedValue != null && z.SelectedValue != string.Empty) ||
                                                                                                                                                 z.ExternalID == "description" || z.ExternalID == "Summary" || z.ExternalID == "name").ToDictionary(x => x.ExternalID, x => x.SelectedValue != null ? x.SelectedValue.Replace("&", "&amp;") : x.SelectedValue = string.Empty)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "description" ? defectSuggestion.ErrorDetails : w.Value)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "Summary" ? defectSuggestion.Summary : w.Value)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "name" ? defectSuggestion.Summary : w.Value);
                        }
                        catch (Exception ex)
                        {
                            currentALMDefectFieldsValues.Add("Summary", defectSuggestion.Summary);
                            currentALMDefectFieldsValues.Add("description", defectSuggestion.ErrorDetails);
                        }
                        
                        currentALMDefectFieldsValues.Add("screenshots", String.Join(",", defectSuggestion.ScreenshotFileNames));
                        currentALMDefectFieldsValues.Add("ActivityGroupExternalID", defectSuggestion.ActivityGroupExternalID);
                        currentALMDefectFieldsValues.Add("ActivityExternalID", defectSuggestion.ActivityExternalID);
                        currentALMDefectFieldsValues.Add("BFExternalID1", defectSuggestion.BFExternalID.Item1);
                        currentALMDefectFieldsValues.Add("BFExternalID2", defectSuggestion.BFExternalID.Item2);
                        defectsForOpening.Add(defectSuggestion.DefectSuggestionGuid, currentALMDefectFieldsValues);
                    }
                }
                else if (DefectsOpeningModeForAll)
                {
                    foreach (DefectSuggestion defectSuggestion in WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList)
                    {
                        Dictionary<string, string> currentALMDefectFieldsValues = new Dictionary<string, string>();
                        try
                        {
                            currentALMDefectFieldsValues = defaultALMDefectProfile.ALMDefectProfileFields.Where(z => (z.SelectedValue != null && z.SelectedValue != string.Empty) ||
                                                                                                                                                 z.ExternalID == "description" || z.ExternalID == "Summary" || z.ExternalID == "name").ToDictionary(x => x.ExternalID, x => x.SelectedValue != null ? x.SelectedValue.Replace("&", "&amp;") : x.SelectedValue = string.Empty)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "description" ? defectSuggestion.ErrorDetails : w.Value)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "Summary" ? defectSuggestion.Summary : w.Value)
                                                                                                             .ToDictionary(w => w.Key, w => w.Key == "name" ? defectSuggestion.Summary : w.Value);
                        }
                        catch (Exception ex)
                        {
                            currentALMDefectFieldsValues.Add("Summary", defectSuggestion.Summary);
                            currentALMDefectFieldsValues.Add("description", defectSuggestion.ErrorDetails);
                        }

                        currentALMDefectFieldsValues.Add("screenshots", String.Join(",", defectSuggestion.ScreenshotFileNames));
                        currentALMDefectFieldsValues.Add("ActivityGroupExternalID", defectSuggestion.ActivityGroupExternalID);
                        currentALMDefectFieldsValues.Add("ActivityExternalID", defectSuggestion.ActivityExternalID);
                        currentALMDefectFieldsValues.Add("BFExternalID1", defectSuggestion.BFExternalID.Item1);
                        currentALMDefectFieldsValues.Add("BFExternalID2", defectSuggestion.BFExternalID.Item2);
                        defectsForOpening.Add(defectSuggestion.DefectSuggestionGuid, currentALMDefectFieldsValues);
                    }
                }
                else
                    return;
                var defectFields = defaultALMDefectProfile.ALMDefectProfileFields.Where(a => a.Mandatory || a.ToUpdate).ToList();
                //update alm type to open defect
                TargetFrameworkHelper.Helper.CreateNewALMDefects(defectsForOpening, defectFields, defaultALMDefectProfile.AlmType);
            }
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string GetEditPage()
        {
            //return new RunSetActionAutomatedALMDefectsEditPage(this);
            return  "RunSetActionAutomatedALMDefectsEditPage";
        }
        public override string Type { get { return "Open ALM Defects"; } }
    }
}
