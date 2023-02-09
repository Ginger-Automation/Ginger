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
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Amdocs.Ginger.Repository;


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

        private executionLoggerStatus _executionLoggerStatus = executionLoggerStatus.NotStartedYet;

        public executionLoggerStatus ExecutionLoggerStatus
        {
            get { return _executionLoggerStatus; }
            set { _executionLoggerStatus = value; }
        }

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

                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActivityIdentifiers> ActivitiesIdentifiers { get; set; } = new ObservableList<ActivityIdentifiers>();

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

        /// <summary>
        /// ID which been provided for each execution instance on the Activity
        /// </summary>
        public Guid ExecutionId { get; set; }

        public Guid ParentExecutionId { get; set; }

        public void AddActivityToGroup(Activity activity, int insertIndx = -1)
        {
            if (activity == null)
            {
                return;
            }
            ActivityIdentifiers actIdents = new ActivityIdentifiers();
            actIdents.IdentifiedActivity = activity;
            actIdents.AddDynamicly = activity.AddDynamicly;
            activity.ActivitiesGroupID = this.Name;

            if (insertIndx >= 0)
            {
                this.ActivitiesIdentifiers.Insert(insertIndx, actIdents);
            }
            else
            {
                this.ActivitiesIdentifiers.Add(actIdents);
            }
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
                foreach (ActivityIdentifiers actIdent in ActivitiesIdentifiers)
                {
                    ((ActivityIdentifiers)actIdent).RefreshActivityIdentifiers();
                }
                List<ActivityIdentifiers> automatedActsInGroup = ActivitiesIdentifiers.Where(x => x.ActivityAutomationStatus ==
                                                                            eActivityAutomationStatus.Automated).ToList();
                double automatedActsPrecanteg;
                if (automatedActsInGroup == null || automatedActsInGroup.Count == 0)
                {
                    automatedActsPrecanteg = 0;
                }
                else
                {
                    automatedActsPrecanteg = ((double)automatedActsInGroup.Count / (double)ActivitiesIdentifiers.Count);
                    automatedActsPrecanteg = Math.Floor(automatedActsPrecanteg * 100);
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

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null, object extraDetails = null)
        {
            ActivitiesGroup activitiesGroupInstance = (ActivitiesGroup)instance;

            //Create new instance of source
            ActivitiesGroup newInstance = (ActivitiesGroup)this.CreateInstance();
            newInstance.IsSharedRepositoryInstance = true;

            //update required part
            ActivitiesGroup.eItemParts ePartToUpdate = (ActivitiesGroup.eItemParts)Enum.Parse(typeof(ActivitiesGroup.eItemParts), partToUpdate);

            if (hostItem != null)
            {
                //replace old instance object with new
                BusinessFlow currentBF = ((BusinessFlow)hostItem);

                // Update details
                activitiesGroupInstance.Name = newInstance.Name;
                activitiesGroupInstance.Description = newInstance.Description;
                activitiesGroupInstance.Tags = newInstance.Tags;

                // Confirm if no group exists in the Business Flow with same name
                currentBF.SetUniqueActivitiesGroupName(activitiesGroupInstance);

                if (ePartToUpdate == eItemParts.Details)
                {
                    currentBF.AttachActivitiesGroupsAndActivities();
                    return;
                }

                int grpIndex = currentBF.ActivitiesGroups.IndexOf(activitiesGroupInstance);
                if (grpIndex >= 0)
                {
                    int insertIndex = currentBF.Activities.IndexOf(currentBF.Activities.Where(a => a.Guid == activitiesGroupInstance.ActivitiesIdentifiers[0].ActivityGuid).FirstOrDefault());

                    List<Activity> existingActivities = new List<Activity>();

                    int exActCount = activitiesGroupInstance.ActivitiesIdentifiers.Count;
                    for (int i = exActCount - 1; i >= 0; i--)
                    {
                        ActivityIdentifiers actIDexist = activitiesGroupInstance.ActivitiesIdentifiers[i];

                        Activity exAct = currentBF.Activities.Where(g => g.Guid == actIDexist.ActivityGuid).FirstOrDefault();

                        if (exAct == null)
                        {
                            exAct = currentBF.Activities.Where(g => g.ParentGuid == actIDexist.ActivityGuid).FirstOrDefault();
                        }

                        if (exAct != null)
                        {
                            // Add to the list of deleted activities
                            existingActivities.Add(exAct);

                            // Remove the activity from the Business Flow
                            currentBF.DeleteActivity(exAct);
                        }
                    }

                    // Add the activities to the Business Flow in sequence they appear in the Updated Shared Group
                    foreach (ActivityIdentifiers actID in newInstance.ActivitiesIdentifiers)
                    {
                        Activity updatedAct = null;

                        // Activity still exist in the group, thus re-add the same activity to the group
                        updatedAct = existingActivities.Where(a => a.Guid == actID.ActivityGuid).FirstOrDefault();

                        // In case, group was Replaced/Overwritten
                        if (updatedAct == null)
                        {
                            updatedAct = existingActivities.Where(a => a.ParentGuid == actID.ActivityGuid).FirstOrDefault();
                        }

                        if (updatedAct != null)
                        {
                            currentBF.AddActivity(updatedAct, activitiesGroupInstance, insertIndex);
                            insertIndex++;
                            existingActivities.Remove(updatedAct);
                        }
                        // Activity doesn't exist in the group and the shared group is recently updated by addition of this activity, thus add this activity to the group instance in the Business Flow
                        else if (extraDetails != null) //not used anywhere, is needed?
                        {
                            updatedAct = (extraDetails as ObservableList<Activity>).Where(a => a.ActivityName == actID.ActivityName && a.Guid == actID.ActivityGuid).FirstOrDefault();
                            if (updatedAct == null)
                            {
                                updatedAct = (extraDetails as ObservableList<Activity>).Where(a => a.Guid == actID.ActivityGuid).FirstOrDefault();
                            }
                            if (updatedAct == null)
                            {
                                updatedAct = (extraDetails as ObservableList<Activity>).Where(a => a.ActivityName == actID.ActivityName).FirstOrDefault();
                            }

                            if (updatedAct != null)
                            {
                                updatedAct = updatedAct.CreateInstance(true) as Activity;
                                currentBF.AddActivity(updatedAct, activitiesGroupInstance, insertIndex);
                                insertIndex++;
                            }
                        }
                        // Activity doesn't exist in the group and the shared group is recently updated by addition of this activity, thus add this activity to the group instance in the Business Flow
                        else
                        {
                            updatedAct = GingerCoreCommonWorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<Activity>(actID.ActivityGuid);
                            updatedAct = updatedAct.CreateInstance(true) as Activity;
                            currentBF.AddActivity(updatedAct, activitiesGroupInstance, insertIndex);
                            insertIndex++;
                        }

                    }

                    currentBF.AttachActivitiesGroupsAndActivities();
                }
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
                    else
                    {
                        foreach(ActivityIdentifiers actIdentify in updatedGroup.ActivitiesIdentifiers)
                        {
                            if(actIdentify.ActivityParentGuid != Guid.Empty)
                            {
                                actIdentify.ActivityGuid = actIdentify.ActivityParentGuid;
                            }
                        }    
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
        //[IsSerializedForLocalRepository]    // !!!!!!!!!!!!!!!!!!!!! Why serialized?
        public double? Elapsed
        {
            get { return mElapsed; }
            set
            {
                if (mElapsed != value)
                {
                    mElapsed = value;
                    OnPropertyChanged(nameof(Elapsed));
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

                Elapsed = (uint)value * 1000;
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

        public Dictionary<Guid, uint> ExecutedActivities { get; set; } = new Dictionary<Guid, uint>();

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
                    OnPropertyChanged(nameof(RunStatus));
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

        //private string mGroupColor;
        //[IsSerializedForLocalRepository]
        //public string GroupColor
        //{
        //    get { return mGroupColor; }
        //    set
        //    {
        //        if (mGroupColor != value)
        //        {
        //            mGroupColor = value;

        //            foreach (ActivityIdentifiers aIdent in ActivitiesIdentifiers)
        //                if (aIdent.IdentifiedActivity != null)
        //                    aIdent.IdentifiedActivity.ActivitiesGroupColor = mGroupColor;

        //            OnPropertyChanged(nameof(GroupColor));
        //        }
        //    }
        //}

        public void ChangeName(string newName)
        {
            Name = newName;
            foreach (ActivityIdentifiers activityIdent in ActivitiesIdentifiers)
            {
                activityIdent.IdentifiedActivity.ActivitiesGroupID = newName;
            }
        }

        public ActivityIdentifiers GetActivityIdentifiers(Activity activity)
        {
            return ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity).FirstOrDefault();
        }

        public override void PrepareItemToBeCopied()
        {
            this.IsSharedRepositoryInstance = TargetFrameworkHelper.Helper.IsSharedRepositoryItem(this);
        }

        public override string GetItemType()
        {
            return nameof(ActivitiesGroup);
        }

    }
}
