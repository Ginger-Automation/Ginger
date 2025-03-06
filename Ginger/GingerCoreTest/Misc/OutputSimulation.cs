#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions.WebServices;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
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
            mBF = new BusinessFlow
            {
                Activities = [],
                Name = "Output Simulation",
                Active = true
            };

            Platform p = new Platform
            {
                PlatformType = ePlatformType.WebServices
            };

            wsAgent = new Agent();
            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent
            {
                Agent = wsAgent
            };

            if (WorkSpace.Instance.Solution != null)
            {
                WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod = Ginger.Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile;
            }
            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [wsAgent];

            mGR.Executor.BusinessFlows.Add(mBF);

            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            if (WorkSpace.Instance.Solution == null)
            {
                WorkSpace.Instance.Solution = new Solution();
            }
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }

        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestMethod]
        [Timeout(60000)]
        public void SimulatedOuputGingerRunnerFlagOn()
        {


            Activity Activity2 = new Activity
            {
                Active = true,
                ActivityName = "Web API REST",
                CurrentAgent = wsAgent
            };
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eRequestContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eResponseContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            ActReturnValue simulateOutput = new ActReturnValue
            {
                Active = true,
                Param = "TestSimulation",
                SimulatedActual = "TestSimulation"
            };

            mBF.Activities[0].Acts.Add(restAct);
            mGR.RunInSimulationMode = true;

            mDriver.StartDriver();
            mGR.Executor.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "OK")
                        {
                            Assert.AreEqual(val.Actual, "OK");
                        }
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void SimulatedOuputActionFlagOn()
        {
            Activity Activity2 = new Activity
            {
                Active = true,
                ActivityName = "Web API REST",
                CurrentAgent = wsAgent
            };
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eRequestContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eResponseContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            restAct.SupportSimulation = true;

            ActReturnValue simulateOutput = new ActReturnValue
            {
                Active = true,
                Param = "TestSimulation",
                SimulatedActual = "TestSimulation"
            };

            mBF.Activities[0].Acts.Add(restAct);


            mDriver.StartDriver();
            mGR.RunInSimulationMode = true;
            mGR.Executor.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "OK")
                        {
                            Assert.AreEqual(val.Actual, "OK");
                        }
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                    {
                        Assert.Fail();
                    }
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void SimulatedOutputWithVETest()
        {
            Activity Activity2 = new Activity
            {
                Active = true,
                ActivityName = "Web API REST with VE",
                CurrentAgent = wsAgent
            };
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eRequestContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eResponseContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST Simulated Output VE";
            restAct.SupportSimulation = true;

            ActReturnValue simulateOutput = new ActReturnValue
            {
                Active = true,
                Param = "TestSimulation"
            };
            ValueExpression testVE = new ValueExpression(null, mBF)
            {
                Value = "simulated VE"
            };
            simulateOutput.SimulatedActual = testVE.Value;

            mBF.Activities[0].Acts.Add(restAct);


            mDriver.StartDriver();
            mGR.Executor.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if ((val.SimulatedActual != null) && (val.Actual != val.SimulatedActual))
                    {
                        if (val.Actual.ToString() == "simulated VE")
                        {
                            Assert.AreEqual(val.Actual, "simulated VE");
                        }
                    }
                    if ((val.SimulatedActual != null) && (val.Actual == val.SimulatedActual))
                    {
                        Assert.Fail();
                    }
                }
            }
        }


        [TestMethod]
        [Timeout(60000)]
        public void SimulatedOutputTest()
        {
            Activity Activity3 = new Activity
            {
                Active = true,
                ActivityName = "Web API REST Simulated Output",
                CurrentAgent = wsAgent
            };
            mBF.Activities.Add(Activity3);


            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eRequestContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eResponseContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST Simulated Output";
            restAct.SupportSimulation = true;

            //Define dummy return value
            ActReturnValue simulateOutput = new ActReturnValue
            {
                Active = true,
                Param = "TestSimulation",
                SimulatedActual = "simulated ok",
                Expected = "simulated ok"
            };

            restAct.ReturnValues.Add(simulateOutput);

            mBF.Activities[0].Acts.Add(restAct);
            mGR.RunInSimulationMode = true;


            mGR.Executor.RunRunner();

            if (restAct.ReturnValues[0].SimulatedActual == restAct.ReturnValues[0].Actual)
            {
                Assert.AreEqual(restAct.ReturnValues[0].Actual, restAct.ReturnValues[0].ExpectedCalculated);
            }
        }
    }
}
