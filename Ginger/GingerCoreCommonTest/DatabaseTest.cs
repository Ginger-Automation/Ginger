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
