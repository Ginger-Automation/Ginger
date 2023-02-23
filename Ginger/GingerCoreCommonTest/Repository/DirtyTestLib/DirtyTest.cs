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

using System;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.Repository
{
    
    [TestClass]
    [Level1]
    public class DirtyTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }        

        [ClassInitialize()]
        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
            
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommonTest);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }

        [TestMethod]  [Timeout(60000)]
        public void ObjWithoutTracking()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "aaa";            

            //Act                        


            //Assert  
            Assert.AreEqual(eDirtyStatus.NoTracked, item.DirtyStatus,  "item is not tracked");
        }


        [TestMethod]  [Timeout(60000)]        
        public void CheckObjDirty()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "aaa";            
            item.StartDirtyTracking();

            //Act            
            item.Name = "bbb";

            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item.DirtyStatus,  "item is dirty");            
        }


        [TestMethod]  [Timeout(60000)]
        public void PropertyNotSerializedNochangetoDirty()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "aaa";            

            item.StartDirtyTracking();

            //Act            
            item.DontSaveMe = "create at run time do not save do not track";

            //Assert  
            Assert.AreEqual(eDirtyStatus.NoChange, item.DirtyStatus,  "item is dirty");
        }

        bool DirtyStatusChangedTriggered = false;
        [TestMethod]  [Timeout(60000)]
        public void ObjWitChildsTracking()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "abc";
            item.childs = new ObservableList<MyComplextRepositoryItemChild>();
            MyComplextRepositoryItemChild child1 = new MyComplextRepositoryItemChild() { Name = "Child 1" };
            item.childs.Add(child1);
            item.StartDirtyTracking();

            item.OnDirtyStatusChanged += aaa;

            //Act                        
            child1.Name = "def";

            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item.DirtyStatus,  "item dirty status changed to modified");
            Assert.AreEqual(eDirtyStatus.Modified, child1.DirtyStatus,  "child item dirty status changed to modified");
            Assert.IsTrue(DirtyStatusChangedTriggered, "DirtyStatusChangedTriggered=true");
        }

        private void aaa(object sender, EventArgs e)
        {
            DirtyStatusChangedTriggered = true;
        }

        [TestMethod]  [Timeout(60000)]
        public void AddChildToList()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();            
            item.Name = "abc";
            item.childs = new ObservableList<MyComplextRepositoryItemChild>();                        
            item.StartDirtyTracking();

            //Act                        
            item.childs.Add(new MyComplextRepositoryItemChild() { Name = "Child 1" });

            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item.DirtyStatus,  "item dirty status changedt to modified since one child was added");
        }


        [TestMethod]  [Timeout(60000)]
        public void RemoveChildFromList()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "abc";
            item.childs = new ObservableList<MyComplextRepositoryItemChild>();            
            item.childs.Add(new MyComplextRepositoryItemChild() { Name = "Child 1" });
            item.childs.Add(new MyComplextRepositoryItemChild() { Name = "Child 2" });
            item.childs.Add(new MyComplextRepositoryItemChild() { Name = "Child 3" });

            item.StartDirtyTracking();

            //Act                        
            item.childs.RemoveAt(1);
            
            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item.DirtyStatus, "item dirty status changed to modified since one child was removed");            
        }

        
        [TestMethod]
        [Timeout(60000)]
        public void CopyItemIsDirty()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "abc";
            item.StartDirtyTracking();

            //Act                        
            MyComplextRepositoryItem item2 = (MyComplextRepositoryItem)item.CreateCopy();            

            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item2.DirtyStatus, "item dirty status changed to modified since it is a copy");
        }


        [TestMethod]  [Timeout(60000)]
        public void UpdateChildPropertyWhichisNotSerialzied()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "Runtime";
            item.childs = new ObservableList<MyComplextRepositoryItemChild>();
            MyComplextRepositoryItemChild child1 = new MyComplextRepositoryItemChild() { Name = "Child 1" };
            item.childs.Add(child1);
            item.StartDirtyTracking();


            //Act                        
            child1.Status = "Pass";

            //Assert  
            Assert.AreEqual(eDirtyStatus.NoChange, item.DirtyStatus, "item dirty status is not modified");
            Assert.AreEqual(eDirtyStatus.NoChange, child1.DirtyStatus, "child item dirty status is not modified");            
        }


        [TestMethod]
        [Timeout(60000)]
        public void EditNewAddedChildToList()
        {
            //Arrange
            MyComplextRepositoryItem item = new MyComplextRepositoryItem();
            item.Name = "abc";
            item.childs = new ObservableList<MyComplextRepositoryItemChild>();
            item.StartDirtyTracking();

            //Act           
            MyComplextRepositoryItemChild newChild = new MyComplextRepositoryItemChild() { Name = "Child 1" };
            item.childs.Add(newChild); //here parent will be marked as dirty
            item.SetDirtyStatusToNoChange(); //like been done on Save item to clear dirty
            newChild.Name = "NewName"; //expecting parent to show as dirty again because one of it's childs modified

            //Assert  
            Assert.AreEqual(eDirtyStatus.Modified, item.DirtyStatus, "item dirty status changed to modified since one child was added and then child was modified");
        }


    }
}
