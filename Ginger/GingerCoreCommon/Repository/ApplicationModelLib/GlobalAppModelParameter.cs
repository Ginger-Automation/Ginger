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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.Repository
{

    public class GlobalAppModelParameter : AppModelParameter
    {

        public static string CURRENT_VALUE = "{Current Value}";

        string mCurrentValue = string.Empty;
       [IsSerializedForLocalRepository]
        public string CurrentValue//Used for Model Global Variables
        {
            get
            {
                return mCurrentValue;
            }
            set
            {
                mCurrentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
            } 
        }

        public override string ParamLevel { get { return "Global"; } set { } }


        public static GlobalAppModelParameter DuplicateAppModelParamAsGlobal(AppModelParameter appModelParameter)
        {
            GlobalAppModelParameter globalParam = new GlobalAppModelParameter();
            globalParam.Guid = appModelParameter.Guid;
            globalParam.PlaceHolder = appModelParameter.PlaceHolder;
            globalParam.Description = appModelParameter.Description;
            globalParam.Path = appModelParameter.Path;

            globalParam.ExecutionValue = appModelParameter.ExecutionValue;

            foreach (OptionalValue ov in appModelParameter.OptionalValuesList)
            {
                OptionalValue newOV = new OptionalValue();
                newOV.Guid = ov.Guid;
                newOV.Value = ov.Value;
                newOV.IsDefault = ov.IsDefault;
                globalParam.OptionalValuesList.Add(newOV);
            }

            bool isOptionalValuesEmpty = globalParam.OptionalValuesList.Count == 0;

            globalParam.OptionalValuesList.Add(new OptionalValue() { Value = GlobalAppModelParameter.CURRENT_VALUE, IsDefault = isOptionalValuesEmpty });

            return globalParam;
        }

        public static void GetListOfUsedGlobalParameters(object item, ref List<string> usedGlobalParam)
        {
            //FIXME !!! use nameof and more - do not use refelction

            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);

            Regex rxGlobalParamPattern = new Regex(@"({GlobalAppsModelsParam Name=(\D*\d*\s*)}})|({GlobalAppsModelsParam Name=(\D*\d*\s*)})", RegexOptions.Compiled);

            foreach (MemberInfo mi in properties)
            {
                if (Amdocs.Ginger.Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                //Get the attr value
                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;   // dynamic is bad!!!
                try
                {
                    if (mi.MemberType == MemberTypes.Property)
                        value = PI.GetValue(item);
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        value = item.GetType().GetField(mi.Name).GetValue(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    value = null;
                }


                if (value is IObservableList)
                {
                    List<dynamic> list = new List<dynamic>();
                    foreach (object o in value)
                        GetListOfUsedGlobalParameters(o, ref usedGlobalParam);
                }
                else
                {
                    if (value != null)
                    {
                        try
                        {
                            if (PI.CanWrite)
                            {
                                string stringValue = value.ToString();
                                MatchCollection matches = rxGlobalParamPattern.Matches(stringValue);
                                foreach (Match match in matches)
                                {
                                    string val = match.Value.Substring(28);
                                    val = val.Replace("}}", "}");
                                    if (!usedGlobalParam.Contains(val))
                                        usedGlobalParam.Add(val);
                                }

                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
                    }
                }
            }
        }

    }
}
