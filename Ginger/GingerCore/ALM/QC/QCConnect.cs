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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using TDAPIOLELib;

namespace GingerCore.ALM.QC
{
    public static class QCConnect
    {
        public static string ServerURL { get; set; }

        //To Do: Swapna
        static TDConnection mTDConn = new TDConnection();

        public static TDConnection TDConn
        {
            get { return mTDConn; }
        }

        public static Boolean IsServerConnected
        {
            get
            {
                try
                {
                    if (mTDConn != null)
                        if (mTDConn.Connected)
                            if (mTDConn.LoggedIn)
                                return true;

                    return false;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    return false;
                }
            }
        }

        public static Boolean IsProjectConnected
        {
            get
            {
                try
                {
                    if (IsServerConnected)
                        if (mTDConn.ProjectConnected)
                            return true;

                    return false;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                    return false;
                }
            }
        }

        public static string CurrentDomain
        {
            get
            {
                if (mTDConn != null)
                    return mTDConn.DomainName;
                else
                    return null;
            }
        }

        public static string CurrentProject
        {
            get
            {
                if (mTDConn != null)
                    return mTDConn.ProjectName;
                else
                    return null;
            }
        }

        // Opening QC Connection 
        public static bool ConnectQCServer(string QCServerUrl, string QCUserName, string QCPassword)
        {
            bool qcConn = false;
            DisconnectQCServer();
            mTDConn.InitConnectionEx(QCServerUrl);
            mTDConn.Login(QCUserName, QCPassword);
            qcConn = true;

            return qcConn;
        }

        public static void DisconnectQCServer()
        {
            if (IsProjectConnected)
                mTDConn.DisconnectProject();
            if (IsServerConnected)
                mTDConn.Logout();
            if (mTDConn != null && mTDConn.Connected)
                mTDConn.Disconnect();
        }

        // QC Login Function      
        public static bool ConnectQCProject(string QCDomain, string QCProject)
        {
            try
            {
                mTDConn.Connect(QCDomain, QCProject);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return false;
            }
            if (mTDConn.Connected == true)
            {
                return true;
            }
            else //testTDConn.Disconnect(); 
                return false;
        }

        public static bool ConnectQCProject(string QCServerUrl, string QCUserName, string QCPassword, string QCDomain, string QCProject)
        {
            if (string.IsNullOrEmpty(QCServerUrl) == false &&
                string.IsNullOrEmpty(QCUserName) == false &&
                string.IsNullOrEmpty(QCPassword) == false &&
                string.IsNullOrEmpty(QCDomain) == false &&
                string.IsNullOrEmpty(QCProject) == false)
            {
                if (ConnectQCServer(QCServerUrl, QCUserName, QCPassword))
                    return ConnectQCProject(QCDomain, QCProject);
            }
            return false;
        }

        public static bool DisconnectQCProject()
        {
            try
            {
                if (IsProjectConnected)
                {
                    mTDConn.DisconnectProject();
                }
                return true;
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.ALMOperationFailed, "disconnect from project", e.Message);
                return false;
            }
        }

        public static bool DisconnectQCProjectStayLoggedIn()
        {
            try
            {
                if (IsProjectConnected)
                {
                    mTDConn.DisconnectProject();
                    mTDConn.Login(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword);
                }
                return true;
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.ALMOperationFailed, "disconnect from project", e.Message);
                return false;
            }
        }

        public static List<string> GetQCDomains()
        {
            List<string> QCDomains = new List<string>();

            foreach (dynamic Dom in mTDConn.VisibleDomains)
            {
                QCDomains.Add(Dom.ToString());

            } //add validation point

            return QCDomains;
        }

        public static List<string> GetQCDomainProjects(string domainName)
        {
            dynamic lstProjects = mTDConn.VisibleProjects[domainName];
            List<string> strProjects = new List<string>();
            foreach (dynamic project in lstProjects)
            {
                strProjects.Add(project.ToString());
            }
            return strProjects;
        }

        //get test plan explorer(tree view)
        public static List<string> GetTestPlanExplorer(string PathNode)
        {
            TreeManager treeM = (TreeManager)mTDConn.TreeManager;

            SubjectNode SubjRoot = treeM.get_NodeByPath(PathNode);
            List SubjectNodeList = SubjRoot.NewList();
            List<string> testPlanPathList = new List<string>();

            foreach (SubjectNode oSubjectNode in SubjectNodeList)
            {
                testPlanPathList.Add(oSubjectNode.Name);
            }

            return testPlanPathList;
        }

        //get test lab explorer(tree view)
        public static List<string> GetTestLabExplorer(string PathNode)
        {
            TestSetTreeManager treeM = (TestSetTreeManager)mTDConn.TestSetTreeManager;
            TestSetFolder tsFolder = treeM.get_NodeByPath(PathNode);
            if (tsFolder == null && PathNode.ToUpper() == "ROOT")
                tsFolder = treeM.Root;

            List FoldersList = tsFolder.NewList();
            List<string> testlabPathList = new List<string>();

            foreach (TestSetFolder folder in FoldersList)
            {
                testlabPathList.Add(folder.Name);
            }

            return testlabPathList;
        }

        //get test set explorer(tree view)
        public static IEnumerable<Object> GetTestSetExplorer(string PathNode)
        {
            TestSetTreeManager treeM = (TestSetTreeManager)mTDConn.TestSetTreeManager;
            TestSetFolder tsFolder = treeM.get_NodeByPath(PathNode);
            if (tsFolder == null && PathNode.ToUpper() == "ROOT")
                tsFolder = treeM.Root;

            TestSetFactory TSetFact = mTDConn.TestSetFactory;
            TDFilter tsFilter = TSetFact.Filter;

            try
            {
                tsFilter["CY_FOLDER_ID"] = "" + tsFolder.NodeID + "";
            }
            catch (Exception e)
            {
                tsFilter["CY_FOLDER_ID"] = "\"" + tsFolder.Path.ToString() + "\"";
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
            }

            List TestsetList = TSetFact.NewList(tsFilter.Text);
            List<QCTestSetSummary> testlabPathList = new List<QCTestSetSummary>();
            foreach (TestSet testset in TestsetList)
            {
                QCTestSetSummary QCTestSetTreeItem = new QCTestSetSummary();
                QCTestSetTreeItem.TestSetID = testset.ID;
                QCTestSetTreeItem.TestSetName = testset.Name;
                testlabPathList.Add(QCTestSetTreeItem);
            }

            return testlabPathList;
        }

        public static Object GetTSRunStatus(dynamic TSItem)
        {
            TestSetFactory TSetFact = mTDConn.TestSetFactory;
            TDFilter tsFilter = TSetFact.Filter;
            tsFilter["CY_CYCLE_ID"] = "" + TSItem.TestSetID + "";
            List Testset = TSetFact.NewList(tsFilter.Text);

            foreach (TestSet testset in Testset)
            {
                if (testset.Name == TSItem.TestSetName)
                {
                    TSTestFactory TSTestFact = testset.TSTestFactory;
                    TDFilter tsTestFilter = TSetFact.Filter;
                    tsTestFilter["TC_CYCLE_ID"] = "" + TSItem.TestSetID + "";
                    List TSActivities = TSTestFact.NewList(tsTestFilter.Text);
                    foreach (TSTest tst in TSActivities)
                    {
                        bool existing = false;
                        foreach (string[] status in TSItem.TestSetStatuses)
                        {
                            if (status[0] == tst.Status)
                            {
                                existing = true;
                                status[1] = (Int32.Parse(status[1]) + 1).ToString();
                            }
                        }
                        if (!existing) { TSItem.TestSetStatuses.Add(new string[] { tst.Status, "1" }); }
                    }
                }
            }
            return TSItem;
        }
    }
}