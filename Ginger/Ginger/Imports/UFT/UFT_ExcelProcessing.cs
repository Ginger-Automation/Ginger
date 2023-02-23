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
using System.Data;
using System.Data.OleDb;
using System.Windows;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Helpers;

namespace Ginger.Imports.UFT
{
    public class UFT_ExcelProcessing
    {
        private string GetExcelString(string sExcelFileName)
        {
            string s;
            if (sExcelFileName.Contains(".xlsx"))
            {
                s = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + sExcelFileName + ";Extended Properties=Excel 12.0;";
            }
            else
            {
                s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sExcelFileName + ";Extended Properties=Excel 8.0;";
            }
            return s;
        }

        public DataTable ProcessExcel(string sExcelFileName)
        {
            //Fetch Connection string
            string ConnString = GetExcelString(sExcelFileName);
            
            //DB Objects
            DataSet ds = new DataSet();
            OleDbCommand Cmd = new OleDbCommand();
            DataTable dt = new DataTable();

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    if (Conn.State != ConnectionState.Open) Conn.Open();
                    var tableschema = Conn.GetSchema();

                    // Get all Sheets in Excel File
                    DataTable dtSheet = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                     // Loop through all Sheets to get data
                    foreach (DataRow dr in dtSheet.Rows)
                    {
                        string sheetName = dr["TABLE_NAME"].ToString();
                        if (!sheetName.EndsWith("$")) continue;

                        if (sheetName == "MAIN$" || sheetName == "Main$")
                        {
                            // Get all rows from the Sheet
                            Cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                            Cmd.Connection = Conn;

                            OleDbDataAdapter da = new OleDbDataAdapter(Cmd);
                            da.SelectCommand = Cmd;
                            dt.TableName = sheetName;
                            da.Fill(dt);
                            ds.Tables.Add(dt);
                            return dt;
                        }
                    }
                    return dt;
                }
                catch (Exception ex)
                {                    
                    Reporter.ToUser(eUserMsgKey.ExcelProcessingError, ex.Message);
                    return null;
                }
            }  
        }

         public Dictionary<string, string> FetchVariableValuesfromCalendar(string sExcelFileName, string sSelectedBusFunction, DataTable dt_BizFlow)
         {
             //Dictionary for storing variable name and its value
            Dictionary<string, string> Variables = new Dictionary<string, string>();
            int i = 1;

            //When a Bus function is selected from calendar, Read its variables
            foreach (DataRow row in dt_BizFlow.Rows)
            {
                int sCount = dt_BizFlow.Columns.Count;
                if (row["Param0"].ToString() == sSelectedBusFunction && row["Param0"].ToString() != "ACTION")
                {
                    while (i < sCount)
                    {
                        if (row[i].ToString().Contains("&") && row[i].ToString() != "" )
                        {
                            //Variable Name
                            string sVariableName = row[i].ToString().Replace("&", "");

                            //Fetch Variable ValueSource from KEEP_REFER
                            string sValue = ReadKEEP_REFER(sVariableName, sExcelFileName);

                            //Add to Dictionary Object
                            if (!Variables.ContainsKey(sVariableName))
                            {
                                Variables.Add(sVariableName, sValue);
                            }
                           
                        }
                        i++;
                    }
                }
            }
            return Variables;
         }

         public string ReadKEEP_REFER(string sVarName, string sExcelFileName)
        {
            //Fetch Connection string
            string ConnString = GetExcelString(sExcelFileName);
            string Value="";

            //DB objects
            DataSet ds = new DataSet();
            OleDbCommand Cmd = new OleDbCommand();
            DataTable dt = new DataTable();

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    if (Conn.State != ConnectionState.Open) Conn.Open();
                    var tableschema = Conn.GetSchema();

                    // Get all Sheets in Excel File
                    DataTable dtSheet = Conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    // Loop through all Sheets to get data
                    foreach (DataRow dr in dtSheet.Rows)
                    {

                        string sheetName = dr["TABLE_NAME"].ToString();
                        if (!sheetName.EndsWith("$")) continue;

                        if (sheetName == "KEEP_REFER$" || sheetName == "Keep_Refer$")
                        {
                            // Get all rows from the Sheet
                            Cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                            Cmd.Connection = Conn;

                            OleDbDataAdapter da = new OleDbDataAdapter(Cmd);
                            da.SelectCommand = Cmd;
                            dt.TableName = sheetName;
                            da.Fill(dt);
                            ds.Tables.Add(dt);
                            Value = LoopKEEP_REFER(dt,sVarName);
                            return Value;
                        }
                    }
                    return Value;
                }
                catch (Exception ex)
                {                    
                    Reporter.ToUser(eUserMsgKey.ExcelProcessingError, ex.Message);
                    return null;
                }
            }  
        }

        private string LoopKEEP_REFER(DataTable KeepRefer_DT,string VarName)
        {
            string VarValue = "";

            //When a Bus function is selected from calendar, Read its variables
            foreach (DataRow row in KeepRefer_DT.Rows)
            {
                if (row[0].ToString()==VarName)
                {
                    //Variable Name
                    VarValue = row[1].ToString();
                    return VarValue;;
                }
              }
              return VarValue;
           }
        }
  }