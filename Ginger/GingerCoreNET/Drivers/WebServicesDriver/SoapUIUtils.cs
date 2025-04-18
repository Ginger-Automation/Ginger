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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GingerCore.Drivers.WebServicesDriverLib
{
    public class SoapUIUtils
    {

        private const char Quotationmark = '\u0022';
        private ActSoapUI mAct;
        private string SoapUIDirectoryPath { get; set; }
        private string ReportExportDirectoryPath { get; set; }
        private string SoapUISettingFile { get; set; }
        private string SoapUISettingFilePassword { get; set; }
        private string ProjectPassword { get; set; }
        private string ReportPathWithXMLFolder { get; set; }
        string mTimestamp;
        public static string ReportPath { get; set; }

        public bool mRunSoapUIProcessAsAdmin { get; set; }
        public bool mSoapUIProcessRedirectStandardError { get; set; }
        public bool mSoapUIProcessRedirectStandardOutput { get; set; }
        public bool mSoapUIProcessUseShellExecute { get; set; }
        public bool mSoapUIProcessWindowStyle { get; set; }
        public bool mSoapUIProcessCreateNoWindow { get; set; }


        public SoapUIUtils(Act act, string soapUIDirectoryPath, string reportExportDirectoryPath, string soapUISettingFile, string soapUISettingFilePassword, string projectPassword, bool RunSoapUIProcessAsAdmin, bool SoapUIProcessRedirectStandardError, bool SoapUIProcessRedirectStandardOutput, bool SoapUIProcessUseShellExecute, bool SoapUIProcessWindowStyle, bool SoapUIProcessCreateNoWindow)
        {
            //initializing components.
            mAct = (ActSoapUI)act;
            SoapUIDirectoryPath = soapUIDirectoryPath;
            ReportExportDirectoryPath = reportExportDirectoryPath;
            SoapUISettingFile = soapUISettingFile;
            SoapUISettingFilePassword = soapUISettingFilePassword;
            ProjectPassword = projectPassword;

            mRunSoapUIProcessAsAdmin = RunSoapUIProcessAsAdmin;
            mSoapUIProcessRedirectStandardError = SoapUIProcessRedirectStandardError;
            mSoapUIProcessRedirectStandardOutput = SoapUIProcessRedirectStandardOutput;
            mSoapUIProcessUseShellExecute = SoapUIProcessUseShellExecute;
            mSoapUIProcessWindowStyle = SoapUIProcessWindowStyle;
            mSoapUIProcessCreateNoWindow = SoapUIProcessCreateNoWindow;

            //Creating string for the report extract folder
            string XMLFileName = Path.GetFileName(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile)).Replace(".xml", string.Empty);


            mTimestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string SolutionFolder = mAct.SolutionFolder;

            //SoapUIDirectoryPath = SoapUIDirectoryPath.Replace(@"~\", SolutionFolder);
            SoapUIDirectoryPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(SoapUIDirectoryPath);

            //Creating Directory to extract the reports.
            //string targetPath = ReportExportDirectoryPath.Replace(@"~\", SolutionFolder);
            string targetPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ReportExportDirectoryPath);

            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            //ReportPathWithXMLFolder = System.IO.Path.Combine(ReportExportDirectoryPath.Replace(@"~\", SolutionFolder), mAct.Description + mTimestamp);
            ReportPathWithXMLFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ReportExportDirectoryPath), mAct.Description + mTimestamp);

            ReportPath = ReportPathWithXMLFolder;
            Directory.CreateDirectory(ReportPath);
            mAct.LastExecutionFolderPath = ReportPath;
        }

        //returns bool if the command successfully created and return the command string as well.
        public bool Command(ref string commandParam)
        {
            try
            {
                if (mAct.AllProperties.Count == 0 && mAct.TestSuitePlaceHolder.Count == 0)
                {
                    //Getting the file XML to run.
                    string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);
                    if (XMLFiledValue[..1].Equals("~"))
                    {
                        XMLFiledValue = System.IO.Path.Combine(mAct.SolutionFolder, XMLFiledValue[2..]);
                    }
                    commandParam = Quotationmark + XMLFiledValue + Quotationmark;
                }
                else
                {
                    commandParam = Quotationmark + UpdateXMLProperty() + Quotationmark;
                }


                //The TestSuite to run, used to narrow down the tests to run
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite)))
                {
                    commandParam = commandParam + " -s" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite) + Quotationmark;
                }

                //The TestCase to run, used to narrow down the tests to run
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase)))
                {
                    commandParam = commandParam + " -c" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase) + Quotationmark;
                }
                //Enables SoapUI UI-related components, required if you use the UISupport class for prompting or displaying information
                bool mUIrelated;
                bool.TryParse((mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.UIrelated)), out mUIrelated);
                if (mUIrelated)
                {
                    commandParam = commandParam + " -i";
                }
                //Overrides the endpoint
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.EndPoint)))
                {
                    commandParam = commandParam + " -e" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.EndPoint) + Quotationmark;
                }
                //The host:port to use when invoking test-requests, overrides only the host part of the endpoint set in the project file
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.HostPort)))
                { commandParam = commandParam + " -h" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.HostPort) + Quotationmark; }

                //The username to use in any authentications, overrides any username set for any TestRequests
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Username)))
                { commandParam = commandParam + " -u" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Username) + Quotationmark; }

                //The password to use in any authentications, overrides any password set for any TestRequests
                string password = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Password);
                if (!string.IsNullOrEmpty(password))
                {
                    var decryptedPassword = General.DecryptPassword(password, ValueExpression.IsThisAValueExpression(password), mAct);
                    commandParam = commandParam + " -p" + Quotationmark + decryptedPassword + Quotationmark;
                }

                //The domain to use in any authentications, overrides any domain set for any TestRequests
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Domain)))
                { commandParam = commandParam + " -d" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Domain) + Quotationmark; }

                //Sets the WSS password type, either 'Text' or 'Digest'
                if (!(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.PasswordWSSType).Equals("")) && !string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.Password)))
                {
                    commandParam = commandParam + " -w" + Quotationmark + mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.PasswordWSSType) + Quotationmark;
                }
                //Sets system property with name=value
                if (mAct.SystemProperties.Any())
                {
                    foreach (ActInputValue row in mAct.SystemProperties)
                    {
                        commandParam = commandParam + " -D" + Quotationmark + row.ItemName + "=" + row.ValueForDriver + Quotationmark;
                    }
                }
                //Sets global property with name=value
                if (mAct.GlobalProperties.Any())
                {
                    foreach (ActInputValue row in mAct.GlobalProperties)
                    {
                        commandParam = commandParam + " -G" + Quotationmark + row.ItemName + "=" + row.ValueForDriver + Quotationmark;
                    }
                }
                //Sets the soapui-settings.xml file to use, required if you have custom proxy, ssl, http, etc setting
                if (!string.IsNullOrEmpty(SoapUISettingFile))
                {
                    commandParam = commandParam + " -t" + Quotationmark + SoapUISettingFile + Quotationmark;
                }

                // Sets password for soapui-settings.xml file
                if (!string.IsNullOrEmpty(SoapUISettingFilePassword))
                {
                    commandParam = commandParam + " -v" + Quotationmark + SoapUISettingFilePassword + Quotationmark;
                }

                //Sets project password for decryption if project is encrypted
                if (!string.IsNullOrEmpty(ProjectPassword))
                {
                    commandParam = commandParam + " -x" + Quotationmark + ProjectPassword + Quotationmark;
                }
                // r : Turns on printing of a small summary report (see below)
                // I : Do not stop if error occurs, ignore them: Execution does not stop if error occurs, but no detailed information about errors are stored to the log. (If you need full information about errors, do not use this option). 
                // a : Turns on exporting of all test results, not only errors
                // M : Creates a Test Run Log Report in XML format
                // f : Specifies the root folder to which test results should be exported (see below)
                commandParam = commandParam + " -r -I -a -M -f" + Quotationmark + ReportPath + Quotationmark;
                return true;
            }
            catch (Exception ex)
            {
                mAct.Error = "An Error Occurred while creating the command.";
                mAct.ExInfo = ex.Message;
                return false;
            }
        }

        //handling the back end batch process.
        public bool StartProcess(string command)
        {
            try
            {
                string commandParam = command;
                Process proc = new Process();
                proc.StartInfo.FileName = SoapUIDirectoryPath + @"\bin\testrunner.bat";
                proc.StartInfo.WorkingDirectory = SoapUIDirectoryPath + @"\bin";
                proc.StartInfo.Arguments = commandParam;
                if (mRunSoapUIProcessAsAdmin)
                {
                    proc.StartInfo.Verb = "runas";
                }

                proc.StartInfo.RedirectStandardError = mSoapUIProcessRedirectStandardError;
                proc.StartInfo.RedirectStandardOutput = mSoapUIProcessRedirectStandardOutput;
                proc.StartInfo.UseShellExecute = mSoapUIProcessUseShellExecute;
                if (mSoapUIProcessWindowStyle)
                {
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                proc.StartInfo.CreateNoWindow = mSoapUIProcessCreateNoWindow;

                string output = string.Empty;
                string error = string.Empty;

                if (mSoapUIProcessRedirectStandardError && mSoapUIProcessRedirectStandardOutput)
                {
                    proc.Start();

                    output = proc.StandardOutput.ReadToEnd();
                    error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    mAct.ExInfo = output;
                    return true;
                }
                else if (!mSoapUIProcessRedirectStandardError && mSoapUIProcessRedirectStandardOutput)
                {
                    proc.Start();
                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    mAct.ExInfo = output;
                    return true;
                }
                else if (mSoapUIProcessRedirectStandardError && !mSoapUIProcessRedirectStandardOutput)
                {
                    proc.Start();
                    error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    mAct.ExInfo = error;
                    return true;

                }
                else if (!mSoapUIProcessRedirectStandardError && !mSoapUIProcessRedirectStandardOutput)
                {
                    proc.Start();
                    proc.WaitForExit();
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                mAct.Error = "An Error occurred while running the process.";
                mAct.ExInfo = ex.Message;
                return false;
            }

        }

        public void Validation()
        {
            //Handling test result from report XML file.
            //creating the report path string
            bool ignoreValidation;
            bool.TryParse(mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.IgnoreValidation), out ignoreValidation);
            if (ignoreValidation)
            {
                return;
            }

            string testCaseRunLogReport = ReportPath + "\\test_case_run_log_report.xml";

            //checking if the content is less then 2 means the run was failed to execute if bigger then 2 means one txt file at lease was generated.
            string[] fileEntries = Directory.GetFiles(ReportPath);
            if (fileEntries.Length <= 1)
            {
                throw new Exception("Failed to execute the command, see Extra Info for more details.");
            }

            //fetching all teststeps from the xml file in order to check the status of each on of them.
            XmlDocument doc = new XmlDocument();
            doc.Load(testCaseRunLogReport);
            XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);
            XmlNodeList CasesResults = doc.DocumentElement.SelectNodes("(//*[local-name()='testCaseRunLog'])", manager);

            int CasesCounter = 0;
            foreach (XmlNode caseNode in CasesResults)
            {
                string testCaseName = caseNode.Attributes["testCase"].Value;
                string testCaseStatus = caseNode.Attributes["status"].Value;
                CasesCounter++;
                XmlNodeList StepsResults = doc.DocumentElement.SelectNodes("(//*[local-name()='testCaseRunLog'][" + CasesCounter + "]/*[local-name()='testCaseRunLogTestStep'])", manager);
                foreach (XmlNode Stepnode in StepsResults)
                {
                    string TestStepStatus = Stepnode.Attributes["status"].Value;
                    string testStepName = Stepnode.Attributes["name"].Value;
                    if (!TestStepStatus.Equals("OK"))
                    {
                        if ((mAct.Error == null))
                        {
                            mAct.Error = "The below list of test steps have been failed: " + Environment.NewLine + "TestCase: " + testCaseName + ", TestStep: " + testStepName + ", Status: " + TestStepStatus;
                        }
                        else
                        {
                            mAct.Error = mAct.Error + Environment.NewLine + "TestCase: " + testCaseName + ", TestStep: " + testStepName + ", Status: " + TestStepStatus;
                        }
                    }
                }
            }

        }

        //returns all the namespace in order to use manager for  SelectNodes method

        //retrieves all requests and response for each test step, to be used after running the batch command.
        public Dictionary<string, List<string>> RequestsAndResponds()
        {
            Dictionary<string, List<string>> dict = [];
            string[] fileEntries = Directory.GetFiles(ReportPath);
            string testStepName = string.Empty;
            string request = string.Empty;
            string response = string.Empty;
            string message = string.Empty;
            string properties = string.Empty;
            string propertyName = string.Empty;
            string propertyValue = string.Empty;

            foreach (string fileName in fileEntries)
            {
                //length validation
                if (CheckFilePathLength(fileName) == false)
                {
                    continue;
                }

                if ((fileName[^3..]).Equals("txt"))
                {
                    testStepName = GenerateTestStepName(fileName);
                    request = GenerateRequest(fileName);
                    response = GenerateResponse(fileName);
                    message = GenerateMessage(fileName);
                    properties = GenerateProperties(fileName);

                    dict.Add(fileName, [testStepName, request, response, ReportPath, message, properties]);
                }
            }

            return dict;
        }

        private bool CheckFilePathLength(string fileName)
        {

            if (fileName.Length > 247)
            {
                mAct.Error = mAct.Error + "FileName is too long for validation, Please define shorter path for execution report under the agent and/or please make test suite + test case + test step shorter" + Environment.NewLine;
                return false;
            }
            else
            {
                return true;
            }
        }

        public Dictionary<List<string>, List<string>> OutputParamAndValues()
        {
            Dictionary<List<string>, List<string>> dict = [];

            string[] fileEntries = Directory.GetFiles(ReportPath);


            foreach (string fileName in fileEntries)
            {
                if ((fileName[^3..]).Equals("txt"))
                {
                    List<string> listNames = OutputNames(fileName);
                    List<string> listValues = OutputValues(fileName);
                    dict.Add(listNames, listValues);
                }
            }
            return dict;
        }

        //fetches the test step from the generated txt file
        private string GenerateTestStepName(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            int contentLeanth = fileContent.Length;
            int startTestStepIndex = fileContent.IndexOf("TestStep:");
            int endTestStepIndex = fileContent.IndexOf("----------------- Messages ------------------------------");
            string fileAfterTestStep = fileContent[startTestStepIndex..];
            int nextRowIndex = fileAfterTestStep.IndexOf("\r\n");
            string testStepName = fileAfterTestStep[9..nextRowIndex];
            return testStepName;
        }

        private string GenerateMessage(string fileName)
        {

            int messageLeanth = 0;
            string message = string.Empty;
            string fileContent = File.ReadAllText(fileName);
            int contentLeanth = fileContent.Length;
            int startMessageIndex = fileContent.IndexOf("----------------- Messages ------------------------------") + 59;
            int endMessageIndex = fileContent.IndexOf("----------------- Properties ------------------------------");
            int endMessageIndexForTransfer = fileContent.IndexOf("----------------------------------------------------");
            if (startMessageIndex == -1)
            {
                message = string.Empty;
            }
            else if (endMessageIndex == -1 && endMessageIndexForTransfer != -1)
            {
                messageLeanth = (endMessageIndexForTransfer - startMessageIndex);
                message = message + fileContent.Substring(startMessageIndex, messageLeanth);
            }
            else if (endMessageIndex == -1 && endMessageIndexForTransfer == -1)
            {
                message = message + fileContent[startMessageIndex..];
            }

            return message;
        }

        private List<string> OutputNames(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            List<string> listNames = [];

            while (true)
            {
                int startOutputPropertyNameIndex = fileContent.IndexOf("[");
                int startOutputPropertyValueIndex = fileContent.IndexOf("[[");
                int endOutputPropertyNameIndex = fileContent.IndexOf("]");
                if (startOutputPropertyNameIndex == -1 || startOutputPropertyValueIndex <= startOutputPropertyNameIndex || endOutputPropertyNameIndex < startOutputPropertyNameIndex)
                {
                    break;
                }

                int outputPropertyNameLeanth = (endOutputPropertyNameIndex - startOutputPropertyNameIndex);
                string outputPropertyNameSection = fileContent.Substring(startOutputPropertyNameIndex + 1, outputPropertyNameLeanth - 1);
                listNames.Add(outputPropertyNameSection);
                fileContent = fileContent[(endOutputPropertyNameIndex + 1)..];
            }
            return listNames;
        }

        private List<string> OutputValues(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            List<string> listNames = [];

            while (true)
            {
                int startOutputPropertyValueIndex = fileContent.IndexOf("[[");
                int endText = fileContent.IndexOf("------------ source path -------------");
                int endOutputPropertyValueIndex = fileContent.IndexOf("]]");
                if (startOutputPropertyValueIndex == -1 || endText <= startOutputPropertyValueIndex)
                {
                    break;
                }

                int outputPropertyNameLeanth = (endOutputPropertyValueIndex - startOutputPropertyValueIndex);
                string outputPropertyValueSection = fileContent.Substring(startOutputPropertyValueIndex + 2, outputPropertyNameLeanth - 2);
                listNames.Add(outputPropertyValueSection);
                fileContent = fileContent[(endText + 38)..];
            }
            return listNames;
        }

        private string GenerateProperties(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            int contentLeanth = fileContent.Length;
            int startPropertiesIndex = fileContent.IndexOf("----------------- Properties ------------------------------");
            int endPropertiesIndex = fileContent.IndexOf("---------------- Request ---------------------------");
            if (startPropertiesIndex == -1)
            {
                return string.Empty;
            }

            int PropertiesLeanth = (endPropertiesIndex - startPropertiesIndex);
            string PropertiesSection = fileContent.Substring(startPropertiesIndex + 61, PropertiesLeanth - 61);
            return PropertiesSection;
        }

        //fetches the requestXML from the generated txt file
        private string GenerateRequest(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            int contentLeanth = fileContent.Length;
            int startRequestIndex = fileContent.IndexOf("---------------- Request ---------------------------");
            int endRequestIndex = fileContent.IndexOf("---------------- Response --------------------------");
            if (startRequestIndex == -1)
            {
                return string.Empty;
            }

            int RequestLeanth = (endRequestIndex - startRequestIndex);
            string requestSection = fileContent.Substring(startRequestIndex, RequestLeanth);
            string requestXML = string.Empty;
            if (requestSection.IndexOf("<") != -1)
            {
                requestXML = requestSection[requestSection.IndexOf("<")..];
            }
            else
            {
                requestXML = requestSection;
            }

            return requestXML;
        }

        //fetches the responseXML from the generated txt file
        private string GenerateResponse(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            int startResponseIndex = fileContent.IndexOf("---------------- Response --------------------------");
            if (startResponseIndex == -1)
            {
                return string.Empty;
            }

            string responseSection = fileContent[startResponseIndex..];

            int startResponseXML = responseSection.IndexOf("<");
            if (startResponseXML > -1)
            {
                return responseSection[startResponseXML..];
            }

            int startResponseJSON = responseSection.IndexOf("{");
            if (startResponseJSON > -1)
            {
                return responseSection[startResponseJSON..];
            }

            return string.Empty;
        }

        public string UpdateXMLProperty()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string userFolderWithGingerTemp = userFolder + @"\Temp\GingerTemp";
            string fileForCommand = userFolder + @"\Temp\GingerTemp\fileForCommand.xml";

            if (!System.IO.Directory.Exists(userFolderWithGingerTemp))
            {
                System.IO.Directory.CreateDirectory(userFolderWithGingerTemp);
            }

            string testSuite = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestSuite);
            string testCase = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.TestCase);
            string XMLFiledValue = mAct.GetInputParamCalculatedValue(ActSoapUI.Fields.XMLFile);
            ObservableList<ActInputValue> TestSuitePlaceHolder = mAct.TestSuitePlaceHolder;

            ObservableList<ActSoapUiInputValue> AllProperties = mAct.AllProperties;

            if (!XMLFiledValue.Equals(string.Empty))
            {
                if (XMLFiledValue[^4..].ToUpper().Equals(".XML"))
                {
                    if (XMLFiledValue[..1].Equals("~"))
                    {
                        string SolutionFolder = mAct.SolutionFolder;
                        XMLFiledValue = System.IO.Path.Combine(SolutionFolder, XMLFiledValue[2..]);
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.Load(XMLFiledValue);
                    XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);

                    XmlNodeList caseProperties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='testCase'][@name='" + testCase + "']/*[local-name()='properties']/*[local-name()='property']", manager);
                    XmlNodeList suiteProperties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='properties']/*[local-name()='property']", manager);
                    XmlNodeList stepProperties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='testSuite'][@name='" + testSuite + "']/*[local-name()='testCase'][@name='" + testCase + "']/*[local-name()='testStep'][@type='properties']/*[local-name()='config']/*[local-name()='properties']/*[local-name()='property']", manager);
                    XmlNodeList projectProperties = doc.SelectNodes("//*[local-name()='soapui-project']/*[local-name()='properties']/*[local-name()='property']", manager);

                    foreach (ActSoapUiInputValue AIV in AllProperties)
                    {
                        if (AIV.Type == ActSoapUiInputValue.ePropertyType.Project.ToString())
                        {
                            UpdateProperty(projectProperties, AIV);
                        }
                        if (AIV.Type == ActSoapUiInputValue.ePropertyType.TestSuite.ToString())
                        {
                            UpdateProperty(suiteProperties, AIV);
                        }
                        if (AIV.Type == ActSoapUiInputValue.ePropertyType.TestCase.ToString())
                        {
                            UpdateProperty(caseProperties, AIV);
                        }
                        if (AIV.Type == ActSoapUiInputValue.ePropertyType.TestStep.ToString())
                        {
                            UpdateProperty(stepProperties, AIV);
                        }
                    }

                    doc.Save(fileForCommand);

                    if (mAct.TestSuitePlaceHolder.Count > 0)
                    {
                        string fileContent;
                        if (File.ReadAllText(fileForCommand).Equals(string.Empty))
                        {
                            fileContent = File.ReadAllText(XMLFiledValue);
                        }
                        else
                        {
                            fileContent = File.ReadAllText(fileForCommand);
                        }

                        foreach (ActInputValue AIV in TestSuitePlaceHolder)
                        {
                            string propertyName = AIV.Param;
                            string propertyValue = AIV.ValueForDriver;
                            fileContent = fileContent.Replace(propertyName, propertyValue);
                        }
                        File.WriteAllText(fileForCommand, fileContent, Encoding.ASCII);
                    }
                }
            }
            return fileForCommand;
        }

        private static void UpdateProperty(XmlNodeList properties, ActInputValue AIV)
        {
            string propertyName = AIV.Param;
            string propertyValue = AIV.ValueForDriver;

            foreach (XmlNode property in properties)
            {
                XmlNodeList propertyTags = property.ChildNodes;
                bool nodeFound = false;
                foreach (XmlNode tag in propertyTags)
                {
                    if (tag.Name == "con:name" && tag.InnerText == propertyName)
                    {
                        nodeFound = true;

                    }
                    if (nodeFound && tag.Name == "con:value")
                    {
                        tag.InnerText = propertyValue;
                        nodeFound = false;
                    }
                }
            }
        }
    }
}
