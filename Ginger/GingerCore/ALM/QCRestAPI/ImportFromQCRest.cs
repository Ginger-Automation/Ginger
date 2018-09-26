using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using QCRestClient;
using QCRestClient.Data_Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace GingerCore.ALM.QCRestAPI
{
    public static class ImportFromQCRest
    {
        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }

        #region Public Functions
        public static QCTestInstanceColl ImportTestSetInstanceData(QCTestSet TS)
        {
            return QCRestAPIConnect.GetTestInstancesOfTestSet(TS.Id);
        }

        public static QCTestCaseColl ImportTestSetTestCasesData(QCTestInstanceColl testinstances)
        {
            List<string> testCasesIds = new List<string>();

            foreach (QCTestInstance testInstance in testinstances)
                testCasesIds.Add(testInstance.TestId);

            return QCRestAPIConnect.GetTestCases(testCasesIds);
        }

        public static QCTestCaseStepsColl ImportTestCasesSteps(QCTestCaseColl testCases)
        {
            QCTestCaseStepsColl testCasesSteps = new QCTestCaseStepsColl();

            foreach (QCTestCase testCase in testCases)
            {
                testCasesSteps.AddRange(QCRestAPIConnect.GetTestCaseSteps(testCase.Id));
            }

            return testCasesSteps;
        }

        public static QC.QCTestSet ImportTestSetData(QC.QCTestSet testSet)
        {
            QCTestInstanceColl testInstances = GetListTSTest(testSet);

            foreach (QCTestInstance testInstance in testInstances)
            {
                testSet.Tests.Add(ImportTSTest(testInstance));
            }

            return testSet;
        }

        public static QC.QCTSTest ImportTSTest(QCTestInstance testInstance)
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
                    newTSTest.TestName = testInstance.Name;
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
                if ((linkedTest.Split(';')[0].ToString() != testCase.Name.ToString()) || (linkedTest.Split(';')[0].ToString() != testCase.Name.ToString()))
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

        public static QCTestCaseParamsColl ImportTestCasesParams(QCTestCaseColl testCases)
        {
            QCTestCaseParamsColl testCasesParams = new QCTestCaseParamsColl();

            foreach (QCTestCase testCase in testCases)
            {
                testCasesParams.AddRange(QCRestAPIConnect.GetTestCaseParams(testCase.Id));
            }

            return testCasesParams;
        }

        public static BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet testSet)
        {
            try
            {
                if (testSet == null) return null;

                //Creat Business Flow
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
                    if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.LinkedTestID).FirstOrDefault();
                    if (repoActivsGroup == null)
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
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
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else //TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.TestName;
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

                    //Add the TC steps as Activities if not already on the Activties group
                    foreach (QC.QCTSTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
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
                            stepActivity.Expected = StripHTML(step.Expected);

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity);
                            tcActivsGroup.AddActivityToGroup(stepActivity);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList = new List<string>();
                        GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                        GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                        foreach (string param in stepParamsList)
                        {
                            //get the param value
                            string paramSelectedValue = string.Empty;
                            bool? isflowControlParam = null;
                            QC.QCTSTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

                            //get the param value
                            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                                paramSelectedValue = tcParameter.Value;
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
                                if (valueParts.Count() == 3)
                                {
                                    linkedVariable = valueParts[1];
                                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                    if (busVariables.Keys.Contains(linkedVariable) == false)
                                    {
                                        busVariables.Add(linkedVariable, valueParts[2]);
                                    }
                                }
                            }

                            //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (paramSelectedValue.StartsWith("$$_"))
                                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                            }
                            else if (paramSelectedValue != "<Empty>")
                                isflowControlParam = true;

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
                                    stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
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
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
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
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
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
                                    ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                                    if (isflowControlParam == true)
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
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
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                            }

                            //add linked variable if needed
                            if (string.IsNullOrEmpty(linkedVariable) == false)
                            {
                                stepActivityVar.LinkedVariableName = linkedVariable;
                            }
                            else
                                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                        }
                    }

                    //order the Activities Group activities accourding to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (QC.QCTSTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null) break;//something wrong- shouldnt be null
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
                                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                    continue;//activity which not originaly came from the TC
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx) break;
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
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
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
                        ((VariableString)busVar).InitialStringValue = var.Value;
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

        public static QCRunStepColl GetRunSteps(string runId)
        {
            return QCRestAPIConnect.GetRunSteps(runId);
        }

        public static BusinessFlow ConvertQCTestSetToBF(QCTestSet tS, QCTestInstanceColl testInstances, QCTestCaseColl tSTestCases, QCTestCaseStepsColl tSTestCaseSteps, QCTestCaseParamsColl tSTestCasesParams)
        {
            try
            {
                if (tS == null)
                    return null;

                //Create Business Flow
                BusinessFlow busFlow = CreateBusinessFlow(tS);
                Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (QCTestInstance testInstance in testInstances)
                {
                    ActivitiesGroup tcActivsGroup = CheckIfTCAlreadyExistInRepo(busFlow, testInstance, tSTestCaseSteps);
                    AddTcStepsAsActivities(tcActivsGroup, busFlow, testInstance, tSTestCaseSteps, tSTestCasesParams, busVariables);

                    //order the Activities Group activities accourding to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (QCTestCaseStep step in tSTestCaseSteps)
                        {
                            int stepIndx = int.Parse(step.StepOrder) + 1;
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.Id).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null) break;//something wrong- shouldnt be null
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
                                        tSTestCaseSteps.Where(x => x.Id == ident.ActivityExternalID).FirstOrDefault() == null)
                                    continue;//activity which not originaly came from the TC
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx) break;
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
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - { MethodBase.GetCurrentMethod().Name }, Error - {ex.Message}");
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
                        ((VariableString)busVar).InitialStringValue = var.Value;
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

        public static QCTestCase GetQCTest(string testID)
        {
            QCTestCaseColl toReturn = QCRestAPIConnect.GetTestCases(new List<string> { testID });
            if (toReturn.Count >= 1)
                return toReturn[0];
            else
                return null;
        }

        public static QCTestSet GetQCTestSet(string testSetID)
        {
            return QCRestAPIConnect.GetTestSetDetails(testSetID);
        }

        private static ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            string testSetfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_SET);
            List<QCField> testSetfieldsCollection = QCRestAPIConnect.GetFields(testSetfieldInRestSyntax);

            string testCasefieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CASE);
            List<QCField> testCasefieldsCollection = QCRestAPIConnect.GetFields(testCasefieldInRestSyntax);

            string designStepfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP);
            List<QCField> designStepfieldsCollection = QCRestAPIConnect.GetFields(designStepfieldInRestSyntax);

            string testInstancefieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CYCLE);
            List<QCField> testInstancefieldsCollection = QCRestAPIConnect.GetFields(testInstancefieldInRestSyntax);

            string designStepParamsfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP_PARAMETERS);
            List<QCField> designStepParamsfieldsCollection = QCRestAPIConnect.GetFields(designStepParamsfieldInRestSyntax);

            string runfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_RUN);
            List<QCField> runfieldsCollection = QCRestAPIConnect.GetFields(runfieldInRestSyntax);

            fields.Append(AddFieldsValues(testSetfieldsCollection, testSetfieldInRestSyntax));
            fields.Append(AddFieldsValues(testCasefieldsCollection, testCasefieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepfieldsCollection, designStepfieldInRestSyntax));
            fields.Append(AddFieldsValues(testInstancefieldsCollection, testInstancefieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepParamsfieldsCollection, designStepParamsfieldInRestSyntax));
            fields.Append(AddFieldsValues(runfieldsCollection, runfieldInRestSyntax));

            return fields;
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields(ALM_Common.DataContracts.ResourceType resourceType)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if(QCRestAPIConnect.QcRestClient == null)
            {
                string qcbin = "qcbin";
                QCRestAPIConnect.QcRestClient = new QCClient(ALMCore.AlmConfig.ALMServerURL.TrimEnd(qcbin.ToCharArray()), ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMDomain, ALMCore.AlmConfig.ALMProjectName, 12);
            }

            if (QCRestAPIConnect.QcRestClient.Login())
            {
                if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
                    return GetALMItemFields();
                else
                {
                    string fieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(resourceType);
                    List<QCField> fieldsCollection = QCRestAPIConnect.GetFields(fieldInRestSyntax);

                    fields.Append(AddFieldsValues(fieldsCollection, fieldInRestSyntax));
                }
            }

            return fields;
        }

        #endregion Public Functions

        #region private functions

        private static QCTestCaseStep GetListTSTestVars(QCTestCase testCase)
        {
            QCTestCaseStepsColl steps = QCRestAPIConnect.GetTestCaseSteps(testCase.Id);

            foreach (QCTestCaseStep step in steps)
            {
                if (step.ElementsField.ContainsKey("link-test"))
                    return step;
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
                    relevantStep = testcaseStep;
            }
            if (relevantStep != null)
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == relevantStep.ElementsField["link-test"].ToString()).FirstOrDefault();
            if (repoActivsGroup == null)
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == testInstance.Id).FirstOrDefault();

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
                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
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
                    tcActivsGroup.ExternalID2 = testInstance.Id; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = QCRestAPIConnect.GetTestCases(new List<string>() { relevantStep.ElementsField["link-test"].ToString() })[0].ElementsField["description"].ToString();                                             //tcActivsGroup.Description = testInstance.des;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup);
            }

            return tcActivsGroup;
        }

        private static void AddTcStepsAsActivities(ActivitiesGroup tcActivsGroup, BusinessFlow busFlow, QCTestInstance testInstance, QCTestCaseStepsColl tSTestCaseSteps, QCTestCaseParamsColl tSTestCasesParams, Dictionary<string, string> busVariables)
        {
            IEnumerable<QCTestCaseStep> relevantSteps = tSTestCaseSteps.Where(step => step.TestId == testInstance.TestId);
            foreach (QCTestCaseStep step in relevantSteps)
            {
                Activity stepActivity;
                bool toAddStepActivity = false;

                //check if mapped activity exist in repository
                Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID == step.Id).FirstOrDefault();
                if (repoStepActivity != null)
                {
                    //check if it is part of the Activities Group
                    ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.Id).FirstOrDefault();
                    if (groupStepActivityIdent != null)
                    {
                        stepActivity = LinkStepAndUpdate(busFlow, groupStepActivityIdent, step, testInstance);
                    }
                    else//not in ActivitiesGroup so get instance from repo
                    {
                        stepActivity = (Activity)repoStepActivity.CreateInstance();
                        toAddStepActivity = true;
                    }
                }
                else//Step not exist in Ginger repository so create new one
                {
                    stepActivity = CreateNewStep(testInstance, step);
                    toAddStepActivity = true;
                }

                if (toAddStepActivity)
                {
                    //not in group- need to add it
                    busFlow.AddActivity(stepActivity);
                    tcActivsGroup.AddActivityToGroup(stepActivity);
                }

                QCTestInstanceParamColl paramsColl = QCRestAPIConnect.GetTestInstanceParams(testInstance.Id);

                PullTCStepParameterAndAddToActivityLevel(stepActivity, step, tSTestCasesParams, paramsColl, busVariables);
            }
        }

        private static Activity LinkStepAndUpdate(BusinessFlow busFlow, ActivityIdentifiers groupStepActivityIdent, QCTestCaseStep step, QCTestInstance testInstance)
        {
            Activity stepActivity;

            //already in Activities Group so get link to it
            stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
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

        private static void PullTCStepParameterAndAddToActivityLevel(Activity stepActivity, QCTestCaseStep step, QCTestCaseParamsColl tSTestCasesParams, QCTestInstanceParamColl testInstanceParamsColl, Dictionary<string, string> busVariables)
        {
            //pull TC-Step parameters and add them to the Activity level
            List<string> stepParamsList = new List<string>();
            GetStepParameters(StripHTML(step.Description), ref stepParamsList);
            GetStepParameters(StripHTML(step.ElementsField["expected"].ToString()), ref stepParamsList);
            foreach (string param in stepParamsList)
            {
                //get the param value
                string paramSelectedValue = string.Empty;
                bool? isflowControlParam = null;
                QCTestCaseParam tcParameter = tSTestCasesParams.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                QCTestInstanceParam testInstanceParameter = testInstanceParamsColl.Where(x => x.ParentId == tcParameter.Id).FirstOrDefault();

                //get the param value
                if (testInstanceParameter.ElementsField["actual-value"] != null)
                    paramSelectedValue = StripHTML(testInstanceParameter.ElementsField["actual-value"].ToString());
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
                    if (valueParts.Count() == 3)
                    {
                        linkedVariable = valueParts[1];
                        paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                        if (busVariables.Keys.Contains(linkedVariable) == false)
                        {
                            busVariables.Add(linkedVariable, valueParts[2]);
                        }
                    }
                }

                //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                if (paramSelectedValue.StartsWith("$$_"))
                {
                    isflowControlParam = false;
                    if (paramSelectedValue.StartsWith("$$_"))
                        paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                }
                else if (paramSelectedValue != "<Empty>")
                    isflowControlParam = true;

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
                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
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
                            stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
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
                            stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
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
                        ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                        if (isflowControlParam == true)
                            stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
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
                            ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                    }
                    catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                }

                //add linked variable if needed
                if (string.IsNullOrEmpty(linkedVariable) == false)
                {
                    stepActivityVar.LinkedVariableName = linkedVariable;
                }
                else
                    stepActivityVar.LinkedVariableName = string.Empty;//clear old links
            }
        }

        private static ObservableList<ExternalItemFieldBase> AddFieldsValues(List<QCField> testSetfieldsCollection, string testSetfieldInRestSyntax)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if ((testSetfieldsCollection != null) && (testSetfieldsCollection.Count > 0))
            {
                foreach (QCField field in testSetfieldsCollection)
                {
                    if (string.IsNullOrEmpty(field.Label)) continue;

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.PhysicalName;
                    itemfield.ExternalID = field.Name;  // Temp ??? Check if ExternalID has other use in this case
                    itemfield.Name = field.Label;
                    itemfield.Mandatory = field.IsRequired;
                    itemfield.SystemFieled = field.IsSystem;
                    if (itemfield.Mandatory)
                        itemfield.ToUpdate = true;
                    itemfield.ItemType = testSetfieldInRestSyntax.ToString();

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

            return fields;
        }

        private static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                    stripped = HttpUtility.HtmlDecode(stripped);

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
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while pulling the parameters names from QC TC Step Description/Expected", ex);
            }
        }

        private static QCTestInstanceColl GetListTSTest(QC.QCTestSet TS)
        {
            return QCRestAPIConnect.GetTestInstancesOfTestSet(TS.TestSetID);
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
                //if (TestParam.LinkedParams.Type(i) != null) { newtsVar.Type = TestParam.LinkedParams.Type(i).ToString(); }
                newTSTest.Parameters.Add(newtsVar);
            }
        }

        private static string CheckLinkedTSTestName(QCTestCase testCase)
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

        private static QCTestCaseStepsColl GetListTSTestSteps(QCTestCase testCase)
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

        public static string GetTSTestLinkedID(QCTestInstance testInstance)
        {
            QCTestCaseColl testCase = QCRestAPIConnect.GetTestCases(new List<string>() { testInstance.TestId });

            string linkedTest = CheckLinkedTSTestName(testCase[0]);
            if (linkedTest != null)
            {
                //Linked TC
                string[] linkTest = linkedTest.Split(';');
                return linkTest[1];
            }
            else
                return "";
        }

        private static QCRunColl GetListTSTestRuns(QCTestCase testCase)
        {
            return QCRestAPIConnect.GetRunsByTestId(testCase.Id);
        }

        #endregion private functions

    }
}
