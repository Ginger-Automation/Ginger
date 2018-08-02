#region License
/*
Copyright © 2014-2018 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.POMs;
using GingerTestHelper;
using GingerWPFUnitTest.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GingerWPFUnitTest.AgentsLib
{
    [TestClass]
    public class AgentsTest
    {
        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static TestContext mTC;

        static string LogFile;
        static string SolutionFolder;
        Mutex mutex = new Mutex();
        static GingerAutomator mGingerAutomator = new GingerAutomator();

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            mTC = TC;
            mGingerAutomator.StartGinger();

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\EnvsTest");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\EnvsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator.OpenSolution(SolutionFolder);

            LogFile = mTC.TestLogsDir + @"\GingerWPF_BasicsTest.txt";
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {         
            mGingerAutomator.CloseGinger();
        }


        [TestInitialize]
        public void TestInitialize()
        {
            mutex.WaitOne();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mutex.ReleaseMutex();
        }


        [TestMethod,Timeout(60000)]
        public void VisualCompareAgentConfig()
        {
            //Arrange
            string name = "Visual Compare";
            Agent a = new Agent() { Name = name, DriverType = Agent.eDriverType.SeleniumFireFox };
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(a);

            //Act            
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            AgentsPOM.SelectAgent(name);
            mGingerAutomator.MainWindowPOM.ChangeSize(1280, 800);
            //Act            
            bool IsEquel = AgentsPOM.IsWindowBitmapEquel(AgentsPOM.GetSelectedAgentEditPage(), "AgentEditPage");

            //Assert
            Assert.IsTrue(IsEquel, "Agent edit page equel to baseline");
        }



        [TestMethod,Timeout(60000)]
        public void AddAgentUsingWizard()
        {
            //Arrange       
            string name = "bondi";
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();

            //Act
            AgentsPOM.CreateAgent(name, ePlatformType.Web, Agent.eDriverType.SeleniumFireFox);
            AgentsPOM.SelectAgent(name);
            Agent agent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(name, agent.Name, "Agent.Name is same");
        }

        [TestMethod,Timeout(60000)]
        public void RenameAgent()
        {
            //Arrange     
            string OldName = "Bond 007";
            string NewName = "James Bond";

            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();

            //Act
            AgentsPOM.CreateAgent(OldName, ePlatformType.Web, Agent.eDriverType.SeleniumFireFox);
            AgentsPOM.RenameAgent(OldName, NewName);
            Agent treeNodeAgent = AgentsPOM.SelectAgent(NewName);

            Agent SRAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == NewName select x).SingleOrDefault();

            // assert
            Assert.AreEqual(treeNodeAgent, SRAgent, "TreeNode Agent = SR.Agent");
            Assert.AreEqual(NewName, SRAgent.Name, "SR.Agent NewName");
        }


        [TestMethod,Timeout(60000)]
        public void AddAgentsFolderinFilesystemShowinTree()
        {
            //Arrange
            string folderName = "Agents sub folder 1";
            string subFolder = Path.Combine(SolutionFolder, "Agents", folderName);
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();

            //Act
            Directory.CreateDirectory(subFolder);
            bool agentExist = AgentsPOM.AgentsTree.IsItemExist(folderName);

            // assert            
            Assert.IsTrue(agentExist, "Agent exist in tree");
        }

        [TestMethod,Timeout(60000)]
        public void AddAgentsFolderUsingMenu()
        {
            //Arrange
            string folderName = "sub folder 1";            

            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            AgentsPOM.AgentsTree.SelectRootItem();

            //Act            
            AgentsPOM.AddSubFolder(folderName);
            bool folderCreatedOnTree = AgentsPOM.AgentsTree.IsItemExist(folderName);
            string subFolder = Path.Combine(SolutionFolder, "Agents", folderName);

            // assert            
            Assert.IsTrue(folderCreatedOnTree,"Folder create on tree");
            Assert.IsTrue(Directory.Exists(subFolder),"sub folder exist");
        }

        [TestMethod,Timeout(60000)]
        public void AddAgentsFolderUsingMenuAndAddAgent()
        {
            //Arrange
            string folderName = "sub folder 2";
            string name = "IE 1";

            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            AgentsPOM.AgentsTree.SelectRootItem();
            AgentsPOM.AddSubFolder(folderName);

            //Act            
            AgentsPOM.CreateAgent(folderName, name, ePlatformType.Web, Agent.eDriverType.SeleniumIE);
            bool folderExist = AgentsPOM.AgentsTree.IsItemExist(folderName);
            bool agentExist = AgentsPOM.AgentsTree.IsItemExist(name);

            // assert            
            Assert.IsTrue(folderExist, "Folder exist");
            Assert.IsTrue(agentExist , "Agent exist");
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", @".\TestData\Agents.csv", "Agents#csv", DataAccessMethod.Sequential)]        
        [TestMethod,Timeout(60000)]
        public void CreateAgentsFromCSV()
        {
            // arrange
            string agentName = mTC.DataRow["Name"].ToString();
            string platfromType = mTC.DataRow["PlatfromType"].ToString();
            string driverType = mTC.DataRow["DriverType"].ToString();

            //Act
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            ePlatformType platform = (ePlatformType)Enum.Parse(typeof(ePlatformType), platfromType);            
            Agent.eDriverType driver = (Agent.eDriverType)Enum.Parse(typeof(Agent.eDriverType), driverType);
            
            AgentsPOM.CreateAgent(agentName, platform, driver);
            bool agentCreated = AgentsPOM.AgentsTree.IsItemExist(agentName);

            //Assert
            Assert.IsTrue(agentCreated, "Agent created: " + agentName + ", " + platfromType + ", " + driverType );
        }


        [DataRow("Web 1", "Web", "SeleniumChrome")]
        [DataRow("Web 2", "Web", "SeleniumFireFox")]
        [TestMethod,Timeout(60000)]
        public void CreateAgentsbyTestData(string agentName, string platfromType, string driverType)
        {
            // arrange
            // We get the data from the params

            //Act
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            ePlatformType platform1 = (ePlatformType)Enum.Parse(typeof(ePlatformType), platfromType);
            Agent.eDriverType driver1 = (Agent.eDriverType)Enum.Parse(typeof(Agent.eDriverType), driverType);

            AgentsPOM.CreateAgent(agentName, platform1, driver1);
            bool agentCreated = AgentsPOM.AgentsTree.IsItemExist(agentName);

            //Assert
            Assert.IsTrue(agentCreated, "Agent created: " + agentName + ", " + platfromType + ", " + driverType);
        }



        [TestMethod,Timeout(60000)]
        public void CopyPasteAgentinAgentRoot()
        {
            //Arrange            
            string name = "C1";
            string copy = "C1_Copy";
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            AgentsPOM.CreateAgent(name, ePlatformType.Web, Agent.eDriverType.SeleniumIE);

            //Act                        
            AgentsPOM.AgentsTree.Copy();
            AgentsPOM.AgentsTree.SelectRootItem();
            AgentsPOM.AgentsTree.Paste(copy);            
            bool agentExist = AgentsPOM.AgentsTree.IsItemExist(copy);

            Agent Acopy = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == copy select x).SingleOrDefault();
            RepositoryFolder<Agent> AgentsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Agent>();
            Agent ACopyTag = (from x in AgentsFolder.GetFolderItems() where x.Name == copy select x).SingleOrDefault();

            // assert                        
            Assert.IsTrue(agentExist, "Agent exist");
            Assert.IsTrue(Acopy != null);
            Assert.AreEqual(Acopy, ACopyTag);

        }

        [TestMethod,Timeout(60000)]
        public void CutPasteAgentFromRootToSubFolder()
        {
            //Arrange            
            string name = "Move Me";
            string folderName = "MySubFolder";
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();
            Agent MyAgent = AgentsPOM.CreateAgent(name, ePlatformType.Web, Agent.eDriverType.SeleniumChrome);
            AgentsPOM.AgentsTree.SelectRootItem();
            AgentsPOM.AddSubFolder(folderName);            

            //Act            
            AgentsPOM.AgentsTree.SelectItem(name);
            AgentsPOM.AgentsTree.Cut();
            AgentsPOM.AgentsTree.SelectItem(folderName);
            AgentsPOM.AgentsTree.Paste();

            bool agentExist = AgentsPOM.AgentsTree.IsItemExist(name);
            
            RepositoryFolder<Agent> AgentsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Agent>();
            RepositoryFolder<Agent> subFolder = AgentsFolder.GetSubFolder(folderName);
            Agent ACopyTag = (from x in subFolder.GetFolderItems() where x.Name == name select x).SingleOrDefault();

            // assert                        
            Assert.IsTrue(agentExist, "Agent exist");
            Assert.IsTrue(ACopyTag != null);
            Assert.AreEqual(@"~\Agents\" + folderName, ACopyTag.ContainingFolder);
            Assert.AreEqual(MyAgent, ACopyTag, "Same agent object in memeory");
        }


        [TestMethod,Timeout(60000)]
        public void CutPasteAgentFromSubFolderToroot()
        {
            //Arrange            
            string name = "Move Me Up";
            string folderName = "MySubFolder 2";
            AgentsPOM AgentsPOM = mGingerAutomator.MainWindowPOM.GotoAgents();            
            AgentsPOM.AgentsTree.SelectRootItem();
            AgentsPOM.AddSubFolder(folderName);
            Agent MyAgent = AgentsPOM.CreateAgent(folderName, name, ePlatformType.Web, Agent.eDriverType.SeleniumChrome);            

            //Act            
            AgentsPOM.AgentsTree.SelectItem(name);
            AgentsPOM.AgentsTree.Cut();
            AgentsPOM.AgentsTree.SelectRootItem();
            AgentsPOM.AgentsTree.Paste();

            bool agentExist = AgentsPOM.AgentsTree.IsItemExist(name);

            RepositoryFolder<Agent> AgentsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Agent>();
            RepositoryFolder<Agent> subFolder = AgentsFolder.GetSubFolder(folderName);
            Agent ACopyTag = (from x in subFolder.GetFolderItems() where x.Name == name select x).SingleOrDefault();

            // assert                        
            Assert.IsTrue(agentExist, "Agent exist");
            Assert.IsTrue(ACopyTag != null);
            Assert.AreEqual(@"~\Agents\" + folderName, ACopyTag.ContainingFolder);
            Assert.AreEqual(MyAgent, ACopyTag, "Same agent object in memeory");
        }

    }
}
