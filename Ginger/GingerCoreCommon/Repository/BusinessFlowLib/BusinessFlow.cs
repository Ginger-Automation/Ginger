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
using Amdocs.Ginger.Common.EnumsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.GeneralLib;

namespace GingerCore
{
    public class BusinessFlow : RepositoryItemBase
    {

        public BusinessFlow()
        {

        }

        public BusinessFlow(string sName)
        {
            Name = sName;
            Activities = new ObservableList<Activity>();
            Variables = new ObservableList<VariableBase>();
            TargetApplications = new ObservableList<TargetBase>();

            Activity a = new Activity() { Active = true };
            a.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
            a.Acts = new ObservableList<IAct>();
            Activities.Add(a);
            Activities.CurrentItem = a;
            CurrentActivity = a;
        }

        public override string ToString()
        {
            return mName;
        }

        public override string GetNameForFileName() { return Name; }

        public enum eBusinessFlowStatus
        {
            Unknown,
            Candidate,
            Development,
            Active,
            Suspended,
            Retired
        }

        // If BF was imported then we need to mark it in Source
        // Can open special page to edit extra params for the BF
        // For example Gherking page, enable to Create Scenarios
        // We keep in External ID the ref to source file for example 
        public enum eSource
        {
            Ginger,    // Create in Ginger
            QC,         // Import from QC
            QTP,        // From QTP convert
            Selenium,   // From Selenium Import
            Gherkin     // From Gherking Feature file
        }
        
        public object Platforms { get; set; } // keep it for backword compatibility when loading old XML, or handle in RI serializer

        public List<string> VariablesBeforeExec { get; set; }


        // Why here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public List<string> SolutionVariablesBeforeExec { get; set; }

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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        private Guid instanceGuid;
        public Guid InstanceGuid { get { return instanceGuid; } set { if (instanceGuid != value) { instanceGuid = value; OnPropertyChanged(nameof(instanceGuid)); } } }

        private string mRunDescription;
        /// <summary>
        /// Used by the user to describe the logic of the BF run with a specific set of variables values
        /// </summary>
        [IsSerializedForLocalRepository]
        public string RunDescription
        {
            get
            {
                return mRunDescription;
            }
            set
            {
                if (mRunDescription != value)
                {
                    mRunDescription = value;
                    OnPropertyChanged(nameof(RunDescription));
                }
            }
        }

        double? mElapsed;
        [IsSerializedForLocalRepository]     // TODO: Needed?
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

                return null;
            }
            set
            {
                Elapsed = value * 1000;
            }
        }

        eBusinessFlowStatus mStatus;
        [IsSerializedForLocalRepository]
        public eBusinessFlowStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private bool mActive = true;
        [IsSerializedForLocalRepository(true)]
        public bool Active
        {
            get { return mActive; }
            set
            {
                if (mActive != value)
                {
                    mActive = value;
                    OnPropertyChanged(nameof(BusinessFlow.Active));
                }
            }
        }

        private bool mMandatory;
        [IsSerializedForLocalRepository]
        public bool Mandatory
        {
            get { return mMandatory; }
            set
            {
                if (mMandatory != value)
                {
                    mMandatory = value;
                    OnPropertyChanged(nameof(Mandatory));
                }
            }
        }
        //Runtime execution status info
        private Amdocs.Ginger.CoreNET.Execution.eRunStatus mRunStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunStatus
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


        // Readonly for user - from where did we import the BF
        private eSource mSource;
        [IsSerializedForLocalRepository]
        public eSource Source
        {
            get { return mSource; }
            set
            {
                if (mSource != value)
                {
                    mSource = value;
                    OnPropertyChanged(nameof(Source));
                }
            }
        }


        // where is it used? why BF need env?
        public string Environment { get; set; }
        //@ Run info 


        private ObservableList<Activity> mActivities;

        [IsSerializedForLocalRepository]
        public ObservableList<Activity> Activities
        {
            get
            {
                if (mActivities == null)
                {
                    mActivities = new ObservableList<Activity>();
                }
                if (mActivities.LazyLoad)
                {
                    mActivities.GetItemsInfo();
                    AttachActivitiesGroupsAndActivities();
                }
                return mActivities;
            }
            set
            {
                mActivities = value;
            }
        }

        [IsSerializedForLocalRepository]
        public new string ExternalID { get; set; } // will use it for QC ID or other external ID
        [IsSerializedForLocalRepository]
        public string AlmData { get; set; }
        //[IsSerializedForLocalRepository]
        //TODO: remove and make it only platform, otherwise is save also the agent details and make isDirsty too sensitive

        //TODO:  Delete not used anymore , but keep so old BF can load till conv part is done
        // public ObservableList<Platform> Platforms;

        [IsSerializedForLocalRepository]
        public ObservableList<TargetBase> TargetApplications = new ObservableList<TargetBase>();

        private Activity mCurrentActivity { get; set; }

        public bool disableChangeonClick = true;

        public Activity CurrentActivity
        {
            get { return mCurrentActivity; }
            set
            {
                if (mCurrentActivity != value)
                {
                    mCurrentActivity = value;
                    OnPropertyChanged("CurrentActivity");
                }
            }
        }
        
        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> Variables { get; set; } = new ObservableList<VariableBase>();


        static public ObservableList<VariableBase> SolutionVariables;

        public VariableBase GetVariable(string name)
        {
            VariableBase v = (from v1 in Variables where v1.Name == name select v1).FirstOrDefault();
            return v;
        }

        public VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true)
        {
            VariableBase var = null;
            if (SolutionVariables != null)
                var = (from v1 in SolutionVariables where v1.Name == varName select v1).FirstOrDefault();
            if (var == null)
            {
                var = (from v1 in Variables where v1.Name == varName select v1).FirstOrDefault();
                if (var == null && CurrentActivity != null)
                    var = (from v1 in CurrentActivity.Variables where v1.Name == varName select v1).FirstOrDefault();
            }

            //check if linked variable was used and return it instead of original one if yes
            if (considerLinkedVar && var != null && string.IsNullOrEmpty(var.LinkedVariableName) == false)
            {
                var = GetHierarchyVariableByName(var.LinkedVariableName, false);
            }

            return var;
        }

        public VariableBase GetHierarchyVariableByNameAndType(string varName, string varType, bool considerLinkedVar = true)
        {
            VariableBase var = null;
            if (SolutionVariables != null)
                var = (from v1 in SolutionVariables where v1.Name == varName && v1.VariableType == varType select v1).FirstOrDefault();
            if (var == null)
            {
                var = (from v1 in Variables where v1.Name == varName && v1.VariableType == varType select v1).FirstOrDefault();
                if (var == null && CurrentActivity != null)
                    var = (from v1 in CurrentActivity.Variables where v1.Name == varName && v1.VariableType == varType select v1).FirstOrDefault();
            }

            //check if linked variable was used and return it instead of original one if yes
            if (considerLinkedVar && var != null && string.IsNullOrEmpty(var.LinkedVariableName) == false)
            {
                var = GetHierarchyVariableByNameAndType(var.LinkedVariableName, varType, false);
            }

            return var;
        }

        public void ResetVaribles()
        {
            foreach (VariableBase va in Variables)
                va.ResetValue();
        }
        public ObservableList<VariableBase> GetSolutionVariables()
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
            if (SolutionVariables != null)
                foreach (VariableBase var in SolutionVariables)
                    varsList.Add(var);
            return varsList;
        }
        public ObservableList<VariableBase> GetAllHierarchyVariables()
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
            if (SolutionVariables != null)
                foreach (VariableBase var in SolutionVariables)
                    varsList.Add(var);
            foreach (VariableBase var in Variables)
                varsList.Add(var);
            if (CurrentActivity != null)
                foreach (VariableBase var in CurrentActivity.Variables)
                    varsList.Add(var);
            return varsList;
        }
        public ObservableList<VariableBase> GetAllVariables(Activity activity)
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
            if (SolutionVariables != null)
                foreach (VariableBase var in SolutionVariables)
                    varsList.Add(var);
            foreach (VariableBase var in Variables)
                varsList.Add(var);
            if (activity != null)
                foreach (VariableBase var in activity.Variables)
                    varsList.Add(var);
            return varsList;
        }

        public bool CheckIfVariableExists(string variable, Activity CurrentActivity)
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
            if (SolutionVariables != null)
                foreach (VariableBase var in SolutionVariables)
                    if (var.Name.Equals(variable))
                        return true;
            foreach (VariableBase var in Variables)
                if (var.Name.Equals(variable))
                    return true;
            if (CurrentActivity != null)
                foreach (VariableBase var in CurrentActivity.Variables)
                    if (var.Name.Equals(variable))
                        return true;
            return false;
        }
        public ObservableList<VariableBase> GetBFandCurrentActivityVariabeles()
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
            foreach (VariableBase var in Variables)
                varsList.Add(var);
            if (CurrentActivity != null)
                foreach (VariableBase var in CurrentActivity.Variables)
                    varsList.Add(var);
            return varsList;
        }
        public ObservableList<VariableBase> GetBFandActivitiesVariabeles(bool includeParentDetails, bool includeOnlySetAsInputValue = false, bool includeOnlySetAsOutputValue = false)
        {
            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();

            foreach (VariableBase var in Variables)
            {
                if (includeParentDetails)
                {
                    var.ParentType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    var.ParentGuid = this.Guid;
                    var.ParentName = this.Name;
                }
                if (includeOnlySetAsInputValue)
                {
                    if (var.SetAsInputValue)
                        varsList.Add(var);
                    continue;
                }
                if (includeOnlySetAsOutputValue)
                {
                    if (var.SetAsOutputValue)
                        varsList.Add(var);
                    continue;
                }
                varsList.Add(var);
            }

            foreach (Activity activ in Activities)
            {
                foreach (VariableBase var in activ.Variables)
                {
                    if (includeParentDetails)
                    {
                        var.ParentType = GingerDicser.GetTermResValue(eTermResKey.Activity);
                        var.ParentGuid = activ.Guid;
                        var.ParentName = activ.ActivityName;
                    }
                    if (includeOnlySetAsInputValue)
                    {
                        if (var.SetAsInputValue)
                            varsList.Add(var);
                        continue;
                    }
                    if (includeOnlySetAsOutputValue)
                    {
                        if (var.SetAsOutputValue)
                            varsList.Add(var);
                        continue;
                    }
                    varsList.Add(var);
                }
            }
            return varsList;
        }


        bool mEnableActivitiesVariablesDependenciesControl;
        [IsSerializedForLocalRepository]
        public bool EnableActivitiesVariablesDependenciesControl
        {
            get
            {
                return mEnableActivitiesVariablesDependenciesControl;
            }
            set
            {
                mEnableActivitiesVariablesDependenciesControl = value;
                OnPropertyChanged(nameof(EnableActivitiesVariablesDependenciesControl));
            }
        }

        /// <summary>
        /// Function will add the act to the current Activity
        /// </summary>
        /// <param name="act">act object to be added</param>
        public void AddAct(Act act, bool setAfterCurrentAction = false)
        {
            if (CurrentActivity == null)
            {
                CurrentActivity = new Activity() { Active = true };
                //TODO: get from combo of current activity
                CurrentActivity.ActivityName = "New";
                //TODO: design how to connect activity and screen + screens repo
                CurrentActivity.Screen = "Main";
                Activities.Add(CurrentActivity);
            }

            act.Active = true;

            int selectedActIndex = -1;
            if (CurrentActivity.Acts.CurrentItem != null)
            {
                selectedActIndex = CurrentActivity.Acts.IndexOf((Act)CurrentActivity.Acts.CurrentItem);
            }

            CurrentActivity.Acts.Add(act);

            if (setAfterCurrentAction && CurrentActivity.Acts.Count > 2 && selectedActIndex >= 0)
            {
                CurrentActivity.Acts.Move(CurrentActivity.Acts.Count - 1, selectedActIndex + 1);
            }
        }
        
        public void AddActivity(Activity activity, ActivitiesGroup activitiesGroup = null, int insertIndex = -1, bool setAsCurrent = true)
        {
            if (activity == null)
            {
                return;
            }

            if (insertIndex == -1)
            {
                insertIndex = 0;

                if (activitiesGroup != null)
                {
                    if (activitiesGroup.ActivitiesIdentifiers.Count > 0)
                    {
                        insertIndex = Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[activitiesGroup.ActivitiesIdentifiers.Count - 1].IdentifiedActivity) + 1;
                    }
                    else
                    {
                        insertIndex = Activities.Count;//last
                    }                    
                }
                else if (CurrentActivity != null)
                {
                    if (string.IsNullOrEmpty(CurrentActivity.ActivitiesGroupID) == false)
                    {
                        activitiesGroup = this.ActivitiesGroups.Where(x => x.Name == CurrentActivity.ActivitiesGroupID).FirstOrDefault();
                        insertIndex = Activities.IndexOf(CurrentActivity);
                        while (!string.IsNullOrEmpty(Activities[insertIndex].ActivitiesGroupID) && Activities[insertIndex].ActivitiesGroupID.Equals(activitiesGroup?.Name) == true)
                        {
                            insertIndex++;
                            if (insertIndex >= Activities.Count)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        insertIndex = Activities.IndexOf(CurrentActivity) + 1;
                    }
                }
            }

            if (activitiesGroup != null)
            {
                activitiesGroup.AddActivityToGroup(activity);
            }

            if (insertIndex >= 0)
            {
                Activities.Insert(insertIndex, activity);
            }
            else
            {
                Activities.Add(activity);
            }

            if (setAsCurrent)
            {
                CurrentActivity = activity;
            }
        }

        public void AddVariable(VariableBase v, int insertIndex = -1)
        {
            if (v != null)
            {
                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
                SetUniqueVariableName(v);
                
                if (insertIndex < 0 || insertIndex > Variables.Count - 1)
                {
                    Variables.Add(v);
                }
                else
                {
                    Variables.Insert(insertIndex, v);
                }
            }
        }

        public void SetUniqueVariableName(VariableBase var)
        {
            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
            if (this.Variables.Where(x => x.Name == var.Name).FirstOrDefault() == null) return; //no name like it

            List<VariableBase> sameNameObjList =
                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((this.Variables.Where(x => x.Name == var.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
                counter++;
            var.Name = var.Name + "_" + counter.ToString();
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActivitiesGroup> ActivitiesGroups { get; set; } = new ObservableList<ActivitiesGroup>();

        public ActivitiesGroup AddActivitiesGroup(ActivitiesGroup activitiesGroup = null, int index = -1)
        {
            if (activitiesGroup == null)
            {
                activitiesGroup = new ActivitiesGroup();
                activitiesGroup.Name = "Group";
            }
            SetUniqueActivitiesGroupName(activitiesGroup);     

            if ((index != -1) && (ActivitiesGroups.Count > index))
            {
                ActivitiesGroups.Insert(index, activitiesGroup);
            }
            else
            {
                ActivitiesGroups.Add(activitiesGroup);
            }

            return activitiesGroup;
        }

        public void SetUniqueActivitiesGroupName(ActivitiesGroup activitiesGroup)
        {
            if (this.ActivitiesGroups.Where(ag => ag.Name == activitiesGroup.Name).FirstOrDefault() == null) return; //no name like it in the group

            List<ActivitiesGroup> sameNameObjList =
                this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name).ToList();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == activitiesGroup) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
                counter++;
            activitiesGroup.Name = activitiesGroup.Name + "_" + counter.ToString();
        }

        //public void SetUniqueGroupColor(ActivitiesGroup activitiesGroup)
        //{
        //    List<eColorName> listOfColors = Enum.GetValues(typeof(eColorName)).Cast<eColorName>().ToList();
        //    Random rnd = new Random();            
        //    List<int> triedColorIndx = new List<int>();
        //    int rndColorIndx = 0;

        //    while (true)
        //    {
        //        rndColorIndx = rnd.Next(0, listOfColors.Count);
        //        while (triedColorIndx.Contains(rndColorIndx) == true && triedColorIndx.Count < listOfColors.Count)
        //        {
        //            rndColorIndx = rnd.Next(0, listOfColors.Count);
        //        }
        //        triedColorIndx.Add(rndColorIndx);

        //        if (this.ActivitiesGroups.Where(ag => ag.GroupColor == listOfColors[rndColorIndx].ToString()).FirstOrDefault() == null || triedColorIndx.Count >= listOfColors.Count)
        //        {
        //            activitiesGroup.GroupColor = listOfColors[rndColorIndx].ToString();
        //            break; 
        //        }
        //    }
        //}

        public bool ImportActivitiesGroupActivitiesFromRepository(ActivitiesGroup activitiesGroup, ObservableList<Activity> activitiesRepository, bool inSilentMode = true, bool keepOriginalTargetApplicationMapping = false)
        {
            string missingActivities = string.Empty;

            if (activitiesGroup != null && activitiesGroup.ActivitiesIdentifiers.Count == 0) return true;

            //import Activities
            if (activitiesGroup != null && activitiesRepository != null)
            {
                foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                {
                    Activity repoAct = activitiesRepository.Where(x => x.ActivityName == actIdent.ActivityName && x.Guid == actIdent.ActivityGuid).FirstOrDefault();
                    if (repoAct == null)
                        repoAct = activitiesRepository.Where(x => x.Guid == actIdent.ActivityGuid).FirstOrDefault();
                    if (repoAct == null)
                        repoAct = activitiesRepository.Where(x => x.ActivityName == actIdent.ActivityName).FirstOrDefault();
                    if (repoAct != null)
                    {
                        Activity actInstance = (Activity)repoAct.CreateInstance(true);
                        actInstance.ActivitiesGroupID = activitiesGroup.Name;
                        
                        if (keepOriginalTargetApplicationMapping == false)
                        {
                            SetActivityTargetApplication(actInstance);
                        }

                        this.AddActivity(actInstance, insertIndex:this.Activities.Count);
                        actIdent.IdentifiedActivity = actInstance;
                    }
                    else
                    {
                        missingActivities += "'" + actIdent.ActivityName + "', ";
                    }
                }

                //notify on missing activities
                if (missingActivities != string.Empty && inSilentMode == false)
                {
                    missingActivities = missingActivities.TrimEnd(new char[] { ',', ' ' });
                    Reporter.ToUser(eUserMsgKey.PartOfActivitiesGroupActsNotFound, missingActivities);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (inSilentMode == false)
            {
                Reporter.ToUser(eUserMsgKey.ActivitiesGroupActivitiesNotFound);
            }

            return false;
        }

        public void AttachActivitiesGroupsAndActivities()
        {
            //Free Activities which attached to missing group
            foreach (Activity activity in this.Activities)
            {
                ActivitiesGroup group = ActivitiesGroups.Where(actg => actg.Name == activity.ActivitiesGroupID).FirstOrDefault();
                if (group != null)
                {
                    if ((group.ActivitiesIdentifiers.Where(actidnt => actidnt.ActivityName == activity.ActivityName && actidnt.ActivityGuid == activity.Guid).FirstOrDefault()) == null)
                    {
                        activity.ActivitiesGroupID = string.Empty;
                    }
                }
            }

            //Attach mapped activities to groups nad clear missing Activities
            foreach (ActivitiesGroup group in this.ActivitiesGroups)
            {
                for (int indx = 0; indx < group.ActivitiesIdentifiers.Count;)
                {
                    ActivityIdentifiers actIdentifis = (ActivityIdentifiers)group.ActivitiesIdentifiers[indx];
                    Activity activ = this.Activities.Where(x => x.ActivityName == actIdentifis.ActivityName && x.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
                    if (activ == null)
                    {
                        activ = this.Activities.Where(x => x.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
                    }
                    if (activ == null)
                    {
                        activ = this.Activities.Where(x => x.ParentGuid == actIdentifis.ActivityGuid).FirstOrDefault();
                    }
                    if (activ != null)
                    {
                        //actIdentifis.IdentifiedActivity = (Activity)activ;
                        activ.ActivitiesGroupID = group.Name;
                        indx++;
                    }
                    else
                    {
                        group.ActivitiesIdentifiers.RemoveAt(indx);//Activity not exist in BF anymore
                    }
                }

                //re-add the activities in correct order
                group.ActivitiesIdentifiers.Clear();
                foreach (Activity activity in Activities.Where(x => x.ActivitiesGroupID == group.Name).ToList())
                {
                    group.AddActivityToGroup(activity);
                }
            }

            //Attach free Activities + make sure Activities groups having valid order
            foreach (Activity activity in this.Activities)
            {
                if (string.IsNullOrEmpty(activity.ActivitiesGroupID) == true
                                || ActivitiesGroups.Where(actg => actg.Name == activity.ActivitiesGroupID).FirstOrDefault() == null)
                {
                    //attach to Activity Group
                    if (Activities.IndexOf(activity) == 0)
                    {
                        ActivitiesGroup group = AddActivitiesGroup(null, 0);
                        group.AddActivityToGroup(activity);
                    }
                    else
                    {
                        ActivitiesGroup group = ActivitiesGroups.Where(actg => actg.Name == Activities[Activities.IndexOf(activity) - 1].ActivitiesGroupID).FirstOrDefault();
                        group.AddActivityToGroup(activity);
                    }
                }
                else
                {
                    if (Activities.IndexOf(activity) != 0 &&
                           activity.ActivitiesGroupID != Activities[Activities.IndexOf(activity) - 1].ActivitiesGroupID)
                    {
                        bool activityGroupIsOutOfSync = false;
                        //validate it is attached in correct order
                        for (int indx = 0; indx < (Activities.IndexOf(activity) - 1); indx++)
                        {
                            if (Activities[indx].ActivitiesGroupID == activity.ActivitiesGroupID)
                            {
                                activityGroupIsOutOfSync = true;
                                break;
                            }
                        }
                        if (activityGroupIsOutOfSync)
                        {
                            //remove from previous group
                            ActivitiesGroup oldGroup = ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
                            if (oldGroup != null)
                            {
                                oldGroup.RemoveActivityFromGroup(activity);
                            }
                            //attach to new group which will be with similar name like older one
                            ActivitiesGroup alreadyAddedGroup = ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID + "_2").FirstOrDefault();
                            if (alreadyAddedGroup != null)
                            {
                                alreadyAddedGroup.AddActivityToGroup(activity);
                            }
                            else
                            {
                                ActivitiesGroup newGroup = AddActivitiesGroup(new ActivitiesGroup() { Name = activity.ActivitiesGroupID + "_2" });
                                newGroup.AddActivityToGroup(activity);
                            }
                        }
                    }
                }
            }

            //Make sure groups order is according to flow
            Dictionary<ActivitiesGroup, int> groupsOrderDic = new Dictionary<ActivitiesGroup, int>();
            int index = 0;
            foreach (Activity activity in Activities)
            {
                ActivitiesGroup group = ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
                if (!groupsOrderDic.ContainsKey(group))
                {
                    groupsOrderDic.Add(group, index);
                    index++;
                }
            }
            foreach (ActivitiesGroup group in groupsOrderDic.Keys)
            {
                if (ActivitiesGroups.IndexOf(group) != groupsOrderDic[group])
                {
                    ActivitiesGroups.Move(ActivitiesGroups.IndexOf(group), groupsOrderDic[group]);
                }
            }
        }

        //public enum eUpdateActivitiesGroupDetailsType { All, ClearUnExistedGroups, FreeUnAttachedActivities }
        //public void UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType updateType)
        //{
        //    switch (updateType)
        //    {
        //        case (eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups):
        //            foreach (Activity act in this.Activities)
        //            {
        //                if (act.ActivitiesGroupID != null && act.ActivitiesGroupID != string.Empty)
        //                {
        //                    if ((this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault()) == null)
        //                    {
        //                        act.ActivitiesGroupID = string.Empty;
        //                        //act.ActivitiesGroupColor = string.Empty;
        //                    }
        //                }
        //            }
        //            break;

        //        case (eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities):
        //            foreach (Activity act in this.Activities)
        //            {
        //                ActivitiesGroup group = this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault();
        //                if (group != null)
        //                {
        //                    if ((group.ActivitiesIdentifiers.Where(actidnt => actidnt.ActivityName == act.ActivityName && actidnt.ActivityGuid == act.Guid).FirstOrDefault()) == null)
        //                    {
        //                        act.ActivitiesGroupID = string.Empty;
        //                        //act.ActivitiesGroupColor = string.Empty;
        //                    }
        //                }
        //            }
        //            break;

        //        case (eUpdateActivitiesGroupDetailsType.All):
        //            UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups);
        //            UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities);
        //            break;
        //    }
        //}


        public int GetActionsCount()
        {
            int i = 0;
            foreach (Activity a in Activities)
            {
                i += a.Acts.Count();
            }
            return i;
        }

        public int GetValidationsCount()
        {
            int i = 0;

            foreach (Activity a in Activities)
            {

                foreach (Act act in a.Acts)
                {
                    if (act.ReturnValues != null)
                        i += act.ReturnValues.Select(k => (!string.IsNullOrEmpty(k.Expected))).Count();
                }
            }

            return i;
        }

        public string ScoreCard()
        {
            string s = "ScoreCard - ";
            int ComplexityScore = 0;
            if (Activities.Count > 10)
            {
                ComplexityScore = 50;
            }
            else
            {
                ComplexityScore = 1;
            }
            s += "Complexity=" + ComplexityScore;
            return s;
        }


        public string MainApplication
        {
            get
            {
                if (TargetApplications != null && TargetApplications.Count() > 0)
                {
                    return TargetApplications[0].Name;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Reset()
        {
            Elapsed = null;
            RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
            ExecutionFullLogFolder = string.Empty;
            foreach (Activity a in Activities)
            {
                a.Reset();
            }
            foreach (ActivitiesGroup ag in ActivitiesGroups)
            {
                ag.Reset();
            }
            CleanDynamicAddedItems();
        }

        public string AutomationPrecentage
        {
            get
            {
                List<Activity> automatedActs = Activities.Where(x => x.AutomationStatus == eActivityAutomationStatus.Automated).ToList();
                double automatedActsPrecantge;
                if (automatedActs == null || automatedActs.Count == 0)
                {
                    automatedActsPrecantge = 0;
                }
                else
                {
                    automatedActsPrecantge = ((double)automatedActs.Count / (double)Activities.Count);
                    automatedActsPrecantge = Math.Floor(automatedActsPrecantge * 100);
                }

                return automatedActsPrecantge.ToString() + "%";
            }
        }

        public List<StatItem> GetActivitiesStats()
        {
            List<StatItem> lst = new List<StatItem>();

            var groups = Activities
            .GroupBy(n => n.Status)
            .Select(n => new
            {
                Status = n.Key.ToString(),
                Count = n.Count()
            }
            )
            .OrderBy(n => n.Status);

            foreach (var v in groups)
            {
                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
            }
            return lst;
        }

        public List<StatItem> GetActionsStat()
        {
            List<StatItem> lst = new List<StatItem>();

            //Get Actions of all activities
            var groups = Activities.SelectMany(p => p.Acts)
            .GroupBy(n => n.Status)
            .Select(n => new
            {
                Status = n.Key.ToString(),
                Count = n.Count()
            }
            )
            .OrderBy(n => n.Status);

            foreach (var v in groups)
            {
                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
            }
            return lst;
        }


        private void AddTotal(List<StatItem> lst)
        {
            double Total = (from x in lst select x.Count).Sum();
            lst.Add(new StatItem() { Description = "Total", Count = Total });
        }






        public object GetValidationsStat(ref bool isValidaionsExist)
        {
            List<StatItem> lst = new List<StatItem>();

            //Get Actions of all activities
            var groups = Activities.SelectMany(p => p.Acts).Where(act => act.FailIgnored != true).SelectMany(z => z.ActReturnValues)
            .GroupBy(n => n.Status)
            .Select(n => new
            {
                Status = n.Key.ToString(),
                Count = n.Count()
            }
            )
            .OrderBy(n => n.Status);

            foreach (var v in groups)
            {
                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
            }

            if (groups.Count() > 0)
                isValidaionsExist = true;
            else
                isValidaionsExist = false;
            return lst;
        }

        public void SetActivityTargetApplication(Activity activity)
        {
            if (this.TargetApplications.Where(x => x.Name == activity.TargetApplication).FirstOrDefault() == null)
            {
                activity.TargetApplication = this.MainApplication;
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

        public enum ePublishStatus
        {
            [EnumValueDescription("Not Published")]
            NotPublished,
            [EnumValueDescription("Published")]
            Published,
            [EnumValueDescription("Publish Failed")]
            PublishFailed,
        }
        private ePublishStatus mPublishStatus = ePublishStatus.NotPublished;
        public ePublishStatus PublishStatus
        {
            get { return mPublishStatus; }
            set
            {
                if (mPublishStatus != value)
                { mPublishStatus = value; OnPropertyChanged(nameof(PublishStatus)); }
            }
        }

        public string GetPublishStatusString()
        {
            switch (mPublishStatus)
            {
                case ePublishStatus.NotPublished:
                    return "Not Published";
                case ePublishStatus.Published:
                    return "Published";
                case ePublishStatus.PublishFailed:
                    return "Publish Failed";

                default: return "Not Published";
            }
        }

        public Activity GetActivity(Guid guidValue, string nameValue = null)
        {
            Activity foundActivity = null;
            if (guidValue != null && guidValue != Guid.Empty)
                foundActivity = GetActivityFromPossibleList(guidValue.ToString());
            if (foundActivity == null && guidValue == Guid.Empty && nameValue != null)//look by name only if do not have GUID so only old flows will still work with name mapping
                foundActivity = GetActivityFromPossibleList(nameToLookBy: nameValue);
            return foundActivity;
        }

        private Activity GetActivityFromPossibleList(string guidToLookByString = null, string nameToLookBy = null)
        {

            Guid guidToLookBy = Guid.Empty;
            if (!string.IsNullOrEmpty(guidToLookByString))
            {
                guidToLookBy = Guid.Parse(guidToLookByString);
            }
            List<Activity> lstActivities = null;
            if (guidToLookBy != Guid.Empty)
                lstActivities = Activities.Where(x => x.Guid == guidToLookBy).ToList();
            else
                lstActivities = Activities.Where(x => x.ActivityName == nameToLookBy).ToList();

            if (lstActivities == null || lstActivities.Count == 0)
                return null;
            else if (lstActivities.Count == 1)
                return lstActivities[0];
            else//we have more than 1
            {
                Activity firstActive = lstActivities.Where(x => x.Active == true).FirstOrDefault();
                if (firstActive != null)
                    return firstActive;
                else
                    return lstActivities[0];//no one is Active so returning the first one
            }
        }

        public void CleanDynamicAddedItems()
        {
            //clean dynamic Activities
            // if nothing was changed and we are in lazy load then no added activities, so safe to ignore, saving time/perf
            if (Activities.LazyLoad) return;


            //Remove dyamically added activities from groups identifies
            foreach (var activitiesGroup in ActivitiesGroups)
            {
                for(int index=0; index < activitiesGroup.ActivitiesIdentifiers.Count; index++)
                {
                    if(activitiesGroup.ActivitiesIdentifiers[index].AddDynamicly)
                    {
                        activitiesGroup.ActivitiesIdentifiers.RemoveAt(index);
                        index--;
                    }
                }
            }

            //Remove dynamically added activities from Business flow
            for (int i = 0; i < Activities.Count; i++)
            {
                if (Activities[i].AddDynamicly)
                {
                    Activities.RemoveAt(i);                   
                    i--;
                }
            }
        }


        public string ExecutionFullLogFolder { get; set; }
        public string ExecutionLogFolder { get; set; } = string.Empty;
        public bool BusinessFlowExecLoggerPopulated
        {
            get
            {
                return ExecutionLogFolder == null || ExecutionLogFolder == string.Empty ? false : true;
            }
        }

        public int ExecutionLogActivityCounter { get; set; }

        public int ExecutionLogActivityGroupCounter { get; set; }

        // Only for Run time, no need to serialize        
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

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

        public ObservableList<VariableBase> GetVariables()
        {
            return Variables;
        }

        //ObservableList<VariableBase> BusinessFlow.GetVariables()
        //{
        //    throw new NotImplementedException();
        //}

        //VariableBase BusinessFlow.GetHierarchyVariableByName(string varName, bool considerLinkedVar)
        //{
        //    throw new NotImplementedException();
        //}

        //BusinessFlow BusinessFlow.CreateCopy(bool v)
        //{
        //    throw new NotImplementedException();
        //}

        [IsSerializedForLocalRepository]
        public ObservableList<FlowControl> BFFlowControls { get; set; } = new ObservableList<FlowControl>();


        public string Applications
        {
            get
            {
                string s = "";
                foreach (TargetApplication TA in TargetApplications)
                {
                    if (s.Length > 0) s += ", ";
                    s += TA.AppName;
                }
                return s;
            }
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.BusinessFlow;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
        public void OffilinePropertiesPrep(string logFolderPath)
        {
            ExecutionLogFolder = logFolderPath;
            VariablesBeforeExec = Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
            SolutionVariablesBeforeExec = GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
            ExecutionLogActivityCounter = 1;
        }

        public void MoveActivitiesGroupUp(ActivitiesGroup groupToMove)
        {
            if (groupToMove != null && ActivitiesGroups.IndexOf(groupToMove) > 0)
            {
                int groupIndex = ActivitiesGroups.IndexOf(groupToMove);
                ActivitiesGroup groupAbove = ActivitiesGroups[groupIndex - 1];

                //move group up
                ActivitiesGroups.Move(groupIndex, groupIndex - 1);

                if (groupToMove.ActivitiesIdentifiers.Count > 0)
                {
                    //move group activities up
                    foreach (ActivityIdentifiers activityIdent in groupToMove.ActivitiesIdentifiers)
                    {
                        Activities.Move(Activities.IndexOf(activityIdent.IdentifiedActivity), Activities.IndexOf(activityIdent.IdentifiedActivity) - groupAbove.ActivitiesIdentifiers.Count);
                    }
                }
            }
        }

        public void MoveActivitiesGroupDown(ActivitiesGroup groupToMove)
        {
            if (groupToMove != null && ActivitiesGroups.IndexOf(groupToMove) < (ActivitiesGroups.Count - 1))
            {
                int groupIndex = ActivitiesGroups.IndexOf(groupToMove);
                ActivitiesGroup groupBlow = ActivitiesGroups[groupIndex + 1];

                //move group down
                ActivitiesGroups.Move(groupIndex, groupIndex + 1);

                if (groupToMove.ActivitiesIdentifiers.Count > 0)
                {
                    //move group activities down by shifting below group up
                    foreach (ActivityIdentifiers activityIdent in groupBlow.ActivitiesIdentifiers)
                    {
                        Activities.Move(Activities.IndexOf(activityIdent.IdentifiedActivity), Activities.IndexOf(activityIdent.IdentifiedActivity) - groupToMove.ActivitiesIdentifiers.Count);
                    }
                }
            }
        }

        public void DeleteActivitiesGroup(ActivitiesGroup groupToDelete)
        {
            //delete group
            ActivitiesGroups.Remove(groupToDelete);

            if (groupToDelete.ActivitiesIdentifiers.Count > 0)
            {
                //delete group activities 
                foreach (ActivityIdentifiers activityIdent in groupToDelete.ActivitiesIdentifiers)
                {
                    Activities.Remove(activityIdent.IdentifiedActivity);
                }
            }
        }

        public ActivitiesGroup GetActivitiesGroupByName(string groupName)
        {
            return ActivitiesGroups.Where(x => x.Name == groupName).FirstOrDefault();
        }

        public Tuple<ActivitiesGroup, ActivityIdentifiers> GetActivityGroupAndIdentifier(Activity activity)
        {
            ActivitiesGroup group = null; ;
            ActivityIdentifiers activityIdent = null;
            group = ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
            if (group != null)
            {
                activityIdent = group.GetActivityIdentifiers(activity);
            }
            return Tuple.Create(group, activityIdent);
        }

        public void DeleteActivity(Activity activityToDelete)
        {
            //remove from group
            Tuple<ActivitiesGroup, ActivityIdentifiers> group = GetActivityGroupAndIdentifier(activityToDelete);
            if (group.Item1 != null && group.Item2 != null)
            {
                group.Item1.ActivitiesIdentifiers.Remove(group.Item2);
            }

            //remove from list of Activites
            Activities.Remove(activityToDelete);
        }

        //public void MoveActivityUp(Activity activityToMove)
        //{
        //    int index = Activities.IndexOf(activityToMove);
        //    if (index > 0 && Activities[index - 1].ActivitiesGroupID == activityToMove.ActivitiesGroupID)
        //    {
        //        //move Activity
        //        Activities.Move(index, index - 1);

        //        //update move in group
        //        Tuple<ActivitiesGroup, ActivityIdentifiers> group = GetActivityGroupAndIdentifier(activityToMove);
        //        if (group.Item1 != null && group.Item2 != null)
        //        {
        //            int idntIndex = group.Item1.ActivitiesIdentifiers.IndexOf(group.Item2);
        //            if (idntIndex > 0)
        //            {
        //                group.Item1.ActivitiesIdentifiers.Move(idntIndex, idntIndex - 1);
        //            }
        //        }
        //    }
        //}

        //public void MoveActivityDown(Activity activityToMove)
        //{
        //    int index = Activities.IndexOf(activityToMove);
        //    if (index < (Activities.Count-1) && Activities[index + 1].ActivitiesGroupID == activityToMove.ActivitiesGroupID)
        //    {
        //        //move Activity
        //        Activities.Move(index, index + 1);

        //        //update move in group
        //        Tuple<ActivitiesGroup, ActivityIdentifiers> group = GetActivityGroupAndIdentifier(activityToMove);
        //        if (group.Item1 != null && group.Item2 != null)
        //        {
        //            int idntIndex = group.Item1.ActivitiesIdentifiers.IndexOf(group.Item2);
        //            if (idntIndex < (group.Item1.ActivitiesIdentifiers.Count - 1))
        //            {
        //                group.Item1.ActivitiesIdentifiers.Move(idntIndex, idntIndex + 1);
        //            }
        //        }
        //    }
        //}

        public void MoveActivityInGroup(Activity activityToMove, int newIndx)
        {
            int currentIndx = Activities.IndexOf(activityToMove);
            if (newIndx >= 0 && newIndx <= (Activities.Count-1) && Activities[newIndx].ActivitiesGroupID == activityToMove.ActivitiesGroupID)
            {
                //move Activity in BF
                Activities.Move(currentIndx, newIndx);

                //update move in group
                Tuple<ActivitiesGroup, ActivityIdentifiers> group = GetActivityGroupAndIdentifier(activityToMove);
                if (group.Item1 != null && group.Item2 != null)
                {
                    int idntIndex = group.Item1.ActivitiesIdentifiers.IndexOf(group.Item2);
                    if (idntIndex >= 0)
                    {
                        group.Item1.ActivitiesIdentifiers.Move(idntIndex, idntIndex + (newIndx - currentIndx));
                    }
                }
            }
        }

        public void MoveActivityBetweenGroups(Activity activityToMove, ActivitiesGroup targetGroup, int targetIndex=-1)
        {
            if (targetGroup == null)
            {
                return;
            }

            //remove from first group
            Tuple<ActivitiesGroup, ActivityIdentifiers> existingGroup = GetActivityGroupAndIdentifier(activityToMove);
            if (existingGroup.Item1 != null && existingGroup.Item2 != null)
            {
                existingGroup.Item1.RemoveActivityFromGroup(activityToMove);
                
                //add to target group                               
                if (targetIndex >= 0)
                {                    
                    //move in group
                    Activity currentActivityInTargetIndx = Activities[targetIndex];
                    int targetIndxInGroup = targetGroup.ActivitiesIdentifiers.IndexOf(targetGroup.GetActivityIdentifiers(currentActivityInTargetIndx));
                    targetGroup.AddActivityToGroup(activityToMove, targetIndxInGroup);
                    //move in BF
                    if (targetIndex > Activities.IndexOf(activityToMove))
                    {
                        targetIndex--;
                    }
                    Activities.Move(Activities.IndexOf(activityToMove), targetIndex);                    
                }
                else
                {                    
                    int indxToMoveInBF = Activities.Count - 1;
                    if (targetGroup.ActivitiesIdentifiers.Count > 0)
                    {
                        indxToMoveInBF = Activities.IndexOf(targetGroup.ActivitiesIdentifiers[targetGroup.ActivitiesIdentifiers.Count-1].IdentifiedActivity) + 1;
                    }
                    //move in group
                    targetGroup.AddActivityToGroup(activityToMove);
                    //move in BF
                    if (indxToMoveInBF > Activities.IndexOf(activityToMove))
                    {
                        indxToMoveInBF--;
                    }
                    Activities.Move(Activities.IndexOf(activityToMove), indxToMoveInBF);
                }
            }

        }

    }
}
