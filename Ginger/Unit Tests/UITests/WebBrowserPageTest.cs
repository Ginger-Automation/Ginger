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


using System.Windows.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GingerTestHelper;


namespace UnitTests.UITests.WebBrowserPageTest
{
    [Ignore]
    [Level3]
    [TestClass]
    public class WebBrowserPageTest
    {
        //Rename Class to WebBrowserPageTest
        //Remove HTMLAgility everywhere

        mshtml.HTMLDocument mDocument = null;
        // HtmlAgilityPack.HtmlDocument hapDoc;
        bool DocLoaded = false;
        MiniBrowserWindow MBW;

        [TestCleanup()]
        public void TestCleanUp()
        {
            MBW.Close();
        }



        [TestInitialize]
        public void TestInitialize()
        {
            // Using HtmlAgilityPack to cross check the XPath
            string html = System.IO.File.ReadAllText(TestResources.GetTestResourcesFile( @"HTML\SCMCusotmersIndex.HTML"));
            //  hapDoc = new HtmlAgilityPack.HtmlDocument();
            // hapDoc.LoadHtml(html);

            //HtmlAgilityPack.HtmlNodeCollection

            //mimic our Internal Browser

            MBW = new MiniBrowserWindow();
            MBW.Show();
            DoEvents();
            MBW.browser.NavigateToString(html);
            MBW.browser.LoadCompleted += browser_LoadCompleted;

            while (!DocLoaded)
            {
                DoEvents();
            }
        }

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            mDocument = (mshtml.HTMLDocument)MBW.browser.Document;
            DocLoaded = true;
        }




        private void TestElementXPath(string DataQAValue, string FireBugXPath)
        {
            //Confirm FireBug provided a valid Xpath and item is found


            // Find the element by QA data tag value
            mshtml.IHTMLElement e = GetElementbyDataQA(DataQAValue);
            if (e == null)
            {
                throw new Exception("Element not found - " + DataQAValue);
            }
            // Calc the Xpath - this is what we want to test for accuracy and make sure it find the correct element           
            string XPath = MBW.GetElementXPath(e);

            mshtml.IHTMLElement n2 = MBW.GetElementByXPath(XPath);
            if (n2 == null)
            {
                throw new Exception("Element not found by XPath - " + XPath);
            }
            string QA2 = n2.getAttribute("data-QA");

            if (QA2 != DataQAValue)
            {
                throw new Exception("ERROR: QA2 != DataQAValue - " + QA2);
            }

            //Validate we found the same exact element based on QA tag - Uniqe
            Assert.AreEqual(QA2, DataQAValue);

            // Check if we created same XPath - can be differnt so fail only if QA2 != DataQAValue , else warning
            //TODO: Make warning - not fail!! can be several XPath to same element
            //Assert.AreEqual(FireBugXPath, XPath);  // False alarm so removed


        }

        [TestMethod]
        public void HomeLinkXPath()
        {
            //Act
            TestElementXPath("QAHome", ".//*[@id='home']");
        }

        [TestMethod]
        public void AdminLinkXPath()
        {
            //Act
            TestElementXPath("QAAdmin", ".//*[@id='menu']/li[2]/a");
        }

        [TestMethod]
        public void ItemInTable()
        {
            //Act
            TestElementXPath("QAItemInTable", ".//*[@id='body']/section/table/tbody[1]/tr[1]/td[3]");
        }



        private mshtml.IHTMLElement GetElementbyDataQA(string QAValue)
        {
            foreach (mshtml.IHTMLElement e in mDocument.all)
            {
                // Console.WriteLine(e.outerHTML);
                dynamic eDataQA = e.getAttribute("data-QA");
                if (!(eDataQA is DBNull))
                {
                    if (QAValue == eDataQA)
                    {
                        return e;
                    }
                }
            }
            //TODO: or err..
            return null;
        }

    }
}
