using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.NonUITests
{
    class DataSourceTest
    {
        DataSourceBase accessDataSource = new AccessDataSource();
        ObservableList<DataSourceTable> sourceTables;
        ObservableList<DataSourceBase> dataSourceBases;
        ProjEnvironment environment = new ProjEnvironment();
        GingerRunner mGR;
        BusinessFlow mBF;

        [TestInitialize]
        public void TestInitialize()
        {
            accessDataSource.FilePath = @"C:/GingerSourceControl/Solutions/Ginger_Regression_Testing1/DataSources/GingerDataSource.mdb";
            accessDataSource.Init(accessDataSource.FilePath);

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<GingerCore.Activity>();
            mBF.Name = "BF Status Result Test";
            mBF.Active = true;

            Platform p = new Platform();
            p.PlatformType = ePlatformType.NA;
            mBF.Platforms = new ObservableList<Platform>();
            //mBF.Platforms.Add(p);
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.NA;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            mGR.ProjEnvironment = environment;

            mGR.DSList = dataSourceBases;


            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.BusinessFlows.Add(mBF);
        }

        [TestMethod]
        public void GetTableList()
        {
            sourceTables = accessDataSource.GetTablesList();
            Assert.AreEqual(sourceTables.Count, 5);
        }

        [TestMethod]
        public void AddTable()
        {
            accessDataSource.AddTable("NewTableAs", "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");
            Assert.AreEqual(accessDataSource.IsTableExist("NewTableAs"), true);
        }

        [TestMethod]
        public void AddAndDeleteColunmfromTable()
        {
            List<string> ColunmList;
            bool IsColunmExists = false;
            accessDataSource.AddColumn("NewTableAs", "New", "Text");
            ColunmList = accessDataSource.GetColumnList("NewTableAs");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            Assert.AreEqual(IsColunmExists, true);

            accessDataSource.RemoveColumn("NewTableAs", "New");
            ColunmList = accessDataSource.GetColumnList("NewTableAs");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            Assert.AreEqual(IsColunmExists, false);

        }


        [TestMethod]
        public void DataSourceActionRowCount()
        {

            Activity a1 = new GingerCore.Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            ActDSTableElement actDSTableElement = new ActDSTableElement();
            actDSTableElement.DSName = "GingerDataSource";
            actDSTableElement.DSTableName = "KEY-TABLE";
            actDSTableElement.ControlAction = ActDSTableElement.eControlAction.RowCount;
            actDSTableElement.Active = true;

            a1.Acts.Add(actDSTableElement);
            sourceTables = accessDataSource.GetTablesList();
            //mGR.DSList.Add(accessDataSource);

            mGR.RunRunner();

            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);

        }

        [TestMethod]
        public void DataSourceActionGetValue()
        {

            Activity a1 = new Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            ActDSTableElement actDSTableElement = new ActDSTableElement();
            actDSTableElement.DSName = "GingerDataSource";
            actDSTableElement.DSTableName = "KEY-DATA";
            actDSTableElement.ControlAction = ActDSTableElement.eControlAction.GetValue;
            actDSTableElement.Active = true;

            a1.Acts.Add(actDSTableElement);
            sourceTables = accessDataSource.GetTablesList();
            //mGR.DSList.Add(accessDataSource);

            mGR.RunRunner();

            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);

        }

        [TestMethod]
        public void DeleteTable()
        {
            accessDataSource.DeleteTable("NewTableAs");
            Assert.AreEqual(accessDataSource.IsTableExist("NewTableAs"), false);
        }
    }
}

