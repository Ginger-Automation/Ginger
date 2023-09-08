using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private bool mIsGrouped;
        [IsSerializedForLocalRepository]
        public bool IsGrouped
        {
            get
            {
                return mIsGrouped;
            }
            set
            {
                if (mIsGrouped != value)
                {
                    mIsGrouped = value;
                    OnPropertyChanged(nameof(IsGrouped));
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

        private ObservableList<DiameterAVP> mNestedAvps = new ObservableList<DiameterAVP>();
        public ObservableList<DiameterAVP> NestedAvps
        {
            get
            {
                return mNestedAvps;
            }
            set
            {
                if (value != mNestedAvps)
                {
                    mNestedAvps = value;
                }
            }
        }
    }
}
