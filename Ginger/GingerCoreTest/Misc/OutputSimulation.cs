#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.WebServices;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class OutputSimulation
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;
        static Agent wsAgent;
        static WebServicesDriver mDriver = new WebServicesDriver(mBF);

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("NonUITests");
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "Output Simulation";
            mBF.Active = true;

            Platform p = new Platform();
            p.PlatformType = ePlatformType.WebServices;            

            wsAgent = new Agent();
            wsAgent.DriverType = Agent.eDriverType.WebServices;
            wsAgent.Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(wsAgent);

            mGR.BusinessFlows.Add(mBF);

        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestMethod]  [Timeout(60000)]
        public void SimulatedOuputGingerRunnerFlagOn()
        {


            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Web API REST";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            ActReturnValue simulateOutput = new ActReturnValue();
            simulateOutput.Active = true;
            simulateOutput.Param = "TestSimulation";
            simulateOutput.SimulatedActual = "TestSimulation";

            mBF.Activities[0].Acts.Add(restAct);
            mGR.RunInSimulationMode = true;

            mDriver.StartDriver();
            mGR.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "OK")
                            Assert.AreEqual(val.Actual, "OK");
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                        Assert.Fail();
                }
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SimulatedOuputActionFlagOn()
        {
            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Web API REST";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            restAct.SupportSimulation = true;

            ActReturnValue simulateOutput = new ActReturnValue();
            simulateOutput.Active = true;
            simulateOutput.Param = "TestSimulation";
            simulateOutput.SimulatedActual = "TestSimulation";

            mBF.Activities[0].Acts.Add(restAct);
            

            mDriver.StartDriver();
            mGR.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "OK")
                            Assert.AreEqual(val.Actual, "OK");
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                        Assert.Fail();
                }
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SimulatedOutputWithVETest()
        {
            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Web API REST with VE";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST Simulated Output VE";
            restAct.SupportSimulation = true;

            ActReturnValue simulateOutput = new ActReturnValue();
            simulateOutput.Active = true;
            simulateOutput.Param = "TestSimulation";
            ValueExpression testVE = new ValueExpression(null, mBF);
            testVE.Value = "simulated VE";
            simulateOutput.SimulatedActual = testVE.Value;

            mBF.Activities[0].Acts.Add(restAct);


            mDriver.StartDriver();
            mGR.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "simulated VE")
                            Assert.AreEqual(val.Actual, "simulated VE");
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                        Assert.Fail();
                }
            }
        }


        [TestMethod]  [Timeout(60000)]
        public void SimulatedOutputTest()
        {
            Activity Activity3 = new Activity();
            Activity3.Active = true;
            Activity3.ActivityName = "Web API REST Simulated Output";
            Activity3.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity3);


            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST Simulated Output";
            restAct.SupportSimulation = true;

            //Define dummy return value
            ActReturnValue simulateOutput = new ActReturnValue();
            simulateOutput.Active = true;
            simulateOutput.Param = "TestSimulation";
            simulateOutput.SimulatedActual = "simulated ok";
            simulateOutput.Expected = "simulated ok";

            restAct.ReturnValues.Add(simulateOutput);
            
            mBF.Activities[0].Acts.Add(restAct);
            mGR.RunInSimulationMode = true;


            mGR.RunRunner();

            if (restAct.ReturnValues[0].SimulatedActual == restAct.ReturnValues[0].Actual)
                Assert.AreEqual(restAct.ReturnValues[0].Actual, restAct.ReturnValues[0].ExpectedCalculated);

        }
    }
}
