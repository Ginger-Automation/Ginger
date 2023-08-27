using System;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterMessage
    {
        public int ProtocolVersion { get; set; } = 1;
        public String MessageLength { get; set; }
        private int mMessageLength { get { return Convert.ToInt32(MessageLength); } }
        public String CommandCode
        {
            get
            {
                return mCommandCode.ToString();
            }
            set
            {
                if (value != mCommandCode.ToString())
                { mCommandCode = Convert.ToInt32(value); }
            }
        }
        private int mCommandCode;
        public String ApplicationId
        {
            get
            {
                return mApplicationId.ToString();
            }
            set
            {
                if (value != mCommandCode.ToString())
                {
                    mApplicationId = Convert.ToInt32(value);
                }
            }
        }
        private int mApplicationId;
        public String HopByHopIdentifier { get; set; }
        private int mHopByHopIdentifier { get { return Convert.ToInt32(HopByHopIdentifier); } }
        public String EndToEndIdentifier { get; set; }
        private int mEndToEndIdentifier { get { return Convert.ToInt32(EndToEndIdentifier); } }
        public DiameterMessage()
        {

        }
    }
}
