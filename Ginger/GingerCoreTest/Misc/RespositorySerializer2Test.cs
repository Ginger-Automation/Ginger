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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class RespositorySerializer2Test
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        NewRepositorySerializer RS = new NewRepositorySerializer();

        string Separator = Path.DirectorySeparatorChar.ToString();

        [ClassInitialize]        
        public static void ClassInitialize(TestContext TestContext)
        {
            //TODO::
            mTestHelper.ClassInitialize(TestContext);
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCore);
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommon);
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreNET);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }

        [TestMethod]  [Timeout(60000)]
        public void NewRepositorySerializer_ReadOldXMLwithOldRS()
        {
            //read old xml using old RS
            // read old using RS2, save and load
            //compare
        }

        [TestMethod]
        [Timeout(60000)]
        public void ConditionValidationTest()
        {
            //Arrange
            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "ConditionVal.Ginger.BusinessFlow.xml");

            //Load BF
            //Act
            BusinessFlow businessFlow = (BusinessFlow)RS.DeserializeFromFile(FileName);

            //Assert
            Assert.AreEqual(1, businessFlow.Activities[0].Acts[0].InputValues.Count);
        }

        public void DBActInputValuesTest()
        {
            //Arrange
            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "DBActionInputValuesTest.Ginger.BusinessFlow.xml");

            //Load BF
            //Act
            BusinessFlow businessFlow = (BusinessFlow)RS.DeserializeFromFile(FileName);

            //Assert
            Assert.AreEqual(1, businessFlow.Activities[0].Acts[0].InputValues.Where(x => x.Param == "SQL").ToList().Count);
        }
        public void ExcelActInputValuesTest()
        {
            //Arrange
            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "ActExcelInputValuesTest.Ginger.BusinessFlow.xml");

            //Load BF
            //Act
            BusinessFlow businessFlow = (BusinessFlow)RS.DeserializeFromFile(FileName);

            //Assert
            Assert.AreEqual(1, businessFlow.Activities[0].Acts[0].InputValues.Where(x => x.Param == "ExcelFileName").ToList().Count);
        }

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void NewRepositorySerializer_ReadOldXML()
        //{
        //    // Using new SR2 to load and write old XML but load old object, save with the style


        //    //Arrange
        //    //GingerCore.Repository.RepositorySerializerInitilizer OldSR = new GingerCore.Repository.RepositorySerializerInitilizer();


        //    //GingerCore.Repository.RepositorySerializerInitilizer.InitClassTypesDictionary();
        //    NewRepositorySerializer RS2 = new NewRepositorySerializer();

        //    string fileName = Common.getGingerUnitTesterDocumentsFolder() + @"Repository\BigFlow1.Ginger.BusinessFlow.xml";

        //    //Act
        //    string txt = System.IO.File.ReadAllText(fileName);
        //    BusinessFlow BF = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(txt);

        //    //load with new 
        //    // BusinessFlow BF = (BusinessFlow)RS2.DeserializeFromFile(fileName);
        //    //Serialize to new style
        //    //string s = RS2.SerializeToString(BF);
        //    // cretae from new style SR2
        //    BusinessFlow BF2 = (BusinessFlow)RS2.DeserializeFromText(typeof(BusinessFlow), txt, filePath: fileName);

        //    //to test the compare change something in b like below
        //    // BF2.Activities[5].Description = "aaa";
        //    // BF2.Activities.Remove(BF2.Activities[10]);

        //    //Assert

        //    // System.IO.File.WriteAllText(@"c:\temp\BF1.xml", s);
        //   // Assert.AreEqual(78, BF.Activities.Count);
        //    //Assert.AreEqual(78, BF2.Activities.Count);

        //    CompareRepoItem(BF, BF2);
        //}

        private void CompareRepoItem(RepositoryItemBase a, RepositoryItemBase b)
        {
            
            var props = a.GetType().GetProperties();   
            foreach(PropertyInfo PI in props)
            {                
                var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token != null)
                {
                    Console.WriteLine("compare: " + a.ToString() + " " + PI.Name);

                    object aProp = PI.GetValue(a);
                    object bProp = b.GetType().GetProperty(PI.Name).GetValue(b);

                    if (aProp == null && bProp == null) continue;


                    if(aProp.ToString() != bProp.ToString())
                    {
                        throw new Exception("Items no match tostring: " + a.ItemName + " attr: " + PI.Name + " a=" + aProp.ToString() + " b=" + bProp.ToString());
                    }

                    //if (aProp != bProp)
                    //{
                    //    throw new Exception("Items no match: " + a.ItemName + " attr: " + PI.Name + " a=" + aProp.ToString() + " b=" + bProp.ToString());
                    //}

                 

                }
            }

            var fields = a.GetType().GetFields();
            foreach (FieldInfo FI in fields)
            {
                var token = FI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token != null)
                {
                    Console.WriteLine("compare: " + a.ToString() + " " + FI.Name);

                    object aFiled = FI.GetValue(a);
                    object bField = b.GetType().GetField(FI.Name).GetValue(b);

                    if (aFiled == null && bField == null) continue;

                    if (aFiled.ToString() != bField.ToString())
                    {
                        throw new Exception("Items no match tostring: " + a.ItemName + " attr: " + FI.Name + " a=" + aFiled.ToString() + " b=" + bField.ToString());
                    }

                    //if (aFiled != bField)
                    //{
                    //    throw new Exception("Items no match: " + a.ItemName + " attr: " + FI.Name + " a=" + aFiled.ToString() + " b=" + bField.ToString());
                    //}

                    if (aFiled is IObservableList)
                    {
                        if (((IObservableList)aFiled).Count != ((IObservableList)bField).Count)
                        {
                            throw new Exception("Items in list count do not match: " + a.ItemName + " attr: " + FI.Name + " a=" + aFiled.ToString() + " b=" + bField.ToString());
                        }
                        var aList = ((IObservableList)aFiled).GetEnumerator();
                        var bList = ((IObservableList)bField).GetEnumerator();

                        

                        while (aList.MoveNext())
                        {
                            bList.MoveNext();
                            RepositoryItemBase o1 = (RepositoryItemBase)aList.Current;
                            RepositoryItemBase o2 = (RepositoryItemBase)bList.Current;
                            CompareRepoItem(o1, o2);
                        } 


                            
                        }
                    }

                }
            }

        }
    }

