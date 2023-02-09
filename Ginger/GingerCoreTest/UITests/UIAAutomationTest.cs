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


//using System.Collections.Generic;
//using System.Linq;
//
//using Amdocs.Ginger.Common;
//using GingerCore.Drivers;
//using GingerCore.Drivers.Common;
//using GingerCore.Actions.UIAutomation;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using GingerUnitTests.UIA_Form;
//using Ginger.Run;
//using GingerCore;
//using GingerCore.Platforms;
//using GingerCore.Drivers.WindowsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

//namespace UnitTestsNonUITests
//{
//    [TestClass]
//    public class UIAAutomationTest
//    {
//        static UIAComWrapperHelper UIA;
//        static XPathHelper mXPathHelper;
//        static AutomationElement AppWindow;
//        static FormEmbeddedBrowser uiaForm = new FormEmbeddedBrowser();
//        static WindowsDriver mDriver = null;
//        static GingerRunner mGR = null;
//        static BusinessFlow mBF;


//        [ClassInitialize]
//        public static void ClassInit(TestContext context)
//        {
//            mGR = new GingerRunner();

//            mBF = new BusinessFlow();
//            mBF.Activities = new ObservableList<Activity>();
//            mBF.Name = "BF UIAAutomationTest";
//            Platform p = new Platform();
//            p.PlatformType = ePlatformType.Windows;
//            mBF.Platforms = new ObservableList<Platform>();
//            mBF.Platforms.Add(p);
//            mBF.TargetApplications.Add(new TargetApplication() { AppName = "PBTestAPP" });
//            Activity activity = new Activity();
//            activity.TargetApplication = "PBTestApp";
//            mBF.Activities.Add(activity);
//            mBF.CurrentActivity = activity;
            
//            mDriver = new WindowsDriver(mBF);
//            mDriver.StartDriver();


//            UIA = (UIAComWrapperHelper)mDriver.mUIAutomationHelper;

//            mXPathHelper = ((IXPath)UIA).GetXPathHelper();

//            //TODO: launch Win Forms Test App
//            uiaForm.ShowInTaskbar = false;
//            uiaForm.Show();
//            List<object> list = UIA.GetListOfWindows();
//            UIA.SwitchToWindow("FormEmbeddedBrowser");
//            AppWindow = (AutomationElement)UIA.GetCurrentWindow();
//        }

//      //  [TestCleanup()]
//        public void TestCleanUp()
//        {
//            if (!uiaForm.IsDisposed && uiaForm.IsHandleCreated)
//            { uiaForm.Close(); }
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_load()
//        {

//            Assert.AreEqual(uiaForm.IsHandleCreated, true);
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void Find_Butto1_XPath_Using_Full_Absolutue_Path()
//        {
//            //Arrange
//            string ElementName = "button1";
//            string Button1Xpath = "/[AutomationId:tableLayoutPanel1]/[AutomationId:tableLayoutPanel2]/button1"; // Simple control with name on the root          
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, ElementName);
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            UIAElementInfo EI = new UIAElementInfo() { ElementObject = button1, WindowExplorer = UIA.WindowExplorer };
//            string XPath = mXPathHelper.GetElementXpathAbsulote(EI);
//            AutomationElement AEButton = UIA.GetElementByXpath(Button1Xpath);

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "button1=AEButton");
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void Find_Button1_ByName_Find_Anywhere()
//        {
//            //Arrange
//            string ElementName = "button1";
//            string ExpectedElementFullPath = "/[AutomationId:tableLayoutPanel1]/[AutomationId:tableLayoutPanel2]/button1";
//            string Button1Xpath = "button1"; // Simple control with name on the root, validate XPath that doesn't start with /         
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, ElementName);
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Descendants, cond);
//            //Act

//            string AbsoluteXPath = UIA.GetElementAbsoluteXPath(button1);

//            AutomationElement AEButton = UIA.GetElementByXpath(Button1Xpath);
//            string AbsoluteXPath2 = UIA.GetElementAbsoluteXPath(AEButton);
//            UIAElementInfo EI = new UIAElementInfo() { ElementObject = AEButton, WindowExplorer = UIA.WindowExplorer };
//            string SmartXPath = mXPathHelper.GetElementXpathSmart(EI);

//            //Assert
//            Assert.AreEqual(ExpectedElementFullPath, AbsoluteXPath, "ExpectedElementFullPath=AbsoluteXPath");
//            Assert.AreEqual(button1, AEButton, "button1=AEButton");
//            Assert.AreEqual(SmartXPath, "button1[AutomationId:button1][LocalizedControlType:button1]", "SmartXPath");  // We expect the smart XPath to find the name is unique
//        }

//        [TestMethod]  [Timeout(60000)]
     
//        public void Find_checkListBox_XPath_Using_Full_Absolutue_Path_andByAutoID()
//        {
//            //Arrange
//            string ElementName = "checkedListBox1";
//            string AElementXpath = "/[AutomationId:tableLayoutPanel1]/[AutomationId:tableLayoutPanel2]/[AutomationId:checkedListBox1]"; // Simple control with name on the root          
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, ElementName);
//            AutomationElement AElement1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            UIAElementInfo EI = new UIAElementInfo() { ElementObject = AElement1, WindowExplorer = UIA.WindowExplorer };
//            string XPath = mXPathHelper.GetElementXpathAbsulote(EI);
//            AutomationElement AElement = UIA.GetElementByXpath(AElementXpath);
//            string SmartXPath = mXPathHelper.GetElementXpathSmart(EI);

//            //Assert
//            Assert.AreEqual(XPath, AElementXpath, "XPath");
//            Assert.AreEqual(AElement1, AElement, "AElement=AElement");
//        //    Assert.AreEqual(SmartXPath, "button1[AutomationId:button1][LocalizedControlType:button1]", "SmartXPath");
//        }

//       // Generic sample to use forward
//       [TestMethod]  [Timeout(60000)]
//        public void Find_Nested_Checkbox1_XPath_Using_Element_Name()
//        {
//            //Arrange
//            string ElementXpath = "checkBox1";
//            string ElementName = "checkBox1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, ElementName);
//            AutomationElement elem = AppWindow.FindFirst(TreeScope.Descendants, cond);
//            //Act

//            string CalculatedXPath = UIA.GetElementAbsoluteXPath(elem);
//            AutomationElement AEElement = UIA.GetElementByXpath(ElementXpath);

//            //Assert
//            Assert.AreEqual(CalculatedXPath, "/[AutomationId:tableLayoutPanel1]/[AutomationId:tableLayoutPanel2]/checkBox1", "CalculatedXPath");
//            Assert.AreEqual(elem, AEElement, "elem=AEElement");
//        }





        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_label()
//        {
//            //Arrange
//            string Button1Xpath = "label1"; // Simple control with name on the root          
//            string ElementName = "label1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "label1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "label1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "label1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_linkLabel()
//        {
//            //Arrange
//            string Button1Xpath = "linkLabel1"; // Simple control with name on the root          
//            string ElementName = "linkLabel1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "linkLabel1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "linkLabel1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "linkLabel1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_listBox1()
//        {
//            //Arrange
//            string Button1Xpath = "listBox1"; // Simple control with name on the root          
//            string ElementName = "listBox1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "listBox1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//                string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "listBox1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "listBox1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_listView()
//        {
//            //Arrange
//            string Button1Xpath = "listView1"; // Simple control with name on the root          
//            string ElementName = "listView1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "listView1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "listView1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "listView1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_maskedTextBox()
//        {
//            //Arrange
//            string Button1Xpath = "maskedTextBox1"; // Simple control with name on the root          
//            string ElementName = "maskedTextBox1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "maskedTextBox1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "maskedTextBox1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "maskedTextBox1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_menuStrip()
//        {
//            //Arrange
//            string Button1Xpath = "menuStrip1"; // Simple control with name on the root          
//            string ElementName = "menuStrip1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "menuStrip1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "menuStrip1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "menuStrip1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_numericUpDown()
//        {
//            //Arrange
//            string Button1Xpath = "numericUpDown1"; // Simple control with name on the root          
//            string ElementName = "numericUpDown1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "numericUpDown1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "numericUpDown1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "numericUpDown1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_monthCalendar()
//        {
//            //Arrange
//            string Button1Xpath = "monthCalendar1"; // Simple control with name on the root          
//            string ElementName = "monthCalendar1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "monthCalendar1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "monthCalendar1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "monthCalendar1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_treeView()
//        {
//            //Arrange
//            string Button1Xpath = "treeView1"; // Simple control with name on the root          
//            string ElementName = "treeView1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "treeView1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "treeView1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "treeView1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_tabControl()
//        {
//            //Arrange
//            string Button1Xpath = "tabControl1"; // Simple control with name on the root          
//            string ElementName = "tabControl1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "tabControl1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "tabControl1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "tabControl1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_medXPathtextBox()
//        {
//            //Arrange
//            string Button1Xpath = "medXPathtextBox1"; // Simple control with name on the root          
//            string ElementName = "medXPathtextBox1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "medXPathtextBox1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "medXPathtextBox1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "medXPathtextBox1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_medXPathradioButton()
//        {
//            //Arrange
//            string Button1Xpath = "medXPathradioButton1"; // Simple control with name on the root          
//            string ElementName = "medXPathradioButton1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "medXPathradioButton1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "medXPathradioButton1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "medXPathradioButton1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_medXPathlabel()
//        {
//            //Arrange
//            string Button1Xpath = "medXPathlabel2"; // Simple control with name on the root          
//            string ElementName = "medXPathlabel2";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "medXPathlabel2");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "medXPathlabel2=AEButton");
//            Assert.AreEqual(button1, AEBtn, "medXPathlabel2=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_medXPathbutton()
//        {
//            //Arrange
//            string Button1Xpath = "medXPathbutton2"; // Simple control with name on the root          
//            string ElementName = "medXPathbutton2";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "medXPathbutton2");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "medXPathbutton2=AEButton");
//            Assert.AreEqual(button1, AEBtn, "medXPathbutton2=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_medXPathcheckBox()
//        {
//            //Arrange
//            string Button1Xpath = "medXPathcheckBox2"; // Simple control with name on the root          
//            string ElementName = "medXPathcheckBox2";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "medXPathcheckBox2");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "medXPathcheckBox2=AEButton");
//            Assert.AreEqual(button1, AEBtn, "medXPathcheckBox2=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_vScrollBar()
//        {
//            //Arrange
//            string Button1Xpath = "vScrollBar1"; // Simple control with name on the root          
//            string ElementName = "vScrollBar1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "vScrollBar1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "vScrollBar1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "vScrollBar1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_richTextBox()
//        {
//            //Arrange
//            string Button1Xpath = "richTextBox1"; // Simple control with name on the root          
//            string ElementName = "richTextBox1";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "richTextBox1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "richTextBox1=AEButton");
//            Assert.AreEqual(button1, AEBtn, "richTextBox1=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_deepXPathbutton()
//        {
//            //Arrange
//            string Button1Xpath = "deepXPathbutton3"; // Simple control with name on the root          
//            string ElementName = "deepXPathbutton3";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "deepXPathbutton3");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "deepXPathbutton3=AEButton");
//            Assert.AreEqual(button1, AEBtn, "deepXPathbutton3=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_deepXPathcheckBox()
//        {
//            //Arrange
//            string Button1Xpath = "deepXPathcheckBox3"; // Simple control with name on the root          
//            string ElementName = "deepXPathcheckBox3";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "deepXPathcheckBox3");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "deepXPathcheckBox3=AEButton");
//            Assert.AreEqual(button1, AEBtn, "deepXPathcheckBox3=AEBtn");
//        }


        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_deepXPathradioButton()
//        {
//            //Arrange
//            string Button1Xpath = "deepXPathradioButton2"; // Simple control with name on the root          
//            string ElementName = "deepXPathradioButton2";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "deepXPathradioButton2");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "deepXPathradioButton2=AEButton");
//            Assert.AreEqual(button1, AEBtn, "deepXPathradioButton2=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_deepXPathlabel()
//        {
//            //Arrange
//            string Button1Xpath = "deepXPathlabel3"; // Simple control with name on the root          
//            string ElementName = "deepXPathlabel3";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "deepXPathlabel3");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "deepXPathlabel3=AEButton");
//            Assert.AreEqual(button1, AEBtn, "deepXPathlabel3=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Form_deepXPathtextBox()
//        {
//            //Arrange
//            string Button1Xpath = "deepXPathtextBox2"; // Simple control with name on the root          
//            string ElementName = "deepXPathtextBox2";
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "deepXPathtextBox2");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(ElementName);
//            AutomationElement AEBtn = UIA.GetElementsByXpath(Button1Xpath).FirstOrDefault();

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "deepXPathtextBox2=AEButton");
//            Assert.AreEqual(button1, AEBtn, "deepXPathtextBox2=AEBtn");
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Button1_Xpath_Simple()
//        {
//            //Arrange
//            string Button1Xpath = "button1"; // Simple control with name on the root  
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, "button1");
//            AutomationElement button1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(button1);
//            AutomationElement AEButton = UIA.OLD_GetElementByXpath_OLD(Button1Xpath);

//            //Assert
//            Assert.AreEqual(XPath, Button1Xpath, "XPath");
//            Assert.AreEqual(button1, AEButton, "button1=AEButton");
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void VerifyHTMLFirstNameTextBox_XPath()
//        {
//            //Arrange

//            //Using partial XPath starting from tagframe
//            string XPath = @"[AutomationId:tbl_tagframe]\[LocalizedControlType:item[1]]\[LocalizedControlType:table]\[LocalizedControlType:item[3]]\[LocalizedControlType:table]\[LocalizedControlType:item[0]]\[LocalizedControlType:table]\[LocalizedControlType:item[3]]\[LocalizedControlType:table]\[LocalizedControlType:item]\[LocalizedControlType:table]\[LocalizedControlType:item[1]]\[AutomationId:standardAddress]\[LocalizedControlType:item[0]]\[LocalizedControlType:edit]";
//            AutomationElement AE = UIA.OLD_GetElementByXpath_OLD(XPath);

//            //Act

//            string XPath2 = UIA.GetElementAbsoluteXPath(AE);

//            //Assert
//            Assert.AreEqual(XPath, "button1", "XPath");  // Simple control with name on the root      
//        }

        
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Browser_Xpath_Simple()
//        {
//            //Arrange
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, "webBrowser1");
//            AutomationElement browser1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(browser1);

//            //Assert
//            Assert.AreEqual(XPath, "[AutomationId:webBrowser1]", "XPath");  // Simple control with no Name and AutomationID         
//        }

  
//        [TestMethod]  [Timeout(60000)]
//        public void Verify_Browser_Xpath_()
//        {
//            //Arrange
//            PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, "webBrowser1");
//            AutomationElement browser1 = AppWindow.FindFirst(TreeScope.Subtree, cond);
//            //Act

//            string XPath = UIA.GetElementAbsoluteXPath(browser1);

//            //Assert
//            Assert.AreEqual(XPath, "[AutomationId:webBrowser1]", "XPath");  // Simple control with no Name and AutomationID         
//        }
//    }
//}
