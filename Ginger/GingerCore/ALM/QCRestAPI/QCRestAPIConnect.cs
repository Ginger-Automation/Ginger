using ALMRestClient;
using GingerCore.ALM.QC;
using QCRestClient;
using System;
using System.Collections.Generic;
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
        public static QCRestClient.QCClient QcRestClient { get; set; }

        public static Dictionary<string, string> ExploredTestLabFolder = new Dictionary<string, string>();
        public static Dictionary<string, string> ExploredTestSets = new Dictionary<string, string>();
        public static Dictionary<string, string> ExploredTestPlanFolder = new Dictionary<string, string>();

        public static bool ConnectQCServer(string qcServerUrl, string qcUserName, string qcPassword)
        {
            if (QcRestClient == null)
                QcRestClient = new QCClient(qcServerUrl, qcUserName, qcPassword);

            ServerURL = qcServerUrl;
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
                        if (QcRestClient.IsAuthenticated())
                            return true;
                        else
                            if (QcRestClient.Login())
                            return true;

                    return false;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    return false;
                }
            }
        }

        public static void DisconnectQCServer()
        {
            if (QcRestClient == null)
                return;

            QcRestClient.Logout();
        }

        public static bool DisconnectQCProjectStayLoggedIn()
        {
            CurrentDomain = null;
            CurrentProject = null;

            if (QcRestClient == null)
                QcRestClient = new QCClient(ServerURL, UserName, Password);
            if (!QcRestClient.IsAuthenticated())
                return QcRestClient.Login();

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

            QcRestClient = new QCRestClient.QCClient(ServerURL, UserName, Password, qcDomain, qcProject);
            return QcRestClient.Login();
        }

        public static List<string> GetQCDomains()
        {
            return QcRestClient.getDomainsList();
        }

        public static List<string> GetQCDomainProjects(string domainName)
        {
            return QcRestClient.getProjectsList(domainName);
        }

        public static int GetLastTestPlanIdFromPath(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testPlanPathList = new List<string>();

            separatePath[0] = ExploredTestPlanFolder.ContainsKey("Subject") ? ExploredTestPlanFolder["Subject"] : QcRestClient.GetTestPlanRootFolder().Id;

            if (!ExploredTestPlanFolder.ContainsKey("Subject"))
                ExploredTestPlanFolder.Add("Subject", separatePath[0]);

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestPlanFolderId(separatePath[i], separatePath[i - 1]);
            }

            return int.Parse(separatePath.Last());
        }

        public static int GetLastTestSetIdFromPath(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testSetPathList = new List<string>();

            separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

            if (!ExploredTestLabFolder.ContainsKey("Root"))
                ExploredTestLabFolder.Add("Root", separatePath[0]);

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
            }

            return int.Parse(separatePath.Last());
        }

        //get test plan explorer(tree view)
        public static List<string> GetTestPlanExplorer(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testPlanPathList = new List<string>();

            separatePath[0] = ExploredTestPlanFolder.ContainsKey("Subject") ? ExploredTestPlanFolder["Subject"] : QcRestClient.GetTestPlanRootFolder().Id;

            if (!ExploredTestPlanFolder.ContainsKey("Subject"))
                ExploredTestPlanFolder.Add("Subject", separatePath[0]);

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestPlanFolderId(separatePath[i], separatePath[i - 1]);
            }

            QCTestPlan testPlanToExplor = QcRestClient.GetTestPlanTreeLayerByFilter(separatePath[separatePath.Length - 1], "");

            foreach (QCTestFolder folder in testPlanToExplor.folders)
                testPlanPathList.Add(folder.Name);

            return testPlanPathList;
        }

        public static List<string> GetTestLabExplorer(string PathNode)
        {
            string[] separatePath = PathNode.Split('\\');
            List<string> testlabPathList = new List<string>();

            separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

            if (!ExploredTestLabFolder.ContainsKey("Root"))
                ExploredTestLabFolder.Add("Root", separatePath[0]);

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
            }

            QCTestSetFolderColl foldersToExplor = QcRestClient.GetTestSetTreeLayerByFilter(separatePath[separatePath.Length - 1], "");

            foreach (QCTestSetFolder folders in foldersToExplor)
                testlabPathList.Add(folders.Name);

            return testlabPathList;
        }

        //get test set explorer(tree view)
        public static List<QCTestSetSummary> GetTestSetExplorer(string PathNode)
        {
            List<QCTestSetSummary> testlabPathList = new List<QCTestSetSummary>();
            string[] separatePath = PathNode.Split('\\');

            separatePath[0] = ExploredTestLabFolder.ContainsKey("Root") ? ExploredTestLabFolder["Root"] : QcRestClient.GetTestSetRootFolder().Id;

            if (!ExploredTestLabFolder.ContainsKey("Root"))
                ExploredTestLabFolder.Add("Root", separatePath[0]);

            for (int i = 1; i < separatePath.Length; i++)
            {
                separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
            }

            QCTestSetColl testSets = QcRestClient.GetAllTestSetsUnderFolder(int.Parse(separatePath[separatePath.Length - 1]));

            foreach (QCRestClient.QCTestSet testset in testSets)
            {
                QCTestSetSummary QCTestSetTreeItem = new QCTestSetSummary();
                QCTestSetTreeItem.TestSetID = testset.Id;
                QCTestSetTreeItem.TestSetName = testset.Name;
                testlabPathList.Add(QCTestSetTreeItem);
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

    }
}
