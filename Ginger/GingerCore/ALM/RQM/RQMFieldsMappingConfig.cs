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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GingerCore.ALM.RQM
{
    [XmlRoot("RQMProjects")]
    public class RQMProjectListConfiguration
    {
        public RQMProjectListConfiguration() { RQMProjects = new List<RQMProject>(); }

        [XmlElement("RQMProject")]
        public List<RQMProject> RQMProjects { get; set; }

        [XmlElement("IsProjectDefault")]
        public bool IsProjectDefault { get; set; }
    }
    public class RQMProject
    {
        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("Guid")]
        public String Guid { get; set; }

        [XmlElement("RQMTestPlansListMapping")]
        public RQMTestPlansListMapping RQMTestPlansListMapping { get; set; }

        [XmlElement("RQMTestPlanMapping")]
        public RQMTestPlanMapping RQMTestPlanMapping { get; set; }

        [XmlElement("RQMTestSuiteMapping")]
        public RQMTestSuiteMapping RQMTestSuiteMapping { get; set; }

        [XmlElement("RQMTestSuiteAsItemMapping")]
        public RQMTestSuiteAsItemMapping RQMTestSuiteAsItemMapping { get; set; }

        [XmlElement("RQMTestCaseMapping")]
        public RQMTestCaseMapping RQMTestCaseMapping { get; set; }

        [XmlElement("RQMTestScriptMapping")]
        public RQMTestScriptMapping RQMTestScriptMapping { get; set; }

        [XmlElement("RQMExecutionRecordsMapping")]
        public RQMExecutionRecordsMapping RQMExecutionRecordsMapping { get; set; }

        [XmlElement("RQMExecutionRecordsAsItemMapping")]
        public RQMExecutionRecordsAsItemMapping RQMExecutionRecordsAsItemMapping { get; set; }

        [XmlElement("RQMExecutionRecordsAsResourseMapping")]
        public RQMExecutionRecordsAsResourseMapping RQMExecutionRecordsAsResourseMapping { get; set; }

        [XmlElement("RQMTestSuiteExecutionRecordMapping")]
        public RQMTestSuiteExecutionRecordMapping RQMTestSuiteExecutionRecordMapping { get; set; }

        [XmlElement("RQMTestSuiteResultsMapping")]
        public RQMTestSuiteResultsMapping RQMTestSuiteResultsMapping { get; set; }

        [XmlElement("RQMStepMapping")]
        public RQMStepMapping RQMStepMapping { get; set; }
    }

    public class RQMTestPlansListMapping
    {
        [XmlElement("XMLPathToTestPlansList")]
        public String XMLPathToTestPlansList { get; set; }

        [XmlElement("URLPath")]
        public String URLPath { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("CreatedBy")]
        public String CreatedBy { get; set; }

        [XmlElement("CreationDate")]
        public String CreationDate { get; set; }

        [XmlElement("ModificationDate")]
        public String ModificationDate { get; set; }

        [XmlElement("RQMTestPlansListNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }

        [XmlElement("ContainedTestSuitesList")]
        public String ContainedTestSuitesList { get; set; }
    }

    public class RQMTestPlanMapping
    {
        [XmlElement("XMLPathToTestCasesList")]
        public String PathXML { get; set; }

        [XmlElement("XMLPathToCategoriesList")]
        public String PathCategoriesXML { get; set; }

        [XmlElement("RQMTestPlanNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }

        [XmlElement("XMLPathToTestSuitesList")]
        public String PathXMLToTestSuitesLists { get; set; }

        [XmlElement("Description")]
        public String Description { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }
    }

    public class RQMTestSuiteMapping
    {
        [XmlElement("XMLPathToTestCasesList")]
        public String PathXML { get; set; }

        [XmlElement("RQMTestSuiteNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }

        [XmlElement("Description")]
        public String Description { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("CreatedBy")]
        public String CreatedBy { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("CreationDate")]
        public String CreationDate { get; set; }
    }

    public class RQMTestSuiteAsItemMapping
    {
        [XmlElement("RQMTestSuiteAsItemNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }
    }

    public class RQMTestCaseMapping
    {
        [XmlElement("XMLPathToTestScriptsList")]
        public String PathXML { get; set; }

        [XmlElement("XMLPathToVariablesList")]
        public String XMLPathToVariablesList { get; set; }

        [XmlElement("VariableName")]
        public String VariableName { get; set; }

        [XmlElement("VariableValue")]
        public String VariableValue { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("CreatedBy")]
        public String CreatedBy { get; set; }

        [XmlElement("Description")]
        public String Description { get; set; }

        [XmlElement("CreationDate")]
        public String CreationDate { get; set; }

        [XmlElement("RQMTestCaseNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }

        [XmlElement("ModificationDate")]
        public String ModificationDate { get; set; }

        [XmlElement("XMLPathToCategoriesList")]
        public String PathCategoriesXML { get; set; }
    }

    public class RQMTestScriptMapping
    {
        [XmlElement("XMLPathToStepsList")]
        public String PathXML { get; set; }

        [XmlElement("XMLPathToVariablesList")]
        public String XMLPathToVariablesList { get; set; }

        [XmlElement("XMLPathToVariables")]
        public String XMLPathToVariables { get; set; }

        [XmlElement("VariableName")]
        public String VariableName { get; set; }

        [XmlElement("VariableValue")]
        public String VariableValue { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("Description")]
        public String Description { get; set; }

        [XmlElement("CreatedBy")]
        public String CreatedBy { get; set; }

        [XmlElement("CreationDate")]
        public String CreationDate { get; set; }

        [XmlElement("RQMTestScriptNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }

    public class RQMStepMapping
    {
        [XmlElement("Description")]
        public String Description { get; set; }

        [XmlElement("ExpectedResult")]
        public String ExpectedResult { get; set; }

        [XmlElement("Name")]
        public String Name { get; set; }
    }

    public class RQMNameSpaces
    {
        [XmlElement("RQMNameSpace")]
        public List<RQMNameSpace> RQMNameSpaceList { get; set; }
    }

    public class RQMNameSpace
    {
        [XmlElement("RQMNameSpacePrefix")]
        public String RQMNameSpacePrefix { get; set; }

        [XmlElement("RQMNameSpaceName")]
        public String RQMNameSpaceName { get; set; }
    }

    public class ExportTemplatesFileNames
    {
        [XmlElement("TestPlanTemplate_FileName")]
        public String TestPlanTemplate_FileName { get; set; }

        [XmlElement("TestCaseTemplate_FileName")]
        public String TestCaseTemplate_FileName { get; set; }

        [XmlElement("TestScriptTemplate_FileName")]
        public String TestScriptTemplate_FileName { get; set; }

        [XmlElement("ExecutionRecordTemplate_FileName")]
        public String ExecutionRecordTemplate_FileName { get; set; }

        [XmlElement("SuiteExecutionRecordTemplate_FileName")]
        public String SuiteExecutionRecordTemplate_FileName { get; set; }

        [XmlElement("ExecutionResultTemplate_FileName")]
        public String ExecutionResultTemplate_FileName { get; set; }
    }

    public class RQMExecutionRecordsMapping
    {
        [XmlElement("XMLPathToDescriptionsList")]
        public String PathXML { get; set; }

        [XmlElement("XMLPathOfSingleSelectionCase")]
        public String XMLPathOfSingleSelectionCase { get; set; }

        [XmlElement("ExecutesTestScriptUri")]
        public String ExecutesTestScriptUri { get; set; }

        [XmlElement("UsesTestScriptUri")]
        public String UsesTestScriptUri { get; set; }

        [XmlElement("VersioinedTestCaseXmlPathToID")]
        public String VersioinedTestCaseXmlPathToID { get; set; }

        [XmlElement("VersioinedTestScriptXmlPathToID")]
        public String VersioinedTestScriptXmlPathToID { get; set; }

        [XmlElement("RelatedTestCaseUri")]
        public String RelatedTestCaseUri { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("RQMExecutionRecordsNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }

    public class RQMTestSuiteExecutionRecordMapping
    {
        [XmlElement("XMLTestSuiteExecutionRecord")]
        public String XMLTestSuiteExecutionRecord { get; set; }

        [XmlElement("CurrentTestSuiteResult")]
        public String CurrentTestSuiteResult { get; set; }

        [XmlElement("RQMID")]
        public String RQMID { get; set; }

        [XmlElement("RQMTestSuiteExecutionRecordsNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }

    public class RQMTestSuiteResultsMapping
    {
        [XmlElement("TestSuiteExecutionRecordURI")]
        public String TestSuiteExecutionRecordURI { get; set; }

        [XmlElement("TestSuiteURI")]
        public String TestSuiteURI { get; set; }

        [XmlElement("XMLPathToResultsExecutionRecordsList")]
        public String XMLPathToResultsExecutionRecordsList { get; set; }

        [XmlElement("RQMTestSuiteResultsNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }

    public class RQMExecutionRecordsAsItemMapping
    {
        [XmlElement("XMLPathToTestCasesList")]
        public String PathXML { get; set; }

        [XmlElement("RQMExecutionRecordsAsItemNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }

    public class RQMExecutionRecordsAsResourseMapping
    {
        [XmlElement("XMLPathToTestCaseLink")]
        public String PathXMLTestCase { get; set; }

        [XmlElement("XMLPathToTestPlanLink")]
        public String PathXMLTestPlan { get; set; }

        [XmlElement("XMLPathToWebID")]
        public String XMLPathToWebID { get; set; }

        [XmlElement("RQMExecutionRecordsAsResourseNameSpaces")]
        public RQMNameSpaces RQMNameSpaces { get; set; }
    }
}
