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
        //private bool mNewSelected = false;
        //public bool NewSelected
        //{
        //    get
        //    {
        //        return mNewSelected;
        //    }
        //    set
        //    {
        //        mNewSelected = value;
        //        mExistingSelected = !value;
        //    }
        //}

        //private bool mExistingSelected = false;
        //public bool ExistingSelected
        //{
        //    get
        //    {
        //        return mExistingSelected;
        //    }
        //    set
        //    {
        //        mExistingSelected = value;
        //        mNewSelected = !value;
        //    }
        //}

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

        private string mMatchingAPIName;
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
                string[] relPathArr = value.ContainingFolderFullPath.Split(new string[] { "API Models" }, StringSplitOptions.RemoveEmptyEntries);
                string relPath = (relPathArr != null && relPathArr.Length > 1) ? relPathArr[1] : string.Empty;
                MatchingAPIName = "~" + relPath + "\\" + value.Name;
            }
        }

        private eHandlingOperations mDefaultOperationEnum;
        public eHandlingOperations DefaultOperationEnum
        {
            get
            {
                return mDefaultOperationEnum;
            }
            set
            {
                mDefaultOperationEnum = value;
            }
        }

        private string mDefaultOperation;
        public string defaultOperation
        {
            get
            {
                return mDefaultOperation;
            }
            set
            {
                mDefaultOperation = value;
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
                if (comparisonStatus == eComparisonOutput.New)
                {
                    if (enumItem == eHandlingOperations.MergeChanges || enumItem == eHandlingOperations.ReplaceExisting)
                        continue;

                    if (enumItem == eHandlingOperations.Add)
                    {
                        defaultOperation = enumDes;
                        DefaultOperationEnum = enumItem;
                    }
                }
                else if (comparisonStatus == eComparisonOutput.Unchanged)
                {
                    if (enumItem == eHandlingOperations.MergeChanges)
                        continue;

                    if (enumItem == eHandlingOperations.DoNotAdd)
                    {
                        defaultOperation = enumDes;
                        DefaultOperationEnum = enumItem;
                    }
                }
                else
                {
                    if (enumItem == eHandlingOperations.ReplaceExisting)
                    {
                        defaultOperation = enumDes;
                        DefaultOperationEnum = enumItem;
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
            New,
            Modified,
            Unchanged
        }

        public Amdocs.Ginger.Common.Enums.eImageType DeltaStatusIcon
        {
            get
            {
                switch(comparisonStatus)
                {
                    case eComparisonOutput.New : return Amdocs.Ginger.Common.Enums.eImageType.Add;
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

            //return 
            //FieldInfo fI = value.GetType().GetField(value.ToString());
            ////EnumValueDescriptionAttribute.GetCustomAttribute;
            //DescriptionAttribute[] desAttr = fI.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            //if(desAttr != null && desAttr.Any<DescriptionAttribute>())
            //{
            //    return desAttr.First().Description;
            //}

            return value.ToString();
        }
    }
}
