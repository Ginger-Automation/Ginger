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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore
{
    public class BusinessFlow : RepositoryItemBase, IBusinessFlow
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
            a.Acts = new ObservableList<Act>();
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

        public new static partial class Fields
        {
            public static string Active = "Active";
            public static string Mandatory = "Mandatory";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string Status = "Status";
            public static string Activities = "Activities";
            public static string Variables = "Variables";
            public static string RunStatus = "RunStatus";
            public static string Elapsed = "ElapsedSecs";
            public static string Platforms = "Platforms";
            public static string EnableActivitiesVariablesDependenciesControl = "EnableActivitiesVariablesDependenciesControl";
            public static string ActivitiesGroups = "ActivitiesGroups";
            public static string AutomationPrecentage = "AutomationPrecentage";
            public static string ExternalID = "ExternalID";
            public static string PublishStatus = "PublishStatus";
            public static string Source = "Source";
        }


        public List<string> VariablesBeforeExec { get; set; }

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
                    OnPropertyChanged(Fields.Name);
                }
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(Fields.Description); } } }

        private Guid instanceGuid;
        public Guid InstanceGuid { get { return instanceGuid; } set { if (instanceGuid != value) { instanceGuid = value; OnPropertyChanged(nameof(instanceGuid)); } } }

        private string mRunDescription;
        /// <summary>
        /// Used by the user to describe the logic of the BF run with a specific set of variables values
        /// </summary>
        [IsSerializedForLocalRepository]
        public string RunDescription { get { return mRunDescription; } set { if (mRunDescription != value) { mRunDescription = value; OnPropertyChanged(Fields.RunDescription); } } }

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
                    OnPropertyChanged(Fields.Status);
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
                    OnPropertyChanged(Fields.Mandatory);
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
                    OnPropertyChanged(Fields.RunStatus);
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
                    OnPropertyChanged(Fields.Source);
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

        //[IsSerializedForLocalRepository]
        //TODO: remove and make it only platform, otherwise is save also the agent details and make isDirsty too sensitive

        //TODO:  Delete not used anymore , but keep so old BF can load till conv part is done
        public ObservableList<Platform> Platforms;

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
        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();


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
                var = (from v1 in SolutionVariables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
            if (var == null)
            {
                var = (from v1 in Variables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
                if (var == null && CurrentActivity != null)
                    var = (from v1 in CurrentActivity.Variables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
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
        public ObservableList<VariableBase> GetAllVariables(Activity CurrentActivity)
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



        [IsSerializedForLocalRepository]
        public bool EnableActivitiesVariablesDependenciesControl { get; set; }

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
            CurrentActivity.Acts.Add(act);

            if (setAfterCurrentAction)
            {
                int selectedActIndex = -1;
                if (CurrentActivity.Acts.CurrentItem != null)
                {
                    selectedActIndex = CurrentActivity.Acts.IndexOf((Act)CurrentActivity.Acts.CurrentItem);
                }
                if (selectedActIndex >= 0)
                {
                    CurrentActivity.Acts.Move(CurrentActivity.Acts.Count - 1, selectedActIndex + 1);
                }
            }
        }

        public void AddActivity(Activity a, bool setAfterCurrentActivity = false, Activity indexActivity = null)
        {
            if (a == null)
                return;
            if(indexActivity == null)
            {
                indexActivity = CurrentActivity;
            }
            if (indexActivity != null && setAfterCurrentActivity)
            {
                int selectedActivityIndex = -1;
                if (indexActivity != null)
                {
                    selectedActivityIndex = Activities.IndexOf(indexActivity);
                }

                if (selectedActivityIndex >= 0)
                {
                    Activities.Insert(selectedActivityIndex + 1, a);
                }
            }
            else
            {
                Activities.Add(a);
            }
        }

        public void InsertActivity(Activity a, int index = -1)
        {
            if (a == null)
                return;

            if ((index != -1) && (ActivitiesGroups.Count > index))
            {
                Activities.Insert(index, a);
            }
            else
            {
                Activities.Add(a);
            }
        }

        public void AddVariable(VariableBase v)
        {
            //if (v.Name == null)
            //{
            //    //make sure the new name is unique
            //    int counter = Variables.Count + 1;
            //    while ((Variables.Where(c => c.Name == "NewVar" + counter).FirstOrDefault()) != null)
            //        counter++;
            //    v.Name = "NewVar" + counter;
            //}
            //Variables.Add(v);

            if (v != null)
            {
                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
                SetUniqueVariableName(v);
                Variables.Add(v);
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
        public ObservableList<ActivitiesGroup> ActivitiesGroups = new ObservableList<ActivitiesGroup>();

        public void AddActivitiesGroup(ActivitiesGroup activitiesGroup = null)
        {
            if (activitiesGroup == null)
            {
                activitiesGroup = new ActivitiesGroup();
                activitiesGroup.Name = "New " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
            }
            SetUniqueActivitiesGroupName(activitiesGroup);
            ActivitiesGroups.Add(activitiesGroup);
        }

        public void InsertActivitiesGroup(ActivitiesGroup activitiesGroup = null, int index = -1)
        {
            if (activitiesGroup == null)
            {
                activitiesGroup = new ActivitiesGroup();
                activitiesGroup.Name = "New " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
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
        }

        public void SetUniqueActivitiesGroupName(ActivitiesGroup activitiesGroup)
        {
            if (this.ActivitiesGroups.Where(ag => ag.Name == activitiesGroup.Name).FirstOrDefault() == null) return; //no name like it in the group

            List<ActivitiesGroup> sameNameObjList =
                this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name).ToList<ActivitiesGroup>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == activitiesGroup) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
                counter++;
            activitiesGroup.Name = activitiesGroup.Name + "_" + counter.ToString();
        }
        public bool ImportActivitiesGroupActivitiesFromRepository(ActivitiesGroup activitiesGroup,
                                                                        ObservableList<Activity> activitiesRepository, bool inSilentMode = true, bool keepOriginalTargetApplicationMapping = false, Activity indexActivity = null)
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
                            SetActivityTargetApplication(actInstance);
                        if(indexActivity == null && ActivitiesGroups.Count > 1)
                        {
                            this.AddActivity(actInstance, (CurrentActivity != null), CurrentActivity);
                        }
                        else
                        {
                            this.AddActivity(actInstance, (CurrentActivity != null), indexActivity);
                        }
                        actIdent.IdentifiedActivity = actInstance;
                        indexActivity = actInstance;
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
                    Reporter.ToUser(eUserMsgKeys.PartOfActivitiesGroupActsNotFound, missingActivities);
                    return false;
                }
                else
                    return true;
            }

            if (inSilentMode == false)
                Reporter.ToUser(eUserMsgKeys.ActivitiesGroupActivitiesNotFound);
            return false;
        }

        public void AttachActivitiesGroupsAndActivities()
        {
            foreach (ActivitiesGroup group in this.ActivitiesGroups)
            {
                for (int indx = 0; indx < group.ActivitiesIdentifiers.Count;)
                {
                    ActivityIdentifiers actIdentifis = group.ActivitiesIdentifiers[indx];
                    Activity activ = this.Activities.Where(act => act.ActivityName == actIdentifis.ActivityName && act.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
                    if (activ == null)
                        activ = this.Activities.Where(act => act.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
                    if (activ == null)
                        activ = this.Activities.Where(act => act.ParentGuid == actIdentifis.ActivityGuid).FirstOrDefault();
                    if (activ != null)
                    {
                        actIdentifis.IdentifiedActivity = activ;
                        activ.ActivitiesGroupID = group.Name;
                        indx++;
                    }
                    else
                        group.ActivitiesIdentifiers.RemoveAt(indx);//Activity not exist in BF anymore
                }
            }
        }

        public enum eUpdateActivitiesGroupDetailsType { All, ClearUnExistedGroups, FreeUnAttachedActivities }
        public void UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType updateType)
        {
            switch (updateType)
            {
                case (eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups):
                    foreach (Activity act in this.Activities)
                    {
                        if (act.ActivitiesGroupID != null && act.ActivitiesGroupID != string.Empty)
                            if ((this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault()) == null)
                                act.ActivitiesGroupID = string.Empty;
                    }
                    break;

                case (eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities):
                    foreach (Activity act in this.Activities)
                    {
                        ActivitiesGroup group = this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault();
                        if (group != null)
                            if ((group.ActivitiesIdentifiers.Where(actidnt => actidnt.ActivityName == act.ActivityName && actidnt.ActivityGuid == act.Guid).FirstOrDefault()) == null)
                                act.ActivitiesGroupID = string.Empty;
                    }
                    break;

                case (eUpdateActivitiesGroupDetailsType.All):
                    UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups);
                    UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities);
                    break;
            }
        }


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

        public new void InvokPropertyChanngedForAllFields()
        {
            foreach (var field in typeof(Fields).GetFields())
                OnPropertyChanged(field.Name);
        }

        public string AutomationPrecentage
        {
            get
            {
                List<Activity> automatedActs = Activities.Where(x => x.AutomationStatus ==
                                                                               Activity.eActivityAutomationStatus.Automated).ToList();
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
                activity.TargetApplication = this.MainApplication;
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
                { mPublishStatus = value; OnPropertyChanged(Fields.PublishStatus); }
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
        public string ExecutionLogFolder { get; set; }
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

        [IsSerializedForLocalRepository]
        public ObservableList<FlowControl> BFFlowControls = new ObservableList<FlowControl>();


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
    }
}
