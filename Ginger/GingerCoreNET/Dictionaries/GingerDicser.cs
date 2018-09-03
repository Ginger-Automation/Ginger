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

using GingerCoreNET.ReporterLib;
using System;
using System.Reflection;

namespace GingerCoreNET.Dictionaries
{
    public enum eSkinDicsType
    {
        Default
    }

    public enum eTerminologyDicsType
    {
        Default,
        Testing,
        Gherkin
    }

    public enum eUserType
    {
        Regular,
        Business
    }

    public enum eTermResKey
    {
        BusinessFlow, BusinessFlows,
        ActivitiesGroup, ActivitiesGroups,
        Activity, Activities,
        ConversionMechanism,
        Variable, Variables,
        RunSet, RunSets, POM
    }

    public class GingerDicser
    {
        public static string GetTermResValue(eTermResKey termResourceKey, string prefixString = "", string suffixString = "", bool setToUpperCase = false)
        {
            object termResValue = null;
            try
            {
                // temp ugly code since we cannot load ResourceDic in GingerCoreNET - need another solution
                if (termResourceKey == eTermResKey.Activity) termResValue = "Activity";
                if (termResourceKey == eTermResKey.BusinessFlows) termResValue = "Business Flows";

                if (termResValue == null)
                {
                    termResValue = "!!!termResValue!!!";  //FIXME
                }
            }
            catch (Exception ex)
            {
                termResValue = null;
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }

            if (termResValue != null)
            {
                string strValue = prefixString + termResValue.ToString() + suffixString;
                if (setToUpperCase) strValue = strValue.ToUpper();
                return strValue;
            }
            else
                //key not found
                return string.Empty;
        } 
    }
}
