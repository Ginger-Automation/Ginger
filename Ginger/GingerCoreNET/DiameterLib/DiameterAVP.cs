using Amdocs.Ginger.Common;
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
        public int Length;

        public byte Flags;
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
                    //OnPropertyChanged(nameof(Code));
                }
            }
        }
        public String Name;
        //public object Value;
        public int VendorId;
        public bool isGrouped;
        public bool isMandatory;
        public bool isVendorSpecific;
        public eDiameterAvpDataType DataType;
        public DiameterAVP() { }
    }
}
