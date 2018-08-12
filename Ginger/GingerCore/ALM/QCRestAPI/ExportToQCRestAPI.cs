using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using QCRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using ALM_Common.DataContracts;
using GingerCore.Variables;
using System.Reflection;
using GingerCore.Actions;
using ALMRestClient;

namespace GingerCore.ALM.QCRestAPI
{
    public static class ExportToQCRestAPI
    {

        #region public Methods
        /// <summary>
        /// Export Activities Group details to QC, can be used for creating new matching QC Test Case or updating an exisitng one
        /// </summary>
        /// <param name="activitiesGroup">Activities Group to Export</param>
        /// <param name="mappedTest">The QC Test Case which mapped to the Activities Group (in case exist) and needs to be updated</param>
        /// <param name="uploadPath">Upload path in QC Test Plan</param>
        /// <param name="result">Export error result</param>
        /// <returns></returns>
        public static bool ExportActivitiesGroupToQC(ActivitiesGroup activitiesGroup, QCTestCase mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string result)
        {
            try
            {
                QCTestCase test = null;

                if (mappedTest == null) //#Create new test case
                {
                    test = CreateNewTestCase(activitiesGroup, uploadPath, testCaseFields);

                    int order = 1;
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                        CreateTestStep(test, actIdent.IdentifiedActivity, designStepsFields, designStepsParamsFields, order++);
                }
                else //##update existing test case
                {
                    test = UpdateExistingTestCase(mappedTest, activitiesGroup, testCaseFields);
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                        UpdateTestStep(test, activitiesGroup, actIdent.IdentifiedActivity, designStepsFields, designStepsParamsFields);
                }

                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the Activities Group to QC/ALM", ex);
                return false;
            }
        }

        public static bool ExportBusinessFlowToQC(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ObservableList<ExternalItemFieldBase> testInstanceFields, ref string result)
        {
            QCTestSet testSet = null;
            ObservableList<ActivitiesGroup> existingActivitiesGroups = new ObservableList<ActivitiesGroup>();

            try
            {
                if (mappedTestSet == null) //##create new Test Set in QC
                {
                    testSet = CreateNewTestSet(businessFlow, uploadPath, testSetFields);
                    CreateNewTestInstances(businessFlow, existingActivitiesGroups, testSet, testInstanceFields);
                }
                else //##update existing test set
                {
                    testSet = UpdateExistingTestSet(businessFlow, mappedTestSet, uploadPath, testSetFields);
                    UpdateTestInstances(businessFlow, existingActivitiesGroups, testSet, testInstanceFields);
                }

                businessFlow.ExternalID = testSet.Id.ToString();
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the Business Flow to QC/ALM", ex);
                return false;
            }
        }

        public static bool ExportExecutionDetailsToQC(BusinessFlow bizFlow, ref string result, PublishToALMConfig publishToALMConfig)
        {
            throw new NotImplementedException();
        }
        #endregion Public Methods

        #region private methods
        private static QCTestCase CreateNewTestCase(ActivitiesGroup activitiesGroup, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields)
        {
            QCTestCase test = new QCTestCase();

            test.ElementsField["subtype-id"] = "MANUAL";
            test.ElementsField["parent-id"] = QCRestAPIConnect.GetLastTestPlanIdFromPath(uploadPath).ToString();

            //set item fields
            foreach (ExternalItemFieldBase field in testCaseFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        test.ElementsField.Add(field.ExternalID, field.SelectedValue);
                    else
                        try { test.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            //post the test
            test.ElementsField["name"] = activitiesGroup.Name;
            test.ElementsField["description"] = activitiesGroup.Description;
            QCItem item = ConvertObjectValuesToQCItem(test, ResourceType.TEST_CASE);
            ALMResponseData response = QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.TEST_CASE, item);

            test.Id = response.IdCreated;
            activitiesGroup.ExternalID = test.Id;
            activitiesGroup.ExternalID2 = test.Id;

            return QCRestAPIConnect.QcRestClient.GetTestCases(new List<string> { test.Id })[0];
        }

        private static QCTestSet CreateNewTestSet(BusinessFlow businessFlow, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields)
        {
            QCTestSet testSet = new QCTestSet();

            //set the upload path
            testSet.ElementsField["parent-id"] = QCRestAPIConnect.GetLastTestSetIdFromPath(uploadPath).ToString();

            //set item fields for test set
            foreach (ExternalItemFieldBase field in testSetFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                        testSet.ElementsField[field.ExternalID] = field.SelectedValue;
                    else
                        try { testSet.ElementsField[field.ID] = "NA"; }
                        catch { }
                }
            }

            testSet.ElementsField["name"] = businessFlow.Name;
            testSet.ElementsField["subtype-id"] = "hp.qc.test-set.default";

            try
            {
                QCItem item = ConvertObjectValuesToQCItem(testSet, ResourceType.TEST_SET);
                ALMResponseData response = QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.TEST_SET, item);
                return QCRestAPIConnect.QcRestClient.GetTestSetDetails(response.IdCreated);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The Test Set already exists"))
                {
                    string result = "Cannot export Business Flow - The Test Set already exists in the selected folder. ";
                    Reporter.ToLog(eLogLevel.ERROR, result, ex);
                    return null;
                }

                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return null;
            }
        }

        private static bool CreateTestStep(QCTestCase test, Activity activity, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, int stepOrder)
        {

            //create new step
            QCTestCaseStep step = new QCTestCaseStep();

            //set item fields
            foreach (ExternalItemFieldBase field in designStepsFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        step.ElementsField.Add(field.ExternalID, field.SelectedValue);
                    else
                        try { step.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            step.ElementsField["name"] = activity.ActivityName;
            step.ElementsField["parent-id"] = test.Id;
            step.ElementsField["step-order"] = stepOrder.ToString();

            string descriptionTemplate =
                    "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Description&&>><br /><<&Parameters&>><br /><<&Actions&>></span></font></div></body></html>";
            string description = descriptionTemplate.Replace("<<&Description&&>>", activity.Description);
            QCTestCaseParamsColl testParams = QCRestAPIConnect.QcRestClient.GetTestCaseParams(test.Id);
            string paramsSigns = string.Empty;
            if (activity.Variables.Count > 0)
            {
                paramsSigns = "<br />Parameters:<br />";
                foreach (VariableBase var in activity.Variables)
                {
                    paramsSigns += "&lt;&lt;&lt;" + var.Name.ToLower() + "&gt;&gt;&gt;<br />";
                    //try to add the paramter to the test case parameters list
                    try
                    {
                        QCTestCaseParam newParam = new QCTestCaseParam();

                        //set item fields
                        foreach (ExternalItemFieldBase field in designStepsParamsFields)
                        {
                            if (field.ToUpdate || field.Mandatory)
                            {
                                if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                                    newParam.ElementsField.Add(field.ExternalID, field.SelectedValue);
                                else
                                    try { newParam.ElementsField.Add(field.ExternalID, "NA"); }
                                    catch { }
                            }
                        }

                        newParam.Name = var.Name.ToLower();
                        newParam.TestId = test.Id;

                        QCItem itemTestCaseParam = ConvertObjectValuesToQCItem(newParam, ResourceType.TEST_CASE_PARAMETERS);
                        QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.TEST_CASE_PARAMETERS, itemTestCaseParam);
                    }
                    catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                }
            }
            description = description.Replace("<<&Parameters&>>", paramsSigns);

            string actsDesc = string.Empty;
            if (activity.Acts.Count > 0)
            {
                actsDesc = "Actions:<br />";
                foreach (Act act in activity.Acts)
                    actsDesc += act.Description + "<br />";
            }
            description = description.Replace("<<&Actions&>>", actsDesc);
            step.Description = description;
            step.ElementsField["expected"] = activity.Expected;

            QCItem itemDesignStep = ConvertObjectValuesToQCItem(step, ResourceType.DESIGN_STEP);
            ALMResponseData response = QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.DESIGN_STEP, itemDesignStep);

            activity.ExternalID = response.IdCreated;

            if (activity.ExternalID != null)
                return true;
            else
                return false;
        }

        private static void CreateNewTestInstances(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> existingActivitiesGroups, QCTestSet testSet, ObservableList<ExternalItemFieldBase> testInstancesFields)
        {
            QCTestInstanceColl testInstanceColl = null;
            int counter = 1;
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                if (existingActivitiesGroups.Contains(ag) == false && string.IsNullOrEmpty(ag.ExternalID) == false && ImportFromQCRest.GetQCTest(ag.ExternalID) != null)
                {
                    QCTestInstance testInstance = new QCTestInstance
                    {
                        TestId = ag.ExternalID,
                        CycleId = testSet.Id,
                        TestOrder = counter++.ToString(),
                    };

                    //set item fields for test instances
                    foreach (ExternalItemFieldBase field in testInstancesFields)
                    {
                        if ((field.ToUpdate || field.Mandatory) && (!(field.ExternalID == "test-id") && !(field.ExternalID == "cycle-id")))
                        {
                            if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                                testInstance.ElementsField[field.ExternalID] = field.SelectedValue;
                            else
                                try { testInstance.ElementsField[field.ID] = "NA"; }
                                catch { }
                        }
                    }

                    testInstance.ElementsField["subtype-id"] = "hp.qc.test-instance.MANUAL";
                    QCItem item = ConvertObjectValuesToQCItem(testInstance, ResourceType.TEST_CYCLE);
                    ALMResponseData response = QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.TEST_CYCLE, item);

                    if (response.IsSucceed) // # Currently bug in HPE failing the test instance creation despite it working.
                    {
                        //QCTestInstance testInstanceCreated = QCRestAPIConnect.QcRestClient.GetTestInstanceDetails(response.IdCreated);
                        ag.ExternalID2 = response.IdCreated;//the test case instance ID in the test set- used for exporting the execution details
                    }
                }
            }
        }

        private static void UpdateTestStep(QCTestCase test, ActivitiesGroup activitiesGroup, Activity identifiedActivity, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields)
        {
            QCTestCaseStepsColl testCaseDesignStep = QCRestAPIConnect.QcRestClient.GetTestCasesSteps(new List<string> { test.Id });

            //delete the un-needed steps
            foreach (QCTestCaseStep step in testCaseDesignStep)
            {
                if (activitiesGroup.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.ExternalID == step.Id.ToString()).FirstOrDefault() == null)
                    QCRestAPIConnect.QcRestClient.DeleteEntity(ALM_Common.DataContracts.ResourceType.DESIGN_STEP, step.Id);
            }

            //delete the existing parameters
            QCTestCaseParamsColl testCaseParams = QCRestAPIConnect.QcRestClient.GetTestCaseParams(test.Id);

            if (testCaseParams.Count > 0)
            {
                for (int indx = 0; indx < testCaseParams.Count; indx++)
                {
                    QCRestAPIConnect.QcRestClient.DeleteEntity(ALM_Common.DataContracts.ResourceType.DESIGN_STEP_PARAMETERS, testCaseParams[indx].Id);
                }
            }

            foreach (QCTestCaseStep step in testCaseDesignStep)
            {
                //set item fields
                foreach (ExternalItemFieldBase field in designStepsFields)
                {
                    if (field.ToUpdate || field.Mandatory)
                    {
                        if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        {
                            if (step.ElementsField.ContainsKey(field.ExternalID))
                                step.ElementsField[field.ExternalID] = field.SelectedValue;
                        }
                    }
                }

                step.ElementsField["name"] = identifiedActivity.ActivityName;
                string descriptionTemplate =
                    "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Description&&>><br /><<&Parameters&>><br /><<&Actions&>></span></font></div></body></html>";
                string description = descriptionTemplate.Replace("<<&Description&&>>", identifiedActivity.Description);

                QCTestCaseParamsColl testParams = QCRestAPIConnect.QcRestClient.GetTestCaseParams(test.Id);
                string paramsSigns = string.Empty;
                if (identifiedActivity.Variables.Count > 0)
                {
                    paramsSigns = "<br />Parameters:<br />";
                    foreach (VariableBase var in identifiedActivity.Variables)
                    {
                        paramsSigns += "&lt;&lt;&lt;" + var.Name.ToLower() + "&gt;&gt;&gt;<br />";
                        //try to add the paramter to the test case parameters list
                        try
                        {
                            QCTestCaseParam newParam = new QCTestCaseParam();

                            //set item fields
                            foreach (ExternalItemFieldBase field in designStepsParamsFields)
                            {
                                if (field.ToUpdate || field.Mandatory)
                                {
                                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                                        newParam.ElementsField.Add(field.ExternalID, field.SelectedValue);
                                    else
                                        try { newParam.ElementsField.Add(field.ExternalID, "NA"); }
                                        catch { }
                                }
                            }

                            newParam.Name = var.Name.ToLower();
                            newParam.TestId = test.Id;

                            QCItem itemTestCaseParam = ConvertObjectValuesToQCItem(newParam, ResourceType.TEST_CASE_PARAMETERS);
                            QCRestAPIConnect.QcRestClient.CreateNewEntity(ResourceType.TEST_CASE_PARAMETERS, itemTestCaseParam);
                        }
                        catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                    }
                }

                description = description.Replace("<<&Parameters&>>", paramsSigns);

                string actsDesc = string.Empty;
                if (identifiedActivity.Acts.Count > 0)
                {
                    actsDesc = "Actions:<br />";
                    foreach (Act act in identifiedActivity.Acts)
                        actsDesc += act.Description + "<br />";
                }
                description = description.Replace("<<&Actions&>>", actsDesc);
                step.Description = description;
                step.ElementsField["expected"] = identifiedActivity.Expected;

                QCItem itemDesignStep = ConvertObjectValuesToQCItem(step, ResourceType.DESIGN_STEP, true);
                ALMResponseData response = QCRestAPIConnect.QcRestClient.UpdateEntity(ResourceType.DESIGN_STEP, step.Id, itemDesignStep);

                identifiedActivity.ExternalID = step.Id;
            }

        }

        private static QCTestCase UpdateExistingTestCase(QCTestCase mappedTest, ActivitiesGroup activitiesGroup, ObservableList<ExternalItemFieldBase> testCaseFields)
        {
            QCTestCase test = mappedTest;

            //set item fields
            foreach (ExternalItemFieldBase field in testCaseFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                    {
                        if (test.ElementsField.ContainsKey(field.ExternalID))
                            test.ElementsField[field.ExternalID] = field.SelectedValue;
                    }

                    else
                        try { test.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            //update the test
            test.ElementsField["name"] = activitiesGroup.Name;
            test.ElementsField["description"] = activitiesGroup.Description;
            QCItem item = ConvertObjectValuesToQCItem(test, ResourceType.TEST_CASE, true);
            ALMResponseData response = QCRestAPIConnect.QcRestClient.UpdateEntity(ResourceType.TEST_CASE, test.Id, item);

            activitiesGroup.ExternalID = test.Id;
            activitiesGroup.ExternalID2 = test.Id;

            return test;
        }

        private static QCTestSet UpdateExistingTestSet(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields)
        {
            QCTestSet testSet = ImportFromQCRest.GetQCTestSet(mappedTestSet.Id.ToString());

            //set item fields for test set
            foreach (ExternalItemFieldBase field in testSetFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                        if (testSet.ElementsField.ContainsKey(field.ID))
                        {
                            testSet.ElementsField[field.ExternalID] = field.SelectedValue;
                        }
                }
            }

            testSet.ElementsField["name"] = businessFlow.Name;

            try
            {
                QCItem item = ConvertObjectValuesToQCItem(testSet, ResourceType.TEST_SET, true);
                ALMResponseData response = QCRestAPIConnect.QcRestClient.UpdateEntity(ResourceType.TEST_SET, testSet.Id, item);
                return QCRestAPIConnect.QcRestClient.GetTestSetDetails(testSet.Id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return null;
            }
        }

        private static void UpdateTestInstances(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> existingActivitiesGroups, QCTestSet testSet, ObservableList<ExternalItemFieldBase> testInstancesFields)
        {
            QCTestInstanceColl testInstances = ImportFromQCRest.ImportTestSetInstanceData(testSet);

            foreach (QCTestInstance testInstance in testInstances)
            {
                ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == testInstance.TestId.ToString() && x.ExternalID2 == testInstance.Id.ToString())).FirstOrDefault();
                if (ag == null)
                    QCRestAPIConnect.QcRestClient.DeleteEntity(ResourceType.TEST_CYCLE, testInstance.Id);
                else
                {
                    existingActivitiesGroups.Add(ag);
                    //set item fields for test instances
                    foreach (ExternalItemFieldBase field in testInstancesFields)
                    {
                        if ((field.ToUpdate || field.Mandatory) && (!(field.ExternalID == "test-id") && !(field.ExternalID == "cycle-id")))
                        {
                            if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                            {
                                if (testInstance.ElementsField.ContainsKey(field.ID))
                                {
                                    testInstance.ElementsField[field.ExternalID] = field.SelectedValue;
                                }
                            }
                        }
                    }

                    try
                    {
                        QCItem item = ConvertObjectValuesToQCItem(testInstance, ResourceType.TEST_CYCLE, true);
                        ALMResponseData response = QCRestAPIConnect.QcRestClient.UpdateEntity(ResourceType.TEST_CYCLE, testInstance.Id, item);

                        if (response.IsSucceed)
                        {
                            testInstances.Add(QCRestAPIConnect.QcRestClient.GetTestInstanceDetails(testInstance.Id));
                            ag.ExternalID2 = response.IdCreated;//the test case instance ID in the test set- used for exporting the execution details
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    }
                }
            }
        }

        private static QCItem ConvertObjectValuesToQCItem(object item, ResourceType type, bool isUpdate = false)
        {
            QCItem itemWithValues = new QCItem();

            dynamic itemVals = item;

            foreach (var keyValue in itemVals.ElementsField)
            {
                if (keyValue.Key != "order-id")
                    itemWithValues.Fields.Add(keyValue.Key, keyValue.Value);
            }

            if (itemVals.GetType().GetProperty("Id") != null)
            {
                // Do nothing
            }
            if (itemVals.GetType().GetProperty("ParentId") != null && itemVals.GetType().GetProperty("ParentId").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("parent-id", itemVals.GetType().GetProperty("ParentId").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("Description") != null && itemVals.GetType().GetProperty("Description").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("description", EscapeChars(itemVals.GetType().GetProperty("Description").GetValue(itemVals, null)));
            }
            if (itemVals.GetType().GetProperty("Name") != null && itemVals.GetType().GetProperty("Name").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("name", EscapeChars(itemVals.GetType().GetProperty("Name").GetValue(itemVals, null)));
            }
            if (itemVals.GetType().GetProperty("Path") != null && itemVals.GetType().GetProperty("Path").GetValue(itemVals, null) != null && !isUpdate)
            {
                if (!(type == ResourceType.TEST_SET))
                    itemWithValues.Fields.Add("hierarchical-path", itemVals.GetType().GetProperty("Path").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("DefualtValue") != null && itemVals.GetType().GetProperty("DefualtValue").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("default-value", itemVals.GetType().GetProperty("DefualtValue").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("ActualValue") != null && itemVals.GetType().GetProperty("ActualValue").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("actual-value", itemVals.GetType().GetProperty("ActualValue").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("TestOrder") != null && itemVals.GetType().GetProperty("TestOrder").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("order-id", itemVals.GetType().GetProperty("TestOrder").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("TestId") != null && itemVals.GetType().GetProperty("TestId").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("test-id", itemVals.GetType().GetProperty("TestId").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("CycleId") != null && itemVals.GetType().GetProperty("CycleId").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("cycle-id", itemVals.GetType().GetProperty("CycleId").GetValue(itemVals, null));
            }

            return itemWithValues;
        }

        private static string EscapeChars(string origStr)
        {
            if (origStr != null)
            {
                origStr = origStr.Replace("&", "&amp;");
                origStr = origStr.Replace("<", "&lt;");
                origStr = origStr.Replace(">", "&gt;");
                origStr = origStr.Replace("\"", "&quot;");
                origStr = origStr.Replace("\'", "&apos;");
            }

            return origStr;
        }
        #endregion private Methods
    }
}
