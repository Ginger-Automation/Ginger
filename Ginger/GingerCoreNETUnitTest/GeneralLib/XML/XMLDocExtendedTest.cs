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

using Amdocs.Ginger.Common;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace GingerCoreNETUnitTest.GeneralLib.XML
{
    [TestClass]
    public class XMLDocExtendedTest
    {
        private static XMLDocExtended XDE = null;
        private static XmlDocument XDoc = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {

            string XmlfFilePath = TestResources.GetTestResourcesFile(@"XML\XmlDoc.xml");
            XDoc = new XmlDocument();
            XDoc.Load(XmlfFilePath);
            XDE = new XMLDocExtended(XDoc);
        }

        [Level2]
        [TestMethod]
        public void XMLDocGetAllNodesTest()

        {
            Assert.AreEqual(191, XDE.GetAllNodes().Count);
        }
        [Level2]
        [TestMethod]
        public void XMLDocGetTerminalNodesTest()

        {
            List<XMLDocExtended> lXD = new List<XMLDocExtended>();
            foreach (XMLDocExtended mXd in XDE.GetEndingNodes(false))
            {
                lXD.Add(mXd);
            }
            Assert.AreEqual(154, lXD.Count);
            foreach (XMLDocExtended mXd in XDE.GetEndingNodes(true))
            {
                lXD.Add(mXd);
            }
            Assert.AreEqual(308, lXD.Count);
        }

        [Level2]
        [TestMethod]
        public void XMLDocExtendedValidateXpathGenerated()

        {
            int max = XDE.GetAllNodes().Count;
            Random r = new Random();
            int rInt = r.Next(1, max);

          XMLDocExtended XDM=  XDE.GetAllNodes().ToArray()[rInt];

            XmlNamespaceManager XNS = XMLDocExtended.GetAllNamespaces(XDoc);

            Assert.AreSame(XDoc.SelectSingleNode(XDM.XPath,XNS), XDM.GetXmlNode());
        }

    }
}
