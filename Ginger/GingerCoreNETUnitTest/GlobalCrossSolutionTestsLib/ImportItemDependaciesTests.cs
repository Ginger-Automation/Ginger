using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GingerCoreNETUnitTest.GlobalCrossSolutionTestsLib
{
    [Level1]
    [TestClass]
    public class ImportItemDependaciesTests
    {
        static SolutionRepository SR;
        NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();
        ObservableList<GlobalSolutionItem> SelectedItemsListToImport = new ObservableList<GlobalSolutionItem>();
        List<VariableBase> VariableListToImport = new List<VariableBase>();
        List<EnvApplication> EnvAppListToImport = new List<EnvApplication>();

        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            mTestHelper.ClassInitialize(TC);

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution"));
            SR = WorkspaceHelper.CreateWorkspaceAndOpenSolution(path);

            GlobalSolutionUtils.Instance.SolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution"));
            GlobalSolutionUtils.Instance.EncryptionKey = EncryptionHandler.GetDefaultKey();
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
        public void TestCleanup()
        {
            mTestHelper.TestCleanup();
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetEnvironmentDependacies()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "Environments" + Path.DirectorySeparatorChar + "Test.Ginger.Environment.xml");

            //Act
            GlobalSolutionItem item = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, filePath, GlobalSolutionUtils.Instance.ConvertToRelativePath(filePath), true, GlobalSolutionUtils.Instance.GetRepositoryItemName(filePath), "");
            GlobalSolutionUtils.Instance.AddDependaciesForEnvironment(item, ref SelectedItemsListToImport, ref VariableListToImport);

            //Assert
            Assert.AreEqual(SelectedItemsListToImport.Count, 2);
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\DataSources\\AccessDS.Ginger.DataSource.xml"));
            Assert.AreEqual(VariableListToImport.Count, 1);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetPOMModelDependacies()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "Applications Models" + Path.DirectorySeparatorChar + "POM Models" + Path.DirectorySeparatorChar + "Facebook" + Path.DirectorySeparatorChar + "Facebook.Ginger.ApplicationPOMModel.xml");

            //Act
            GlobalSolutionItem item = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, filePath, GlobalSolutionUtils.Instance.ConvertToRelativePath(filePath), true, GlobalSolutionUtils.Instance.GetRepositoryItemName(filePath), "");
            GlobalSolutionUtils.Instance.AddDependaciesForPOMModel(item, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);

            //Assert
            Assert.AreEqual(SelectedItemsListToImport.Count, 1);
            Assert.AreEqual(VariableListToImport.Count, 1);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));

            Assert.AreEqual(EnvAppListToImport.Count, 0);
        }
        [TestMethod]
        [Timeout(60000)]
        public void GetSharedActionDependacies()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "SharedRepository" + Path.DirectorySeparatorChar + "Actions" + Path.DirectorySeparatorChar + "UIElement Action.Ginger.Action.xml");

            //Act
            GlobalSolutionItem item = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, filePath, GlobalSolutionUtils.Instance.ConvertToRelativePath(filePath), true, GlobalSolutionUtils.Instance.GetRepositoryItemName(filePath), "");
            GlobalSolutionUtils.Instance.AddDependaciesForSharedAction(item, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);

            //Assert
            Assert.AreEqual(SelectedItemsListToImport.Count, 2);
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\Applications Models\\POM Models\\SeleniumDemoValid.Ginger.ApplicationPOMModel.xml"));

            Assert.AreEqual(VariableListToImport.Count, 1);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));

            Assert.AreEqual(EnvAppListToImport.Count, 0);
        }
        [TestMethod]
        [Timeout(60000)]
        public void GetSharedActivityDependacies()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "SharedRepository" + Path.DirectorySeparatorChar + "Activities" + Path.DirectorySeparatorChar + "Activity 2.Ginger.Activity.xml");

            //Act
            GlobalSolutionItem item = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, filePath, GlobalSolutionUtils.Instance.ConvertToRelativePath(filePath), true, GlobalSolutionUtils.Instance.GetRepositoryItemName(filePath), "");
            GlobalSolutionUtils.Instance.AddDependaciesForSharedActivity(item, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);

            //Assert
            Assert.AreEqual(SelectedItemsListToImport.Count, 4);
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\Applications Models\\POM Models\\SeleniumDemoValid.Ginger.ApplicationPOMModel.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\SharedRepository\\Actions\\Browser Action.Ginger.Action.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\SharedRepository\\Actions\\UIElement Action.Ginger.Action.xml"));

            Assert.AreEqual(VariableListToImport.Count, 1);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));

            Assert.AreEqual(EnvAppListToImport.Count, 1);
            Assert.IsNotNull(EnvAppListToImport.Where(x => x.Name == "MyWebApp"));

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetBusinessFlowDependacies()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Act
            GlobalSolutionItem item = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, filePath, GlobalSolutionUtils.Instance.ConvertToRelativePath(filePath), true, GlobalSolutionUtils.Instance.GetRepositoryItemName(filePath), "");
            GlobalSolutionUtils.Instance.AddDependaciesForBusinessFlows(item, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);

            //Assert
            Assert.AreEqual(SelectedItemsListToImport.Count, 12);
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\Applications Models\\POM Models\\SeleniumDemoValid.Ginger.ApplicationPOMModel.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\SharedRepository\\Actions\\Browser Action.Ginger.Action.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\SharedRepository\\Actions\\UIElement Action.Ginger.Action.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\SharedRepository\\Activities\\Activity 2.Ginger.Activity.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\DataSources\\AccessDS.Ginger.DataSource.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\Documents\\bankCode3.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\Documents\\Multiple Values.xlsx"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x=>x.ItemExtraInfo == "~\\\\Environments\\Test.Ginger.Environment.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "NewVarString"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "MyWebApp"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "MyWebServicesApp"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "MyWindowsApp"));

            Assert.AreEqual(VariableListToImport.Count, 2);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarPasswordString"));
            string strValuetoPass = EncryptionHandler.DecryptwithKey(VariableListToImport.Where(x => x.Name == "NewVarPasswordString").FirstOrDefault().Value, EncryptionHandler.GetDefaultKey());
            Assert.AreEqual(strValuetoPass, "ABCD");
            
            Assert.AreEqual(EnvAppListToImport.Count, 1);
            Assert.IsNotNull(EnvAppListToImport.Where(x => x.Name == "MyWebApp"));

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetDependaciesForEnvParamUsingRegex()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Act
            GlobalSolutionUtils.Instance.AddDependaciesForEnvParam(filePath, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);

            //Assert
            Assert.AreEqual(EnvAppListToImport.Count, 1);
            Assert.IsNotNull(EnvAppListToImport.Where(x => x.Name == "MyWebApp"));
            string strValuetoPass = EncryptionHandler.DecryptwithKey(EnvAppListToImport[0].GeneralParams.Where(x => x.Name == "Password").FirstOrDefault().Value, EncryptionHandler.GetDefaultKey());
            Assert.AreEqual(strValuetoPass, "ABCD");

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetDependaciesForGlobalVariableUsingRegex()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Act
            GlobalSolutionUtils.Instance.AddDependaciesForGlobalVariable(filePath, ref SelectedItemsListToImport, ref VariableListToImport);

            //Assert
            Assert.AreEqual(VariableListToImport.Count, 2);
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarString"));
            Assert.IsNotNull(VariableListToImport.Where(x => x.Name == "NewVarPasswordString"));
            string strValuetoPass = EncryptionHandler.DecryptwithKey(VariableListToImport.Where(x => x.Name == "NewVarPasswordString").FirstOrDefault().Value, EncryptionHandler.GetDefaultKey());
            Assert.AreEqual(strValuetoPass, "ABCD");

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetDependaciesForDataSourceUsingRegex()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Act
            GlobalSolutionUtils.Instance.AddDependaciesForDataSource(filePath, ref SelectedItemsListToImport);

            //Assert
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\DataSources\\AccessDS.Ginger.DataSource.xml"));

        }
        [TestMethod]
        [Timeout(60000)]
        public void GetDependaciesForDocumentsUsingRegex()
        {
            //Arrange            
            string filePath = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "GlobalCrossSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Act
            GlobalSolutionUtils.Instance.AddDependaciesForDocuments(filePath, ref SelectedItemsListToImport);

            //Assert
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\Documents\\bankCode3.xml"));
            Assert.IsNotNull(SelectedItemsListToImport.Where(x => x.ItemExtraInfo == "~\\\\Documents\\Multiple Values.xlsx"));

        }
    }
}
