//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Repository;
//using Amdocs.Ginger.Common;
//using GingerCoreNET.Dictionaries;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.Common.Repository;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib
//{
//    public class BusinessFlow : RepositoryItem
//    {
//        //This class is container for AAA
//        //This class is being serialized and saved to XML when working local
//        public BusinessFlow()
//        {
//        }

//        public BusinessFlow(string sName)
//        {
//            Name = sName;
//            Activities = new ObservableList<Activity>();
//            Variables = new ObservableList<VariableBase>();
//            TargetApplications = new ObservableList<TargetApplication>();

//            Activity a = new Activity();
//            a.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
//            a.Acts = new ObservableList<Act>();
//            Activities.Add(a);
//            Activities.CurrentItem = a;
//            CurrentActivity = a;
//        }

//        public override string ToString()
//        {
//            return mName;
//        }

//        public override string GetNameForFileName() { return Name; }

//        public enum eBusinessFlowStatus
//        {
//            Unknown,
//            Candidate,
//            Development,
//            Active,
//            Suspended,
//            Retired
//        }       

//        // If BF was imported then we need to mark it in Source
//        // Can open special page to edit extra params for the BF
//        // For example Gherking page, enable to Create Scenarios
//        // We keep in External ID the ref to source file for example 
//        public enum eSource
//        {
//            Ginger,    // Create in Ginger
//            QC,         // Import from QC
//            QTP,        // From QTP convert
//            Selenium,   // From Selenium Import
//            Gherkin     // From Gherking Feature file
//        }

//        public static partial class Fields
//        {
//            public static string Active = "Active";
//            public static string Name = "Name";
//            public static string Description = "Description";
//            public static string RunDescription = "RunDescription";
//            public static string Status = "Status";
//            public static string Activities = "Activities";
//            public static string Variables = "Variables";
//            public static string RunStatus = "RunStatus";
//            public static string Elapsed = "ElapsedSecs";
//            public static string Platforms = "Platforms";
//            public static string EnableActivitiesVariablesDependenciesControl = "EnableActivitiesVariablesDependenciesControl";
//            public static string ActivitiesGroups = "ActivitiesGroups";
//            public static string AutomationPrecentage = "AutomationPrecentage";
//            public static string ExternalID = "ExternalID";
//            public static string PublishStatus = "PublishStatus";
//            public static string Source = "Source";
//        }

//        public bool UploadALMExecResultAfterRun { get; set; }

//        public List<string> VariablesBeforeExec { get; set; }

//        public List<string> SolutionVariablesBeforeExec { get; set; }

//        private string mName;

//        [IsSerializedForLocalRepository]
//        public string Name
//        {
//            get { return mName; }
//            set {
//                if (mName != value)
//                {
//                    mName = value;
//                    OnPropertyChanged(Fields.Name);
//                }
//            }
//        }

//        private string mDescription;
//        [IsSerializedForLocalRepository]
//        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(Fields.Description); } } }
        
//        private string mRunDescription;
//        /// <summary>
//        /// Used by the user to describe the logic of the BF run with a specific set of variables values
//        /// </summary>
//        [IsSerializedForLocalRepository]
//        public string RunDescription { get { return mRunDescription; } set { if (mRunDescription != value) { mRunDescription = value; OnPropertyChanged(Fields.RunDescription); } } }

//        double? mElapsed;
//        [IsSerializedForLocalRepository]
//        public double? Elapsed
//        {
//            get { return mElapsed; }
//            set
//            {
//                if (mElapsed != value)
//                {
//                    mElapsed = value;
//                    OnPropertyChanged(Fields.Elapsed);
//                }
//            }
//        }
//        public Single? ElapsedSecs
//        {
//            get
//            {
//                if (Elapsed != null)
//                {
//                    return ((Single)Elapsed / 1000);
//                }
//                else
//                {
//                    return null;
//                }
//            }

//            set
//            {

//                Elapsed = value * 1000;
//            }
//        }
//        eBusinessFlowStatus mStatus;
//        [IsSerializedForLocalRepository]
//        public eBusinessFlowStatus Status
//        {
//            get { return mStatus; }
//            set
//            {
//                if (mStatus != value)
//                {
//                    mStatus = value;
//                    OnPropertyChanged(Fields.Status);
//                }
//            }
//        }


//        private bool mActive = true;

//        [IsSerializedForLocalRepository(DefaultValue: true)]    // Why do we need to serialzie is active? for RunSet?
//        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }


//        //Runtime execution status info
//        private eRunStatus mRunStatus;
//        public eRunStatus RunStatus
//        {
//            get { return mRunStatus; }
//            set
//            {
//                if (mRunStatus != value)
//                {
//                    mRunStatus = value;
//                    OnPropertyChanged(Fields.RunStatus);
//                }
//            }
//        }


//        // Readonly for user - from where did we import the BF
//        private eSource mSource;
//        [IsSerializedForLocalRepository(DefaultValue:eSource.Ginger)]
//        public eSource Source
//        {
//            get { return mSource; }
//            set
//            {
//                if (mSource != value)
//                {
//                    mSource = value;
//                    OnPropertyChanged(Fields.Source);
//                }
//            }
//        }


//        // where is it used? why BF need env?
//        public string Environment { get; set; }
//        //@ Run info 


//        [IsSerializedForLocalRepository]
//        public ObservableList<Activity> Activities; //DO NOT USE  { get; set; } it will break repo serializer

//        [IsSerializedForLocalRepository]
//        public new string ExternalID { get; set; } // will use it for QC ID or other external ID
        
//        //TODO:  Delete not used anymore , but keep so old BF can load till conv part is done
//        public ObservableList<Platform> Platforms;
        
//        [IsSerializedForLocalRepository]
//        public ObservableList<TargetApplication> TargetApplications = new ObservableList<TargetApplication>();
        
//        private Activity mCurrentActivity { get; set; }

//        public bool disableChangeonClick = true;

//        public Activity CurrentActivity { get { return mCurrentActivity; }
//            set
//            {
//                if (mCurrentActivity != value)
//                {
//                    mCurrentActivity = value;
//                    OnPropertyChanged("CurrentActivity");
//                }
//            } }

//        [IsSerializedForLocalRepository]
//        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();

//        static public ObservableList<VariableBase> SolutionVariables;

//        public VariableBase GetVariable(string name)
//        {
//            VariableBase v = (from v1 in Variables where v1.Name == name select v1).FirstOrDefault();
//            return v;
//        }

//        public VariableBase GetHierarchyVariableByName(string varName, bool considreLinkedVar = true)
//        {
//            VariableBase var = null;
//            if (SolutionVariables != null)
//                var = (from v1 in SolutionVariables where v1.Name == varName select v1).FirstOrDefault();
//            if (var == null)
//            {
//                var = (from v1 in Variables where v1.Name == varName select v1).FirstOrDefault();
//                if (var == null && CurrentActivity != null)
//                    var = (from v1 in CurrentActivity.Variables where v1.Name == varName select v1).FirstOrDefault();
//            }

//            //check if linked variable was used and return it instead of original one if yes
//            if (considreLinkedVar==true && var != null && string.IsNullOrEmpty(var.LinkedVariableName) == false)
//            {               
//                var = GetHierarchyVariableByName(var.LinkedVariableName,false);
//            }

//            return var;
//        }

//        public VariableBase GetHierarchyVariableByNameAndType(string varName, string varType)
//        {
//            VariableBase var = null;
//            if (SolutionVariables != null)
//                var = (from v1 in SolutionVariables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
//            if (var == null)
//            {
//                var = (from v1 in Variables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
//                if (var == null && CurrentActivity != null)
//                    var = (from v1 in CurrentActivity.Variables where v1.Name == varName && v1.VariableType() == varType select v1).FirstOrDefault();
//            }

//            //check if linked variable was used and return it instead of original one if yes
//            if (var != null && string.IsNullOrEmpty(var.LinkedVariableName) == false)
//            {
//                var = GetHierarchyVariableByNameAndType(var.LinkedVariableName, varType);
//            }

//            return var;
//        }

//        public void ResetVaribles()
//        {
//            foreach (VariableBase va in Variables)
//                va.ResetValue();
//        }
//        public ObservableList<VariableBase> GetSolutionVariables()
//        {
//            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
//            if (SolutionVariables != null)
//                foreach (VariableBase var in SolutionVariables)
//                    varsList.Add(var);
//            return varsList;
//        }
//        public ObservableList<VariableBase> GetAllHierarchyVariables()
//        {
//            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
//            if (SolutionVariables != null)
//                foreach (VariableBase var in SolutionVariables)
//                    varsList.Add(var);
//            foreach (VariableBase var in Variables)
//                varsList.Add(var);
//            if (CurrentActivity != null)
//                foreach (VariableBase var in CurrentActivity.Variables)
//                    varsList.Add(var);
//            return varsList;
//        }
//        public ObservableList<VariableBase> GetAllVariables(Activity CurrentActivity)
//        {
//            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
//            if (SolutionVariables != null)
//                foreach (VariableBase var in SolutionVariables)
//                    varsList.Add(var);
//            foreach (VariableBase var in Variables)
//                varsList.Add(var);
//            if (CurrentActivity != null)
//                foreach (VariableBase var in CurrentActivity.Variables)
//                    varsList.Add(var);
//            return varsList;
//        }
        
//        public ObservableList<VariableBase> GetBFandCurrentActivityVariabeles()
//        {
//            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();
//            foreach (VariableBase var in Variables)
//                varsList.Add(var);
//            if (CurrentActivity != null)
//                foreach (VariableBase var in CurrentActivity.Variables)
//                    varsList.Add(var);
//            return varsList;
//        }
//        public ObservableList<VariableBase> GetBFandActivitiesVariabeles(bool includeParentDetails, bool includeOnlySetAsInputValue = false, bool includeOnlySetAsOutputValue = false)
//        {
//            ObservableList<VariableBase> varsList = new ObservableList<VariableBase>();

//            foreach (VariableBase var in Variables)
//            {
//                if (includeParentDetails)
//                {
//                    var.ParentType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
//                    var.ParentGuid = this.Guid;
//                    var.ParentName = this.Name;
//                }
//                if (includeOnlySetAsInputValue)
//                {
//                    if (var.SetAsInputValue)
//                        varsList.Add(var);
//                    continue;
//                }
//                if (includeOnlySetAsOutputValue)
//                {
//                    if (var.SetAsOutputValue)
//                        varsList.Add(var);
//                    continue;
//                }
//                varsList.Add(var);
//            }

//            foreach (Activity activ in Activities)
//            {
//                foreach (VariableBase var in activ.Variables)
//                {
//                    if (includeParentDetails)
//                    {
//                        var.ParentType = GingerDicser.GetTermResValue(eTermResKey.Activity);
//                        var.ParentGuid = activ.Guid;
//                        var.ParentName = activ.ActivityName;
//                    }
//                    if (includeOnlySetAsInputValue)
//                    {
//                        if (var.SetAsInputValue)
//                            varsList.Add(var);
//                        continue;
//                    }
//                    if (includeOnlySetAsOutputValue)
//                    {
//                        if (var.SetAsOutputValue)
//                            varsList.Add(var);
//                        continue;
//                    }
//                    varsList.Add(var);
//                }
//            }
//            return varsList;
//        }

//        private bool mEnableActivitiesVariablesDependenciesControl = false;
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool EnableActivitiesVariablesDependenciesControl { get { return mEnableActivitiesVariablesDependenciesControl; } set { mEnableActivitiesVariablesDependenciesControl = value; } }
        
//        /// <summary>
//        /// Function will add the act to the current Activity
//        /// </summary>
//        /// <param name="act">act object to be added</param>
//        public void AddAct(Act act, bool setAfterCurrentAction = false)
//        {
//            if (CurrentActivity == null)
//            {
//                CurrentActivity = new Activity() { Active = true };
//                //TODO: get from combo of current activity
//                CurrentActivity.ActivityName = "New";
//                //TODO: design how to connect activity and screen + screens repo
//                CurrentActivity.Screen = "Main";
//                Activities.Add(CurrentActivity);
//            }

//            act.Active = true;
//            CurrentActivity.Acts.Add(act);

//            if (setAfterCurrentAction)
//            {
//                int selectedActIndex = -1;
//                if (CurrentActivity.Acts.CurrentItem != null)
//                {
//                    selectedActIndex = CurrentActivity.Acts.IndexOf((Act)CurrentActivity.Acts.CurrentItem);
//                }
//                if (selectedActIndex >= 0)
//                {
//                    CurrentActivity.Acts.Move(CurrentActivity.Acts.Count - 1, selectedActIndex + 1);
//                }
//            }
//        }
        
//        public void AddActivity(Activity a, bool setAfterCurrentActivity = false)
//        {
//            if (a == null)
//                return;

//            if (CurrentActivity != null && setAfterCurrentActivity)
//            {
//                int selectedActivityIndex = -1;
//                if (CurrentActivity != null)
//                {
//                    selectedActivityIndex = Activities.IndexOf(CurrentActivity);
//                }

//                if (selectedActivityIndex >= 0)
//                {
//                    Activities.Insert(selectedActivityIndex + 1, a);
//                }
//            }
//            else
//            {
//                Activities.Add(a);
//            }
//        }


//        public void AddVariable(VariableBase v)
//        {
//            if (v != null)
//            {
//                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
//                SetUniqueVariableName(v);
//                Variables.Add(v);
//            }
//        }

//        public void SetUniqueVariableName(VariableBase var)
//        {
//            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
//            if (this.Variables.Where(x => x.Name == var.Name).FirstOrDefault() == null) return; //no name like it

//            List<VariableBase> sameNameObjList =
//                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
//            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

//            //Set unique name
//            int counter = 2;
//            while ((this.Variables.Where(x => x.Name == var.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
//                counter++;
//            var.Name = var.Name + "_" + counter.ToString();
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<ActivitiesGroup> ActivitiesGroups = new ObservableList<ActivitiesGroup>();

//        public void AddActivitiesGroup(ActivitiesGroup activitiesGroup = null)
//        {
//            if (activitiesGroup == null)
//            {
//                activitiesGroup = new ActivitiesGroup();
//                activitiesGroup.Name = "NewGroup";
//            }
//            SetUniqueActivitiesGroupName(activitiesGroup);
//            ActivitiesGroups.Add(activitiesGroup);
//        }

//        public void SetUniqueActivitiesGroupName(ActivitiesGroup activitiesGroup)
//        {
//            if (this.ActivitiesGroups.Where(ag => ag.Name == activitiesGroup.Name).FirstOrDefault() == null) return; //no name like it in the group

//            List<ActivitiesGroup> sameNameObjList =
//                this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name).ToList<ActivitiesGroup>();
//            if (sameNameObjList.Count == 1 && sameNameObjList[0] == activitiesGroup) return; //Same internal object

//            //Set unique name
//            int counter = 2;
//            while ((this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
//                counter++;
//            activitiesGroup.Name = activitiesGroup.Name + "_" + counter.ToString();
//        }

//        public void AttachActivitiesGroupsAndActivities()
//        {
//            foreach (ActivitiesGroup group in this.ActivitiesGroups)
//            {
//                for (int indx = 0; indx < group.ActivitiesIdentifiers.Count;)
//                {
//                    ActivityIdentifiers actIdentifis = group.ActivitiesIdentifiers[indx];
//                    Activity activ = this.Activities.Where(act => act.ActivityName == actIdentifis.ActivityName && act.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
//                    if (activ == null)
//                        activ = this.Activities.Where(act => act.Guid == actIdentifis.ActivityGuid).FirstOrDefault();
//                    if (activ == null)
//                        activ = this.Activities.Where(act => act.ParentGuid == actIdentifis.ActivityGuid).FirstOrDefault();
//                    if (activ != null)
//                    {
//                        actIdentifis.IdentifiedActivity = activ;
//                        activ.ActivitiesGroupID = group.Name;
//                        indx++;
//                    }
//                    else
//                        group.ActivitiesIdentifiers.RemoveAt(indx);//Activity not exist in BF anymore
//                }
//            }
//        }

//        public enum eUpdateActivitiesGroupDetailsType { All, ClearUnExistedGroups, FreeUnAttachedActivities }
//        public void UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType updateType)
//        {
//            switch (updateType)
//            {
//                case (eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups):
//                    foreach (Activity act in this.Activities)
//                    {
//                        if (act.ActivitiesGroupID != null && act.ActivitiesGroupID != string.Empty)
//                            if ((this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault()) == null)
//                                act.ActivitiesGroupID = string.Empty;
//                    }
//                    break;

//                case (eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities):
//                    foreach (Activity act in this.Activities)
//                    {
//                        ActivitiesGroup group = this.ActivitiesGroups.Where(actg => actg.Name == act.ActivitiesGroupID).FirstOrDefault();
//                        if (group != null)
//                            if ((group.ActivitiesIdentifiers.Where(actidnt => actidnt.ActivityName == act.ActivityName && actidnt.ActivityGuid == act.Guid).FirstOrDefault()) == null)
//                                act.ActivitiesGroupID = string.Empty;
//                    }
//                    break;

//                case (eUpdateActivitiesGroupDetailsType.All):
//                    UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups);
//                    UpdateActivitiesGroupDetails(eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities);
//                    break;
//            }
//        }

//        public int GetActionsCount()
//        {
//            int i = 0;
//            foreach (Activity a in Activities)
//            {
//                i += a.Acts.Count();
//            }
//            return i;
//        }

//        public int GetValidationsCount()
//        {
//            int i = 0;

//            foreach (Activity a in Activities)
//            {

//                foreach (Act act in a.Acts)
//                {
//                    if (act.ReturnValues != null)
//                        i += act.ReturnValues.Select(k => (!string.IsNullOrEmpty(k.Expected))).Count();
//                }
//            }

//            return i;
//        }

//        public string ScoreCard()
//        {
//            string s = "ScoreCard - ";
//            int ComplexityScore = 0;
//            if (Activities.Count > 10)
//            {
//                ComplexityScore = 50;
//            }
//            else
//            {
//                ComplexityScore = 1;
//            }
//            s += "Complexity=" + ComplexityScore;
//            return s;
//        }

//        public string MainApplication
//        {
//            get
//            {
//                if (TargetApplications != null && TargetApplications.Count() > 0)
//                {
//                    return TargetApplications[0].AppName;
//                }
//                else
//                {
//                    return null;
//                }
//            }
//        }

//        public void Reset()
//        {
//            Elapsed = null;
//            RunStatus = eRunStatus.Pending;
//            PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
//            foreach (Activity a in Activities)
//            {
//                a.Reset();
//            }
//            CleanDynamicAddedItems();
//        }

//        public void InvokPropertyChanngedForAllFields()
//        {
//            foreach (var field in typeof(Fields).GetFields())
//                OnPropertyChanged(field.Name);
//        }

//        public string AutomationPrecentage
//        {
//            get
//            {
//                List<Activity> automatedActs = Activities.Where(x => x.AutomationStatus ==
//                                                                               Activity.eActivityAutomationStatus.Automated).ToList();
//                double automatedActsPrecantge;
//                if (automatedActs == null || automatedActs.Count == 0)
//                {
//                    automatedActsPrecantge = 0;
//                }
//                else
//                {
//                    automatedActsPrecantge = ((double)automatedActs.Count / (double)Activities.Count);
//                    automatedActsPrecantge = Math.Floor(automatedActsPrecantge * 100);
//                }

//                return automatedActsPrecantge.ToString() + "%";
//            }
//        }

//        public List<StatItem> GetActivitiesStats()
//        {
//            List<StatItem> lst = new List<StatItem>();

//            var groups = Activities
//            .GroupBy(n => n.Status)
//            .Select(n => new
//            {
//                Status = n.Key.ToString(),
//                Count = n.Count()
//            }
//            )
//            .OrderBy(n => n.Status);

//            foreach (var v in groups)
//            {
//                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
//            }
//            return lst;
//        }

//        public List<StatItem> GetActionsStat()
//        {
//            List<StatItem> lst = new List<StatItem>();

//            //Get Actions of all activities
//            var groups = Activities.SelectMany(p => p.Acts)
//            .GroupBy(n => n.Status)
//            .Select(n => new
//            {
//                Status = n.Key.ToString(),
//                Count = n.Count()
//            }
//            )
//            .OrderBy(n => n.Status);

//            foreach (var v in groups)
//            {
//                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
//            }
//            return lst;
//        }

//        private void AddTotal(List<StatItem> lst)
//        {
//            double Total = (from x in lst select x.Count).Sum();
//            lst.Add(new StatItem() { Description = "Total", Count = Total });
//        }

//        public object GetValidationsStat(ref bool isValidaionsExist)
//        {
//            List<StatItem> lst = new List<StatItem>();

//            //Get Actions of all activities
//            var groups = Activities.SelectMany(p => p.Acts).Where(act => act.FailIgnored != true).SelectMany(z => z.ActReturnValues)
//            .GroupBy(n => n.Status)
//            .Select(n => new
//            {
//                Status = n.Key.ToString(),
//                Count = n.Count()
//            }
//            )
//            .OrderBy(n => n.Status);

//            foreach (var v in groups)
//            {
//                lst.Add(new StatItem() { Description = v.Status, Count = v.Count });
//            }

//            // AddTotal(lst);
//            if (groups.Count() > 0)
//                isValidaionsExist = true;
//            else
//                isValidaionsExist = false;
//            return lst;
//        }

//        public void SetActivityTargetApplication(Activity activity)
//        {
//            if (this.TargetApplications.Where(x => x.AppName == activity.TargetApplication).FirstOrDefault() == null)
//                activity.TargetApplication = this.MainApplication;
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Name;
//            }
//            set
//            {
//                this.Name = value;
//            }
//        }

//        public enum ePublishStatus
//        {
//            [EnumValueDescription("Not Published")]
//            NotPublished,
//            [EnumValueDescription("Published")]
//            Published,
//            [EnumValueDescription("Publish Failed")]
//            PublishFailed,
//        }
//        private ePublishStatus mPublishStatus = ePublishStatus.NotPublished;
//        public ePublishStatus PublishStatus
//        {
//            get { return mPublishStatus; }
//            set
//            {
//                if (mPublishStatus != value)
//                { mPublishStatus = value; OnPropertyChanged(Fields.PublishStatus); }
//            }
//        }

//        public string GetPublishStatusString()
//        {
//            switch (mPublishStatus)
//            {
//                case ePublishStatus.NotPublished:
//                    return "Not Published";
//                case ePublishStatus.Published:
//                    return "Published";
//                case ePublishStatus.PublishFailed:
//                    return "Publish Failed";

//                default: return "Not Published";
//            }
//        }

//            public Activity GetActivity(Guid guidValue, string nameValue = null)
//            {
//                Activity foundActivity = null;
//                if (guidValue != null && guidValue != Guid.Empty)
//                    foundActivity = Activities.Where(x => x.Guid == guidValue).FirstOrDefault();
//                if (foundActivity == null && nameValue != null)
//                    foundActivity = Activities.Where(x => x.ActivityName == nameValue).FirstOrDefault();
//                return foundActivity;
//            }

//            public void CleanDynamicAddedItems()
//            {
//                //clean dynamic Activities
//                // if nothing was changed and we are in lazy load then no added activities, so safe to ignore, saving time/perf
//                if (Activities.LazyLoad) return;

//                for (int i = 0; i < Activities.Count; i++)
//                {
//                    if (Activities[i].AddDynamicly)
//                    {
//                        Activities.RemoveAt(i);
//                        i--;
//                    }
//                }
//            }
        

//        public string ExecutionLogFolder { get; set; }

//        public bool BusinessFlowExecLoggerPopulated
//        {
//            get
//            {
//                return ExecutionLogFolder == null || ExecutionLogFolder == string.Empty ? false : true;
//            }
//        }

//        public int ExecutionLogActivityCounter { get; set; }

//        // Only for Run time, no need to serialize
//        public DateTime StartTimeStamp { get; set; }

//        public DateTime EndTimeStamp { get; set; }

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();
        
//        public string Applications
//        {
//            get
//            {
//                string s = "";
//                foreach(TargetApplication TA in TargetApplications)
//                {
//                    if (s.Length > 0) s += ", ";
//                    s += TA.AppName;
//                }
//                return s;
//            }
//        }
//    }
//}
