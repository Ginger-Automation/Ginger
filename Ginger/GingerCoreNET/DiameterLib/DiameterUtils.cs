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
using Microsoft.Azure.Cosmos.Core.Collections;
using System.Net.Sockets;
using System.Net;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;
using System.Threading.Tasks;
using System.Collections;
using Org.BouncyCastle.Utilities;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterUtils
    {
        private const string DIAMETER_AVP_DICTIONARY_FILENAME = "AVPDictionary.xml";

        private static ObservableList<DiameterAVP> mAvpDictionaryList;
        private static readonly object dictionaryLock = new object();
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
        private DiameterMessage mMessage = null;
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
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"AVPs dictionary file '{DIAMETER_AVP_DICTIONARY_FILENAME}' not found. Issue: {ex.Message}\nStack: {ex.StackTrace}");
            }
            catch (XmlException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to read AVPs dictionary from file '{DIAMETER_AVP_DICTIONARY_FILENAME}'. Issue: {ex.Message}\nStack: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while loading AVPs dictionary. Issue: {ex.Message}\nStack: {ex.StackTrace}");
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
                    System.Collections.Generic.List<DiameterAVP> avps = AvpDictionaryList.Where(avp => avpsNamesCER.Contains(avp.Name)).ToList();
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
                    string requestMessage = CreateMessageRawResponse(act, diameterUtils.Message);
                    return requestMessage;
                }
                else
                {
                    return String.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Diameter Message Request preview content", ex);
                return string.Empty;
            }
        }

        private static string CreateMessageRawResponse(ActDiameter act, DiameterMessage message)
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
                + ">\r\n");
            foreach (DiameterAVP avp in message.AvpList)
            {
                if (avp.IsGrouped)
                {

                }
                else
                {
                    stringBuilder.Append($"\t<avp name=\"{avp.Name}\" mandatory=\"{avp.IsMandatory.ToString().ToLower()}\" value=\"{avp.ValueForDriver}\" </avp>\r\n");
                }
            }
            stringBuilder.Append("</Diameter Message>");
            return stringBuilder.ToString();
        }

        public static void AddAvpToMessage(DiameterAVP diameterAvp, ref DiameterMessage message)
        {
            if (diameterAvp != null)
            {
                message.AvpList.Add(diameterAvp);
            }
        }

        public bool ConstructDiameterRequest(ActDiameter act)
        {
            try
            {
                string[] messagePropertyNames = new string[]
                {
                    nameof(DiameterMessage.IsRequestBitSet),
                    nameof(DiameterMessage.IsProxiableBitSet),
                    nameof(DiameterMessage.CommandCode),
                    nameof(DiameterMessage.ApplicationId),
                    nameof(DiameterMessage.HopByHopIdentifier),
                    nameof(DiameterMessage.EndToEndIdentifier)
                };

                foreach (string property in messagePropertyNames)
                {
                    if (!SetMessageProperty(act, property))
                    {
                        HandleSetMessagePropertyError(act, property);
                        return false;
                    }
                }

                if (!SetMessageAvps(act))
                {
                    HandleSetMessageAvpsError(act);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to construct the diameter message\nerror message: '{0}'", ex.Message));
                return false;
            }
        }
        private void HandleSetMessagePropertyError(ActDiameter act, string property)
        {
            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to construct the diameter message on property '{0}'", property));
            act.Error = $"An error occurred while constructing the diameter message for the {property} property.";
        }
        private void HandleSetMessageAvpsError(ActDiameter act)
        {
            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to construct the diameter message on AVPs"));
            act.Error = $"An error occurred while adding AVPs to the diameter message.";
        }
        private bool SetMessageProperty(ActDiameter act, string property)
        {
            try
            {
                PropertyInfo propertyInfo = typeof(DiameterMessage).GetProperty(property);

                if (propertyInfo == null)
                {
                    HandlePropertyNotFound(act, property);
                    return false;
                }

                bool isSuccessfullyParsed = false;

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
                    act.Error = $"Failed to set {property} value";
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
            act.Error = $"Property {property} not found";
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
            Message.AvpList = act.RequestAvpList;
            return true;
        }
        public async Task<DiameterMessage> SendMessageAsync(ActDiameter act, TcpClient tcpClient)
        {
            try
            {
                byte[] messageBytesToSend = ConvertMessageToBytes();
                if (messageBytesToSend == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert message: {act.DiameterMessageType} in {act.ToString()} to bytes");
                    return null;
                }

                using (NetworkStream networkStream = tcpClient.GetStream())
                {
                    StateObject state = new StateObject()
                    {
                        TcpClient = tcpClient,
                        ReceivedBytes = new byte[1024],
                        DiameterAction = act,
                        // Create a TaskCompletionSource to await the response
                        TaskCompletionSource = new TaskCompletionSource<DiameterMessage>()
                    };

                    networkStream.Write(messageBytesToSend, 0, messageBytesToSend.Length);

                    // Begin the asynchronous receive operation.
                    tcpClient.Client.BeginReceive(state.ReceivedBytes, 0, state.ReceivedBytes.Length, 0, new AsyncCallback(ReceiveCallback), state);

                    return await state.TaskCompletionSource.Task;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error in SendMessageAsync: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (StateObject)result.AsyncState;
            Socket client = state.TcpClient.Client;

            try
            {
                int bytesRead = client.EndReceive(result);
                if (bytesRead > 0)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Received response successfully. bytes read: {bytesRead}");

                    // Increase buffer if bytes received from server is bigger than buffer
                    if (bytesRead > state.ReceivedBytes.Length)
                    {
                        byte[] tempBuffer = new byte[bytesRead];
                        Array.Copy(state.ReceivedBytes, 0, tempBuffer, 0, bytesRead);
                        state.ReceivedBytes = tempBuffer;
                    }

                    DiameterMessage response = ProcessDiameterResponse(state.ReceivedBytes, state.DiameterAction);

                    state.TaskCompletionSource.SetResult(response);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to process the response");
                    state.TaskCompletionSource.SetResult(null);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error in ReceiveCallback: {ex.Message}\n{ex.StackTrace}");
                // Set the exception to the TaskCompletionSource if an error occurs.
                state.TaskCompletionSource.SetException(ex);
            }
        }
        private byte[] ConvertMessageToBytes()
        {
            try
            {
                const int messageLengthOffset = 1;
                const int applicationIdOffset = 8;
                const int hopByHopIdentifierOffset = 12;
                const int endToEndIdentifierOffset = 16;

                using (MemoryStream stream = new MemoryStream())
                {
                    byte protocolVersion = (byte)Message.ProtocolVersion;
                    //Write protocol version
                    stream.WriteByte(protocolVersion);

                    //Reserve space for message length
                    stream.Write(BitConverter.GetBytes(0), 1, 3);

                    //Write command flags
                    byte commandFlags = GetCommandFlags(Message);
                    stream.WriteByte(commandFlags);

                    // Write command code
                    stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Message.CommandCode)), 1, 3);

                    // application Id, hop-by-hop and end-to-end identifiers
                    WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(Message.ApplicationId), applicationIdOffset);
                    WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(Message.HopByHopIdentifier), hopByHopIdentifierOffset);
                    WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(Message.EndToEndIdentifier), endToEndIdentifierOffset);

                    foreach (DiameterAVP avp in Message.AvpList)
                    {
                        byte[] avpAsBytes = ConvertAvpToBytes(avp);
                        if (avpAsBytes != null)
                        {
                            stream.Write(avpAsBytes, 0, avpAsBytes.Length);
                            Reporter.ToLog(eLogLevel.DEBUG, $"Converted AVP {avp.Name} to bytes successfully");
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert AVP {avp.Name} to bytes");
                            return null;
                        }
                    }

                    int messageLength = (int)stream.Length;
                    stream.Seek(messageLengthOffset, SeekOrigin.Begin);
                    stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageLength)), 1, 3);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting message object into bytes\nError message: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
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
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(bytes, 0, bytes.Length);
        }

        private byte[] ConvertAvpToBytes(DiameterAVP avp)
        {
            try
            {
                const int avpCodeOffset = 0;
                const int vendorIdOffset = 8;
                const int avpLengthOffset = 5;
                int avpValueOffset = 8;
                int padding = 0;

                using (MemoryStream stream = new MemoryStream())
                {
                    // Write Avp Code
                    WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(avp.Code), avpCodeOffset);

                    // Write Avp Flags
                    byte avpFlags = GetAvpFlags(avp);
                    stream.WriteByte(avpFlags);

                    // Reserve space for AVP length
                    stream.Write(BitConverter.GetBytes(0), 1, 3);

                    if (avp.IsVendorSpecific)
                    {
                        WriteInt32ToStream(stream, IPAddress.HostToNetworkOrder(avp.VendorId), vendorIdOffset);
                        avpValueOffset = 12;
                    }

                    // Add Avp Value
                    byte[] avpValueAsBytes = GetAvpValueAsBytes(avp.ValueForDriver, avp.DataType, ref padding);

                    // Get the AVP Length, Discard 0th Byte and Always excluding the paddings from length 
                    avp.Length = (avpValueAsBytes.Length + (int)stream.Length) - padding;

                    // Write Avp Length
                    stream.Seek(avpLengthOffset, SeekOrigin.Begin);
                    stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avp.Length)), 1, 3);

                    // Add Value Bytes
                    stream.Seek(avpValueOffset, SeekOrigin.Begin);
                    stream.Write(avpValueAsBytes, 0, avpValueAsBytes.Length);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Avps into bytes\nError message: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
            }
        }

        private byte[] GetAvpValueAsBytes(string valueForDriver, eDiameterAvpDataType dataType, ref int padding)
        {
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
                            valueAsBytes = ConvertGroupedToBytes();
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
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to process Avps for sending the request {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        private byte[] ConvertGroupedToBytes()
        {
            throw new NotImplementedException();
        }

        private byte[] ConvertTimeToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Time Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertEnumeratedToBytes(string avpValue, ref int padding)
        {
            try
            {
                int avpValueAsInt = Convert.ToInt32(avpValue);
                byte[] enumeratedBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)avpValueAsInt));

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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Enumerated Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertUTF8StringToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting UTF8String Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertInteger32ToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Integer32 Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertUnsigned64ToBytes(string valueForDriver, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Unsgined64 Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertUnsigned32ToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting Unsgined32 Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertDiamIdentToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting DiamIdent Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertOctetStringToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting OctetString Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private byte[] ConvertIpAddressToBytes(string avpValue, ref int padding)
        {
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
                Reporter.ToLog(eLogLevel.ERROR, $"Error converting IPAddress Avp value to bytes {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static int CalculatePadding(int bytesArrayLength)
        {
            int remainder = bytesArrayLength % 4;
            return remainder == 0 ? 0 : 4 - remainder;
        }

        private static void AddPaddingBytesToAvpValue(int padding, byte[] valueAsBytes)
        {
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
            Buffer.BlockCopy(source, 0, destination, destinationOffset, count);
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
            DiameterMessage responseMessage = null;
            try
            {
                responseMessage = new DiameterMessage();
                using (MemoryStream stream = new MemoryStream(receivedBytes))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    //Get the protocol Version
                    responseMessage.ProtocolVersion = reader.ReadByte();
                    ValidateProtocolVersion(responseMessage.ProtocolVersion);

                    // Get message length
                    byte[] messageLengthBytes = reader.ReadBytes(3);
                    responseMessage.MessageLength = ConvertMessageBytesToInt(messageLengthBytes);
                    // Get Command Flags
                    byte commandFlagByte = reader.ReadByte();
                    //SetResponseCommandFlags(commandFlagByte);
                    //byte[] commandFlagByteArray = new byte[] { commandFlagByte };
                    //BitArray commandFlags = new BitArray(commandFlagByteArray);
                    // Get Command Code
                    byte[] commandCodeBytes = reader.ReadBytes(3);
                    responseMessage.CommandCode = ConvertMessageBytesToInt(commandCodeBytes);
                    // Get Application Id, Hop-By-Hop and End-To-End Identifier
                    responseMessage.ApplicationId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                    responseMessage.HopByHopIdentifier = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                    responseMessage.EndToEndIdentifier = IPAddress.NetworkToHostOrder(reader.ReadInt32());

                    // Get the AVPs
                    int headerLength = 20;
                    if (stream.Length - stream.Position < responseMessage.MessageLength - headerLength)
                    {
                        throw new Exception("Insufficient data for AVPs");
                    }
                    byte[] avpBytes = reader.ReadBytes(responseMessage.MessageLength - headerLength);

                    responseMessage.AvpList = ProcessMessageResponseAvpList(avpBytes, receivedBytes);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error processing Diameter response + {ex.Message}\n{ex.StackTrace}");
            }
            return responseMessage;
        }

        private void ValidateProtocolVersion(int protocolVersion)
        {
            if (protocolVersion != 1)
            {
                throw new Exception($"Diameter protocol only support version 1");
            }
        }
        private void SetResponseCommandFlags(byte commandFlagsByte)
        {
            BitArray commandFlags = new BitArray(new byte[] { commandFlagsByte });
        }
        private ObservableList<DiameterAVP> ProcessMessageResponseAvpList(byte[] avpBytes, byte[] receivedBytes)
        {
            ObservableList<DiameterAVP> avps = new ObservableList<DiameterAVP>();
            using (MemoryStream stream = new MemoryStream(avpBytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {

            }
            return null;
        }

        private int ConvertMessageBytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes[0] + (bytes[1] << 8) + (bytes[2] << 16);
        }
        private class StateObject
        {
            public TcpClient TcpClient { get; set; }
            public byte[] ReceivedBytes { get; set; }
            public ActDiameter DiameterAction { get; set; }
            // TaskCompletionSource property to store the result of the async operation.
            public TaskCompletionSource<DiameterMessage> TaskCompletionSource { get; set; }
        }
    }
}
