#region License
/*
Copyright © 2014-2020 European Support Limited

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
using System.Linq;
using System.Reflection;
using GingerCore.Actions;
using Amdocs.Ginger.Common;

using MongoDB.Driver;
using MongoDB.Bson;

using Newtonsoft.Json;

namespace GingerCore.NoSqlBase
{
    public class GingerMongoDb : NoSqlBase
    {        
        MongoClient mMongoClient = null;
        ActDBValidation Act = null;
        string DbName;
      

        public override bool Connect()
        {
            try
            {
                ///
                /// ConnectionString format
                /// "mongodb://user1:password1@localhost/DB"
                ///
                if (Db.ConnectionString != null && !string.IsNullOrEmpty(Db.ConnectionString))
                {
                    var connectionString = Db.ConnectionStringCalculated.ToString();
                    DbName = MongoUrl.Create(Db.ConnectionStringCalculated.ToString()).DatabaseName;
                    if (DbName == null)
                    {
                        return false;
                    }
                    mMongoClient = new MongoClient(connectionString);
                }
                else
                {
                    ///
                    /// Host format
                    /// "mongodb://HostOrIP:27017/DBName"
                    ///
                    string[] HostPortDB = Db.TNSCalculated.Split('/');
                    string[] HostPort = HostPortDB[0].Split(':');
                    //need to get db name

                    MongoCredential mongoCredential = null;
                    MongoClientSettings mongoClientSettings = null;
                    if (HostPort.Length == 2 && HostPortDB.Length == 2)
                    {
                        if (string.IsNullOrEmpty(HostPortDB[HostPortDB.Length - 1]))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Database is not mentioned in the TNS/Host.");
                            return false;
                        }

                        if (string.IsNullOrEmpty(Db.Pass) && string.IsNullOrEmpty(Db.User))
                        {
                            mongoClientSettings = new MongoClientSettings
                            {
                                Server = new MongoServerAddress(HostPort[0], Convert.ToInt32(HostPort[1]))
                            };
                        }
                        else
                        {
                            bool res = false;
                            String deCryptValue = EncryptionHandler.DecryptString(Db.PassCalculated.ToString(), ref res, false);
                            if (res == true)
                            {
                                mongoCredential = MongoCredential.CreateCredential(HostPortDB[HostPortDB.Length-1], Db.UserCalculated, deCryptValue);
                            }
                            else
                            {
                                mongoCredential = MongoCredential.CreateCredential(HostPortDB[HostPortDB.Length - 1], Db.UserCalculated, Db.PassCalculated.ToString());
                            }
                            mongoClientSettings = new MongoClientSettings
                            {
                                Server = new MongoServerAddress(HostPort[0], Convert.ToInt32(HostPort[1])),
                                //UseSsl = true,
                                Credentials = new[] { mongoCredential }
                            };
                        }
                        DbName = HostPortDB[HostPortDB.Length - 1];
                        mMongoClient = new MongoClient(mongoClientSettings);
                    }
                    else
                    {
                        return false;
                    }

                }
                //check dbname is present in the dblist
                if (GetDatabaseList().Contains(DbName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to connect to Mongo DB", e);
                return false;
            }
        }

        public override bool MakeSureConnectionIsOpen()
        {
            try
            {
                if (GetDatabaseList().Contains(DbName))
                {
                    return true;
                }
                else
                {
                    return Connect();
                }
            }
            catch (Exception ex)
            {
                return Connect();
            }
        }

        //TODO: need this while checking Test Connection , need to find a better way
        public GingerMongoDb(Environments.Database mDB)
        {
            this.Db = mDB;
        }

        public GingerMongoDb(ActDBValidation.eDBValidationType DBValidationtype, Environments.Database mDB, ActDBValidation mact)
        {
            Action = DBValidationtype;
            this.Db = mDB;
            Act = mact;
        }

        public override List<string> GetKeyspaceList()
        {
            return null;
        }
        public List<string> GetDatabaseList()
        {
            return mMongoClient.ListDatabaseNames().ToList();
        }
        public override List<string> GetTableList(string dbName)
        {
            Connect();
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = this.DbName;
            }
            List<string> table = new List<string>();
            var db = mMongoClient.GetDatabase(dbName);
            var names = db.ListCollectionNames().ToList();
            foreach (var item in names)
            {
                table.Add(item.ToString());
            }
            return table;
        }

        public override List<string> GetColumnList(string collectionName)
        {
            Connect();
            List<string> columns = new List<string>();
            var db = mMongoClient.GetDatabase(DbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            var result = collection.Find(new BsonDocument()).Project(Builders<BsonDocument>.Projection.Exclude("_id")).ToList();
            Dictionary<string, object> dictionary = null;
            foreach(var row in result)
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(row.ToString());
                List <string> previousRowColumns = columns.ToList();
                var currentRowColumns = previousRowColumns.Union(dictionary.Keys);
                if (currentRowColumns.Count() > previousRowColumns.Count)
                {
                    columns.Clear();//clear previousRowColumns columns
                    foreach (string key in currentRowColumns)
                    {
                        columns.Add(key);
                    }
                }
            }

            return columns;
        }

        private void Disconnect()
        {
            //Driver maintains a connection pool internally. 
            //No need to dispose of any connections
        }
        private string GetCollectionName(string inputSQL)
        {
            if (Action == ActDBValidation.eDBValidationType.RecordCount)
            {
                return inputSQL;
            }
            else
            {
                if (inputSQL.Contains("."))
                {
                    string[] sqlWords = inputSQL.Split('.');
                    return sqlWords[1];
                }
                else
                {
                    Act.Error = "Invalid Query format.";
                    return null;
                }
            }
            
        }
        private string GetUpdateQueryParams(string inputSQL)
        {
            int startIndex = inputSQL.IndexOf("(") + 1;
            int endIndex = inputSQL.IndexOf(")");
            string updateQueryParams = inputSQL.Substring(startIndex, endIndex - startIndex);
            return updateQueryParams.Trim();
        }
        private string GetQueryParamater(string inputSQL,string param)
        {
            int startIndex = inputSQL.IndexOf(param);
            string queryParameterValue = "";
            if (startIndex != -1)
            {
                int endIndex = inputSQL.IndexOf(")", startIndex);
                queryParameterValue = inputSQL.Substring(startIndex, endIndex - startIndex);
            }
            queryParameterValue = queryParameterValue.Replace(param + "(", "");
            if (param.Equals("find") && string.IsNullOrEmpty(queryParameterValue))
            {
                return "{}";
            }
            if (param.Equals("sort") && string.IsNullOrEmpty(queryParameterValue))
            {
                return "{ _id:-1 }";
            }
            if (param.Equals("limit") && string.IsNullOrEmpty(queryParameterValue))
            {
                return "0";
            }
            return queryParameterValue;
        }
        public override void PerformDBAction()
        {
            ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList);
            VE.Value = Act.SQL;
            string SQLCalculated = VE.ValueCalculated;
            string collectionName = "";
            if (Action== Actions.ActDBValidation.eDBValidationType.SimpleSQLOneValue)
            {
                collectionName = Act.Table;
            }
            else
            {
                collectionName = GetCollectionName(SQLCalculated);
            }
            var DB = mMongoClient.GetDatabase(DbName);
            var collection = DB.GetCollection<BsonDocument>(collectionName);

            try
            {
                switch (Action)
                {
                    case Actions.ActDBValidation.eDBValidationType.FreeSQL:

                        if (SQLCalculated.Contains("insertMany"))
                        {
                            string queryParam = GetUpdateQueryParams(SQLCalculated).ToString();
                            Newtonsoft.Json.Linq.JArray jsonArray = Newtonsoft.Json.Linq.JArray.Parse(queryParam);
                            List<BsonDocument> documents = new List<BsonDocument>();
                            foreach (Newtonsoft.Json.Linq.JObject obj in jsonArray.Children<Newtonsoft.Json.Linq.JObject>())
                            {
                                BsonDocument document = BsonDocument.Parse(obj.ToString());
                                documents.Add(document);
                            }
                            collection.InsertMany(documents);
                        }
                        else if (SQLCalculated.Contains("insertOne") || SQLCalculated.Contains("insert"))
                        {
                            BsonDocument insertDocumnet = BsonDocument.Parse(GetUpdateQueryParams(SQLCalculated));
                            collection.InsertOne(insertDocumnet);
                        }
                        else
                        {
                            var result = collection.Find(GetQueryParamater(SQLCalculated, "find")).
                            Project(Builders<BsonDocument>.Projection.Exclude("_id").Exclude(GetQueryParamater(SQLCalculated, "projection"))).
                            Sort(BsonDocument.Parse(GetQueryParamater(SQLCalculated, "sort"))).
                            Limit(Convert.ToInt32(GetQueryParamater(SQLCalculated, "limit"))).
                            ToList();

                            var obj = result.ToJson();
                            Act.ParseJSONToOutputValues(obj.ToString(), 1);
                        }
                        break;
                    case Actions.ActDBValidation.eDBValidationType.RecordCount:
                        var count = collection.Count(new BsonDocument());
                        Act.AddOrUpdateReturnParamActual("Record Count", count.ToString());
                        break;
                    case Actions.ActDBValidation.eDBValidationType.UpdateDB:
                        
                        //do commit
                        if (Act.CommitDB_Value == true)
                        {
                            var session = mMongoClient.StartSession();
                            session.StartTransaction();
                            UpdateCollection(SQLCalculated, collection);
                            session.CommitTransaction();
                        }
                        else
                        {
                            UpdateCollection(SQLCalculated, collection);
                        }
                        break;
                    case Actions.ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                        string col = Act.Column;
                        string where = Act.Where;
                        string filter = "";
                        var isNumeric = double.TryParse(where, out double n);

                        //Simply matches on specific column type int
                        //For ex where contains any int value
                        if (isNumeric)
                        {
                            filter = "{" + col + ":" + where + "}";
                        }
                        else 
                        {
                            //Equality matches on the whole embedded document require an exact match of the specified <value> document, including the field order
                            //For ex where contains value = {field1:_value1,field2:_value2,field3:"_value3",...}
                            if (where.Contains(","))
                            {
                                filter = "{" + col + ":" + where + "}";
                            }
                            //Simply matches on specific column
                            //For ex where contains any string value
                            else
                            {
                                filter = "{" + col + ":\"" + where + "\"}";
                            }
                        }
                        var resultSimpleSQLOne = collection.Find(filter).Project(Builders<BsonDocument>.Projection.Exclude("_id")).ToList().ToJson();
                        Act.ParseJSONToOutputValues(resultSimpleSQLOne.ToString(), 1);
                        break;
                    default:                        
                        Act.Error+= "Operation Type "+ Action +" is not yes supported for Mongo DB";
                        break;
                }
            }
            catch (Exception e)
            {
                Act.Error = "Failed to execute. Error :" + e.Message;
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            if (!Db.KeepConnectionOpen)
            {
                Disconnect();
            }


        }
        void UpdateCollection(string query, IMongoCollection<BsonDocument> collection)
        {
            string updateQueryParams = GetUpdateQueryParams(query);
            var updateQueryParamsStrings = updateQueryParams.Split(new char[] { ',' }, 2);
            //set filter
            var filterString = updateQueryParamsStrings[0];
            //set param
            var paramString = updateQueryParamsStrings[1];
            BsonDocument filterDocumnet = BsonDocument.Parse(filterString);
            BsonDocument paramDocumnet = BsonDocument.Parse(paramString);

            if (query.Contains("updateMany"))
            {
                collection.UpdateMany(filterDocumnet, paramDocumnet);
            }
            else
            {
                collection.UpdateOne(filterDocumnet, paramDocumnet);
            }
        }
    }
}
