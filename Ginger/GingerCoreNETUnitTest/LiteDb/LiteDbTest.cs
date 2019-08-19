using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

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
            var filterData = dbConector.FilterCollection(bfLiteColl, Query.Contains("Name", "bf name"));
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
            List<GingerActvityGroup> data = new List<GingerActvityGroup>();
            for (var a = 0; a < 5; a++)
            {
                GingerActvityGroup item = new GingerActvityGroup();
                item.Name = ($"name.{a.ToString()}");
                item.Description = ($"description.{a.ToString()}");
                item.GUID = Guid.NewGuid().ToString();
                item.RunStatus = "run";
                data.Add(item);
            }
            return data;
        }

        private List<GingerBusinessFlow> GetGingerBf(List<GingerActvityGroup> acGrpData)
        {
            return new List<GingerBusinessFlow>()
            {
              new  GingerBusinessFlow{ Description = "bf desc1", Name = "bf name1", GUID = Guid.NewGuid().ToString(),
                RunStatus = "failed", ActivitiesGroupColl = acGrpData }
            };

        }

        private LiteCollection<GingerBusinessFlow> GetBfLiteData()
        {
            return dbConector.GetCollection<GingerBusinessFlow>("BusinessFlows");
        }

        private LiteCollection<GingerActvityGroup> GetActGrLiteData()
        {
            return dbConector.GetCollection<GingerActvityGroup>("ActivityGroups");
        }
    }
}
