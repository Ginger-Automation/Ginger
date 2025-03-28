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
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using QCRestClientStd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using TDAPIOLELib;

namespace GingerCore.ALM.QC
{
    public enum eQCItemType { TestCase, TestSet, Defect }

    public static class ImportFromQC
    {
        static TDConnection mTDConn = QCConnect.TDConn;

        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public static ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }
        public static ALMTestSet ImportTestSetData(ALMTestSet TS)
        {
            //fetch all TestStepTests in TS //(equivalent to Activities)
            List TSTests = GetListTSTest(TS);

            foreach (TSTest tsTest in TSTests)
            {
                TS.Tests.Add(ImportTSTest(tsTest)); //(equivalent to Activities)
            }

            return TS;
        }

        public static ALMTSTest ImportTSTest(TSTest tsTest)
        {
            ALMTSTest newTSTest = new ALMTSTest();
            string linkedTest = CheckLinkedTSTestName(tsTest);

            //Get the TC general details
            if (linkedTest != null)
            {
                //Linked TC
                string[] linkTest = linkedTest.Split(';');
                newTSTest.TestID = (string)tsTest.ID;
                newTSTest.TestName = linkTest[0];
                newTSTest.LinkedTestID = linkTest[1];
            }
            else
            {
                //Regular TC
                newTSTest.TestID = (string)tsTest.ID;
                newTSTest.TestName = tsTest.Name;
                newTSTest.LinkedTestID = (string)tsTest.TestId;
            }

            //Get the TC design steps
            List TSTestSteps = GetListTSTestSteps(tsTest);
            foreach (DesignStep tsStep in TSTestSteps)
            {
                ALMTSTestStep newtsStep = new ALMTSTestStep
                {
                    StepID = tsStep.ID.ToString(),
                    StepName = tsStep.StepName,
                    Description = tsStep.StepDescription,
                    Expected = tsStep.StepExpectedResult
                };
                newTSTest.Steps.Add(newtsStep);
            }

            //Get the TC parameters and their selected value
            if (linkedTest != null)
            {
                if ((linkedTest.Split(';')[0].ToString() != tsTest.Name.ToString()) || (linkedTest.Split(';')[0].ToString() != tsTest.TestName.ToString()))
                {
                    if (newTSTest.Description == null)
                    {
                        newTSTest.Description = string.Empty;
                    }
                    newTSTest.Description = tsTest.TestName.ToString() + System.Environment.NewLine + newTSTest.Description;
                }

                //Linked TC
                DesignStep TestParam = GetListTSTestVars(tsTest);
                //for (int i = 0; i <= ((List<object>)TestParam.LinkedParams).Count - 1; i++)
                //{
                //    ALMTSTestParameter newtsVar = new ALMTSTestParameter();
                //    if (((List<object>)TestParam.LinkedParams).ParamName[i] != null) { newtsVar.Name = TestParam.LinkedParams.ParamName(i); }
                //    if (TestParam.LinkedParams.ParamValue(i) != null) { newtsVar.Value = TestParam.LinkedParams.ParamValue(i).ToString(); }
                //    if (TestParam.LinkedParams.Type(i) != null) { newtsVar.Type = TestParam.LinkedParams.Type(i).ToString(); }
                //    newTSTest.Parameters.Add(newtsVar);
                //}
            }
            else
            {
                //Regular TC
                //for (int i = 0; i <= tsTest.Params.Count - 1; i++)
                //{
                //    ALMTSTestParameter newtsVar = new ALMTSTestParameter();
                //    if (tsTest.Params.ParamName(i) != null) { newtsVar.Name = tsTest.Params.ParamName(i); }
                //    if (tsTest.Params.ParamValue(i) != null) { newtsVar.Value = tsTest.Params.ParamValue(i).ToString(); }
                //    if (tsTest.Params.Type(i) != null) { newtsVar.Type = tsTest.Params.Type(i).ToString(); }
                //    newTSTest.Parameters.Add(newtsVar);
                //}
            }

            //Get the TC execution history
            try
            {
                List TSTestRuns = GetListTSTestRuns(tsTest);
                newTSTest.Runs = [];
                foreach (Run run in TSTestRuns)
                {
                    ALMTSTestRun newtsRun = new ALMTSTestRun
                    {
                        RunID = run.ID.ToString(),
                        RunName = run.Name,
                        Status = run.Status,
                        ExecutionDate = (run["RN_EXECUTION_DATE"]).ToString(),
                        ExecutionTime = (run["RN_EXECUTION_TIME"]).ToString(),
                        Tester = (run["RN_TESTER_NAME"]).ToString()
                    };
                    newTSTest.Runs.Add(newtsRun);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                newTSTest.Runs = [];
            }

            return newTSTest;
        }


        public static string GetTSTestLinkedID(TSTest tsTest)
        {
            string linkedTest = CheckLinkedTSTestName(tsTest);

            //Get the TC general details
            if (linkedTest != null)
            {
                //Linked TC
                string[] linkTest = linkedTest.Split(';');
                return linkTest[1];
            }
            else
            {
                return "0";
            }
        }

        private static DesignStep GetListTSTestVars(TSTest tsTest)
        {
            TestFactory TestF = (TestFactory)mTDConn.TestFactory;
            TDFilter filter = TestF.Filter as TDFilter;
            filter["TS_TEST_ID"] = "\"" + tsTest.TestId + "\"";
            List testsList = TestF.NewList(filter.Text);

            foreach (Test test in testsList)
            {
                //return test;
                DesignStepFactory DesStepF = (DesignStepFactory)test.DesignStepFactory;
                List DStepList = DesStepF.NewList("");
                foreach (DesignStep tsStep in DStepList)
                {
                    if (tsStep.LinkTestID != 0)
                    {
                        return tsStep;

                    }
                }
            }
            return null;
        }

        public static List GetListTSTest(ALMTestSet TS)
        {
            //TODO filter by name, path, id so only one will return
            TestSetFactory TSetFact = (TestSetFactory)mTDConn.TestSetFactory;
            TDFilter tsFilter = (TDFilter)TSetFact.Filter;
            tsFilter["CY_CYCLE_ID"] = "" + TS.TestSetID + "";
            List Testset = TSetFact.NewList(tsFilter.Text);

            foreach (TestSet testset in Testset)
            {
                if (testset.Name == TS.TestSetName)
                {
                    TSTestFactory TSTestFact = (TSTestFactory)testset.TSTestFactory;
                    TDFilter tsTestFilter = (TDFilter)TSetFact.Filter;
                    tsTestFilter["TC_CYCLE_ID"] = "" + TS.TestSetID + "";
                    List TSActivities = TSTestFact.NewList(tsTestFilter.Text);
                    return TSActivities;
                }
            }

            return new List();
        }

        private static List GetListTSTestSteps(TSTest tsTest)
        {
            TestFactory TestF = (TestFactory)mTDConn.TestFactory;
            TDFilter filter = TestF.Filter as TDFilter;
            filter["TS_TEST_ID"] = "\"" + tsTest.TestId + "\"";
            List testsList = TestF.NewList(filter.Text);

            foreach (Test test in testsList)
            {
                DesignStepFactory DesStepF = (DesignStepFactory)test.DesignStepFactory;
                List DStepList = DesStepF.NewList("");
                foreach (DesignStep tsStep in DStepList)
                {
                    if (tsStep.LinkTestID != 0)
                    {
                        filter["TS_TEST_ID"] = "\"" + tsStep.LinkTestID + "\"";
                        List LinkTestList = TestF.NewList(filter.Text);
                        foreach (Test LinkTestCase in LinkTestList)
                        {
                            DesignStepFactory LinkDesStepF = (DesignStepFactory)LinkTestCase.DesignStepFactory;
                            List LinkDStepList = LinkDesStepF.NewList("");
                            return LinkDStepList;
                        }
                    }
                }
                return DStepList;
            }
            return new List();
        }

        private static List GetListTSTestRuns(TSTest tsTest)
        {
            RunFactory RunF = (RunFactory)mTDConn.RunFactory;
            TDFilter filter = RunF.Filter as TDFilter;
            filter["TS_TEST_ID"] = "\"" + tsTest.TestId + "\"";
            List runssList = RunF.NewList(filter.Text);
            return runssList;
        }

        private static string CheckLinkedTSTestName(TSTest tsTest)
        {
            TestFactory TestF = (TestFactory)mTDConn.TestFactory;
            TDFilter filter = TestF.Filter as TDFilter;
            filter["TS_TEST_ID"] = "\"" + tsTest.TestId + "\"";
            List testsList = TestF.NewList(filter.Text);

            foreach (Test test in testsList)
            {
                DesignStepFactory DesStepF = (DesignStepFactory)test.DesignStepFactory;
                List DStepList = DesStepF.NewList("");
                foreach (DesignStep tsStep in DStepList)
                {
                    if (tsStep.LinkTestID != 0)
                    {

                        filter["TS_TEST_ID"] = "\"" + tsStep.LinkTestID + "\"";
                        List LinkTestList = TestF.NewList(filter.Text);
                        foreach (Test LinkTestCase in LinkTestList)
                        {
                            return LinkTestCase.Name + ";" + LinkTestCase.ID;

                        }
                    }
                }
            }
            return null;
        }

        public static BusinessFlow ConvertQCTestSetToBF(ALMTestSet testSet)
        {
            try
            {
                if (testSet == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = testSet.TestSetName,
                    ExternalID = testSet.TestSetID,
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = []
                };
                Dictionary<string, string> busVariables = [];//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (ALMTSTest tc in testSet.Tests)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID == tc.LinkedTestID);
                    }

                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID == tc.TestID);
                    }

                    if (repoActivsGroup != null)
                    {
                        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance(true);

                        var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                        for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                        {
                            if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                            {
                                tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                                indx--;
                            }
                        }

                        tcActivsGroup.ExternalID2 = tc.TestID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else //TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup
                        {
                            Name = tc.TestName
                        };
                        if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                        {
                            tcActivsGroup.ExternalID = tc.TestID;
                            tcActivsGroup.ExternalID2 = tc.TestID;
                        }
                        else
                        {
                            tcActivsGroup.ExternalID = tc.LinkedTestID;
                            tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                            tcActivsGroup.Description = tc.Description;
                        }
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (ALMTSTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID == step.StepID);
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
                                // in any case update description/expected/name - even if "step" was taken from repository
                                stepActivity.Description = StripHTML(step.Description);
                                stepActivity.Expected = StripHTML(step.Expected);
                                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            }
                            else//not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else//Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity
                            {
                                ActivityName = tc.TestName + ">" + step.StepName,
                                ExternalID = step.StepID,
                                Description = StripHTML(step.Description),
                                Expected = StripHTML(step.Expected)
                            };

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList = [];
                        GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                        GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                        foreach (string param in stepParamsList)
                        {
                            //get the param value
                            string paramSelectedValue = string.Empty;
                            bool? isflowControlParam = null;
                            ALMTSTestParameter tcParameter = tc.Parameters.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());

                            //get the param value
                            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                            {
                                paramSelectedValue = tcParameter.Value;
                            }
                            else
                            {
                                isflowControlParam = null;//empty value
                                paramSelectedValue = "<Empty>";
                            }

                            //check if parameter is part of a link
                            string linkedVariable = null;
                            if (paramSelectedValue.StartsWith("#$#"))
                            {
                                string[] valueParts = paramSelectedValue.Split(new string[] { "#$#" }, StringSplitOptions.None);
                                if (valueParts.Length == 3)
                                {
                                    linkedVariable = valueParts[1];
                                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                    if (busVariables.Keys.Contains(linkedVariable) == false)
                                    {
                                        busVariables.Add(linkedVariable, valueParts[2]);
                                    }
                                }
                            }

                            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (paramSelectedValue.StartsWith("$$_"))
                                {
                                    paramSelectedValue = paramSelectedValue[3..];//get value without "$$_"
                                }
                            }
                            else if (paramSelectedValue != "<Empty>")
                            {
                                isflowControlParam = true;
                            }

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param
                                    };
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                }
                            }
                            else
                            {
                                //#param exist
                                if (isflowControlParam == true)
                                {
                                    if (stepActivityVar is not VariableSelectionList)
                                    {
                                        //flow control param must be Selection List so transform it
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableSelectionList
                                        {
                                            Name = param
                                        };
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (isflowControlParam == false)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString
                                        {
                                            Name = param
                                        };
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == paramSelectedValue);
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    if (isflowControlParam == true)
                                    {
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                    }
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = paramSelectedValue;
                                    if (stepActivityVar is VariableString)
                                    {
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    }
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            }

                            //add linked variable if needed
                            if (string.IsNullOrEmpty(linkedVariable) == false)
                            {
                                stepActivityVar.LinkedVariableName = linkedVariable;
                            }
                            else
                            {
                                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                            }
                        }
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (ALMTSTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldn't be null
                            }

                            Activity act = actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                                {
                                    continue;//activity which not originally came from the TC
                                }

                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
                            }
                            ActivityIdentifiers identOnPlace = tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                            if (identOnPlace.ActivityGuid != act.Guid)
                            {
                                //replace places in group
                                tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                                //replace places in business flow
                                busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                    }
                }

                //Add the BF variables (linked variables)
                if (busVariables.Keys.Count > 0)
                {
                    foreach (KeyValuePair<string, string> var in busVariables)
                    {
                        //add as String param
                        VariableString busVar = new VariableString
                        {
                            Name = var.Key,
                            InitialStringValue = var.Value
                        };
                        busFlow.AddVariable(busVar);
                    }
                }

                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        public static void UpdatedQCTestInBF(ref BusinessFlow busFlow, List<ALMTSTest> tcsList)
        {
            if ((busFlow == null) || (tcsList == null) || (tcsList.Count < 1))
            {
                return;
            }

            Dictionary<string, string> busVariables = [];

            int startGroupActsIndxInBf = 0;
            Dictionary<string, int> activityGroupsToRemoveIndexes = [];
            foreach (ALMTSTest tc in tcsList)
            {
                var activitiesToRemove = busFlow.Activities.Where(x => x.ActivitiesGroupID == tc.TestName).ToList();
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                    {
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    }

                    busFlow.Activities.Remove(activityToRemove);
                }
                var activityGroupsToRemove = busFlow.ActivitiesGroups.Where(x => x.ExternalID2 == tc.TestID).ToList();
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    activityGroupsToRemoveIndexes.Add(activityGroupToRemove.ExternalID2, busFlow.ActivitiesGroups.IndexOf(activityGroupToRemove));
                }
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    busFlow.ActivitiesGroups.Remove(activityGroupToRemove);
                }
            }

            int activityGroupToRemoveIndex;
            foreach (ALMTSTest tc in tcsList)
            {
                activityGroupsToRemoveIndexes.TryGetValue(tc.TestID, out activityGroupToRemoveIndex);

                //check if the TC is already exist in repository
                ActivitiesGroup tcActivsGroup = new ActivitiesGroup();

                tcActivsGroup = new ActivitiesGroup
                {
                    Name = tc.TestName
                };
                if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                {
                    tcActivsGroup.ExternalID = tc.TestID;
                    tcActivsGroup.ExternalID2 = tc.TestID;
                }
                else
                {
                    tcActivsGroup.ExternalID = tc.LinkedTestID;
                    tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = tc.Description;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (ALMTSTestStep step in tc.Steps)
                {
                    Activity stepActivity;
                    bool toAddStepActivity = false;

                    //check if mapped activity exist in repository
                    Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID == step.StepID);
                    if (repoStepActivity != null)
                    {
                        //check if it is part of the Activities Group
                        ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (groupStepActivityIdent != null)
                        {
                            //already in Activities Group so get link to it
                            stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
                        }
                        else//not in ActivitiesGroup so get instance from repo
                        {
                            stepActivity = (Activity)repoStepActivity.CreateInstance();
                            toAddStepActivity = true;
                        }
                        stepActivity.IsSharedRepositoryInstance = true;
                    }
                    else//Step not exist in Ginger repository so create new one
                    {
                        stepActivity = new Activity
                        {
                            ActivityName = tc.TestName + ">" + step.StepName,
                            ExternalID = step.StepID,
                            Description = StripHTML(step.Description),
                            Expected = StripHTML(step.Expected)
                        };

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = [];
                    GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                    GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        ALMTSTestParameter tcParameter = tc.Parameters.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());

                        //get the param value
                        if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                        {
                            paramSelectedValue = tcParameter.Value;
                        }
                        else
                        {
                            isflowControlParam = null;//empty value
                            paramSelectedValue = "<Empty>";
                        }

                        //check if parameter is part of a link
                        string linkedVariable = null;
                        if (paramSelectedValue.StartsWith("#$#"))
                        {
                            string[] valueParts = paramSelectedValue.Split(new string[] { "#$#" }, StringSplitOptions.None);
                            if (valueParts.Length == 3)
                            {
                                linkedVariable = valueParts[1];
                                paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                if (busVariables.Keys.Contains(linkedVariable) == false)
                                {
                                    busVariables.Add(linkedVariable, valueParts[2]);
                                }
                            }
                        }

                        //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                        if (paramSelectedValue.StartsWith("$$_"))
                        {
                            isflowControlParam = false;
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                paramSelectedValue = paramSelectedValue[3..];//get value without "$$_"
                            }
                        }
                        else if (paramSelectedValue != "<Empty>")
                        {
                            isflowControlParam = true;
                        }

                        //check if already exist param with that name
                        VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());
                        if (stepActivityVar == null)
                        {
                            //#Param not exist so add it
                            if (isflowControlParam == true)
                            {
                                //add it as selection list param                               
                                stepActivityVar = new VariableSelectionList
                                {
                                    Name = param
                                };
                                stepActivity.AddVariable(stepActivityVar);
                                stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                            }
                            else
                            {
                                //add as String param
                                stepActivityVar = new VariableString
                                {
                                    Name = param
                                };
                                ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                stepActivity.AddVariable(stepActivityVar);
                            }
                        }
                        else
                        {
                            //#param exist
                            if (isflowControlParam == true)
                            {
                                if (stepActivityVar is not VariableSelectionList)
                                {
                                    //flow control param must be Selection List so transform it
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param
                                    };
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                }
                            }
                            else if (isflowControlParam == false)
                            {
                                if (stepActivityVar is VariableSelectionList)
                                {
                                    //change it to be string variable
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                }
                            }
                        }

                        //add the variable selected value                          
                        if (stepActivityVar is VariableSelectionList)
                        {
                            OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == paramSelectedValue);
                            if (stepActivityVarOptionalVar == null)
                            {
                                //no such variable value option so add it
                                stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                if (isflowControlParam == true)
                                {
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
                            }
                            //set the selected value
                            ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                        }
                        else
                        {
                            //try just to set the value
                            try
                            {
                                stepActivityVar.Value = paramSelectedValue;
                                if (stepActivityVar is VariableString)
                                {
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                }
                            }
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                        }

                        //add linked variable if needed
                        if (string.IsNullOrEmpty(linkedVariable) == false)
                        {
                            stepActivityVar.LinkedVariableName = linkedVariable;
                        }
                        else
                        {
                            stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                        }
                    }
                }

                //order the Activities Group activities according to the order of the matching steps in the TC
                try
                {
                    foreach (ALMTSTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (actIdent == null || actIdent.IdentifiedActivity == null)
                        {
                            break;//something wrong- shouldn't be null
                        }

                        Activity act = actIdent.IdentifiedActivity;
                        int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                        int bfActIndx = busFlow.Activities.IndexOf(act);

                        //set it in the correct place in the group
                        int numOfSeenSteps = 0;
                        int groupIndx = -1;
                        foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                        {
                            groupIndx++;
                            if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                    tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                            {
                                continue;//activity which not originally came from the TC
                            }

                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx)
                            {
                                break;
                            }
                        }
                        ActivityIdentifiers identOnPlace = tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                        if (identOnPlace.ActivityGuid != act.Guid)
                        {
                            //replace places in group
                            tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                            //replace places in business flow
                            busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                }
            }
            return;
        }

        public static void UpdateBusinessFlow(ref BusinessFlow busFlow, List<ALMTSTest> tcsList)
        {
            if ((busFlow == null) || (tcsList == null) || (tcsList.Count < 1))
            {
                return;
            }

            Dictionary<string, string> busVariables = [];
            int startGroupActsIndxInBf = 0;

            busFlow.Activities.Clear();
            busFlow.ActivitiesGroups.Clear();

            foreach (ALMTSTest tc in tcsList)
            {
                //check if the TC is already exist in repository
                ActivitiesGroup tcActivsGroup = new ActivitiesGroup();

                tcActivsGroup = new ActivitiesGroup
                {
                    Name = tc.TestName
                };
                if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                {
                    tcActivsGroup.ExternalID = tc.TestID;
                    tcActivsGroup.ExternalID2 = tc.TestID;
                }
                else
                {
                    tcActivsGroup.ExternalID = tc.LinkedTestID;
                    tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = tc.Description;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (ALMTSTestStep step in tc.Steps)
                {
                    Activity stepActivity;
                    bool toAddStepActivity = false;

                    //check if mapped activity exist in repository
                    Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID == step.StepID);
                    if (repoStepActivity != null)
                    {
                        //check if it is part of the Activities Group
                        ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (groupStepActivityIdent != null)
                        {
                            //already in Activities Group so get link to it
                            stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
                        }
                        else//not in ActivitiesGroup so get instance from repo
                        {
                            stepActivity = (Activity)repoStepActivity.CreateInstance();
                            toAddStepActivity = true;
                        }
                        stepActivity.IsSharedRepositoryInstance = true;
                    }
                    else//Step not exist in Ginger repository so create new one
                    {
                        stepActivity = new Activity
                        {
                            ActivityName = tc.TestName + ">" + step.StepName,
                            ExternalID = step.StepID,
                            Description = StripHTML(step.Description),
                            Expected = StripHTML(step.Expected)
                        };

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = [];
                    GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                    GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        ALMTSTestParameter tcParameter = tc.Parameters.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());

                        //get the param value
                        if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                        {
                            paramSelectedValue = tcParameter.Value;
                        }
                        else
                        {
                            isflowControlParam = null;//empty value
                            paramSelectedValue = "<Empty>";
                        }

                        //check if parameter is part of a link
                        string linkedVariable = null;
                        if (paramSelectedValue.StartsWith("#$#"))
                        {
                            string[] valueParts = paramSelectedValue.Split(new string[] { "#$#" }, StringSplitOptions.None);
                            if (valueParts.Length == 3)
                            {
                                linkedVariable = valueParts[1];
                                paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                if (busVariables.Keys.Contains(linkedVariable) == false)
                                {
                                    busVariables.Add(linkedVariable, valueParts[2]);
                                }
                            }
                        }

                        //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                        if (paramSelectedValue.StartsWith("$$_"))
                        {
                            isflowControlParam = false;
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                paramSelectedValue = paramSelectedValue[3..];//get value without "$$_"
                            }
                        }
                        else if (paramSelectedValue != "<Empty>")
                        {
                            isflowControlParam = true;
                        }

                        //check if already exist param with that name
                        VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());
                        if (stepActivityVar == null)
                        {
                            //#Param not exist so add it
                            if (isflowControlParam == true)
                            {
                                //add it as selection list param                               
                                stepActivityVar = new VariableSelectionList
                                {
                                    Name = param
                                };
                                stepActivity.AddVariable(stepActivityVar);
                                stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                            }
                            else
                            {
                                //add as String param
                                stepActivityVar = new VariableString
                                {
                                    Name = param
                                };
                                ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                stepActivity.AddVariable(stepActivityVar);
                            }
                        }
                        else
                        {
                            //#param exist
                            if (isflowControlParam == true)
                            {
                                if (stepActivityVar is not VariableSelectionList)
                                {
                                    //flow control param must be Selection List so transform it
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param
                                    };
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                }
                            }
                            else if (isflowControlParam == false)
                            {
                                if (stepActivityVar is VariableSelectionList)
                                {
                                    //change it to be string variable
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                }
                            }
                        }

                        //add the variable selected value                          
                        if (stepActivityVar is VariableSelectionList)
                        {
                            OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == paramSelectedValue);
                            if (stepActivityVarOptionalVar == null)
                            {
                                //no such variable value option so add it
                                stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                if (isflowControlParam == true)
                                {
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
                            }
                            //set the selected value
                            ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                        }
                        else
                        {
                            //try just to set the value
                            try
                            {
                                stepActivityVar.Value = paramSelectedValue;
                                if (stepActivityVar is VariableString)
                                {
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                }
                            }
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                        }

                        //add linked variable if needed
                        if (string.IsNullOrEmpty(linkedVariable) == false)
                        {
                            stepActivityVar.LinkedVariableName = linkedVariable;
                        }
                        else
                        {
                            stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                        }
                    }
                }

                //order the Activities Group activities according to the order of the matching steps in the TC
                try
                {
                    foreach (ALMTSTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (actIdent == null || actIdent.IdentifiedActivity == null)
                        {
                            break;//something wrong- shouldn't be null
                        }

                        Activity act = actIdent.IdentifiedActivity;
                        int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                        int bfActIndx = busFlow.Activities.IndexOf(act);

                        //set it in the correct place in the group
                        int numOfSeenSteps = 0;
                        int groupIndx = -1;
                        foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                        {
                            groupIndx++;
                            if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                    tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                            {
                                continue;//activity which not originally came from the TC
                            }

                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx)
                            {
                                break;
                            }
                        }
                        ActivityIdentifiers identOnPlace = tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                        if (identOnPlace.ActivityGuid != act.Guid)
                        {
                            //replace places in group
                            tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                            //replace places in business flow
                            busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                }
            }
            return;
        }

        private static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                {
                    stripped = HttpUtility.HtmlDecode(stripped);
                }

                stripped = stripped.TrimStart(new char[] { '\r', '\n' });
                stripped = stripped.TrimEnd(new char[] { '\r', '\n' });

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }

        private static void GetStepParameters(string stepText, ref List<string> stepParamsList)
        {
            try
            {
                MatchCollection stepParams = Regex.Matches(stepText, @"\<<<([^>]*)\>>>");

                foreach (var param in stepParams)
                {
                    string strParam = param.ToString().TrimStart(new char[] { '<' });
                    strParam = strParam.TrimEnd(new char[] { '>' });
                    stepParamsList.Add(strParam);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while pulling the parameters names from QC TC Step Description/Expected", ex);
            }
        }

        public static Test GetQCTest(string testID)
        {
            int intTestSetID = -1;
            if (int.TryParse(testID, out intTestSetID) == false)
            {
                return null;
            }

            TestFactory testFact = (TestFactory)mTDConn.TestFactory;
            TDFilter filter = testFact.Filter as TDFilter;
            filter["TS_TEST_ID"] = "" + testID + "";
            List testsList = testFact.NewList(filter.Text);
            if (testsList != null && testsList.Count == 1)
            {
                foreach (Test test in testsList)
                {
                    return test;
                }
            }

            return null;
        }

        public static List<ALMTSTest> GetTSQCTestsList(string testSetID, List<string> TCsIDs = null)
        {
            List<ALMTSTest> TSQCTestsList = [];

            TestSet testSet = ImportFromQC.GetQCTestSet(testSetID);
            if (testSet != null)
            {
                List<TSTest> qcTSTests = ImportFromQC.GetTSTestsList(testSet);
                if ((TCsIDs != null) && (TCsIDs.Count > 0))
                {
                    foreach (string TCID in TCsIDs)
                    {
                        TSTest tsTest = qcTSTests.FirstOrDefault(x => x.ID == TCID.ToString());
                        if (tsTest != null)
                        {
                            TSQCTestsList.Add(ImportTSTest(tsTest));
                        }
                    }
                }
                else
                {
                    foreach (TSTest tc in qcTSTests)
                    {
                        if (tc != null)
                        {
                            TSQCTestsList.Add(ImportTSTest(tc));
                        }
                    }
                }
            }

            return TSQCTestsList;
        }

        public static TestSet GetQCTestSet(string testSetID)
        {
            int intTestSetID = -1;
            if (int.TryParse(testSetID, out intTestSetID) == false)
            {
                return null;
            }

            TestSetFactory TSetFact = (TestSetFactory)mTDConn.TestSetFactory;
            TDFilter tsFilter = (TDFilter)TSetFact.Filter;
            tsFilter["CY_CYCLE_ID"] = "" + testSetID + "";
            List Testset = TSetFact.NewList(tsFilter.Text);
            if (Testset != null && Testset.Count == 1)
            {
                foreach (TestSet testset in Testset)
                {
                    return testset;
                }
            }

            return null;
        }

        public static string GetQCTestSetPath(TestSet testSet)
        {
            dynamic tsFolderNode = testSet.TestSetFolder;
            return (tsFolderNode.Path.ToString());
        }


        public static List<TSTest> GetTSTestsList(TestSet testset)
        {
            List<TSTest> tSTests = [];
            TSTestFactory TSTestFact = (TSTestFactory)testset.TSTestFactory;
            TestSetFactory TSetFact = (TestSetFactory)mTDConn.TestSetFactory;
            TDFilter tsFilter = (TDFilter)TSetFact.Filter;
            TDFilter tsTestFilter = (TDFilter)TSetFact.Filter;
            tsFilter["CY_CYCLE_ID"] = "" + testset.ID + "";
            tsTestFilter["TC_CYCLE_ID"] = "" + testset.ID + "";
            List TSActivities = TSTestFact.NewList(tsTestFilter.Text);
            foreach (TSTest tsTest in TSActivities)
            {
                tSTests.Add(tsTest);
            }
            return tSTests;
        }

        public static void GetItemCustomizedFields(string itemType, ref Dictionary<string, string> fields, ref Dictionary<string, List<string>> fieldsListSelections, bool onlyMandatoryfields = false)
        {
            if (fields == null)
            {
                fields = [];
            }

            if (fieldsListSelections == null)
            {
                fieldsListSelections = [];
            }

            Customization customiz = (Customization)mTDConn.Customization;
            CustomizationFields customizFields = (CustomizationFields)customiz.Fields;
            //List testFieldsList = customizFields.get_Fields("TEST");
            List testFieldsList = customizFields.get_Fields(itemType);
            foreach (CustomizationField testField in testFieldsList)
            {
                if (onlyMandatoryfields == true && testField.IsRequired != true)
                {
                    continue;
                }

                List<string> fieldList = [];
                //if (testField.List != null && testField.List.RootNode.Children.Count > 0)
                //{
                //    CustomizationListNode lnode = testField.List.RootNode;
                //    List cNodes = lnode.Children;
                //    foreach (CustomizationListNode ccNode in cNodes)
                //    {
                //        //adds list of valid selections of Field
                //        fieldList.Add(ccNode.Name);
                //    }
                //}
                //field name to set = TestField.ColumnName
                //user label (what is shown in QC webpage) = TestField.UserLabel
                if (!fields.ContainsKey(testField.UserLabel))
                {
                    fields.Add(testField.UserLabel, testField.ColumnName);
                    if (fieldList.Count > 0)
                    {
                        fieldsListSelections.Add(testField.UserLabel, fieldList);
                    }
                    else
                    {
                        fieldsListSelections.Add(testField.UserLabel, null);
                    }
                }
            }
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = [];

            Customization customiz = (Customization)mTDConn.Customization;
            CustomizationFields customizFields = (CustomizationFields)customiz.Fields;

            List testSetFieldsList;
            List testCaseFieldsList;

            testSetFieldsList = customizFields.get_Fields("CYCLE");

            testCaseFieldsList = customizFields.get_Fields("TEST");


            //Populate Test Set fields
            foreach (CustomizationField field in testSetFieldsList)
            {
                if (string.IsNullOrEmpty(field.UserLabel))
                {
                    continue;
                }

                ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                {
                    ID = field.ColumnName,
                    Name = field.UserLabel,
                    Mandatory = field.IsRequired
                };
                if (itemfield.Mandatory)
                {
                    itemfield.ToUpdate = true;
                }

                itemfield.ItemType = eQCItemType.TestSet.ToString();

                //if (field.List != null) // field.List.RootNode.Children.Count > 0
                //{
                //    CustomizationListNode lnode = field.List.RootNode;
                //    List cNodes = lnode.Children;
                //    foreach (CustomizationListNode ccNode in cNodes)
                //    {
                //        //adds list of valid selections of Field
                //        itemfield.PossibleValues.Add(ccNode.Name);
                //    }
                //}

                if (itemfield.PossibleValues.Count > 0)
                {
                    itemfield.SelectedValue = itemfield.PossibleValues[0];
                }
                else
                {
                    itemfield.SelectedValue = "NA";
                }

                fields.Add(itemfield);
            }

            //Get Test Case fields
            foreach (CustomizationField field in testCaseFieldsList)
            {
                if (string.IsNullOrEmpty(field.UserLabel))
                {
                    continue;
                }

                ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                {
                    ID = field.ColumnName,
                    Name = field.UserLabel,
                    Mandatory = field.IsRequired
                };
                if (itemfield.Mandatory)
                {
                    itemfield.ToUpdate = true;
                }

                itemfield.ItemType = eQCItemType.TestCase.ToString();

                //if (field.List != null) // field.List.RootNode.Children.Count > 0
                //{
                //    CustomizationListNode lnode = field.List.RootNode;
                //    List cNodes = lnode.Children;
                //    foreach (CustomizationListNode ccNode in cNodes)
                //    {
                //        //adds list of valid selections of Field
                //        itemfield.PossibleValues.Add(ccNode.Name);
                //    }
                //}

                if (itemfield.PossibleValues.Count > 0)
                {
                    itemfield.SelectedValue = itemfield.PossibleValues[0];
                }
                else
                {
                    itemfield.SelectedValue = "NA";
                }

                fields.Add(itemfield);
            }

            return fields;
        }

        public static ObservableList<ExternalItemFieldBase> GetQCEntityFieldsREST(AlmDataContractsStd.Enums.ResourceType fieldType)
        {
            ObservableList<ExternalItemFieldBase> fields = [];

            string qcbin = "qcbin";
            QCRestClientStd.QCClient qcClientREST = new QCClient(ALMCore.DefaultAlmConfig.ALMServerURL.TrimEnd(qcbin.ToCharArray()), ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMDomain, ALMCore.DefaultAlmConfig.ALMProjectName, 11);

            if (qcClientREST.Login())
            {
                // List<QCField> fieldsCollection = qcClientREST.GetFields(fieldType.ToString()).OrderBy(x => x.IsRequired).OrderBy(x => x.Name).ToList();
                List<QCField> fieldsCollection = qcClientREST.GetFields(fieldType.ToString()).OrderByDescending(x => x.IsRequired).ToList();

                if ((fieldsCollection != null) && (fieldsCollection.Count > 0))
                {
                    foreach (QCField field in fieldsCollection)
                    {
                        if (string.IsNullOrEmpty(field.Label))
                        {
                            continue;
                        }

                        ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                        {
                            ID = field.PhysicalName,
                            ExternalID = field.Name,  // Temp ??? Check if ExternalID has other use in this case
                            Name = field.Label,
                            Mandatory = field.IsRequired,
                            SystemFieled = field.IsSystem
                        };
                        if (itemfield.Mandatory)
                        {
                            itemfield.ToUpdate = true;
                        }

                        itemfield.ItemType = eQCItemType.Defect.ToString();
                        itemfield.Type = field.Type;

                        if ((field.ListId != null) && (field.ListId != string.Empty) && (field.FieldValues != null) && (field.FieldValues.Count > 0))
                        {
                            foreach (string value in field.FieldValues)
                            {
                                itemfield.PossibleValues.Add(value);
                            }
                        }

                        if (itemfield.PossibleValues.Count > 0)
                        {
                            itemfield.SelectedValue = itemfield.PossibleValues[0];
                        }
                        else
                        {
                            // itemfield.SelectedValue = "NA";
                        }

                        fields.Add(itemfield);
                    }
                }
            }

            return fields;
        }


    }
}
