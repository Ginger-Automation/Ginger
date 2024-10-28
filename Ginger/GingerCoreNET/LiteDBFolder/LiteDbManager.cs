#region License
/*
Copyright © 2014-2024 European Support Limited

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

using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <summary>
        /// Initializes the mappers for LiteDB collections.
        /// </summary>
        public void InitMappers()
        {
            InitRunSetMapper();
            InitRunnerMapper();
            InitBusinessFlowMapper();
            InitActivityGroupMapper();
            InitActivityMapper();
        }

        /// <summary>
        /// Initializes the mapper for LiteDbRunSet collection.
        /// </summary>
        private void InitRunSetMapper()
        {
            var runSetMapper = dbConnector.GetMapper<LiteDbRunSet>();
            runSetMapper.DbRef(rf => rf.RunnersColl, NameInDb<LiteDbRunner>());
        }

        /// <summary>
        /// Initializes the mapper for LiteDbRunner collection.
        /// </summary>
        private void InitRunnerMapper()
        {
            var runnerMapper = dbConnector.GetMapper<LiteDbRunner>();
            runnerMapper.DbRef(rf => rf.BusinessFlowsColl, NameInDb<LiteDbBusinessFlow>());
            runnerMapper.DbRef(rf => rf.AllBusinessFlowsColl, NameInDb<LiteDbBusinessFlow>());
        }

        /// <summary>
        /// Initializes the mapper for LiteDbBusinessFlow collection.
        /// </summary>
        private void InitBusinessFlowMapper()
        {
            var bfMapper = dbConnector.GetMapper<LiteDbBusinessFlow>();
            bfMapper.DbRef(rf => rf.ActivitiesGroupsColl, NameInDb<LiteDbActivityGroup>());
            bfMapper.DbRef(rf => rf.ActivitiesColl, NameInDb<LiteDbActivity>());
            bfMapper.DbRef(rf => rf.AllActivitiesColl, NameInDb<LiteDbActivity>());
        }

        /// <summary>
        /// Initializes the mapper for LiteDbActivityGroup collection.
        /// </summary>
        private void InitActivityGroupMapper()
        {
            var agMapper = dbConnector.GetMapper<LiteDbActivityGroup>();
            agMapper.DbRef(rf => rf.ActivitiesColl, NameInDb<LiteDbActivity>());
            agMapper.DbRef(rf => rf.AllActivitiesColl, NameInDb<LiteDbActivity>());
        }

        /// <summary>
        /// Initializes the mapper for LiteDbActivity collection.
        /// </summary>
        private void InitActivityMapper()
        {
            var activityMapper = dbConnector.GetMapper<LiteDbActivity>();
            activityMapper.DbRef(rf => rf.ActionsColl, NameInDb<LiteDbAction>());
            activityMapper.DbRef(rf => rf.AllActionsColl, NameInDb<LiteDbAction>());
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
                filterData = LiteDbRunSet.IncludeAllReferences(result).Find(a => a._id.Equals(new ObjectId(runsetId))).ToList();
            }
            else
            {
                ObjectId runsetObjectID = LiteDbRunSet.IncludeAllReferences(result).Max(x => x._id);
                filterData = LiteDbRunSet.IncludeAllReferences(result).Find(a => a._id.Equals(runsetObjectID)).ToList();
            }
            return filterData.Last();
        }

        public List<T> FilterCollection<T>(ILiteCollection<T> baseColl, Query query)
        {
            return dbConnector.FilterCollection(baseColl, query);
        }

        public bool DeleteDocumentByLiteDbRunSet(LiteDbRunSet liteDbRunSet)
        {
            return dbConnector.DeleteDocumentByLiteDbRunSet(liteDbRunSet);
        }

        public ILiteCollection<LiteDbReportBase> GetObjectLiteData(string reportLevelName)
        {
            return dbConnector.GetCollection<LiteDbReportBase>(reportLevelName);
        }
        public ILiteCollection<LiteDbRunSet> GetRunSetLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());
        }
        public ILiteCollection<LiteDbRunner> GetRunnerLiteData()
        {
            return dbConnector.GetCollection<LiteDbRunner>(NameInDb<LiteDbRunner>());
        }
        public ILiteCollection<LiteDbBusinessFlow> GetBfLiteData()
        {
            return dbConnector.GetCollection<LiteDbBusinessFlow>(NameInDb<LiteDbBusinessFlow>());
        }

        private ILiteCollection<LiteDbActivityGroup> GetActGrLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivityGroup>(NameInDb<LiteDbActivityGroup>());
        }
        public ILiteCollection<LiteDbActivity> GetActivitiesLiteData()
        {
            return dbConnector.GetCollection<LiteDbActivity>(NameInDb<LiteDbActivity>());
        }
        public ILiteCollection<LiteDbAction> GetActionsLiteData()
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
            var bfsColl = this.GetGingerBf(acgColl, activitiesColl);
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
            List<LiteDbRunSet> data = [];
            for (var a = 0; a < 2; a++)
            {
                LiteDbRunSet item = new LiteDbRunSet
                {
                    Seq = a,
                    GUID = Guid.NewGuid(),
                    Name = ("RunSet_name"),
                    Description = ("RunSet_description"),
                    StartTimeStamp = DateTime.Now,
                    EndTimeStamp = DateTime.Today,
                    GingerVersion = "66",
                    MachineName = "my machine",
                    ExecutedbyUser = "my name",
                    RunnersColl = runnersData
                };
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbRunner> GetGingerRunner(List<LiteDbBusinessFlow> bfsData)
        {
            List<LiteDbRunner> data = [];
            for (var a = 0; a < 3; a++)
            {
                LiteDbRunner item = new LiteDbRunner
                {
                    Seq = 1,
                    GUID = Guid.NewGuid(),
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    RunStatus = "run",
                    AllBusinessFlowsColl = bfsData
                };
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbBusinessFlow> GetGingerBf(List<LiteDbActivityGroup> acGrpData, List<LiteDbActivity> activitiesColl)
        {
            List<LiteDbBusinessFlow> data = [];
            for (var a = 0; a < 3; a++)
            {
                LiteDbBusinessFlow item = new LiteDbBusinessFlow
                {
                    Seq = a,
                    GUID = Guid.NewGuid(),
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    StartTimeStamp = DateTime.Now,
                    EndTimeStamp = DateTime.Today,
                    Elapsed = 17,
                    RunStatus = "run",
                    ActivitiesGroupsColl = acGrpData,
                    AllActivitiesColl = activitiesColl
                };
                data.Add(item);
            }
            return data;

        }
        private List<LiteDbActivityGroup> GetGingerActvityGroup(List<LiteDbActivity> activitiesColl)
        {
            List<LiteDbActivityGroup> data = [];
            for (var a = 0; a < 2; a++)
            {
                LiteDbActivityGroup item = new LiteDbActivityGroup
                {
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    GUID = Guid.NewGuid(),
                    RunStatus = "run",
                    AllActivitiesColl = activitiesColl
                };
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbActivity> GetGingerActivities(List<LiteDbAction> actionsColl)
        {
            List<LiteDbActivity> data = [];
            for (var a = 0; a < 3; a++)
            {
                LiteDbActivity item = new LiteDbActivity
                {
                    Seq = a,
                    GUID = Guid.NewGuid(),
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    StartTimeStamp = DateTime.Now,
                    EndTimeStamp = DateTime.Today,
                    Elapsed = 17,
                    RunStatus = "run",
                    AllActionsColl = actionsColl
                };
                data.Add(item);
            }
            return data;
        }
        private List<LiteDbAction> GetGingerActions()
        {
            List<LiteDbAction> data = [];
            for (var a = 0; a < 3; a++)
            {
                LiteDbAction item = new LiteDbAction
                {
                    Seq = a,
                    GUID = Guid.NewGuid(),
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    StartTimeStamp = DateTime.Now,
                    EndTimeStamp = DateTime.Today,
                    Elapsed = 17,
                    RunStatus = "run"
                };
                data.Add(item);
            }
            return data;
        }
    }
}
