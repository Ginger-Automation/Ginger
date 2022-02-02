#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Ginger.Reports;
using GingerCore;
using GingerCore.ALM;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;
using Amdocs.Ginger.Common.InterfacesLib;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using GingerCore.Activities;

namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionPublishToQC : RunSetActionBase
    {
        public new static class Fields
        {
            public static string VariableForTCRunName = "VariableForTCRunName";
            public static string isVariableInTCRunUsed = "isVariableInTCRunUsed";
            public static string toAttachActivitiesGroupReport = "toAttachActivitiesGroupReport";
        }
        PublishToALMConfig PublishToALMConfig = new PublishToALMConfig();
        readonly ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);                

        private string mVariableForTCRunName;
        [IsSerializedForLocalRepository]
        public string VariableForTCRunName { get { return mVariableForTCRunName; } set { if (mVariableForTCRunName != value) { mVariableForTCRunName = value; OnPropertyChanged(Fields.VariableForTCRunName); } } }

        private bool mIsVariableInTCRunUsed;
        [IsSerializedForLocalRepository]
        public bool isVariableInTCRunUsed { get { return mIsVariableInTCRunUsed; } set { if (mIsVariableInTCRunUsed != value) { mIsVariableInTCRunUsed = value; OnPropertyChanged(Fields.isVariableInTCRunUsed); } } }

        private bool mtoAttachActivitiesGroupReport;
        [IsSerializedForLocalRepository]
        public bool toAttachActivitiesGroupReport { get { return mtoAttachActivitiesGroupReport; } set { if (mtoAttachActivitiesGroupReport != value) { mtoAttachActivitiesGroupReport = value; OnPropertyChanged(Fields.toAttachActivitiesGroupReport); } } }        

        private FilterByStatus mFilterStatus;
        [IsSerializedForLocalRepository]
        public FilterByStatus FilterStatus
        {
            get { return mFilterStatus; }
            set { mFilterStatus = value; }
        }
        private eALMType mPublishALMType;
        [IsSerializedForLocalRepository]
        public eALMType PublishALMType 
        {
            get
            {
                return mPublishALMType;
            }
            set
            {
                if (mPublishALMType != value) 
                { 
                    mPublishALMType = value; 
                    OnPropertyChanged(nameof(PublishToALMConfig.PublishALMType)); 
                }
            }
        }
        private eALMTestSetLevel mALMTestSetLevel;
        [IsSerializedForLocalRepository]
        public eALMTestSetLevel ALMTestSetLevel
        {
            get
            {
                return mALMTestSetLevel;
            }
            set
            {
                if (mALMTestSetLevel != value)
                {
                    mALMTestSetLevel = value;
                    OnPropertyChanged(nameof(PublishToALMConfig.ALMTestSetLevel));
                }
            }
        }
        private eExportType mExportType;
        [IsSerializedForLocalRepository]
        public eExportType ExportType
        {
            get
            {
                return mExportType;
            }
            set
            {
                if (mExportType != value)
                {
                    mExportType = value;
                    OnPropertyChanged(nameof(PublishToALMConfig.ALMTestSetLevel));
                }
            }
        }
        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            list.Add(RunSetActionBase.eRunAt.DuringExecution);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return false; }
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            //Set flag for each BF to execute runset when BF execute finish
            SetExportToALMConfig();
            foreach (GingerRunner GR in WorkSpace.Instance.RunsetExecutor.Runners)
            {
                GR.PublishToALMConfig = PublishToALMConfig;
            }               
        }
        private void SetExportToALMConfig()
        {
            PublishToALMConfig.IsVariableInTCRunUsed = isVariableInTCRunUsed;
            PublishToALMConfig.ToAttachActivitiesGroupReport = toAttachActivitiesGroupReport;
            PublishToALMConfig.VariableForTCRunName = VariableForTCRunName;
            PublishToALMConfig.CalculateTCRunName(mVE);
            PublishToALMConfig.FilterStatus = FilterStatus;
            PublishToALMConfig.PublishALMType = PublishALMType; // bind?
            PublishToALMConfig.ALMTestSetLevel = ALMTestSetLevel; // bind?
            PublishToALMConfig.ExportType = ExportType; // bind?
        }
        public override void Execute(ReportInfo RI)
        {
            string result = string.Empty;
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            SetExportToALMConfig();
            // ALM Test Set Level: if "Run Set" convert Run Set to Business flow
            if(PublishToALMConfig.ALMTestSetLevel == eALMTestSetLevel.RunSet)
            {
                bfs.Add(ConvertRunSetToBF(RI));
                // Export Type: if eExportType.EntitiesAndResults then export Business Flow to ALM.
                if (PublishToALMConfig.ExportType == eExportType.EntitiesAndResults)
                {
                    if (bfs.Count > 0)
                    {
                        TargetFrameworkHelper.Helper.ExportVirtualBusinessFlowToALM(bfs[0], false, eALMConnectType.Silence);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Export Business Flow to ALM failed while publish results to ALM");
                    }
                }
            }
            else
            {
                foreach (BusinessFlowReport BFR in RI.BusinessFlows)
                {
                    bfs.Add((BusinessFlow)BFR.GetBusinessFlow());
                }
            }
            // TODO fix the report ??? check data in DB
            
            if (!TargetFrameworkHelper.Helper.ExportBusinessFlowsResultToALM(bfs,ref result, PublishToALMConfig))
            {
                Errors= result;
                Status = eRunSetActionStatus.Failed;
            }
            else
            {
                Status = eRunSetActionStatus.Completed;
            }
        }
        
        public override string GetEditPage()
        {
            //return new ExportResultsToALMConfigPage(this);
            return "ExportResultsToALMConfigPage";
        }

        

        public override string Type { get { return "Publish Execution Results to ALM"; } }
        public BusinessFlow ConvertRunSetToBF(ReportInfo reportInfo)
        {
            //GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            //GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            try
            {
                if (reportInfo == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow virtualBF = new BusinessFlow();
                virtualBF.Name = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;
                virtualBF.Description = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Description;
                //virtualBF.ExternalID = testSet.TestSetID;  ??? option of add external id.
                virtualBF.Status = BusinessFlow.eBusinessFlowStatus.Development;  // ??? 
                virtualBF.RunStatus = WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetExecutionStatus;
                virtualBF.Activities = new ObservableList<Activity>();
                //busFlow.Variables = new ObservableList<VariableBase>();  // ???
                //Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables
                foreach(GingerRunner runSetrunner in WorkSpace.Instance.RunsetExecutor.Runners)
                {
                    foreach(BusinessFlow runSetBF in runSetrunner.BusinessFlows)
                    {
                        ActivitiesGroup virtualAG = new ActivitiesGroup();
                        virtualAG.Name = runSetBF.Name;
                        virtualAG.Description = runSetBF.Description;
                        //virtualAG.RunStatus = runSetBF.RunStatus; // need to convert bf status to ag status.
                        virtualBF.AddActivitiesGroup(virtualAG);
                        // all the activities from share repository , none, some???
                        foreach(Activity runSetAct in runSetBF.Activities)
                        {
                            Activity act = new Activity();
                            Activity act2 = runSetAct.CreateCopy(true) as Activity;
                            act.ActivityName = runSetAct.ActivityName;
                            act.Description = runSetAct.Description;
                            act.Expected = runSetAct.Expected;
                            act2.ParentExecutionId = runSetAct.ParentExecutionId;
                            act2.Status = runSetAct.Status;
                            virtualBF.AddActivity(act2, virtualAG);
                            
                        }
                    }
                }
                ////Create Activities Group + Activities for each TC
                //foreach (QC.ALMTSTest tc in testSet.Tests)
                //{
                //    //check if the TC is already exist in repository
                //    ActivitiesGroup tcActivsGroup;
                //    ActivitiesGroup repoActivsGroup = null;
                //    if (repoActivsGroup == null)
                //    {
                //        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
                //    }
                //    if (repoActivsGroup != null)
                //    {
                //        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                //                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                //        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();

                //        var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                //        for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                //        {
                //            if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                //            {
                //                tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                //                indx--;
                //            }
                //        }

                //        tcActivsGroup.ExternalID2 = tc.TestID;
                //        busFlow.AddActivitiesGroup(tcActivsGroup);
                //        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                //        busFlow.AttachActivitiesGroupsAndActivities();
                //    }
                //    else //TC not exist in Ginger repository so create new one
                //    {
                //        tcActivsGroup = new ActivitiesGroup();
                //        tcActivsGroup.Name = tc.TestName;
                //        tcActivsGroup.ExternalID = tc.TestID;
                //        tcActivsGroup.ExternalID2 = tc.LinkedTestID;
                //        tcActivsGroup.Description = tc.Description;
                //        busFlow.AddActivitiesGroup(tcActivsGroup);
                //    }
///////////////////////////////////////steps////////////////////////////////////////////////////////////////
                //    //Add the TC steps as Activities if not already on the Activities group
                //    foreach (QC.ALMTSTestStep step in tc.Steps)
                //    {
                //        Activity stepActivity;
                //        bool toAddStepActivity = false;

                //        //check if mapped activity exist in repository
                //        Activity repoStepActivity = (Activity)GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                //        if (repoStepActivity != null)
                //        {
                //            //check if it is part of the Activities Group
                //            ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                //            if (groupStepActivityIdent != null)
                //            {
                //                //already in Activities Group so get link to it
                //                stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                //                // in any case update description/expected/name - even if "step" was taken from repository
                //                stepActivity.Description = StripHTML(step.Description);
                //                stepActivity.Expected = StripHTML(step.Expected);
                //                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                //            }
                //            else//not in ActivitiesGroup so get instance from repo
                //            {
                //                stepActivity = (Activity)repoStepActivity.CreateInstance();
                //                toAddStepActivity = true;
                //            }
                //        }
                //        else//Step not exist in Ginger repository so create new one
                //        {
                //            stepActivity = new Activity();
                //            stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                //            stepActivity.ExternalID = step.StepID;
                //            stepActivity.Description = StripHTML(step.Description);

                //            toAddStepActivity = true;
                //        }

                //        if (toAddStepActivity)
                //        {
                //            //not in group- need to add it
                //            busFlow.AddActivity(stepActivity, tcActivsGroup);
                //        }
////////////////////////////////steps parameters /////////////////////////////////////////////
                //        //pull TC-Step parameters and add them to the Activity level
                //        List<string> stepParamsList = new List<string>();
                //        foreach (string param in stepParamsList)
                //        {
                //            //get the param value
                //            string paramSelectedValue = string.Empty;
                //            bool? isflowControlParam = null;
                //            QC.ALMTSTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

                //            //get the param value
                //            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                //            {
                //                paramSelectedValue = tcParameter.Value;
                //            }
                //            else
                //            {
                //                isflowControlParam = null;//empty value
                //                paramSelectedValue = "<Empty>";
                //            }

                //            //check if parameter is part of a link
                //            string linkedVariable = null;
                //            if (paramSelectedValue.StartsWith("#$#"))
                //            {
                //                string[] valueParts = paramSelectedValue.Split(new[] { "#$#" }, StringSplitOptions.None);
                //                if (valueParts.Count() == 3)
                //                {
                //                    linkedVariable = valueParts[1];
                //                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                //                    if (!busVariables.Keys.Contains(linkedVariable))
                //                    {
                //                        busVariables.Add(linkedVariable, valueParts[2]);
                //                    }
                //                }
                //            }

                //            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                //            if (paramSelectedValue.StartsWith("$$_"))
                //            {
                //                isflowControlParam = false;
                //                if (paramSelectedValue.StartsWith("$$_"))
                //                {
                //                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                //                }
                //            }
                //            else if (paramSelectedValue != "<Empty>")
                //            {
                //                isflowControlParam = true;
                //            }
                //            //check if already exist param with that name
                //            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                //            if (stepActivityVar == null)
                //            {
                //                //#Param not exist so add it
                //                if (isflowControlParam == true)
                //                {
                //                    //add it as selection list param                               
                //                    stepActivityVar = new VariableSelectionList();
                //                    stepActivityVar.Name = param;
                //                    stepActivity.AddVariable(stepActivityVar);
                //                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                //                }
                //                else
                //                {
                //                    //add as String param
                //                    stepActivityVar = new VariableString();
                //                    stepActivityVar.Name = param;
                //                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                //                    stepActivity.AddVariable(stepActivityVar);
                //                }
                //            }
                //            else
                //            {
                //                //#param exist
                //                if (isflowControlParam == true)
                //                {
                //                    if (!(stepActivityVar is VariableSelectionList))
                //                    {
                //                        //flow control param must be Selection List so transform it
                //                        stepActivity.Variables.Remove(stepActivityVar);
                //                        stepActivityVar = new VariableSelectionList();
                //                        stepActivityVar.Name = param;
                //                        stepActivity.AddVariable(stepActivityVar);
                //                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                //                    }
                //                }
                //                else if (isflowControlParam == false)
                //                {
                //                    if (stepActivityVar is VariableSelectionList)
                //                    {
                //                        //change it to be string variable
                //                        stepActivity.Variables.Remove(stepActivityVar);
                //                        stepActivityVar = new VariableString();
                //                        stepActivityVar.Name = param;
                //                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                //                        stepActivity.AddVariable(stepActivityVar);
                //                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                //                    }
                //                }
                //            }

                //            //add the variable selected value                          
                //            if (stepActivityVar is VariableSelectionList)
                //            {
                //                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
                //                if (stepActivityVarOptionalVar == null)
                //                {
                //                    //no such variable value option so add it
                //                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                //                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                //                    if (isflowControlParam == true)
                //                    {
                //                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                //                    }
                //                }
                //                //set the selected value
                //                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                //            }
                //            else
                //            {
                //                //try just to set the value
                //                try
                //                {
                //                    stepActivityVar.Value = paramSelectedValue;
                //                    if (stepActivityVar is VariableString)
                //                    {
                //                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                //                    }
                //                }
                //                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                //            }

                //            //add linked variable if needed
                //            if (!string.IsNullOrEmpty(linkedVariable))
                //            {
                //                stepActivityVar.LinkedVariableName = linkedVariable;
                //            }
                //            else
                //            {
                //                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                //            }
                //        }
                //    }

                //    //order the Activities Group activities according to the order of the matching steps in the TC
                //    try
                //    {
                //        int startGroupActsIndxInBf = 0;
                //        if (busFlow.Activities.Count > 0)
                //        {
                //            startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                //        }
                //        foreach (QC.ALMTSTestStep step in tc.Steps)
                //        {
                //            int stepIndx = tc.Steps.IndexOf(step) + 1;
                //            ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                //            if (actIdent == null || actIdent.IdentifiedActivity == null)
                //            {
                //                break;//something wrong- shouldn't be null
                //            }
                //            Activity act = (Activity)actIdent.IdentifiedActivity;
                //            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                //            int bfActIndx = busFlow.Activities.IndexOf(act);

                //            //set it in the correct place in the group
                //            int numOfSeenSteps = 0;
                //            int groupIndx = -1;
                //            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                //            {
                //                groupIndx++;
                //                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                //                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                //                {
                //                    continue;//activity which not originally came from the TC
                //                }
                //                numOfSeenSteps++;

                //                if (numOfSeenSteps >= stepIndx)
                //                {
                //                    break;
                //                }
                //            }
                //            ActivityIdentifiers identOnPlace = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                //            if (identOnPlace.ActivityGuid != act.Guid)
                //            {
                //                //replace places in group
                //                tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                //                //replace places in business flow
                //                busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                //        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                //    }
                //}

                ////Add the BF variables (linked variables)
                //if (busVariables.Keys.Count > 0)
                //{
                //    foreach (KeyValuePair<string, string> var in busVariables)
                //    {
                //        //add as String param
                //        VariableString busVar = new VariableString();
                //        busVar.Name = var.Key;
                //        busVar.InitialStringValue = var.Value;
                //        busFlow.AddVariable(busVar);
                //    }
                //}

                return virtualBF;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }
    }
}
