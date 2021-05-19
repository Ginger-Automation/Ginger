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

using ALM_Common.DataContracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QCRestAPI;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json.Linq;
using QCRestClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using ZephyrEntSDK.Models;
using ZephyrEntSDK.Models.Base;
using Zepyhr_Ent_Repository;

namespace GingerCore.ALM.ZephyrEnt.Bll
{
    public class ZephyrEntImportManager
    {
        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public static ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }
        enum StatuesName { NoRun = 0, PASS = 1, FAIL = 2, WIP = 3, Blocked = 4}
        Dictionary<int, int> statusesDic = new Dictionary<int, int>() 
        { {(int)StatuesName.NoRun, 0 }, {(int)StatuesName.PASS, 0 }, 
            {(int)StatuesName.FAIL, 0 }, {(int)StatuesName.WIP, 0 }, {(int)StatuesName.Blocked, 0 }};
        private ZephyrEntRepository zephyrEntRepository;

        public ZephyrEntImportManager(ZephyrEntRepository zephyrEntRepository)
        {
            this.zephyrEntRepository = zephyrEntRepository;
        }

        #region Public Functions
      
        public QC.QCTSTest ImportTSTest(QCTestInstance testInstance)
        {
            QC.QCTSTest newTSTest = new QC.QCTSTest();
            QCTestCase testCase = QCRestAPIConnect.GetTestCases(new List<string>() { testInstance.TestId })[0];
            string linkedTest = CheckLinkedTSTestName(testCase);

            if (testInstance != null)
            {
                //Get the TC general details
                if (linkedTest != null)
                {
                    //Linked TC
                    string[] linkTest = linkedTest.Split(';');
                    newTSTest.TestID = testInstance.Id;
                    newTSTest.TestName = linkTest[0];
                    newTSTest.LinkedTestID = linkTest[1];
                }
                else
                {
                    //Regular TC
                    newTSTest.TestID = testInstance.Id;
                    newTSTest.TestName = testInstance.Name ?? testCase.Name;
                    newTSTest.LinkedTestID = testInstance.TestId;
                }
            }

            //Get the TC design steps
            QCTestCaseStepsColl TSTestSteps = GetListTSTestSteps(testCase);
            foreach (QCTestCaseStep testcaseStep in TSTestSteps)
            {
                QC.QCTSTestStep newtsStep = new QC.QCTSTestStep();
                newtsStep.StepID = testcaseStep.Id.ToString();
                newtsStep.StepName = testcaseStep.Name;
                newtsStep.Description = testcaseStep.Description;
                newtsStep.Expected = testcaseStep.ElementsField["expected"].ToString();
                newTSTest.Steps.Add(newtsStep);
            }

            //Get the TC parameters and their selected value
            if (linkedTest != null)
            {
                if (linkedTest.Split(';')[0] != testCase.Name)
                {
                    if (newTSTest.Description == null)
                    {
                        newTSTest.Description = string.Empty;
                    }
                    newTSTest.Description = testCase.Name.ToString() + System.Environment.NewLine + newTSTest.Description;
                }

                //Linked TC
                QCTestCaseStep TSLinkedTestCaseStep = GetListTSTestVars(testCase);
                if (TSLinkedTestCaseStep != null)
                {
                    FillRelevantDataForStepParams(newTSTest, TSLinkedTestCaseStep);
                }
            }
            else
            {
                ////Regular TC
                QCTestCaseStepsColl TSLinkedTestCaseSteps = QCRestAPIConnect.GetTestCaseSteps(testCase.Id);
                foreach (QCTestCaseStep step in TSLinkedTestCaseSteps)
                {
                    FillRelevantDataForStepParams(newTSTest, step);
                }
            }

            //Get the TC execution history
            try
            {
                QCRunColl TSTestRuns = GetListTSTestRuns(testCase);

                foreach (QCRun run in TSTestRuns)
                {
                    QC.QCTSTestRun newtsRun = new QC.QCTSTestRun();
                    newtsRun.RunID = run.Id;
                    newtsRun.RunName = run.Name;
                    newtsRun.Status = run.Status;
                    newtsRun.ExecutionDate = (run.ElementsField["execution-date"]).ToString();
                    newtsRun.ExecutionTime = (run.ElementsField["execution-time"]).ToString();
                    newtsRun.Tester = (run.Owner).ToString();
                    newTSTest.Runs.Add(newtsRun);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                newTSTest.Runs = new List<QC.QCTSTestRun>();
            }

            return newTSTest;
        }

        public List<string[]> GetTCsDataSummary(int tsId)
        {
            try
            {
                List<string[]> tcsSummary = new List<string[]>();
                statusesDic.Keys.ToList().ForEach(k => statusesDic[k] = 0);

                List<BaseResponseItem> tcsData = zephyrEntRepository.GetTCsByTreeId(tsId);
                var token = (JToken)tcsData[0].TryGetItem("results");
                foreach (var test in token)
                {
                    if (((JObject)test).ContainsKey("rts"))
                    {
                        statusesDic[Convert.ToInt32(test["rts"]["status"])] += 1;
                    }
                    else
                    {
                        statusesDic[0] += 1;
                    }
                }
                foreach (var item in statusesDic)
                {
                    if (item.Value > 0)
                    {
                        string statusName = ((StatuesName)item.Key).ToString();
                        tcsSummary.Add(new [] { statusName, item.Value.ToString() });
                    }
                }
                return tcsSummary;
            }

            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test case summary " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), ex);
                return null;
            }
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet testSet)
        {
            GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            try
            {
                if (testSet == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testSet.TestSetName;
                busFlow.ExternalID = testSet.TestSetID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
                Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (QC.QCTSTest tc in testSet.Tests)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
                    }
                    if (repoActivsGroup != null)
                    {
                        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();

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
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.TestName;
                        tcActivsGroup.ExternalID = tc.TestID;
                        tcActivsGroup.ExternalID2 = tc.LinkedTestID;
                        tcActivsGroup.Description = tc.Description;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (QC.QCTSTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = (Activity)GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
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
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            stepActivity.ExternalID = step.StepID;
                            stepActivity.Description = StripHTML(step.Description);

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList = new List<string>();
                        foreach (string param in stepParamsList)
                        {
                            //get the param value
                            string paramSelectedValue = string.Empty;
                            bool? isflowControlParam = null;
                            QC.QCTSTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

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
                                string[] valueParts = paramSelectedValue.Split(new [] { "#$#" }, StringSplitOptions.None);
                                if (valueParts.Count() == 3)
                                {
                                    linkedVariable = valueParts[1];
                                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                    if (!busVariables.Keys.Contains(linkedVariable))
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
                                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                                }
                            }
                            else if (paramSelectedValue != "<Empty>")
                            {
                                isflowControlParam = true;
                            }
                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param;
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                }
                            }
                            else
                            {
                                //#param exist
                                if (isflowControlParam == true)
                                {
                                    if (!(stepActivityVar is VariableSelectionList))
                                    {
                                        //flow control param must be Selection List so transform it
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableSelectionList();
                                        stepActivityVar.Name = param;
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
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param;
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
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
                            if (!string.IsNullOrEmpty(linkedVariable))
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
                        int startGroupActsIndxInBf = 0;
                        if (busFlow.Activities.Count > 0)
                        {
                            startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        }
                        foreach (QC.QCTSTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldn't be null
                            }
                            Activity act = (Activity)actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                {
                                    continue;//activity which not originally came from the TC
                                }
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
                            }
                            ActivityIdentifiers identOnPlace = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers[groupIndx];
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
                        VariableString busVar = new VariableString();
                        busVar.Name = var.Key;
                        busVar.InitialStringValue = var.Value;
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

        #endregion Public Functions

        #region private functions

        private static QCTestCaseStep GetListTSTestVars(QCTestCase testCase)
        {
            QCTestCaseStepsColl steps = QCRestAPIConnect.GetTestCaseSteps(testCase.Id);

            foreach (QCTestCaseStep step in steps)
            {
                if (step.ElementsField.ContainsKey("link-test"))
                {
                    return step;
                }
            }
            return null;
        }

        private static BusinessFlow CreateBusinessFlow(QCTestSet tS)
        {
            BusinessFlow busFlow = new BusinessFlow();
            busFlow.Name = tS.Name;
            busFlow.ExternalID = tS.Id;
            busFlow.Description = tS.ElementsField["description"].ToString();
            busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
            busFlow.Activities = new ObservableList<Activity>();
            busFlow.Variables = new ObservableList<VariableBase>();

            return busFlow;
        }

        private static ActivitiesGroup CheckIfTCAlreadyExistInRepo(BusinessFlow busFlow, QCTestInstance testInstance, QCTestCaseStepsColl tSTestCaseSteps)
        {
            ActivitiesGroup tcActivsGroup;
            ActivitiesGroup repoActivsGroup = null;
            QCTestCaseStepsColl relevantTestCaseSteps = QCRestAPIConnect.GetTestCaseSteps(testInstance.TestId);
            QCTestCaseStep relevantStep = null;
            foreach (QCTestCaseStep testcaseStep in relevantTestCaseSteps)
            {
                if (testcaseStep.ElementsField.ContainsKey("link-test"))
                {
                    relevantStep = testcaseStep;
                }
            }
            if (relevantStep != null)
            {
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == relevantStep.ElementsField["link-test"].ToString()).FirstOrDefault();
            }
            if (repoActivsGroup == null)
            {
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == testInstance.Id).FirstOrDefault();
            }
            if (repoActivsGroup != null)
            {
                List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                               .Where(x => !tSTestCaseSteps.Where(item => item.TestId == testInstance.TestId).Select(y => y.Id).ToList().Contains(x.ExternalID)).ToList();

                tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();

                var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                {
                    if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                    {
                        tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                        indx--;
                    }
                }

                tcActivsGroup.ExternalID2 = testInstance.Id;
                busFlow.AddActivitiesGroup(tcActivsGroup);
                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                busFlow.AttachActivitiesGroupsAndActivities();
            }
            else //TC not exist in Ginger repository so create new one
            {
                tcActivsGroup = new ActivitiesGroup();
                tcActivsGroup.Name = testInstance.Name;
                if (relevantStep == null)
                {
                    tcActivsGroup.ExternalID = testInstance.Id;
                    tcActivsGroup.ExternalID2 = testInstance.Id;
                }
                else
                {
                    tcActivsGroup.ExternalID = relevantStep.ElementsField["link-test"].ToString();
                    tcActivsGroup.ExternalID2 = testInstance.Id; 
                }
                busFlow.AddActivitiesGroup(tcActivsGroup);
            }

            return tcActivsGroup;
        }

        private static Activity LinkStepAndUpdate(BusinessFlow busFlow, ActivityIdentifiers groupStepActivityIdent, QCTestCaseStep step, QCTestInstance testInstance)
        {
            Activity stepActivity;

            //already in Activities Group so get link to it
            stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
            // in any case update description/expected/name - even if "step" was taken from repository
            stepActivity.Description = StripHTML(step.Description);
            stepActivity.Expected = StripHTML(step.ElementsField["expected"].ToString());
            stepActivity.ActivityName = testInstance.Name + ">" + step.Name;

            return stepActivity;
        }

        private static Activity CreateNewStep(QCTestInstance testInstance, QCTestCaseStep step)
        {
            Activity stepActivity = new Activity();
            stepActivity.ActivityName = testInstance.Name + ">" + step.Name;
            stepActivity.ExternalID = step.Id;
            stepActivity.Description = StripHTML(step.Description);
            stepActivity.Expected = StripHTML(step.ElementsField["expected"].ToString());

            return stepActivity;
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

                stripped = stripped.Trim();
                stripped = stripped.TrimStart('\n', '\r');
                stripped = stripped.TrimEnd('\n', '\r');

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }


        private static void FillRelevantDataForStepParams(QC.QCTSTest newTSTest, QCTestCaseStep tSLinkedTestCaseStep)
        {
            string description = StripHTML(tSLinkedTestCaseStep.Description).Replace("\n", "");
            MatchCollection mc = Regex.Matches(description, "\\w*\\s*=\\s*\\w*");

            foreach (Match m in mc)
            {
                string[] currentParam = m.ToString().Split('=');
                string paramName = currentParam[0].Trim(' ');
                string paramValue = currentParam[1].Trim(' ');
                QC.QCTSTestParameter newtsVar = new QC.QCTSTestParameter();
                if (paramName != null) { newtsVar.Name = paramName; }
                if (paramValue != null) { newtsVar.Value = paramValue; }
                newTSTest.Parameters.Add(newtsVar);
            }
        }

        private string CheckLinkedTSTestName(QCTestCase testCase)
        {
            QCTestCaseStepsColl testCasesSteps = QCRestAPIConnect.GetTestCaseSteps(testCase.Id);

            foreach (QCTestCaseStep step in testCasesSteps)
            {
                if (step.ElementsField.ContainsKey("link-test"))
                {
                    QCTestCase linkedTestCase = QCRestAPIConnect.GetTestCases(new List<string>() { step.ElementsField["link-test"].ToString() })[0];
                    return linkedTestCase.Name + ";" + linkedTestCase.Id;
                }
            }

            return null;
        }

        private QCTestCaseStepsColl GetListTSTestSteps(QCTestCase testCase)
        {
            QCTestCaseStepsColl testCaseSteps = QCRestAPIConnect.GetTestCaseSteps(testCase.Id);

            foreach (QCTestCaseStep step in testCaseSteps)
            {
                if (step.ElementsField.ContainsKey("link-test"))
                {
                    QCTestCaseStepsColl linkTestCaseSteps = QCRestAPIConnect.GetTestCasesSteps(new List<string>() { step.ElementsField["link-test"].ToString() });
                    return linkTestCaseSteps;
                }
            }

            return testCaseSteps;
        }
        private QCRunColl GetListTSTestRuns(QCTestCase testCase)
        {
            return QCRestAPIConnect.GetRunsByTestId(testCase.Id);
        }

        #endregion private functions
        public ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> almFields = new ObservableList<ExternalItemFieldBase>();
            List<Preference> fieldsValues = zephyrEntRepository.GetCustomFieldsValues();
            zephyrEntRepository.GetCustomFields().ForEach(ent => {
                if (!String.IsNullOrEmpty(ent.columnName) && ent.columnName.StartsWith("zcf_"))
                {
                    almFields.Add(new ExternalItemFieldBase()
                    {
                        ID = ent.columnName,
                        Name = String.IsNullOrEmpty(ent.displayName) ? ent.fieldName : ent.displayName,
                        ExternalID = ent.description,
                        Mandatory = ent.mandatory,
                        ItemType = ent.entityName,
                        PossibleValues = AddValuesToField(fieldsValues, ent.entityName, ent.fieldName)
                    });
                }
            });

            return almFields;
        }

        private ObservableList<string> AddValuesToField(List<Preference> fieldsValues, string entityName, string fieldName)
        {
            ObservableList<string> possibleValues = new ObservableList<string>();
            Preference field = fieldsValues.Find(val => val.name.Contains(String.Join(".", new [] { entityName.ToLower(), fieldName.ToLower() })));
            if (field != null)
            {
                List<dynamic> values = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(field.value);
                values.ForEach(x =>
                {
                    Dictionary<string, string> data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(((Newtonsoft.Json.Linq.JObject)x).ToString().Replace("{{", "{").Replace("}}", "}"));
                    if(data != null && data.ContainsKey("value"))
                    {
                        possibleValues.Add(String.Join("#",new[] { data["id"], data["value"] }));
                    }
                });
            }
            return possibleValues;
        }
    }
}