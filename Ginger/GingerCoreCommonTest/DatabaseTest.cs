using GingerCore.Environments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest
{
    [TestClass]
    public class DatabaseTest
    {

        [TestMethod]
        public void TestPasswordVE()
        {
            string valueExpVar = "{Var Name= Database Name}";
            string valueExpDSPattern = "{DS Name=DefaultDataSource DST=MyCustomizedDataTable MASD=N IDEN=Cust ICOLVAL=GINGER_ID IROW=RowNum ROWNUM=0 Query QUERY=SELECT $ FROM MyCustomizedDataTable}";
            string valueExpEnvURL = "{EnvURL App=MyWebApp-2}";
            string valueExpFun = "{Function Fun=GetGUID()}";
            string valueExpCS = "{CS Exp=System.Environment.UserName}";
            string valueExpRegEx = "{RegEx Fun= matchValue Pat=\\d+ P1= aaa 123 bbb }";
            string valueExpFDObj = "{FD Object=Environment Field=Name}";

            Database database = new()
            {
                Pass = valueExpVar
            };

            database.DatabaseOperations = new DatabaseOperations(database);
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpVar, database.Pass);
            
            database.Pass = valueExpDSPattern;
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpDSPattern, database.Pass);

            database.Pass = valueExpEnvURL;
            database.EncryptDatabasePass();

            Assert.AreEqual(valueExpEnvURL, database.Pass);


            database.Pass = valueExpFun;
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpFun, database.Pass);

            database.Pass = valueExpCS;
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpCS, database.Pass);


            database.Pass = valueExpRegEx;
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpRegEx, database.Pass);
            
            database.Pass = valueExpFDObj;
            database.EncryptDatabasePass();
            Assert.AreEqual(valueExpFDObj, database.Pass);
        }



    }
}
