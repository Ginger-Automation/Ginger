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
using System.Diagnostics;
using System.Text;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using GingerCore.Platforms;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions.Common;


namespace UnitTests.UITests.ASCFDriverTest
{
    [TestClass]

    public class ASCFDriverTest 
    {/*

        BusinessFlow mBF;

        static Process proc;

        // make it static for reuse so no need to init every time when running test by click test button
        static ASCFDriver mDriver = null;
        private bool InitDone;

        [TestInitialize]
        public void TestInitialize()
        {
            // launch CRM from BAT file...        
          if (InitDone) return;

            //if (proc == null || proc.HasExited)
            //{
            //    proc = new Process();
            //    proc.StartInfo.FileName = @"PBGCRM8.bat";
            //    proc.StartInfo.WorkingDirectory = Common.getGingerUnitTesterDocumentsFolder() + @"ASCF\";
            //    proc.Start();
            //}
            
            //TODO: wait for user to launch - temp

            if (mDriver == null)
            {
                mBF = new BusinessFlow();
                mBF.Activities = new ObservableList<Activity>();
                mBF.Name = "BF Test ASCF Driver";
                Platform p = new Platform();
                p.PlatformType = Platform.eType.ASCF;
                mBF.Platforms = new ObservableList<Platform>();
                mBF.Platforms.Add(p);
            
                mBF.Activities.Add(new Activity());

            
                mDriver = new ASCFDriver(mBF, "ASCF Driver Test");
                mDriver.GingerToolBoxHost = "127.0.0.1";
                mDriver.GingerToolBoxPort = 7777;                
                mDriver.bClosing = false;
                mDriver.StartDriver();

                int cnt = 0;
                
                while (!mDriver.IsRunning() && cnt < 100)
                {
                    System.Threading.Thread.Sleep(500);
                    General.DoEvents();
                    cnt++;
                }
                // string status = d.LaunchForm("com.amdocs.ginger.AllControls");
                string status = mDriver.LaunchForm("com.amdocs.ginger.BasicControls");

                mDriver.SetHighLightMode(true);

               Assert.AreEqual(status, "OK");
            }

            InitDone = true;

        }

        [TestCleanup()]
        public void TestCleanUp()
        {
            string rc = mDriver.Send("CloseWindow", "ByName", "Application", " ", " ", true);

            mDriver.CloseDriver();
            mDriver = null;

            //close CRM
            if (proc!= null && !proc.HasExited)
            {
                proc.Kill();
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SetTextField_FirstName_Value()
        {           
            //Arrange            
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;            
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;            
            c.LocateValueCalculated = "FirstNameTextBox";         
            c.ValueForDriver =  "Jenny";

            //Act
            mDriver.RunAction(c);
            
            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.IsTrue(c.ExInfo.StartsWith("OK|Jenny set"), "c.ExInfo.StartsWith 'Jenny set'");
        }

        [TestMethod]  [Timeout(60000)]
        public void DriverCommunicationSpeedTest()
        {
            //Arrange            
            ActASCFControl act = new ActASCFControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;
            act.LocateValueCalculated = "FirstNameTextBox";
            int PassCounter = 0;

            //Act
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 1; i <= 10; i++)
            {
                act.ValueForDriver = "A" + i;
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                mDriver.RunAction(act);
                // General.DoEvents();
                sw.Stop();
                Console.WriteLine("#" + i + " - " + sw.ElapsedMilliseconds);
                if (act.Status == eRunStatus.Passed) PassCounter++;
            }
            st.Stop();

            //Assert            
           Assert.AreEqual(PassCounter, 10, "Passed Counter = 10");
            Assert.IsTrue(st.ElapsedMilliseconds < 700, "Elapsed < 300");
        }

        [TestMethod]  [Timeout(60000)]
        public void ClickButton_MessageButton()
        {           
            //Arrange            

            // Click  Message button - will pop up exeption message err on CRM
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;            
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Click;
            c.LocateValueCalculated = "MessageButton";


            // Get the label to make sure it happened
            ActASCFControl MSG_Label = new ActASCFControl();
            MSG_Label.LocateBy = eLocateBy.ByName;
            MSG_Label.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty;
            MSG_Label.ControlProperty = ActASCFControl.eControlProperty.Value;
            MSG_Label.LocateValueCalculated = "MSG_Label";

            // close the msgbox window
            // FIX me: zzz
            //ActASCFControl MSG_Close = new ActASCFControl();
            //MSG_Label.LocateBy = Act.eLocatorType.ByName;
            //MSG_Label.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty;
            //MSG_Label.ControlProperty = ActASCFControl.eControlProperty.Value;
            //MSG_Label.LocateValueCalculated = "MSG_Label";            

            //Act
            mDriver.RunAction(c);
            mDriver.RunAction(MSG_Label);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status" );
           Assert.AreEqual(MSG_Label.ExInfo, "OK|MSG Button Clicked!", "MSG_Label.ExInfo");            
        }

        

        [TestMethod]  [Timeout(60000)]
        public void GetLabelValue_CustomerIDLabel()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty;
            c.ControlProperty = ActASCFControl.eControlProperty.Value;
            c.LocateValueCalculated = "CustomerIDLabel";            

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.ExInfo, "OK|Customer ID: 125671", "Ex Info");
        }

        [TestMethod]  [Timeout(60000)]
        public void TakeScreenShot()
        {
            // Make sure we take screen shot only of CRM screen not the main screen, need to use C# to take Screen shot and then compare!?
            // Make sure it is not to high resolution
            // Make sure elapsed is less than 200ms

            //Arrange
            ActScreenShot a = new ActScreenShot();
            
            //Act
            Stopwatch st = new Stopwatch();
            st.Start();
            mDriver.RunAction(a);
            st.Stop();

            //Assert
           Assert.AreEqual(a.Status, eRunStatus.Passed, "Action Status");
            Assert.IsTrue(a.ScreenShots.Count != 0, "a.ScreenShot != 0");
            //Assert.AreEqual(a.ScreenShot.Size.Width, 800, "a.ScreenShot.Size.Width");
            //Assert.AreEqual(a.ScreenShot.Size.Height, 600, "a.ScreenShot.Size.Height");
            
            //TODO find alternative 
            //Assert.Isrue(st.ElapsedMilliseconds < 500, "Action Elapsed < 5", Assert.AssertType.Warning);
            
        }

        [TestMethod]  [Timeout(60000)]
        public void GetDateTimeValue_OrderDateTime()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty;
            c.ControlProperty = ActASCFControl.eControlProperty.DateTimeValue;
            c.LocateValueCalculated = "OrderDateTime";

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.ExInfo, "2015-07-28 21:09" , "ExInfo");
        }

        [TestMethod]  [Timeout(60000)]
        public void GetComboBoxText()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty;
            c.ControlProperty = ActASCFControl.eControlProperty.Text;
            c.LocateValueCalculated = "StateComboBox";

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string v = c.GetReturnParam("Value");
           Assert.AreEqual(v, "a", "Actual = a");
            Assert.IsTrue(c.ExInfo.StartsWith("OK|a"), "ExInfo");
        }

        [TestMethod]  [Timeout(60000)]
        public void SetComboBoxText()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;
            c.ControlProperty = ActASCFControl.eControlProperty.Text;
            c.LocateValueCalculated = "StateComboBox";
            c.ValueForDriver = "c";

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.IsTrue(c.ExInfo.StartsWith("OK|c set"), "ExInfo");
        }


        [TestMethod]  [Timeout(60000)]
        public void SetDateTime_OrderDateTime()
        {           
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;            
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;
            c.ControlProperty = ActASCFControl.eControlProperty.DateTimeValue;
            c.LocateValueCalculated = "OrderDateTime";            
            c.ValueForDriver = "2016-05-28 16:07";
            
            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.IsTrue(c.ExInfo.StartsWith("OK|1464469620000 set"), "ExInfo");
        }

        [TestMethod]  [Timeout(60000)]
        public void GridFormGetValue()
        {            
            //Arrange

            string status = mDriver.LaunchForm("com.amdocs.ginger.GridForm");

            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.GetControlProperty ;
            c.ControlProperty = ActASCFControl.eControlProperty.Text;
            c.LocateValueCalculated = "myGrid/[title=\"Label\"]:[[title=\"TextArea\"]:[value=~\"dfsas\"]]";
            
            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string v = c.GetReturnParam("Value");
           Assert.AreEqual(v, "fa", "Value = fa");
            Assert.IsTrue(c.ExInfo.StartsWith("OK|fa"), "ExInfo");
        }

        [TestMethod]  [Timeout(60000)]
        public void GridFormSetValue()
        {
            //Arrange

            string status = mDriver.LaunchForm("com.amdocs.ginger.GridForm");

            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;
            c.ControlProperty = ActASCFControl.eControlProperty.Text;
            c.LocateValueCalculated = "myGrid/[title=\"TextField\"]:[[title=\"Label\"]:[value=~\"fda\"]]";
            c.ValueForDriver = "ABCDE";

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");            
            Assert.IsTrue(c.ExInfo.StartsWith("OK|ABCDE set"), "ExInfo");
        }
        
        [TestMethod]  [Timeout(60000)]
        public void SetFieldNotFound_Check_Action_Fail()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.SetValue;
            c.LocateValueCalculated = "Control123";
            c.ValueForDriver = "AAA";

            //Act
            mDriver.RunAction(c);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Failed, "Action Status");
            Assert.IsTrue(c.ExInfo.StartsWith("Error|Control not found:  Control123"), "c.ExInfo.StartsWith 'Control not found:  Control123'");            
        }

        [TestMethod]  [Timeout(60000)]
        public void SelectCheckBoxFromAllControlsHGRid()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            if (status == "OK")
            {
                //Act
                ActASCFControl c = new ActASCFControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Click;                
                c.LocateValueCalculated = "HierarchicalGrid_3/[title=\"Select\"]:[[title=\"My Tree\"]:[value=~\"111\"]]";

                //Act
                mDriver.RunAction(c);

                //Assert            
               Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
                
            }
            else
            {
               Assert.AreEqual(status, "OK");
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void CollapseTreeFromAllControlsHGRid()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            if (status == "OK")
            {
                //Act
                ActASCFControl c = new ActASCFControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Collapse;
                c.LocateValueCalculated = "HierarchicalGrid_3/[name=\"_tree\"]:[name=\"_tree\"]:[path=\"Node:Amdocs my test for word wrap 1:Barry-abc-def-ghi\"]]";

                //Act
                mDriver.RunAction(c);

                //Assert            
               Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");

            }
            else
            {
               Assert.AreEqual(status, "OK");
            }
        }
         [TestMethod]  [Timeout(60000)]
        public void ExpandTreeFromAllControlsHGRid()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            if (status == "OK")
            {
                //Act
                ActASCFControl c = new ActASCFControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Expand;
                c.LocateValueCalculated = "HierarchicalGrid_3/[name=\"_tree\"]:[name=\"_tree\"]:[path=\"Node:Amdocs my test for word wrap 1:Barry-abc-def-ghi\"]]";

                //Act
                mDriver.RunAction(c);

                //Assert            
               Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");

            }
            else
            {
               Assert.AreEqual(status, "OK");
            }
        }
         [TestMethod]  [Timeout(60000)]
         public void ClickButtonFromAllControlsHGRid()
         {
             //Arrange            
             string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

             if (status == "OK")
             {
                 //Act
                 ActASCFControl c = new ActASCFControl();
                 c.LocateBy = eLocateBy.ByName;
                 c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Click;
                 c.LocateValueCalculated = "HierarchicalGrid_3/[title=\"Button\"]:[[title=\"My Tree\"]:[value=~\"Tommy\"]]";

                 //Act
                 mDriver.RunAction(c);

                 //Assert            
                Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");

             }
             else
             {
                Assert.AreEqual(status, "OK");
             }
         }
        [TestMethod]  [Timeout(60000)]
        public void SelectRadioButtonFromAllControlsHGRid()
        {
            //Arrange            
            
           string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            if (status == "OK")
            {
                //Act
                ActASCFControl c = new ActASCFControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Click;                
                c.LocateValueCalculated = "HierarchicalGrid_3/[title=\"Radio\"]:[[title=\"My Tree\"]:[value=~\"Nortel\"]]";

                //Act
                mDriver.RunAction(c);

                //Assert            
               Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
                
            }
            else
            {
               Assert.AreEqual(status, "OK");
            }
        }


        [TestMethod]  [Timeout(60000)]
        public void MultiColumnsConditionsSelectFromHierarchicalGrid()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            if (status == "OK")
            {
                //Act
                ActASCFControl c = new ActASCFControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.Click;

                // [
                // [col4:[value="hi"][enabled="true"]]
                // [col5:[type="UifCheckBox"][value="true"]]
                // ]

                string conditions = "[";
                conditions +=  "[[title=\"My Tree\"]:[value=~\"Tommy\"]]";
                conditions +=  "[[title=\"Site ID\"]:[text=~\"Long\"]]";
                conditions +=  "]";
                
                c.LocateValueCalculated = "HierarchicalGrid_3/[title=\"Select\"]:" + conditions;

                //Act
                mDriver.RunAction(c);

                //Assert            
               Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");

            }
            else
            {
               Assert.AreEqual(status, "OK");
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void Echo()
        {
            //Arrange            

            //Act
            string RC = mDriver.Echo("Hello");

            //Assert            
           Assert.AreEqual(RC, "OK|Hello", "RC");
        }

        [TestMethod]  [Timeout(60000)]
        public void Echo_1_to_200000()
        {
            //Target of this test is to check sending and receiving of packets different sizes, ince the first 4 bytes used as int length we verify the calc and parsing on both sides

            //Arrange   
            StringBuilder sb = new StringBuilder(200000);
            

            for (int i=0;i<200000;i++)
            {
                sb.Append("A");
                if (sb.Length % 10000 == 0)  // send packet every 100 chars
                {
                    string s = sb.ToString();
                    //Act
                    string RC = mDriver.Echo(s);

                    //Assert            
                   Assert.AreEqual(RC, "OK|" + s, "RC");
                }
            }
            
        }

        [TestMethod]  [Timeout(60000)]
        public void Echo_Special_Letters1()
        {
            //Arrange   
            //TODO: add more special chars
            
            // ~ is important to check as it is used as seperator

            string s = @" +()*|\/)|),=^?:$<>[]~";

            //Act
            string RC = mDriver.Echo(s);

            //Assert            
           Assert.AreEqual(RC, "OK|" + s, "RC");
            
        }


        [TestMethod]  [Timeout(60000)]
        public void Echo_Spanish_Letter()
        {
            //Arrange   
            //TODO: add more special chars


            // Not working: ~ - FIXME!!!
            string s = "Spanish áé";

            //Act
            string RC = mDriver.Echo(s);

            //Assert            
           Assert.AreEqual(RC, "OK|" + s, "RC");

        }

        


        [TestMethod]  [Timeout(60000)]
        public void CreateContact()
        {
            //Arrange            

            // Activity a = new Activity();

            // Do create contact in one action - runs better
            ActMenuItem AMI_menuCreate = new ActMenuItem();
            AMI_menuCreate.LocateBy = eLocateBy.ByTitle;
            AMI_menuCreate.LocateValueCalculated = "Create/Contact";

            // Click Create Menu            
            //ActMenuItem AMI_menuCreate = new ActMenuItem();
            //AMI_menuCreate.LocateBy = Act.eLocatorType.ByName;
            //AMI_menuCreate.LocateValueCalculated = "menuCreate";            

            //// Click Create Contact            
            //ActMenuItem AMI_menuItemCreateContact = new ActMenuItem();
            //AMI_menuItemCreateContact.LocateBy = Act.eLocatorType.ByName;
            //AMI_menuItemCreateContact.LocateValueCalculated = "menuItemCreateContact";            

            // Set First Name
            VariableRandomString VRS = new VariableRandomString() {Min = 4, Max = 15 };
            //VRS.ResetValue();
            VRS.GenerateAutoValue();
            string FirstName = "Yaron_" + VRS.Value;
            ActASCFControl AAC_SetFirstName = new ActASCFControl() { ControlAction = ActASCFControl.eControlAction.SetValue,
                            LocateBy = eLocateBy.ByName, LocateValueCalculated = "first_name", ValueForDriver = FirstName };

            // Set Last Name
            ActASCFControl AAC_SetLastName = new ActASCFControl() { ControlAction = ActASCFControl.eControlAction.SetValue,
                            LocateBy = eLocateBy.ByName, LocateValueCalculated = "last_name", ValueForDriver = "Weiss" };

            // Set Last Name
            ActASCFControl AAC_SetAddress = new ActASCFControl() { ControlAction = ActASCFControl.eControlAction.SetValue,
                                                         LocateBy = eLocateBy.ByName,
                                                         LocateValueCalculated = "address",
                                                         ValueForDriver = "2109 Fox Dr"};

            ActASCFControl AAC_SetCity = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.SetValue,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "city",
                ValueForDriver = "Champaign"
            };

            // Set Drop down
            ActASCFControl AAC_DropDown_SaveAndViewDetails = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.SetValue,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "SaveOptionComboBox",
                ValueForDriver = "Save and View Details"
            };

            // Click Save button
            ActASCFControl AAC_ClickSaveButton = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.Click,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "Save",                
            };

            // Click Tab Contact Identifier
            ActASCFControl AAC_Click_Contact_identifier = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.Click,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "cv150",                
            };

            // Click Tab Subscriptions
            ActASCFControl AAC_Click_TabSubscriptions = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.Click,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "assigned_services_tab",                
            };

            // Get Contact ID number
            ActASCFControl AAC_GetContactIdFromLabel = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.GetControlProperty,
                LocateBy = eLocateBy.ByName,
                ControlProperty = ActASCFControl.eControlProperty.Value,
                LocateValueCalculated = "contact_id",
            };

            //Search with one action
            ActMenuItem AMI_menuSearch = new ActMenuItem();
            AMI_menuSearch.LocateBy = eLocateBy.ByTitle;
            AMI_menuSearch.LocateValueCalculated = "Search/Contacts";

            // Click Create Menu            
            //ActMenuItem AMI_menuSearch = new ActMenuItem();
            //AMI_menuSearch.LocateBy = Act.eLocatorType.ByName;
            //AMI_menuSearch.LocateValueCalculated = "menuSearch";            

            // Click Create Contact            
            //ActMenuItem AMI_menuItemSeachContacts = new ActMenuItem();
            //AMI_menuItemSeachContacts.LocateBy = Act.eLocatorType.ByName;
            //AMI_menuItemSeachContacts.LocateValueCalculated = "menuItemSearchContacts";            

            ActASCFControl AAC_adhocValue_col_col_first_name_SetFirstNameSearch = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.SetValue,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "adhocValue_col_col_first_name",
                ValueForDriver = FirstName.Substring(0,7)  // Take first 7 chars for search so we can get mre records
            };

            // Click Tab Subscriptions
            ActASCFControl AAC_ClickButton_btnSearchNow = new ActASCFControl()
            {
                ControlAction = ActASCFControl.eControlAction.Click,
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "btnSearchNow"
            };

            //TODO: find the row with same name


            //Fixme
            //Worked on Vu
            //string locator = "gridListing/[title=\"Select\"]:[[title=\"First Name\"]:[value=\"" + FirstName + "\"]]";

            // Not working on Everest since there is no slect column
            //string locator = "gridListing/[[title=\"First Name\"]:[value=\"" + FirstName + "\"]]";
            //ActActivateRow AAR = new ActActivateRow() { LocateBy = Act.eLocatorType.ByName, LocateValueCalculated = locator };

            // Activate row and compare contact id

            //Act            
            mDriver.RunAction(AMI_menuCreate);
           //mDriver.RunAction(AMI_menuItemCreateContact);
            mDriver.RunAction(AAC_SetFirstName);
            mDriver.RunAction(AAC_SetLastName);
            mDriver.RunAction(AAC_SetAddress);
            mDriver.RunAction(AAC_SetCity);
            mDriver.RunAction(AAC_DropDown_SaveAndViewDetails);
            mDriver.RunAction(AAC_ClickSaveButton);
            mDriver.RunAction(AAC_Click_Contact_identifier);
            mDriver.RunAction(AAC_Click_TabSubscriptions);
            mDriver.RunAction(AAC_GetContactIdFromLabel);
            mDriver.RunAction(AMI_menuSearch);
            //mDriver.RunAction(AMI_menuItemSeachContacts);
            mDriver.RunAction(AAC_adhocValue_col_col_first_name_SetFirstNameSearch);
            mDriver.RunAction(AAC_ClickButton_btnSearchNow);
            //mDriver.RunAction(AAR);
            

            //Assert                        
           Assert.AreEqual(AMI_menuCreate.Status, eRunStatus.Passed, "AMI_menuCreate");
            //Assert.AreEqual(AMI_menuItemCreateContact.Status, eRunStatus.Pass, "AMI_menuItemCreateContact");
           Assert.AreEqual(AAC_SetFirstName.Status, eRunStatus.Passed, "AAC_SetFirstName");
           Assert.AreEqual(AAC_SetLastName.Status, eRunStatus.Passed, "AAC_SetLastName");
           Assert.AreEqual(AAC_SetAddress.Status, eRunStatus.Passed, "AAC_SetAddress");
           Assert.AreEqual(AAC_SetCity.Status, eRunStatus.Passed, "AAC_SetCity");
           Assert.AreEqual(AAC_DropDown_SaveAndViewDetails.Status, eRunStatus.Passed, "AAC_DropDown_SaveAndViewDetails");
           Assert.AreEqual(AAC_ClickSaveButton.Status, eRunStatus.Passed, "AAC_ClickSaveButton");
           Assert.AreEqual(AAC_Click_Contact_identifier.Status, eRunStatus.Passed, "AAC_Click_Contact_identifier");
           Assert.AreEqual(AAC_Click_TabSubscriptions.Status, eRunStatus.Passed, "AAC_Click_TabSubscriptions");
           Assert.AreEqual(AAC_GetContactIdFromLabel.Status, eRunStatus.Passed, "AAC_GetContactIdFromLabel");
           Assert.AreEqual(AMI_menuSearch.Status, eRunStatus.Passed, "AMI_menuSearch");
            //Assert.AreEqual(AMI_menuItemSeachContacts.Status, eRunStatus.Pass, "AMI_menuItemSeachContacts");
           Assert.AreEqual(AAC_adhocValue_col_col_first_name_SetFirstNameSearch.Status, eRunStatus.Passed, "AAC_adhocValue_col_col_first_name_SetFirstNameSearch");
           Assert.AreEqual(AAC_ClickButton_btnSearchNow.Status, eRunStatus.Passed, "AAC_ClickButton_btnSearchNow");
            //Assert.AreEqual(AAR.Status, eRunStatus.Pass, "AAR.Status=Pass");

            
            // TODO: get contact id from DB/or screen - instead of hard coded 205...
            //Assert.AreEqual(AAC_GetContactIdFromLabel.ExInfo, "205", "contact id on screen = DB contact id");
        }


        //TODO:
        //test command err-  Error|Command is not defined:  Command123


        [TestMethod]  [Timeout(60000)]
        public void SwitchWindowByTitle()
        {
            //Arrange            
            // string status = mDriver.LaunchForm("com.amdocs.ginger.HierarchicalGridSelectAll");

            
            //Act
            ActSwitchWindow ASW = new ActSwitchWindow();
            ASW.LocateBy = eLocateBy.ByTitle;
            ASW.LocateValueCalculated = "Basic Controls";

            //Act
            mDriver.RunAction(ASW);

            //Assert            
           Assert.AreEqual(ASW.Status, eRunStatus.Passed, "Status");
            Assert.IsTrue(ASW.ExInfo.StartsWith("OK|Switch to form: Name=com.amdocs.ginger.BasicControls, Title=Basic Controls"), "ExInfo");
            
            
        }

        [TestMethod]  [Timeout(60000)]
        public void SwitchWindowByTitleNotExistWillErr()
        {
            //Arrange            
            

            //Act
            ActSwitchWindow ASW = new ActSwitchWindow();
            ASW.LocateBy = eLocateBy.ByTitle;
            ASW.LocateValueCalculated = "XZXZXZ";

            //Act
            mDriver.RunAction(ASW);

            //Assert            
           Assert.AreEqual(ASW.Status, eRunStatus.Failed, "Status");
           Assert.AreEqual(ASW.ExInfo, "Error|Form not found: ByTitle XZXZXZ", "ExInfo");

            //TODO: add elapsed sec should be more than 10 sec as we wait for window
        }


        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void SwitchWindowsByTitleX4()
        //{
        //    //Arrange            
        //    string status = mDriver.LaunchForm("com.amdocs.ginger.MultiForms");

        //    //Act
        //    ActSwitchWindow ASW = new ActSwitchWindow();
        //    ASW.LocateBy = Act.eLocatorType.ByTitle;
        //    ASW.LocateValueCalculated = "Jimmy James";

        //    //Act
        //    mDriver.RunAction(ASW);

        //    //Assert            
        //   Assert.AreEqual(ASW.Status, eRunStatus.Pass, "Status");
        //   Assert.AreEqual(ASW.ExInfo, "Switch to form: Name=com.amdocs.ginger.MultiForms, Title=Multi Form - Contact Jimmy James", "ExInfo");


        //}

        [TestMethod]  [Timeout(60000)]
        public void IsVisibleActionTest()
        {
            //Arrange
            ActASCFControl c = new ActASCFControl();
            c.ControlAction = GingerCore.Actions.ActASCFControl.eControlAction.IsVisible;
            c.LocateBy = eLocateBy.ByName;            
            c.LocateValueCalculated = "aaa";

            //Act
            mDriver.RunAction(c);           

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual((c.ReturnValues[0]).Param, "Actual", "Return Value Param Name");
           Assert.AreEqual((c.ReturnValues[0]).Actual, "False", "Return Value Param Value");
        }

        [TestMethod]  [Timeout(60000)]
        public void BrowserTestForm()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.BrowserTestForm");

            //Act
            
            //Act
        //    mDriver.RunAction(ASW);

            //Assert            
            


        }

        [TestMethod]  [Timeout(60000)]
        public void OMSFrameworkWidgetsTestForm()
        {
            //Arrange            
            string status = mDriver.LaunchForm("com.amdocs.ginger.OMSFrameworkWidgetsTestForm");

            //Act
            
            //Act
        //    mDriver.RunAction(ASW);

            //Assert            
            


        }

        */


        
    }
    
}
