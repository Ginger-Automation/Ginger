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

        public static QCTestInstanceColl ImportTestSetInstanceData(QCTestSet TS)
        {
            return QCRestAPIConnect.QcRestClient.GetTestInstancesOfTestSet(TS.Id);
        }

        public static QCTestCaseColl ImportTestSetTestCasesData(QCTestInstanceColl testinstances)
        {
            List<string> testCasesIds = new List<string>();

            foreach (QCTestInstance testInstance in testinstances)
                testCasesIds.Add(testInstance.TestId);

            return QCRestAPIConnect.QcRestClient.GetTestCases(testCasesIds);
        }

        public static QCTestCaseStepsColl ImportTestCasesSteps(QCTestCaseColl testCases)
        {
            QCTestCaseStepsColl testCasesSteps = new QCTestCaseStepsColl();

            foreach (QCTestCase testCase in testCases)
            {
                testCasesSteps.AddRange(QCRestAPIConnect.QcRestClient.GetTestCaseSteps(testCase.Id));
            }

            return testCasesSteps;
        }

        public static QCTestSet ImportTestSetData(string tSId)
        {
            return QCRestAPIConnect.QcRestClient.GetTestSetDetails(tSId);
        }

        public static QCTestCaseParamsColl ImportTestCasesParams(QCTestCaseColl testCases)
        {
            QCTestCaseParamsColl testCasesParams = new QCTestCaseParamsColl();

            foreach (QCTestCase testCase in testCases)
            {
                testCasesParams.AddRange(QCRestAPIConnect.QcRestClient.GetTestCaseParams(testCase.Id));
            }

            return testCasesParams;
        }

        public static QCTestInstanceColl GetQCTestInstances(QCTestSet testSet)
        {
            return QCRestAPIConnect.QcRestClient.GetTestInstancesOfTestSet(testSet.Id);
        }

        public static string GetTSTestLinkedID(QCTestInstance testInstance)
        {
            QCTestCaseStepsColl relevantTestCaseSteps = QCRestAPIConnect.QcRestClient.GetTestCaseSteps(testInstance.TestId);

            foreach (QCTestCaseStep step in relevantTestCaseSteps)
            {
                if (step.ElementsField["link-test"] != null)
                    return step.ElementsField["link-test"].ToString();
            }

            return "0";
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

        #region private functions
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
            if (testInstance.TestId != null && testInstance.TestId != string.Empty)
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == testInstance.TestId).FirstOrDefault();
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
                if (testInstance.TestId == null || testInstance.TestId == string.Empty)
                {
                    tcActivsGroup.ExternalID = testInstance.Id;
                    tcActivsGroup.ExternalID2 = testInstance.Id;
                }
                else
                {
                    tcActivsGroup.ExternalID = testInstance.TestId;
                    tcActivsGroup.ExternalID2 = testInstance.Id; //original TC ID will be used for uploading the execution details back to QC
                                                                 //tcActivsGroup.Description = testInstance.des;
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

                QCTestInstanceParamColl paramsColl = QCRestAPIConnect.QcRestClient.GetTestInstanceParams(testInstance.Id);

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

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            string testSetfieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_SET);
            List<QCField> testSetfieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(testSetfieldInRestSyntax);

            string testCasefieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CASE);
            List<QCField> testCasefieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(testCasefieldInRestSyntax);

            string designStepfieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP);
            List<QCField> designStepfieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(designStepfieldInRestSyntax);

            string designStepParamsfieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP_PARAMETERS);
            List<QCField> designStepParamsfieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(designStepfieldInRestSyntax);

            fields.Append(AddFieldsValues(testSetfieldsCollection, testSetfieldInRestSyntax));
            fields.Append(AddFieldsValues(testCasefieldsCollection, testCasefieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepfieldsCollection, designStepfieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepParamsfieldsCollection, designStepParamsfieldInRestSyntax));

            return fields;
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields(ALM_Common.DataContracts.ResourceType resourceType)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            string fieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(resourceType);
            List<QCField> fieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(fieldInRestSyntax);

            fields.Append(AddFieldsValues(fieldsCollection, fieldInRestSyntax));

            return fields;
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
                stripped = stripped.TrimStart('\n' , '\r');
                stripped = stripped.TrimEnd('\n' , '\r');

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

        public static QCTestCase GetQCTest(string testID)
        {
            return QCRestAPIConnect.QcRestClient.GetTestCases(new List<string> { testID })[0];
        }

        public static QCTestSet GetQCTestSet(string testSetID)
        {
            return QCRestAPIConnect.QcRestClient.GetTestSetDetails(testSetID);
        }

        public static ObservableList<ExternalItemFieldBase> GetQCEntityFieldsREST(ALM_Common.DataContracts.ResourceType fieldType)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            string fieldInRestSyntax = QCRestAPIConnect.QcRestClient.ConvertResourceType(fieldType);
            List<QCField> fieldsCollection = QCRestAPIConnect.QcRestClient.GetFields(fieldInRestSyntax);

            if ((fieldsCollection != null) && (fieldsCollection.Count > 0))
            {
                foreach (QCField field in fieldsCollection)
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
                    itemfield.ItemType = fieldType.ToString();

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
        #endregion private functions

    }
}
