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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GingerTestHelper;
using GingerCore.Actions;
using GingerCore.Actions.XML;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.GingerOCR;

namespace GingerCoreTest.Misc
{
    [TestClass]
    public class OcrTest
    {
        string OcrPdfFilePath = TestResources.GetTestResourcesFile(@"OCR\OcrSample.pdf");
        string OcrImageFilePath = TestResources.GetTestResourcesFile(@"OCR\Capture-OCR.JPG");
        string OcrImageAllTextFilePath = TestResources.GetTestResourcesFile(@"OCR\Capture-OCR-AllText.JPG");
        string OcrPdfAllTextFilePath = TestResources.GetTestResourcesFile(@"OCR\ReadAllTextPdf.pdf");
        [TestMethod]
        public void ReadAllTextImage()
        {
            string txtOutput = GingerOcrOperations.ReadTextFromImage(OcrImageAllTextFilePath);
            string txtExpectedOutput = "Microsoft Teams meeting\n";
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }

        [TestMethod]
        public void ReadTextBetweenLabelsImage()
        {
            string txtOutput = GingerOcrOperations.ReadTextFromImageBetweenStrings(OcrImageFilePath,
                                                                                   "Version", "Steps");
            string txtExpectedOutput = ": 4.1\n\n";
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }

        [TestMethod]
        public void ReadTextAfterLabelImage()
        {
            string txtOutput = GingerOcrOperations.ReadTextFromImageAfterLabel(OcrImageFilePath, "Version");
            string txtExpectedOutput = ": 4.1\n\n";
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }

        [TestMethod]
        public void ReadAllTextPdf()
        {
            string txtOutput = GingerOcrOperations.ReadTextFromPdfSinglePage(OcrPdfAllTextFilePath, "1");
            string txtExpectedOutput = "Hi, try reading this text\n" + Environment.NewLine;
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }

        [TestMethod]
        public void ReadTextAfterLabelsPdf()
        {
            string txtOutput = GingerOcrOperations.ReadTextAfterLabelPdf(OcrPdfFilePath, "Processed By");
            string txtExpectedOutput = " : 107W0000\n";
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }

        [TestMethod]
        public void ReadTextBetweenLabelsPdf()
        {
            string txtOutput = GingerOcrOperations.ReadTextBetweenLabelsPdf(OcrPdfFilePath, "Installer", "Stock Issue Form No", string.Empty);
            string txtExpectedOutput = " : MOHD AZHARI BIN MAD ATARI (70020776) ";
            Assert.AreEqual(txtExpectedOutput, txtOutput);
        }
    }
}
