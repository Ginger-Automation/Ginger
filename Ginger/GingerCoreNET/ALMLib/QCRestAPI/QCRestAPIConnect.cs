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

using AlmDataContractsStd.Enums;
using ALMRestClientStd;
using Amdocs.Ginger.Common;
using GingerCore.ALM.QC;
using QCRestClientStd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GingerCore.ALM.QCRestAPI
{
    public static class QCRestAPIConnect
    {
        public static string ServerURL { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string CurrentDomain { get; set; }
        public static string CurrentProject { get; set; }
        public static QCClient QcRestClient { get; set; }

        public static Dictionary<string, string> ExploredTestLabFolder = [];
        public static Dictionary<string, string> ExploredTestSets = [];
        public static Dictionary<string, string> ExploredTestPlanFolder = [];

        public static bool ConnectQCServer(string qcServerUrl, string qcUserName, string qcPassword)
        {
            string validateQcServerUrl = qcServerUrl;
            if (validateQcServerUrl.ToLowerInvariant().EndsWith("qcbin"))
            {
                validateQcServerUrl = qcServerUrl.Remove(qcServerUrl.Length - 5);
            }
            if (QcRestClient == null || validateQcServerUrl != ServerURL || qcUserName != UserName || qcPassword != Password)
            {
                QcRestClient = new QCClient(validateQcServerUrl, qcUserName, qcPassword);
            }

            ServerURL = validateQcServerUrl;
            UserName = qcUserName;
            Password = qcPassword;

            return QcRestClient.Login();
        }

        public static Boolean IsServerConnected
        {
            get
            {
                try
                {
                    if (QcRestClient != null)
                    {
                        if (QcRestClient.IsAuthenticated())
                        {
                            return true;
                        }
                        else
                            if (QcRestClient.Login())
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    return false;
                }
            }
        }

        public static void DisconnectQCServer()
        {
            if (QcRestClient == null)
            {
                return;
            }

            QcRestClient.Logout();
        }

        public static bool DisconnectQCProjectStayLoggedIn()
        {
            CurrentDomain = null;
            CurrentProject = null;

            if (QcRestClient == null)
            {
                QcRestClient = new QCClient(ServerURL, UserName, Password);
            }

            if (!QcRestClient.IsAuthenticated())
            {
                return QcRestClient.Login();
            }

            return true;
        }

        public static bool ConnectQCProject(string qcServerUrl, string qcUserName, string qcPassword, string qcDomain, string qcProject)
        {
            if (string.IsNullOrEmpty(qcServerUrl) == false &&
                string.IsNullOrEmpty(qcUserName) == false &&
                string.IsNullOrEmpty(qcPassword) == false &&
                string.IsNullOrEmpty(qcDomain) == false &&
                string.IsNullOrEmpty(qcProject) == false)
            {
                if (ConnectQCServer(qcServerUrl, qcUserName, qcPassword))
                {
                    return ConnectQCProject(qcDomain, qcProject);
                }
            }
            return false;
        }

        public static bool ConnectQCProject(string qcDomain, string qcProject)
        {
            CurrentDomain = qcDomain;
            CurrentProject = qcProject;

            QcRestClient = new QCRestClientStd.QCClient(ServerURL, UserName, Password, qcDomain, qcProject);
            return QcRestClient.Login();
        }

        public static List<string> GetQCDomains()
        {
            return QcRestClient.getDomainsList();
        }

        public static Dictionary<string, string> GetQCDomainProjects(string domainName)
        {
            return QcRestClient.getProjectsList(domainName).ToDictionary(prj => prj, prj => prj);
        }

        public static int GetLastTestPlanIdFromPath(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testPlanPathList = [];

            separatePath[0] = ExploredTestPlanFolder.ContainsKey("Subject") ? ExploredTestPlanFolder["Subject"] : QcRestClient.GetTestPlanRootFolder().Id;

            if (!ExploredTestPlanFolder.ContainsKey("Subject"))
            {
                ExploredTestPlanFolder.Add("Subject", separatePath[0]);
            }

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestPlanFolderId(separatePath[i], separatePath[i - 1]);
            }

            return int.Parse(separatePath.Last());
        }

        public static int GetLastTestSetIdFromPath(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testSetPathList = [];

            separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

            if (!ExploredTestLabFolder.ContainsKey("Root"))
            {
                ExploredTestLabFolder.Add("Root", separatePath[0]);
            }

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
            }

            return int.Parse(separatePath.Last());
        }

        // get test plan explorer(tree view)
        public static List<string> GetTestPlanExplorer(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testPlanPathList = [];

            separatePath[0] = ExploredTestPlanFolder.ContainsKey("Subject") ? ExploredTestPlanFolder["Subject"] : QcRestClient.GetTestPlanRootFolder().Id;

            if (!ExploredTestPlanFolder.ContainsKey("Subject"))
            {
                ExploredTestPlanFolder.Add("Subject", separatePath[0]);
            }

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestPlanFolderId(separatePath[i], separatePath[i - 1]);
            }

            QCTestPlan testPlanToExplor = QcRestClient.GetTestPlanTreeLayerByFilter(separatePath[^1], "");

            foreach (QCTestFolder folder in testPlanToExplor.folders)
            {
                testPlanPathList.Add(folder.Name);
            }

            return testPlanPathList;
        }

        public static QCRun GetRunDetail(string id)
        {
            return QcRestClient.GetRun(id);
        }

        public static List<string> GetTestLabExplorer(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testlabPathList = [];
            try
            {
                separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

                if (!ExploredTestLabFolder.ContainsKey("Root"))
                {
                    ExploredTestLabFolder.Add("Root", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                QCTestSetFolderColl foldersToExplor = QcRestClient.GetTestSetTreeLayerByFilter(separatePath[^1], "");

                foreach (QCTestSetFolder folders in foldersToExplor)
                {
                    testlabPathList.Add(folders.Name);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Lab with REST API", ex);
            }
            return testlabPathList;
        }

        // get test set explorer(tree view)
        public static List<ALMTestSetSummary> GetTestSetExplorer(string PathNode)
        {
            List<ALMTestSetSummary> testlabPathList = [];
            string[] separatePath = PathNode.Split('\\');
            try
            {
                separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

                if (!ExploredTestLabFolder.ContainsKey("Root"))
                {
                    ExploredTestLabFolder.Add("Root", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                QCTestSetColl testSets = QcRestClient.GetAllTestSetsUnderFolder(int.Parse(separatePath[^1]));

                foreach (QCRestClientStd.QCTestSet testset in testSets)
                {
                    ALMTestSetSummary QCTestSetTreeItem = new ALMTestSetSummary
                    {
                        TestSetID = testset.Id,
                        TestSetName = testset.Name
                    };
                    testlabPathList.Add(QCTestSetTreeItem);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Set with REST API", ex);
            }
            return testlabPathList;
        }

        public static dynamic GetTSRunStatus(dynamic TSItem)
        {
            QCTestInstanceColl testInstances = QcRestClient.GetTestInstancesOfTestSet(TSItem.TestSetID);

            foreach (QCTestInstance testInstance in testInstances)
            {
                bool existing = false;
                foreach (string[] status in TSItem.TestSetStatuses)
                {
                    if (status[0] == testInstance.Status)
                    {
                        existing = true;
                        status[1] = (Int32.Parse(status[1]) + 1).ToString();
                    }
                }
                if (!existing) { TSItem.TestSetStatuses.Add(new string[] { testInstance.Status, "1" }); }
            }

            return TSItem;
        }

        private static string GetTestLabFolderId(string separateAti, string separateAtIMinusOne)
        {
            if (!ExploredTestLabFolder.ContainsKey(separateAti))
            {
                QCTestSetFolder testSetFol = QcRestClient.GetTestSetFolderDetailsByNameAndId(separateAti, separateAtIMinusOne);
                ExploredTestLabFolder.Add(testSetFol.Name, testSetFol.Id);
                return testSetFol.Id;
            }
            else
            {
                return ExploredTestLabFolder[separateAti];
            }
        }

        private static string GetTestPlanFolderId(string separateAti, string separateAtIMinusOne)
        {
            if (!ExploredTestPlanFolder.ContainsKey(separateAti))
            {
                QCTestFolder testPlanFol = QcRestClient.GetTestPlansByNameAndParentId(separateAti, separateAtIMinusOne);
                ExploredTestPlanFolder.Add(testPlanFol.Name, testPlanFol.Id);
                return testPlanFol.Id;
            }
            else
            {
                return ExploredTestPlanFolder[separateAti];
            }
        }

        #region QCRestclient manager functions

        public static QCTestCaseColl GetTestCases(List<string> testCasesIds)
        {
            try
            {
                return QcRestClient.GetTestCases(testCasesIds);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test cases with REST API", ex);
                return null;
            }
        }

        public static QCRestClientStd.QCTestSet GetTestSetDetails(string testSetID)
        {
            try
            {
                return QcRestClient.GetTestSetDetails(testSetID);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test set details with REST API", ex);
                return null;
            }
        }

        public static string ConvertResourceType(ResourceType resourceType)
        {
            try
            {
                return QcRestClient.ConvertResourceType(resourceType);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to convert resource type with REST API", ex);
                return null;
            }
        }

        public static List<QCField> GetFields(string testSetfieldInRestSyntax)
        {
            try
            {
                return QcRestClient.GetFields(testSetfieldInRestSyntax);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get fields with REST API", ex);
                return null;
            }
        }

        public static QCTestCaseStepsColl GetTestCaseSteps(string id)
        {
            try
            {
                return QcRestClient.GetTestCaseSteps(id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test case steps with REST API", ex);
                return null;
            }
        }

        public static QCTestInstanceParamColl GetTestInstanceParams(string id)
        {
            try
            {
                return QcRestClient.GetTestInstanceParams(id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test instances with REST API", ex);
                return null;
            }
        }

        public static QCTestInstanceColl GetTestInstancesOfTestSet(string testSetID)
        {
            try
            {
                return QcRestClient.GetTestInstancesOfTestSet(testSetID);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test instances of test set with REST API", ex);
                return null;
            }
        }

        public static QCTestCaseStepsColl GetTestCasesSteps(List<string> list)
        {
            try
            {
                return QcRestClient.GetTestCasesSteps(list);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test cases steps with REST API", ex);
                return null;
            }
        }

        public static QCRunColl GetRunsByTestId(string id)
        {
            try
            {
                return QcRestClient.GetRunsByTestId(id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get runs by test id with REST API", ex);
                return null;
            }
        }


        public static QCTestCaseParamsColl GetTestCaseParams(string id)
        {
            try
            {
                return QcRestClient.GetTestCaseParams(id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test cases parameters with REST API", ex);
                return null;
            }
        }

        public static QCTestInstance GetTestInstanceDetails(string id)
        {
            try
            {
                return QcRestClient.GetTestInstanceDetails(id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test instance details with REST API", ex);
                return null;
            }
        }

        public static ALMResponseData CreateNewEntity(ResourceType resourceType, QCItem item)
        {
            try
            {
                return QcRestClient.CreateNewEntity(resourceType, item);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create entity with REST API", ex);
                return null;
            }
        }

        public static ALMResponseData CreateAttachment(ResourceType resourceType, string id, string zipFileName)
        {
            FileStream fs = new FileStream(zipFileName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            byte[] fileData = br.ReadBytes((Int32)fs.Length);
            ALMResponseData response = QcRestClient.CreateAttachmentForEntitiyId(ResourceType.TEST_RUN, id, zipFileName.Split(Path.DirectorySeparatorChar).Last(), fileData);
            fs.Close();

            return response;
        }

        public static void DeleteEntity(ResourceType resourceType, string id)
        {
            try
            {
                QcRestClient.DeleteEntity(resourceType, id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to delete entity with REST API", ex);
            }
        }

        public static ALMResponseData UpdateEntity(ResourceType resourceType, string id, QCItem itemDesignStep)
        {
            try
            {
                return QcRestClient.UpdateEntity(resourceType, id, itemDesignStep);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update entity with REST API", ex);
                return null;
            }
        }

        public static QCRunStepColl GetRunSteps(string runId)
        {
            return QcRestClient.GetRunSteps(runId);
        }

        #endregion QCRestclient manager functions

    }
}
