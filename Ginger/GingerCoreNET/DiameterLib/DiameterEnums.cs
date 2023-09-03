using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterEnums
    {
        public enum eDiameterMessageType
        {
            [EnumValueDescription("Custom Message")]
            None,
            [EnumValueDescription("Capabilities Exchange Request")]
            CapabilitiesExchangeRequest,
            [EnumValueDescription("Credit Control Request")]
            CreditControlRequest
        }

        public enum eDiameterAvpDataType
        {
            Address,
            OctetString,
            DiamIdent,
            Unsigned32,
            Unsigned64,
            Integer8,
            Integer32,
            Integer64,
            UTF8String,
            Enumerated,
            Time,
            Grouped
        }
    }
}
