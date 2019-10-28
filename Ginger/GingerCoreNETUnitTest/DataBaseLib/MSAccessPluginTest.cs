using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.DataBaseLib
{
    [Ignore]
    [TestClass]
    public class MSAccessPluginTest
    {
        [TestMethod]
        public void TestAccessPluginConnTest()
        {
            GingerGrid gingerGrid = WorkSpace.Instance.LocalGingerGrid; //new GingerGrid(15001);   // Get free port !!!!!!!!!!!!!!!
            PluginPackage pluginPackage = new PluginPackage(@"C:\Users\Yaron\source\repos\Ginger\Ginger\MSAccessDB\bin\Debug");

            // run MSAccess Plugin
            PluginsManager.LoadPluginAndStartService(pluginPackage, "MSAccessService");

            // Wait for the plugin to register at the grid
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (gingerGrid.NodeList.Count == 0 && stopwatch.ElapsedMilliseconds < 10000) // max 10 seconds
            {
                Thread.Sleep(100);
            }

            GingerNodeProxy gingerNodeProxy =  gingerGrid.GetNodeProxy(gingerGrid.NodeList[0]);

            NewPayLoad newPayLoad = new NewPayLoad("xxxzzz");
            NewPayLoad RC = gingerNodeProxy.RunAction(newPayLoad);

            // Send config params

            // Send test db conn


        }

        [TestMethod]
        public void t1()
        {
            Database database = new Database();
            database.DBType = Database.eDBTypes.MSAccess;
            database.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + TestResources.GetTestResourcesFile(@"Database\GingerUnitTest.mdb");
            database.TestConnection();
        }
    }
}
