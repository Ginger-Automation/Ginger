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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Amdocs.Ginger.Repository;

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
        public void FindTopLevelAttributeNames(RepositoryItemBase item, ObservableList<FoundItem> foundItemsList, string textToFind, SearchConfig searchConfig, RepositoryItemBase parentItemToSave, string itemParent = "")
        {
            var members = item.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(m => m.MemberType is MemberTypes.Property or MemberTypes.Field);

            foreach (var member in members)
            {
                try
                {
                    if (member.Name is
                        nameof(ActInputValue.ValueForDriver) or "mBackupDic" or nameof(RepositoryItemBase.FileName) or
                        nameof(RepositoryItemBase.Guid) or nameof(RepositoryItemBase.ObjFolderName) or
                        nameof(RepositoryItemBase.ObjFileExt) or "ScreenShots" or
                        nameof(RepositoryItemBase.ContainingFolder) or nameof(RepositoryItemBase.ContainingFolderFullPath) or
                        nameof(RepositoryItemBase.ParentGuid) or "Created" or "Version" or
                        "CreatedBy" or "LastUpdate" or "LastUpdateBy")
                    {
                        continue;
                    }

                    object value = null;
                    Type memberType = null;
                    bool isSerializable = false;
                    bool isAllowedToEdit = false;



                    if (member is PropertyInfo prop)
                    {
                        isSerializable = prop.GetCustomAttribute<IsSerializedForLocalRepositoryAttribute>() != null;
                        if (!isSerializable)
                        {
                            continue;
                        }
                        isAllowedToEdit = prop.GetCustomAttribute<AllowUserToEdit>() != null;
                        if (!isAllowedToEdit)
                        {
                            continue;
                        }
                        value = prop.GetValue(item);
                        memberType = prop.PropertyType;
                    }
                    else if (member is FieldInfo field)
                    {
                        isSerializable = field.GetCustomAttribute<IsSerializedForLocalRepositoryAttribute>() != null;
                        if (!isSerializable)
                        {
                            continue;
                        }
                        isAllowedToEdit = field.GetCustomAttribute<AllowUserToEdit>() != null;
                        if (!isAllowedToEdit)
                        {
                            continue;
                        }
                        value = field.GetValue(item);
                        memberType = field.FieldType;
                    }

                    string attributeName = (string)member.GetCustomAttribute<AllowUserToEdit>().GetDefualtValue();

                    string matchAgainst = searchConfig.MatchCase ? attributeName : attributeName.ToUpper();
                    string search = searchConfig.MatchCase ? textToFind : textToFind.ToUpper();

                    if (matchAgainst == search)
                    {
                        foundItemsList.Add(new FoundItem
                        {
                            OriginObject = item,
                            ItemObject = item,
                            ParentItemToSave = parentItemToSave,
                            FieldName=member.Name,
                            FieldValue = value?.ToString() ?? "",
                            ItemParent = itemParent,
                            FoundField = attributeName,
                            FieldType = memberType,
                            OptionalValuesToRepalce = [],
                            Status = FoundItem.eStatus.Pending,
                        });
                    }

                }
                catch
                {
                    Reporter.ToLog(eLogLevel.WARN, $"Failed to process member {member.Name} in {item.GetType().Name}");
                    continue;
                }
            }
        }

        public List<string> GetSerializableEditableMemberNames(RepositoryItemBase item)
        {
            var memberNames = new List<string>();

            var members = item.GetType()
                              .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                try
                {
                    bool isSerializable = false;
                    bool isEditable = false;

                    if (member is PropertyInfo prop)
                    {
                        isSerializable = prop.GetCustomAttribute<IsSerializedForLocalRepositoryAttribute>() != null;
                        isEditable = prop.GetCustomAttribute<AllowUserToEdit>()!= null;
                        
                    }
                    else if (member is FieldInfo field)
                    {
                        isSerializable = field.GetCustomAttribute<IsSerializedForLocalRepositoryAttribute>() != null;
                        isEditable = field.GetCustomAttribute<AllowUserToEdit>() != null;
                    }

                    if (isSerializable && isEditable)
                    {
                        memberNames.Add((string)member.GetCustomAttribute<AllowUserToEdit>().GetDefualtValue());
                    }
                }
                catch
                {
                    continue;
                }
            }

            return memberNames;
        }



        public void FindItemsByReflection(RepositoryItemBase OriginItemObject, RepositoryItemBase item, ObservableList<FoundItem> foundItemsList, string textToFind, SearchConfig searchConfig, RepositoryItemBase parentItemToSave, string itemParent, string foundField)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);

            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name is (nameof(ActInputValue.ValueForDriver)) or "mBackupDic" or (nameof(RepositoryItemBase.FileName)) or (nameof(RepositoryItemBase.Guid)) or
                    (nameof(RepositoryItemBase.ObjFolderName)) or (nameof(RepositoryItemBase.ObjFileExt)) or "ScreenShots" or
                    (nameof(RepositoryItemBase.ContainingFolder)) or (nameof(RepositoryItemBase.ContainingFolderFullPath)) or (nameof(RepositoryItemBase.Guid)) or (nameof(RepositoryItemBase.ParentGuid)) or "Created" or "Version" or "CreatedBy" or "LastUpdate" or "LastUpdateBy")
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
                            if (o is not null)
                                paramNameString = o.ItemName;
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

                                    FoundItem foundItem = foundItemsList.FirstOrDefault(x => x.FieldName == mi.Name && x.FieldValue == stringValue && x.ItemObject == item);
                                    if (foundItem == null)
                                    {
                                        List<string> OptionalValuseToReplaceList = [];
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
                                        var item1 = new FoundItem() { OriginObject = OriginItemObject, ItemObject = item, ParentItemToSave = parentItemToSave, FieldName = mi.Name, FieldValue = stringValue, ItemParent = itemParent, FoundField = finalFoundFieldPath, OptionalValuesToRepalce = OptionalValuseToReplaceList };

                                        item1.Status=FoundItem.eStatus.Pending;
                                        foundItemsList.Add(item1);
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

        public bool ReplaceItem(SearchConfig searchConfig, string findWhat, FoundItem FI, string newValue)
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
                    // change the dirty status of the the modified object to populate the object into the modified solution files.
                    if (FI.ItemObject is not null)
                    {
                        FI.ItemObject.DirtyStatus = Enums.eDirtyStatus.Modified;
                    }
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

/*
        public bool ReplaceItemEnhanced( FoundItem FI )
        {
            try
            {
                string newValue = FI.FieldValue;
                if (FI?.ItemObject == null || string.IsNullOrEmpty(FI.FieldName))
                {
                    return false;
                }

                PropertyInfo property = FI.ItemObject.GetType().GetProperty(FI.FieldName);
                if (property == null || !property.CanWrite)
                {
                    return false;
                }
                Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object valueToSet = null;
                bool result = false;

                switch (Type.GetTypeCode(propertyType))
                {
                    case TypeCode.Boolean:
                        result = bool.TryParse(newValue, out bool boolValue);
                        valueToSet = boolValue;
                        break;

                    case TypeCode.String:
                        valueToSet = newValue;
                        result = true;
                        break;

                    default:
                        if (propertyType.IsEnum)
                        {
                            result = Enum.TryParse(propertyType, newValue, out object enumValue);
                            valueToSet = enumValue;
                        }
                        break;
                }

                if (result)
                {
                    property.SetValue(FI.ItemObject, valueToSet);
                    FI.FieldValue = valueToSet?.ToString();

                    if (FI.ItemObject is not null)
                    {
                        FI.ItemObject.DirtyStatus = Enums.eDirtyStatus.Modified;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eLogLevel.ERROR, $"Failed to replace value for property {FI.FieldName}", ex);
                return false;
            }
        }*/

        public bool ReplaceItemEnhanced(FoundItem foundItem)
        {
            if (foundItem?.ItemObject == null || string.IsNullOrWhiteSpace(foundItem.FieldName))
                return false;

            try
            {
                var property = foundItem.ItemObject.GetType().GetProperty(foundItem.FieldName);
                if (property == null || !property.CanWrite)
                    return false;

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object valueToSet = null;
                bool isValid = false;

                string input = foundItem.FieldValue;

                if (targetType.IsEnum)
                {
                    isValid = Enum.TryParse(targetType, input, out valueToSet);
                }
                else
                {
                    switch (Type.GetTypeCode(targetType))
                    {
                        case TypeCode.Boolean:
                            isValid = bool.TryParse(input, out var boolVal);
                            valueToSet = boolVal;
                            break;

                        case TypeCode.String:
                            valueToSet = input;
                            isValid = true;
                            break;

                    }
                }

                if (!isValid) 
                {
                    return false; 
                }

                property.SetValue(foundItem.ItemObject, valueToSet);
                foundItem.FieldValue = valueToSet?.ToString();

                if (foundItem.ItemObject is not null)
                {
                    foundItem.ItemObject.DirtyStatus = Enums.eDirtyStatus.Modified;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to replace value for property {foundItem.FieldName}", ex);
                return false;
            }
        }




    }
}
