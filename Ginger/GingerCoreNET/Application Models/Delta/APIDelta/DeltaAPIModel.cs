#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;
using GingerCoreNET.Application_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET.Application_Models
{
    public class DeltaAPIModel
    {
        public object mergerPageObject { get; set; }
        private bool mIsSelected;
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                mIsSelected = value;
            }
        }

        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        private string mDescription;
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                mDescription = value;
            }
        }

        public string ShowMergerLink
        {
            get
            {
                if (string.IsNullOrEmpty(MatchingAPIName))
                {
                    return "False";
                }
                else
                {
                    return "True";
                }
            }
        }

        private string mMatchingAPIName = "";
        public string MatchingAPIName
        {
            get
            {
                return mMatchingAPIName;
            }
            set
            {
                mMatchingAPIName = value;
            }
        }

        private ApplicationAPIModel mlearnedAPIl;
        public ApplicationAPIModel learnedAPI
        {
            get
            {
                return mlearnedAPIl;
            }
            set
            {
                mlearnedAPIl = value;
                mName = value.Name;
                mDescription = value.Description;
            }
        }

        private ApplicationAPIModel mMatchingAPIModel;
        public ApplicationAPIModel matchingAPIModel
        {
            get
            {
                return mMatchingAPIModel;
            }
            set
            {
                mMatchingAPIModel = value;
                if (value != null)
                {
                    string[] fPath = value.ContainingFolder.Split(new string[] { "\\" }, 4, StringSplitOptions.RemoveEmptyEntries);
                    if (fPath != null)
                    {
                        MatchingAPIName = "~\\" + ((fPath.Length > 3) ? fPath.Last() + "\\" : "") + value.Name;
                    }
                }
                else
                {
                    MatchingAPIName = "";
                }
            }
        }

        private ApplicationAPIModel mMergedAPIModel;
        public ApplicationAPIModel MergedAPIModel
        {
            get
            {
                return mMergedAPIModel;
            }
            set
            {
                mMergedAPIModel = value;
            }
        }

        private eHandlingOperations mSelectedOperationEnum;
        public eHandlingOperations SelectedOperationEnum
        {
            get
            {
                return mSelectedOperationEnum;
            }
            set
            {
                mSelectedOperationEnum = value;
            }
        }

        private string mSelectedOperation;
        public string SelectedOperation
        {
            get
            {
                return mSelectedOperation;
            }
            set
            {
                mSelectedOperation = value;
                //OnPropertyChanged(nameof(defaultOperation));
            }
        }

        private ObservableList<string> _OperationsList = new ObservableList<string>();
        public ObservableList<string> OperationsList
        {
            get
            {
                return _OperationsList;
            }
            set
            {
                _OperationsList = value;
            }
        }

        void SetOperationsList()
        {
            OperationsList.Clear();
            foreach (eHandlingOperations enumItem in Enum.GetValues(typeof(eHandlingOperations)))
            {
                string enumDes = GetEnumDescription(enumItem);
                if (comparisonStatus == eComparisonOutput.New || comparisonStatus == eComparisonOutput.Unknown)
                {
                    if (enumItem == eHandlingOperations.MergeChanges || enumItem == eHandlingOperations.ReplaceExisting)
                        continue;

                    if (enumItem == eHandlingOperations.Add)
                    {
                        SelectedOperation = enumDes;
                        SelectedOperationEnum = enumItem;
                    }
                }
                else if (comparisonStatus == eComparisonOutput.Unchanged)
                {
                    if (enumItem == eHandlingOperations.MergeChanges)
                        continue;

                    if (enumItem == eHandlingOperations.DoNotAdd)
                    {
                        SelectedOperation = enumDes;
                        SelectedOperationEnum = enumItem;
                    }
                }
                else
                {
                    if (enumItem == eHandlingOperations.ReplaceExisting)
                    {
                        SelectedOperation = enumDes;
                        SelectedOperationEnum = enumItem;
                    }
                }

                OperationsList.Add(enumDes);
            }
        }

        public enum eHandlingOperations
        {
            [Description("Add New")]
            Add,
            [Description("Do Not Add New")]
            DoNotAdd,
            [Description("Merge Changes")]
            MergeChanges,
            [Description("Replace Existing with New")]
            ReplaceExisting,
        };

        private eComparisonOutput mComparisonOutput = eComparisonOutput.New;
        public eComparisonOutput comparisonStatus
        {
            get
            {
                return mComparisonOutput;
            }
            set
            {
                mComparisonOutput = value;
                SetOperationsList();
            }
        }

        public enum eComparisonOutput
        {
            Modified,
            New,
            Unchanged,
            Unknown
        }

        public Amdocs.Ginger.Common.Enums.eImageType DeltaStatusIcon
        {
            get
            {
                switch(comparisonStatus)
                {
                    case eComparisonOutput.New : return Amdocs.Ginger.Common.Enums.eImageType.Added;
                    case eComparisonOutput.Modified : return Amdocs.Ginger.Common.Enums.eImageType.Changed;
                    case eComparisonOutput.Unchanged : return Amdocs.Ginger.Common.Enums.eImageType.Unchanged;
                    default : return Amdocs.Ginger.Common.Enums.eImageType.Unknown;
                }
            }
        }

        public static string GetEnumDescription(Enum value)
        {
            Type enumType = value.GetType();
            MemberInfo[] enumMember = enumType.GetMember(value.ToString());

            if (enumMember != null && enumMember.Length > 0)
            {
                object[] attrs = enumMember[0].GetCustomAttributes(typeof(DescriptionAttribute)) as object[];
                if (attrs != null && attrs.Length > 0)
                {
                    return (attrs[0] as DescriptionAttribute).Description;
                }
            }

            return value.ToString();
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
            // or return default(T);
        }
    }
}
