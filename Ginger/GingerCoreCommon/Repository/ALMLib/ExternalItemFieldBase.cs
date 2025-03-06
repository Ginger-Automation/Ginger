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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Microsoft.CodeAnalysis;

namespace Amdocs.Ginger.Repository
{
    public class ExternalItemFieldBase : RepositoryItemBase
    {
        public static class Fields
        {
            public static string ToUpdate = "ToUpdate";
            public static string ID = "ID";
            public static string Name = "Name";
            public static string ItemType = "ItemType";
            public static string Type = "Type";
            public static string Mandatory = "Mandatory";
            public static string PossibleValues = "PossibleValues";
            public static string PossibleValueKeys = "PossibleValueKeys";
            public static string SelectedValue = "SelectedValue";
            public static string SelectedValueKey = "SelectedValueKey";
        }

        [IsSerializedForLocalRepository]
        public bool ToUpdate { get; set; }

        [IsSerializedForLocalRepository]
        public string ID { get; set; }

        public string TypeIdentifier { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string ItemType { get; set; }

        [IsSerializedForLocalRepository]
        public string Type { get; set; }

        [IsSerializedForLocalRepository]
        public bool Mandatory { get; set; }

        [IsSerializedForLocalRepository]
        public bool SystemFieled { get; set; }
        public bool IsMultiple { get; set; } = false;

        [IsSerializedForLocalRepository]
        public bool IsCustomField { get; set; } = false;

        [IsSerializedForLocalRepository]
        public string ProjectGuid { get; set; }

        ObservableList<string> mPossibleValues = [];
        public ObservableList<string> PossibleValues
        {
            get
            {
                return mPossibleValues;
            }
            set
            {
                mPossibleValues = value;
                OnPropertyChanged(Fields.PossibleValues);
            }
        }

        ObservableList<string> mPossibleValueKeys = [];
        public ObservableList<string> PossibleValueKeys
        {
            get
            {
                return mPossibleValueKeys;
            }
            set
            {
                mPossibleValueKeys = value;
                OnPropertyChanged(Fields.PossibleValueKeys);
            }
        }

        string mSelectedValue;
        [IsSerializedForLocalRepository]
        public string SelectedValue
        {
            get
            {
                return mSelectedValue;
            }
            set
            {
                if (mSelectedValue != value)
                {
                    mSelectedValue = value;
                    SelectedValueKey = UpdateSelectedValueKey(mSelectedValue);
                    OnPropertyChanged(Fields.SelectedValue);
                }
            }
        }

        string mSelectedValueKey;
        [IsSerializedForLocalRepository]
        public string SelectedValueKey
        {
            get
            {
                return mSelectedValueKey;
            }
            set
            {
                if (mSelectedValueKey != value)
                {
                    mSelectedValueKey = value;
                    OnPropertyChanged(Fields.SelectedValueKey);
                }
            }
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

        public string UpdateSelectedValueKey(string SelectedValue)
        {
            string ValueKey = string.Empty;
            if (!string.IsNullOrEmpty(SelectedValue))
            {
                if (mPossibleValues.Count != mPossibleValueKeys.Count)
                {
                    return mSelectedValueKey ?? string.Empty;
                }

                int indexofValue = mPossibleValues.IndexOf(SelectedValue);

                if(indexofValue != -1)
                {
                    ValueKey = mPossibleValueKeys[indexofValue];
                }
                else
                {
                    ValueKey = mSelectedValueKey;
                }
            }
            return ValueKey;
        }

    }
}
