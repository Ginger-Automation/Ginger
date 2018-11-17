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
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.Core;
//using GingerCoreNET.DisplayLib;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.Common.Enums;
////TODO: change add core
//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib
//{
//    //Activity can have several steps - Acts
//    // The activities can come from external like: QC TC Step, vStorm    
//    public class Activity : RepositoryItem
//    {
//        public enum eActivityAutomationStatus
//        {
//            Development = 0,
//            Automated = 1
//        }
               
//        public enum eActionRunOption
//        {
//            [EnumValueDescription("Stop Actions Run on Failure")]
//            StopActionsRunOnFailure = 0,
//            [EnumValueDescription("Continue Actions Run on Failure")]
//            ContinueActionsRunOnFailure = 1,
//        }

//        public enum eItemParts
//        {
//            All,
//            Details,
//            Actions,
//            Variables
//        }

//        public static class Fields
//        {
//            public static string Image = "Image";
//            public static string ActivityName = "ActivityName";
//            public static string Description = "Description";
//            public static string Screen = "Screen";
//            public static string Active = "Active";
//            public static string Mandatory = "Mandatory";
//            public static string PercentAutomated = "PercentAutomated";
//            public static string Elapsed = "Elapsed";
//            public static string ElapsedSecs = "ElapsedSecs";
//            public static string Status = "Status"; //TODO: change to 'RunStatus'
//            public static string ActionRunOption = "ActionRunOption";
//            public static string Acts = "Acts";
//            public static string Expected = "Expected";
//            public static string AutomationStatus = "AutomationStatus";
//            public static string Variables = "Variables";
//            public static string VariablesNames = "VariablesNames";
//            public static string EnableActionsVariablesDependenciesControl = "EnableActionsVariablesDependenciesControl";
//            public static string TargetApplication = "TargetApplication";
//            public static string ExternalID = "ExternalID";
//            public static string ActivitiesGroupID = "ActivitiesGroupID";
//            public static string Linked = "Linked";
//            public static string IsNotGherkinOptimizedActivity = "IsNotGherkinOptimizedActivity";
//            public static string AGSelected = "AGSelected";
//        }
        
//        private bool mAGSelected;
//        public bool AGSelected { get { return mAGSelected; } set { if (mAGSelected != value) { mAGSelected = value; OnPropertyChanged(Fields.AGSelected); } } }

//        public List<string> VariablesBeforeExec { get; set; }
        
//        public override string ToString()
//        {
//            return ActivityName;
//        }
        
//        private bool mActive = true;
//        [IsSerializedForLocalRepository(DefaultValue:true)]
//        public Boolean Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(Fields.Active); } } }
        
//        //TODO: Create linked activity to item in shared repo, if true will copy: per user config: Activities, name, etc...
        
//        private string mActivityName;
//        [IsSerializedForLocalRepository]
//        public string ActivityName
//        {
//            get { return mActivityName; }
//            set
//            {
//                if (mActivityName != value)
//                {
//                    mActivityName = value;
//                    OnPropertyChanged(Fields.ActivityName);
//                }
//            }
//        }

//        private bool mMandatory;
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool Mandatory
//        {
//            get { return mMandatory; }
//            set
//            {
//                if (mMandatory != value)
//                {
//                    mMandatory = value;
//                    OnPropertyChanged(Fields.Mandatory);
//                }
//            }
//        }

//        private string mDescription;
//        [IsSerializedForLocalRepository]
//        public string Description
//        {
//            get { return mDescription; }
//            set
//            {
//                if (mDescription != value)
//                {
//                    mDescription = value;
//                    OnPropertyChanged(Fields.Description);
//                }
//            }
//        }

//        private string mExpected { get; set; }
//        [IsSerializedForLocalRepository]
//        public string Expected
//        {
//            get { return mExpected; }
//            set
//            {
//                if (mExpected != value)
//                {
//                    mExpected = value;
//                    OnPropertyChanged(Fields.Expected);
//                }
//            }
//        }

//        /// <summary>
//        /// Precentage of active Activity Actions
//        /// </summary>
//        public string PercentAutomated
//        {
//            get
//            {
//                double iAuto = 0;
//                double percent = 0;
//                if (Acts.Count != 0)
//                {
//                    foreach (Act act in Acts)
//                    {
//                        if (act.Active)
//                        {
//                            iAuto++;
//                        }
//                    }
//                    if (iAuto != 0)
//                    {
//                        percent = Math.Round((double)iAuto / Acts.Count, 2);
//                    }
//                }
//                return percent.ToString("P").Split('.')[0] + "%";
//            }
//            //TODO: fixme UCGrid does have OneWay, no need for set, but check impact
//            //set //N.C. had to include Set bc UCGrid only has TwoWay Binding and not OneWay. TODO:
//            //{
//            //    double iAuto = 0;
//            //    double percent = 0;
//            //    if (Acts.Count != 0)
//            //    {
//            //        foreach (Act act in Acts)
//            //        {
//            //            if (act.Active)
//            //            {
//            //                iAuto++;
//            //            }
//            //        }
//            //    }
//            //    if (iAuto != 0)
//            //    {
//            //        percent = Math.Round((double)iAuto / Acts.Count, 2);
//            //    }
//            //    value = percent.ToString("P").Split('.')[0] + "%";
//            //    //RaisePropertyChanged("PercentAutomated");
//            //    OnPropertyChanged(nameof(Activity.PercentAutomated));
//            //}
//        }
        
//        private eActivityAutomationStatus mAutomationStatus =  eActivityAutomationStatus.Development;
//        /// <summary>
//        /// Automation development status of the Activity
//        /// </summary>
//        [IsSerializedForLocalRepository(DefaultValue: eActivityAutomationStatus.Development)]
//        public eActivityAutomationStatus AutomationStatus
//        {
//            get { return mAutomationStatus; }
//            set
//            {
//                if (mAutomationStatus != value)
//                {
//                    mAutomationStatus = value;
//                    OnPropertyChanged(nameof(AutomationStatus));
//                }
//            }
//        }

//        eActionRunOption? mActionRunOption = eActionRunOption.StopActionsRunOnFailure;
//        [IsSerializedForLocalRepository(DefaultValue: eActionRunOption.StopActionsRunOnFailure)]
//        public eActionRunOption? ActionRunOption
//        {
//            get { return mActionRunOption; }
//            set
//            {
//                if (mActionRunOption != value)
//                {
//                    mActionRunOption = value;
//                    OnPropertyChanged(Fields.ActionRunOption);
//                }
//            }
//        }

//        private eRunStatus? mStatus;
//        /// <summary>
//        /// Run status of the activity
//        /// </summary>
//        //TODO: check if status is different
//        public eRunStatus? Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(nameof(Status)); OnPropertyChanged(nameof(StatusIcon)); } }
//        //TODO: add change history log in class and save it
        
//        private String mActivitiesGroupID;
//        /// <summary>
//        /// Used to store the Activities Group ID/Name which this Activity is belong to in the Business Flow
//        /// </summary>
//        [IsSerializedForLocalRepository]
//        public String ActivitiesGroupID 
//        { 
//            get { return mActivitiesGroupID; } 
//            set { mActivitiesGroupID = value; OnPropertyChanged(Fields.ActivitiesGroupID); } 
//        }


//        private string mTargetApplication;
//        [IsSerializedForLocalRepository]        
//        public string TargetApplication          
//        {
//            get { return mTargetApplication; }
//            set
//            {
//                if (mTargetApplication != value)
//                {
//                    mTargetApplication = value;
//                    OnPropertyChanged(Fields.TargetApplication);
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<Act> Acts = new ObservableList<Act>();


//        [IsSerializedForLocalRepository]
//        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();
        
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool EnableActionsVariablesDependenciesControl { get; set; }
        
//        public VariableBase GetVariable(string name)
//        {
//            VariableBase v = (from v1 in Variables where v1.Name == name select v1).FirstOrDefault();
//            return v;
//        }
        
//        private long? mElapsed;
//        public long? Elapsed
//        {
//            get { return mElapsed; }
//            set
//            {
//                mElapsed = value;
//                OnPropertyChanged(Fields.Elapsed);
//                OnPropertyChanged(Fields.ElapsedSecs);
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
//        }
        
//        private string mScreen { get; set; }
//        [IsSerializedForLocalRepository]
//        public string Screen
//        {
//            get { return mScreen; }
//            set
//            {
//                if (mScreen != value)
//                {
//                    mScreen = value;
//                    OnPropertyChanged(Fields.Screen);
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public string Params { get; set; } //not used
//        public override string GetNameForFileName() { return ActivityName; }
      
//        [IsSerializedForLocalRepository]
//        public ObservableList<VariableDependency> VariablesDependencies = new ObservableList<VariableDependency>();
        
//        /// <summary>
//        /// Check if the Activity supposed to be executed according to it variables dependencies configurations
//        /// </summary>
//        /// <param name="parentActivity">The Activity parent Business Flow</param>  
//        /// <param name="setActivityStatus">Define of to set the Activity Status value in case the check fails</param>   
//        /// <returns></returns>
//        public bool? CheckIfVaribalesDependenciesAllowsToRun(BusinessFlow parentBusinessFlow, bool setActivityStatus = false)
//        {
//            bool? checkStatus = null;
//            try
//            {
//                //check objects are valid
//                if (parentBusinessFlow != null)
//                {
//                    //check if the Activities-variables dependencies mechanisem is enabled
//                    if (parentBusinessFlow.EnableActivitiesVariablesDependenciesControl)
//                    {
//                        //check if the Activity configured to run with all BF selection list variables selected value
//                        List<VariableBase> bfListVars = parentBusinessFlow.Variables.Where(v => v.GetType() == typeof(VariableSelectionList) && v.Value != null).ToList();
//                        if (bfListVars != null && bfListVars.Count > 0)
//                        {
//                            foreach (VariableBase listVar in bfListVars)
//                            {
//                                VariableDependency varDep = null;
//                                if (this.VariablesDependencies != null)
//                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid).FirstOrDefault();
//                                if (varDep == null)
//                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableGuid == listVar.Guid).FirstOrDefault();
//                                if (varDep != null)
//                                {
//                                    if (!varDep.VariableValues.Contains(listVar.Value))
//                                    {
//                                        checkStatus = false;//the Selection List variable selected Value was not configured on the Activity
//                                        break;
//                                    }
//                                }
//                                else
//                                {
//                                    checkStatus = false;//the Selection List variable was not configured on the Activity
//                                    break;
//                                }
//                            }
//                            if (checkStatus == null)
//                                checkStatus = true;//All Selection List variable selected values were configured on the Activity
//                        }
//                        else
//                            checkStatus = true;//the BF dont has Selection List variables
//                    }
//                    else
//                        checkStatus = true;//the mechanisem is disabled                    
//                }
//                else
//                    checkStatus = false; //BF object is null

//                //Check failed
//                if (checkStatus == false && setActivityStatus == true)
//                {
//                    this.Status = eRunStatus.Skipped;
//                }

//                return checkStatus;
//            }
//            catch (Exception)
//            {
//                //Check failed
//                if (setActivityStatus)
//                {
//                    this.Status = eRunStatus.Skipped;
//                }
//                return false;
//            }
//        }
        
//        public void Reset()
//        {
//            Elapsed = null;
//            Status = eRunStatus.Pending;
//            foreach (Act act in Acts)
//            {
//                act.Reset();
//            }
//        }
        
//        public NewAgent CurrentAgent { get; set; }

//        public override string ItemName
//        {
//            get
//            {
//                return this.ActivityName;
//            }
//            set
//            {
//                this.ActivityName = value;
//            }
//        }
        
//        public Act GetAct(Guid guidValue, string nameValue = null)
//        {
//            Act foundAct = null;
//            if (guidValue != null && guidValue != Guid.Empty)
//                foundAct = Acts.Where(x => x.Guid == guidValue).FirstOrDefault();
//            if (foundAct == null && nameValue != null)
//                foundAct = Acts.Where(x => x.Description == nameValue).FirstOrDefault();
//            return foundAct;
//        }

//        public bool AddDynamicly { get; set; }
//        public string ExecutionLogFolder { get; set; }
//        public int ExecutionLogActionCounter { get; set; }
//        public DateTime StartTimeStamp { get; set; }
//        public DateTime EndTimeStamp { get; set; }
//        public object Reporter { get; private set; }

//        public eImageType StatusIcon
//        {
//            get
//            {
//                switch (mStatus)
//                {
//                    case eRunStatus.Passed:
//                        return eImageType.Passed;
//                    case eRunStatus.Failed:
//                        return eImageType.Failed;
//                    case eRunStatus.Pending:
//                        return eImageType.Pending;
//                    case eRunStatus.Running:
//                        return eImageType.Processing;
//                    case eRunStatus.Blocked:
//                        return eImageType.Blocked;
//                    case eRunStatus.Skipped:
//                        return eImageType.Skipped;
//                    case eRunStatus.Stopped:
//                        return eImageType.Stopped;
//                    //TODO: all the rest
//                    default:
//                        return eImageType.Empty;
//                }
//            }
//        }


//        public override bool IsTempItem
//        {
//            get
//            {
//                // no need to save activities which were added dynamically
//                if (AddDynamicly)
//                    return true;
//                else
//                    return false;                
//            }
//        }

//    }
//}
