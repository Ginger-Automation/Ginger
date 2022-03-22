#region License
/*
Copyright © 2014-2022 European Support Limited

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

using GingerCoreNET.RosLynLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerTestHelper;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GingerCore.Actions;
using Ginger.Run;
using System.IO;
using Amdocs.Ginger.CoreNET.Repository;
using GingerCoreNETUnitTest.RunTestslib;
using Ginger.SolutionGeneral;

namespace GingerCoreNETUnitTests.ValueExpressionTest
{
    [TestClass]
    public class ValueExpressionTest
    {
        RunSetConfig runset;
        GingerExecutionEngine runner;
        BusinessFlow mBF;
        ProjEnvironment mEnv;
        GingerCore.Activity mActivity;
        Act mAct;
        SolutionRepository mSolutionRepository;

        [TestInitialize]
        public void TestInitialize()
        {          
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            mSolutionRepository = WorkSpace.Instance.SolutionRepository;           
            string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
            mSolutionRepository.Open(TempRepositoryFolder);
            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution();
            sol.ContainingFolderFullPath = TempRepositoryFolder;
            WorkSpace.Instance.Solution = sol;
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
            mSolutionRepository.StopAllRepositoryFolderWatchers();

            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");
           
            runset = new RunSetConfig();
            runset.Name = "NewRunset1";
            WorkSpace.Instance.RunsetExecutor.RunSetConfig = runset;
            runner = new GingerExecutionEngine(new GingerRunner());
            runner.GingerRunner.Name = "Runner1";
            runner.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            WorkSpace.Instance.RunsetExecutor.Runners.Add(runner.GingerRunner);            
            mEnv = new ProjEnvironment();
            mEnv.Name = "Environment1";
            EnvApplication app1 = new EnvApplication();
            app1.Name = "App1";
            app1.Url = "URL123";
            mEnv.Applications.Add(app1);        
            GeneralParam GP1 = new GeneralParam();
            GP1.Name = "GP1";
            GP1.Value = "GP1Value";
            app1.GeneralParams.Add(GP1);

            mBF = new BusinessFlow();
            mBF.Name = "Businessflow1";
            runner.BusinessFlows.Add(mBF);
            mActivity = new GingerCore.Activity();
            mActivity.Active = true;
            mActivity.ActivityName = "Activity1";
            mAct = new ActDummy();
            mAct.Active = true;
            mAct.Description = "Action1";
            mActivity.Acts.Add(mAct);
            mActivity.Acts.CurrentItem = mAct;
            mBF.AddActivity(mActivity);


            BusinessFlow BF1 = new BusinessFlow();
            BF1.Name = "Businessflow2";
            runner.BusinessFlows.Add(BF1);
            GingerCore.Activity activity = new GingerCore.Activity();
            activity.Active = true;
            activity.ActivityName = "Activity1";
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Description = "Dummy1";
            activity.Acts.Add(dummy);
            activity.Acts.CurrentItem = dummy;
            BF1.AddActivity(activity);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mSolutionRepository.StopAllRepositoryFolderWatchers();
        }


        [TestMethod]
        [Timeout(60000)]
        public void SimpleSumEval()
        {
            //Arrange            
            string s = "1+2";

            //Act
            CodeProcessor SCT = new CodeProcessor();
            int rc = (int)SCT.EvalExpression(s);

            //Assert
            Assert.AreEqual(3, rc);
        }


        [TestMethod]
        [Timeout(60000)]
        public void EvalDateTime()
        {

            string dt = DateTime.Now.ToString("M/d/y");
            //Arrange            
            string s = "using System;  DateTime.Now.ToString(\"M/d/y\")";

            //Act
            CodeProcessor SCT = new CodeProcessor();
            string rc = (string)SCT.EvalExpression(s);

            //Assert
            Assert.AreEqual(rc, dt);
        }

        [TestMethod]
        [Timeout(60000)]
        public void SumForLoopl()
        {
            //Arrange            
            string s = "int t=0;  for(int i=0;i<5;i++) { t+=i;}; t  ";

            //Act
            CodeProcessor SCT = new CodeProcessor();
            int rc = (int)SCT.EvalExpression(s);

            //Assert
            Assert.AreEqual(10, rc);
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void Comparison()
        {
            //Arrange                      
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{CS Exp=20==10}";

            //Act
            bool v = Convert.ToBoolean(VE.ValueCalculated);

            //Assert
            Assert.AreEqual(false, v);
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void LogicalOperationAND()
        {
            //Arrange                      
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{CS Exp=20>10 && 9<10}";

            //Act
            bool v = Convert.ToBoolean(VE.ValueCalculated);

            //Assert
            Assert.AreEqual(true, v);
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void LogicalOperationOR()
        {
            //Arrange                        
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{CS Exp=5>6 || 4<5}";

            //Act
            bool v = Convert.ToBoolean(VE.ValueCalculated);

            //Assert
            Assert.AreEqual(true, v);
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void MachineName()
        {
            //Arrange    
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{CS Exp=System.Environment.MachineName}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(System.Environment.MachineName, v);
        }


        #region Flow Details
        [TestMethod]
        [Timeout(60000)]
        public void EnvironmentName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=Environment Field=Name}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Environment1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void BusinessFlowName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=BusinessFlow Field=Name}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Businessflow1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActivityName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=Activity Field=ActivityName}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Activity1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=Action Field=Description}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Action1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RunsetName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=Runset Field=Name}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "NewRunset1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RunnerName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=Runner Field=Name}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Runner1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void PreviousBusinessflowName()
        {
            //Arrange  
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Value = "{FD Object=PreviousBusinessFlow Field=Name}";
            runner.BusinessFlows[1].Activities[0].Acts.Add(dummy);

            //Act             
            runner.RunRunner();
            string v = dummy.ValueForDriver;

            //Assert
            Assert.AreEqual(v, "Businessflow1");
        }


        [TestMethod]
        [Timeout(60000)]
        public void PreviousBusinessflowStatus()
        {
            //Arrange              
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Value = "{FD Object=PreviousBusinessFlow Field=RunStatus}";
            runner.BusinessFlows[1].Activities[0].Acts.Add(dummy);

            //Act                
            runner.RunRunner();
            string v = dummy.ValueForDriver;

            //Assert
            Assert.AreEqual(v, "Passed");
        }

        [TestMethod]
        [Timeout(60000)]
        public void PreviousActivityName()
        {
            //Arrange  
            GingerCore.Activity activity = new GingerCore.Activity();
            activity.ActivityName = "Activity2";
            activity.Active = true;
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Value = "{FD Object=PreviousActivity Field=ActivityName}";
            activity.Acts.Add(dummy);
            mBF.AddActivity(activity);

            //Act                            
            runner.RunRunner();
            string v = dummy.ValueForDriver;

            //Assert
            Assert.AreEqual(v, "Activity1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void PreviousActionName()
        {
            //Arrange            
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Description = "Dummy1";
            dummy.Value = "{FD Object=PreviousAction Field=Description}";
            mActivity.Acts.Add(dummy);
           
            //Act                            
            runner.RunRunner();
            string v = dummy.ValueForDriver;

            //Assert
            Assert.AreEqual(v, "Action1");
        }


        [TestMethod]
        [Timeout(60000)]
        public void LastBusinessflowFailedName()
        {
            //Arrange           
            mAct.ActReturnValues.Add(new ActReturnValue() { Active = true, Actual = "a", Expected = "b" });
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Value = "{FD Object=LastFailedBusinessFlow Field=Name}";
            runner.BusinessFlows[1].Activities[0].Acts.Add(dummy);

            //Act              
            runner.RunRunner();
            string v = dummy.ValueForDriver;

            //Assert
            Assert.AreEqual(v, "Businessflow1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void LastActivityFailedName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=LastFailedActivity Field=ActivityName}";
            GingerCore.Activity activity = new GingerCore.Activity();
            activity.Active = true;
            activity.ActivityName = "Activity2";
            ActDummy dummy1 = new ActDummy();
            dummy1.Active = true;
            dummy1.Description = "Dummy action";
            activity.Acts.Add(dummy1);
            mBF.AddActivity(activity);
            mAct.ActReturnValues.Add(new ActReturnValue() { Active = true, Actual = "a", Expected = "b" });

            //Act            
            runner.RunRunner();
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Activity1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void LastActionFailedName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=LastFailedAction Field=Description}";
            ActDummy dummy1 = new ActDummy();
            dummy1.Active = true;
            dummy1.Description = "Dummy action1";
            mActivity.Acts.Add(dummy1);
            mAct.ActReturnValues.Add(new ActReturnValue() { Active = true, Actual = "a", Expected = "b" });

            //Act             
            runner.RunRunner();
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Action1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActivityName()
        {
            //Arrange  
            mActivity.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=ErrorHandlerOriginActivity Field=ActivityName}";
            mAct.ActReturnValues.Add(new ActReturnValue() { Active = true, Actual = "a", Expected = "b" });
            ErrorHandler errorHandler = new ErrorHandler();
            errorHandler.Active = true;
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Description = "Error Handler Dummy action";
            errorHandler.Acts.Add(dummy);
            mBF.AddActivity(errorHandler);

            //Act                        
            runner.RunRunner();
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Activity1");
        }


        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActionName()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{FD Object=ErrorHandlerOriginAction Field=Description}";
            GingerCore.Activity activity = new GingerCore.Activity();
            activity.Active = true;
            activity.ActivityName = "Error Handler Activity";
            ActDummy dummy1 = new ActDummy();
            dummy1.Active = true;
            dummy1.Description = "Dummy action";
            dummy1.ActReturnValues.Add(new ActReturnValue() { Active = true, Actual = "a", Expected = "b" });
            activity.Acts.Add(dummy1);
            activity.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;
            mBF.AddActivity(activity);
            ErrorHandler errorHandler = new ErrorHandler();
            errorHandler.Active = true;
            ActDummy dummy = new ActDummy();
            dummy.Active = true;
            dummy.Description = "Error Handler Dummy action";
            errorHandler.Acts.Add(dummy);
            mBF.AddActivity(errorHandler);

            //Act              
            runner.RunRunner();
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Dummy action");
        }

        #endregion Flow Details


        #region GeneralFunctions

        [TestMethod]
        [Timeout(60000)]
        public void GenerateHashCode()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GenerateHashCode(\"Hello\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "9/+ei3uy4Jtwk1pdeF4MxdnQq/A=");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GenerateHashCodeStringWithSpecialChar()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GenerateHashCode(\"He\"l\"lo\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "fjDK9jS31cZI2UYC/m/Fl4t/ibU=");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetEncryptedBase64String()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetEncryptedBase64String(\"Hello\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "SGVsbG8=");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetDecryptedBase64String()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetDecryptedBase64String(\"SGVsbG8=\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Hello");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetEncryptedBase64StringWithSpecialChar()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetEncryptedBase64String(\"He\"l\"lo\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "SGUibCJsbw==");
        }
        
        [TestMethod]
        [Timeout(60000)]
        public void GetDecryptedBase64StringWithSpecialChar()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetDecryptedBase64String(\"SGUibCJsbw==\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "He\"l\"lo");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetEncryptedBase64StringWithJson()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetEncryptedBase64String(\"{\"name\":\"John\", \"age\":31, \"city\":\"New York\"}\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "eyJuYW1lIjoiSm9obiIsICJhZ2UiOjMxLCAiY2l0eSI6Ik5ldyBZb3JrIn0=");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetDecryptedBase64StringWithJson()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=GetDecryptedBase64String(\"eyJuYW1lIjoiSm9obiIsICJhZ2UiOjMxLCAiY2l0eSI6Ik5ldyBZb3JrIn0=\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "{\"name\":\"John\", \"age\":31, \"city\":\"New York\"}");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceSpecialCharsFromStringWithNoCharsInside()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=ReplaceSpecialChars(\"Hello\",\",_)}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "Hello");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceSpecialCharsFromString()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=ReplaceSpecialChars(\"He\"l\"lo\",\",_)}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "He_l_lo");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceSpecialCharsFromJson()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=ReplaceSpecialChars(\"{\"name\":\"John\", \"age\":31, \"city\":\"New York\"}\",\",_)}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "{_name_:_John_, _age_:31, _city_:_New York_}");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceSpecialCharsFromJsonContainsSpecChars()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=ReplaceSpecialChars(\"{\"name\":\"Joh\"n\", \"age\":31, \"city\":\"New York\"}\",\",_)}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "{_name_:_Joh_n_, _age_:31, _city_:_New York_}");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NestedFunCalculateTest()
        {
            //Arrange    
            ValueExpression VE = new ValueExpression(mEnv, mBF);

            //Act
            VE.Value = @"UsernameToken-{Function Fun=GetHashCode({Function Fun=GetGUID()})}-abcd";

            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(true, v.EndsWith("-abcd"));
            Assert.AreEqual(true, v.StartsWith("UsernameToken-"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void NestedFunCalculateTestWihtReplacCharFunc()
        {
            //Arrange    
            ValueExpression VE = new ValueExpression(mEnv, mBF);

            //Act
            VE.Value = "{Function Fun=GetEncryptedBase64String(\"{Function Fun=ReplaceSpecialChars(\"Hel\"lo\",\",_)}\")}";
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual("SGVsX2xv", v);

        }

        [TestMethod]
        [Timeout(60000)]
        public void NestedFunCalculateTestWihtReplacCharFunc2()
        {
            //Arrange    
            ValueExpression VE = new ValueExpression(mEnv, mBF);

            //Act
            VE.Value = "{Function Fun=GetEncryptedBase64String(\"{Function Fun=ReplaceSpecialChars(\"{\"name\":\"John\",\"age\":31,\"city\":\"New York\"}\",\",_)}\")}";
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual("e19uYW1lXzpfSm9obl8sX2FnZV86MzEsX2NpdHlfOl9OZXcgWW9ya199", v);

        }

        [TestMethod]
        [Timeout(60000)]
        public void ReplaceSpecialCharsBackToJson()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Function Fun=ReplaceSpecialChars(\"{_name_:_John_, _age_:31, _city_:_New York_}\",_,\")}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "{\"name\":\"John\", \"age\":31, \"city\":\"New York\"}");
        }

        #endregion GeneralFunctions
    }
}
