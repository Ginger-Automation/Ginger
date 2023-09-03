using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterMessage
    {
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
        private int mEndToEndIdentifier;

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
        public DiameterMessage()
        {

        }
    }
}
