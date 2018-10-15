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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GingerCore.Actions;
using System.Windows;
using GingerCore.Variables;
using GingerCore.Properties;
using Amdocs.Ginger.Common.Enums;

//TODO: chang add core
namespace GingerCore
{
    // Activity can have several steps - Acts
    // The activities can come from external like: QC TC Step, vStorm    
    public class Activity : RepositoryItemBase
    {
        public enum eActivityAutomationStatus
        {
            Development = 0,
            Automated = 1
        }
       
        public enum eActionRunOption
        {
            [EnumValueDescription("Stop Actions Run on Failure")]
            StopActionsRunOnFailure = 0,
            [EnumValueDescription("Continue Actions Run on Failure")]
            ContinueActionsRunOnFailure = 1,
        }

        public enum eItemParts
        {
            All,
            Details,
            Actions,
            Variables
        }


        public enum eHandlerMappingType
        {
            [EnumValueDescription("All Available Error Handlers ")]
            AllAvailableHandlers = 0,
            None = 1,
            [EnumValueDescription("Specific Error Handlers")]
            SpecificErrorHandlers = 2
        }
        
        public new static class Fields
        {            
            public static string ActivityName = "ActivityName";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string Screen = "Screen";
            //public static string Params = "Params";
            public static string Active = "Active";
            public static string Mandatory = "Mandatory";
            public static string PercentAutomated = "PercentAutomated";
            //public static string Automated = "Automated";
            public static string Elapsed = "Elapsed";
            public static string ElapsedSecs = "ElapsedSecs";
            public static string Status = "Status"; //TODO: change to 'RunStatus'
            public static string ActionRunOption = "ActionRunOption";
            public static string Acts = "Acts";
            public static string Expected = "Expected";
            public static string AutomationStatus = "AutomationStatus";
            public static string Variables = "Variables";
            public static string VariablesNames = "VariablesNames";
            public static string EnableActionsVariablesDependenciesControl = "EnableActionsVariablesDependenciesControl";
            public static string TargetApplication = "TargetApplication";
            public static string ExternalID = "ExternalID";
            public static string ActivitiesGroupID = "ActivitiesGroupID";
            public static string Linked = "Linked";
            public static string IsNotGherkinOptimizedActivity = "IsNotGherkinOptimizedActivity";
            public static string AGSelected = "AGSelected";
            public static string SelectedForConversion = "SelectedForConversion";
            public static string MappedErrorHandlers = "MappedErrorHandlers";
            public static string ErrorHandlerMappingType = "ErrorHandlerMappingType";
        }

        public bool SelectedForConversion { get; set; }

        #region Activity-Error Handler Mapping
        [IsSerializedForLocalRepository]
        public ObservableList<Guid> MappedErrorHandlers = new ObservableList<Guid>();

        Activity.eHandlerMappingType mErrorHandlerMappingType;

        [IsSerializedForLocalRepository]
        public Activity.eHandlerMappingType  ErrorHandlerMappingType
        {
            get { return mErrorHandlerMappingType; }
            set
            {
                if (mErrorHandlerMappingType != value)
                {
                    mErrorHandlerMappingType = value;
                    OnPropertyChanged(Fields.ErrorHandlerMappingType);
                }
            }
        }
        #endregion

        public bool IsNotGherkinOptimizedActivity { get { return ActivitiesGroupID != "Optimized Activities" && ActivitiesGroupID != "Optimized Activities - Not in Use"; } }

        private bool mAGSelected;
        public bool AGSelected { get { return mAGSelected; } set { if (mAGSelected != value) { mAGSelected = value; OnPropertyChanged(Fields.AGSelected); } } }

        public List<string> VariablesBeforeExec { get; set; }
        
        public Activity()
        {
            //set fields default values
            mAutomationStatus = eActivityAutomationStatus.Development;
            mActionRunOption = eActionRunOption.StopActionsRunOnFailure;
        }

        public override string ToString()
        {
            return ActivityName;
        }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public Boolean Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(Fields.Active); } } }

        private string mActivityName;
        [IsSerializedForLocalRepository]
        public string ActivityName
        {
            get { return mActivityName; }
            set
            {
                if (mActivityName != value)
                {
                    mActivityName = value;
                    OnPropertyChanged(Fields.ActivityName);
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

        private string mRunDescription;
        [IsSerializedForLocalRepository]
        public string RunDescription
        {
            get { return mRunDescription; }
            set
            {
                if (mRunDescription != value)
                {
                    mRunDescription = value;
                    OnPropertyChanged(Fields.RunDescription);
                }
            }
        }

        private string mExpected { get; set; }
        [IsSerializedForLocalRepository]
        public string Expected
        {
            get { return mExpected; }
            set
            {
                if (mExpected != value)
                {
                    mExpected = value;
                    OnPropertyChanged(Fields.Expected);
                }
            }
        }

        /// <summary>
        /// Precentage of active Activity Actions
        /// </summary>
        [IsSerializedForLocalRepository]
        public string PercentAutomated
        {
            get
            {
                double iAuto = 0;
                double percent = 0;
                if (Acts.Count != 0)
                {
                    foreach (GingerCore.Actions.Act act in Acts)
                    {
                        if (act.Active)
                        {
                            iAuto++;
                        }
                    }
                    if (iAuto != 0)
                    {
                        percent = Math.Round((double)iAuto / Acts.Count, 2);
                    }
                }
                return percent.ToString("P").Split('.')[0] + "%";
            }
            //TODO: fixme UCGrid does have OneWay, no need for set, but check impact
            set //N.C. had to include Set bc UCGrid only has TwoWay Binding and not OneWay. TODO:
            {
                double iAuto = 0;
                double percent = 0;
                if (Acts.Count != 0)
                {
                    foreach (GingerCore.Actions.Act act in Acts)
                    {
                        if (act.Active)
                        {
                            iAuto++;
                        }
                    }
                }
                if (iAuto != 0)
                {
                    percent = Math.Round((double)iAuto / Acts.Count, 2);
                }
                value = percent.ToString("P").Split('.')[0] + "%";
                //RaisePropertyChanged("PercentAutomated");
                OnPropertyChanged(Fields.PercentAutomated);
            }
        }
        
        eActivityAutomationStatus? mAutomationStatus;
        /// <summary>
        /// Automation development status of the Activity
        /// </summary>
        [IsSerializedForLocalRepository]
        public eActivityAutomationStatus? AutomationStatus
        {
            get { return mAutomationStatus; }
            set
            {
                if (mAutomationStatus != value)
                {
                    mAutomationStatus = value;
                    OnPropertyChanged(Fields.AutomationStatus);
                }
            }
        }

        eActionRunOption? mActionRunOption;
        [IsSerializedForLocalRepository]
        public eActionRunOption? ActionRunOption
        {
            get { return mActionRunOption; }
            set
            {
                if (mActionRunOption != value)
                {
                    mActionRunOption = value;
                    OnPropertyChanged(Fields.ActionRunOption);
                }
            }
        }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus? mStatus;
        /// <summary>
        /// Run status of the activity
        /// </summary>
        //[IsSerializedForLocalRepository]    
        //TODO: check if status is different
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus? Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(Fields.Status); } }
        //TODO: add change history log in class and save it
        

        private String mActivitiesGroupID;
        /// <summary>
        /// Used to store the Activities Group ID/Name which this Activity is belong to in the Business Flow
        /// </summary>
        [IsSerializedForLocalRepository]
        public String ActivitiesGroupID 
        { 
            get { return mActivitiesGroupID; } 
            set { mActivitiesGroupID = value; OnPropertyChanged(Fields.ActivitiesGroupID); } 
        }


        private string mTargetApplication;
        [IsSerializedForLocalRepository]        
        public string TargetApplication          
        {
            get { return mTargetApplication; }
            set
            {
                if (mTargetApplication != value)
                {
                    mTargetApplication = value;
                    OnPropertyChanged(Fields.TargetApplication);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<Act> Acts = new ObservableList<Act>();


        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();

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


        [IsSerializedForLocalRepository]
        public bool EnableActionsVariablesDependenciesControl { get; set; }

        public string VariablesNames
        {
            get
            {
                string varsNames = string.Empty;
                foreach (VariableBase var in Variables)
                    varsNames += var.Name + ", ";
                return (varsNames.TrimEnd(new char[] {',', ' '}));                
            }
        }
        public void RefreshVariablesNames() { OnPropertyChanged(Fields.VariablesNames); }

        public VariableBase GetVariable(string name)
        {
            VariableBase v = (from v1 in Variables where v1.Name == name select v1).FirstOrDefault();
            return v;
        }

        public void ResetVaribles()
        {
            foreach (VariableBase va in Variables)
                va.ResetValue();
        }

        public void AddVariable(VariableBase v)
        {
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
        

        private long? mElapsed;
        //[IsSerializedForLocalRepository]
        public long? Elapsed
        {
            get { return mElapsed; }
            set
            {
                mElapsed = value;
                OnPropertyChanged(Fields.Elapsed);
                OnPropertyChanged(Fields.ElapsedSecs);
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
        }

        private string mScreen { get; set; }
        [IsSerializedForLocalRepository]
        public string Screen
        {
            get { return mScreen; }
            set
            {
                if (mScreen != value)
                {
                    mScreen = value;
                    OnPropertyChanged(Fields.Screen);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string Params { get; set; } //not used


        public override string GetNameForFileName() { return ActivityName; }
      
        [IsSerializedForLocalRepository]
        public ObservableList<VariableDependency> VariablesDependencies = new ObservableList<VariableDependency>();
        
        /// <summary>
        /// Check if the Activity supposed to be executed according to it variables dependencies configurations
        /// </summary>
        /// <param name="parentActivity">The Activity parent Business Flow</param>  
        /// <param name="setActivityStatus">Define of to set the Activity Status value in case the check fails</param>   
        /// <returns></returns>
        public bool? CheckIfVaribalesDependenciesAllowsToRun(BusinessFlow parentBusinessFlow, bool setActivityStatus = false)
        {
            bool? checkStatus = null;
            try
            {
                //check objects are valid
                if (parentBusinessFlow != null)
                {
                    //check if the Activities-variables dependencies mechanisem is enabled
                    if (parentBusinessFlow.EnableActivitiesVariablesDependenciesControl)
                    {
                        //check if the Activity configured to run with all BF selection list variables selected value
                        List<VariableBase> bfListVars = parentBusinessFlow.Variables.Where(v => v.GetType() == typeof(VariableSelectionList) && v.Value != null).ToList();
                        if (bfListVars != null && bfListVars.Count > 0)
                        {
                            foreach (VariableBase listVar in bfListVars)
                            {
                                VariableDependency varDep = null;
                                if (this.VariablesDependencies != null)
                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                if (varDep == null)
                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableGuid == listVar.Guid).FirstOrDefault();
                                if (varDep != null)
                                {
                                    if (!varDep.VariableValues.Contains(listVar.Value))
                                    {
                                        checkStatus = false;//the Selection List variable selected Value was not configured on the Activity
                                        break;
                                    }
                                }
                                else
                                {
                                    checkStatus = false;//the Selection List variable was not configured on the Activity
                                    break;
                                }
                            }
                            if (checkStatus == null)
                                checkStatus = true;//All Selection List variable selected values were configured on the Activity
                        }
                        else
                            checkStatus = true;//the BF dont has Selection List variables
                    }
                    else
                        checkStatus = true;//the mechanisem is disabled                    
                }
                else
                    checkStatus = false; //BF object is null

                //Check failed
                if (checkStatus == false && setActivityStatus == true)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }

                return checkStatus;
            }
            catch (Exception ex)
            {
                //Check failed
                if (setActivityStatus)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return false;
            }
        }

        public new void InvokPropertyChanngedForAllFields()
        {
            foreach (var field in typeof(Fields).GetFields())
                OnPropertyChanged(field.Name);
        }

        public void Reset()
        {
            Elapsed = null;
            Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            foreach (Act act in Acts)
            {
                act.Reset();
            }
        }

        public bool WarnFromMissingVariablesUse(ObservableList<VariableBase> solutionVars, ObservableList<VariableBase> bfVars, bool silentMode = true, bool autoAddMissingVars = true)
        {
            List<string> usedVariables = new List<string>();
            foreach (Act action in this.Acts)
                VariableBase.GetListOfUsedVariables(action, ref usedVariables);

            for (int indx = 0; indx < usedVariables.Count; indx++)
            {
                if (this.Variables.Where(x => x.Name == usedVariables[indx]).FirstOrDefault() != null)
                {
                    usedVariables.RemoveAt(indx);
                    indx--;
                }
            }

            if (usedVariables.Count > 0)
            {
                string missingVars = string.Empty;
                foreach (string var in usedVariables)
                    missingVars += "'" + var + "',";
                missingVars = missingVars.TrimEnd(new char[] { ',' });

                if (!silentMode)
                    if (Reporter.ToUser(eUserMsgKeys.WarnRegradingMissingVariablesUse, ActivityName, missingVars) == MessageBoxResult.Yes)
                        autoAddMissingVars = true;
                    else
                        autoAddMissingVars = false;

                if (autoAddMissingVars)
                {
                    //add missing vars from bf / global vars
                    for (int indx = 0; indx < usedVariables.Count; indx++)
                    {
                        VariableBase var = bfVars.Where(x => x.Name == usedVariables[indx]).FirstOrDefault();
                        if (var == null)
                            var = solutionVars.Where(x => x.Name == usedVariables[indx]).FirstOrDefault();
                        if (var != null)
                        {
                            this.Variables.Add(var);
                            usedVariables.RemoveAt(indx);
                            indx--;
                        }
                    }

                    if (usedVariables.Count > 0)
                    {
                        //not all vars were found and added automatically
                        if (!silentMode)
                            Reporter.ToUser(eUserMsgKeys.NotAllMissingVariablesWereAdded, missingVars);
                        return true;//not all missing vars were added automatically
                    }
                    else
                    {
                        return false;//missing vars were added automatically
                    }
                }
                else
                {
                    return true; //not auto adding the missing vars
                }
            }

            return false; //no missing vars
        }

        public Agent CurrentAgent { get; set; }

        public override string ItemName
        {
            get
            {
                return this.ActivityName;
            }
            set
            {
                this.ActivityName = value;
            }
        }

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null)
        {
            Activity activityInstance = (Activity)instance;
            //Create new instance of source
            Activity newInstance = (Activity)this.CreateInstance();
            

            newInstance.IsSharedRepositoryInstance = true;

            //update required part
            Activity.eItemParts ePartToUpdate = (Activity.eItemParts)Enum.Parse(typeof(Activity.eItemParts), partToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                case eItemParts.Details:
                    newInstance.Guid = activityInstance.Guid;
                    newInstance.ParentGuid = activityInstance.ParentGuid;
                    newInstance.ExternalID = activityInstance.ExternalID;
                    newInstance.ActivitiesGroupID = activityInstance.ActivitiesGroupID;
                    newInstance.TargetApplication = activityInstance.TargetApplication;
                    newInstance.Active = activityInstance.Active;
                    newInstance.VariablesDependencies = activityInstance.VariablesDependencies;                   
                    if (ePartToUpdate == eItemParts.Details)
                    {
                        //keep other parts
                        newInstance.Acts = activityInstance.Acts;
                        newInstance.Variables = activityInstance.Variables;
                    }
                    if (instance.ExternalID == this.ExternalID) 
                        AddExistingSelectionListVariabelesValues(newInstance, activityInstance);//increase selection list vars values- needed for GingerATS integration so based on External ID
                    if (hostItem != null)
                    {
                        //replace old instance object with new
                        int originalIndex = ((BusinessFlow)hostItem).Activities.IndexOf(activityInstance);
                        ((BusinessFlow)hostItem).Activities.Remove(activityInstance);
                        ((BusinessFlow)hostItem).Activities.Insert(originalIndex, newInstance);
                    }
                    break;
                case eItemParts.Actions:
                    activityInstance.Acts = newInstance.Acts;
                    break;
                case eItemParts.Variables:
                    activityInstance.Variables = newInstance.Variables;
                    break;
            }
        }

        public override RepositoryItemBase GetUpdatedRepoItem(RepositoryItemBase itemToUpload, RepositoryItemBase existingRepoItem, string itemPartToUpdate)
        {
            Activity updatedActivity = null;
            
            //update required part
            eItemParts ePartToUpdate = (eItemParts)Enum.Parse(typeof(Activity.eItemParts), itemPartToUpdate);

            switch (ePartToUpdate)
            {
                case eItemParts.All:                 

                case eItemParts.Details:
                    updatedActivity= (Activity)itemToUpload.CreateCopy(false);                 

                    if (ePartToUpdate == eItemParts.Details)
                    {
                        updatedActivity.Acts = ((Activity)existingRepoItem).Acts;
                        updatedActivity.Variables = ((Activity)existingRepoItem).Variables;
                    }
              
                    break;
                case eItemParts.Actions:
                    updatedActivity= (Activity)existingRepoItem.CreateCopy(false);
                    updatedActivity.Acts = ((Activity)itemToUpload).Acts;
                    break;
                case eItemParts.Variables:
                    updatedActivity = (Activity)existingRepoItem.CreateCopy(false);
                    updatedActivity.Variables = ((Activity)itemToUpload).Variables;                   
                    break;
            }

            return updatedActivity;
        }


        private void AddExistingSelectionListVariabelesValues(Activity repositoryItem, Activity usageItem)
        {
            try
            {
                List<VariableBase> usageVars =
                    usageItem.Variables.Where(a => a is VariableSelectionList).ToList<VariableBase>();
                if (usageVars != null && usageVars.Count > 0)
                {
                    foreach (VariableBase usageVar in usageVars)
                    {
                        VariableSelectionList usageVarList = (VariableSelectionList)usageVar;
                        //get the matching var in the repo item
                        VariableBase repoVar = repositoryItem.Variables.Where(x => x.Name.ToUpper() == usageVarList.Name.ToUpper()).FirstOrDefault();
                        if (repoVar != null)
                        {
                            VariableSelectionList repoVarList = (VariableSelectionList)repoVar;

                            //go over all optional values and add the missing ones
                            foreach (OptionalValue usageValue in usageVarList.OptionalValuesList)
                            {
                                OptionalValue val = repoVarList.OptionalValuesList.Where(x => x.Value == usageValue.Value).FirstOrDefault();
                                if (val == null)
                                {
                                    //add the val
                                    repoVarList.OptionalValuesList.Add(usageValue);
                                    repoVarList.SyncOptionalValuesListAndString();
                                    repositoryItem.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset the status because new variable optional value was added
                                }
                            }

                            //keep original variable value selection
                            if (repoVarList.OptionalValuesList.Where(pv => pv.Value == usageVar.Value).FirstOrDefault() != null)
                                repoVarList.Value = usageVar.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        public Act GetAct(Guid guidValue, string nameValue = null)
        {
            Act foundAct = null;
            if (guidValue != null && guidValue != Guid.Empty)
                foundAct = GetActFromPossibleList(guidValue.ToString());
            if (foundAct == null && guidValue == Guid.Empty && nameValue != null)//look by name only if do not have GUID so only old flows will still work with name mapping
                foundAct = GetActFromPossibleList(nameToLookBy: nameValue);
            return foundAct;
        }

        private Act GetActFromPossibleList(String guidToLookByString = null, string nameToLookBy = null)
        {
            Guid guidToLookBy = Guid.Empty;
            if (!string.IsNullOrEmpty(guidToLookByString))
            {
                guidToLookBy = Guid.Parse(guidToLookByString);
            }
           
            List<Act> lstActions = null;
            if (guidToLookBy != Guid.Empty)
                lstActions = Acts.Where(x => x.Guid == guidToLookBy).ToList();
            else
                lstActions = Acts.Where(x => x.Description == nameToLookBy).ToList();

            if (lstActions == null || lstActions.Count == 0)
                return null;
            else if (lstActions.Count == 1)
                return lstActions[0];
            else//we have more than 1
            {
                Act firstActive = lstActions.Where(x => x.Active == true).FirstOrDefault();
                if (firstActive != null)
                    return firstActive;
                else
                    return lstActions[0];//no one is Active so returning the first one
            }            
        }

        public bool AddDynamicly { get; set; }

        public string ExecutionLogFolder { get; set; }

        public int ExecutionLogActionCounter { get; set; }

        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        public override void UpdateItemFieldForReposiotryUse()
        {
            base.UpdateItemFieldForReposiotryUse();
            ActivitiesGroupID = null;            
        }

        public override bool IsTempItem
        {
            get
            {
                // no need to save activities which were added dynamically
                if (AddDynamicly)
                    return true;
                else
                    return false;
            }
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Activity;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.ActivityName);
            }
        }
    }
}
