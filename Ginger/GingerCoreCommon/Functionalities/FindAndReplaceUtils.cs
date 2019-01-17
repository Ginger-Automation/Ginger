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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.Common.Functionalities
{
    public class FindAndReplaceUtils : INotifyPropertyChanged
    {
        public enum eProcessingState
        {
            Pending = 0,
            Running = 1,
            Stopping = 2
        }

        private eProcessingState mProcessingState;
        public eProcessingState ProcessingState
        {
            get
            {
                return mProcessingState;
            }
            set
            {
                mProcessingState = value;
                OnPropertyChanged(nameof(ProcessingState));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void FindItemsByReflection(RepositoryItemBase OriginItemObject, RepositoryItemBase item, ObservableList<FoundItem> foundItemsList, string textToFind ,SearchConfig searchConfig, RepositoryItemBase parentItemToSave, string itemParent, string foundField)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);

            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name == nameof(ActInputValue.StoreToVariable) || mi.Name == /*nameof(RepositoryItemBase.mBackupDic)*/ "mBackupDic" || mi.Name == nameof(RepositoryItemBase.FileName) || mi.Name == nameof(RepositoryItemBase.Guid) ||
                    mi.Name == nameof(RepositoryItemBase.ObjFolderName) || mi.Name == nameof(RepositoryItemBase.ObjFileExt) || mi.Name == "ScreenShots" ||
                    mi.Name == nameof(RepositoryItemBase.ContainingFolder) || mi.Name == nameof(RepositoryItemBase.ContainingFolderFullPath) || mi.Name == nameof(RepositoryItemBase.Guid) || mi.Name == nameof(RepositoryItemBase.ParentGuid) || mi.Name == "Created" || mi.Name == "Version" || mi.Name == "CreatedBy" || mi.Name == "LastUpdate" || mi.Name == "LastUpdateBy") 
                    continue;


                    //Get the attr value
                    PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                        if (token == null) continue;
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        var token = item.GetType().GetField(mi.Name).GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                        if (token == null) continue;
                    }
                    if (PI != null && (PI.PropertyType == typeof(DateTime) || PI.PropertyType == typeof(Guid)))
                    {
                        continue;
                    }

                    dynamic value = null;
                    try
                    {
                        if (mi.MemberType == MemberTypes.Property)
                            value = PI.GetValue(item);
                        else if (mi.MemberType == MemberTypes.Field)
                            value = item.GetType().GetField(mi.Name).GetValue(item);
                    }
                    catch 
                    {
                        
                        continue;
                    }

                    if (value == null)
                    {
                        continue;
                    }

                    if (value is IObservableList)
                    {


                        string foundListField = string.Empty;

                        if (string.IsNullOrEmpty(foundField))
                            foundListField = mi.Name;
                        else
                            foundListField = foundField + @"\" + mi.Name;

                        int index = 0;
                        foreach (RepositoryItemBase o in value)
                        {
                            index++;
                            string paramNameString = string.Empty;
                            if (o is RepositoryItemBase)
                                paramNameString = ((RepositoryItemBase)o).ItemName;
                            else
                                paramNameString = o.ToString();

                            string foundListFieldValue = string.Format(@"{0}[{1}]\{2}", foundListField, index, paramNameString);

                            FindItemsByReflection(OriginItemObject, o, foundItemsList, textToFind, searchConfig, parentItemToSave, itemParent, foundListFieldValue);
                        }
                    }
                    else if (value is RepositoryItemBase)//!RegularTypeList.Contains(value.GetType().Name) && value.GetType().BaseType.Name != nameof(Enum) && value.GetType().Name != "Bitmap" && value.GetType().Name != nameof(Guid) && value.GetType().Name !=  nameof(RepositoryItemKey))
                    {

                        //TODO taking care of List which is not iobservableList

                        if (string.IsNullOrEmpty(foundField))
                            foundField = @"\" + mi.Name;
                        else
                            foundField = foundField + @"\" + mi.Name;
                        FindItemsByReflection(OriginItemObject, value, foundItemsList, textToFind, searchConfig, parentItemToSave, itemParent, foundField);
                    }
                    else
                    {
                        if (value != null)
                        {
                            try
                            {
                                string stringValue = value.ToString();
                                string matchedStringValue = string.Empty;
                                if (searchConfig.MatchCase == false)
                                {
                                    matchedStringValue = stringValue.ToUpper();
                                    textToFind = textToFind.ToUpper();
                                }
                                else
                                    matchedStringValue = stringValue;

                                if ((searchConfig.MatchAllWord == true && matchedStringValue == textToFind) || (searchConfig.MatchAllWord == false && matchedStringValue.Contains(textToFind))/* || new Regex(textToFind).Match(stringValue).Success == true*/) //Comment out until fixing Regex search
                                {
                                    string finalFoundFieldPath = string.Empty;
                                    if (string.IsNullOrEmpty(foundField))
                                        finalFoundFieldPath = mi.Name;
                                    else
                                        finalFoundFieldPath = foundField + @"\" + mi.Name;

                                    FoundItem foundItem = foundItemsList.Where(x => x.FieldName == mi.Name && x.FieldValue == stringValue && x.ItemObject == item).FirstOrDefault();
                                    if (foundItem == null)
                                    {
                                        List<string> OptionalValuseToReplaceList = new List<string>();
                                        if (PI.PropertyType.BaseType == typeof(Enum))
                                        {
                                            Array enumValues = Enum.GetValues(PI.PropertyType);
                                            for (int i = 0; i < enumValues.Length; i++)
                                            {
                                                object enumValue = enumValues.GetValue(i);
                                                OptionalValuseToReplaceList.Add(enumValue.ToString());
                                            }
                                        }
                                        else if (PI.PropertyType == typeof(bool))
                                        {
                                            OptionalValuseToReplaceList.Add("True");
                                            OptionalValuseToReplaceList.Add("False");
                                        }

                                        foundItemsList.Add(new FoundItem() { OriginObject = OriginItemObject, ItemObject = item, ParentItemToSave = parentItemToSave, FieldName = mi.Name, FieldValue = stringValue, ItemParent = itemParent, FoundField = finalFoundFieldPath, OptionalValuesToRepalce = OptionalValuseToReplaceList });
                                    }
                                        
                                    else
                                    {
                                    }
                                }

                            }
                            catch 
                            {

                            }
                        }
                    }
                }
                catch 
                {

                }
                
            }
        }

        public bool ReplaceItem(SearchConfig searchConfig ,string findWhat, FoundItem FI,string newValue)
        {
            try
            {
                bool result = false;

                PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);

                object ValueToReplace = string.Empty;


                if (PI.PropertyType == typeof(int))
                {
                    int i = 0;
                    if (!searchConfig.MatchAllWord)
                    {
                        newValue = FI.FieldValue.Replace(findWhat, newValue);
                    }
                    result = Int32.TryParse(newValue, out i);
                    ValueToReplace = i;
                }
                else if (PI.PropertyType == typeof(bool))
                {
                    bool boolValue = false;
                    result = Boolean.TryParse(newValue, out boolValue);
                    ValueToReplace = boolValue;
                }
                else if (PI.PropertyType.BaseType == typeof(Enum))
                {
                    Array enumValues = Enum.GetValues(PI.PropertyType);
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        object enumValue = enumValues.GetValue(i);
                        if (enumValue.ToString() == newValue)
                        {
                            ValueToReplace = enumValue;
                            result = true;
                            break;
                        }
                    }
                }
                else if ((PI.PropertyType == typeof(string)))
                {
                    string stringValue = FI.FieldValue;
                    if (!searchConfig.MatchCase && !searchConfig.MatchAllWord)
                    {
                        RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;
                        foreach (Match match in Regex.Matches(stringValue, findWhat, options))
                        {
                            stringValue = stringValue.Replace(match.Value, newValue);
                        }
                    }
                    else if (searchConfig.MatchCase && !searchConfig.MatchAllWord)
                    {
                        stringValue = stringValue.Replace(findWhat, newValue);
                    }
                    else
                    {
                        stringValue = newValue;
                    }
                    ValueToReplace = stringValue;
                    result = true;
                }


                if (result)
                {
                    PI.SetValue(FI.ItemObject, ValueToReplace);
                    FI.FieldValue = ValueToReplace.ToString();
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch 
            {
                return false;
            }

        }

    }
}
