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

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ginger.Run
{
    // public class DefectSuggestion : RepositoryItem // to check if need to be saved
    public class DefectSuggestion
    {
        public Guid FailedActionGuid { get; set; }
        public Guid DefectSuggestionGuid { get; set; }
        public bool ToOpenDefectFlag { get; set; }
        public bool IsOpenDefectFlagEnabled { get; set; }
        public bool AutomatedOpeningFlag { get; set; }
        public string ALMDefectID { get; set; }
        public string RunnerName { get; set; }
        public string BusinessFlowName { get; set; }
        public string ActivitiesGroupName { get; set; }
        public int ActivitySequence { get; set; }
        public string ActivityName { get; set; }
        public int ActionSequence { get; set; }
        public string ActionDescription { get; set; }
        public int RetryIteration { get; set; }
        public string ErrorDetails { get; set; }
        public string ExtraDetails { get; set; }
        public bool IsScreenshotButtonEnabled { get; set; }
        public List<string> ScreenshotFileNames { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ActivityGroupExternalID { get; set; }
        public string ActivityExternalID { get; set; }
        public Tuple<string, string>  BFExternalID { get; set; }

        public DefectSuggestion(Guid failedActionGuid, string runnerName, string businessFlowName, string activitiesGroupName, 
                                int activitySequence, string activityName, int actionSequence,
                                string actionDescription, int retryIteration, string errorDetails,
                                string extraDetails, List<string> screenshotFileNames,
                                bool isScreenshotButtonEnabled, bool automatedOpeningFlag, string description,
                                string activityGroupExternalID, string activityExternalID, Tuple<string, string> bfExternalID)
        {
            FailedActionGuid = failedActionGuid;
            DefectSuggestionGuid = Guid.NewGuid();
            RunnerName = runnerName;
            BusinessFlowName = businessFlowName;
            ActivitiesGroupName = activitiesGroupName;
            ActivitySequence = activitySequence;
            ActivityName = activityName;
            ActionSequence = actionSequence;
            ActionDescription = actionDescription;
            RetryIteration = retryIteration;
            ErrorDetails = errorDetails;
            ExtraDetails = extraDetails;
            ScreenshotFileNames = screenshotFileNames;
            AutomatedOpeningFlag = automatedOpeningFlag;
            IsScreenshotButtonEnabled = isScreenshotButtonEnabled;

            Summary = businessFlowName + "_" + activityName + "_" + actionDescription + "_Failed";
            Description = description;

            ToOpenDefectFlag = false;
            IsOpenDefectFlagEnabled = true;
            ALMDefectID = string.Empty;

            ActivityGroupExternalID = activityGroupExternalID;
            ActivityExternalID = activityExternalID;
            BFExternalID = bfExternalID;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
