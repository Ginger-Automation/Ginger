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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static GingerCore.Actions.ActDBValidation;

namespace GingerCore.NoSqlBase
{
    public class GingerCosmos : NoSqlBase
    {
        ActDBValidation Act = null;
        public GingerCosmos(eDBValidationType DBValidationtype, Environments.Database mDB, ActDBValidation mact)
        {
            Action = DBValidationtype;
            this.Db = mDB;
            Act = mact;
        }

        public GingerCosmos()
        {
        }

        private string ActualConnectionString
        {
            get
            {
                int lastIndexOf = Db.ConnectionString.LastIndexOf(';');
                string[] strArray = Db.ConnectionString.Split(';');
                string connectionString = strArray[0] + ";" + strArray[1];
                return connectionString;
            }
        }

        private string DatabaseName
        {
            get
            {
                int lastIndexOf = Db.ConnectionString.LastIndexOf(';');
                string[] strArray = Db.ConnectionString.Split(';');
                string dbString = strArray[2];
                return dbString.Split('=')[1];
            }
        }

        private CosmosClient GetCosmosClient()
        {
            return new CosmosClient(ActualConnectionString, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway, ApplicationName = "GingerCosmos" });
        }
        public override bool Connect()
        {
            using (CosmosClient cosmosClient = GetCosmosClient())
            {
                try
                {
                    AccountProperties response = cosmosClient.ReadAccountAsync().Result;
                    if (response == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to connect to cosmos db", ex);
                    return false;
                }
            }
        }

        public override List<string> GetColumnList(string table)
        {
            throw new NotImplementedException("Cosmos does not support columns");
        }

        public override List<string> GetKeyspaceList()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTableList(string Keyspace)
        {
            List<string> colList = new List<string>();
            Database objDatabase = GetDatabaseObject(DatabaseName);
            FeedIterator<object> queryObj = objDatabase.GetContainerQueryIterator<object>();
            if (queryObj.HasMoreResults)
            {
                FeedResponse<object> lstContainer = queryObj.ReadNextAsync().Result;
                foreach (object container in lstContainer)
                {
                    JObject parsed = JObject.Parse(container.ToString());
                    string id = parsed.SelectToken("id").Value<string>();
                    colList.Add(id);
                }
            }
            return colList;
        }

        public override bool MakeSureConnectionIsOpen()
        {
            return Connect();
        }

        public override void PerformDBAction()
        {
            try
            {
                ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList);
                VE.Value = Act.SQL;
                string SQLCalculated = VE.ValueCalculated.ToLower();
                string dbName = "";
                string containerName = "";
                Action = Act.DBValidationType;
                if (Action == eDBValidationType.SimpleSQLOneValue || Action == eDBValidationType.UpdateDB
                    || Action == eDBValidationType.Insert)
                {
                    dbName = DatabaseName;
                    containerName = Act.Table;
                }
                switch (Action)
                {
                    case eDBValidationType.FreeSQL:
                        dbName = DatabaseName;
                        if (!SQLCalculated.Contains("where"))
                        {
                            string[] chArray = VE.ValueCalculated.Split(' ');
                            int idxFrom = 0;
                            for (int i = 0; i < chArray.Length; i++)
                            {
                                if (chArray[i].Equals("from", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    idxFrom = i + 1;
                                    break;
                                }
                            }
                            containerName = chArray[idxFrom];
                            if (containerName.Contains("."))
                            {
                                containerName = containerName.Split('.')[0];
                            }
                        }
                        else
                        {
                            int indexOfFrom = SQLCalculated.IndexOf("from") + 4;
                            int indexOfWhere = SQLCalculated.IndexOf("where");
                            containerName = SQLCalculated.Substring(indexOfFrom, indexOfWhere - indexOfFrom).Trim();
                            containerName = VE.ValueCalculated.Substring(VE.ValueCalculated.IndexOf(containerName, StringComparison.CurrentCultureIgnoreCase)
                                , containerName.Length).Split(' ')[0];
                        }
                        Container container = GetContainer(dbName, containerName);
                        SetOutputFromApiResponse(container, VE.ValueCalculated);
                        break;
                    case eDBValidationType.SimpleSQLOneValue:
                        Container objContainer = GetContainer(dbName, containerName);
                        SQLCalculated = "select * from " + containerName;
                        if (!string.IsNullOrEmpty(Act.Where))
                        {
                            SQLCalculated += " where " + Act.Where;
                        }
                        SetOutputFromApiResponse(objContainer, SQLCalculated);
                        break;
                    case eDBValidationType.RecordCount:
                        dbName = DatabaseName;
                        SQLCalculated = "select count(1) from " + SQLCalculated;
                        string properSql = "select count(1) from " + VE.ValueCalculated;
                        if (!SQLCalculated.Contains("where"))
                        {
                            string[] chArray = VE.ValueCalculated.Split(' ');
                            int idxFrom = 0;
                            for (int i = 0; i < chArray.Length; i++)
                            {
                                if (chArray[i].Equals("from", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    idxFrom = i + 1;
                                    break;
                                }
                            }
                            containerName = chArray[idxFrom];
                        }
                        else
                        {
                            int indexOfFrom = SQLCalculated.IndexOf("from") + 4;
                            int indexOfWhere = SQLCalculated.IndexOf("where");
                            containerName = SQLCalculated.Substring(indexOfFrom, indexOfWhere - indexOfFrom).Trim();
                            containerName = properSql.Substring(properSql.IndexOf(containerName, StringComparison.CurrentCultureIgnoreCase)
                                , containerName.Length).Split(' ')[0];
                        }
                        Container recordContainer = GetContainer(dbName, containerName);
                        SetOutputFromApiResponse(recordContainer, properSql);
                        break;
                    case eDBValidationType.UpdateDB:
                        Container objRecordContainer = GetContainer(DatabaseName, containerName);
                        string primaryKey = Act.GetInputParamCalculatedValue(nameof(Act.PrimaryKey));
                        string partitionKey = Act.GetInputParamCalculatedValue(nameof(Act.PartitionKey));
                        if (string.IsNullOrEmpty(primaryKey))
                        {
                            Act.Error = "Primary Key cannot be empty";
                            return;
                        }
                        if (string.IsNullOrEmpty(partitionKey))
                        {
                            Act.Error = "Partition Key cannot be empty";
                            return;
                        }
                        if (Act.UpdateOperationInputValues == null || Act.UpdateOperationInputValues.Count == 0)
                        {
                            Act.Error = "Please provide fields to be modified";
                            return;
                        }
                        if (objRecordContainer == null)
                        {
                            Act.Error = "Please select valid container/table";
                            return;
                        }
                        List<PatchOperation> lstPatchOperations = new List<PatchOperation>();
                        foreach (ActInputValue cosmosPatch in Act.UpdateOperationInputValues)
                        {
                            string param, value;
                            VE.Value = cosmosPatch.Param;
                            param = VE.ValueCalculated;
                            VE.Value = cosmosPatch.Value;
                            value = VE.ValueCalculated;
                            lstPatchOperations.Add(PatchOperation.Replace(param, value));
                        }
                        IReadOnlyList<PatchOperation> enumerablePatchOps = lstPatchOperations;
                        ItemResponse<object> response = objRecordContainer.PatchItemAsync<object>(id: primaryKey, partitionKey: new PartitionKey(partitionKey), patchOperations: enumerablePatchOps
                            , null, default).Result;
                        if (response != null && response.Resource != null)
                        {
                            object outputVals = response.Resource;
                            JObject parsed = JObject.Parse(outputVals.ToString());
                            string key = parsed.GetValue("id").ToString();
                            Dictionary<string, object> dctOutputVals = new Dictionary<string, object>();
                            dctOutputVals.Add(key, outputVals);
                            Act.AddToOutputValues(dctOutputVals);
                        }
                        break;
                    case eDBValidationType.Insert:
                        Container objContainerForInsert = GetContainer(dbName, containerName);
                        string insertJson = Act.GetInputParamCalculatedValue("InsertJson");
                        if (string.IsNullOrEmpty(insertJson))
                        {
                            Act.Error = "JSON cannot be empty";
                            return;
                        }
                        JObject jsonObject = JObject.Parse(insertJson);
                        ItemResponse<object> objReturn = objContainerForInsert.CreateItemAsync<object>(jsonObject).Result;

                        if (objReturn != null && objReturn.Resource != null)
                        {
                            object outputVals = objReturn.Resource;
                            JObject parsed = JObject.Parse(outputVals.ToString());
                            string key = parsed.GetValue("id").ToString();
                            Dictionary<string, object> dctOutputVals = new Dictionary<string, object>();
                            dctOutputVals.Add(key, outputVals);
                            Act.AddToOutputValues(dctOutputVals);
                        }
                        break;
                    default:
                        //do nothing
                        break;
                }
            }
            catch (Exception ex)
            {
                Act.Error = ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }

        private JObject GetObjectFromCosmosDb(string Database, string Container, string id, string partitionKey)
        {
            try
            {
                Container objContainer = GetContainer(Database, Container);
                ItemResponse<object> getObjectFromCosmos = objContainer.ReadItemAsync<object>(id, new PartitionKey(partitionKey)).Result;
                if (getObjectFromCosmos != null && getObjectFromCosmos.Resource != null)
                {
                    return JObject.Parse(getObjectFromCosmos.Resource.ToString());
                }
                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Error in getting cosmos object from {0} db {1} container with {2} id and {3} partition key", Database, Container, id, partitionKey), ex);
            }
            return null;
        }

        private void SetOutputFromApiResponse(Container objContainer, string sqlCalculated)
        {
            try
            {
                FeedResponse<object> currentResultSet = null;
                Dictionary<string, object> outputVals = new Dictionary<string, object>();
                FeedIterator<object> queryResultSetIterator = null;
                queryResultSetIterator = objContainer.GetItemQueryIterator<object>(sqlCalculated);
                while (queryResultSetIterator.HasMoreResults)
                {
                    currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                    int i = 1;
                    foreach (object response in currentResultSet)
                    {
                        Act.ParseJSONToOutputValues(response.ToString(), i);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                if (!GetTableList(string.Empty).Contains(objContainer.Id))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Container name is invalid", null);
                    Act.Error = "Container name is invalid";
                }
                else
                {
                    Act.Error = ex.Message;
                }
            }
        }

        private List<string> GetListOfDatabaseId()
        {
            List<string> lstDb = new List<string>();
            try
            {
                CosmosClient cosmosClient = GetCosmosClient();
                var listOfDb = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
                if (listOfDb.HasMoreResults)
                {
                    FeedResponse<DatabaseProperties> databaseProperties = listOfDb.ReadNextAsync().Result;
                    foreach (DatabaseProperties dbProp in databaseProperties)
                    {
                        lstDb.Add(dbProp.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return lstDb;
        }

        private List<Database> GetListOfDatabases()
        {
            List<Database> lstDb = new List<Database>();
            try
            {
                CosmosClient cosmosClient = GetCosmosClient();
                var listOfDb = GetListOfDatabaseId();
                if (listOfDb != null && listOfDb.Count > 0)
                {
                    foreach (string dbPropId in listOfDb)
                    {
                        Database objDb = cosmosClient.GetDatabase(dbPropId);
                        lstDb.Add(objDb);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return lstDb;
        }

        private Database GetDatabaseObject(string databaseId)
        {
            CosmosClient cosmosClient = GetCosmosClient();
            try
            {
                Database objDb = cosmosClient.GetDatabase(databaseId);
                if (objDb == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Unable to find Cosmos Db with id: " + databaseId);
                    return null;
                }
                else
                {
                    return objDb;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in getting database " + ex.Message, ex);
                return null;
            }
        }

        private Container GetContainer(string dbName, string containerId)
        {
            try
            {
                Database db = GetDatabaseObject(dbName);
                Container container = db.GetContainer(containerId);
                return container;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error getting container " + containerId + " for DB: " + dbName, ex);
                return null;
            }
        }

    }
}
