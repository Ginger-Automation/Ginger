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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Properties;
using GingerCore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Activities
{
    /// <summary>
    /// ActivitiesGroup used to store the ID's and execution order of few Activities
    /// </summary>
    public class ActivitiesGroup : RepositoryItemBase
    {
        public enum eItemParts
        {
            All,
            Details,
            Activities
        }

        public enum eActivitiesGroupRunStatus
        {
            Pending,
            Running,
            Passed,
            Failed,
            Stopped,
            Blocked,
            Skipped
        }

        public enum executionLoggerStatus
        {
            NotStartedYet,
            StartedNotFinishedYet,
            Finished
        }

        private executionLoggerStatus _executionLoggerStatus = executionLoggerStatus.NotStartedYet;

        public executionLoggerStatus ExecutionLoggerStatus
        {
            get { return _executionLoggerStatus; }
            set { _executionLoggerStatus = value; }
        }

        public new static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string ActivitiesIdentifiers = "ActivitiesIdentifiers";
            public static string Elapsed = "ElapsedSecs";
            public static string AutomationPrecentage = "AutomationPrecentage";
            public static string RefreshfromALMOption = "RefreshfromALMOption";
            public static string RunStatus = "RunStatus";
            public static string TestSuiteTitle = "TestSuiteTitle";
            public static string TestSuiteId = "TestSuiteId";
        }

        //public virtual System.Drawing.Image Image { get { return Resources.Group_16x16; } }

        public ActivitiesGroup()
        {
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;

                    foreach (ActivityIdentifiers aIdent in ActivitiesIdentifiers)
                        if (aIdent.IdentifiedActivity != null)
                            aIdent.IdentifiedActivity.ActivitiesGroupID = mName;

                    OnPropertyChanged(Fields.Name);
                }
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(Fields.Description);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActivityIdentifiers> ActivitiesIdentifiers = new ObservableList<ActivityIdentifiers>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        [IsSerializedForLocalRepository]
        public string TestSuiteTitle { get; set; }

        [IsSerializedForLocalRepository]
        public string TestSuiteId { get; set; }

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).Where(x => tagGuid.Equals(x) == true).FirstOrDefault();
                        if (!guid.Equals(Guid.Empty))
                            return true;
                    }
                    break;
            }
            return false;
        }

        public override string GetNameForFileName() { return Name; }

        public void AddActivityToGroup(Activity activity)
        {
            if (activity == null)
                return;
            ActivityIdentifiers actIdents = new ActivityIdentifiers();
            actIdents.IdentifiedActivity = activity;
            activity.ActivitiesGroupID = this.Name;
            this.ActivitiesIdentifiers.Add(actIdents);
        }

        public bool CheckActivityInGroup(Activity activity)
        {
            ObservableList<ActivityIdentifiers> lstactIden = this.ActivitiesIdentifiers;
            foreach (ActivityIdentifiers actIdents in lstactIden)
            {
                if (actIdents.IdentifiedActivity == activity)
                    return true;
            }
            return false;
        }

        public void RemoveActivityFromGroup(Activity activity)
        {
            ObservableList<ActivityIdentifiers> lstactIden = this.ActivitiesIdentifiers;
            foreach (ActivityIdentifiers actIdents in lstactIden)
            {
                if (actIdents.IdentifiedActivity == activity)
                {
                    this.ActivitiesIdentifiers.Remove(actIdents);
                    break;
                }
                    
            }            
        }

        [IsSerializedForLocalRepository]
        public string ExternalID2 { get; set; } // used to store the actual TC ID when importing it from QC in case the TC type is linked TC


        public string AutomationPrecentage 
        { 
            get
            {
                foreach (ActivityIdentifiers actIdent in ActivitiesIdentifiers) actIdent.RefreshActivityIdentifiers();
                List<ActivityIdentifiers> automatedActsInGroup = ActivitiesIdentifiers.Where(x=>x.ActivityAutomationStatus ==
                                                                               Activity.eActivityAutomationStatus.Automated).ToList();
                double automatedActsPrecanteg;
                if (automatedActsInGroup == null || automatedActsInGroup.Count == 0)
                {
                    automatedActsPrecanteg=0;
                }
                else
                {
                    automatedActsPrecanteg = ((double)automatedActsInGroup.Count / (double)ActivitiesIdentifiers.Count);
                    automatedActsPrecanteg = Math.Floor(automatedActsPrecanteg *100);                    
                }

                return automatedActsPrecanteg.ToString() + "%";
            }
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null)
        {
            ActivitiesGroup activitiesGroupInstance = (ActivitiesGroup)instance;

            //Create new instance of source
            ActivitiesGroup newInstance = (ActivitiesGroup)this.CreateInstance();
            newInstance.IsSharedRepositoryInstance = true;

            //update required part
            ActivitiesGroup.eItemParts ePartToUpdate = (ActivitiesGroup.eItemParts)Enum.Parse(typeof(ActivitiesGroup.eItemParts), partToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                case eItemParts.Details:
                    newInstance.Guid = activitiesGroupInstance.Guid;
                    newInstance.ParentGuid = activitiesGroupInstance.ParentGuid;
                    newInstance.ExternalID = activitiesGroupInstance.ExternalID;
                    if (ePartToUpdate == eItemParts.Details)
                    {
                        //keep other parts
                        newInstance.ActivitiesIdentifiers = activitiesGroupInstance.ActivitiesIdentifiers;
                    }
                    if (hostItem != null)
                    {
                        //replace old instance object with new
                        int originalIndex = ((BusinessFlow)hostItem).ActivitiesGroups.IndexOf(activitiesGroupInstance);
                        ((BusinessFlow)hostItem).ActivitiesGroups.Remove(activitiesGroupInstance);
                        ((BusinessFlow)hostItem).SetUniqueActivitiesGroupName(newInstance);
                        ((BusinessFlow)hostItem).ActivitiesGroups.Insert(originalIndex, newInstance);
                        ((BusinessFlow)hostItem).AttachActivitiesGroupsAndActivities();
                        ((BusinessFlow)hostItem).UpdateActivitiesGroupDetails(BusinessFlow.eUpdateActivitiesGroupDetailsType.All);
                    }
                    break;
                case eItemParts.Activities:
                    activitiesGroupInstance.ActivitiesIdentifiers = newInstance.ActivitiesIdentifiers;
                    break;
            }    
        }


        public override RepositoryItemBase GetUpdatedRepoItem(RepositoryItemBase itemToUpload, RepositoryItemBase existingRepoItem, string itemPartToUpdate)
        {
            ActivitiesGroup updatedGroup = null;
                   
            //update required part
            eItemParts ePartToUpdate = (eItemParts)Enum.Parse(typeof(eItemParts), itemPartToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                case eItemParts.Details:
                    updatedGroup = (ActivitiesGroup)itemToUpload.CreateCopy(false);

                    if (ePartToUpdate == eItemParts.Details)
                    {
                        updatedGroup.ActivitiesIdentifiers = ((ActivitiesGroup)existingRepoItem).ActivitiesIdentifiers;
                    }
                  
                    break;
                case eItemParts.Activities:
                    updatedGroup = (ActivitiesGroup)existingRepoItem.CreateCopy(false);
                    updatedGroup.ActivitiesIdentifiers = ((ActivitiesGroup)itemToUpload).ActivitiesIdentifiers;
                    break;
            }

            return updatedGroup;
        }

        public void Reset()
        {
            Elapsed = null;
            ExecutionLoggerStatus = executionLoggerStatus.NotStartedYet;
            RunStatus = eActivitiesGroupRunStatus.Pending;
        }

        double? mElapsed;
        [IsSerializedForLocalRepository]
        public double? Elapsed
        {
            get { return mElapsed; }
            set
            {
                if (mElapsed != value)
                {
                    mElapsed = value;
                    OnPropertyChanged(Fields.Elapsed);
                }
            }
        }
        public Single? ElapsedSecs
        {
            get
            {
                if (Elapsed != null)
                {
                    return ((Single)Elapsed / 1000);
                }
                else
                {
                    return null;
                }
            }

            set
            {

                Elapsed = value * 1000;
            }
        }

        public string ExecutionLogFolder { get; set; }
        public bool ActivitiesGroupExecLoggerPopulated
        {
            get
            {
                return ExecutionLogFolder == null || ExecutionLogFolder == string.Empty ? false : true;
            }
        }

        public Dictionary<Guid, DateTime> ExecutedActivities = new Dictionary<Guid, DateTime>();

        public string TempReportFolder { get; set; }

        // Only for Run time, no need to serialize
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        private eActivitiesGroupRunStatus mRunStatus;
        public eActivitiesGroupRunStatus RunStatus
        {
            get { return mRunStatus; }
            set
            {
                if (mRunStatus != value)
                {
                    mRunStatus = value;
                    OnPropertyChanged(Fields.RunStatus);
                }
            }
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.ActivitiesGroup;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
    }
}
