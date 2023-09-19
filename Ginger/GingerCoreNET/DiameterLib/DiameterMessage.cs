using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterMessage
    {
        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                }
            }
        }
        public int ProtocolVersion { get; set; } = 1;
        private int mMessageLength;
        public int MessageLength
        {
            get
            {
                return mMessageLength;
            }
            set
            {
                if (mMessageLength != value)
                {
                    mMessageLength = value;
                }
            }
        }
        private int mCommandCode;
        public int CommandCode
        {
            get
            {
                return mCommandCode;
            }
            set
            {
                if (value != mCommandCode)
                {
                    mCommandCode = value;
                }
            }
        }
        private int mApplicationId;
        public int ApplicationId
        {
            get
            {
                return mApplicationId;
            }
            set
            {
                if (value != mApplicationId)
                {
                    mApplicationId = value;
                }
            }
        }
        private int mHopByHopIdentifier;
        public int HopByHopIdentifier
        {
            get
            {
                return mHopByHopIdentifier;
            }
            set
            {
                if (mHopByHopIdentifier != value)
                {
                    mHopByHopIdentifier = value;
                }
            }
        }
        private int mEndToEndIdentifier;
        public int EndToEndIdentifier
        {
            get
            {
                return mEndToEndIdentifier;
            }
            set
            {
                if (mEndToEndIdentifier != value)
                {
                    mEndToEndIdentifier = value;
                }
            }
        }

        private bool mIsRequestBitSet;
        public bool IsRequestBitSet
        {
            get
            {
                return mIsRequestBitSet;
            }
            set
            {
                if (mIsRequestBitSet != value)
                {
                    mIsRequestBitSet = value;
                }
            }
        }
        private bool mIsProxiableBitSet;
        public bool IsProxiableBitSet
        {
            get
            {
                return mIsProxiableBitSet;
            }
            set
            {
                if (mIsProxiableBitSet != value)
                {
                    mIsProxiableBitSet = value;
                }
            }
        }
        private bool mIsErrorBitSet;
        public bool IsErrorBitSet
        {
            get
            {
                return mIsErrorBitSet;
            }
            set
            {
                if (mIsErrorBitSet != value)
                {
                    mIsErrorBitSet = value;
                }
            }
        }
        private bool mIsRetransmittedBitSet;
        public bool IsRetransmittedBitSet
        {
            get
            {
                return mIsRetransmittedBitSet;
            }
            set
            {
                if (mIsRetransmittedBitSet != value)
                {
                    mIsRetransmittedBitSet = value;
                }
            }
        }

        private ObservableList<DiameterAVP> mAvpList = new ObservableList<DiameterAVP>();
        public ObservableList<DiameterAVP> AvpList
        {
            get
            {
                return mAvpList;
            }
            set
            {
                if (mAvpList != value)
                {
                    mAvpList = value;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
