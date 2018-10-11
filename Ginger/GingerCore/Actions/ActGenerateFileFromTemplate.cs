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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using GingerCore.Properties;
using GingerCore.Repository;
using System.Data.OleDb;
using System.IO;
using System.Data;
using GingerCore.Platforms;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;

namespace GingerCore.Actions
{
    public class ActGenerateFileFromTemplate : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Generate File From Template Action"; } }
        public override string ActionUserDescription { get { return "Generates File From Template Action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action when you want to generate file from template action.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To generate file from template action,Select file action type from File Action drop down and then enter data file name by clicking browse button.Then provide template file name and output file name and after that enter value and run the action.");
        }        

        public override string ActionEditPage { get { return "ActGenerateFileFromTemplateEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public enum eFileAction
        {
            CSVFromTemplate = 1,
        }

        [IsSerializedForLocalRepository]
        public eFileAction FileAction { get; set; }

        public ValueExpression VE { get; set; }

        public new static partial class Fields
        {
            public static string TemplateFileName = "TemplateFileName";
            public static string OutputFileName = "OutputFileName";
            public static string FileAction = "FileAction";
            public static string DataFileName = "DataFileName";            
        }

        [IsSerializedForLocalRepository]
        public string TemplateFileName { set; get; }

        [IsSerializedForLocalRepository]
        public string OutputFileName
        {
            get
            {
                return GetInputParamValue("OutputFileName");
            }
            set
            {
                AddOrUpdateInputParamValue("OutputFileName", value);
            }
        }

        [IsSerializedForLocalRepository]
        public string DataFileName { set; get; }

        public override String ToString()
        {
            return "ActGenerateFileFromTemplate - " + FileAction;
        }

        public override String ActionType
        {
            get
            {
                return "ActGenerateFileFromTemplate: " + FileAction.ToString();
            }
        }

        //TODO: icon
        public override System.Drawing.Image Image { get { return Resources.ASCF16x16; } }


        public override void Execute()
        {
            try
            {                
                switch (FileAction)
                {
                    case eFileAction.CSVFromTemplate:
                        string CompleteOutputFileName = GenerateOutputPath();
                        string txt = GenerateCSVFromTemplate();
                        System.IO.File.WriteAllText(CompleteOutputFileName, txt, Encoding.UTF8);                       
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
            }
        }
        //to get the absolute path of output file
        private string GenerateOutputPath()
        {
            string OutputFilePathNew;
            OutputFilePathNew = System.IO.Path.Combine(SolutionFolder, @"Documents\OutputFiles\", OutputFileName);
            return OutputFilePathNew;
        }

        private string GenerateCSVFromTemplate()
        {            
            string[] lines=new string[]{};
            if(File.Exists(TemplateFileName))
                lines = System.IO.File.ReadAllLines(TemplateFileName);
            
            StringBuilder sbheader= new StringBuilder();
            StringBuilder sbdata = new StringBuilder();
            StringBuilder sbtrailer = new StringBuilder();
            bool inHeader=false;
            bool inTrailer = false;
            bool inData=false;
            foreach (string l in lines)
            {

                if (string.IsNullOrEmpty(l))
                    continue;
                if (l.StartsWith("#GINGER_REM"))
                {
                    continue;
                    // Do nothing
                }

                if (l.Contains("#GINGER_HEADER_START"))
                {                    
                    inHeader = true;
                    inTrailer = false;
                    inData = false;
                    continue;
                }

                if (l.Contains("#GINGER_HEADER_END"))
                {
                    inHeader = false;
                    inTrailer = false;
                    inData = false;
                    continue;
                }
              

                if (l.Contains("#GINGER_RECORD_TEMPLATE_START"))
                {
                    //step = 3; // template line
                    inHeader = false;
                    inTrailer = false;
                    inData = true;
                    continue;
                }

                if (l.Contains("#GINGER_RECORD_TEMPLATE_END"))
                {
                    //step = 4; // end template line
                    inHeader = false;
                    inTrailer = false;
                    inData = false;
                    continue;
                }
                
                if (l.Contains("#GINGER_TAIL_START"))
                {
                    //step = 5; // Tail
                    inHeader = false;
                    inTrailer = true;
                    inData = false;
                    continue;
                }
                if (l.Contains("#GINGER_TAIL_END"))
                {
                    inHeader = false;
                    inTrailer = false;
                    inData = false;
                    continue;
                }

                if (inHeader) 
                {
                    string ld = ProcessLine(l);
                    sbheader.AppendLine(ld);
                    continue;
                }
                if (inData)
                {
                    List<string> lds = ProcessData(l);
                    foreach (var ld in lds)
                        sbdata.AppendLine(ld);
                    continue;
                } 
                if(inTrailer)
                {
                    string ld = ProcessLine(l);
                    sbtrailer.AppendLine(ld);
                    continue;
                } 
            }
            return sbheader.Append(sbdata.ToString()).Append(sbtrailer.ToString()).ToString();
        }

        private string ProcessLine(string l)
        {
            // TODO: process params VE          
            VE.Value = l;
            return VE.ValueCalculated;
        }

        private List<string> ProcessData(string l)
        {
            string DataFilePath;
            if (DataFileName.StartsWith("~"))
            {
                DataFilePath = System.IO.Path.Combine(SolutionFolder, DataFileName.Replace("~\\", ""));
            }
            else DataFilePath = DataFileName;

            // TODO: process params VE
            // Process Data for each line use l as template
            string[] lines = convertExcelToStringArray(DataFilePath, "Sheet1");
            string[] headers=lines[0].Split(','); //get col headers
            List<string> results = new List<string>();
            string template = l;
            for (int i = 1; i < lines.Length; i++)
            {
                template = l;
                string[] tmp = lines[i].Split(',');
                for (int j = 0; j < tmp.Length; j++)
                {
                    if (headers.Length < j)
                        continue;
                    template = template.Replace(",,", ",#,");
                    template = template.Replace("{Param=" + headers[j] + "}", tmp[j]);
                }
                VE.Value = template;
                results.Add(VE.ValueCalculated);
            }

            return results;
        }

        static string[] convertExcelToStringArray(string sourceFile, string worksheetName)
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + sourceFile + ";Extended Properties=\"Excel 12.0 Xml;HDR=NO;IMEX=1\"";
            OleDbConnection conn = null;            
            OleDbCommand cmd = null;
            OleDbDataAdapter da = null;
            List<string> ls = new List<string>();
            try
            {
                conn = new OleDbConnection(strConn);
                conn.Open();

                cmd = new OleDbCommand("SELECT * FROM [" + worksheetName + "$]", conn);
                cmd.CommandType = CommandType.Text;

                da = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string rowString = "";
                    for (int y = 0; y < dt.Columns.Count; y++)
                    {
                        rowString +=  dt.Rows[x][y].ToString() + ",";
                    }
                    ls.Add(rowString.Substring(0,rowString.Length-1));
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, exc.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Dispose();
                cmd.Dispose();
                da.Dispose();
            }
            return ls.ToArray();
        }
    }
}
