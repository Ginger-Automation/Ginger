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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ValueExpression;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace GingerCore
{
    public class ValueExpression
    {
        // regEx Cheat sheet
        //  ^  Depending on whether the MultiLine option is set, matches the position before the first character in a line, or the first character in the string. 
        //  $  Depending on whether the MultiLine option is set, matches the position after the last character in a line, or the last character in the string. 
        //  *  Matches the preceding character zero or more times. For example, "zo*" matches either "z" or "zoo". 
        //  +  Matches the preceding character one or more times. For example, "zo+" matches "zoo" but not "z". 
        //  ?  Matches the preceding character zero or one time. For example, "a?ve?" matches the "ve" in "never".  
        //  . Matches any single character except a newline character.  
        //  (pattern)  Matches pattern and remembers the match. The matched substring can be retrieved from the resulting Matches collection, using Item [0]...[n]. To match parentheses characters ( ), use "\(" or "\)". 
        //  (?<name>pattern)  Matches pattern and gives the match a name.  
        //  (?:pattern)  A non-capturing group  
        //  (?=...)  A positive lookahead  
        //  (?!...)  A negative lookahead  
        //  (?<=...)  A positive lookbehind .  
        //  (?<!...)  A negative lookbehind .  
        //  x|y Matches either x or y. For example, "z|wood" matches "z" or "wood". "(z|w)oo" matches "zoo" or "wood".  
        //  {n} n is a non-negative integer. Matches exactly n times. For example, "o{2}" does not match the "o" in "Bob," but matches the first two o's in "foooood". 
        //  {n,}  n is a non-negative integer. Matches at least n times. For example, "o{2,}" does not match the "o" in "Bob" and matches all the o's in "foooood." "o{1,}" is equivalent to "o+". "o{0,}" is equivalent to "o*". 
        //  {n,m}  m and n are non-negative integers. Matches at least n and at most m times. For example, "o{1,3}" matches the first three o's in "fooooood." "o{0,1}" is equivalent to "o?". 
        //  [xyz]  A character set. Matches any one of the enclosed characters. For example, "[abc]" matches the "a" in "plain".  
        //  [^xyz]  A negative character set. Matches any character not enclosed. For example, "[^abc]" matches the "p" in "plain".  
        //  [a-z]  A range of characters. Matches any character in the specified range. For example, "[a-z]" matches any lowercase alphabetic character in the range "a" through "z".  
        //  [^m-z]  A negative range characters. Matches any character not in the specified range. For example, "[m-z]" matches any character not in the range "m" through "z".  
        //  \b  Matches a word boundary, that is, the position between a word and a space. For example, "er\b" matches the "er" in "never" but not the "er" in "verb".  
        //  \B  Matches a non-word boundary. "ea*r\B" matches the "ear" in "never early".  
        //  \d  Matches a digit character. Equivalent to [0-9].  
        //  \D  Matches a non-digit character. Equivalent to [^0-9].  
        //  \f  Matches a form-feed character.  
        //  \k  A back-reference to a named group.  
        //  \n  Matches a newline character.  
        //  \r  Matches a carriage return character.  
        //  \s  Matches any white space including space, tab, form-feed, etc. Equivalent to "[ \f\n\r\t\v]". 
        //  \S  Matches any nonwhite space character. Equivalent to "[^ \f\n\r\t\v]".  
        //  \t  Matches a tab character.  
        //  \v  Matches a vertical tab character.  
        //  \w  Matches any word character including underscore. Equivalent to "[A-Za-z0-9_]".  
        //  \W  Matches any non-word character. Equivalent to "[^A-Za-z0-9_]".  
        //  \num  Matches num, where num is a positive integer. A reference back to remembered matches. For example, "(.)\1" matches two consecutive identical characters.  
        //  \n Matches n, where n is an octal escape value. Octal escape values must be 1, 2, or 3 digits long. For example, "\11" and "\011" both match a tab character. "\0011" is the equivalent of "\001" & "1". Octal escape values must not exceed 256. If they do, only the first two digits comprise the expression. Allows ASCII codes to be used in regular expressions. 
        //  \xn Matches n, where n is a hexadecimal escape value. Hexadecimal escape values must be exactly two digits long. For example, "\x41" matches "A". "\x041" is equivalent to "\x04" & "1". Allows ASCII codes to be used in regular expressions. 
        //  \un Matches a Unicode character expressed in hexadecimal notation with exactly four numeric digits. "\u0200" matches a space character.  
        //  \A Matches the position before the first character in a string. Not affected by the MultiLine setting  
        //  \Z Matches the position after the last character of a string. Not affected by the MultiLine setting.  
        //  \G Specifies that the matches must be consecutive, without any intervening non-matching characters.  
        

        private static string rxVar = "";
        private static string rxVare = "";

        // ^{} = exclude { inside or } inside - so we don't want to get var if there are 2 { - like VBS calc of 2 vars or if the } at the end
        public static Regex rxVarPattern = new Regex(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);

        public static Regex rxGlobalParamPattern = new Regex(@"({GlobalAppsModelsParam Name=(\D*\d*\s*)}})|({GlobalAppsModelsParam Name=(\D*\d*\s*)})", RegexOptions.Compiled);
        
        private static Regex rxDSPattern = new Regex(@"{(\bDS Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
        public static Regex rxEnvParamPattern = new Regex(@"{(\bEnvParam App=)\w+\b[^{}]*}", RegexOptions.Compiled);
        public static Regex rxEnvUrlPattern = new Regex(@"{(\bEnvURL App=)\w+\b[^{}]*}", RegexOptions.Compiled);
        
        private static Regex rx = new Regex(@"{[V|E|VBS]" + rxVar + "[^{}]*}", RegexOptions.Compiled);
        private static Regex rxe = new Regex(@"{RegEx" + rxVare + ".*}", RegexOptions.Compiled);
        private static Regex rfunc = new Regex("{Function(\\s)*Fun(\\s)*=(\\s)*([a-zA-Z]|\\d)*\\((\")*([^\\)}\\({])*(\")*\\)}", RegexOptions.Compiled);
        // Enable setting value simply by assigned string, 
        // so no need to create new VE class everywhere in code
        // Can simpliy do: ValueExpression VE = "{Var Name=V1}"
        public static string SolutionFolder = "";

        public static explicit operator ValueExpression(string Value)
        {
            ValueExpression VE = new ValueExpression(null, null);
            VE.Value = Value;
            return VE;
        }

        ObservableList<DataSourceBase> DSList;

        BusinessFlow BF;
        ProjEnvironment Env;
        bool bUpdate;
        string updateValue;
        bool bDone;

        public bool DecryptFlag = false;
        private string mValueCalculated = null;

        ObservableList<VariableBase> mSolutionVariables = null;

        public string Value { get; set; }

        public string ValueCalculated
        {
            get
            {
                Calculate();
                return mValueCalculated;
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public ValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null)
        {
            this.Env = Env;
            this.BF = BF;
            this.DSList = DSList;
            this.bUpdate = bUpdate;
            this.updateValue = UpdateValue;
            this.bDone = bDone;
            mSolutionVariables = solutionVariables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">the Object containing the Vlaue Expression field</param>
        /// <param name="attr">The Attribute name of the VE field</param>
        public ValueExpression(Object obj, string attr)
        {
            this.Obj = obj;
            this.ObjAttr = attr;
        }

        //In case we need to pass VE to another control like grid -> edit then we fill this 2 values
        public Object Obj { get; set; }
        public string ObjAttr { get; set; }

        private void Calculate()
        {
            if (Value == null)
            {
                mValueCalculated = "";
                return;
            }
            mValueCalculated = Value;

            //Do the operation based on order!!!
            //First replace Vars - since they can apear in other func like VBS v1+v2 or VBS mid(v1,1,4);
            ReplaceVars();

            ReplaceGlobalParameters();

            //replace environment parameters which embeded into functions like VBS
            ReplaceEnvVars();
            ReplaceDataSources();

            CalculateFunctions();

            if (!string.IsNullOrEmpty(SolutionFolder))
                mValueCalculated = mValueCalculated.Replace(@"~\", SolutionFolder);

        }

        private void ReplaceGlobalParameters()
        {
            MatchCollection matches = rxGlobalParamPattern.Matches(mValueCalculated);
            if (matches.Count == 0)
            {
                return;
            }

            foreach (Match match in matches)
            {
                ReplaceGlobalParameter(match.Value);
            }
        }

        private void ReplaceGlobalParameter(string p)
        {
            string VarName = p.Replace("{GlobalAppsModelsParam Name=", "");
            VarName = VarName.Substring(0, VarName.Length -1);

            ObservableList<GlobalAppModelParameter> ModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            GlobalAppModelParameter Param = ModelsGlobalParamsList.Where(x => x.PlaceHolder == VarName).FirstOrDefault();

            if (Param != null)
            {
                if (DecryptFlag == true)
                {
                    bool res = false;
                    String strValuetoPass;
                    strValuetoPass = EncryptionHandler.DecryptString(Param.CurrentValue, ref res);
                    if (res == true)
                        mValueCalculated = mValueCalculated.Replace(p, strValuetoPass);
                    else
                        mValueCalculated = mValueCalculated.Replace(p, Param.CurrentValue);
                }
                else
                    mValueCalculated = mValueCalculated.Replace(p, Param.CurrentValue);
            }
            else
            {
                mValueCalculated = mValueCalculated.Replace(p,string.Format("ERROR: The Global Model Parameter '{0}' was not found",VarName));
            }
        }

        public static bool IsThisDynamicVE(string VE)
        {
            MatchCollection VariablesMatches = rxVarPattern.Matches(VE);
            if (VariablesMatches.Count > 0) { return true; }
            MatchCollection envParamMatches = rxEnvParamPattern.Matches(VE);
            if (envParamMatches.Count > 0) { return true; }
            MatchCollection envUrlMatches = rxEnvUrlPattern.Matches(VE);
            if (envUrlMatches.Count > 0) { return true; }
            MatchCollection DSmatches = rxDSPattern.Matches(VE);
            if (DSmatches.Count > 0) { return true; }
            MatchCollection matchesRegEx = rxe.Matches(VE);
            if (matchesRegEx.Count > 0) { return true; }
            MatchCollection matcheVBS = rx.Matches(VE);
            if (matcheVBS.Count > 0) { return true; }

            return false;
        }

        private void ReplaceDataSources()
        {
            int iCount = 0;// defining no to go in endless loop            
            MatchCollection matches = rxDSPattern.Matches(mValueCalculated);
            while (matches.Count > 0 && iCount < 10)
            {
                foreach (Match match in matches)
                {
                    bool bChange = false;
                    if (bUpdate == true)
                    {
                        // Addign this to update value only for the main DS Expression .. not VE in Parameter
                        Regex rxDS = new Regex("{DS Name=");
                        MatchCollection dsMatch = rxDS.Matches(mValueCalculated);
                        if (dsMatch.Count > 1)
                        {
                            bUpdate = false;
                            bChange = true;
                        }
                    }
                    ReplaceDataSource(match.Value);
                    //Setting update back
                    if (bChange == true)
                        bUpdate = true;
                }
                matches = rxDSPattern.Matches(mValueCalculated);
                iCount++;
            }
        }

        public void ReplaceDataSource(string p)
        {
            string pOrg = p;
            string bMarkAsDone = "";
            string bMultiRow = "N";
            string iColVal = "";
            DataSourceBase DataSource = null;

            string DSName = p.Substring(9, p.IndexOf(" DST=") - 9);
            
            foreach (DataSourceBase ds in DSList)
                if (ds.Name == DSName)
                    DataSource = ds;

            p = p.Substring(p.IndexOf(" DST=")).Trim();
            if (DataSource == null)
            {
                mValueCalculated = mValueCalculated.Replace(p, string.Format("ERROR: The Data Source Variable '{0}' was not found", DataSource));
                return;
            }

            if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
            {
                if (DataSource.FileFullPath.StartsWith("~"))
                {
                    DataSource.FileFullPath = DataSource.FileFullPath.Replace(@"~\","").Replace("~", "");
                    DataSource.FileFullPath = Path.Combine(DataSource.ContainingFolderFullPath.Replace("DataSources", "") , DataSource.FileFullPath);
                }
                DataSource.Init(DataSource.FileFullPath);
            }

            string Query = "";
            string updateQuery = "";
            List<string> mColList = null;
            string rowNum = "0";
            string DSTable = "";
            string sAct = "";
            string IRow = "";
            string ExcelPath = "";
            string ExcelSheet = "";            
            try
            {
                DSTable = p.Substring(p.IndexOf("DST=") + 4, p.IndexOf(" ") - 4);
                mColList = DataSource.DSC.GetColumnList(DSTable);
                p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();

                if (p.IndexOf("ACT=") != -1)
                {
                    sAct = p.Substring(p.IndexOf("ACT=") + 4, p.IndexOf(" ") - 4);

                    if (sAct == "DA") // Delete All Rows
                    {
                        updateQuery = "Delete From " + DSTable;
                        p = "";
                    }
                    else if (sAct == "YA") // Mark All Used
                    {
                        updateQuery = "Update " + DSTable + " SET GINGER_USED='True'";
                        p = "";
                    }
                    else if (sAct == "NA") // Mark All UnUsed
                    {
                        updateQuery = "Update " + DSTable + " SET GINGER_USED='False'";
                        p = "";
                    }
                    else if (sAct == "RC") // Get Row Count
                    {
                        Query = "Select COUNT(*) FROM " + DSTable;
                        p = "";
                    }
                    else if (sAct == "ETE") // Get Row Count
                    {
                        Query = "";
                        p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    }
                    else
                        p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                }
                if (p.IndexOf("EP=") != -1)
                {
                    ExcelPath = p.Substring(p.IndexOf("EP=") + 3, p.IndexOf(" ") - 3);
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    ExcelSheet = p.Substring(p.IndexOf("ES=") + 3, p.IndexOf("}") - 3);
                }
                else if (p.IndexOf("KEY=") != -1)
                {
                    string KeyName = p.Substring(p.IndexOf("KEY=") + 4, p.IndexOf("}") - 4);
                    if (sAct == "DR")
                        updateQuery = "DELETE FROM " + DSTable + " WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                    else
                    {
                        if (bUpdate == true)
                        {
                            DataTable dtTemp = DataSource.DSC.GetQueryOutput("Select count(*) from " + DSTable + " where GINGER_KEY_NAME= '" + KeyName + "'");
                            if (dtTemp.Rows[0].ItemArray[0].ToString() != "0")
                                updateQuery = "UPDATE " + DSTable + " SET GINGER_KEY_VALUE = '" + updateValue.Replace("'", "''") + "',GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "',GINGER_LAST_UPDATE_DATETIME='" + DateTime.Now.ToString() + "' WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                            else
                                updateQuery = "INSERT INTO " + DSTable + "(GINGER_KEY_NAME,GINGER_KEY_VALUE,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME) VALUES ('" + KeyName + "','" + updateValue.Replace("'", "''") + "','" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "')";
                        }
                        else
                            Query = "Select GINGER_KEY_VALUE FROM " + DSTable + " WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                    }
                }
                else if (p != "" && (sAct == "MASD" || sAct == "DR" || sAct == ""))
                {
                    bMarkAsDone = p.Substring(p.IndexOf("MASD=") + 5, p.IndexOf(" ") - 5);
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    if(p.IndexOf("MR=") == 0)
                    {
                        bMultiRow = p.Substring(p.IndexOf("MR=") + 3, p.IndexOf(" ") - 3);
                        p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    }
                    string DSIden = p.Substring(p.IndexOf("IDEN=") + 5, p.IndexOf(" ") - 5);
                    p = p.Substring(p.TrimStart().IndexOf(" ")).Trim();
                    if (DSIden == "Query")
                    {
                        Query = p.Substring(p.IndexOf("QUERY=") + 6, p.Length - 7);                        
                        if (Query.ToUpper().IndexOf("SELECT *") == -1)
                        {
                            Query = Regex.Replace(Query, " FROM ", ",GINGER_ID FROM ", RegexOptions.IgnoreCase);
                        }
                    }
                    else
                    {
                        Query = "Select ";
                        iColVal = p.Substring(p.IndexOf("ICOLVAL=") + 8, p.IndexOf("IROW=") - 9);
                        p = p.Substring(p.TrimStart().IndexOf("IROW="));
                        Query = Query + iColVal + ",GINGER_ID from " + DSTable;

                        if (p.IndexOf(" ") > 0)
                            IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf(" ") - 5);
                        else
                            IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf("}") - 5);
                        if (IRow == "NxtAvail")
                        {
                            Query = Query + " Where GINGER_USED <> 'True' or GINGER_USED is null";
                        }
                        else if (IRow == "RowNum")
                        {
                            p = p.Substring(p.TrimStart().IndexOf("ROWNUM="));
                            rowNum = p.Substring(p.IndexOf("ROWNUM=") + 7, p.IndexOf("}") - 7);
                        }
                        else if (IRow == "Where")
                        {
                            if (p.TrimStart().IndexOf("COND=") != -1)
                            {
                                p = p.Substring(p.TrimStart().IndexOf("COND="));
                                string Cond = p.Substring(p.IndexOf("COND=") + 5, p.IndexOf("}") - 5);
                                Query = Query + " Where " + Cond;
                            }
                            else if (p.TrimStart().IndexOf("WCOLVAL=") != -1 && p.TrimStart().IndexOf("WOPR=") != -1)
                            {
                                p = p.Substring(p.TrimStart().IndexOf("WCOLVAL="));
                                string wColVal = p.Substring(p.IndexOf("WCOLVAL=") + 8, p.IndexOf("WOPR=") - 9);
                                Query = Query + " Where ";
                                p = p.Substring(p.TrimStart().IndexOf("WOPR="));
                                string wOpr = "";
                                string wRowVal = "";
                                if (p.IndexOf("WROWVAL=") == -1)
                                    wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("}") - 5);
                                else
                                    wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("WROWVAL=") - 6);
                                if (wOpr != "Is Null" && wOpr != "Is Null")
                                {
                                    p = p.Substring(p.TrimStart().IndexOf("WROWVAL="));
                                    wRowVal = p.Substring(p.IndexOf("WROWVAL=") + 8, p.IndexOf("}") - 8);
                                }
                                if (wOpr == "Equals")
                                {
                                    if (wColVal == "GINGER_ID")
                                        Query = Query + wColVal + " = " + wRowVal + "";
                                    else
                                        Query = Query + wColVal + " = '" + wRowVal + "'";
                                }
                                else if (wOpr == "NotEquals")
                                {
                                    if (wColVal == "GINGER_ID")
                                        Query = Query + wColVal + " <> " + wRowVal + "";
                                    else
                                        Query = Query + wColVal + " <> '" + wRowVal + "'";
                                }
                                else if (wOpr == "Contains")
                                    Query = Query + wColVal + " LIKE " + "'%" + wRowVal + "%'";
                                else if (wOpr == "Not Contains")
                                    Query = Query + wColVal + " NOT LIKE " + "'%" + wRowVal + "%'";
                                else if (wOpr == "Starts With")
                                    Query = Query + wColVal + " LIKE '" + wRowVal + "%'";
                                else if (wOpr == "Not Starts With")
                                    Query = Query + wColVal + " NOT LIKE '" + wRowVal + "%'";
                                else if (wOpr == "Ends With")
                                    Query = Query + wColVal + " LIKE '%" + wRowVal + "'";
                                else if (wOpr == "Not Ends With")
                                    Query = Query + wColVal + " NOT LIKE '%" + wRowVal + "'";
                                else if (wOpr == "Is Null")
                                    Query = Query + wColVal + " IS NULL";
                                else if (wOpr == "Is Not Null")
                                    Query = Query + wColVal + " IS NOT NULL";
                            }
                            else
                                return;
                        }
                    }
                }
            }
            catch (Exception e) {
                mValueCalculated = pOrg;
                Console.WriteLine(e.StackTrace);
            }
            if (Query != "")
            {
                DataTable dt = DataSource.DSC.GetQueryOutput(Query);
                if (dt.Rows.Count == 0 && IRow == "NxtAvail" && bUpdate == true)
                {
                    DataSource.DSC.RunQuery("INSERT INTO " + DSTable + "(GINGER_USED) VALUES ('False')");
                    dt = DataSource.DSC.GetQueryOutput(Query);
                }
                if (dt.Rows.Count == 0)
                {
                    mValueCalculated = "No Row found with " + Query;
                    return;
                }
                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                    if (rowNum.All(char.IsDigit))
                        mValueCalculated = mValueCalculated.Replace(pOrg, dt.Rows[Convert.ToInt32(rowNum)].ItemArray[0].ToString());
                    else
                        mValueCalculated = "ERROR: Not Valid RowNum:" + rowNum;

                string GingerIds = "";
                if(dt.Columns.Contains("GINGER_ID"))
                {
                    if (bMultiRow == "Y")
                    {
                        foreach (DataRow row in dt.Rows)
                            GingerIds += row["GINGER_ID"].ToString() + ",";
                        GingerIds = GingerIds.Substring(0, GingerIds.Length - 1);
                    }
                    else
                        GingerIds = dt.Rows[Convert.ToInt32(rowNum)]["GINGER_ID"].ToString();
                }
                if (bUpdate == true)
                {
                    if (updateQuery == "")
                    {
                        if (iColVal == "")
                            iColVal = dt.Columns[0].ColumnName;
                        if (updateValue == null)
                        {
                            updateValue = string.Empty;
                        }
                        updateQuery = "UPDATE " + DSTable + " SET ";
                        foreach (DataColumn sCol in dt.Columns)
                        {
                            if (!new List<string> { "GINGER_ID", "GINGER_LAST_UPDATED_BY", "GINGER_LAST_UPDATE_DATETIME", "GINGER_KEY_NAME" }.Contains(sCol.ColumnName))
                                updateQuery += sCol.ColumnName + "='" + updateValue.Replace("'", "''") + "' ,";
                        }
                        updateQuery = updateQuery.Substring(0, updateQuery.Length - 1);
                        if (mColList.Contains("GINGER_LAST_UPDATED_BY"))
                            updateQuery = updateQuery + ",GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "' ";
                        if (mColList.Contains("GINGER_LAST_UPDATE_DATETIME"))
                            updateQuery = updateQuery + ",GINGER_LAST_UPDATE_DATETIME = '" + DateTime.Now.ToString() + "' ";

                        updateQuery = updateQuery + "WHERE GINGER_ID IN (" + GingerIds + ")";
                    }
                    DataSource.DSC.RunQuery(updateQuery);
                }
                if (bMarkAsDone == "Y" && bDone == true)
                {
                    DataSource.DSC.RunQuery("UPDATE " + DSTable + " SET GINGER_USED ='True' WHERE GINGER_ID IN (" + GingerIds + ")");
                }
                else if (sAct == "DR" && bDone == true)
                    DataSource.DSC.RunQuery("DELETE FROM " + DSTable + " WHERE GINGER_ID IN  (" + GingerIds + ")");
            }
            else if (updateQuery != "" && bDone == true)
            {
                DataSource.DSC.RunQuery(updateQuery);
                mValueCalculated = "";
            }
            else if (sAct == "ETE" && bDone == true)
            {
                if (ExcelSheet == "")
                    ExcelSheet = DSTable;
                if (ExcelPath.ToLower().EndsWith(".xlsx"))
                {
                    DataSource.DSC.ExporttoExcel(DSTable, ExcelPath, ExcelSheet);
                    mValueCalculated = "";
                }
                else
                    mValueCalculated = "The Export Excel can be *.xlsx only";
            }
            DataSource.Close();
        }


        private void CalculateFunctions()
        {      
            string value = mValueCalculated;
            MatchCollection matches = rxe.Matches(value);
            MatchCollection ms;
            if (matches.Count == 0)
            {
                // no variables found
                matches = rx.Matches(value);
                if (matches.Count == 0)
                {
                    matches = rfunc.Matches(value);
                    if (matches.Count == 0)
                        return;
                }
            }

            // found matched replace with var(s) funcs etc... value   
            foreach (Match match in matches)
            {
                ms = rx.Matches(match.Value);
                int iCount = 0;// defining no to go in endless loop
                while (ms.Count > 0 && iCount < 10)
                {
                    foreach (Match m in ms)
                    {
                        ProcessFunction(m.Value);
                    }
                    ms = rx.Matches(mValueCalculated);
                    iCount++;
                }
            }

            value = mValueCalculated;
            matches = rxe.Matches(value);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    ProcessFunction(match.Value);
                }

            }
            ProcessGeneralFuncations();
        }

        private void ProcessGeneralFuncations()
        {
            MatchCollection mc = rfunc.Matches(mValueCalculated);
            if(mc.Count==0)
            {
                return;
            }
            foreach (Match m in mc)
            {
                ProcessFunction(m.Value);
            }
            ProcessGeneralFuncations();
        }

        private void ReplaceVars()
        {
            MatchCollection matches = rxVarPattern.Matches(mValueCalculated);
            if (matches.Count == 0)
            {
                return;
            }
            
            foreach (Match match in matches)
            {
                ReplaceVar(match.Value);
            }
        }

        private void ReplaceEnvVars()
        {
            MatchCollection envParamMatches = rxEnvParamPattern.Matches(mValueCalculated);
            foreach (Match match in envParamMatches)            
                ReplaceEnvParamWithValue(match.Value, null);

            MatchCollection envUrlMatches = rxEnvUrlPattern.Matches(mValueCalculated);
            foreach (Match match2 in envUrlMatches)
                ReplaceEnvURLWithValue(match2.Value, null);
        }

        private void ReplaceVar(string p)
        {
            string VarName = p.Replace("{Var Name=", "");
            VarName = VarName.Replace("}", "");

            VariableBase vb = null;
            if (BF != null)
                vb = BF.GetHierarchyVariableByName(VarName);
            else if (mSolutionVariables != null)
                vb = (from v1 in mSolutionVariables where v1.Name == VarName select v1).FirstOrDefault();
            if (vb != null)
            {
                if (DecryptFlag == true)
                {
                    bool res = false;
                    String strValuetoPass;
                    strValuetoPass = EncryptionHandler.DecryptString(vb.Value, ref res);
                    if (res == true) mValueCalculated = mValueCalculated.Replace(p, strValuetoPass);
                    else mValueCalculated = mValueCalculated.Replace(p, vb.Value);
                }
                else
                 mValueCalculated = mValueCalculated.Replace(p, vb.Value);
            }
            else
            {
                mValueCalculated = mValueCalculated.Replace(p, string.Format("ERROR: The {0} '{1}' was not found", GingerDicser.GetTermResValue(eTermResKey.Variable), VarName));
            }
        }

        private void ProcessFunction(string p)
        {
            string pc = p.Substring(1, p.Length - 2);
            string[] a = pc.Split(' ');
            string Function = a[0];
            
            switch (Function)
            {
                case "Var":
                    ReplaceVarWithValue(p, a);
                    break;
                case "EnvParam":
                    ReplaceEnvParamWithValue(p, a);
                    break;
                case "EnvURL":
                    ReplaceEnvURLWithValue(p, a);
                    break;
                case "RegEx":
                    ReplaceRegExWithValue(p, a);
                    break;
                case "VBS":
                    ReplaceVBSCalcWithValue(p, a);
                    break;
                case "Function":
                    ReplaceGeneralFunctionsWithValue(p, a);
                    break;
                default:
                    //The expression does not include recognizable key words
                    //return string as is.
                    break;
            }
        }

        private void ReplaceVBSCalcWithValue(string p, string[] a)
        {
            try
            {      
                string Expr = p.Replace("\r\n", "vbCrLf");
                Expr = Expr.Substring(1, Expr.Length - 2);
                Expr = Expr.Replace("VBS Eval=", "");
                //check whether the Expr contains Split.If yes the take user entered number and decreased it to -1
                if (p.Contains("{VBS Eval=Split("))
                {
                    Expr = DecreaseVBSSplitFunIndexNumber(Expr);
                }
                string v = VBS.ExecuteVBSEval(@"" + Expr);
                mValueCalculated = mValueCalculated.Replace(p, v.ToString());
            }
            catch (Exception e)
            {
                //TODO: err
                mValueCalculated = mValueCalculated.Replace(p, "ERROR: " + e.Message);
            }
        }

        private string DecreaseVBSSplitFunIndexNumber(string Expr)
        {
            int posA = Expr.LastIndexOf("(");
            int posB = Expr.LastIndexOf(")");
            if (posA == -1)
            { return Expr; }
            if (posB == -1)
            { return Expr; }

            string sNum = Expr.Substring(posA + 1, posB - posA - 1);

            int iNum;
            bool bResult = int.TryParse(sNum, out iNum);
            if (bResult == true && iNum > 0)
            {
                iNum = iNum - 1;
                Expr = Expr.Remove(posA + 1, posB - posA - 1).Insert(posA + 1, "" + iNum + "");
            }

            return Expr;
        }

        private void ReplaceEnvURLWithValue(string p, string[] a)
        {
            string AppName = null;
            string URL = null;
            AppName = p.Replace("\r\n", "vbCrLf");
            AppName = AppName.Substring(1, AppName.Length - 2);
            AppName = AppName.Replace("EnvURL App=", "");

            EnvApplication app = Env.GetApplication(AppName);
            if (app != null)
            {
                URL = app.Url + "";
                mValueCalculated = mValueCalculated.Replace(p, URL);
            }
            else
            {
                // TODO: err
            }
        }

        private void ReplaceEnvParamWithValue(string p, string[] a)
        {
            string AppName = null;
            string GlobalParamName = null;
            
            p = p.Replace("\r\n", "vbCrLf");
            string appStr = " App=";
            string paramStr = " Param=";
            int indxOfApp = p.IndexOf(appStr);
            int indexOfParam = p.IndexOf(paramStr);
            AppName = p.Substring(indxOfApp + appStr.Length, indexOfParam - (indxOfApp + appStr.Length));
            GlobalParamName = p.Substring(indexOfParam + paramStr.Length, (p.Length - 1) - (indexOfParam + paramStr.Length));

            string ParamValue = null;

            EnvApplication app = Env.GetApplication(AppName);
            if (app != null)
            {
                GeneralParam GP = app.GetParam(GlobalParamName);
                if (GP != null)
                {
                    ParamValue = GP.Value + "";  // Autohandle in case param is null convert to empty string

                    if (DecryptFlag == true)
                    {
                        bool res = false;
                        String strValuetoPass;
                        strValuetoPass = EncryptionHandler.DecryptString(GP.Value, ref res);
                        if (res == true) mValueCalculated = mValueCalculated.Replace(p, strValuetoPass);
                        else mValueCalculated = mValueCalculated.Replace(p, ParamValue);
                    }
                    else
                    {
                        ValueExpression VE = new ValueExpression(Env, BF, DSList);
                        VE.Value = ParamValue;
                        ParamValue = VE.ValueCalculated;
                        mValueCalculated = mValueCalculated.Replace(p, ParamValue);
                    }
                }
                else
                {
                    // TODO: err
                    mValueCalculated = mValueCalculated.Replace(p, "");
                }
            }
            else
            {
                // TODO: err
                mValueCalculated = mValueCalculated.Replace(p, "");
            }


        }
        private void ReplaceGeneralFunctionsWithValue(string p, string[] a)
        {
            string pc = string.Join(" ", a);
            string[] dis = new[] { "Fun=" };

            string[] aa = pc.Split(dis, StringSplitOptions.RemoveEmptyEntries);

            string FunName = (aa.Length <= 1) ? "default" : aa[1].Trim();
            //Reflection to make generic so as to change at only at class "ValueExpessionGeneralFunctions.cs" 
            //rest is taken care
            try
            {
                Type t = typeof(ValueExpessionGeneralFunctions);
                MethodInfo[] members = t.GetMethods();
                object classInstance = Activator.CreateInstance(t, null);
                foreach (MethodInfo mi in members)
                {
                    if (FunName.Trim().Contains(mi.Name))
                    {
                        if (mi.GetParameters().Length == 0)
                        {
                            mValueCalculated = mValueCalculated.Replace(p, mi.Invoke(classInstance, null).ToString());
                            break;
                        }
                        
                        string functionPattern = @"\b[^()]+\((.*)\)$";
                        string paramPattern = @"([^,]+\\(.+?\\))|([^,]+)";
                        List<string> FuncSplit = Regex.Split(FunName, functionPattern).ToList<string>();
                        FuncSplit.RemoveAll(x => x.Equals(""));
                        List<string> parameters = Regex.Split(FuncSplit[0].ToString(), paramPattern).ToList<string>();
                        parameters.RemoveAll(y => y.Equals(""));
                        parameters = parameters.Where(z => z.Contains("\"")).Select(z => z.Replace("\"", "")).ToList<string>();

                        object[] listOfParams = new object[parameters.Count];
                        int index = 0;
                        foreach (var item in parameters)
                        {
                            listOfParams[index++] = item;
                        }
                        string funcOut = mi.Invoke(classInstance, new object[] { listOfParams }).ToString();

                        mValueCalculated = mValueCalculated.Replace(p, funcOut);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Replace General Function Error:", ex);
            }
        }

        private void ReplaceRegExWithValue(string p, string[] a)
        {
            string pc = string.Join(" ", a);
            string[] dis = new[] { "Fun=", "Pat=", "P1=", "P2=" };

            string[] aa = pc.Split(dis, StringSplitOptions.RemoveEmptyEntries);

            string FunName = (aa.Length <= 1) ? "IsMatch" : aa[1].Trim();
            string Pattern = aa.Length <= 2 ? ".*" : aa[2].Trim();
            string P1 = aa.Length <= 3 ? "" : aa[3].Trim();
            if (string.IsNullOrEmpty(P1) == false)
            {
                if (P1.Contains("\""))
                    P1 = P1.Replace("\"", "");
            }

            string P2 = aa.Length <= 4 ? "" : aa[4].Trim();
            if (string.IsNullOrEmpty(P2) == false)
            {
                if (P2.Contains("\""))                
                    P2 = P2.Replace("\"", "");                
            }

            switch (FunName.Trim().ToLower())
            {
                case "replace":
                    mValueCalculated = mValueCalculated.Replace(p, new Regex(Pattern).Replace(P1, P2));
                    break;
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    MatchCollection Ms = new Regex(Pattern).Matches(P1);
                    if (Ms.Count > 0)
                        foreach (Match m in Ms)
                        {
                            mValueCalculated = mValueCalculated.Replace(p, m.Groups[Convert.ToInt32(FunName.Trim())].Value);
                            break;
                        }
                    else
                        mValueCalculated = "";
                    break;

                case "matchvalue":
                    Regex re = new Regex(Pattern);
                    Match match = re.Match(P1);
                    if (match.Success)
                        mValueCalculated = mValueCalculated.Replace(p, match.Value);
                    else
                        mValueCalculated = mValueCalculated.Replace(p, string.Empty);
                    break;

                case "ismatch":
                default:
                    mValueCalculated = mValueCalculated.Replace(p, new Regex(Pattern).IsMatch(P1).ToString());
                    break;
            }
        }

        private void ReplaceVarWithValue(string p, string[] a)
        {
            string VarName = null;
            string tmp = mValueCalculated;
            
            string suba = string.Join(" ", new ArraySegment<string>(a, 1, a.Length - 1));

            string[] ParamVal = suba.Split('=');
            {
                string ParamName = ParamVal[0];
                string Val = suba.Substring(suba.IndexOf('=') + 1);
                switch (ParamName)
                {
                    case "Name":
                        VarName = Val;
                        break;
                    default:
                        // TODO err unknown param
                        break;
                }
            }

            string VarValue;
            VariableBase vb = BF.GetHierarchyVariableByName(VarName);
            if (vb != null)
            {
                VarValue = vb.Value;
                mValueCalculated = tmp.Replace(p, VarValue);
            }

            //Use VBS instead of below
            else
            {
                //TODO: throw excpetion, log handler
                VarValue = "!!!" + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Not found!!! - " + a[1] + " <<<<<<<<<";
                mValueCalculated = VarValue;
            }
        }

        /// <summary>
        /// Static function to calculate string Expression like: "{Var Name=v1}"
        /// </summary>
        /// <param name="ProjEnvironment">Env is used for Env Params</param>
        /// <param name="BusinessFlow">Business Flow containing the Variables</param>
        /// <param name="Value">the Expression string</param>
        /// <returns></returns>
        public static string Calculate(ProjEnvironment ProjEnvironment, BusinessFlow BusinessFlow, string Value,ObservableList <DataSourceBase> DSList,bool bUpdate = false, string UpdateValue = "")
        {
            //TODO: this is static func, we can later on do cache and other stuff for performence if needed
            ValueExpression VE = new ValueExpression(ProjEnvironment, BusinessFlow, DSList, bUpdate,UpdateValue);
            VE.Value = Value;
            return VE.ValueCalculated;
        }
    }
}
