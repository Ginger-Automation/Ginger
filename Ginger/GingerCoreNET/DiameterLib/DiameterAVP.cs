#region License
/*
Copyright © 2014-2024 European Support Limited

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
using System;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterAVP : ActInputValue
    {
        public int Length { get; set; }
        private int mCode;
        [IsSerializedForLocalRepository]
        public int Code
        {
            get
            {
                return mCode;
            }
            set
            {
                if (mCode != value)
                {
                    mCode = value;
                    OnPropertyChanged(nameof(Code));
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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        /// <summary>
        /// Enable getting the condition as VE - used in Grid cell for example
        /// </summary>
        public IValueExpression ValueVE
        {
            get
            {
                IValueExpression ve = TargetFrameworkHelper.Helper.CreateValueExpression(this, nameof(Value));
                return ve;
            }
        }
        private int mVendorId;
        [IsSerializedForLocalRepository]
        public int VendorId
        {
            get
            {
                return mVendorId;
            }
            set
            {
                if (mVendorId != value)
                {
                    mVendorId = value;
                    OnPropertyChanged(nameof(VendorId));
                }
            }
        }
        private bool mIsMandatory;
        [IsSerializedForLocalRepository]
        public bool IsMandatory
        {
            get
            {
                return mIsMandatory;
            }
            set
            {
                if (mIsMandatory != value)
                {
                    mIsMandatory = value;
                    OnPropertyChanged(nameof(IsMandatory));
                }
            }
        }
        private bool mIsVendorSpecific;
        [IsSerializedForLocalRepository]
        public bool IsVendorSpecific
        {
            get
            {
                return mIsVendorSpecific;
            }
            set
            {
                if (mIsVendorSpecific != value)
                {
                    mIsVendorSpecific = value;
                    OnPropertyChanged(nameof(IsVendorSpecific));
                }
            }
        }
        private eDiameterAvpDataType mDataType;
        [IsSerializedForLocalRepository]
        public eDiameterAvpDataType DataType
        {
            get
            {
                return mDataType;
            }
            set
            {
                if (mDataType != value)
                {
                    mDataType = value;
                    OnPropertyChanged(nameof(DataType));
                }
            }
        }

        private ObservableList<DiameterAVP> mNestedAvpList;
        public ObservableList<DiameterAVP> NestedAvpList
        {
            get
            {
                return mNestedAvpList;
            }
            set
            {
                if (mNestedAvpList != value)
                {
                    mNestedAvpList = value;
                    OnPropertyChanged(nameof(NestedAvpList));
                }
            }
        }

        private string mParentName;
        public string ParentName
        {
            get
            {
                return mParentName;
            }
            set
            {
                if (mParentName != value)
                {
                    mParentName = value;
                    OnPropertyChanged(nameof(ParentName));
                }
            }
        }

        private Guid mParentAvpGuid;
        [IsSerializedForLocalRepository]
        public Guid ParentAvpGuid
        {
            get
            {
                return mParentAvpGuid;
            }
            set
            {
                if (mParentAvpGuid != value)
                {
                    mParentAvpGuid = value;
                    OnPropertyChanged(nameof(ParentAvpGuid));
                }
            }
        }
        public DiameterAVP()
        {
            mNestedAvpList = [];
        }
    }
}
