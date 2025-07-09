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
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.ValueExpression;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.RosLynLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using static GingerCore.Environments.Database;


namespace GingerCore
{
    public class ValueExpression : IValueExpression
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
        public static Regex rxVarPattern = new(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);

        public static readonly Regex rxVarFormulaParams = new(@"[A-Za-z]*=[^,}]*", RegexOptions.Compiled);

        public static Regex rxGlobalParamPattern = new(@"({GlobalAppsModelsParam Name=(\D*\d*\s*)}})|({GlobalAppsModelsParam Name=(\D*\d*\s*)})", RegexOptions.Compiled);

        private static Regex rxDSPattern = new(@"{(\bDS Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
        public static Regex rxEnvParamPattern = new(@"{(\bEnvParam App=)\w+\b[^{}]*}", RegexOptions.Compiled);
        public static Regex rxEnvDB = new(@"{\s*(EnvApp=\w+)\s+(EnvAppDB=\w+)\s+(Query=[^{}]+)\s*}", RegexOptions.Compiled);
        public static Regex rxEnvUrlPattern = new(@"{(\bEnvURL App=)\w+\b[^{}]*}", RegexOptions.Compiled);
        private static Regex rxFDPattern = new(@"{(\bFD Object=)\w+\b[^{}]*}", RegexOptions.Compiled);
        private static Regex rxExecutionJsonDataPattern = new(@"{ExecutionJsonData}", RegexOptions.Compiled);

        private static Regex VBSRegex = new(@"{[V|E|VBS]" + rxVar + "[^{}]*}", RegexOptions.Compiled);
        private static Regex rxe = new(@"{RegEx" + rxVare + ".*}", RegexOptions.Compiled | RegexOptions.Singleline);

        private static Regex rNestedfunc = new("{Function(\\s)*Fun(\\s)*=(\\s)*([a-zA-Z]|\\d)*\\(([^()])*\\)}", RegexOptions.Compiled);
        private static Regex MockDataExpPattern = new("{MockDataExp({.*}|[^{}]*)*}", RegexOptions.Compiled, new TimeSpan(0, 0, 5));
        private static Regex CsExppattern = new("{CS Exp({.*}|[^{}]*)*}", RegexOptions.Compiled);

        // Enable setting value simply by assigned string, 
        // so no need to create new VE class everywhere in code
        // Can simpliy do: ValueExpression VE = "{Var Name=V1}"
        public static string SolutionFolder = "";

        public static explicit operator ValueExpression(string Value)
        {
            ValueExpression VE = new ValueExpression(null, null)
            {
                Value = Value
            };
            return VE;
        }

        ObservableList<DataSourceBase> DSList;
        Context mContext;
        BusinessFlow BF;
        ProjEnvironment Env;
        bool bUpdate;
        string updateValue;
        bool bDone;


        public bool DecryptFlag { get; set; } = false;

        private string mEncryptedValue = null;


        public string EncryptedValue
        {
            get
            {
                if (mEncryptedValue == null)
                {
                    return mValueCalculated;
                }
                return mEncryptedValue;
            }
        }
        private string mValueCalculated = null;


        public string Value { get; set; }

        public string ValueCalculated
        {
            get
            {
                Calculate();
                return mValueCalculated;
            }
        }

        public string Calculate(string expression)
        {
            Value = expression;
            Calculate();
            return mValueCalculated;
        }

        public override string ToString()
        {
            return Value;
        }

        public ValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true)
        {
            this.Env = Env;
            this.BF = BF;
            this.DSList = DSList;
            this.bUpdate = bUpdate;
            this.updateValue = UpdateValue;
            this.bDone = bDone;

        }
        public ValueExpression(ProjEnvironment Env, Context ObjContext, ObservableList<DataSourceBase> DSList)
        {
            this.Env = Env;
            this.mContext = ObjContext;
            if (ObjContext != null)
            {
                this.BF = ObjContext.BusinessFlow;
            }
            this.DSList = DSList;
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
        public ValueExpression() { }
        //[IsSerializedForLocalRepository] // why we are serializing this
        public GingerCore.Actions.ActDSTableElement actDSTableElement { get; set; }

        //In case we need to pass VE to another control like grid -> edit then we fill this 2 values
        public Object Obj { get; set; }
        public string ObjAttr { get; set; }




        private void Calculate()
        {
            if (string.IsNullOrEmpty(Value))
            {
                mValueCalculated = "";
                return;
            }
            mValueCalculated = Value;
            mEncryptedValue = null;

            //Do the operation based on order
            //First replace Vars - since they can appear in other func like VBS v1+v2 or VBS mid(v1,1,4);
            int expressionCounter = 0;
            while (mValueCalculated.Contains('{') && expressionCounter < 10 && ContainsFormula(mValueCalculated))
            {
                expressionCounter++;
                ReplaceGlobalParameters();
                //replace environment parameters which embedded into functions like VBS
                ReplaceEnvVars();
                ReplaceEnvDB();
                CalculateComplexFormulas();
                ReplaceDataSources();
                ProcessGeneralFuncations();
                EvaluateFlowDetails();
                EvaluateCSharpFunctions();
                EvaluateBogusDataGenrateFunctions();

            }

            if (!string.IsNullOrEmpty(SolutionFolder))
            {

                if (WorkSpace.Instance != null && WorkSpace.Instance.SolutionRepository != null)
                {
                    mValueCalculated = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(mValueCalculated);
                }
                else if (!string.IsNullOrWhiteSpace(SolutionFolder))
                {
                    if (mValueCalculated.StartsWith("~"))
                    {
                        mValueCalculated = mValueCalculated.TrimStart(new char[] { '~', '\\', '/' });
                        mValueCalculated = Path.Combine(SolutionFolder, mValueCalculated);
                    }

                }
            }
            if (mValueCalculated.StartsWith(@"\~"))
            {
                mValueCalculated = "~" + mValueCalculated[2..];
            }
        }

        #region Flow Details

        private void EvaluateFlowDetails()
        {
            MatchCollection matches = rxFDPattern.Matches(mValueCalculated);
            MatchCollection matchesExecutionJsonData = rxExecutionJsonDataPattern.Matches(mValueCalculated);
            if (matches.Count == 0 && matchesExecutionJsonData.Count == 0)
            {
                return;
            }

            foreach (Match match in matches)
            {
                try
                {
                    ReplaceFlowDetails(match.Value);
                }
                catch (Exception ex)
                {
                    mValueCalculated = mValueCalculated.Replace(match.Value, string.Format("['{0}' Expression is not valid, Error:'{1}']", match.Value, ex.Message));
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}'", match.Value), ex);
                }
            }

            foreach (Match match in matchesExecutionJsonData)
            {
                try
                {
                    ReplaceExecutionJsonDataDetails(match.Value);
                }
                catch (Exception ex)
                {
                    mValueCalculated = mValueCalculated.Replace(match.Value, string.Format("['{0}' Expression is not valid, Error:'{1}']", match.Value, ex.Message));
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}'", match.Value), ex);
                }
            }
        }

        private void ReplaceExecutionJsonDataDetails(string josnExpression)
        {
            //Select fields from selected template configuration
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            HTMLReportConfiguration defaultTemplate = null;
            if (mContext?.RunsetAction != null)
            {
                RunSetActionSendDataToExternalSource runSetAction = (RunSetActionSendDataToExternalSource)mContext.RunsetAction;
                defaultTemplate = HTMLReportConfigurations.FirstOrDefault(x => (x.ID == runSetAction.selectedHTMLReportTemplateID));
                if (defaultTemplate == null && runSetAction.selectedHTMLReportTemplateID == 100)
                {
                    defaultTemplate = HTMLReportConfigurations.FirstOrDefault(x => x.IsDefault);
                }
            }
            else
            {
                defaultTemplate = HTMLReportConfigurations.FirstOrDefault(x => x.IsDefault);
            }

            //Get Last Execution details
            LiteDbRunSet liteDbRunSet = null;
            var loggerMode = WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod;
            if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                LiteDbManager dbManager = new LiteDbManager(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
                liteDbRunSet = dbManager.GetLatestExecutionRunsetData("");
            }
            else
            {
                mValueCalculated = mValueCalculated.Replace(josnExpression, string.Format("['{0}' Expression Supported only in case the selected Execution Logger type is DB.]", josnExpression));
                return;
            }

            string json = ((ExecutionLogger)WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.ExecutionLoggerManager.mExecutionLogger).CalculateExecutionJsonData(liteDbRunSet, defaultTemplate);

            mValueCalculated = mValueCalculated.Replace(josnExpression, json);
        }
        public enum eFlowDetailsObjects
        {
            Environment, Runset, Runner, BusinessFlow, ActivitiesGroup, Activity, Action, PreviousBusinessFlow, PreviousActivity, PreviousAction, LastFailedAction, ErrorHandlerOriginActivitiesGroup, ErrorHandlerOriginActivity, ErrorHandlerOriginAction, LastFailedBusinessFlow, LastFailedActivity, Solution
        }
        public static Tuple<eFlowDetailsObjects, string> GetFlowDetailsParams(string flowDetailsExpression)
        {
            try
            {
                string objStr = string.Empty;
                string field = string.Empty;
                int FieldStartIndex = flowDetailsExpression.IndexOf("Field");
                int ObjIndex = flowDetailsExpression.IndexOf("Object");
                int LastIndex = flowDetailsExpression.IndexOf("}");
                objStr = flowDetailsExpression.Substring(11, FieldStartIndex - 12);
                field = flowDetailsExpression.Substring(FieldStartIndex + 6, flowDetailsExpression.Length - (FieldStartIndex + 7)).Replace("\"", "").Trim();

                //convert string object to enum
                if (Enum.TryParse(objStr, out eFlowDetailsObjects objEnum) == false)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}' due to invalid 'Object'", flowDetailsExpression));
                    return null;
                }
                return new Tuple<eFlowDetailsObjects, string>(objEnum, field);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}' due to wrong format", flowDetailsExpression));
                return null;
            }
        }

        public static Mockdata GetMockDataDatasetsFunction(string MockDataExpression)
        {
            try
            {
                string objStr, functions, Locale;
                int DatasetStartIndex, DatasetEndIndex, LocaleStartIndex, LocaleEndIndex;
                MockdataExpressionExtract(MockDataExpression, out objStr, out functions, out Locale, out DatasetStartIndex, out DatasetEndIndex, out LocaleStartIndex, out LocaleEndIndex);
                Mockdata mockdata = new()
                {
                    MockDataDatasets = objStr,
                    Locale = Locale,
                    Function = functions,
                    MockExpression = MockDataExpression,
                    DatasetStartIndex = DatasetStartIndex,
                    DatasetEndIndex = DatasetEndIndex,
                    LocaleStartIndex = LocaleStartIndex,
                    LocaleEndIndex = LocaleEndIndex
                };
                return mockdata;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}' due to wrong format", MockDataExpression));
                return null;
            }
        }
        /// <summary>
        /// This method extracts specific parts from the MockDataExpression string. 
        /// It initializes several string variables and then uses indices to parse substrings, 
        /// assigning these substrings to the initialized variables.
        /// </summary>
        /// <param name="MockDataExpression">
        /// <param name="objStr"></param>
        /// <param name="functions"></param>
        /// <param name="Locale"></param>
        /// <param name="DatasetStartIndex"></param> Index of '=' in MockDataExpression.
        /// <param name="DatasetEndIndex"></param> Index of '(' in MockDataExpression.
        /// <param name="LocaleStartIndex"></param> Index of '"' in MockDataExpression.
        /// <param name="LocaleEndIndex"></param> Index of ')' in MockDataExpression.
        private static void MockdataExpressionExtract(string MockDataExpression, out string objStr, out string functions, out string Locale, out int DatasetStartIndex, out int DatasetEndIndex, out int LocaleStartIndex, out int LocaleEndIndex)
        {
            objStr = string.Empty;
            functions = string.Empty;
            Locale = string.Empty;
            DatasetStartIndex = MockDataExpression.IndexOf('=');
            DatasetEndIndex = MockDataExpression.IndexOf('(');
            LocaleStartIndex = MockDataExpression.IndexOf('"');
            LocaleEndIndex = MockDataExpression.IndexOf(')');
            int functionstartindex = MockDataExpression.IndexOf('.');
            string funsubstring = MockDataExpression[(functionstartindex + 1)..];
            int functionendindex = funsubstring.IndexOf('(');
            Locale = MockDataExpression.Substring(LocaleStartIndex + 1, LocaleEndIndex - 2 - LocaleStartIndex);
            objStr = MockDataExpression.Substring(DatasetStartIndex + 1, DatasetEndIndex - 1 - DatasetStartIndex);
            functions = funsubstring[..functionendindex].Replace("\"", "").Trim();
        }

        private void ReplaceFlowDetails(string flowDetailsExpression)
        {
            eFlowDetailsObjects obj;
            string field = string.Empty;
            string value = string.Empty;
            Tuple<eFlowDetailsObjects, string> expParams = GetFlowDetailsParams(flowDetailsExpression);
            if (expParams == null)
            {
                mValueCalculated = mValueCalculated.Replace(flowDetailsExpression, string.Format("['{0}' Expression is not valid]", flowDetailsExpression));
                return;
            }
            obj = expParams.Item1;
            field = expParams.Item2;

            RunSetConfig runset = null;
            GingerRunner runner = null;
            if (WorkSpace.Instance.RunsetExecutor != null)
            {
                runset = WorkSpace.Instance.RunsetExecutor.RunSetConfig;
            }
            if (runset != null)
            {
                runner = WorkSpace.Instance.RunsetExecutor.Runners.FirstOrDefault(x => x.Executor.BusinessFlows.Contains(this.BF));
            }

            RepositoryItemBase objtoEval = null;
            switch (obj)
            {
                case eFlowDetailsObjects.Environment:
                    objtoEval = this.Env;
                    break;
                case eFlowDetailsObjects.Runset:
                    objtoEval = runset;
                    break;
                case eFlowDetailsObjects.Runner:
                    objtoEval = runner;
                    break;
                case eFlowDetailsObjects.LastFailedBusinessFlow:
                    if (runner != null)
                    {
                        objtoEval = ((GingerExecutionEngine)runner.Executor).LastFailedBusinessFlow;
                    }
                    break;
                case eFlowDetailsObjects.BusinessFlow:
                    objtoEval = this.BF;
                    break;
                case eFlowDetailsObjects.ActivitiesGroup:
                    objtoEval = this.BF.CurrentActivitiesGroup;
                    break;
                case eFlowDetailsObjects.Activity:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.CurrentActivity;
                    }
                    break;
                case eFlowDetailsObjects.Action:
                    objtoEval = (RepositoryItemBase)this.BF.CurrentActivity.Acts.CurrentItem;
                    break;
                case eFlowDetailsObjects.PreviousBusinessFlow:
                    if (runner != null)
                    {
                        objtoEval = ((GingerExecutionEngine)runner.Executor).PreviousBusinessFlow;
                    }
                    break;
                case eFlowDetailsObjects.PreviousActivity:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.PreviousActivity;
                    }
                    break;
                case eFlowDetailsObjects.PreviousAction:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.PreviousAction;
                    }
                    break;
                case eFlowDetailsObjects.LastFailedAction:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.LastFailedAction;
                    }
                    break;
                case eFlowDetailsObjects.ErrorHandlerOriginActivitiesGroup:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.ErrorHandlerOriginActivitiesGroup;
                    }
                    break;
                case eFlowDetailsObjects.ErrorHandlerOriginActivity:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.ErrorHandlerOriginActivity;
                    }
                    break;
                case eFlowDetailsObjects.ErrorHandlerOriginAction:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.ErrorHandlerOriginAction;
                    }
                    break;
                case eFlowDetailsObjects.LastFailedActivity:
                    if (this.BF != null)
                    {
                        objtoEval = this.BF.LastFailedActivity;
                    }
                    break;
                case eFlowDetailsObjects.Solution:
                    if (WorkSpace.Instance != null)
                    {
                        objtoEval = WorkSpace.Instance.Solution;
                    }
                    break;
            }
            if (objtoEval != null)
            {
                value = GetValueFromReflection(objtoEval, field);
                if (value != null)
                {
                    mValueCalculated = mValueCalculated.Replace(flowDetailsExpression, value);
                    return;
                }
                else
                {
                    mValueCalculated = mValueCalculated.Replace(flowDetailsExpression, string.Format("['{0}' Expression is not valid]", flowDetailsExpression));
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to evaluate flow details expression '{0}' due to invalid 'Field'", flowDetailsExpression));
                    return;
                }
            }
            else
            {
                mValueCalculated = mValueCalculated.Replace(flowDetailsExpression, "N/A");//obj is null so representing as empty field
            }
        }

        private static readonly JsonSerializerOptions s_writeOptions = new()
        {
            WriteIndented = true
        };

        private string GetValueFromReflection(object obj, string field)
        {
            object value = null;
            PropertyInfo prop = obj.GetType().GetProperty(field);
            FieldInfo fieldInfo = null;
            if (prop != null)
            {
                value = prop.GetValue(obj);
            }
            else
            {
                fieldInfo = obj.GetType().GetField(field);
                if (fieldInfo != null)
                {
                    value = fieldInfo.GetValue(obj);
                }
            }
            if (prop != null || fieldInfo != null)
            {
                if (value != null)
                {
                    // Check if the provided value is an IEnumerable but not a string  
                    if (value is not string and System.Collections.IEnumerable enumerableValue)
                    {
                        // Serialize the enumerable value to JSON using the specified write options  
                        return JsonSerializer.Serialize(enumerableValue, s_writeOptions);
                    }
                    else
                    {
                        // If the value is a string and not an enumerable,  
                        // return its string representation  
                        return value.ToString();
                    }
                }
                else
                {
                    return "N/A";
                }
            }
            else
            {
                return null;
            }
        }
        #endregion Flow Details

        private void EvaluateCSharpFunctions()
        {
            mValueCalculated = CodeProcessor.GetResult(mValueCalculated);
        }

        private void EvaluateBogusDataGenrateFunctions()
        {
            mValueCalculated = CodeProcessor.GetBogusDataGenerateresult(mValueCalculated);
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
            VarName = VarName[..^1];

            ObservableList<GlobalAppModelParameter> ModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            GlobalAppModelParameter Param = ModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == VarName);

            if (Param != null)
            {
                //if (DecryptFlag == true)
                //{
                //    bool res = false;
                //    String strValuetoPass;
                //    strValuetoPass = EncryptionHandler.DecryptString(Param.CurrentValue, ref res);
                //    if (res == true)
                //        mValueCalculated = mValueCalculated.Replace(p, strValuetoPass);
                //    else
                //        mValueCalculated = mValueCalculated.Replace(p, Param.CurrentValue);
                //}
                //else

                mValueCalculated = mValueCalculated.Replace(p, Param.CurrentValue);
            }
            else
            {
                mValueCalculated = mValueCalculated.Replace(p, string.Format("ERROR: The Global Model Parameter '{0}' was not found", VarName));
            }
        }


        // this is costly !!!!!!!!!!!!1 we find the matches... then destry and using regex, search for {
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
            MatchCollection matcheVBS = VBSRegex.Matches(VE);
            if (matcheVBS.Count > 0) { return true; }

            return false;
        }
        /// <summary>
        /// This function checks if the string is a value expression of any kind
        /// </summary>
        /// <param name="VE">String that is possibly a value expression</param>
        /// <returns></returns>
        public static bool IsThisAValueExpression(string VE)
        {
            if (string.IsNullOrEmpty(VE))
            {
                return false;
            }

            if (IsThisDynamicVE(VE))
            {
                return true;
            }

            if (VE.Contains("{CS Exp="))
            {
                return true;
            }

            MatchCollection functionMatches = rNestedfunc.Matches(VE);

            if (functionMatches.Count > 0)
            {
                return true;
            }

            MatchCollection FDObjectMatches = rxFDPattern.Matches(VE);

            if (FDObjectMatches.Count > 0)
            {
                return true;
            }

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
                        // Adding this to update value only for the main DS Expression .. not VE in Parameter
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
                    {
                        bUpdate = true;
                    }
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

            string DSName = p[9..p.IndexOf(" DST=")];

            if (DSList == null || !DSList.Any())
            {
                DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            }

            foreach (DataSourceBase ds in DSList)
            {
                if (ds.Name == DSName)
                {
                    DataSource = ds;
                }
            }

            p = p[p.IndexOf(" DST=")..].Trim();
            if (DataSource == null)
            {
                // whenever error is to be displayed, do not use Replace,
                // use only the error string otherwise the output might look anamalous
                // eg: //*[text()="{Error: The Data Source Variable was not found }"]

                mValueCalculated = string.Format("ERROR: The Data Source Variable '{0}' was not found", DSName);
                return;
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

            if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
            {
                //if (DataSource.FileFullPath.StartsWith("~"))
                //{
                //    DataSource.FileFullPath = DataSource.FileFullPath.Replace(@"~\","").Replace("~", "");
                //    DataSource.FileFullPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, DataSource.FileFullPath);
                //}
                DataSource.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DataSource.FileFullPath);
                try
                {
                    DSTable = p.Substring(p.IndexOf("DST=") + 4, p.IndexOf(" ") - 4);
                    mColList = DataSource.GetColumnList(DSTable);
                    p = p[p.TrimStart().IndexOf(" ")..].Trim();

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
                        else if (sAct == "ARC") // Get Available Row Count
                        {
                            Query = "Select COUNT(*) FROM " + DSTable + " WHERE GINGER_USED <> 'True' or GINGER_USED is null";
                            p = "";
                        }
                        else if (sAct == "ETE") // Export to Excel
                        {
                            Query = "";
                            p = p[p.TrimStart().IndexOf(" ")..].Trim();
                        }
                        else
                        {
                            p = p[p.TrimStart().IndexOf(" ")..].Trim();
                        }
                    }
                    if (p.IndexOf("EP=") != -1)
                    {
                        ExcelPath = p.Substring(p.IndexOf("EP=") + 3, p.IndexOf(" ES") - 3);
                        p = p[p.TrimStart().IndexOf(" ES")..].Trim();
                        ExcelSheet = p.Substring(p.IndexOf("ES=") + 3, p.IndexOf("}") - 3);
                    }
                    else if (p.IndexOf("KEY=") != -1)
                    {
                        string KeyName = p.Substring(p.IndexOf("KEY=") + 4, p.IndexOf("}") - 4);
                        if (sAct == "DR")
                        {
                            updateQuery = "DELETE FROM " + DSTable + " WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                        }
                        else
                        {
                            if (bUpdate == true)
                            {
                                DataTable dtTemp = DataSource.GetQueryOutput("Select count(*) from " + DSTable + " where GINGER_KEY_NAME= '" + KeyName + "'");
                                if (dtTemp.Rows[0].ItemArray[0].ToString() != "0")
                                {
                                    updateQuery = "UPDATE " + DSTable + " SET GINGER_KEY_VALUE = '" + updateValue.Replace("'", "''") + "',GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "',GINGER_LAST_UPDATE_DATETIME='" + DateTime.Now.ToString() + "' WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                                }
                                else
                                {
                                    updateQuery = "INSERT INTO " + DSTable + "(GINGER_KEY_NAME,GINGER_KEY_VALUE,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME) VALUES ('" + KeyName + "','" + updateValue.Replace("'", "''") + "','" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "')";
                                }
                            }
                            else
                            {
                                Query = "Select GINGER_KEY_VALUE FROM " + DSTable + " WHERE GINGER_KEY_NAME = '" + KeyName + "'";
                            }
                        }
                    }
                    else if (p != "" && (sAct == "MASD" || sAct == "DR" || sAct == ""))
                    {
                        bMarkAsDone = p.Substring(p.IndexOf("MASD=") + 5, p.IndexOf(" ") - 5);
                        p = p[p.TrimStart().IndexOf(" ")..].Trim();
                        if (p.IndexOf("MR=") == 0)
                        {
                            bMultiRow = p.Substring(p.IndexOf("MR=") + 3, p.IndexOf(" ") - 3);
                            p = p[p.TrimStart().IndexOf(" ")..].Trim();
                        }
                        string DSIden = p.Substring(p.IndexOf("IDEN=") + 5, p.IndexOf(" ") - 5);
                        p = p[p.TrimStart().IndexOf(" ")..].Trim();
                        if (DSIden == "Query")
                        {
                            Query = p.Substring(p.IndexOf("QUERY=") + 6, p.Length - 7);
                            if (Query.ToUpper().IndexOf("SELECT *") == -1)
                            {
                                Query = Regex.Replace(Query, " FROM ", ",[GINGER_ID] FROM ", RegexOptions.IgnoreCase);
                            }
                        }
                        else
                        {
                            Query = "Select ";
                            iColVal = p.Substring(p.IndexOf("ICOLVAL=") + 8, p.IndexOf("IROW=") - 9);
                            iColVal = "[" + iColVal + "]";
                            p = p[p.TrimStart().IndexOf("IROW=")..];
                            if (iColVal != "[]")
                            {
                                Query = Query + iColVal + ",[GINGER_ID] from " + DSTable;
                            }
                            else
                            {
                                Query = "Select [GINGER_ID] from " + DSTable;
                            }
                            if (p.IndexOf(" ") > 0)
                            {
                                IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf(" ") - 5);
                            }
                            else
                            {
                                IRow = p.Substring(p.IndexOf("IROW=") + 5, p.IndexOf("}") - 5);
                            }

                            if (IRow == "NxtAvail")
                            {
                                Query = Query + " Where GINGER_USED <> 'True' or GINGER_USED is null";
                            }
                            else if (IRow == "RowNum")
                            {
                                p = p[p.TrimStart().IndexOf("ROWNUM=")..];
                                rowNum = p.Substring(p.IndexOf("ROWNUM=") + 7, p.IndexOf("}") - 7);
                            }
                            else if (IRow == "Where")
                            {
                                if (p.TrimStart().IndexOf("COND=") != -1)
                                {
                                    p = p[p.TrimStart().IndexOf("COND=")..];
                                    string Cond = p.Substring(p.IndexOf("COND=") + 5, p.IndexOf("}") - 5);
                                    Query = Query + " Where " + Cond;
                                }
                                else if (p.TrimStart().IndexOf("WCOLVAL=") != -1 && p.TrimStart().IndexOf("WOPR=") != -1)
                                {
                                    p = p[p.TrimStart().IndexOf("WCOLVAL=")..];
                                    string wColVal = p.Substring(p.IndexOf("WCOLVAL=") + 8, p.IndexOf("WOPR=") - 9);
                                    wColVal = "[" + wColVal + "]";
                                    Query = Query + " Where ";
                                    p = p[p.TrimStart().IndexOf("WOPR=")..];
                                    string wOpr = "";
                                    string wRowVal = "";
                                    if (p.IndexOf("WROWVAL=") == -1)
                                    {
                                        wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("}") - 5);
                                    }
                                    else
                                    {
                                        wOpr = p.Substring(p.IndexOf("WOPR=") + 5, p.IndexOf("WROWVAL=") - 6);
                                    }

                                    if (wOpr is not "Is Null" and not "Is Null")
                                    {
                                        p = p[p.TrimStart().IndexOf("WROWVAL=")..];
                                        wRowVal = p.Substring(p.IndexOf("WROWVAL=") + 8, p.IndexOf("}") - 8);
                                    }
                                    if (wOpr == "Equals")
                                    {
                                        if (wColVal == "[GINGER_ID]")
                                        {
                                            Query = Query + wColVal + " = " + wRowVal + "";
                                        }
                                        else
                                        {
                                            Query = Query + wColVal + " = '" + wRowVal + "'";
                                        }
                                    }
                                    else if (wOpr == "NotEquals")
                                    {
                                        if (wColVal == "[GINGER_ID]")
                                        {
                                            Query = Query + wColVal + " <> " + wRowVal + "";
                                        }
                                        else
                                        {
                                            Query = Query + wColVal + " <> '" + wRowVal + "'";
                                        }
                                    }
                                    else if (wOpr == "Contains")
                                    {
                                        Query = Query + wColVal + " LIKE " + "'%" + wRowVal + "%'";
                                    }
                                    else if (wOpr == "Not Contains")
                                    {
                                        Query = Query + wColVal + " NOT LIKE " + "'%" + wRowVal + "%'";
                                    }
                                    else if (wOpr == "Starts With")
                                    {
                                        Query = Query + wColVal + " LIKE '" + wRowVal + "%'";
                                    }
                                    else if (wOpr == "Not Starts With")
                                    {
                                        Query = Query + wColVal + " NOT LIKE '" + wRowVal + "%'";
                                    }
                                    else if (wOpr == "Ends With")
                                    {
                                        Query = Query + wColVal + " LIKE '%" + wRowVal + "'";
                                    }
                                    else if (wOpr == "Not Ends With")
                                    {
                                        Query = Query + wColVal + " NOT LIKE '%" + wRowVal + "'";
                                    }
                                    else if (wOpr == "Is Null")
                                    {
                                        Query = Query + wColVal + " IS NULL";
                                    }
                                    else if (wOpr == "Is Not Null")
                                    {
                                        Query = Query + wColVal + " IS NOT NULL";
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mValueCalculated = pOrg;
                    Console.WriteLine(e.StackTrace);
                }
                if (Query != "")
                {
                    DataTable dt = DataSource.GetQueryOutput(Query);
                    if (dt == null)
                    {
                        return;//need to check why it return null
                    }
                    if (dt.Rows.Count == 0 && IRow == "NxtAvail" && bUpdate == true)
                    {
                        DataSource.RunQuery("INSERT INTO " + DSTable + "(GINGER_USED) VALUES ('False')");
                        dt = DataSource.GetQueryOutput(Query);
                    }
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        mValueCalculated = "No Row Found";
                        return;
                    }
                    if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                    {
                        if (rowNum.All(char.IsDigit))
                        {
                            mValueCalculated = mValueCalculated.Replace(pOrg, dt.Rows[Convert.ToInt32(rowNum)].ItemArray[0].ToString());
                        }
                        else
                        {
                            mValueCalculated = "ERROR: Not Valid RowNum:" + rowNum;
                        }
                    }

                    string GingerIds = "";
                    if (dt.Columns.Contains("GINGER_ID"))
                    {
                        if (bMultiRow == "Y")
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                GingerIds += row["GINGER_ID"].ToString() + ",";
                            }

                            GingerIds = GingerIds[..^1];
                        }
                        else
                        {
                            GingerIds = dt.Rows[Convert.ToInt32(rowNum)]["GINGER_ID"].ToString();
                        }
                    }
                    if (bUpdate == true)
                    {
                        if (updateQuery == "")
                        {
                            if (iColVal == "")
                            {
                                iColVal = dt.Columns[0].ColumnName;
                            }

                            if (updateValue == null)
                            {
                                updateValue = string.Empty;
                            }
                            updateQuery = "UPDATE " + DSTable + " SET ";
                            foreach (DataColumn sCol in dt.Columns)
                            {
                                if (!new List<string> { "GINGER_ID", "GINGER_LAST_UPDATED_BY", "GINGER_LAST_UPDATE_DATETIME", "GINGER_KEY_NAME" }.Contains(sCol.ColumnName))
                                {
                                    updateQuery += "[" + sCol.ColumnName + "]='" + updateValue.Replace("'", "''") + "' ,";
                                }
                            }
                            updateQuery = updateQuery[..^1];
                            if (mColList.Contains("GINGER_LAST_UPDATED_BY"))
                            {
                                updateQuery = updateQuery + ",GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "' ";
                            }

                            if (mColList.Contains("GINGER_LAST_UPDATE_DATETIME"))
                            {
                                updateQuery = updateQuery + ",GINGER_LAST_UPDATE_DATETIME = '" + DateTime.Now.ToString() + "' ";
                            }

                            updateQuery = updateQuery + "WHERE GINGER_ID IN (" + GingerIds + ")";
                        }
                        DataSource.RunQuery(updateQuery);
                    }
                    if (bMarkAsDone == "Y" && bDone == true)
                    {
                        DataSource.RunQuery("UPDATE " + DSTable + " SET GINGER_USED ='True' WHERE GINGER_ID IN (" + GingerIds + ")");
                    }
                    else if (sAct == "DR" && bDone == true)
                    {
                        DataSource.RunQuery("DELETE FROM " + DSTable + " WHERE GINGER_ID IN  (" + GingerIds + ")");
                    }
                }
                else if (updateQuery != "" && bDone == true)
                {
                    DataSource.RunQuery(updateQuery);
                    mValueCalculated = "";
                }

            }
            else if (DataSource.DSType == DataSourceBase.eDSType.LiteDataBase)
            {
                try
                {
                    //DataBase connection 
                    GingerCoreNET.DataSource.GingerLiteDB liteDB = new GingerCoreNET.DataSource.GingerLiteDB
                    {
                        FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DataSource.FileFullPath)
                    };

                    // Getting all values to execute query
                    int rowNumber = 0;
                    bool Markasdone = false;

                    string litedbquery = p.Substring(p.IndexOf("QUERY=") + 6, p.Length - (p.IndexOf("QUERY=") + 7));
                    bool IsQueryOld = IsQueryFromOldVersion(litedbquery);
                    // Query is with Customized option
                    if (p.Contains("ICOLVAL="))
                    {
                        string[] tokens = p.Split(new[] { "ICOLVAL=" }, StringSplitOptions.None);
                        char[] splitchar = { ' ' };
                        string[] Name = tokens[1].Split(splitchar);

                        string[] markasdone = tokens[0].Split(new[] { "MASD=" }, StringSplitOptions.None)[1].Split(splitchar);
                        if (!bDone)
                        {
                            markasdone[0] = "N";
                        }
                        string[] tableName = tokens[0].Split(new[] { "DST=" }, StringSplitOptions.None)[1].Split(splitchar);

                        iColVal = Name[0];

                        string[] irow = Name[1].Split(new[] { "IROW=" }, StringSplitOptions.None);
                        IRow = irow[1];

                        if (IRow == "NxtAvail")
                        {
                            rowNumber = 0;
                            if (IsQueryOld)
                            {
                                litedbquery = $"SELECT $ FROM {tableName[0]} where GINGER_USED =\"False\"";
                            }
                        }
                        else if (IRow == "RowNum")
                        {
                            string[] rownum = Name[2].Split(new[] { "ROWNUM=" }, StringSplitOptions.None)[1].Split(splitchar);
                            rowNumber = Int32.Parse(rownum[0]);

                            if (IsQueryOld)
                            {
                                litedbquery = $"SELECT $ FROM {tableName[0]}";
                            }
                        }

                        else if (IRow.ToLower().Equals("where"))
                        {
                            if (IsQueryOld)
                            {
                                litedbquery = $"SELECT $ FROM {tableName[0]} {litedbquery[litedbquery.IndexOf("where", StringComparison.CurrentCultureIgnoreCase)..]}";
                            }
                        }
                        if (markasdone[0] == "Y")
                        {
                            Markasdone = true;
                        }
                        // Get Value query
                        if (litedbquery.Contains("select", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Use Replace because incase, if it is used with something else for example xpath (//*[text() = "<DataSource Query>"]) the whole string should be the output instead of just the result
                            // data source query
                            mValueCalculated = mValueCalculated.Replace(pOrg, liteDB.GetQueryOutput(litedbquery, Name[0], rowNumber, Markasdone, tableName[0]));
                        }

                        // Set value Query
                        else if (litedbquery.Contains("update", StringComparison.CurrentCultureIgnoreCase) && this.updateValue != null)
                        {
                            if (litedbquery.Contains("where", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string[] querysplit = litedbquery.Split(new[] { "where" }, StringSplitOptions.None);
                                char[] split = { ' ' };
                                string[] Token = querysplit[0].Split(new[] { "=" }, StringSplitOptions.None);

                                litedbquery = Token[0] + "=\"" + updateValue + "\" where " + querysplit[1];
                            }
                            else
                            {
                                litedbquery = litedbquery.Replace("\"\"", "\"" + updateValue + "\"");
                            }
                            bool nextavail = false;
                            if (IRow == "NxtAvail")
                            {
                                nextavail = true;
                            }

                            if (litedbquery != "")
                            {
                                liteDB.RunQuery(litedbquery, rowNumber, tableName[0], Markasdone, nextavail);
                                mValueCalculated = "";
                            }

                        }
                    }
                    else
                    {
                        string[] tokens = p.Split(new[] { "MASD=" }, StringSplitOptions.None);
                        char[] splitchar = { ' ' };
                        string[] Token = tokens[1].Split(splitchar);

                        string[] col = tokens[0].Split(new[] { "DST=" }, StringSplitOptions.None)[1].Split(splitchar);
                        if (Token[0] == "Y")
                        {
                            Markasdone = true;
                        }
                        if (litedbquery.Contains("update"))
                        {
                            if (litedbquery.Contains("where"))
                            {
                                string[] querysplit = litedbquery.Split(new[] { "where" }, StringSplitOptions.None);
                                char[] split = { ' ' };
                                string[] token1 = querysplit[0].Split(new[] { "=" }, StringSplitOptions.None);

                                litedbquery = token1[0] + "=\"" + updateValue + "\" where " + querysplit[1];
                            }
                            else
                            {
                                litedbquery = litedbquery.Replace("\"\"", "\"" + updateValue + "\"");
                            }
                            liteDB.GetResult(litedbquery);
                        }
                        else
                        {
                            // Use Replace because incase, if it is used with something else for example xpath (//*[text() = "<DataSource Query>"]) the whole string should be the output instead of just the result
                            // data source query

                            mValueCalculated = mValueCalculated.Replace(pOrg, liteDB.GetResut(litedbquery, col[0], Markasdone));
                        }
                    }
                }
                catch (Exception e)
                {
                    mValueCalculated = e.Message.Contains("There is no row at") ? "No Row Found" : pOrg;
                    // Is there a reason to print an error in the console? 
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static bool IsQueryFromOldVersion(string Query)
        {
            if (Query.Contains(".find", StringComparison.CurrentCultureIgnoreCase) || Query.Contains(".select", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }


        private void CalculateComplexFormulas()
        {
            string value = mValueCalculated;
            MatchCollection matches = rxe.Matches(value);
            MatchCollection ms;
            if (matches.Count == 0)
            {
                // no variables found
                matches = VBSRegex.Matches(value);
                if (matches.Count == 0)
                {
                    matches = rNestedfunc.Matches(value);

                    if (matches.Count == 0)
                    {
                        return;
                    }

                }
            }

            // found matched replace with var(s) funcs etc... value   
            foreach (Match match in matches)
            {
                ms = VBSRegex.Matches(match.Value);
                int iCount = 0;// defining no to go in endless loop
                while (ms.Count > 0 && iCount < 10)
                {
                    foreach (Match m in ms)
                    {
                        HandleComplexFormula(m.Value);
                    }
                    ms = VBSRegex.Matches(mValueCalculated);
                    iCount++;
                }
            }

            value = mValueCalculated;
            matches = rxe.Matches(value);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    HandleComplexFormula(match.Value);
                }

            }

        }

        private void ProcessGeneralFuncations()
        {
            var mc = rNestedfunc.Matches(mValueCalculated);

            if (mc.Count == 0)
            {
                return;
            }
            foreach (Match m in mc)
            {
                HandleComplexFormula(m.Value);
            }
            ProcessGeneralFuncations();
        }

        private Dictionary<string, string> GetExtraParam(string p)
        {
            Dictionary<string, string> extraParamDict = [];
            List<Match> matches = rxVarFormulaParams.Matches(p).ToList();

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];

                if (!string.IsNullOrEmpty(match.Value) && match.Value.Contains('='))
                {
                    string[] keyValue = match.Value.Split('=');
                    extraParamDict.TryAdd(keyValue[0].Trim(), keyValue[1]);
                }
            }

            return extraParamDict;
        }
        private void ReplaceEnvDB()
        {
            MatchCollection envParamMatches = rxEnvDB.Matches(mValueCalculated);
            foreach (Match match in envParamMatches)
            {
                ReplaceEnvDBWithValue(match.Value);
            }
        }
        private void ReplaceEnvVars()
        {
            MatchCollection envParamMatches = rxEnvParamPattern.Matches(mValueCalculated);
            foreach (Match match in envParamMatches)
            {
                ReplaceEnvParamWithValue(match.Value, null);
            }

            MatchCollection envUrlMatches = rxEnvUrlPattern.Matches(mValueCalculated);
            foreach (Match match2 in envUrlMatches)
            {
                ReplaceEnvURLWithValue(match2.Value, null);
            }
        }

        private void HandleComplexFormula(string p)
        {
            string pc = p[1..^1];
            string[] a = pc.Split(' ');
            string expressionType = a[0];

            switch (expressionType)
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
            bool FailonUnix = false;
            try
            {
                string Expr = p.Replace("\r\n", "vbCrLf");
                Expr = Expr[1..^1];
                Expr = Expr.Replace("VBS Eval=", "");
                //check whether the Expr contains Split.If yes the take user entered number and decreased it to -1
                if (p.Contains("{VBS Eval"))
                {


                    FailonUnix = true;
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
            if (FailonUnix && !System.Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {


                throw new PlatformNotSupportedException("VBS functions are not supported on Unix/Mac systems");
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
        public static void GetEnvAppFromEnvURL(string EnvValExp, ref string AppName)
        {

            AppName = EnvValExp.Replace("\r\n", "vbCrLf");
            AppName = AppName[1..^1];
            AppName = AppName.Replace("EnvURL App=", "");
        }


        private void ReplaceEnvURLWithValue(string p, string[] a)
        {
            string AppName = null;
            string URL = null;
            GetEnvAppFromEnvURL(p, ref AppName);

            EnvApplication app = null;
            if (Env != null)
            {
                app = Env.GetApplication(AppName);
            }

            if (app != null)
            {
                URL = app.Url + "";
                mValueCalculated = mValueCalculated.Replace(p, URL);
            }
            else
            {
                // TODO: err                
                mValueCalculated = mValueCalculated.Replace(p, "");
            }
        }
        private void ReplaceEnvDBWithValue(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Input string is null or empty.", null);
                    return;
                }

                // Use regex to extract values
                var envAppMatch = Regex.Match(str, @"\{EnvApp=(.*?)\s");
                var envAppDBMatch = Regex.Match(str, @"EnvAppDB=(.*?)\s");
                var queryMatch = Regex.Match(str, @"Query=(.*?)\}");

                string envApp = envAppMatch.Groups[1].Value;
                string envAppDB = envAppDBMatch.Groups[1].Value;
                string query = queryMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(envApp) || string.IsNullOrWhiteSpace(envAppDB) || string.IsNullOrWhiteSpace(query))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "ReplaceEnvDBWithValue Error: Missing required parameters.", null);
                    return;
                }

                if (!query.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Given query is not a valid SELECT query.", null);
                    return;
                }

                if (Env == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "ReplaceEnvDBWithValue Error: Env object is null.", null);
                    return;
                }

                var envApplication = Env.GetApplication(envApp);
                if (envApplication == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Application '{envApp}' not found.", null);
                    return;
                }

                var db = envApplication.Dbs.FirstOrDefault(d => d.Name == envAppDB) as Database;
                if (db == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Database '{envAppDB}' not found.", null);
                    return;
                }

                if ((db.DBType == eDBTypes.MySQL || db.DBType == eDBTypes.PostgreSQL) && !query.TrimEnd().EndsWith("LIMIT 1", StringComparison.OrdinalIgnoreCase))
                {
                    query = Regex.Replace(query.TrimEnd(), ";?$", " LIMIT 1;");
                }

                db.DatabaseOperations = new DatabaseOperations(db);
                var dbResponse = db.DatabaseOperations.FreeSQL(query, 50);

                if (dbResponse == null || dbResponse.Count < 2)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Invalid or empty DB response.", null);
                    return;
                }

                if (dbResponse[1] is List<List<string>> records && records.Count > 0 && records[0].Count > 0)
                {
                    var value1 = records[0][0];
                    mValueCalculated = mValueCalculated.Replace(str, value1);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "No records found in DB response.", null);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "ReplaceEnvDBWithValue Exception:", ex);
            }
        }      
        public static void GetEnvAppAndParam(string EnvValueExp, ref string AppName, ref string GlobalParamName)
        {
            EnvValueExp = EnvValueExp.Replace("\r\n", "vbCrLf");
            string appStr = " App=";
            string paramStr = " Param=";
            int indxOfApp = EnvValueExp.IndexOf(appStr);
            int indexOfParam = EnvValueExp.IndexOf(paramStr);
            AppName = EnvValueExp[(indxOfApp + appStr.Length)..indexOfParam];
            GlobalParamName = EnvValueExp[(indexOfParam + paramStr.Length)..^1];
        }

        private void ReplaceEnvParamWithValue(string p, string[] a)
        {

            string ParamValue = null;
            string AppName = string.Empty, GlobalParamName = string.Empty;

            GetEnvAppAndParam(p, ref AppName, ref GlobalParamName);


            EnvApplication app = null;
            if (Env != null)
            {
                app = Env.GetApplication(AppName);
            }
            if (app != null)
            {
                VariableBase VB = app.GetVariable(GlobalParamName);
                if (VB != null)
                {
                    if (VB is VariableDynamic variableDynamic)
                    {
                        ParamValue = variableDynamic.ValueExpression + "";

                    }
                    else
                    {
                        ParamValue = VB.Value + "";  // Autohandle in case param is null convert to empty string
                    }

                    if (DecryptFlag && VB is VariablePasswordString)
                    {
                        string strValuetoPass = EncryptionHandler.DecryptwithKey(VB.Value);
                        if (!string.IsNullOrEmpty(strValuetoPass))
                        {
                            mValueCalculated = mValueCalculated.Replace(p, strValuetoPass);
                            mEncryptedValue = VB.Value;
                        }
                        else
                        {
                            mValueCalculated = mValueCalculated.Replace(p, ParamValue);
                        }
                    }
                    else
                    {
                        ValueExpression VE = new ValueExpression(Env, BF, DSList)
                        {
                            Value = ParamValue
                        };
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

                        Regex paramRegEx = new Regex("(.*{.*.}\\\")|([,]+)", RegexOptions.Compiled | RegexOptions.Singleline);

                        //Remove Function name
                        int firstStringPosition = FunName.IndexOf("(") + 1;
                        int secondStringPosition = FunName.IndexOf(")");
                        string allParams = FunName[firstStringPosition..secondStringPosition];

                        List<string> parameters = paramRegEx.Split(allParams.ToString()).ToList<string>();
                        parameters.RemoveAll(y => y.Equals(""));

                        parameters = parameters.Select(z => z.Length > 1 ? z.Trim('\"') : z).ToList<string>();

                        object[] listOfParams = new object[parameters.Count];
                        int index = 0;
                        foreach (var item in parameters)
                        {
                            if (item.Equals(","))
                            {
                                continue;
                            }
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

                Reporter.ToLog(eLogLevel.ERROR, "Replace General Function Error:", ex);
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
                {
                    P1 = P1.Replace("\"", "");
                }
            }

            string P2 = aa.Length <= 4 ? "" : aa[4].Trim();
            if (string.IsNullOrEmpty(P2) == false)
            {
                if (P2.Contains("\""))
                {
                    P2 = P2.Replace("\"", "");
                }
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
                    {
                        foreach (Match m in Ms)
                        {
                            mValueCalculated = mValueCalculated.Replace(p, m.Groups[Convert.ToInt32(FunName.Trim())].Value);
                            break;
                        }
                    }
                    else
                    {
                        mValueCalculated = "";
                    }

                    break;

                case "matchvalue":
                    Regex re = new Regex(Pattern);
                    Match match = re.Match(P1);
                    if (match.Success)
                    {
                        mValueCalculated = mValueCalculated.Replace(p, match.Value);
                    }
                    else
                    {
                        mValueCalculated = mValueCalculated.Replace(p, string.Empty);
                    }

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

            Dictionary<string, string> extraParamDict = GetExtraParam(p);

            string VarValue;

            bool isNameExist = extraParamDict.TryGetValue("Name", out VarName);
            if (!isNameExist)
            {
                VarValue = "ERROR!!! Variable name does not exist, or does not written correctly.";
            }

            VariableBase vb = null;
            if (BF != null)
            {
                vb = BF.GetHierarchyVariableByName(VarName);
            }
            else
            {
                vb = (from v1 in WorkSpace.Instance.Solution.Variables where v1.Name == VarName select v1).FirstOrDefault();
            }

            if (vb != null)
            {
                extraParamDict.Remove("Name");

                if (extraParamDict.Count > 0)
                {
                    VarValue = vb.GetValueWithParam(extraParamDict);
                }
                else
                {
                    if (vb is VariablePasswordString)
                    {
                        string strValuetoPass = String.Empty;
                        if (DecryptFlag)
                        {
                            strValuetoPass = EncryptionHandler.DecryptwithKey(vb.Value);
                            mEncryptedValue = vb.Value;
                        }
                        if (!string.IsNullOrEmpty(strValuetoPass))
                        {
                            VarValue = strValuetoPass;
                        }
                        else
                        {
                            VarValue = vb.Value;
                        }
                    }
                    else if (vb is VariableDynamic variableDynamic)
                    {
                        variableDynamic.Init(Env, BF);
                        VarValue = variableDynamic.Value;
                    }
                    else
                    {
                        VarValue = vb.Value;
                    }
                }

                mValueCalculated = mValueCalculated.Replace(p, VarValue);
            }

            //Use VBS instead of below
            else
            {
                //TODO: throw excpetion, log handler
                VarValue = "ERROR: The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " " + a[1] + " was not found";
                mValueCalculated = VarValue;
            }
        }

        private bool ContainsFormula(string mValueCalculated)
        {
            try
            {
                if (rxGlobalParamPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxEnvParamPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxEnvUrlPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxe.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (VBSRegex.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rNestedfunc.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxDSPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxFDPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxExecutionJsonDataPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (CsExppattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (MockDataExpPattern.IsMatch(mValueCalculated))
                {
                    return true;
                }
                else if (rxEnvDB.IsMatch(mValueCalculated))
                {
                    return true;
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Timeout Exception", ex);
                return false;
            }

            return false;
        }


        /// <summary>
        /// Static function to calculate string Expression like: "{Var Name=v1}"
        /// </summary>
        /// <param name="ProjEnvironment">Env is used for Env Params</param>
        /// <param name="BusinessFlow">Business Flow containing the Variables</param>
        /// <param name="Value">the Expression string</param>
        /// <returns></returns>
        public static string Calculate(ProjEnvironment ProjEnvironment, BusinessFlow BusinessFlow, string Value, ObservableList<DataSourceBase> DSList, bool bUpdate = false, string UpdateValue = "")
        {
            //TODO: this is static func, we can later on do cache and other stuff for performence if needed
            ValueExpression VE = new ValueExpression(ProjEnvironment, BusinessFlow, DSList, bUpdate, UpdateValue)
            {
                Value = Value
            };
            return VE.ValueCalculated;
        }

        /// <summary>
        /// Calculates the actual value from the input string based on its type.
        /// If the input is a value expression, it computes the expression to get the value.
        /// If the input is an encrypted string, it decrypts the string to retrieve the original value.
        /// Returns the input as is if it doesn't match the above conditions.
        /// </summary>
        /// <param name="value">The input string which might be a value expression or an encrypted string.</param>
        /// <returns>The calculated or decrypted value, or the input string if no processing is needed.</returns>
        public static string PasswordCalculation(string value)
        {
            try
            {
                if (IsThisAValueExpression(value))
                {
                    ValueExpression valueExpression = new()
                    {
                        DecryptFlag = true
                    };
                    value = valueExpression.Calculate(value);
                    valueExpression.DecryptFlag = false;
                    return value;
                }
                else if (EncryptionHandler.IsStringEncrypted(value))
                {
                    value = EncryptionHandler.DecryptwithKey(value);
                    return value;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to decrypt the value expression", ex);
            }
            return value;
        }
    }

    public class Mockdata
    {
        public string MockDataDatasets { get; set; }

        public string Locale { get; set; }

        public string Function { get; set; }

        public string MockExpression { get; set; }

        public int DatasetStartIndex { get; set; }

        public int DatasetEndIndex { get; set; }

        public int LocaleStartIndex { get; set; }

        public int LocaleEndIndex { get; set; }

        public int FunctionStartIndex { get; set; }

    }
}
