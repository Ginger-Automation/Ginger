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

using Amdocs.Ginger.CoreNET.LiteDBFolder;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GingerCoreNETUnitTest.LiteDb
{
    [Ignore] // Fail on Mac

    [TestClass]
    public class LiteDbTest
    {
        private LiteDbConnector dbConector;

        [TestMethod]
        public void WriteToLiteDb()
        {
            var bfLiteColl = GetBfLiteData();
            var acgLiteColl = GetActGrLiteData();
            var acgColl = this.GetGingerActvityGroup();
            dbConector.SetCollection(bfLiteColl, this.GetGingerBf(acgColl));
            dbConector.SetCollection(acgLiteColl, acgColl);
            Assert.IsTrue(acgLiteColl.Count() > 0);
        }

        [TestMethod]
        public void ReadAndUpdateLiteDbData()
        {
            var bfLiteColl = dbConector.GetCollection<GingerBusinessFlow>("BusinessFlows");
            var filterData = dbConector.FilterCollection(GingerBusinessFlow.IncludeAllReferences(bfLiteColl), Query.Contains("Name", "bf name"));
            filterData.ForEach(a => a.Name = a.Name + " Modified BF");
            dbConector.SetCollection(bfLiteColl, filterData);
            Assert.IsTrue(bfLiteColl.Count() > 0);
        }

        [TestMethod]
        public void WriteToLiteDbFromObject()
        {
            LiteDbManager dbManager = new LiteDbManager();
            dbManager.WriteToLiteDb();
        }

        [TestInitialize]
        public void InitTest()
        {
            dbConector = new LiteDbConnector("GingerExecutionResults.db");
            var mapper = dbConector.GetMapper<GingerBusinessFlow>();
            mapper.DbRef(x => x.ActivitiesGroupColl, "acGrColl");
        }


        //[TestMethod]
        //public void ReadGingerRunSet()
        //{
        //    LiteDbManager dbManager = new LiteDbManager(@"C:\Ginger_sourc\Liran Test\ExecutionResults");
        //    var result=dbManager.GetRunSetLiteData();
        //    var filterData = dbManager.FilterCollection(result,Query.All());
        //}

        private List<GingerActvityGroup> GetGingerActvityGroup()
        {
            List<GingerActvityGroup> data = [];
            for (var a = 0; a < 5; a++)
            {
                GingerActvityGroup item = new GingerActvityGroup
                {
                    Name = ($"name.{a}"),
                    Description = ($"description.{a}"),
                    GUID = Guid.NewGuid().ToString(),
                    RunStatus = "run"
                };
                data.Add(item);
            }
            return data;
        }

        private List<GingerBusinessFlow> GetGingerBf(List<GingerActvityGroup> acGrpData)
        {
            return
            [
              new  GingerBusinessFlow{ Description = "bf desc1", Name = "bf name1", GUID = Guid.NewGuid().ToString(),
                RunStatus = "failed", ActivitiesGroupColl = acGrpData }
            ];

        }

        private ILiteCollection<GingerBusinessFlow> GetBfLiteData()
        {
            return dbConector.GetCollection<GingerBusinessFlow>("BusinessFlows");
        }

        private ILiteCollection<GingerActvityGroup> GetActGrLiteData()
        {
            return dbConector.GetCollection<GingerActvityGroup>("ActivityGroups");
        }
    }
}
