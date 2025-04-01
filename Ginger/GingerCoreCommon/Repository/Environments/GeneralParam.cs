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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public class GeneralParam : RepositoryItemBase
    {
        public static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string Value = "Value";
            public static string Encrypt = "Encrypt";
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(Fields.Description);
                }
            }
        }

        private string mValue;
        [IsSerializedForLocalRepository]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(Fields.Value);
                }
            }
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(Fields.Name);
                }
            }
        }
        public string NameBeforeEdit { get; set; }
        private bool mEncryptValue;
        [IsSerializedForLocalRepository]
        public bool Encrypt
        {
            get
            {
                return mEncryptValue;
            }
            set
            {
                if (mEncryptValue != value)
                {
                    mEncryptValue = value;
                    OnPropertyChanged(Fields.Encrypt);
                }
            }
        }

        public static void UpdateNameChangeInItem(object item, string app, string prevVarName, string newVarName, ref bool ItemWasChanged)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                //Get the attr value
                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;
                if (mi.MemberType == MemberTypes.Property && PI != null)
                {
                    try
                    {
                        value = PI.GetValue(item);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                    }
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    value = item.GetType().GetField(mi.Name).GetValue(item);
                }

                if (value is IObservableList)
                {
                    List<dynamic> list = [];
                    foreach (object o in value)
                    {
                        UpdateNameChangeInItem(o, app, prevVarName, newVarName, ref ItemWasChanged);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        {
                            try
                            {
                                if (PI != null && PI.CanWrite)
                                {
                                    string stringValue = value.ToString();
                                    string placeHolder = "{EnvParam App=YYY Param=XXX}";
                                    placeHolder = placeHolder.Replace("YYY", app);
                                    string oldParamPlaceHolder = placeHolder.Replace("XXX", prevVarName);
                                    string newParamPlaceHolder = placeHolder.Replace("XXX", newVarName);

                                    if (stringValue.Contains(oldParamPlaceHolder))
                                    {
                                        PI.SetValue(item, stringValue.Replace(oldParamPlaceHolder, newParamPlaceHolder));
                                        ItemWasChanged = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsParamBeingUsedInBFs(object action, string appName, string paramOldName)
        {
            var actionMembers = action.GetType().GetMembers().Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);
            foreach (MemberInfo mi in actionMembers)
            {
                if (Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                dynamic value = GetActionMemberValue(action, mi);
                if (value == null)
                {
                    continue;
                }

                if (value is IObservableList)
                {
                    foreach (object o in value)
                    {
                        if (IsParamBeingUsedInBFs(o, appName, paramOldName))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    try
                    {
                        string stringValue = value.ToString();
                        string oldParamPlaceHolder = $"{'{'}EnvParam App={appName} Param={paramOldName}{'}'}";

                        if (stringValue != null && stringValue.Contains(oldParamPlaceHolder))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                    }
                }
            }
            return false;
        }

        private static dynamic GetActionMemberValue(object action, MemberInfo mi)
        {
            PropertyInfo PI = action.GetType().GetProperty(mi.Name);
            dynamic value = null;

            try
            {
                if (mi.MemberType == MemberTypes.Property && PI != null)
                {
                    value = PI.GetValue(action);
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    value = action.GetType().GetField(mi.Name).GetValue(action);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }

            return value;
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
