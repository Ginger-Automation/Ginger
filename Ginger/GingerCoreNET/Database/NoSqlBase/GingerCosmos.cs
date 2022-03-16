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
                return Db.ConnectionString.Substring(0, lastIndexOf);
            }
        }

        private string DatabaseName
        {
            get
            {
                int lastIndexOf = Db.ConnectionString.LastIndexOf(';');
                string dbString = Db.ConnectionString.Substring(lastIndexOf + 1);
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
            ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList);
            VE.Value = Act.SQL;
            string SQLCalculated = VE.ValueCalculated;
            string dbName = "";
            string containerName = "";
            if (Action == eDBValidationType.SimpleSQLOneValue || Action == eDBValidationType.UpdateDB)
            {
                dbName = DatabaseName;
                containerName = Act.Table;
            }
            switch (Action)
            {
                case eDBValidationType.FreeSQL:
                    dbName = DatabaseName;
                    containerName = SQLCalculated.Split(' ')[SQLCalculated.Split(' ').Length - 1];
                    Container container = GetContainer(dbName, containerName);
                    SetOutputFromApiResponse(container, SQLCalculated);
                    break;
                case eDBValidationType.SimpleSQLOneValue:
                    Container objContainer = GetContainer(dbName, containerName);
                    SQLCalculated = "select * from " + containerName + " where " + Act.Where;
                    SetOutputFromApiResponse(objContainer, SQLCalculated);
                    break;
                case eDBValidationType.RecordCount:
                    dbName = DatabaseName;
                    if (SQLCalculated.Contains("where"))
                    {
                        containerName = SQLCalculated.Substring(0, SQLCalculated.IndexOf("where"));
                    }
                    else
                    {
                        containerName = SQLCalculated;
                    }
                    Container recordContainer = GetContainer(dbName, containerName);
                    SQLCalculated = "select Count(1) from " + SQLCalculated;
                    SetOutputFromApiResponse(recordContainer, SQLCalculated);
                    break;
                case eDBValidationType.UpdateDB:
                    throw new NotImplementedException("Update not yet implemented for Cosmos Db");
                default:
                    break;
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
            FeedResponse<object> currentResultSet = null;
            Dictionary<string, object> outputVals = new Dictionary<string, object>();
            FeedIterator<object> queryResultSetIterator = null;
            queryResultSetIterator = objContainer.GetItemQueryIterator<object>(sqlCalculated);
            while (queryResultSetIterator.HasMoreResults)
            {
                currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (object response in currentResultSet)
                {
                    JObject parsed = JObject.Parse(response.ToString());
                    var key = parsed.GetValue("id").ToString();
                    outputVals.Add(key, response);
                }
                Act.AddToOutputValues(outputVals);
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
