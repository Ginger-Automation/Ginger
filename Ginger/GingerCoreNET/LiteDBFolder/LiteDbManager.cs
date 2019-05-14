using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using LiteDB;

namespace Amdocs.Ginger.CoreNET.LiteDBFolder
{
    public class LiteDbManager
    {
        private LiteDbConnector dbConnector;
        public LiteDbManager()
        {
            string folderName = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder;
            dbConnector = new LiteDbConnector(Path.Combine(folderName,"LiteDbData.db")); // Set data to ExecutionResults folder name
            InitMappers();
        }
        public void InitMappers()
        {
            var runSetMapper = dbConnector.GetMapper<LiteDbRunSet>();
            var runnerMapper = dbConnector.GetMapper<LiteDbRunner>();
            var bfMapper = dbConnector.GetMapper<LiteDbBusinessFlow>();
            var agMapper = dbConnector.GetMapper<LiteDbActivityGroup>();
            var activityMapper = dbConnector.GetMapper<LiteDbActivity>();
            var action = dbConnector.GetMapper<LiteDbAction>();

            activityMapper.DbRef(rf => rf.actionsColl, "actionsColl");

            agMapper.DbRef(rf => rf.ActivitiesColl, "activitiesColl");

            bfMapper.DbRef(rf => rf.ActivitiesGroupColl, "agsColl");

            bfMapper.DbRef(rf => rf.ActivitiesColl, "activitiesColl");

            runnerMapper.DbRef(rf => rf.BusinessFlowColl, "bfsColl");

            runSetMapper.DbRef(rf => rf.RunnerColl, "runnersColl");
        }
        private LiteCollection<LiteDbReportBase> GetObjectLiteData(string reportLevelName)
        {
            return dbConnector.GetCollection<LiteDbReportBase>(reportLevelName);
        }
        private LiteCollection<LiteDbRunSet> GetRunSetLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunSet>("Runset");
        }
        private LiteCollection<LiteDbRunner> GetRunnerLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunner>("Runners");
        }
        private LiteCollection<LiteDbBusinessFlow> GetBfLiteData()
        {
            return dbConnector.GetCollection<LiteDbBusinessFlow>("BusinessFlows");
        }

        private LiteCollection<LiteDbActivityGroup> GetActGrLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivityGroup>("ActivityGroups");
        }
        private LiteCollection<LiteDbActivity> GetActivitiesLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivity>("Activities");
        }
        private LiteCollection<LiteDbAction> GetActionsLiteData()
        {
            return dbConnector.GetCollection<LiteDbAction>("Actions");
        }
        public void WriteToLiteDb()
        {
            var runsetLiteColl = GetRunSetLiteData();
            var runnerLiteColl = GetRunnerLiteData();
            var bfLiteColl = GetBfLiteData();
            var acgLiteColl = GetActGrLiteData();
            var activitiesLiteColl = GetActivitiesLiteData();
            var actionsLiteColl = GetActionsLiteData();

            var actionsColl = this.GetGingerActions();
            var activitiesColl = this.GetGingerActivities(actionsColl);
            var acgColl = this.GetGingerActvityGroup(activitiesColl);
            var bfsColl = this.GetGingerBf(acgColl,activitiesColl);
            var runnersColl = this.GetGingerRunner(bfsColl);
            var runSet = this.GetGingerRunSet(runnersColl);


            dbConnector.SetCollection(runsetLiteColl, runSet);
            dbConnector.SetCollection(runnerLiteColl, runnersColl);
            dbConnector.SetCollection(bfLiteColl, bfsColl);
            dbConnector.SetCollection(acgLiteColl, acgColl);
            dbConnector.SetCollection(activitiesLiteColl, activitiesColl);
            dbConnector.SetCollection(actionsLiteColl, actionsColl);
        }
        public void WriteToLiteDb(string reportLevelName, List<LiteDbReportBase> objectColl)
        {
            var objectLiteColl = GetObjectLiteData(reportLevelName);
            //var objectColl = this.GetGingerObject(reportLevelName, gingerObject);
            dbConnector.SetCollection(objectLiteColl, objectColl);
        }
        private LiteDbReportBase GetGingerObject(String reportLevelName,RepositoryItemBase objectData)
        {
            LiteDbReportBase data = new LiteDbReportBase();
            switch (reportLevelName)
            {
                case "Actions":
                    data = new LiteDbAction();
                    data.SetReportData((Act)objectData);
                    break;
                case "Activities":
                    data = new LiteDbActivity();
                    data.SetReportData((Activity)objectData);
                    break;
                case "ActivityGroups":
                    break;
                case "BusinessFlows":
                    break;
                case "Runners":
                    break;
                case "RunSet":
                    break;
                default:
                    break;
            }
            return data;
        }
        private List<LiteDbRunSet> GetGingerRunSet(List<LiteDbRunner> runnersData)
        {
            List<LiteDbRunSet> data = new List<LiteDbRunSet>();
            for (var a = 0; a < 2; a++)
            {
                LiteDbRunSet item = new LiteDbRunSet();
                item.Seq = a;
                item.GUID = Guid.NewGuid();
                item.Name = ("RunSet_name");
                item.Description = ("RunSet_description");
                item.StartTimeStamp = DateTime.Now;
                item.EndTimeStamp = DateTime.Today;
                item.GingerVersion = "66";
                item.MachineName = "my machine";
                item.ExecutedbyUser = "my name";
                item.RunnerColl = runnersData;
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbRunner> GetGingerRunner(List<LiteDbBusinessFlow> bfsData)
        {
            List<LiteDbRunner> data = new List<LiteDbRunner>();
            for (var a = 0; a < 3; a++)
            {
                LiteDbRunner item = new LiteDbRunner();
                item.Seq = 1;
                item.GUID = Guid.NewGuid();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.RunStatus = "run";
                item.BusinessFlowColl = bfsData;
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbBusinessFlow> GetGingerBf(List<LiteDbActivityGroup> acGrpData, List<LiteDbActivity> activitiesColl)
        {
            List<LiteDbBusinessFlow> data = new List<LiteDbBusinessFlow>();
            for (var a = 0; a < 3; a++)
            {
                LiteDbBusinessFlow item = new LiteDbBusinessFlow();
                item.Seq = a;
                item.GUID = Guid.NewGuid();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.StartTimeStamp = DateTime.Now;
                item.EndTimeStamp = DateTime.Today;
                item.Elapsed = 17;
                item.RunStatus = "run";
                item.ActivitiesGroupColl = acGrpData;
                item.ActivitiesColl = activitiesColl;
                data.Add(item);
            }
            return data;

        }
        private List<LiteDbActivityGroup> GetGingerActvityGroup(List<LiteDbActivity> activitiesColl)
        {
            List<LiteDbActivityGroup> data = new List<LiteDbActivityGroup>();
            for (var a = 0; a < 2; a++)
            {
                LiteDbActivityGroup item = new LiteDbActivityGroup();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.GUID = Guid.NewGuid();
                item.RunStatus = "run";
                item.ActivitiesColl = activitiesColl;
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbActivity> GetGingerActivities(List<LiteDbAction>actionsColl)
        {
            List<LiteDbActivity> data = new List<LiteDbActivity>();
            for (var a = 0; a < 3; a++)
            {
                LiteDbActivity item = new LiteDbActivity();
                item.Seq = a;
                item.GUID = Guid.NewGuid();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.StartTimeStamp = DateTime.Now;
                item.EndTimeStamp = DateTime.Today;
                item.Elapsed = 17;
                item.RunStatus = "run";
                item.actionsColl = actionsColl;
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbAction> GetGingerActions()
        {
            List<LiteDbAction> data = new List<LiteDbAction>();
            for (var a = 0; a < 3; a++)
            {
                LiteDbAction item = new LiteDbAction();
                item.Seq = a;
                item.GUID = Guid.NewGuid();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.StartTimeStamp = DateTime.Now;
                item.EndTimeStamp = DateTime.Today;
                item.Elapsed = 17;
                item.RunStatus = "run";
                data.Add(item);
            }
            return data;
        }
    }
}
