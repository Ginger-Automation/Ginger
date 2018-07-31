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

using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTests.NonUITests
{
    //[Ignore]
    //[TestClass]
    //public class HTMLReportLoggerTest 
    //{
    //    ExecutionLogger mExecutionLogger;
        
    //    Ginger.Run.GingerRunner GR;

    //    [TestInitialize]
    //    [Ignore]
    //    public void TestInitialize()
    //    {

    //        GingerRunner.UseExeuctionLogger = true; 

    //        GR = new Ginger.Run.GingerRunner();
    //        GR.Name = "GR1";

    //        //TODO: put back time stamp when it is in the Solution
    //        // in app save it to ExecutionResults
    //        // App.UserProfile.Solution.Folder + @"\ExecutionResults\Logs\" + Runset + "\" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff") + GR.Name + @"\";                        

    //        // in automate tab - save to: App.UserProfile.Solution.Folder + @"\ExecutionResults\Logs\Automate"  - no history            

    //        string LogsFolder = getGingerUnitTesterTempFolder() + "GingerExecutionFolder";
    //        System.IO.Directory.CreateDirectory(LogsFolder);

    //        mExecutionLogger = new ExecutionLogger(LogsFolder, GR);            
    //    }

    //    private BusinessFlow GetBF()
    //    {             
            
    //        BusinessFlow BF = new BusinessFlow();
    //        BF.Activities = new ObservableList<Activity>();
    //        BF.Name = "Create Customer";
    //        BF.Active = true;
    //        BF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });
    //        Platform p = new Platform();
    //        p.PlatformType = Platform.eType.Web;
    //        BF.Platforms = new ObservableList<Platform>();
    //        BF.Platforms.Add(p);

    //        Activity a1 = new Activity();
    //        a1.ActivityName = "Login";
    //        a1.Active = true;

    //        BF.Activities.Add(a1);

    //        ActGotoURL act1 = new ActGotoURL() { Description="Goto URL", LocateBy = Act.eLocatorType.NA, Value = "http://:8099/", Active = true };
    //        a1.Acts.Add(act1);

    //        ActTextBox act2 = new ActTextBox() { Description = "Enter User Name", LocateBy = Act.eLocatorType.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
    //        a1.Acts.Add(act2);

    //        ActTextBox act3 = new ActTextBox() { Description = "Enter Password", LocateBy = Act.eLocatorType.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
    //        a1.Acts.Add(act3);

    //        ActSubmit act4 = new ActSubmit() { Description = "Click Login", LocateBy = Act.eLocatorType.ByValue, LocateValue = "Log in", Active = true };
    //        a1.Acts.Add(act4);

    //        ActLink act5 = new ActLink() { Description = "Manage Customer",  LocateBy = Act.eLocatorType.ByLinkText, Wait = 1, LocateValue = "Manage Customer", LinkAction = ActLink.eLinkAction.Click, Active = true };
    //        a1.Acts.Add(act5);

    //        Activity a2 = new Activity();
    //        a2.Active = true;
    //        a2.ActivityName = "Goto URL";
    //        BF.Activities.Add(a2);

    //        ActGotoURL act11 = new ActGotoURL() { Description = "Goto URL", LocateBy = Act.eLocatorType.NA, Value = "http://:8099/", Active = true };
    //        a2.Acts.Add(act1);

    //        return BF;
    //    }

    //    [TestCleanup()]
    //    public void TestCleanUp()
    //    {
    //        GingerRunner.UseExeuctionLogger = false;
    //    }

    //    [TestMethod]
    //    public void SimulateRun()
    //    {
    //        //We simulate run

    //        int BFsCounter = 0;

    //        mExecutionLogger.ExecutionStart(null);

    //        //Create Sample BF
    //        BusinessFlow BF = GetBF();
    //        RunFlow(BF, ref BFsCounter);

    //        BusinessFlow BF2 = GetBF();            
    //        RunFlow(BF2, ref BFsCounter);

    //        BusinessFlow BF3 = GetBF();
    //        BF3.Name = "Customer Order Product";
    //        RunFlow(BF3, ref BFsCounter);

    //        mExecutionLogger.ExecutionEnd(null);
    //    }


    //    //[TestMethod]
    //    //[Ignore]
    //    //public void CreateHTMLReport()
    //    //{
    //    //    string TempFolder = getGingerUnitTesterTempFolder();
    //    //    ReportInfo RI = mExecutionLogger.GetReportInfo();

    //    //    
    //    //    string[] fileEntries = Directory.GetDirectories(TempFolder);
    //    //    foreach (string folderName in fileEntries)
    //    //    {
    //    //        FileAttributes attr = File.GetAttributes(folderName);
    //    //        string curFile = folderName + @"\BusinessFlow.txt";
    //    //        if (((attr & FileAttributes.Directory) == FileAttributes.Directory) && (File.Exists(curFile)))
    //    //        {
    //    //            BusinessFlowReport BFR1 = mExecutionLogger.LoadBusinessFlow(folderName + @"\BusinessFlow.txt");
    //    //            BFR1.ExecutionLoggerIsEnabled = true;
    //    //            RI.BusinessFlowReports.Add(BFR1);
    //    //        }
    //    //    }
    //    //    RI.ExecutionLoggerIsEnabled = true;

    //    //    //BusinessFlowReport BFR1 = mExecutionLogger.LoadBusinessFlow(TempFolder + @"GingerExecutionFolder1 Create Customer\BusinessFlow.txt");
    //    //    //RI.BusinessFlowReports.Add(BFR1);

    //    //    //BusinessFlowReport BFR2 = mExecutionLogger.LoadBusinessFlow(TempFolder + @"GingerExecutionFolder2 Create Customer\BusinessFlow.txt");
    //    //    //RI.BusinessFlowReports.Add(BFR2);

    //    //    //BusinessFlowReport BFR3 = mExecutionLogger.LoadBusinessFlow(TempFolder + @"GingerExecutionFolder3 Customer Order Product\BusinessFlow.txt");
    //    //    //RI.BusinessFlowReports.Add(BFR3);

    //    //    HTMLDrillDownReport l = new HTMLDrillDownReport(RI);


    //    //    //TODO: put back time stamp when it is saved to Solution folder
    //    //    // HTMLReportFolder = @"c:\temp\HTMLReport\" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff") + @"\";

    //    //    //for debug and browser refresh it is easier to use same file without timestamp
    //    //    // HTMLReportFolder = App.UserProfile.Solution.Folder + @"\ExecutionResults\Reports\" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff") + @"\";
    //    //    // System.IO.Directory.CreateDirectory(HTMLReportFolder);

    //    //    string OutputHTMLReportFolder = getGingerUnitTesterTempFolder() + @"DrillDownReport\";
    //    //    System.IO.Directory.CreateDirectory(OutputHTMLReportFolder);            

    //    //    string TemplatesFolder = getGingerEXEFileName() + @"Reports\GingerExecutionReport\";
    //    //    TemplatesFolder = TemplatesFolder.Replace("Ginger.exe", "");  //TODO: use io.path to removed the file name
    //    //    //l.CreateReport(TemplatesFolder, "Styles.css", OutputHTMLReportFolder);
    //    //}


    //    private void RunFlow(BusinessFlow BF, ref int BFsCounter)
    //    {
    //        BFsCounter++;
            
    //        mExecutionLogger.BusinessFlowStart(BF);

    //        //TODO: simulate running the same activity 3 times and show 

    //        // int ActivitiesCounter = 0;

    //        foreach (Activity a in BF.Activities)
    //        {
    //            RunActivity(BF, a);
    //        }

    //        //// Run the Activity[1] 3 times to show that we see history
    //        RunActivity(BF, BF.Activities[1]);
    //        RunActivity(BF, BF.Activities[1]);
    //        RunActivity(BF, BF.Activities[1]);

    //        BF.RunStatus = eRunStatus.Pass;

    //        // only for first flow we want to fail it, so we will have in the report failed BF to check the color of Pass/Fail style working good
    //        if (BFsCounter == 1)
    //        {
    //            // Create some fail 
    //            BF.Activities[1].Acts[0].Status = eRunStatus.Fail;
    //            BF.Activities[1].Status = eRunStatus.Fail;                
    //            BF.RunStatus = eRunStatus.Fail;
    //        }

            
    //        BF.Elapsed = BFsCounter * 1000 + 100;

    //        mExecutionLogger.BusinessFlowEnd(BF);  
    //    }

    //    private void RunActivity(BusinessFlow BF, Activity a)
    //    {

    //        // ActivitiesCounter++;

    //        // ActivityReport AR = new ActivityReport(a);
    //        // AR.Seq = ActivitiesCounter;
    //        mExecutionLogger.ActivityStart(BF, a);

    //        //Update Activity 
    //        a.Status = eRunStatus.Pass;
    //        a.Elapsed = 3412;
    //        foreach (Act act in a.Acts)
    //        {
                
    //            mExecutionLogger.ActionStart(a, act);
    //            //Update Action
    //            act.Status = eRunStatus.Pass;
    //            act.Elapsed = 1463;  // TODO: Create based on calc
    //            // act.AddScreenShot() //TODO: add screen shot

    //            mExecutionLogger.ActionEnd(a, act);
    //        }

    //        mExecutionLogger.ActivityEnd(BF, a);
    //    }
    //}
}
