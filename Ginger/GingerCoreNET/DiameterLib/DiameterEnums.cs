#region License
/*
Copyright © 2014-2026 European Support Limited

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

        public enum eDiameterVendor
        {
            [EnumValueDescription("")]
            Base,
            [EnumValueDescription("Huawei")]
            Huawei = 2011,
            [EnumValueDescription("3GPP")]
            _3GPP = 10415
        }

        public enum eCCRequestType
        {
            [EnumValueDescription("INITIAL_REQUEST")]
            InitialRequest = 1,
            [EnumValueDescription("UPDATE_REQUEST")]
            UpdateRequest = 2,
            [EnumValueDescription("TERMINATION_REQUEST")]
            TerminationRequest = 3,
            [EnumValueDescription("EVENT_REQUEST")]
            EventRequest = 4
        }

        public enum eSubscriptionIdType
        {
            [EnumValueDescription("END_USER_E164")]
            EndUserE164 = 0,
            [EnumValueDescription("END_USER_IMSI")]
            EndUserImsi = 1,
            [EnumValueDescription("END_USER_SIP_URI")]
            EndUserSipUri = 2,
            [EnumValueDescription("END_USER_NAI")]
            EndUserNai = 3,
            [EnumValueDescription("END_USER_PRIVATE")]
            EndUserPrivate = 4
        }
        public enum eCCSessionFailover
        {
            [EnumValueDescription("FAILOVER_NOT_SUPPORTED")]
            FailoverNotSupported = 0,
            [EnumValueDescription("FAILOVER_SUPPORTED")]
            FailoverSupported = 1
        }
        public enum eCCUnitType
        {
            [EnumValueDescription("TIME")]
            Time = 0,
            [EnumValueDescription("MONEY")]
            Money = 1,
            [EnumValueDescription("TOTAL-OCTETS")]
            TotalOctets = 2,
            [EnumValueDescription("INPUT-OCTETS")]
            InputOctets = 3,
            [EnumValueDescription("OUTPUT-OCTETS")]
            OutputOctets = 4,
            [EnumValueDescription("SERVICE-SPECIFIC-UNITS")]
            ServiceSpecificUnits = 5
        }
        public enum eCheckBalanceResult
        {
            [EnumValueDescription("ENOUGH_CREDIT")]
            EnoughCredit = 0,
            [EnumValueDescription("NO_CREDIT")]
            NoCredit = 1
        }
        public enum eCreditControl
        {
            [EnumValueDescription("CREDIT_AUTHORIZATION")]
            CreditAutorization,
            [EnumValueDescription("RE_AUTHORIZATION")]
            ReAuthorization
        }
        public enum eCreditControlFailureHandling
        {
            [EnumValueDescription("TERMINATE")]
            Terminate,
            [EnumValueDescription("CONTINUE")]
            Continue,
            [EnumValueDescription("RETRY_AND_TERMINATE")]
            RetryAndTerminate
        }
        public enum eDirectDebitingFailureHandling
        {
            [EnumValueDescription("TERMINATE_OR_BUFFER")]
            TerminateOrBuffer,
            [EnumValueDescription("CONTINUE")]
            Continue
        }
        public enum eFinalUnitAction
        {
            [EnumValueDescription("TERMINATE")]
            Terminate,
            [EnumValueDescription("REDIRECT")]
            Redirect,
            [EnumValueDescription("RESTRICT_ACCESS")]
            RestrictAccess
        }
        public enum eMultipleServicesIndicator
        {
            [EnumValueDescription("MULTIPLE_SERVICES_NOT_SUPPORTED")]
            MultipleServicesNotSupported,
            [EnumValueDescription("MULTIPLE_SERVICES_SUPPORTED")]
            MultipleServicesSupported
        }
        public enum eRedirectAddressType
        {
            [EnumValueDescription("IPv4 Address")]
            IPv4Address,
            [EnumValueDescription("IPv6 Address")]
            IPv6Address,
            [EnumValueDescription("URL")]
            Url,
            [EnumValueDescription("SIP URI")]
            SipUri
        }
        public enum eRequestedAction
        {
            [EnumValueDescription("DIRECT_DEBITING")]
            DirectDebiting,
            [EnumValueDescription("REFUND_ACCOUNT")]
            RefundAccount,
            [EnumValueDescription("CHECK_BALANCE")]
            CheckBalance,
            [EnumValueDescription("PRICE_ENQUIRY")]
            PriceEnquiry
        }
        public enum eTariffChangeUsage
        {
            [EnumValueDescription("UNIT_BEFORE_TARIFF_CHANGE")]
            UnitBeforeTariffChange,
            [EnumValueDescription("UNIT_AFTER_TARIFF_CHANGE")]
            UnitAfterTariffChange,
            [EnumValueDescription("UNIT_INDETERMINATE")]
            UnitIndeterminate
        }
        public enum eUserEquipmentInfoType
        {
            [EnumValueDescription("IMEISV")]
            Imeisv,
            [EnumValueDescription("MAC")]
            Mac,
            [EnumValueDescription("EUI64")]
            Eui64,
            [EnumValueDescription("MODIFIED_EUI64")]
            ModifiedEui64
        }
        public enum eDynamicAddressFlag
        {
            Static,
            Dynamic
        }
        public enum eServingNodeType
        {
            [EnumValueDescription("SGSN")]
            Sgsn,
            [EnumValueDescription("PMIPSGW")]
            Pmipsgw,
            [EnumValueDescription("GTPSGW")]
            Gtpsgw,
            [EnumValueDescription("ePDG")]
            _ePDG,
            [EnumValueDescription("hSGW")]
            _hSGW,
            [EnumValueDescription("MME")]
            Mme,
            [EnumValueDescription("TWAN")]
            Twan
        }
    }
}
