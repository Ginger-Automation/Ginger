using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    class ActValidationTests
    {
        RunSetConfig runset;
        GingerRunner runner;
        BusinessFlow mBF;
        ProjEnvironment mEnv;
        GingerCore.Activity mActivity;
        ActValidation mAct;

        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            SolutionRepository mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
            mSolutionRepository.Open(TempRepositoryFolder);
            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution();
            sol.ContainingFolderFullPath = TempRepositoryFolder;
            WorkSpace.Instance.Solution = sol;

            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");

            runset = new RunSetConfig();
            runset.Name = "NewRunset1";
            WorkSpace.Instance.RunsetExecutor.RunSetConfig = runset;
            runner = new GingerRunner();
            runner.Name = "Runner1";
            runner.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            WorkSpace.Instance.RunsetExecutor.Runners.Add(runner);
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
            mAct = new ActValidation();
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
            ActValidation validation = new ActValidation();
            validation.Condition = "22 > 12";
            validation.Active = true;
            validation.Description = "condition equal";
            activity.Acts.Add(validation);
            activity.Acts.CurrentItem = validation;
            BF1.AddActivity(activity);
        }
        [TestCleanup]
        public void TestCleanUp()
        {

        }


        [TestMethod]
        [Timeout(60000)]
        public void ConditionEqual()
        {
            //Arrange            
            mAct.Condition = "2=2";

            //Act
            mAct.CalculateCondition(mBF, mEnv, mAct);
            runner.RunAction(mAct);

            //Assert
            Assert.AreEqual(mAct.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }
    }
}
