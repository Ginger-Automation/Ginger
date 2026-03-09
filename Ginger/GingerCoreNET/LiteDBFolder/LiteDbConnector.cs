#region License
/*
Copyright © 2014-2026 European Support Limited

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
using LiteDB;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.LiteDBFolder
{

    //TODO: manaska: Need to check when LiteDB will appropriately handle multi access of db files.
    //As of now we are manually using Locks to handle multithreading but in the future.
    //Need to make sure that LiteDb handles it internally
    public class LiteDbConnector
    {
        public ConnectionString ConnectionString { get; set; }
        public LiteDbConnector(string filePath)
        {
            ConnectionString = new()
            {
                Filename = filePath,
                Connection = ConnectionType.Shared
            };
            TryUpgradeDataFile();
        }

        private bool TryUpgradeDataFile()
        {
            try
            {
                string dbFilePath = ConnectionString.Filename;
                return LiteEngine.Upgrade(dbFilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public EntityBuilder<T> GetMapper<T>()
        {
            return BsonMapper.Global.Entity<T>();
        }
        public ILiteCollection<T> GetCollection<T>(string collectionName)
        {
            ILiteCollection<T> collection = null;

            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    collection = db.GetCollection<T>(collectionName);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Access denied while trying to get collection: {collectionName}", ex);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to Get Collection: {collectionName}", ex);
            }
            return collection;
        }


        //This function is not used anywhere
        //public bool DeleteCollectionItems<T>(LiteCollection<T> baseColl, Query query)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (var db = new LiteDatabase(this.ConnectionString))
        //        {
        //            result = baseColl.Delete(query) > 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("DeleteCollectionItems Error - " + ex.Message);
        //    }
        //    return result;
        //}

        public bool DeleteDocumentByLiteDbRunSet(LiteDbRunSet liteDbRunSet, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            bool result = true;
            try
            {
                var runSetLiteColl = GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());
                var runnerssLiteColl = GetCollection<LiteDbRunner>(NameInDb<LiteDbRunner>());
                foreach (LiteDbRunner ldbRunner in liteDbRunSet.RunnersColl)
                {
                    var businessFlowsLiteColl = GetCollection<LiteDbBusinessFlow>(NameInDb<LiteDbBusinessFlow>());
                    foreach (LiteDbBusinessFlow ldbBF in ldbRunner.BusinessFlowsColl)
                    {
                        var activitiesLiteColl = GetCollection<LiteDbActivity>(NameInDb<LiteDbActivity>());
                        var activitiesGroupsLiteColl = GetCollection<LiteDbActivityGroup>(NameInDb<LiteDbActivityGroup>());
                        foreach (LiteDbActivityGroup ldbAG in ldbBF.ActivitiesGroupsColl)
                        {
                            activitiesGroupsLiteColl.Delete(ldbAG._id);
                        }
                        foreach (LiteDbActivity ldbActivity in ldbBF.ActivitiesColl)
                        {
                            var actionsLiteColl = GetCollection<LiteDbAction>(NameInDb<LiteDbAction>());
                            foreach (LiteDbAction ldbAction in ldbActivity.ActionsColl)
                            {
                                actionsLiteColl.Delete(ldbAction._id);
                            }
                            activitiesLiteColl.Delete(ldbActivity._id);
                        }
                        businessFlowsLiteColl.Delete(ldbBF._id);
                    }
                    if (executedFrom == eExecutedFrom.Run)
                    {
                        runnerssLiteColl.Delete(ldbRunner._id);
                    }
                }
                if (executedFrom == eExecutedFrom.Run)
                {
                    runSetLiteColl.Delete(liteDbRunSet._id);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to delete document for RunSet: {liteDbRunSet._id}", ex);
                result = false;
            }
            return result;
        }
        public string NameInDb<T>()
        {
            var name = typeof(T).Name + "s";
            return name;
        }
        public List<T> FilterCollection<T>(ILiteCollection<T> baseColl, Query query)
        {
            return baseColl.Find(query).ToList();
        }
        public List<T> FilterCollection<T>(ILiteCollection<T> baseColl, BsonExpression expression)
        {
            return baseColl.Find(expression).ToList();
        }

        public void SetCollection<T>(ILiteCollection<T> baseColl, List<T> updateData)
        {
            try
            {
                foreach (T data in updateData)
                {
                    if (data is LiteDbReportBase baseObj && baseObj._id == null)
                    {
                        baseObj._id = ObjectId.NewObjectId();
                    }
                }
                using var db = new LiteDatabase(this.ConnectionString);
                baseColl.Upsert(updateData);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while attempting to insert data into LiteDB.", ex);
            }
        }

        public void SaveImage(string imagePath, string imageName)
        {
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    db.FileStorage.Upload(imageName, imagePath);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while attempting to save image.", ex);
            }
        }

        public void GetImage(string imagePath)
        {
            try
            {
                using (var db = new LiteDatabase(this.ConnectionString))
                {
                    using (FileStream fs = File.Create(this.ConnectionString.ToString()))
                    {
                        var file = db.FileStorage.Download(imagePath, fs);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while attempting to get image.", ex);
            }
        }
    }
}
