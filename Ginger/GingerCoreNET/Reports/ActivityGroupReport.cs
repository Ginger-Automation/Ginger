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

using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Run;
using GingerCore;
using GingerCore.Activities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Ginger.Reports
{
    //This is special class for report generation
    // it is in separate class from the following reason
    // it is contract - user can change the report we need to keep the fields name
    // it is faster to flatten the data instead of creating sub reports
    // it is more simple to use for users
    // it use get for each item - so on demand calculations from real object = faster
    // We create fields only for the fields we do want to expose, not exposing internal fields
    // we save on memory as we provide ref object and return on demand
    // Json will serialize only marked attr and not all
    [JsonObject(MemberSerialization.OptIn)]
    public class ActivityGroupReport
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string ExecutionDescription = "ExecutionDescription";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string ExecutionDuration = "ExecutionDuration";
            public static string Elapsed = "Elapsed";
            public static string RunStatus = "RunStatus";
            public static string NumberOfActivities = "NumberOfActivities";
            public static string PassPercent = "PassPercent";
            public static string VariablesDetails = "VariablesDetails";
            public static string ActivityDetails = "ActivityDetails";
            public static string SolutionVariablesDetails = "SolutionVariablesDetails";
        }

        private bool _showAllIterationsElements = false;

        public bool AllIterationElements
        {
            get { return _showAllIterationsElements; }
            set { _showAllIterationsElements = value; }
        }

        private ActivitiesGroup mActivitiesGroup;

        /// <summary>
        /// This tag currently used due to common use of this class by new and old report!!! Should be removed in future!
        /// </summary>
        public bool ExecutionLoggerIsEnabled = false;

        public string ExecutionLogFolder { get; set; }

        public string LogFolder { get; set; }

        // We use empty constructor when we load from file via Json
        public ActivityGroupReport()
        {

            mActivitiesGroup = new ActivitiesGroup();
        }

        public ActivityGroupReport(ActivitiesGroup AG, BusinessFlow BF)
        {
            mActivitiesGroup = AG;
            if ((AG != null) && (AG.ExecutedActivities != null))
            {
                this.ExecutedActivitiesGUID = AG.ExecutedActivities.Select(x => x.Key).ToList();
            }
        }

        [JsonProperty]
        public int Seq { get; set; }

        [JsonProperty]
        public List<Guid> ExecutedActivitiesGUID { get; set; }

        [JsonProperty]
        public string GUID { get { return mActivitiesGroup != null ? mActivitiesGroup.Guid.ToString() : SourceGuid; } set { SourceGuid = value; } }

        public string SourceGuid = string.Empty;

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Activity Group Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string Name { get { return mActivitiesGroup.Name; } set { mActivitiesGroup.Name = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Activity Group Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get { return mActivitiesGroup.Description; } set { mActivitiesGroup.Description = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Automation Percentage")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string AutomationPrecentage { get { return mActivitiesGroup.AutomationPrecentage; } set { } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime StartTimeStamp { get { return mActivitiesGroup.StartTimeStamp; }
             // !!!!!!!!!!!!!!!!!!!!!!!!!!FIXME for load
            // set { mActivitiesGroup.StartTimeStamp = value; }
             }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime EndTimeStamp { get { return mActivitiesGroup.EndTimeStamp; }
            // !!!!!!!!!!!!!!!!!!!!!!!!!!FIXME for load
            //    set { mActivitiesGroup.EndTimeStamp = value; }
        }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Elapsed")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double? Elapsed { get { return mActivitiesGroup.ElapsedSecs; } set { mActivitiesGroup.ElapsedSecs = (Single)value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Activity Group Execution Status")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunStatus
        {
            get { return mActivitiesGroup.RunStatus.ToString(); }
            set { mActivitiesGroup.RunStatus = (eActivitiesGroupRunStatus)Enum.Parse(typeof(eActivitiesGroupRunStatus), value); }
        }
       
        [FieldParams]
        [FieldParamsNameCaption("Activities Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ActivityDetails { get; set; }
    }
}
