#region License
/*
Copyright © 2014-2019 European Support Limited

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

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null)
        {
            ActivitiesGroup activitiesGroupInstance = (ActivitiesGroup)instance;

            //Create new instance of source
            ActivitiesGroup newInstance = (ActivitiesGroup)this.CreateInstance();
            newInstance.IsSharedRepositoryInstance = true;

            newInstance.ActivitiesIdentifiers = this.ActivitiesIdentifiers;
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
                        BusinessFlow currentBF = ((BusinessFlow)hostItem);

                        int grpIndex = currentBF.ActivitiesGroups.IndexOf(activitiesGroupInstance);
                        if (grpIndex >= 0)
                        {
                            int firstActivityIndex = currentBF.Activities.IndexOf(currentBF.ActivitiesGroups[grpIndex].ActivitiesIdentifiers.FirstOrDefault().IdentifiedActivity);
                            if (firstActivityIndex == -1)
                            {
                                firstActivityIndex = currentBF.Activities.IndexOf(currentBF.Activities.Where(a => a.Guid == currentBF.ActivitiesGroups[grpIndex].ActivitiesIdentifiers.FirstOrDefault().ActivityGuid).FirstOrDefault());
                            }

                            int insertIndex = firstActivityIndex;

                            // look for Activities that were added to this Group, and Add those into the current Business Flow
                            List<ActivityIdentifiers> missingActivitiesInBF = newInstance.ActivitiesIdentifiers.Where(m => !currentBF.Activities.Any(x => x.Guid == m.ActivityGuid) && !currentBF.Activities.Any(x => x.ParentGuid == m.ActivityGuid)).ToList();

                            if (missingActivitiesInBF != null && missingActivitiesInBF.Count > 0)
                            {
                                foreach (ActivityIdentifiers srActivityIdentifier in missingActivitiesInBF)
                                {
                                    //int aiIndex = this.ActivitiesIdentifiers.IndexOf(srActivityIdentifier);
                                    int aiIndex = this.ActivitiesIdentifiers.IndexOf(this.ActivitiesIdentifiers.Where(a => a.ActivityGuid == srActivityIdentifier.ActivityGuid).FirstOrDefault());
                                    Activity newSRActInst = this.ActivitiesIdentifiers[aiIndex].IdentifiedActivity.CreateInstance(true) as Activity;
                                    currentBF.SetActivityTargetApplication(newSRActInst);
                                    insertIndex++;
                                    currentBF.Activities.Insert(insertIndex, newSRActInst);
                                    srActivityIdentifier.IdentifiedActivity = newSRActInst;
                                }
                            }

                            // Check in case Activities were moved/shifted
                            ReArrangeActivitiesSequenceBasedOnGroup(currentBF, firstActivityIndex, newInstance);

                            currentBF.AttachActivitiesGroupsAndActivities();

                            // look for Activities that were deleted from this Group, and delete those from the current Business Flow
                            List<Activity> missingActivitiesFromBF = currentBF.Activities.Where(m => m.ActivitiesGroupID == newInstance.Name && !newInstance.ActivitiesIdentifiers.Any(x => x.ActivityGuid == m.Guid) && !newInstance.ActivitiesIdentifiers.Any(x => x.ActivityGuid == m.ParentGuid)).ToList();
                            if (missingActivitiesFromBF != null && missingActivitiesFromBF.Count > 0)
                            {
                                foreach (Activity srActivity in missingActivitiesFromBF)
                                {
                                    currentBF.Activities.Remove(srActivity);
                                }
                            }

                            currentBF.ActivitiesGroups.Remove(activitiesGroupInstance);
                            currentBF.ActivitiesGroups.Insert(grpIndex, newInstance);

                            currentBF.ActivitiesGroups = currentBF.ActivitiesGroups;
                            currentBF.Activities = currentBF.Activities;
                        }
                    }
                    break;
                case eItemParts.Activities:
                    activitiesGroupInstance.ActivitiesIdentifiers = newInstance.ActivitiesIdentifiers;
                    if (hostItem != null)
                    {
                        //replace old instance object with new
                        BusinessFlow currentBF = ((BusinessFlow)hostItem);

                        //int grpIndex = currentBF.ActivitiesGroups.IndexOf(activitiesGroupInstance);
                        //currentBF.ActivitiesGroups.Remove(activitiesGroupInstance);
                        //currentBF.ActivitiesGroups.Insert(grpIndex, activitiesGroupInstance);

                        //if(activitiesList.Count != grpActivitiesCount)
                        //{
                        //    activitiesList = new ObservableList<Activity>(this.ActivitiesGroups.SelectMany(g => g.ActivitiesIdentifiers).Where(a => a.IdentifiedActivity != null).Select(a => a.IdentifiedActivity).ToList());
                        //}

                        currentBF.AttachActivitiesGroupsAndActivities();
                        currentBF.ActivitiesGroups = currentBF.ActivitiesGroups;
                        currentBF.Activities = currentBF.Activities;
                    }
                    break;
            }
        }

        private void ReArrangeActivitiesSequenceBasedOnGroup(BusinessFlow currentBF, int firstActivityIndex, ActivitiesGroup actGroup)
        {
            //if (firstActivityIndex >= 0)
            //{
            //    for (int i = firstActivityIndex; i < firstActivityIndex + this.ActivitiesIdentifiers.Count; i++)
            //    {
            //        Activity bfActivity = currentBF.Activities[i];
            //        Activity grpActivity = this.ActivitiesIdentifiers[i - firstActivityIndex].IdentifiedActivity;

            //        if (bfActivity.Guid != grpActivity.Guid)
            //        {
            //            int indexInGrp = this.ActivitiesIdentifiers.IndexOf(this.ActivitiesIdentifiers.Where(a => a.ActivityGuid == bfActivity.Guid).FirstOrDefault());
            //            currentBF.Activities.Move(i, firstActivityIndex + indexInGrp);
            //        }
            //    }
            //}

            if (firstActivityIndex >= 0)
            {
                for (int c = firstActivityIndex; c < firstActivityIndex + actGroup.ActivitiesIdentifiers.Count; c++)
                {
                    if (currentBF.Activities[c].Guid != actGroup.ActivitiesIdentifiers[c - firstActivityIndex].ActivityGuid)
                    {
                        int posInGroup = actGroup.ActivitiesIdentifiers.IndexOf(actGroup.ActivitiesIdentifiers.Where(ac => ac.ActivityGuid == currentBF.Activities[c].Guid).FirstOrDefault());
                        currentBF.Activities.Move(c, firstActivityIndex + posInGroup);
                    }
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
        [IsSerializedForLocalRepository]    // !!!!!!!!!!!!!!!!!!!!! Why serialized?
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
        public uint StartTimeStamp { get; set; }

        public uint EndTimeStamp { get; set; }

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

    }
}
