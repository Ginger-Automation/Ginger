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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.UITests
{
    [TestClass]
    public class ActLaunchJavaWSApplicationTest
    {

        // Launching and attaching to jnlp


        //static GingerRunner mGR = null;
        //static JavaDriver mDriver = null;
        //static String AppName = "Java Swing Test App";
        //[ClassInitialize()]
        //public static void ClassInit(TestContext context)
        //{
        //    mGR = new GingerRunner();

        //}
        //[TestMethod]  [Timeout(60000)]
        //public void LaunchJNLPAndAttachJavaAgent()
        //{
        //    ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
        //    LJA.LaunchJavaApplication = true;
        //    LJA.LaunchWithAgent = true;
        //    LJA.WaitForWindowTitle = "SimpleTableDemo";
        //    LJA.Port = "9898";
        //    LJA.URL = Common.getGingerUnitTesterDocumentsFolder() + @"JavaTestApp\SimpleTableDemo.jnlp";
          
        //    LJA.Execute();


        //    mGR.CalculateActionFinalStatus(LJA);


        //   // Assert.AreEqual(LJA.Status, Act.eStatus.Passed, "Action status");

        //}


        //Launching and attaching to  jar

        //[TestMethod]  [Timeout(60000)]
        //public void LaunchJARAndAttachJavaAgent()
        //{
        //    ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
        //    LJA.LaunchJavaApplication = true;
        //    LJA.LaunchWithAgent = true;
        //    LJA.WaitForWindowTitle = AppName;
        //    LJA.Port = "9899";
        //    LJA.URL = Common.getGingerUnitTesterDocumentsFolder() + @"JavaTestApp\JavaTestApp.jar";
          
        //    LJA.Execute();
        //    mGR.CalculateActionFinalStatus(LJA);


        //   Assert.AreEqual(LJA.Status, eRunStatus.Passed, "Action status");
        //}

        //[ClassCleanup()]
        //public static void ClassCleanup()
        //{

        //    mDriver = new JavaDriver(null);
        //    mDriver.JavaAgentHost = "127.0.0.1";
        //    mDriver.JavaAgentPort = 9899;
        //    mDriver.CommandTimeout = 120;
        //    mDriver.cancelAgentLoading = false;
        //    mDriver.DriverLoadWaitingTime = 30;
        //    mDriver.ImplicitWait = 30;
        //    mDriver.StartDriver();

        //    PayLoad PLClose = new PayLoad("WindowAction");
        //    PLClose.AddValue("CloseWindow");
        //    // PLClose.AddEnumValue(AJTE.WaitforIdle);
        //    PLClose.AddValue(eLocateBy.ByTitle.ToString());
        //    PLClose.AddValue(AppName);
        //    // PLClose.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
        //    PLClose.ClosePackage();
        //    mDriver.Send(PLClose);
        //}

        //Launching and attaching  to exe


        //[TestMethod]  [Timeout(60000)]
        //public void LaunchEXEAndAttachJavaAgent()
        //{
        //    ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
        //    LJA.LaunchJavaApplication = true;
        //    LJA.LaunchWithAgent = true;
        //    LJA.WaitForWindowTitle = "Manual";
        //    LJA.WaitForWindowTitleMaxTime = "20";
        //    LJA.Port = "9895";
        //    LJA.URL = @"C:\Program Files (x86)\Jaydeep Marakana\ManualResponseTool_setup_V1.1\ManualResponseTool_setup_V1.1.exe";

        //    LJA.Execute();

        //    mGR.CalculateActionFinalStatus(LJA);


        //   //Assert.AreEqual(LJA.Status, Act.eStatus.Passed, "Action status");
        //}


        //attaching to existing process

        //attaching to process with empty title

        //attaching to process whose name is not starting with jp2 or java



    }
}
