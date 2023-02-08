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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore;
using GingerCore.Actions;


namespace Ginger.Imports.UFT
{
    public class CommonFunctionConvertor : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string Notes { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<CommonFunctionMapping> CommonFunctionMappingList = new ObservableList<CommonFunctionMapping>();

        public string sParamLocateBy;
        public string sParamLocateValue;
        public string ActionType;
        public string getParametersListFromLine(string line)
        {
            string str = "";
            int startIndex = line.IndexOf("(");
            int lastIndex = line.LastIndexOf(")");
            int paramsStrLength = lastIndex - startIndex - 2;

            if (paramsStrLength != 0)
            {
                str = line.Substring(startIndex + 1, paramsStrLength);// lastIndex - startIndex);
                str = str.Replace("\"", "");
            }
            return str.Trim();
        }
        public Dictionary<string, string> ActionFromCommonFunction = new Dictionary<string, string>();
        public int ParamDiff;
        public int CollateTill;
        public string sLocateDesc = "";

        /// <summary>
        /// Convert one line of code
        /// Will return null if no mapping found
        /// </summary>
        /// <param name="CodeLine"></param>
        /// <returns></returns>
        public Dictionary<string, string> ConvertCodeLine(string CodeLine)
        {
            //Clear the Dictionary object
            ActionFromCommonFunction.Clear();

            foreach(CommonFunctionMapping CFP in CommonFunctionMappingList)
            {
                if (CodeLine.Contains(CFP.Function_Name + "(" )) 
                {
                    //Identify the Type of Action
                    if (CFP.Function_Name.Contains("Click")) { ActionType = "Click"; }
                    else if (CFP.Function_Name.Contains("Enter")) { ActionType = "Enter"; }
                    else if (CFP.Function_Name.Contains("Select")) { ActionType = "Select"; }

                    //Create a lst of paramns from function Def
                    char[] delimiterChars = { ',' };
                    string paramsStr = "";
                    List<string> ParamsLst = new List<string>();

                    if (CodeLine.Length != 0)
                    {
                        paramsStr = getParametersListFromLine(CodeLine);
                        string[] paramArr = paramsStr.Split(delimiterChars);
                        foreach (string param in paramArr)
                        { ParamsLst.Add(param.Trim()); }
                    }

                    //for Descriptive
                    if (ParamsLst.Count() > Int32.Parse(CFP.NoOfParameters) )
                    {
                        ParamDiff = ParamsLst.Count() - Int32.Parse(CFP.NoOfParameters);
                        sParamLocateBy = CFP.LocateBy.Replace("{", "").Replace("}", "").Replace("Param", "").Trim();
                        CollateTill = Int32.Parse(sParamLocateBy) + ParamDiff;

                        for (int i = Int32.Parse(sParamLocateBy); i<= CollateTill; i++ )
                        {
                            sLocateDesc = sLocateDesc + ParamsLst[i] + ","; 
                        }

                        if (sLocateDesc.ToUpper().Contains("WEBELEMENT(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebElement(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBEDIT(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebEdit(", "),");
                        else if (sLocateDesc.ToUpper().Contains("LINK(")) sLocateDesc = GetStringBetween(sLocateDesc, "Link(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBBUTTON(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebButton(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBLIST(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebList(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBRADIOGROUP(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebRadioGroup(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBTABLE(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebTable(", "),");
                        else if (sLocateDesc.ToUpper().Contains("WEBIMAGE(")) sLocateDesc = GetStringBetween(sLocateDesc, "WebImage(", "),");

                        if (sLocateDesc.Contains("(")) sLocateDesc = sLocateDesc.Replace("(", "");
                        if (sLocateDesc.Contains(")")) sLocateDesc = sLocateDesc.Replace(")", "");
                        if (sLocateDesc.EndsWith(",")) { sLocateDesc = sLocateDesc.Remove(sLocateDesc.Length - 1, 1); }

                        ActionFromCommonFunction.Add("LocateBy", sLocateDesc);
                        ActionFromCommonFunction.Add("LocateValue", sLocateDesc);
                    }

                    // if there is no descriptive
                    else if (ParamsLst.Count() == Int32.Parse(CFP.NoOfParameters))
                    {
                        //Fetch the Parameter to Locate By
                        if (CFP.LocateBy != null)
                        {
                            sParamLocateBy = CFP.LocateBy.Replace("{", "").Replace("}", "").Replace("Param", "").Trim();
                            sParamLocateBy = ParamsLst[Int32.Parse(sParamLocateBy)];
                        }

                        //Ftech Value to Set
                        if (CFP.Value != null)
                        {
                            sParamLocateValue = CFP.Value.Replace("{", "").Replace("}", "").Replace("Param", "").Trim();
                            sParamLocateValue = ParamsLst[Int32.Parse(sParamLocateValue)];
                        }

                        ActionFromCommonFunction.Add("LocateBy", sParamLocateBy);
                        ActionFromCommonFunction.Add("LocateValue", sParamLocateValue);
                    }

                    ActionFromCommonFunction.Add("ActionType", ActionType);
                    return ActionFromCommonFunction;
                }
            }

            return null;
        }

        //Fetches string between First and Last strings passed as parameter (Helper Function)
        public string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            else
            {
                return "";
            }
        }
        
        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
