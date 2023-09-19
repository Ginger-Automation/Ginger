using System.Data;
using System.Reflection;
using System;
using System.IO;
using Amdocs.Ginger.Common;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;
using System.Linq;
using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using System.Text;
using GingerCoreNET.GeneralLib;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.IO;
using System.Collections.Generic;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterUtils
    {
        private const string DIAMETER_AVP_DICTIONARY_FILENAME = "AVPDictionary.xml";
        private static ObservableList<DiameterAVP> mAvpDictionaryList;
        private static readonly object dictionaryLock = new object();
        private static readonly object fileLock = new object();
        public static ObservableList<DiameterAVP> AvpDictionaryList
        {
            get
            {
                if (mAvpDictionaryList == null || !mAvpDictionaryList.Any())
                {
                    lock (dictionaryLock)
                    {
                        if (mAvpDictionaryList == null || !mAvpDictionaryList.Any())
                        {
                            mAvpDictionaryList = LoadDictionary();
                        }
                    }
                }
                return mAvpDictionaryList;
            }
        }

        public string ResponseMessage;
        public string RequestFileContent;
        public string ResponseFileContent;
        private DiameterMessage mMessage;
        public DiameterMessage Message
        {
            get
            {
                return mMessage;
            }
            set
            {
                if (mMessage != value)
                {
                    mMessage = value;
                }
            }
        }
        private DiameterMessage mResponse;
        public DiameterMessage Response
        {
            get { return mResponse; }
            set
            {
                mResponse = value;
            }
        }
        public DiameterUtils(DiameterMessage message)
        {
            mMessage = message ?? new DiameterMessage();
        }
        public static ObservableList<DiameterAVP> LoadDictionary()
        {
            ObservableList<DiameterAVP> avpListDictionary = new ObservableList<DiameterAVP>();
            string resourcePath = Path.Combine(Path.GetDirectoryName(typeof(DiameterUtils).Assembly.Location), "DiameterLib", DIAMETER_AVP_DICTIONARY_FILENAME);
            try
            {
                using (DataSet dataSet1 = new DataSet())
                {
                    dataSet1.ReadXml(resourcePath);
                    DataTable dataTable = dataSet1.Tables.Count > 0 ? dataSet1.Tables[0] : null;
                    if (dataTable != null)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            DiameterAVP avp = new DiameterAVP()
                            {
                                Name = row["name"].ToString(),
                                Code = Convert.ToInt32(row["code"]),
                                DataType = (eDiameterAvpDataType)Enum.Parse(typeof(eDiameterAvpDataType), row["type"].ToString()),
                                IsMandatory = Convert.ToBoolean(row["isMandatory"]),
                                IsVendorSpecific = Convert.ToBoolean(row["isVendorSpecific"]),
                            };
                            avp.IsGrouped = avp.DataType == eDiameterAvpDataType.Grouped;
                            avpListDictionary.Add(avp);
                        }
                        var sortedDictionary = avpListDictionary.OrderBy(a => a.Name).ToList();
                        avpListDictionary = new ObservableList<DiameterAVP>(sortedDictionary);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"AVPs dictionary file '{DIAMETER_AVP_DICTIONARY_FILENAME}' not found. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
            }
            catch (XmlException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read AVPs dictionary from file '{DIAMETER_AVP_DICTIONARY_FILENAME}'. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while loading AVPs dictionary. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
            }

            return avpListDictionary;
        }
        public static ObservableList<DiameterAVP> GetMandatoryAVPForMessage(DiameterEnums.eDiameterMessageType messageType)
        {
            ObservableList<DiameterAVP> avpList = null;
            if (messageType == eDiameterMessageType.CapabilitiesExchangeRequest)
            {
                string[] avpsNamesCER = { "Origin-Host", "Origin-Realm", "Host-IP-Address", "Vendor-Id", "Product-Name", "Origin-State-Id" };
                if (AvpDictionaryList != null && AvpDictionaryList.Any())
                {
                    List<DiameterAVP> avps = AvpDictionaryList.Where(avp => avpsNamesCER.Contains(avp.Name)).ToList();
                    if (avps.Any())
                    {
                        avpList = new ObservableList<DiameterAVP>(avps);
                    }
                }
            }
            else if (messageType == eDiameterMessageType.CreditControlRequest)
            {
                string[] avpsNamesCCR = {
                    "Session-Id", "Origin-Host", "Origin-Realm", "Destination-Realm",
                    "Auth-Application-Id", "Service-Context-Id", "CC-Request-Type",
                    "CC-Request-Number", "Destination-Host", "Origin-State-Id",
                    "User-Name", "3GPP-RAT-Type", "Event-Timestamp"};
                if (AvpDictionaryList != null && AvpDictionaryList.Any())
                {
                    List<DiameterAVP> avps = AvpDictionaryList.Where(avp => avpsNamesCCR.Contains(avp.Name)).ToList();
                    if (avps.Any())
                    {
                        avpList = new ObservableList<DiameterAVP>(avps);
                    }
                }
            }

            return avpList;
        }
        public static string GetRawRequestContentPreview(ActDiameter act)
        {
            try
            {
                DiameterUtils diameterUtils = new DiameterUtils(new DiameterMessage());
                //Prepare act input values
                Context context = Context.GetAsContext(act.Context);
                if (context != null && context.Runner != null)
                {
                    context.Runner.PrepActionValueExpression(act, context.BusinessFlow);
                }
                //Create Request content
                if (diameterUtils.ConstructDiameterRequest(act))
                {
                    string requestMessage = CreateMessageRawRequestResponse(act, diameterUtils.Message);
                    return requestMessage;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Diameter Message Request preview content", ex);
                return string.Empty;
            }
        }
        private static string CreateMessageRawRequestResponse(ActDiameter act, DiameterMessage message)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<Diameter Message ::= <" + General.GetEnumValueDescription(typeof(eDiameterMessageType), act.DiameterMessageType)
                + $", code=\"{message.CommandCode}\""
                + $", request=\"{message.IsRequestBitSet.ToString().ToLower()}\""
                + $", proxiable=\"{message.IsProxiableBitSet.ToString().ToLower()}\""
                + $", error=\"{message.IsErrorBitSet.ToString().ToLower()}\""
                + $", retransmit=\"{message.IsRetransmittedBitSet.ToString().ToLower()}\""
                + $", hopbyhop=\"{message.HopByHopIdentifier}\""
                + $", endtoend=\"{message.EndToEndIdentifier}\""
                + $">{Environment.NewLine}");
            foreach (DiameterAVP avp in message?.AvpList)
            {
                stringBuilder.Append(CreateAVPAsString(avp) + Environment.NewLine);
            }
            stringBuilder.Append("</Diameter Message>");
            return stringBuilder.ToString();
        }
        private static string CreateAVPAsString(DiameterAVP avp, int identLevel = 1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string identation = new string('\t', identLevel);
            if (avp != null)
            {
                if (avp.DataType == eDiameterAvpDataType.Grouped)
                {
                    stringBuilder.Append($"{identation}<grouped avp name=\"{avp.Name}\" mandatory=\"{avp.IsMandatory.ToString().ToLower()}\">");
                    if (avp.NestedAvpList != null && avp.NestedAvpList.Any())
                    {
                        foreach (DiameterAVP nestedAVP in avp.NestedAvpList)
                        {
                            stringBuilder.Append(Environment.NewLine + CreateAVPAsString(nestedAVP, identLevel + 1));
                        }
                        stringBuilder.Append($"{Environment.NewLine}{identation}</grouped avp>");
                    }
                    else
                    {
                        stringBuilder.Append($" </grouped avp>");
                    }
                }
                else
                {
                    stringBuilder.Append($"{identation}<avp name=\"{avp.Name}\" mandatory=\"{avp.IsMandatory.ToString().ToLower()}\" value=\"{avp.ValueForDriver}\" </avp>");
                }
            }
            return stringBuilder.ToString();
        }
        public bool ConstructDiameterRequest(ActDiameter act)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to construct the diameter request");
            try
            {
                string[] messagePropertyNames = new string[]
                {
                    nameof(DiameterMessage.IsRequestBitSet),
                    nameof(DiameterMessage.IsProxiableBitSet),
                    nameof(DiameterMessage.IsErrorBitSet),
                    nameof(DiameterMessage.CommandCode),
                    nameof(DiameterMessage.ApplicationId),
                    nameof(DiameterMessage.HopByHopIdentifier),
                    nameof(DiameterMessage.EndToEndIdentifier)
                };

                foreach (string property in messagePropertyNames)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Setting Message's property {property}");
                    if (!SetMessageProperty(act, property))
                    {
                        HandleSetMessagePropertyError(act, property);
                        return false;
                    }
                }

                Message.Name = General.GetEnumValueDescription(typeof(eDiameterMessageType), act.DiameterMessageType);

                Reporter.ToLog(eLogLevel.DEBUG, $"Setting Message's AVPs");
                if (!SetMessageAvps(act))
                {
                    HandleSetMessageAvpsError(act);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to construct the diameter message {Message.Name}{Environment.NewLine}error message: '{ex.Message}{Environment.NewLine}{ex.StackTrace}'");
                return false;
            }
        }
        private void HandleSetMessagePropertyError(ActDiameter act, string property)
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Failed to construct the diameter message on property '{property}'");
            UpdateActionError(act, $"An error occurred while constructing the diameter message for the {property} property.");
        }
        private void HandleSetMessageAvpsError(ActDiameter act)
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Failed to construct the diameter message on AVPs");
            UpdateActionError(act, $"An error occurred while adding AVPs to the diameter message.");
        }
        private bool SetMessageProperty(ActDiameter act, string property)
        {
            try
            {
                PropertyInfo propertyInfo = typeof(DiameterMessage).GetProperty(property);
                bool isSuccessfullyParsed = false;

                if (propertyInfo == null)
                {
                    HandlePropertyNotFound(act, property);
                    return false;
                }

                if (propertyInfo.PropertyType == typeof(int))
                {
                    isSuccessfullyParsed = TryParseAndSetValue<int>(propertyInfo, act, property);
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    isSuccessfullyParsed = TryParseAndSetValue<bool>(propertyInfo, act, property);
                }
                else
                {
                    HandleUnsupportedPropertyType(property);
                }

                if (!isSuccessfullyParsed)
                {
                    UpdateActionError(act, $"Failed to set {property} value");
                }

                return isSuccessfullyParsed;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while setting {property}: {ex.Message}");
                return false;
            }
        }
        private static void HandleUnsupportedPropertyType(string property)
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Unsupported property type for {property}.");
        }

        private void HandlePropertyNotFound(ActDiameter act, string property)
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Property {property} not found in DiameterMessage.");
            UpdateActionError(act, $"Property {property} not found");
        }
        private bool TryParseAndSetValue<T>(PropertyInfo propertyInfo, ActDiameter act, string property)
        {
            if (TryParse<T>(act.GetInputParamCalculatedValue(property), out T value))
            {
                propertyInfo.SetValue(Message, value);
                Reporter.ToLog(eLogLevel.DEBUG, $"Parsed the {property} as a {typeof(T).Name} with value - {value}");
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to parse {property} as a {typeof(T).Name}.");
                return false;
            }
        }
        private bool TryParse<T>(string input, out T result)
        {
            try
            {
                result = (T)Convert.ChangeType(input, typeof(T));
                return true;
            }
            catch (FormatException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Format exception occurred while parsing: {ex.Message}");
                result = default;
                return false;
            }
            catch (InvalidCastException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Invalid cast exception occurred while parsing: {ex.Message}");
                result = default;
                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected exception occurred while parsing: {ex.Message}");
                result = default;
                return false;
            }
        }
        private bool SetMessageAvps(ActDiameter act)
        {
            if (act.RequestAvpList == null || !act.RequestAvpList.Any())
            {
                return false;
            }
            ClearChildrenFromAVPs(act);
            foreach (DiameterAVP avp in act.RequestAvpList)
            {
                if (avp.ParentAvpGuid == Guid.Empty)
                {
                    Message.AvpList.Add(avp);
                }
                else
                {
                    AddAVPToParent(act, avp);
                }
            }
            return Message.AvpList != null && Message.AvpList.Any();
        }
        private void ClearChildrenFromAVPs(ActDiameter act)
        {
            foreach (DiameterAVP avp in act.RequestAvpList)
            {
                if (avp.NestedAvpList != null && avp.NestedAvpList.Any())
                {
                    avp.NestedAvpList.Clear();
                }
            }
            foreach (DiameterAVP avp in act.CustomResponseAvpList)
            {
                if (avp.NestedAvpList != null && avp.NestedAvpList.Any())
                {
                    avp.NestedAvpList.Clear();
                }
            }
        }
        private void AddAVPToParent(ActDiameter act, DiameterAVP childAvp)
        {
            DiameterAVP parentAVP = act.RequestAvpList.FirstOrDefault(avp => avp.Guid == childAvp.ParentAvpGuid);
            if (parentAVP != null)
            {
                parentAVP.NestedAvpList?.Add(childAvp);
            }
        }
        private byte[] ConvertMessageToBytes()
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to convert message: {Message} to bytes");
            try
            {
                const int messageLengthOffset = 1;
                const int commandCodeOffset = 5;
                const int applicationIdOffset = 8;
                const int hopByHopIdentifierOffset = 12;
                const int endToEndIdentifierOffset = 16;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    if (!ConvertProtocolVersionToByte(memoryStream))
                    {
                        return null;
                    }

                    //Reserve space for message length
                    if (!ConvertMessageLengthToBytes(memoryStream, isReserve: true, messageLengthOffset))
                    {
                        return null;
                    }

                    if (!ConvertMessageCommandFlagsToByte(memoryStream))
                    {
                        return null;
                    }

                    if (!ConvertMessageCommandCodeToBytes(memoryStream, commandCodeOffset))
                    {
                        return null;
                    }

                    // application Id, hop-by-hop and end-to-end identifiers
                    if (!ConvertMessageApplicationIdToBytes(memoryStream, applicationIdOffset))
                    {
                        return null;
                    }
                    if (!ConvertMessageHopByHopToBytes(memoryStream, hopByHopIdentifierOffset))
                    {
                        return null;
                    }
                    if (!ConvertMessageEndToEndToBytes(memoryStream, endToEndIdentifierOffset))
                    {
                        return null;
                    }

                    if (!ConvertMessageAvpListToBytes(memoryStream))
                    {
                        return null;
                    }

                    SetMessageLength((int)memoryStream.Length);

                    // Write message length into memory stream with its actual value
                    if (!ConvertMessageLengthToBytes(memoryStream, isReserve: false, messageLengthOffset))
                    {
                        return null;
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting message object into bytes{Environment.NewLine}Error message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return null;
            }
        }
        private bool ConvertMessageAvpListToBytes(MemoryStream memoryStream)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to convert avp list into bytes");
            try
            {

                foreach (DiameterAVP avp in Message.AvpList)
                {
                    byte[] avpAsBytes = ConvertAvpToBytes(avp);
                    if (avpAsBytes != null)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Converted AVP {avp.Name} to bytes successfully");
                        WriteBytesToStream(memoryStream, avpAsBytes, (int)memoryStream.Position);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert AVP {avp.Name} to bytes");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert AVPs to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageEndToEndToBytes(MemoryStream memoryStream, int endToEndIdentifierOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting message End-To-End Identifier: {Message.EndToEndIdentifier} to bytes");
                WriteInt32ToStream(memoryStream, IPAddress.HostToNetworkOrder(Message.EndToEndIdentifier), endToEndIdentifierOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert end-to-end identifier: {Message.EndToEndIdentifier} to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageHopByHopToBytes(MemoryStream memoryStream, int hopByHopOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting message Hop-By-Hop Identifier: {Message.HopByHopIdentifier} to bytes");
                WriteInt32ToStream(memoryStream, IPAddress.HostToNetworkOrder(Message.HopByHopIdentifier), hopByHopOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert hop-by-hop identifier: {Message.HopByHopIdentifier} to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageApplicationIdToBytes(MemoryStream memoryStream, int applicationIdOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting message application id: {Message.ApplicationId} to bytes");
                WriteInt32ToStream(memoryStream, IPAddress.HostToNetworkOrder(Message.ApplicationId), applicationIdOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert application id: {Message.ApplicationId} to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageCommandCodeToBytes(MemoryStream memoryStream, int commandCodeOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting message command code: {Message.CommandCode} to bytes");
                WriteThreeBytesToStream(memoryStream, IPAddress.HostToNetworkOrder(Message.CommandCode), commandCodeOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert message command code {Message.CommandCode} to bytes. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageCommandFlagsToByte(MemoryStream memoryStream)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting message command flags to byte");
                //Write command flags
                byte commandFlags = GetCommandFlags(Message);
                memoryStream.WriteByte(commandFlags);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert message command flags to byte {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private void SetMessageLength(int messageLength)
        {
            Message.MessageLength = messageLength;
        }
        private bool ConvertMessageLengthToBytes(MemoryStream memoryStream, bool isReserve, int messageLengthOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, isReserve ? $"Reserving space in memory for message length bytes" : $"Starting to convert message length: {Message.MessageLength} to bytes");
                if (isReserve)
                {
                    //Reserve space for message length
                    WriteThreeBytesToStream(memoryStream, value: 0, messageLengthOffset);
                }
                else
                {
                    WriteThreeBytesToStream(memoryStream, value: IPAddress.HostToNetworkOrder(Message.MessageLength), messageLengthOffset);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, isReserve ? $"Failed to reserve space for message length {ex.Message}{Environment.NewLine}{ex.StackTrace}" : $"Failed to convert message length {Message.MessageLength} into bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private byte GetCommandFlags(DiameterMessage message)
        {
            byte commandFlags = 0;
            if (message.IsRequestBitSet)
            {
                commandFlags |= 1 << 7;
            }
            if (message.IsProxiableBitSet)
            {
                commandFlags |= 1 << 6;
            }
            if (message.IsErrorBitSet)
            {
                commandFlags |= 1 << 5;
            }
            return commandFlags;
        }
        private void WriteInt32ToStream(MemoryStream stream, int value, int offset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Writing value: {value} to memory stream");
                byte[] bytes = BitConverter.GetBytes(value);
                WriteBytesToStream(stream, data: bytes, seekPosition: offset);
            }
            catch (InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to write 4 bytes with value: {value} to memory stream. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private void WriteThreeBytesToStream(MemoryStream stream, int value, int offset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Writing value: {value} to memory stream");
                byte[] bytes = BitConverter.GetBytes(value);
                WriteBytesToStream(stream, data: bytes, seekPosition: offset, offsetInData: 1, byteCount: 3);
            }
            catch (InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to write 4 bytes with value: {value} to memory stream. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private void WriteBytesToStream(MemoryStream stream, byte[] data, int seekPosition, int offsetInData = 0, int byteCount = 0)
        {
            try
            {
                stream.Seek(seekPosition, SeekOrigin.Begin);

                if (byteCount == 0)
                {
                    byteCount = data.Length;
                }

                stream.Write(data, offsetInData, byteCount);
            }
            catch (InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to write bytes to memory stream. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertAvpToBytes(DiameterAVP avp)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to convert avp: {avp.Name} to bytes");
            try
            {
                const int avpCodeOffset = 0;
                const int vendorIdOffset = 8;
                const int avpLengthOffset = 5;
                int avpValueOffset = 8;
                int padding = 0;

                using (MemoryStream stream = new MemoryStream())
                {
                    if (!ConvertAvpCodeToBytes(stream, avp.Code, avpCodeOffset))
                    {
                        return null;
                    }

                    if (!ConvertAvpFlagsToByte(stream, avp))
                    {
                        return null;
                    }

                    // Reserve space for AVP length
                    if (!ConvertAvpLengthToBytes(stream, avpLengthOffset, isReserve: true))
                    {
                        return null;
                    }

                    if (avp.IsVendorSpecific)
                    {
                        if (!ConvertAvpVendorIdToBytes(stream, avp.VendorId, vendorIdOffset))
                        {
                            return null;
                        }
                        avpValueOffset = 12;
                    }

                    // Grouped AVP doesn't have value
                    if (avp.DataType != eDiameterAvpDataType.Grouped)
                    {
                        // Get avp value as bytes
                        if (string.IsNullOrEmpty(avp.ValueForDriver))
                        {
                            return null;
                        }
                    }

                    byte[] avpValueAsBytes = GetAvpValueAsBytes(avp.ValueForDriver, avp.DataType, ref padding, stream, avp);

                    // Set the avp length
                    SetAvpLength(avp, padding, (int)stream.Length, avpValueAsBytes.Length);

                    // Write Avp Length
                    if (!ConvertAvpLengthToBytes(stream, avpLengthOffset, value: IPAddress.HostToNetworkOrder(avp.Length)))
                    {
                        return null;
                    }

                    if (!WriteAvpValueToStream(stream, avpValueAsBytes, avpValueOffset))
                    {
                        return null;
                    }

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Avps into bytes{Environment.NewLine}Error message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return null;
            }
        }
        private bool WriteAvpValueToStream(MemoryStream stream, byte[] avpValueAsBytes, int avpValueOffset)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Writing AVP value bytes to memory stream");
            try
            {
                WriteBytesToStream(stream, avpValueAsBytes, avpValueOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to write avp value bytes to stream {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private static void SetAvpLength(DiameterAVP avp, int padding, int streamLength, int avpValueLength)
        {
            // Get the AVP Length, Discard 0th Byte and Always excluding the paddings from length 
            avp.Length = avpValueLength + streamLength - padding;
        }
        private bool ConvertAvpVendorIdToBytes(MemoryStream stream, int vendorId, int vendorIdOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting avp vendor id: {vendorId} to bytes");
                WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(vendorId), vendorIdOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert Vendor Id: {vendorId} to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertAvpLengthToBytes(MemoryStream memoryStream, int avpLengthOffset, bool isReserve = false, int value = 0)
        {
            Reporter.ToLog(eLogLevel.DEBUG, isReserve ? $"Reserving space for avp length bytes" : $"Converting avp length: {value} to bytes");
            try
            {
                if (isReserve)
                {
                    //Reserve space for message length
                    WriteThreeBytesToStream(memoryStream, value: value, avpLengthOffset);
                }
                else
                {
                    WriteThreeBytesToStream(memoryStream, value: value, avpLengthOffset);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, isReserve ? $"Failed to reserve space for avp length {ex.Message}{Environment.NewLine}{ex.StackTrace}" : $"Failed to convert avp length: {value} into bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertAvpFlagsToByte(MemoryStream stream, DiameterAVP avp)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting avp flags to byte");
                // Write Avp Flags
                byte avpFlags = GetAvpFlags(avp);
                stream.WriteByte(avpFlags);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert avp flags to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertAvpCodeToBytes(MemoryStream memoryStream, int avpCode, int avpCodeOffset)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Converting avp code: {avpCode} to bytes");
                WriteInt32ToStream(memoryStream, IPAddress.HostToNetworkOrder(avpCode), avpCodeOffset);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert avp code: {avpCode} to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private byte[] GetAvpValueAsBytes(string valueForDriver, eDiameterAvpDataType dataType, ref int padding, MemoryStream stream, DiameterAVP avp)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting avp value: {valueForDriver} to bytes");
            try
            {
                byte[] valueAsBytes = null;

                switch (dataType)
                {
                    case eDiameterAvpDataType.Address:
                        {
                            valueAsBytes = ConvertIpAddressToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.OctetString:
                        {
                            valueAsBytes = ConvertOctetStringToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.DiamIdent:
                        {
                            valueAsBytes = ConvertDiamIdentToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Unsigned32:
                        {
                            valueAsBytes = ConvertUnsigned32ToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Integer8:
                    case eDiameterAvpDataType.Integer64:
                    case eDiameterAvpDataType.Unsigned64:
                        {
                            valueAsBytes = ConvertUnsigned64ToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Integer32:
                        {
                            valueAsBytes = ConvertInteger32ToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.UTF8String:
                        {
                            valueAsBytes = ConvertUTF8StringToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Enumerated:
                        {
                            valueAsBytes = ConvertEnumeratedToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Time:
                        {
                            valueAsBytes = ConvertTimeToBytes(valueForDriver, ref padding);
                            break;
                        }
                    case eDiameterAvpDataType.Grouped:
                        {
                            //TODO: DIAMETER - convert grouped data type to bytes
                            valueAsBytes = ConvertGroupedToBytes(stream, avp, ref padding);
                            break;
                        }
                    default:
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Type {dataType} is not supported.");
                            return null;
                        }
                }

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to process Avps for sending the request {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return null;
        }
        private byte[] ConvertGroupedToBytes(MemoryStream stream, DiameterAVP avp, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting children AVPs to bytes");
            List<byte> childrenAVPsBytesList = new List<byte>();
            foreach (var childAvp in avp.NestedAvpList)
            {
                byte[] childAvpBytes = ConvertAvpToBytes(childAvp);
                if (childAvpBytes != null)
                {
                    childrenAVPsBytesList.AddRange(childAvpBytes);
                }
            }

            byte[] childrenAVPsBytes = childrenAVPsBytesList.ToArray();

            padding = CalculatePadding(childrenAVPsBytes.Length);

            //Add Padding Bytes
            AddPaddingBytesToAvpValue(padding, childrenAVPsBytes);

            return childrenAVPsBytes;
        }
        private byte[] ConvertTimeToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Time value: {avpValue} to bytes");
            try
            {
                ulong seconds = Convert.ToUInt64(avpValue);

                byte[] ntpTimeStamp = ConvertMilisecondsToNTP(seconds * 1000);

                //Calculate Padding
                padding = CalculatePadding(ntpTimeStamp.Length);

                byte[] valueAsBytes = new byte[ntpTimeStamp.Length + padding];

                //Copy Value Bytes
                BufferCopy(ntpTimeStamp, valueAsBytes, 0, ntpTimeStamp.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Time Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertEnumeratedToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Enumerated value: {avpValue} to bytes");
            try
            {
                int avpValueAsInt = Convert.ToInt32(avpValue);
                byte[] enumeratedBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

                //Calculate Padding
                padding = CalculatePadding(enumeratedBytes.Length);

                byte[] valueAsBytes = new byte[enumeratedBytes.Length + padding];


                //Copy Value Bytes
                BufferCopy(enumeratedBytes, valueAsBytes, 0, enumeratedBytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Enumerated Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertUTF8StringToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting UTF8String value: {avpValue} to bytes");
            try
            {
                byte[] UTF8Bytes = Encoding.UTF8.GetBytes(avpValue);
                //Calculate Padding
                padding = CalculatePadding(UTF8Bytes.Length);

                byte[] valueAsBytes = new byte[UTF8Bytes.Length + padding];

                //Copy Value Bytes
                BufferCopy(UTF8Bytes, valueAsBytes, 0, UTF8Bytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting UTF8String Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertInteger32ToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Integer32 value: {avpValue} to bytes");
            try
            {
                int avpValueAsInt = Convert.ToInt32(avpValue);
                byte[] integer32Bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

                //Calculate Padding
                padding = CalculatePadding(integer32Bytes.Length);

                byte[] valueAsBytes = new byte[integer32Bytes.Length + padding];

                //Copy Value Bytes
                BufferCopy(integer32Bytes, valueAsBytes, 0, integer32Bytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Integer32 Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertUnsigned64ToBytes(string valueForDriver, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Unsigned64/Integer64/Integer8 value: {valueForDriver} to bytes");
            try
            {
                long avpValue = Convert.ToInt64(valueForDriver);

                byte[] U64Bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValue));

                //Calculate Padding
                padding = CalculatePadding(U64Bytes.Length);

                byte[] valueAsBytes = new byte[U64Bytes.Length + padding];

                //Copy Value Bytes
                BufferCopy(U64Bytes, valueAsBytes, 0, U64Bytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Unsgined64 Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertUnsigned32ToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Unsigned32: {avpValue} to bytes");
            try
            {
                int avpValueAsInt = Convert.ToInt32(avpValue);

                byte[] unsigned32Bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

                //Calculate padding
                padding = CalculatePadding(unsigned32Bytes.Length);

                byte[] valueAsBytes = new byte[unsigned32Bytes.Length + padding];

                //Copy Value Bytes
                BufferCopy(unsigned32Bytes, valueAsBytes, 0, unsigned32Bytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Unsgined32 Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertDiamIdentToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting DiamIdent value: {avpValue} to bytes");
            try
            {
                byte[] diamIdentityBytes = Encoding.UTF8.GetBytes(avpValue);

                // Calculate padding
                padding = CalculatePadding(diamIdentityBytes.Length);

                byte[] valueAsBytes = new byte[diamIdentityBytes.Length + padding];

                //Copy Value Bytes
                BufferCopy(diamIdentityBytes, valueAsBytes, 0, diamIdentityBytes.Length);

                //Add Padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting DiamIdent Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertOctetStringToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting OctetString value: {avpValue} to bytes");
            try
            {
                byte[] octetStringBytes;
                if (avpValue.StartsWith("0x"))
                {
                    // Remove "0x" and spaces if present, and parse as hexadecimal
                    avpValue = avpValue.Replace("0x", "").Replace(" ", "");

                    int byteCount = avpValue.Length / 2;
                    octetStringBytes = new byte[byteCount];

                    for (int i = 0; i < byteCount; i++)
                    {
                        octetStringBytes[i] = Convert.ToByte(avpValue.Substring(i * 2, 2), 16);
                    }
                }
                else
                {
                    octetStringBytes = Encoding.ASCII.GetBytes(avpValue);
                }

                // Calculate padding
                padding = CalculatePadding(octetStringBytes.Length);

                byte[] valueAsBytes = new byte[octetStringBytes.Length + padding];

                // Copy Value bytes
                BufferCopy(octetStringBytes, valueAsBytes, 0, octetStringBytes.Length);

                //Add padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting OctetString Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private byte[] ConvertIpAddressToBytes(string avpValue, ref int padding)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting Ip Address value: {avpValue} to bytes");
            try
            {
                const short addressFamily = 1;
                byte[] addressFamilyBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(addressFamily));
                byte[] addressBytes = IPAddress.Parse(avpValue).GetAddressBytes();

                // Calculate padding
                padding = CalculatePadding(addressFamilyBytes.Length + addressBytes.Length);

                byte[] valueAsBytes = new byte[addressFamilyBytes.Length + addressBytes.Length + padding];

                // Copy Address Family
                BufferCopy(addressFamilyBytes, valueAsBytes, 0, addressFamilyBytes.Length);

                // Copy Address Bytes
                BufferCopy(addressBytes, valueAsBytes, 2, addressBytes.Length);

                //Add padding Bytes
                AddPaddingBytesToAvpValue(padding, valueAsBytes);

                return valueAsBytes;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting IPAddress Avp value to bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }
        private static int CalculatePadding(int bytesArrayLength)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Calculating padding for bytes array length: {bytesArrayLength}");
            int remainder = bytesArrayLength % 4;
            return remainder == 0 ? 0 : 4 - remainder;
        }
        private static void AddPaddingBytesToAvpValue(int padding, byte[] valueAsBytes)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Adding {padding} bytes as padding");
            if (padding > 0)
            {
                int destinationOffset = valueAsBytes.Length - padding;
                Buffer.BlockCopy(Enumerable.Repeat((byte)0x0, padding).ToArray(), 0, valueAsBytes, destinationOffset, padding);
            }
        }
        private byte GetAvpFlags(DiameterAVP avp)
        {
            byte avpFlags = 0;
            if (avp.IsVendorSpecific)
            {
                avpFlags |= 1 << 7;
            }
            if (avp.IsMandatory)
            {
                avpFlags |= 1 << 6;
            }
            return avpFlags;
        }
        private void BufferCopy(byte[] source, byte[] destination, int destinationOffset, int count)
        {
            Buffer.BlockCopy(src: source, srcOffset: 0, dst: destination, dstOffset: destinationOffset, count);
        }
        private byte[] ConvertMilisecondsToNTP(decimal milliseconds)
        {
            var ntpData = new byte[4];

            decimal intpart = milliseconds / 1000;
            decimal fractpart = milliseconds % 1000 * 0x100000000L / 1000m;


            var temp = intpart;
            for (var i = 3; i >= 0; i--)
            {
                ntpData[i] = (byte)(temp % 256);
                temp = temp / 256;
            }

            return ntpData;
        }
        private DiameterMessage ProcessDiameterResponse(byte[] receivedBytes, ActDiameter act)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to process diameter response");
            try
            {
                DiameterMessage response = new DiameterMessage();
                using (MemoryStream stream = new MemoryStream(receivedBytes))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (!ConvertProtocolVersionFromByte(reader, response))
                    {
                        UpdateActionError(act, $"Diameter only supports Protocol Version 1, Protocol Version received from response was {response.ProtocolVersion}");
                        return null;
                    }

                    if (!ConvertMessageLengthFromBytes(reader, byteCount: 3, response))
                    {
                        UpdateActionError(act, $"Failed to read message length from response: {response.MessageLength}");
                        return null;
                    }

                    // Get Command Flags
                    if (!ConvertCommandFlagsFromByte(reader, response))
                    {
                        UpdateActionError(act, $"Failed to read command flags from response");
                        return null;
                    }

                    if (!ConvertCommandCodeFromBytes(reader, byteCount: 3, response))
                    {
                        UpdateActionError(act, $"Failed to read command code from response: {response.CommandCode}");
                        return null;
                    }

                    if (!ConvertApplicationIdFromBytes(reader, response))
                    {
                        UpdateActionError(act, $"Failed to read application id from response: {response.ApplicationId}");
                        return null;
                    }

                    if (!ConvertHopByHopFromBytes(reader, response))
                    {
                        UpdateActionError(act, $"Failed to read hop-by-hop identifier from response: {response.HopByHopIdentifier}");
                        return null;
                    }
                    if (!ConvertEndToEndFromBytes(reader, response))
                    {
                        UpdateActionError(act, $"Failed to read end-to-end identifier from response: {response.EndToEndIdentifier}");
                        return null;
                    }

                    // Get the AVPs
                    int headerLength = 20;
                    if (stream.Length - stream.Position < response.MessageLength - headerLength)
                    {
                        UpdateActionError(act, $"Insufficient data to process the response AVPs");
                        return null;
                    }

                    if (!ReadResponseAvpBytes(reader, headerLength, response, out byte[] avpBytes))
                    {
                        if (avpBytes == null || !avpBytes.Any())
                        {
                            response.AvpList = null;
                        }

                        UpdateActionError(act, $"failed to read the response AVP list bytes");
                        return null;
                    }

                    response.AvpList = ProcessMessageResponseAvpList(avpBytes, act);

                    if (response.AvpList == null || !response.AvpList.Any())
                    {
                        UpdateActionError(act, $"Failed to read AVP List from response");
                        return null;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error processing Diameter response + {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private bool ReadResponseAvpBytes(BinaryReader reader, int headerLength, DiameterMessage response, out byte[] avpBytes)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to read response AVPs list bytes");
            try
            {
                avpBytes = reader.ReadBytes(response.MessageLength - headerLength);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Failed to read AVPs bytes from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                avpBytes = null;
                return false;
            }
        }
        private bool ConvertCommandFlagsFromByte(BinaryReader reader, DiameterMessage response)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Reading response command flags byte");
            try
            {
                byte commandFlags = reader.ReadByte();
                if (commandFlags != 0)
                {
                    SetResponseCommandFlags(commandFlags, response);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert command flags from byte {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertEndToEndFromBytes(BinaryReader reader, DiameterMessage response)
        {
            try
            {
                response.EndToEndIdentifier = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                return true;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read end-to-end value from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertHopByHopFromBytes(BinaryReader reader, DiameterMessage response)
        {
            try
            {
                response.HopByHopIdentifier = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                return true;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read hop-by-hop value from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertApplicationIdFromBytes(BinaryReader reader, DiameterMessage response)
        {
            try
            {
                response.ApplicationId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                return true;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read application id value from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertCommandCodeFromBytes(BinaryReader reader, int byteCount, DiameterMessage response)
        {
            try
            {
                byte[] commandCodeBytes = reader.ReadBytes(byteCount);
                response.CommandCode = ConvertBytesToInt(commandCodeBytes);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read command code value from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertMessageLengthFromBytes(BinaryReader reader, int byteCount, DiameterMessage response)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Reading message length bytes");
            try
            {
                byte[] messageLengthBytes = reader.ReadBytes(byteCount);
                response.MessageLength = ConvertBytesToInt(messageLengthBytes);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read message length value from response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertProtocolVersionFromByte(BinaryReader binaryReader, DiameterMessage response)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Reading protocl version byte");
            try
            {
                //Get the protocol Version
                response.ProtocolVersion = binaryReader.ReadByte();
                return ValidateProtocolVersion(response.ProtocolVersion);
            }
            catch (EndOfStreamException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Reached end of the stream while trying to read for the protocol version: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            catch (ObjectDisposedException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Reader has been disposed: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"unexpected error occured: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return false;
        }
        private bool ValidateProtocolVersion(int protocolVersion)
        {
            return protocolVersion == 1;
        }
        private void SetResponseCommandFlags(byte commandFlagsByte, DiameterMessage response)
        {
            response.IsRequestBitSet = (commandFlagsByte & 1 << 7) != 0;
            response.IsProxiableBitSet = (commandFlagsByte & 1 << 6) != 0;
            response.IsErrorBitSet = (commandFlagsByte & 1 << 5) != 0;
            response.IsRetransmittedBitSet = (commandFlagsByte & 1 << 4) != 0;
        }
        private ObservableList<DiameterAVP> ProcessMessageResponseAvpList(byte[] avpBytes, ActDiameter act)
        {
            ObservableList<DiameterAVP> avps = new ObservableList<DiameterAVP>();
            using (MemoryStream stream = new MemoryStream(avpBytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < avpBytes.Length)
                {
                    try
                    {
                        DiameterAVP avp = ProcessDiameterAVPFromBytes(reader, act);
                        if (avp != null)
                        {
                            avps.Add(avp);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to process response avp list {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return null;
                    }
                }
            }
            return avps;
        }
        private DiameterAVP ProcessDiameterAVPFromBytes(BinaryReader binaryReader, ActDiameter act)
        {
            try
            {
                int avpHeaderLength = 8;
                DiameterAVP avp = new DiameterAVP();
                if (!ConvertAvpCodeFromBytes(binaryReader, avp))
                {
                    return null;
                }

                if (!ConvertAvpFlagsFromByte(binaryReader, avp))
                {
                    return null;
                }

                if (!ConvertAvpLengthFromBytes(binaryReader, avp))
                {
                    return null;
                }

                if (avp.IsVendorSpecific)
                {
                    if (!ConvertAvpVendorIdFromBytes(binaryReader, avp))
                    {
                        return null;
                    }
                    avpHeaderLength = 12;
                }

                int dataLength = avp.Length - avpHeaderLength;

                var avpInfo = FetchAvpInfoFromDictionary(avp.Code, act);
                if (avpInfo == null)
                {
                    UpdateActionError(act, $"Failed to find AVP with code: {avp.Code} in action's Request/Response list or in the avp dictionary file");
                    return null;
                }
                avp.DataType = avpInfo.DataType;
                avp.Name = avpInfo.Name;

                avp.Value = ConvertAvpValueFromBytes(binaryReader, avp.DataType, dataLength, act, avp);
                avp.ValueForDriver = !string.IsNullOrEmpty(avp.Value) ? avp.Value.ToString() : string.Empty;

                if (string.IsNullOrEmpty(avp.Value) && avp.DataType != eDiameterAvpDataType.Grouped)
                {
                    UpdateActionError(act, $"Failed to get the value from the response for avp {avp.Name}");
                    return null;
                }

                int padding = CalculatePadding(dataLength);
                if (padding > 0)
                {
                    binaryReader.ReadBytes(padding);
                }

                return avp;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to process the avp from the response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertAvpValueFromBytes(BinaryReader binaryReader, eDiameterAvpDataType dataType, int dataLength, ActDiameter act, DiameterAVP avp)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Converting value bytes of type: {dataType} to actual value");
            try
            {
                string avpValue = null;
                switch (dataType)
                {
                    case eDiameterAvpDataType.Address:
                        {
                            avpValue = ConvertAddressBytesToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.OctetString:
                        {
                            avpValue = ConvertOctetStringToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.UTF8String:
                    case eDiameterAvpDataType.DiamIdent:
                        {
                            avpValue = ConvertDiamIdentUTF8StringToValue(binaryReader, dataLength, dataType);
                            break;
                        }
                    case eDiameterAvpDataType.Unsigned32:
                        {
                            avpValue = ConvertUnsigned32ToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.Integer8:
                    case eDiameterAvpDataType.Integer64:
                    case eDiameterAvpDataType.Unsigned64:
                        {
                            avpValue = ConvertInteger8Integer64Unsigned64ToValue(binaryReader, dataLength, dataType);
                            break;
                        }
                    case eDiameterAvpDataType.Integer32:
                        {
                            avpValue = ConvertInteger32ToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.Enumerated:
                        {
                            avpValue = ConvertEnumeratedToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.Time:
                        {
                            avpValue = ConvertTimeToValue(binaryReader, dataLength);
                            break;
                        }
                    case eDiameterAvpDataType.Grouped:
                        {
                            avpValue = ConvertGroupedToValue(binaryReader, dataLength, act, avp);
                            break;
                        }
                }
                return !string.IsNullOrEmpty(avpValue) ? avpValue : string.Empty;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert avp value bytes to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return string.Empty;
            }
        }
        private string ConvertGroupedToValue(BinaryReader binaryReader, int dataLength, ActDiameter act, DiameterAVP avp)
        {
            try
            {
                // Grouped AVPs doesn't have value
                byte[] data = binaryReader.ReadBytes(dataLength);
                if (data != null && data.Any())
                {
                    avp.NestedAvpList = ProcessChildrenAVP(data, act, avp.Name);
                    if (avp.NestedAvpList == null)
                    {
                        UpdateActionError(act, $"Failed to process children AVPs for avp = '{avp.Name}'");
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Grouped to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private ObservableList<DiameterAVP> ProcessChildrenAVP(byte[] childrenData, ActDiameter act, string name)
        {
            ObservableList<DiameterAVP> childrenAVPs = null;
            using (MemoryStream stream = new MemoryStream(childrenData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                childrenAVPs = new ObservableList<DiameterAVP>();
                while (stream.Position < childrenData.Length)
                {
                    try
                    {
                        DiameterAVP childAvp = ProcessDiameterAVPFromBytes(reader, act);
                        if (childAvp != null)
                        {
                            childrenAVPs.Add(childAvp);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to process children avp list for avp = '{name}' {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return null;
                    }
                }
            }
            return childrenAVPs;
        }
        private string ConvertTimeToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                const int startIndex = 0;

                byte[] data = binaryReader.ReadBytes(dataLength);

                // Get NTP timestamp from NTP Bytes ( 4 High Order Bytes from NTP Timestamp)
                DateTime baseDate = DateTime.MinValue; // 1900, 1, 1, 0, 0, 0, 0
                baseDate = DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
                uint seconds = TimeBytesToSeconds(ntpTime: data);
                DateTime value = baseDate.AddSeconds(seconds);

                return value.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Time to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private uint TimeBytesToSeconds(byte[] ntpTime)
        {
            decimal integerPart = 0;
            for (var i = 0; i <= 3; i++)
            {
                integerPart = 256 * integerPart + ntpTime[i];
            }
            uint milliseconds = (uint)integerPart * 1000;

            return milliseconds;
        }
        private string ConvertEnumeratedToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                const int startIndex = 0;

                byte[] data = binaryReader.ReadBytes(dataLength);
                int enumerated = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, startIndex));

                return enumerated.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Enumerated to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertInteger32ToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                const int startIndex = 0;

                byte[] data = binaryReader.ReadBytes(dataLength);
                int signedValue = BitConverter.ToInt32(data, startIndex);

                int value = IPAddress.NetworkToHostOrder(signedValue);

                return value.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Integer32 to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertInteger8Integer64Unsigned64ToValue(BinaryReader binaryReader, int dataLength, eDiameterAvpDataType dataType)
        {
            try
            {
                const int startIndex = 0;

                byte[] data = binaryReader.ReadBytes(dataLength);
                long unsigned64 = BitConverter.ToInt64(data, startIndex);

                long value = IPAddress.NetworkToHostOrder(unsigned64);

                return value.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: {dataType} to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertUnsigned32ToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                const int startIndex = 0;

                byte[] data = binaryReader.ReadBytes(dataLength);
                int unsigned32 = BitConverter.ToInt32(data, startIndex);

                int value = Math.Abs(IPAddress.NetworkToHostOrder(unsigned32));

                return value.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Unsigned32 to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertDiamIdentUTF8StringToValue(BinaryReader binaryReader, int dataLength, eDiameterAvpDataType dataType)
        {
            try
            {
                byte[] data = binaryReader.ReadBytes(dataLength);
                string diamIdent = Encoding.UTF8.GetString(data);

                return diamIdent;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: {dataType} to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertOctetStringToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                byte[] data = binaryReader.ReadBytes(dataLength);
                string octetString = Encoding.Default.GetString(data);

                return octetString;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: OctetString to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private string ConvertAddressBytesToValue(BinaryReader binaryReader, int dataLength)
        {
            try
            {
                const int addressFamilySize = 2;
                const int bytesCount = 4;

                short addressFamily = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                byte[] addressBytes = binaryReader.ReadBytes(dataLength - addressFamilySize);
                sbyte[] data = new sbyte[4];
                Buffer.BlockCopy(addressBytes, 0, data, 0, bytesCount);

                IPAddress avpValue = ConvertBytesToIPAddress(data);

                return avpValue.ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert value bytes of type: Address to actual value {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }
        private IPAddress ConvertBytesToIPAddress(sbyte[] data)
        {
            const int sourceOffset = 0;
            const int destinationOffset = 0;

            byte[] ipAddressBytes = new byte[data.Length];
            Buffer.BlockCopy(data, sourceOffset, ipAddressBytes, destinationOffset, data.Length);

            return new IPAddress(ipAddressBytes);
        }
        private DiameterAVP FetchAvpInfoFromDictionary(int avpCode, ActDiameter act)
        {
            DiameterAVP avpInfo = null;
            try
            {
                // First search in the user request avp list
                if (act.RequestAvpList != null)
                {
                    avpInfo = act.RequestAvpList.FirstOrDefault(avp => avp.Code == avpCode);
                    if (avpInfo != null)
                    {
                        return avpInfo;
                    }
                }

                // Search in the user custom response avp list
                if (act.CustomResponseAvpList != null)
                {
                    avpInfo = act.CustomResponseAvpList.FirstOrDefault(avp => avp.Code == avpCode);
                    if (avpInfo != null)
                    {
                        return avpInfo;
                    }
                }

                // Avp was not found on the request and in the custom response, fetch the avp from the dictionary
                avpInfo = AvpDictionaryList.FirstOrDefault(avp => avp.Code == avpCode);
                return avpInfo;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to fetch avp info for avp code = '{avpCode}' {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return avpInfo;
            }
        }
        private bool ConvertAvpVendorIdFromBytes(BinaryReader binaryReader, DiameterAVP avp)
        {
            try
            {
                avp.VendorId = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read avp vendor id {avp.VendorId} from the response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertAvpLengthFromBytes(BinaryReader binaryReader, DiameterAVP avp)
        {
            try
            {
                avp.Length = ConvertBytesToInt(binaryReader.ReadBytes(3));
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read avp length {avp.Length} from the response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private bool ConvertAvpFlagsFromByte(BinaryReader binaryReader, DiameterAVP avp)
        {
            try
            {
                byte avpFlags = binaryReader.ReadByte();
                if (avpFlags != 0)
                {
                    SetResponseAvpFlags(avp, avpFlags);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read avp flags from the response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private void SetResponseAvpFlags(DiameterAVP avp, byte avpFlags)
        {
            avp.IsVendorSpecific = (avpFlags & 1 << 7) != 0;
            avp.IsMandatory = (avpFlags & 1 << 6) != 0;
        }
        private bool ConvertAvpCodeFromBytes(BinaryReader reader, DiameterAVP avp)
        {
            try
            {
                avp.Code = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read avp code {avp.Code} from the response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private int ConvertBytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes[0] + (bytes[1] << 8) + (bytes[2] << 16);
        }
        private bool ConvertProtocolVersionToByte(MemoryStream memoryStream)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Starting to convert protocol version: {Message.ProtocolVersion} to byte");
                byte protocolVersion = (byte)Message.ProtocolVersion;
                //Write protocol version
                memoryStream.WriteByte(protocolVersion);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred when converting protocol version: {Message.ProtocolVersion} to byte. Error {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }
        private void UpdateActionError(ActDiameter act, string error)
        {
            act.Error += $"{error}{Environment.NewLine}";
        }
        public void SaveRequestToFile(bool isSaveRequest, string saveDirectory, ActDiameter act)
        {
            if (isSaveRequest)
            {
                RequestFileContent = CreateMessageRawRequestResponse(act, Message);
                string fullFilePath = SaveToFile("Request", RequestFileContent, saveDirectory, act);
                act.AddOrUpdateReturnParamActual("Saved Request Filename", Path.GetFileName(fullFilePath));
            }
        }
        public void SaveResponseToFile(bool isSaveResponse, string saveDirectory, ActDiameter act, DiameterMessage response)
        {
            if (isSaveResponse)
            {
                ResponseFileContent = CreateMessageRawRequestResponse(act, response);
                string fullFilePath = SaveToFile("Response", ResponseFileContent, saveDirectory, act);
                act.AddOrUpdateReturnParamActual("Saved Response Filename", Path.GetFileName(fullFilePath));
            }
        }
        public void ParseResponseToOutputParams(ActDiameter act, DiameterMessage response)
        {
            act.AddOrUpdateReturnParamActual($"Command Code: ", response.CommandCode.ToString());
            act.AddOrUpdateReturnParamActual($"Application Id: ", response.ApplicationId.ToString());
            act.AddOrUpdateReturnParamActual($"Hob-By-Hop: ", response.HopByHopIdentifier.ToString());
            act.AddOrUpdateReturnParamActual($"End-To-End: ", response.EndToEndIdentifier.ToString());

            act.AddOrUpdateReturnParamActual($"Request: ", response.IsRequestBitSet.ToString());
            act.AddOrUpdateReturnParamActual($"Proxiable: ", response.IsProxiableBitSet.ToString());
            act.AddOrUpdateReturnParamActual($"Error: ", response.IsErrorBitSet.ToString());
            act.AddOrUpdateReturnParamActual($"Retransmit: ", response.IsRetransmittedBitSet.ToString());

            //AVPs
            foreach (var avp in response.AvpList)
            {
                ParseResponseAvpToOutputParams(act, avp);
            }

            AddRawResponseAndRequestToOutputParams(act);
        }
        private void AddRawResponseAndRequestToOutputParams(ActDiameter act)
        {
            act.RawResponseValues = ">>>>>>>>>>>>>>>>>>>>>>>>>>> REQUEST:" + Environment.NewLine + Environment.NewLine + RequestFileContent;
            act.RawResponseValues += Environment.NewLine + Environment.NewLine;
            act.RawResponseValues += ">>>>>>>>>>>>>>>>>>>>>>>>>>> RESPONSE:" + Environment.NewLine + Environment.NewLine + ResponseFileContent;
            act.AddOrUpdateReturnParamActual("Raw Request: ", RequestFileContent);
            act.AddOrUpdateReturnParamActual("Raw Response: ", ResponseFileContent);
        }
        private void ParseResponseAvpToOutputParams(ActDiameter act, DiameterAVP avp)
        {
            act.AddOrUpdateReturnParamActual($"AVP {nameof(avp.Name)}: ", avp.Name);
            act.AddOrUpdateReturnParamActual($"AVP {nameof(avp.Value)}: ", avp.Value);
            if (avp.DataType == eDiameterAvpDataType.Grouped)
            {
                if (avp.NestedAvpList != null)
                {
                    foreach (var childAVP in avp.NestedAvpList)
                    {
                        ParseResponseAvpToOutputParams(act, childAVP);
                    }
                }
            }

        }
        private string SaveToFile(string fileType, string fileContent, string saveDirectory, ActDiameter act)
        {
            try
            {
                string fileExtention = "txt";

                string directoryFullPath = Path.Combine(saveDirectory.Replace("~//", WorkSpace.Instance.Solution.ContainingFolderFullPath), fileType + "s");
                if (!Directory.Exists(directoryFullPath))
                {
                    Directory.CreateDirectory(directoryFullPath);
                }

                string fullFileName = "";
                lock (fileLock)
                {
                    String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
                    string actionName = PathHelper.CleanInValidPathChars(act.Description);
                    fullFileName = Path.Combine(directoryFullPath, actionName + "_" + timeStamp + "_" + fileType + "." + fileExtention);
                    File.WriteAllText(fullFileName, fileContent);
                }

                return fullFileName;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Failed to save the {fileType} file to the path {saveDirectory} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return string.Empty;
            }
        }
        public bool SendRequest(ActDiameter act, TcpClient tcpClient, string tcpHostname, string tcpPort)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Starting to send message to {tcpHostname}:{tcpPort}");

                // Convert message to bytes
                byte[] messageBytesToSend = ConvertMessageToBytes();
                if (messageBytesToSend == null)
                {
                    UpdateActionError(act, $"Encountered an issue while attempting to process the message");
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert message: {Message.Name} to bytes");
                    return false;
                }

                // check if TCP client is connected
                if (!tcpClient.Connected)
                {
                    UpdateActionError(act, $"Tcp client is not connected to: {tcpHostname}:{tcpPort}");
                    return false;
                }

                // Set a timeout
                int timeoutMilliseconds = 0;
                if (!string.IsNullOrEmpty(Convert.ToString(act.Timeout)))
                {
                    timeoutMilliseconds = ((int)act.Timeout) * 1000;
                    tcpClient.ReceiveTimeout = timeoutMilliseconds;
                }

                _ = tcpClient.Client.SendAsync(messageBytesToSend).Result;

                var responseDataBytes = new byte[1024];

                var received = tcpClient.Client.ReceiveAsync(responseDataBytes).Result;

                // Process the response
                Response = ProcessDiameterResponse(responseDataBytes, act);

                bool isResponseValid = ValidateResponse(act);
                if (!isResponseValid)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Invalid response - error: '{act.Error}'");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error in SendRequest: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        private bool ValidateResponse(ActDiameter act)
        {
            if (Response == null)
            {
                UpdateActionError(act, $"Error occurred trying to process the response");
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to process response from server");
                return false;
            }

            if (Response.CommandCode != Message.CommandCode)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Received response with different command code - '{Response.CommandCode}' than expected: {Message.CommandCode}");
                return false;
            }
            return true;
        }
    }
}
