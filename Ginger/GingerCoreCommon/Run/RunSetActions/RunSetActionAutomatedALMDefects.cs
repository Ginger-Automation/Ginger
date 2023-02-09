#region License
/*
Copyright © 2014-2023 European Support Limited

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
        public IRunSetActionAutomatedALMDefectsOperations RunSetActionAutomatedALMDefectsOperations;
        public enum eDefectsOpeningMode
        {
            HTMLReport,
            FreeText
        }

        private int mSelectedDefectsProfileID;
        [IsSerializedForLocalRepository]
        public int SelectedDefectsProfileID { get { return mSelectedDefectsProfileID; } set { if (mSelectedDefectsProfileID != value) { mSelectedDefectsProfileID = value; OnPropertyChanged(nameof(SelectedDefectsProfileID)); } } }

        private bool mDefectsOpeningModeForAll;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeForAll { get { return mDefectsOpeningModeForAll; } set { if (mDefectsOpeningModeForAll != value) { mDefectsOpeningModeForAll = value; OnPropertyChanged(nameof(DefectsOpeningModeForAll)); } } }

        private bool mDefectsOpeningModeForMarked;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeForMarked { get { return mDefectsOpeningModeForMarked; } set { if (mDefectsOpeningModeForMarked != value) { mDefectsOpeningModeForMarked = value; OnPropertyChanged(nameof(DefectsOpeningModeForMarked)); } } }

        private bool mDefectsOpeningModeReviewOnly;
        [IsSerializedForLocalRepository]
        public bool DefectsOpeningModeReviewOnly { get { return mDefectsOpeningModeReviewOnly; } set { if (mDefectsOpeningModeReviewOnly != value) { mDefectsOpeningModeReviewOnly = value; OnPropertyChanged(nameof(DefectsOpeningModeReviewOnly)); } } }

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
            RunSetActionAutomatedALMDefectsOperations.Execute(RI);
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
