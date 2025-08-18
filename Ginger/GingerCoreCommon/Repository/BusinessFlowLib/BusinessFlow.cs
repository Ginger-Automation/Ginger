#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using Ginger.Variables;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace GingerCore
{
    public class BusinessFlow : RepositoryItemBase
    {
        private Stopwatch _stopwatch;
        public BusinessFlow()
        {
            AllowAutoSave = true;
            this.OnDirtyStatusChanged += BusinessFlow_OnDirtyStatusChanged;
        }

        public BusinessFlow(string sName)
        {
            Name = sName;
            Activities = [];
            Variables = [];
            TargetApplications = [];
            Activity a = new Activity
            {
                Active = true,
                ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1",
                Acts = []
            };
            Activities.Add(a);
            Activities.CurrentItem = a;
            CurrentActivity = a;
            AllowAutoSave = true;

            this.OnDirtyStatusChanged += BusinessFlow_OnDirtyStatusChanged;
        }

        private void BusinessFlow_OnDirtyStatusChanged(object sender, EventArgs e)
        {
            if (DirtyStatus == eDirtyStatus.Modified)
            {
                StartTimer();
            }
        }

        public override string ToString()
        {
            return mName;
        }

        /// <summary>
        /// ID which been provided for each execution instance on the Activity
        /// </summary>
        public Guid ExecutionId { get; set; }

        public Guid ParentExecutionId { get; set; }

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

        public bool IsVirtual = false;
        public List<string> VariablesBeforeExec { get; set; }

        private TimeSpan mDevelopmentTime;
        [IsSerializedForLocalRepository]
        public TimeSpan DevelopmentTime
        {
            get
            {
                StopTimer();
                return mDevelopmentTime;
            }
            set
            {
                if (mDevelopmentTime != value)
                {
                    mDevelopmentTime = value;
                }
            }
        }

        public void StartTimer()
        {
            if (_stopwatch == null)
            {
                _stopwatch = new Stopwatch();
            }

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }
            else
            {
                _stopwatch.Restart();
            }
        }

        public void StartTimerWithActivities(IEnumerable<Activity> activitiesToStart)
        {
            try
            {
                StartTimer();

                foreach (Activity activity in activitiesToStart)
                {
                    activity.StartTimer();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while starting timer with activities", ex);
            }
        }

        public bool IsTimerRunning()
        {
            return _stopwatch != null && _stopwatch.IsRunning;
        }

        public void StopTimer()
        {
            if (_stopwatch != null && _stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                TimeSpan elapsedTime = new TimeSpan(_stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds);
                DevelopmentTime = DevelopmentTime.Add(elapsedTime);
                _stopwatch.Reset();
            }
        }

        public IEnumerable<Activity> StopTimerWithActivities()
        {
            List<Activity> stoppedActivities = [];
            try
            {
                StopTimer();


                foreach (Activity activity in Activities)
                {
                    if (activity.IsTimerRunning())
                    {
                        stoppedActivities.Add(activity);
                        activity.StopTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while stopping timer with activities", ex);
            }

            return stoppedActivities;
        }

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
        [AllowUserToEdit("Description")]
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

        private string mExternalIdCalCulated;
        public string ExternalIdCalCulated
        {
            get
            {
                return mExternalIdCalCulated;
            }
            set
            {
                if (mExternalIdCalCulated != value)
                {
                    mExternalIdCalCulated = value;
                    OnPropertyChanged(nameof(ExternalIdCalCulated));
                }
            }
        }

        double? mElapsed;
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
        [AllowUserToEdit("Status")]
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
        [AllowUserToEdit("Active")]
        [IsSerializedForLocalRepository(DefaultValue: true)]
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
        [AllowUserToEdit("Mandatory")]
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

        private bool mLazyLoadFlagForUnitTest;
        public bool LazyLoadFlagForUnitTest
        {
            get
            {
                return mLazyLoadFlagForUnitTest;
            }
            set
            {
                mLazyLoadFlagForUnitTest = value;
            }
        }

        // where is it used? why BF need env?
        public string Environment { get; set; }
        //@ Run info 


        private ObservableList<Activity> mActivities;
        /// <summary>
        /// Been used to identify if Activities were loaded by lazy load or not
        /// </summary>
        public bool ActivitiesLazyLoad { get { return (mActivities != null) && mActivities.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<Activity> Activities
        {
            get
            {
                if (mActivities == null)
                {
                    mActivities = [];
                }
                DoActivitiesLazyLoad();
                return mActivities;
            }
            set
            {
                mActivities = value;
            }
        }

        private void CheckIfLazyLoadInfoNeedsUpdate()
        {
            string folderName = this.ContainingFolderFullPath;
            if (mActivities != null && mActivities.LazyLoadDetails != null && !string.IsNullOrEmpty(mActivities.LazyLoadDetails.XmlFilePath))
            {
                string previousFilePath = mActivities.LazyLoadDetails.XmlFilePath;
                string directoryName = Path.GetDirectoryName(previousFilePath);
                if (!directoryName.Equals(folderName) && File.Exists(Path.Combine(folderName, Path.GetFileName(previousFilePath))))
                {
                    string fileName = Path.GetFileName(previousFilePath);
                    mActivities.LazyLoadDetails.XmlFilePath = Path.Combine(folderName, fileName);
                }
            }
        }

        private void DoActivitiesLazyLoad()
        {
            CheckIfLazyLoadInfoNeedsUpdate();
            if (mActivities.LazyLoad)
            {
                mActivities.LoadLazyInfo();
                mLazyLoadFlagForUnitTest = true;
                LoadLinkedActivities();
                AttachActivitiesGroupsAndActivities(mActivities);
                if (this.DirtyStatus != eDirtyStatus.NoTracked)
                {
                    this.TrackObservableList(mActivities);
                }

                IEnumerable<string> distinctTargetApp = mActivities.Select((activity) => activity.TargetApplication).Distinct();

                for (int indx = 0; indx < TargetApplications.Count;)
                {
                    if (!distinctTargetApp.Contains(TargetApplications[indx].Name))
                    {
                        TargetApplications.RemoveAt(indx);
                    }
                    else
                    {
                        indx++;
                    }
                }
            }
        }

        //    [IsSerializedForLocalRepository] Commented as it is duplicate
        //    public new string ExternalID { get; set; } // will use it for QC ID or other external ID
        [IsSerializedForLocalRepository]
        public string AlmData { get; set; }
        //[IsSerializedForLocalRepository]
        //TODO: remove and make it only platform, otherwise is save also the agent details and make isDirsty too sensitive

        //TODO:  Delete not used anymore , but keep so old BF can load till conv part is done
        // public ObservableList<Platform> Platforms;

        [IsSerializedForLocalRepository]
        public ObservableList<TargetBase> TargetApplications = [];

        public ObservableList<ApplicationPlatform> TargetApplicationPlatforms
        {
            get
            {
                ObservableList<ApplicationPlatform> appsPlatform = [];
                foreach (TargetBase target in TargetApplications)
                {
                    ApplicationPlatform appPlat = GingerCoreCommonWorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == target.Name);
                    if (appPlat != null)
                    {
                        appsPlatform.Add(appPlat);
                    }
                }
                return appsPlatform;
            }
        }

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
                    OnPropertyChanged(nameof(CurrentActivity));
                }
            }
        }

        public ActivitiesGroup CurrentActivitiesGroup
        {
            get
            {
                if (CurrentActivity != null && string.IsNullOrEmpty(CurrentActivity.ActivitiesGroupID) == false)
                {
                    return ActivitiesGroups.FirstOrDefault(x => x.Name == CurrentActivity.ActivitiesGroupID);
                }
                else
                {
                    return null;
                }
            }
        }

        private Activity mPreviousActivity;
        public Activity PreviousActivity
        {
            get
            {
                return mPreviousActivity;
            }
            set
            {
                mPreviousActivity = value;
            }
        }

        private Act mPreviousAction;
        public Act PreviousAction
        {
            get
            {
                return mPreviousAction;
            }
            set
            {
                mPreviousAction = value;
            }
        }

        private Act mLastFailedAction;
        public Act LastFailedAction
        {
            get
            {
                return mLastFailedAction;
            }
            set
            {
                mLastFailedAction = value;
            }
        }

        private Activity mErrorHandlerOriginActivity;
        public Activity ErrorHandlerOriginActivity
        {
            get
            {
                return mErrorHandlerOriginActivity;
            }
            set
            {
                mErrorHandlerOriginActivity = value;
            }
        }

        public ActivitiesGroup ErrorHandlerOriginActivitiesGroup
        {
            get
            {
                if (ErrorHandlerOriginActivity != null && string.IsNullOrEmpty(ErrorHandlerOriginActivity.ActivitiesGroupID) == false)
                {
                    return ActivitiesGroups.FirstOrDefault(x => x.Name == ErrorHandlerOriginActivity.ActivitiesGroupID);
                }
                else
                {
                    return null;
                }
            }
        }

        private Activity mLastFailedActivity;
        public Activity LastFailedActivity
        {
            get
            {
                return mLastFailedActivity;
            }
            set
            {
                mLastFailedActivity = value;
            }
        }

        private Act mErrorHandlerOriginAction;
        public Act ErrorHandlerOriginAction
        {
            get
            {
                return mErrorHandlerOriginAction;
            }
            set
            {
                mErrorHandlerOriginAction = value;
            }
        }

        private ObservableList<VariableBase> mVariables;
        /// <summary>
        /// Been used to identify if BF Variables were lazy loaded already or not
        /// </summary>
        public bool VariablesLazyLoad { get { return (mVariables != null) && mVariables.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.StringData)]
        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> Variables
        {
            get
            {
                if (mVariables == null)
                {
                    mVariables = [];
                }
                if (mVariables.LazyLoad)
                {
                    mVariables.LoadLazyInfo();
                    if (this.DirtyStatus != eDirtyStatus.NoTracked)
                    {
                        this.TrackObservableList(mVariables);
                    }
                }
                return mVariables;
            }
            set
            {
                mVariables = value;
            }
        }


        public static ObservableList<VariableBase> SolutionVariables;

        public VariableBase GetVariable(string name)
        {
            VariableBase v = Variables.FirstOrDefault(v1 => v1.Name == name);
            return v;
        }

        public VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true)
        {
            VariableBase var = null;
            if (SolutionVariables != null)
            {
                var = SolutionVariables.FirstOrDefault(v1 => v1.Name == varName);
            }
            if (var != null)
            {
                Reporter.AddFeatureUsage(FeatureId.GlobalParameter, new TelemetryMetadata()
                {
                    { "Operation", "Use" },
                });
            }
            if (var == null)
            {
                var = Variables.FirstOrDefault(v1 => v1.Name == varName);
                if (var == null && CurrentActivity != null)
                    var = CurrentActivity.Variables.FirstOrDefault(v1 => v1.Name == varName);
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
                var = SolutionVariables.FirstOrDefault(v1 => v1.Name == varName && v1.VariableType == varType);
            if (var == null)
            {
                var = Variables.FirstOrDefault(v1 => v1.Name == varName && v1.VariableType == varType);
                if (var == null && CurrentActivity != null)
                    var = CurrentActivity.Variables.FirstOrDefault(v1 => v1.Name == varName && v1.VariableType == varType);
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
            ObservableList<VariableBase> varsList = [];
            if (SolutionVariables != null)
                foreach (VariableBase var in SolutionVariables)
                    varsList.Add(var);
            return varsList;
        }
        public ObservableList<VariableBase> GetAllHierarchyVariables()
        {
            ObservableList<VariableBase> varsList = [];
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
            ObservableList<VariableBase> varsList = [];
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
            ObservableList<VariableBase> varsList = [];
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
            ObservableList<VariableBase> varsList = [.. Variables];
            if (CurrentActivity != null)
                foreach (VariableBase var in CurrentActivity.Variables)
                    varsList.Add(var);
            return varsList;
        }
        public ObservableList<VariableBase> GetBFandActivitiesVariabeles(bool includeParentDetails, bool includeOnlySetAsInputValue = false, bool includeOnlySetAsOutputValue = false, bool includeOnlyPublishedVars = false, bool includeOnlyMandatoryInputs = false)
        {
            ObservableList<VariableBase> varsList = [];

            foreach (VariableBase var in Variables)
            {
                if (includeParentDetails)
                {
                    var.ParentType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    var.ParentGuid = this.Guid;
                    var.ParentName = this.Name;
                }
                if (includeOnlyPublishedVars && var.Publish == false)
                {
                    continue;
                }
                if (includeOnlyMandatoryInputs && var.MandatoryInput == false)
                {
                    continue;
                }
                if (includeOnlySetAsInputValue && var.SetAsInputValue == false)
                {
                    continue;
                }
                if (includeOnlySetAsOutputValue && var.SetAsOutputValue == false)
                {
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
                        if (string.IsNullOrEmpty(activ.ActivitiesGroupID))
                        {
                            var.ParentName = string.Format("{0}\\{1}", this.Name, activ.ActivityName);
                        }
                        else
                        {
                            var.ParentName = string.Format("{0}\\{1}\\{2}", this.Name, activ.ActivitiesGroupID, activ.ActivityName);
                        }
                    }
                    if (includeOnlyPublishedVars && var.Publish == false)
                    {
                        continue;
                    }
                    if (includeOnlySetAsInputValue && var.SetAsInputValue == false)
                    {
                        continue;
                    }
                    if (includeOnlySetAsOutputValue && var.SetAsOutputValue == false)
                    {
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
                if (mEnableActivitiesVariablesDependenciesControl != value)
                {
                    mEnableActivitiesVariablesDependenciesControl = value;
                    OnPropertyChanged(nameof(EnableActivitiesVariablesDependenciesControl));
                }
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
                CurrentActivity = new Activity
                {
                    Active = true,                 //TODO: get from combo of current activity
                    ActivityName = "New",
                    //TODO: design how to connect activity and screen + screens repo
                    Screen = "Main"
                };
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
                        insertIndex = Activities.IndexOf(activitiesGroup.ActivitiesIdentifiers[^1].IdentifiedActivity) + 1;
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
                        activitiesGroup = ActivitiesGroups.FirstOrDefault(x => x.Name == CurrentActivity.ActivitiesGroupID);
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

            if (!string.IsNullOrEmpty(activity.TargetApplication) && !TargetApplications.Any(bfTA => ((TargetApplication)bfTA).AppName.Equals(activity.TargetApplication)))
            {
                TargetApplications.Add(GingerCoreCommonWorkSpace.Instance.Solution.GetSolutionTargetApplications().FirstOrDefault(f => f.Name.Equals(activity.TargetApplication)));
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
            if (Variables.FirstOrDefault(x => x.Name == var.Name) == null) return; //no name like it

            List<VariableBase> sameNameObjList =
                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((Variables.FirstOrDefault(x => x.Name == var.Name + "_" + counter.ToString())) != null)
                counter++;
            var.Name = var.Name + "_" + counter.ToString();
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActivitiesGroup> ActivitiesGroups { get; set; } = [];

        public ActivitiesGroup AddActivitiesGroup(ActivitiesGroup activitiesGroup = null, int index = -1)
        {
            if (activitiesGroup == null)
            {
                activitiesGroup = new ActivitiesGroup
                {
                    Name = "Group"
                };
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
            if (ActivitiesGroups.FirstOrDefault(ag => ag.Name == activitiesGroup.Name) == null) return; //no name like it in the group

            List<ActivitiesGroup> sameNameObjList =
                this.ActivitiesGroups.Where(obj => obj.Name == activitiesGroup.Name).ToList();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == activitiesGroup) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((ActivitiesGroups.FirstOrDefault(obj => obj.Name == activitiesGroup.Name + "_" + counter.ToString())) != null)
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

        private bool mAttachActivitiesGroupsWasDone = false;
        public void AttachActivitiesGroupsAndActivities(ObservableList<Activity> activitiesList = null)
        {
            if (activitiesList == null)
            {
                activitiesList = this.Activities;
            }

            //Free Activities which attached to missing group
            foreach (Activity activity in activitiesList)
            {
                ActivitiesGroup group = ActivitiesGroups.FirstOrDefault(actg => actg.Name == activity.ActivitiesGroupID);
                if (group != null)
                {
                    if ((group.ActivitiesIdentifiers.FirstOrDefault(actidnt => actidnt.ActivityName == activity.ActivityName && actidnt.ActivityGuid == activity.Guid)) == null)
                    {
                        activity.ActivitiesGroupID = string.Empty;
                    }
                }
            }

            //Attach mapped activities to groups and clear missing Activities
            foreach (ActivitiesGroup group in ActivitiesGroups)
            {
                for (int AIindex = 0; AIindex < group.ActivitiesIdentifiers.Count;)
                {
                    ActivityIdentifiers actIdentifis = group.ActivitiesIdentifiers[AIindex];
                    Activity activ = activitiesList.FirstOrDefault(x => x.ActivityName == actIdentifis.ActivityName && x.Guid == actIdentifis.ActivityGuid && x.ActivitiesGroupID == group.Name);
                    if (activ == null)
                    {
                        activ = activitiesList.FirstOrDefault(x => x.Guid == actIdentifis.ActivityGuid && x.ActivitiesGroupID == group.Name);
                    }
                    if (activ == null)
                    {
                        activ = activitiesList.FirstOrDefault(x => x.ParentGuid == actIdentifis.ActivityGuid && x.ActivitiesGroupID == group.Name);
                    }
                    if (activ != null)
                    {
                        activ.ActivitiesGroupID = group.Name;
                        AIindex++;
                    }
                    else
                    {
                        group.ActivitiesIdentifiers.RemoveAt(AIindex);//Activity not exist in BF anymore
                    }
                }

                //reorder activity-identifiers to match order the of activities
                IEnumerable<Activity> groupActivities = activitiesList.Where(a => string.Equals(a.ActivitiesGroupID, group.Name));
                List<ActivityIdentifiers> groupActivityIdentifiers = new(group.ActivitiesIdentifiers);
                group.ActivitiesIdentifiers.Clear();
                foreach (Activity activity in groupActivities)
                {
                    ActivityIdentifiers activityIdentifier = groupActivityIdentifiers
                        .FirstOrDefault(identifier =>
                            identifier.ActivityGuid == activity.Guid &&
                            string.Equals(identifier.ActivityName, activity.ActivityName));
                    if (activityIdentifier == null)
                    {
                        activityIdentifier = groupActivityIdentifiers
                            .FirstOrDefault(identifier =>
                                identifier.ActivityGuid == activity.Guid);
                    }
                    if (activityIdentifier == null)
                    {
                        activityIdentifier = groupActivityIdentifiers
                            .FirstOrDefault(identifier =>
                                identifier.ActivityGuid == activity.ParentGuid);
                    }
                    if (activityIdentifier == null)
                    {
                        group.AddActivityToGroup(activity);
                    }
                    else
                    {
                        activityIdentifier.IdentifiedActivity = activity;
                        activityIdentifier.AddDynamicly = activity.AddDynamicly;
                        activity.ActivitiesGroupID = group.Name;
                        group.ActivitiesIdentifiers.Add(activityIdentifier);
                    }
                }


                mAttachActivitiesGroupsWasDone = true;
            }

            //Attach free Activities + make sure Activities groups having valid order
            foreach (Activity activity in activitiesList)
            {
                if (string.IsNullOrEmpty(activity.ActivitiesGroupID) == true
                                || ActivitiesGroups.FirstOrDefault(actg => actg.Name == activity.ActivitiesGroupID) == null)
                {
                    //attach to Activity Group
                    if (activitiesList.IndexOf(activity) == 0)
                    {
                        ActivitiesGroup group = AddActivitiesGroup(null, 0);
                        group.AddActivityToGroup(activity);
                    }
                    else
                    {
                        ActivitiesGroup group = ActivitiesGroups.FirstOrDefault(actg => actg.Name == activitiesList[activitiesList.IndexOf(activity) - 1].ActivitiesGroupID);
                        group.AddActivityToGroup(activity);
                    }
                }
                else
                {
                    if (activitiesList.IndexOf(activity) != 0 &&
                           activity.ActivitiesGroupID != activitiesList[activitiesList.IndexOf(activity) - 1].ActivitiesGroupID)
                    {
                        bool activityGroupIsOutOfSync = false;
                        //validate it is attached in correct order
                        for (int indx = 0; indx < (activitiesList.IndexOf(activity) - 1); indx++)
                        {
                            if (activitiesList[indx].ActivitiesGroupID == activity.ActivitiesGroupID)
                            {
                                activityGroupIsOutOfSync = true;
                                break;
                            }
                        }
                        if (activityGroupIsOutOfSync)
                        {
                            //remove from previous group
                            ActivitiesGroup oldGroup = ActivitiesGroups.FirstOrDefault(x => x.Name == activity.ActivitiesGroupID);
                            if (oldGroup != null)
                            {
                                oldGroup.RemoveActivityFromGroup(activity);
                            }
                            //attach to new group which will be with similar name like older one
                            ActivitiesGroup alreadyAddedGroup = ActivitiesGroups.FirstOrDefault(x => x.Name == activity.ActivitiesGroupID + "_2");
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
            Dictionary<ActivitiesGroup, int> groupsOrderDic = [];
            int index = 0;
            foreach (Activity activity in activitiesList)
            {
                ActivitiesGroup group = ActivitiesGroups.FirstOrDefault(x => x.Name == activity.ActivitiesGroupID);
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

        public void LoadLinkedActivities(SolutionRepository solutionRepository = null)
        {
            if (this.Activities.Any(f => f.IsLinkedItem))
            {
                Parallel.For(0, this.Activities.Count, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, i =>
                {
                    if (!this.Activities[i].IsLinkedItem)
                    {
                        return;
                    }
                    Activity sharedActivity = null;

                    if (solutionRepository != null)
                    {
                        sharedActivity = solutionRepository?.GetRepositoryItemByGuid<Activity>(this.Activities[i].ParentGuid);
                    }
                    else
                    {
                        sharedActivity = GingerCoreCommonWorkSpace.Instance?.SolutionRepository?.GetRepositoryItemByGuid<Activity>(this.Activities[i].ParentGuid);
                    }

                    if (sharedActivity != null)
                    {
                        Activity copyItem = Activity.CopySharedRepositoryActivity(sharedActivity, originFromSharedRepository: true);
                        copyItem.Guid = this.Activities[i].Guid;
                        copyItem.ActivitiesGroupID = this.Activities[i].ActivitiesGroupID;
                        copyItem.Type = this.Activities[i].Type;
                        copyItem.Active = this.Activities[i].Active;
                        copyItem.DevelopmentTime = this.Activities[i].DevelopmentTime;
                        this.Activities[i] = copyItem;
                    }
                    else
                    {
                        this.Activities[i].Active = false;
                    }
                });
            }
        }

        public int GetActionsCount()
        {
            int i = 0;
            foreach (Activity a in Activities)
            {
                i += a.Acts.Count;
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
                    {
                        i += act.ReturnValues.Count(k => !string.IsNullOrEmpty(k.Expected));
                    }
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
                if (TargetApplications != null && TargetApplications.Any())
                {
                    return TargetApplications[0].Name;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Reset(bool reSetActionErrorHandlerExecutionStatus = false)
        {
            Elapsed = null;
            RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
            ExecutionFullLogFolder = string.Empty;
            foreach (Activity a in Activities)
            {
                a.Reset(reSetActionErrorHandlerExecutionStatus);
            }
            foreach (ActivitiesGroup ag in ActivitiesGroups)
            {
                ag.Reset();
            }
            CleanDynamicAddedItems();
            PreviousActivity = null;
            PreviousAction = null;
            LastFailedAction = null;
            ErrorHandlerOriginActivity = null;
            ErrorHandlerOriginAction = null;
            LastFailedActivity = null;
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
                    automatedActsPrecantge = (automatedActs.Count / (double)Activities.Count);
                    automatedActsPrecantge = Math.Floor(automatedActsPrecantge * 100);
                }

                return automatedActsPrecantge.ToString() + "%";
            }
        }

        public List<StatItem> GetActivitiesStats()
        {
            List<StatItem> lst = [];

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
            List<StatItem> lst = [];

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



        public bool ImportActivitiesGroupActivitiesFromRepository(ActivitiesGroup activitiesGroup, ObservableList<Activity> activitiesRepository, ObservableList<ApplicationPlatform> ApplicationPlatforms, bool inSilentMode = true)
        {
            string missingActivities = string.Empty;

            if (activitiesGroup != null && activitiesGroup.ActivitiesIdentifiers.Count == 0) return true;


            eUserMsgSelection userSelection = inSilentMode ? eUserMsgSelection.OK : eUserMsgSelection.None;

            //import Activities
            if (activitiesGroup != null && activitiesRepository != null)
            {
                foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                {
                    Activity repoAct = activitiesRepository.FirstOrDefault(x => x.ActivityName == actIdent.ActivityName && x.Guid == actIdent.ActivityGuid);
                    if (repoAct == null)
                        repoAct = activitiesRepository.FirstOrDefault(x => x.Guid == actIdent.ActivityGuid);
                    if (repoAct == null)
                        repoAct = activitiesRepository.FirstOrDefault(x => x.ActivityName == actIdent.ActivityName);
                    if (repoAct != null)
                    {
                        Activity actInstance = (Activity)repoAct.CreateInstance(true);
                        actInstance.ActivitiesGroupID = activitiesGroup.Name;

                        this.AddActivity(actInstance, insertIndex: this.Activities.Count);

                        userSelection = this.MapTAToBF(userSelection, actInstance, ApplicationPlatforms);

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

        /// <summary>
        /// Check if mapping Activity Target Application is missing in BF, if missing, map it to BF
        /// </summary>
        /// <param name="businessFlow">BF to check it in</param>
        /// <param name="userSelection">userselection to check if user need to be prompted or not</param>
        /// <param name="activityIns">Activity from which TA to check</param>
        /// <returns></returns>
        public eUserMsgSelection MapTAToBF(eUserMsgSelection userSelection, Activity activityIns, ObservableList<ApplicationPlatform> ApplicationPlatforms, bool silently = false)
        {
            var consumerApplicationsGUIDs = activityIns.ConsumerApplications.Select(g => g.ConsumerGuid).ToList();
            if (!TargetApplications.Any(x => x.Name == activityIns.TargetApplication) ||
                (consumerApplicationsGUIDs.Count > 0 &&
                consumerApplicationsGUIDs.Any(ca => !TargetApplications.Any(ta => ta.ParentGuid.Equals(ca)))))
            {
                if (userSelection == eUserMsgSelection.None)
                {
                    string messageToUser = "";
                    if (!TargetApplications.Any(x => x.Name == activityIns.TargetApplication))
                    {
                        messageToUser = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} is not mapped to selected {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}. Ginger will map the {GingerDicser.GetTermResValue(eTermResKey.Activity)}'s {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} to {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}.{System.Environment.NewLine} ";
                    }

                    if (silently)
                    {
                        userSelection = eUserMsgSelection.OK;
                    }
                    else
                    {
                        userSelection = Reporter.ToUser(eUserMsgKey.StaticInfoMessage, messageToUser);
                    }
                }

                if (userSelection == eUserMsgSelection.OK)
                {
                    ApplicationPlatform appAgent = ApplicationPlatforms.FirstOrDefault(x => x.AppName == activityIns.TargetApplication);

                    if (appAgent != null && !TargetApplications.Any(x => x.Name == activityIns.TargetApplication))
                    {
                        this.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName, TargetGuid = appAgent.Guid });
                    }
                }
            }

            return userSelection;
        }

        public object GetValidationsStat(ref bool isValidaionsExist)
        {
            List<StatItem> lst = [];

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

            if (groups.Any())
                isValidaionsExist = true;
            else
                isValidaionsExist = false;
            return lst;
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
                {
                    mPublishStatus = value;
                    OnPropertyChanged(nameof(PublishStatus));
                }
            }
        }

        public string GetPublishStatusString()
        {
            return mPublishStatus switch
            {
                ePublishStatus.NotPublished => "Not Published",
                ePublishStatus.Published => "Published",
                ePublishStatus.PublishFailed => "Publish Failed",
                _ => "Not Published",
            };
        }

        public Activity GetActivity(Guid guidValue, string nameValue = null)
        {
            Activity foundActivity = null;
            if (guidValue != Guid.Empty)
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
                Activity firstActive = lstActivities.FirstOrDefault(x => x.Active == true);
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


            //Remove dynamically added activities from groups identifies
            foreach (var activitiesGroup in ActivitiesGroups)
            {
                for (int index = 0; index < activitiesGroup.ActivitiesIdentifiers.Count; index++)
                {
                    if (activitiesGroup.ActivitiesIdentifiers[index].AddDynamicly)
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
                return ExecutionLogFolder != null && ExecutionLogFolder != string.Empty;
            }
        }

        public int ExecutionLogActivityCounter { get; set; }

        public int ExecutionLogActivityGroupCounter { get; set; }

        // Only for Run time, no need to serialize        
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = [];

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).FirstOrDefault(x => tagGuid.Equals(x) == true);
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
        public ObservableList<FlowControl> BFFlowControls { get; set; } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<InputVariableRule> InputVariableRules { get; set; } = [];

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
            return ActivitiesGroups.FirstOrDefault(x => x.Name == groupName);
        }

        public Tuple<ActivitiesGroup, ActivityIdentifiers> GetActivityGroupAndIdentifier(Activity activity)
        {
            ActivitiesGroup group = null;
            ActivityIdentifiers activityIdent = null;
            group = ActivitiesGroups.FirstOrDefault(x => x.Name == activity.ActivitiesGroupID);
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
            if (newIndx >= 0 && newIndx <= (Activities.Count - 1) && Activities[newIndx].ActivitiesGroupID == activityToMove.ActivitiesGroupID)
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
                        ((IObservableList)group.Item1.ActivitiesIdentifiers).Move(idntIndex, idntIndex + (newIndx - currentIndx));
                    }
                }
            }
        }

        public void MoveActivityBetweenGroups(Activity activityToMove, ActivitiesGroup targetGroup, int targetIndex = -1)
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
                        indxToMoveInBF = Activities.IndexOf(targetGroup.ActivitiesIdentifiers[^1].IdentifiedActivity) + 1;
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

        [IsSerializedForLocalRepository]
        public ObservableList<SolutionCategoryDefinition> CategoriesDefinitions = [];

        public ObservableList<SolutionCategoryDefinition> MergedCategoriesDefinitions
        {
            get
            {
                return General.MergeCategories(CategoriesDefinitions);
            }
            set
            {
                General.UpdateStoredCategories(CategoriesDefinitions, value);
            }
        }

        public override void PostDeserialization()
        {
            if (mAttachActivitiesGroupsWasDone)
            {
                AttachActivitiesGroupsAndActivities();//so attach will be done also in case BF will be reloaded by FileWatcher
            }
        }

        public Action DynamicPostSaveHandler;

        public override void PostSaveHandler()
        {
            base.PostSaveHandler();
            DynamicPostSaveHandler?.Invoke();
        }

        public bool MarkActivityAsLink(Guid activityGuid, Guid parentGuid)
        {
            if (Activities.Any(act => act.Guid == activityGuid))
            {
                Activity activity = Activities.First(a => a.Guid == activityGuid);
                bool wasPreviouslyActive = activity.Active;
                activity.Type = eSharedItemType.Link;
                activity.Active = wasPreviouslyActive;
                activity.ParentGuid = parentGuid;
                return true;
            }
            return false;
        }

        public bool UnMarkActivityAsLink(Guid activityGuid, Guid parentGuid)
        {
            if (Activities.Any(act => act.Guid == activityGuid))
            {
                Activities.FirstOrDefault(act => act.Guid == activityGuid).Type = eSharedItemType.Regular;
                Activities.FirstOrDefault(act => act.Guid == activityGuid).ParentGuid = parentGuid;
                return true;
            }
            return false;
        }

        public override void PrepareItemToBeCopied()
        {
            DoActivitiesLazyLoad();//call activities for making sure lazy load was done and activities groups attach was done
        }

        public override void UpdateCopiedItem()
        {
            AttachActivitiesGroupsAndActivities();
        }

        public override string GetItemType()
        {
            return nameof(BusinessFlow);
        }

        public void CalculateExternalId(IValueExpression ve)
        {
            if (ExternalID != null && ExternalID != string.Empty)
            {
                ve.Value = ExternalID;
                ExternalIdCalCulated = ve.ValueCalculated;
            }
        }
        public string ALMTestSetLevel { get; set; }

        public bool IsEntitySearchByName { get; set; }

        /// <summary>
        /// Gets the summary of variables in the Business flow.
        /// </summary>
        public List<General.VariableMinimalRecord> VariablesSummary
        {
            get
            {
                List<General.VariableMinimalRecord> variableDetails = [];
                foreach (VariableBase variable in Variables)
                {
                    variableDetails.Add(new General.VariableMinimalRecord(variable.Name, variable.GetInitialValue(), variable.Value));
                }
                return variableDetails;
            }
        }

        public bool AreEqual(BusinessFlow other)
        {
            if (other == null || string.IsNullOrEmpty(this.Name) || !this.Name.StartsWith(other.Name, StringComparison.InvariantCultureIgnoreCase)
                 || this.Activities.Count != other.Activities.Count
                 || this.Variables.Count != other.Variables.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Variables.Count; i++)
            {
                if (!this.Variables[i].AreEqual(other.Variables[i]))
                {
                    return false;
                }
            }

            LoadLinkedActivities();

            for (int i = 0; i < this.Activities.Count; i++)
            {
                if (!this.Activities[i].AreEqual(other.Activities[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares this instance with another object to determine if they are equal.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public bool AreEqual(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return AreEqual(obj as BusinessFlow);
        }
    }
}
