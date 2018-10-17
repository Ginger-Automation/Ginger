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
using System;
using System.Reflection;
using System.Windows;

namespace GingerCore
{    
    public enum eTermResKey
    {
        BusinessFlow, BusinessFlows,
        ActivitiesGroup, ActivitiesGroups,
        Activity, Activities,
        ConversionMechanism,
        Variable, Variables,
        RunSet, RunSets
    }

    public class GingerDicser
    {
        public static string GetTermResValue(eTermResKey termResourceKey, string prefixString = "", string suffixString = "", bool setToUpperCase = false)
        {
            object termResValue = null;
            try
            {
                //termResValue = (new FrameworkElement()).TryFindResource(termResourceKey.ToString());
                termResValue = Application.Current.Resources[termResourceKey.ToString()];
            }
            catch (Exception ex)
            {
                termResValue = null;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
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

        public static void LinkToTermResource(FrameworkElement obj, DependencyProperty objPropertyToLink, eTermResKey termResourceKey)
        {
            if (obj != null && objPropertyToLink != null) //&& termResourceKey != null ... termResourceKey is never null
                obj.SetResourceReference(objPropertyToLink, termResourceKey.ToString());
        }

        public static void LinkToResource(FrameworkElement obj, DependencyProperty objPropertyToLink, string resourceKey)
        {
            if (obj != null && objPropertyToLink != null && string.IsNullOrEmpty(resourceKey) == false)
                obj.SetResourceReference(objPropertyToLink, resourceKey);
        }        
    }
}
