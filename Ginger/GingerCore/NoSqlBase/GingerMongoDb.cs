#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Authentication;
using Couchbase.N1QL;
using Couchbase.Configuration.Server.Serialization;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GingerCore.NoSqlBase
{
    public class GingerMongoDb : NoSqlBase
    {        
        MongoClient mongoClient = null;
        ActDBValidation Act = null;
        //MongoServer
       
        public override List<eNoSqlOperations> GetSupportedActions()
        {
            List<eNoSqlOperations> SupportedActions = new List<eNoSqlOperations>();
            mongoClient.GetServer();
            SupportedActions.Add(eNoSqlOperations.freesql);
            return SupportedActions;
        }

        public bool Connect()
        {
            try
            {
                mongoClient = new MongoClient();
                var db = mongoClient.GetDatabase("inventory");//database name 

                return true;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to connect to Mongo DB", e);
                return false;
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

        public override List<string> GetTableList(string keyspace)
        {
            Connect();
            List<string> table = new List<string>();
            var db = mongoClient.GetDatabase("inventory"); //database name 
            foreach (var item in db.ListCollectionsAsync().Result.ToListAsync<BsonDocument>().Result)
            {
                table.Add(item.ToString());
            }
            return table;
        }

        public override List<string> GetColumnList(string tablename)
        {
            return null;
        }

        private void Disconnect()
        {
            //            
        }

        public override void PerformDBAction()
        {
            if (!Connect())
            {
                Act.Error = "Failed to connect to Mongo DB";
                return;
            }
            string SQL = Act.SQL;
            string keyspace = Act.Keyspace;
            ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList);
            VE.Value = SQL;
            string SQLCalculated = VE.ValueCalculated;

            IQueryResult<dynamic> result = null;
            try
            {
                switch (Action)
                {
                    case Actions.ActDBValidation.eDBValidationType.FreeSQL:
                        //result = clusterCB.Query<dynamic>(SQLCalculated);
                        //for (int i = 0; i < result.Rows.Count; i++)
                        //{
                        //    Act.ParseJSONToOutputValues(result.Rows[i].ToString(), i + 1);
                        //}
                        break;
                    case Actions.ActDBValidation.eDBValidationType.RecordCount:
                        //result = clusterCB.Query<dynamic>("Select Count(*) as RECORDCOUNT from `" + bucketName + "`");
                        //Act.ParseJSONToOutputValues(result.Rows[0].ToString(), 1);
                        break;
                    case Actions.ActDBValidation.eDBValidationType.UpdateDB:
                        //var RS1 = clusterCB.Query<dynamic>(SQLCalculated);
                        break;

                    default:
                        throw new Exception("Action Not SUpported");
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
    }
}
