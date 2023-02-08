#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public LiteDbManager(String dbFolderName = "")
        {
            dbConnector = new LiteDbConnector(Path.Combine(dbFolderName, "GingerExecutionResults.db")); // Set data to ExecutionResults folder name
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

            activityMapper.DbRef(rf => rf.ActionsColl, NameInDb<LiteDbAction>());

            agMapper.DbRef(rf => rf.ActivitiesColl, NameInDb<LiteDbActivity>());

            bfMapper.DbRef(rf => rf.ActivitiesGroupsColl, NameInDb<LiteDbActivityGroup>());

            bfMapper.DbRef(rf => rf.ActivitiesColl, NameInDb<LiteDbActivity>());

            runnerMapper.DbRef(rf => rf.BusinessFlowsColl, NameInDb<LiteDbBusinessFlow>());

            runSetMapper.DbRef(rf => rf.RunnersColl, NameInDb<LiteDbRunner>());
        }
        public string NameInDb<T>()
        {
            var name = typeof(T).Name + "s";
            return name;
        }

        public LiteDbRunSet GetLatestExecutionRunsetData(string runsetId)
        {
            var result = GetRunSetLiteData();
            List<LiteDbRunSet> filterData = null;
            if (!string.IsNullOrEmpty(runsetId))
            {
                filterData = result.IncludeAll().Find(a => a._id.ToString() == runsetId).ToList();
            }
            else
            {
                runsetId = result.IncludeAll().Max(x => x._id).AsString;
                filterData = result.IncludeAll().Find(a => a._id.ToString() == runsetId).ToList();
            }
            return filterData.Last();
        }

        public List<T> FilterCollection<T>(LiteCollection<T> baseColl, Query query)
        {
            return dbConnector.FilterCollection(baseColl, query);
        }

        public bool DeleteDocumentByLiteDbRunSet(LiteDbRunSet liteDbRunSet)
        {
            return dbConnector.DeleteDocumentByLiteDbRunSet(liteDbRunSet);            
        }

        public LiteCollection<LiteDbReportBase> GetObjectLiteData(string reportLevelName)
        {
            return dbConnector.GetCollection<LiteDbReportBase>(reportLevelName);
        }
        public LiteCollection<LiteDbRunSet> GetRunSetLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());
        }
        public LiteCollection<LiteDbRunner> GetRunnerLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunner>(NameInDb<LiteDbRunner>());
        }
        public LiteCollection<LiteDbBusinessFlow> GetBfLiteData()
        {
            return dbConnector.GetCollection<LiteDbBusinessFlow>(NameInDb<LiteDbBusinessFlow>());
        }

        private LiteCollection<LiteDbActivityGroup> GetActGrLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivityGroup>(NameInDb<LiteDbActivityGroup>());
        }
        public LiteCollection<LiteDbActivity> GetActivitiesLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivity>(NameInDb<LiteDbActivity>());
        }
        public LiteCollection<LiteDbAction> GetActionsLiteData()
        {
            return dbConnector.GetCollection<LiteDbAction>(NameInDb<LiteDbAction>());
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
                item.RunnersColl = runnersData;
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
                item.BusinessFlowsColl = bfsData;
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
                item.ActivitiesGroupsColl = acGrpData;
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
                item.ActionsColl = actionsColl;
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
