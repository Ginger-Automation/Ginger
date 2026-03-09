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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.IO;
using DocumentFormat.OpenXml;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterUtils
    {
        private const string DIAMETER_LIB_FOLDER_NAME = "DiameterLib";
        private const string AVPS_PER_MESSAGE_CONFIGURATION_FILENAME = "AvpsPerMessageConfiguration.json";
        private const string DIAMETER_AVP_DICTIONARY_FILENAME = "AVPDictionary.xml";
        private const string DIAMETER_AVP_ENUMS_NAMESPACE = "Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums+";
        private static readonly Lazy<ObservableList<DiameterAvpDictionaryItem>> lazyAvpDictionaryList = new(() => LoadAVPDictionary());
        public static ObservableList<DiameterAvpDictionaryItem> AvpDictionaryList => lazyAvpDictionaryList.Value;

        private static Dictionary<string, string[]> mAvpsPerMessageDictionary;
        public static Dictionary<string, string[]> AvpsPerMessageDictionary
        {
            get
            {
                if (mAvpsPerMessageDictionary == null || !mAvpsPerMessageDictionary.Any())
                {
                    mAvpsPerMessageDictionary = DeserializeAvpsPerMessageConfigFile();
                }
                return mAvpsPerMessageDictionary;
            }
        }

        private static readonly object fileLock = new object();

        private TcpClient mTcpClient;
        private ActDiameter mAct;
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

        public static ObservableList<DiameterAvpDictionaryItem> LoadAVPDictionary(string xmlPath = "")
        {
            ObservableList<DiameterAvpDictionaryItem> avpListDictionary = null;
            try
            {
                string resourcePath = !string.IsNullOrEmpty(xmlPath)
                    ? xmlPath : Path.Combine(Path.GetDirectoryName(typeof(DiameterUtils).Assembly.Location), DIAMETER_LIB_FOLDER_NAME, DIAMETER_AVP_DICTIONARY_FILENAME);

                using (FileStream fs = new FileStream(resourcePath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiameterAvpDictionary));
                    DiameterAvpDictionary avpList = (DiameterAvpDictionary)xmlSerializer.Deserialize(fs);

                    if (avpList != null && avpList.AvpDictionaryList != null)
                    {
                        avpListDictionary = new ObservableList<DiameterAvpDictionaryItem>(avpList.AvpDictionaryList);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"AVPs dictionary file '{DIAMETER_AVP_DICTIONARY_FILENAME}' not found. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is XmlException xmlEx)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to read AVPs dictionary from file '{DIAMETER_AVP_DICTIONARY_FILENAME}'. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to deserialize AVPs dictionary from file '{DIAMETER_AVP_DICTIONARY_FILENAME}'. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while loading AVPs dictionary. Issue: {ex.Message}{Environment.NewLine}Stack: {ex.StackTrace}");
            }

            return avpListDictionary;
        }
        public static ObservableList<DiameterAVP> GetMandatoryAVPForMessage(eDiameterMessageType messageType, string configurationPath = "")
        {
            Dictionary<string, string[]> messageAVPsDictionary = !string.IsNullOrEmpty(configurationPath)
                ? DeserializeAvpsPerMessageConfigFile(configurationPath) : AvpsPerMessageDictionary;

            string messageNameKey = GetMessageNameFromType(messageType);
            string[] avpsNameArray = GetAVPNamesForMessage(messageAVPsDictionary, messageNameKey);

            ObservableList<DiameterAVP> avpList = GetAvpsForMessage(avpsNameArray);

            return avpList;
        }

        private static string[] GetAVPNamesForMessage(Dictionary<string, string[]> messageAVPsDictionary, string messageNameKey)
        {
            if (messageAVPsDictionary != null && messageAVPsDictionary.ContainsKey(messageNameKey))
            {
                return messageAVPsDictionary[messageNameKey];
            }
            return new string[0];
        }

        private static string GetMessageNameFromType(eDiameterMessageType messageType)
        {
            return messageType switch
            {
                eDiameterMessageType.CapabilitiesExchangeRequest or eDiameterMessageType.CreditControlRequest => General.GetEnumValueDescription(typeof(eDiameterMessageType), messageType),
                _ => "",
            };
        }

        public static Dictionary<string, string[]> DeserializeAvpsPerMessageConfigFile(string configurationPath = "")
        {
            Dictionary<string, string[]> avpsPerMessageDictionary = [];
            string avpNamesConfiguaritonPath = string.IsNullOrEmpty(configurationPath) ? Path.Combine(Path.GetDirectoryName(typeof(DiameterUtils).Assembly.Location), DIAMETER_LIB_FOLDER_NAME, AVPS_PER_MESSAGE_CONFIGURATION_FILENAME) : configurationPath;

            try
            {
                using (FileStream fs = new FileStream(avpNamesConfiguaritonPath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string configContent = reader.ReadToEnd();
                        avpsPerMessageDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(configContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error occurred while deserializing the AVPs per message configuartion file {ex.Message}{ex.StackTrace}");
            }

            return avpsPerMessageDictionary;
        }

        private static ObservableList<DiameterAVP> GetAvpsForMessage(string[] avpsNames)
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<DiameterAutoMapperProfile>());
            var mapper = mapperConfig.CreateMapper();

            List<DiameterAvpDictionaryItem> dictionaryItems = AvpDictionaryList.Where(avp => avpsNames.Contains(avp.Name)).ToList();
            ObservableList<DiameterAVP> avps = mapper.Map<ObservableList<DiameterAVP>>(dictionaryItems);

            return avps != null && avps.Any() ? avps : [];
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
            if (message == null)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.Append("<Diameter Message ::= <" + General.GetEnumValueDescription(typeof(eDiameterMessageType), act.DiameterMessageType)
                + $", code=\"{message.CommandCode}\""
                + $", request=\"{message.IsRequestBitSet.ToString().ToLower()}\""
                + $", proxiable=\"{message.IsProxiableBitSet.ToString().ToLower()}\""
                + $", error=\"{message.IsErrorBitSet.ToString().ToLower()}\""
                + $", retransmit=\"{message.IsRetransmittedBitSet.ToString().ToLower()}\""
                + $", hopbyhop=\"{message.HopByHopIdentifier}\""
                + $", endtoend=\"{message.EndToEndIdentifier}\""
                + $">{Environment.NewLine}");
            if (message.AvpList != null)
            {
                foreach (DiameterAVP avp in message.AvpList)
                {
                    stringBuilder.Append(CreateAVPAsString(avp) + Environment.NewLine);
                }
            }

            stringBuilder.Append("</Diameter Message>");
            return stringBuilder.ToString();
        }
        private static string CreateAVPAsString(DiameterAVP avp, int identLevel = 1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string avpString = "avp";
            string groupedAvp = "groupedavp";
            string identation = new string('\t', identLevel);
            if (avp != null)
            {
                string displayValue = avp.ValueForDriver;

                if (avp.DataType == eDiameterAvpDataType.Grouped)
                {
                    stringBuilder.Append($"{identation}<{groupedAvp} name=\"{avp.Name}\" mandatory=\"{avp.IsMandatory.ToString().ToLower()}\">");
                    if (avp.NestedAvpList != null && avp.NestedAvpList.Any())
                    {
                        foreach (DiameterAVP nestedAVP in avp.NestedAvpList)
                        {
                            stringBuilder.Append(Environment.NewLine + CreateAVPAsString(nestedAVP, identLevel + 1));
                        }
                        stringBuilder.Append($"{Environment.NewLine}{identation}</{groupedAvp}>");
                    }
                    else
                    {
                        stringBuilder.Append($" </{groupedAvp}>");
                    }
                }
                else
                {
                    if (avp.DataType == eDiameterAvpDataType.Enumerated)
                    {
                        var enumValue = TryGetAvpValueAsAvpEnumValue(avp);
                        displayValue = !string.IsNullOrEmpty(enumValue) ? enumValue : displayValue;
                    }

                    stringBuilder.Append($"{identation}<{avpString} name=\"{avp.Name}\" mandatory=\"{avp.IsMandatory.ToString().ToLower()}\" value=\"{displayValue}\" </{avpString}>");
                }
            }
            return stringBuilder.ToString();
        }

        private static string TryGetAvpValueAsAvpEnumValue(DiameterAVP avp)
        {
            try
            {
                string avpEnumTypeName = AvpDictionaryList.FirstOrDefault(x => x.Code == avp.Code)?.AvpEnumName;
                if (!string.IsNullOrEmpty(avpEnumTypeName))
                {
                    avpEnumTypeName = $"{DIAMETER_AVP_ENUMS_NAMESPACE}{avpEnumTypeName}";
                    Type valueEnumType = Type.GetType(avpEnumTypeName);

                    if (valueEnumType != null && valueEnumType.IsEnum)
                    {
                        bool isEnumValueSuccess = Enum.TryParse(valueEnumType, avp.ValueForDriver, true, out object valueAsEnum);
                        return isEnumValueSuccess ? General.GetEnumValueDescription(valueEnumType, valueAsEnum) : string.Empty;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error on TryGetValueAsEnumValue for Enumerated avp: {avp.Name} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return string.Empty;
            }
        }
        public bool ConstructDiameterRequest(ActDiameter act)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to construct the diameter request");
            mAct = act;
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
                    if (!SetMessageProperty(property))
                    {
                        UpdateActionError($"Failed to set {property} value");
                        return false;
                    }
                }

                Message.Name = General.GetEnumValueDescription(typeof(eDiameterMessageType), mAct.DiameterMessageType);

                Reporter.ToLog(eLogLevel.DEBUG, $"Setting Message's AVPs");
                if (!SetMessageAvps())
                {
                    HandleSetMessageAvpsError();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to construct the Diameter message {Message.Name}{Environment.NewLine}error message: '{ex.Message}{Environment.NewLine}{ex.StackTrace}'");
                return false;
            }
        }
        private void HandleSetMessageAvpsError()
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Failed to construct the Diameter message on AVPs");
            UpdateActionError($"An error occurred while adding request AVPs to the Diameter message.");
        }
        private bool SetMessageProperty(string property)
        {
            try
            {
                PropertyInfo propertyInfo = typeof(DiameterMessage).GetProperty(property);
                bool isSuccessfullyParsed = false;

                if (propertyInfo == null)
                {
                    HandlePropertyNotFound(property);
                    return false;
                }

                if (propertyInfo.PropertyType == typeof(int))
                {
                    isSuccessfullyParsed = TryParseAndSetValue<int>(propertyInfo, property);
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    isSuccessfullyParsed = TryParseAndSetValue<bool>(propertyInfo, property);
                }
                else
                {
                    HandleUnsupportedPropertyType(property);
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

        private void HandlePropertyNotFound(string property)
        {
            Reporter.ToLog(eLogLevel.ERROR, $"Property {property} not found in DiameterMessage.");
        }
        private bool TryParseAndSetValue<T>(PropertyInfo propertyInfo, string property)
        {
            if (TryParse<T>(mAct.GetInputParamCalculatedValue(property), out T value))
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
        private bool SetMessageAvps()
        {
            ClearChildrenFromRequestAVPs();

            if (mAct.RequestAvpList == null || !mAct.RequestAvpList.Any())
            {
                return false;
            }

            foreach (DiameterAVP avp in mAct.RequestAvpList)
            {
                if (avp != null)
                {
                    if (avp.ParentAvpGuid == Guid.Empty)
                    {
                        Message.AvpList.Add(avp);
                    }
                    else
                    {
                        AddAVPToParent(avp);
                    }
                }
            }

            return Message.AvpList != null && Message.AvpList.Any();
        }
        private void ClearChildrenFromRequestAVPs()
        {
            if (mAct.RequestAvpList != null)
            {
                foreach (DiameterAVP avp in mAct.RequestAvpList)
                {
                    if (avp.NestedAvpList != null && avp.NestedAvpList.Any())
                    {
                        avp.NestedAvpList.Clear();
                    }
                }
            }
        }
        private void AddAVPToParent(DiameterAVP childAvp)
        {
            DiameterAVP parentAVP = mAct.RequestAvpList.FirstOrDefault(avp => avp.Guid == childAvp.ParentAvpGuid);
            if (parentAVP != null && parentAVP.NestedAvpList != null)
            {
                parentAVP.NestedAvpList.Add(childAvp);
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
                        UpdateActionError($"Failed to process message protocol version: {Message.ProtocolVersion} for sending");
                        return null;
                    }

                    //Reserve space for message length
                    if (!ConvertMessageLengthToBytes(memoryStream, isReserve: true, messageLengthOffset))
                    {
                        UpdateActionError($"Failed to process message length for sending");
                        return null;
                    }

                    if (!ConvertMessageCommandFlagsToByte(memoryStream))
                    {
                        UpdateActionError($"Failed to process message command flags for sending");
                        return null;
                    }

                    if (!ConvertMessageCommandCodeToBytes(memoryStream, commandCodeOffset))
                    {
                        UpdateActionError($"Failed to process message command code: {Message.CommandCode} for sending");
                        return null;
                    }

                    // application Id, hop-by-hop and end-to-end identifiers
                    if (!ConvertMessageApplicationIdToBytes(memoryStream, applicationIdOffset))
                    {
                        UpdateActionError($"Failed to process message application id: {Message.ApplicationId} for sending");
                        return null;
                    }
                    if (!ConvertMessageHopByHopToBytes(memoryStream, hopByHopIdentifierOffset))
                    {
                        UpdateActionError($"Failed to process message hop-by-hop identifier: {Message.HopByHopIdentifier} for sending");
                        return null;
                    }
                    if (!ConvertMessageEndToEndToBytes(memoryStream, endToEndIdentifierOffset))
                    {
                        UpdateActionError($"Failed to process message end-to-end identifier: {Message.EndToEndIdentifier} for sending");
                        return null;
                    }

                    if (!ConvertMessageAvpListToBytes(memoryStream))
                    {
                        UpdateActionError($"Failed to process message AVPs list for sending");
                        return null;
                    }

                    SetMessageLength((int)memoryStream.Length);

                    // Write message length into memory stream with its actual value
                    if (!ConvertMessageLengthToBytes(memoryStream, isReserve: false, messageLengthOffset))
                    {
                        UpdateActionError($"Failed to process message length: {Message.MessageLength} for sending");
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
            byte[] avpAsBytes = null;
            Reporter.ToLog(eLogLevel.DEBUG, $"Starting to convert avp list into bytes");
            try
            {

                foreach (DiameterAVP avp in Message.AvpList)
                {
                    avpAsBytes = ConvertAvpToBytes(avp);
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
            finally
            {
                Array.Clear(avpAsBytes);
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
            byte[] bytes = System.BitConverter.GetBytes(value);
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Writing value: {value} to memory stream");

                WriteBytesToStream(stream, data: bytes, seekPosition: offset);
            }
            catch (InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to write 4 bytes with value: {value} to memory stream. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }
            }
        }
        private void WriteThreeBytesToStream(MemoryStream stream, int value, int offset)
        {
            byte[] bytes = System.BitConverter.GetBytes(value);
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Writing value: {value} to memory stream");

                WriteBytesToStream(stream, data: bytes, seekPosition: offset, offsetInData: 1, byteCount: 3);
            }
            catch (InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to write 4 bytes with value: {value} to memory stream. Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }
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
            finally
            {
                Array.Clear(data);
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
                        UpdateActionError($"Failed to process AVP: {avp.Name} code: {avp.Code} for sending");
                        return null;
                    }

                    if (!ConvertAvpFlagsToByte(stream, avp))
                    {
                        UpdateActionError($"Failed to process AVP: {avp.Name} flags for sending");
                        return null;
                    }

                    // Reserve space for AVP length
                    if (!ConvertAvpLengthToBytes(stream, avpLengthOffset, isReserve: true))
                    {
                        UpdateActionError($"Failed to process AVP: {avp.Name} length for sending");
                        return null;
                    }

                    if (avp.IsVendorSpecific)
                    {
                        if (!ConvertAvpVendorIdToBytes(stream, avp.VendorId, vendorIdOffset))
                        {
                            UpdateActionError($"Failed to process AVP: {avp.Name} vendor id: {avp.VendorId} for sending");
                            return null;
                        }
                        avpValueOffset = 12;
                    }

                    // Grouped AVP doesn't have value
                    if (avp.DataType != eDiameterAvpDataType.Grouped && string.IsNullOrEmpty(avp.ValueForDriver))
                    {
                        UpdateActionError($"AVP: {avp.Name} value is missing");
                        return null;
                    }

                    byte[] avpValueAsBytes = GetAvpValueAsBytes(avp.ValueForDriver, avp.DataType, ref padding, stream, avp);
                    if (avpValueAsBytes == null)
                    {
                        UpdateActionError($"Failed to process AVP: {avp.Name} value for sending");
                        return null;
                    }

                    // Set the avp length
                    SetAvpLength(avp, padding, (int)stream.Length, avpValueAsBytes.Length);

                    // Write Avp Length
                    if (!ConvertAvpLengthToBytes(stream, avpLengthOffset, value: IPAddress.HostToNetworkOrder(avp.Length)))
                    {
                        UpdateActionError($"Failed to process AVP: {avp.Name} length: {avp.Length} for sending");
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
            List<byte> childrenAVPsBytesList = [];
            foreach (var childAvp in avp.NestedAvpList)
            {
                if (childAvp == null)
                {
                    UpdateActionError($"Failed to process child AVP for Parent AVP: {avp.Name}");
                    return null;
                }

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
                byte[] enumeratedBytes = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

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
                byte[] integer32Bytes = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

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

                byte[] U64Bytes = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValue));

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

                byte[] unsigned32Bytes = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(avpValueAsInt));

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
                byte[] addressFamilyBytes = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(addressFamily));
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
        private DiameterMessage ProcessDiameterResponse(byte[] receivedBytes)
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
                        UpdateActionError($"Diameter only supports Protocol Version 1, Protocol Version received from response was {response.ProtocolVersion}");
                        return null;
                    }

                    if (!ConvertMessageLengthFromBytes(reader, byteCount: 3, response))
                    {
                        UpdateActionError($"Failed to read message length from response: {response.MessageLength}");
                        return null;
                    }

                    // Get Command Flags
                    if (!ConvertCommandFlagsFromByte(reader, response))
                    {
                        UpdateActionError($"Failed to read command flags from response");
                        return null;
                    }

                    if (!ConvertCommandCodeFromBytes(reader, byteCount: 3, response))
                    {
                        UpdateActionError($"Failed to read command code from response: {response.CommandCode}");
                        return null;
                    }

                    if (!ConvertApplicationIdFromBytes(reader, response))
                    {
                        UpdateActionError($"Failed to read application id from response: {response.ApplicationId}");
                        return null;
                    }

                    if (!ConvertHopByHopFromBytes(reader, response))
                    {
                        UpdateActionError($"Failed to read hop-by-hop identifier from response: {response.HopByHopIdentifier}");
                        return null;
                    }
                    if (!ConvertEndToEndFromBytes(reader, response))
                    {
                        UpdateActionError($"Failed to read end-to-end identifier from response: {response.EndToEndIdentifier}");
                        return null;
                    }

                    // Get the AVPs
                    int headerLength = 20;
                    if (stream.Length - stream.Position < response.MessageLength - headerLength)
                    {
                        UpdateActionError($"Insufficient data to process the response AVPs");
                        return null;
                    }

                    if (!ReadResponseAvpBytes(reader, headerLength, response, out byte[] avpBytes))
                    {
                        if (avpBytes == null || !avpBytes.Any())
                        {
                            response.AvpList = null;
                        }

                        UpdateActionError($"failed to read the response AVP list bytes");
                        return null;
                    }

                    response.AvpList = ProcessMessageResponseAvpList(avpBytes);

                    if (response.AvpList == null || !response.AvpList.Any())
                    {
                        UpdateActionError($"Failed to read AVP List from response");
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
                Reporter.ToLog(eLogLevel.ERROR, $"unexpected error occurred: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
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
        private ObservableList<DiameterAVP> ProcessMessageResponseAvpList(byte[] avpBytes)
        {
            ObservableList<DiameterAVP> avps = [];
            using (MemoryStream stream = new MemoryStream(avpBytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < avpBytes.Length)
                {
                    try
                    {
                        DiameterAVP avp = ProcessDiameterAVPFromBytes(reader);
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
        private DiameterAVP ProcessDiameterAVPFromBytes(BinaryReader binaryReader)
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

                DiameterAvpDictionaryItem avpInfo = FetchAvpInfoFromDictionary(avp.Code);
                if (avpInfo == null)
                {
                    UpdateActionError($"Failed to find AVP with code: {avp.Code} in action's response AVP list or in the AVP dictionary file");
                    return null;
                }
                avp.DataType = avpInfo.AvpDataType;
                avp.Name = avpInfo.Name;

                avp.Value = ConvertAvpValueFromBytes(binaryReader, avp.DataType, dataLength, avp);
                avp.ValueForDriver = !string.IsNullOrEmpty(avp.Value) ? avp.Value.ToString() : string.Empty;

                if (string.IsNullOrEmpty(avp.Value) && avp.DataType != eDiameterAvpDataType.Grouped)
                {
                    UpdateActionError($"Failed to get the value from the response for avp {avp.Name}");
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
        private string ConvertAvpValueFromBytes(BinaryReader binaryReader, eDiameterAvpDataType dataType, int dataLength, DiameterAVP avp)
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
                            avpValue = ConvertGroupedToValue(binaryReader, dataLength, avp);
                            break;
                        }
                    default:
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Value bytes type: '{dataType}' is not supported");
                            UpdateActionError($"Value bytes type: '{dataType}' is not supported");
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
        private string ConvertGroupedToValue(BinaryReader binaryReader, int dataLength, DiameterAVP avp)
        {
            try
            {
                // Grouped AVPs doesn't have value
                byte[] data = binaryReader.ReadBytes(dataLength);
                if (data != null && data.Any())
                {
                    avp.NestedAvpList = ProcessChildrenAVP(data, avp.Name);
                    if (avp.NestedAvpList == null)
                    {
                        UpdateActionError($"Failed to process children AVPs for avp = '{avp.Name}'");
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
        private ObservableList<DiameterAVP> ProcessChildrenAVP(byte[] childrenData, string name)
        {
            ObservableList<DiameterAVP> childrenAVPs = null;
            using (MemoryStream stream = new MemoryStream(childrenData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                childrenAVPs = [];
                while (stream.Position < childrenData.Length)
                {
                    try
                    {
                        DiameterAVP childAvp = ProcessDiameterAVPFromBytes(reader);
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
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to process children AVPs list for AVP = '{name}' {ex.Message}{Environment.NewLine}{ex.StackTrace}");
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
                int enumerated = IPAddress.NetworkToHostOrder(System.BitConverter.ToInt32(data, startIndex));

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
                int signedValue = System.BitConverter.ToInt32(data, startIndex);

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
                long unsigned64 = System.BitConverter.ToInt64(data, startIndex);

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
                int unsigned32 = System.BitConverter.ToInt32(data, startIndex);

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
        private DiameterAvpDictionaryItem FetchAvpInfoFromDictionary(int avpCode)
        {
            DiameterAvpDictionaryItem avpInfo = null;
            try
            {
                // Search in the user custom response avp list
                var avp = mAct.CustomResponseAvpList?.FirstOrDefault(avp => avp.Code == avpCode);

                var mapperConfig = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<DiameterAutoMapperProfile>());
                var mapper = mapperConfig.CreateMapper();

                avpInfo = mapper.Map<DiameterAvpDictionaryItem>(avp);
                if (avpInfo != null)
                {
                    return avpInfo;
                }

                // Avp was not found in the custom response, fetch the avp from the dictionary
                avpInfo = AvpDictionaryList?.FirstOrDefault(avp => avp.Code == avpCode);
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
            if (System.BitConverter.IsLittleEndian)
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
        private void UpdateActionError(string error)
        {
            mAct.Error += $"{error}{Environment.NewLine}";
        }
        public void SaveRequestToFile(bool isSaveRequest, string saveDirectory)
        {
            if (isSaveRequest)
            {
                RequestFileContent = CreateMessageRawRequestResponse(mAct, Message);
                string fullFilePath = SaveToFile("Request", RequestFileContent, saveDirectory, mAct);
                mAct.AddOrUpdateReturnParamActual("Saved Request Filename", Path.GetFileName(fullFilePath));
            }
        }
        public void SaveResponseToFile(bool isSaveResponse, string saveDirectory)
        {
            if (isSaveResponse)
            {
                ResponseFileContent = CreateMessageRawRequestResponse(mAct, Response);
                string fullFilePath = SaveToFile("Response", ResponseFileContent, saveDirectory, mAct);
                mAct.AddOrUpdateReturnParamActual("Saved Response Filename", Path.GetFileName(fullFilePath));
            }
        }
        public void ParseResponseToOutputParams()
        {
            mAct.AddOrUpdateReturnParamActual($"Command Code: ", Response.CommandCode.ToString());
            mAct.AddOrUpdateReturnParamActual($"Application Id: ", Response.ApplicationId.ToString());
            mAct.AddOrUpdateReturnParamActual($"Hob-By-Hop: ", Response.HopByHopIdentifier.ToString());
            mAct.AddOrUpdateReturnParamActual($"End-To-End: ", Response.EndToEndIdentifier.ToString());

            mAct.AddOrUpdateReturnParamActual($"Request: ", Response.IsRequestBitSet.ToString());
            mAct.AddOrUpdateReturnParamActual($"Proxiable: ", Response.IsProxiableBitSet.ToString());
            mAct.AddOrUpdateReturnParamActual($"Error: ", Response.IsErrorBitSet.ToString());
            mAct.AddOrUpdateReturnParamActual($"Retransmit: ", Response.IsRetransmittedBitSet.ToString());

            //AVPs
            foreach (var avp in Response.AvpList)
            {
                ParseResponseAvpToOutputParams(avp);
            }

            AddRawResponseAndRequestToOutputParams();
        }
        private void AddRawResponseAndRequestToOutputParams()
        {
            mAct.RawResponseValues = ">>>>>>>>>>>>>>>>>>>>>>>>>>> REQUEST:" + Environment.NewLine + Environment.NewLine + RequestFileContent;
            mAct.RawResponseValues += Environment.NewLine + Environment.NewLine;
            mAct.RawResponseValues += ">>>>>>>>>>>>>>>>>>>>>>>>>>> RESPONSE:" + Environment.NewLine + Environment.NewLine + ResponseFileContent;
            mAct.AddOrUpdateReturnParamActual("Raw Request: ", RequestFileContent);
            mAct.AddOrUpdateReturnParamActual("Raw Response: ", ResponseFileContent);
        }
        private void ParseResponseAvpToOutputParams(DiameterAVP avp, string parentPath = "")
        {
            string paramName = $"{avp.Name}: ";
            int avpInstanceCount = mAct.GetReturnParamCount(paramName);

            string paramFullPath = string.IsNullOrEmpty(parentPath) ?
                $"AVPs/{avp.Name}[{avpInstanceCount + 1}]"
                : $"{parentPath}/{avp.Name}[{avpInstanceCount + 1}]";

            mAct.AddOrUpdateReturnParamActualWithPath(paramName, avp.Value, paramFullPath);

            if (avp.DataType == eDiameterAvpDataType.Grouped)
            {
                if (avp.NestedAvpList != null)
                {
                    foreach (var childAVP in avp.NestedAvpList)
                    {
                        ParseResponseAvpToOutputParams(childAVP, parentPath: paramFullPath);
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
        public bool SendRequest(string tcpHostname, string tcpPort)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Starting to send message to {tcpHostname}:{tcpPort}");

                // Convert message to bytes
                byte[] messageBytesToSend = ConvertMessageToBytes();
                if (messageBytesToSend == null)
                {
                    UpdateActionError($"Encountered an issue while attempting to process the message");
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to convert message: {Message.Name} to bytes");
                    return false;
                }

                // check if TCP client is connected
                if (!mTcpClient.Connected)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Tcp client is not connected to: {tcpHostname}:{tcpPort}");
                    if (!Reconnect(tcpHostname, tcpPort))
                    {
                        UpdateActionError($"Failed to establish connection to: {tcpHostname}:{tcpPort}");
                        return false;
                    }
                }

                // Set send and receive timeout
                SetTCPClientTimeout();

                bool isMessageSent = SendDiameterMessage(messageBytesToSend, tcpHostname, tcpPort);
                if (!isMessageSent)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to send Diameter message to: {tcpHostname}:{tcpPort}");
                    UpdateActionError($"Failed to send Diameter message to: {tcpHostname}:{tcpPort}");
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

        private bool Reconnect(string tcpHostname, string tcpPort)
        {
            try
            {
                // The socket has been closed, initialize a new tcp client
                mTcpClient.Dispose();

                mTcpClient = new TcpClient(tcpHostname, Convert.ToInt32(tcpPort));

                return mTcpClient.Connected;
            }
            catch (SocketException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error when trying to connect the TCP client to: {tcpHostname}:{tcpPort} - {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while reconnecting the TCP client to: {tcpHostname}:{tcpPort} - {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        private byte[] ReceiveDiameterResponse(string hostname, string port, int retryReceive = 0)
        {
            // TODO: have a dynamic buffer rather than a fixed one
            byte[] buffer = new byte[2048];
            try
            {
                int responseLength = mTcpClient.Client.ReceiveAsync(buffer).Result;

                if (responseLength == 0)
                {
                    return null;
                }

                byte[] responseDataBytes = new byte[responseLength];

                Array.Copy(buffer, responseDataBytes, responseLength);

                if (!ValidateResponseCommandCode(responseDataBytes) && retryReceive <= 1)
                {
                    responseDataBytes = ReceiveDiameterResponse(hostname, port, retryReceive + 1);
                }

                return responseDataBytes;
            }
            catch (AggregateException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"AggregateException occurred in ReceiveDiameterResponse {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                UpdateActionError($"The connection to {hostname}:{port} was lost");
                CloseTcpConnection();
                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error in receiving diameter response {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }

        private void CloseTcpConnection()
        {
            try
            {
                if (mTcpClient != null)
                {
                    mTcpClient.Close();
                    mTcpClient.Dispose();
                    mTcpClient = null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to close TCP connection gracefully {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private bool ValidateResponseCommandCode(byte[] responseDataBytes)
        {
            try
            {
                int commandCodeOffset = 5;
                int commandCodeBytesLength = 3;
                byte[] commandCodeBytes = new byte[commandCodeBytesLength];

                Array.Copy(responseDataBytes, commandCodeOffset, commandCodeBytes, 0, commandCodeBytesLength);

                int responseCommandCode = ConvertBytesToInt(commandCodeBytes);

                // Server sent a keep alive response or response received for a different message type, try to get the real response
                if (responseCommandCode == 280 || responseCommandCode != Message.CommandCode)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error occurred while validating the response command code {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }

        }

        private bool SendDiameterMessage(byte[] messageBytesToSend, string hostname, string port)
        {
            try
            {
                int bytesSent = mTcpClient.Client.SendAsync(messageBytesToSend).Result;
                return bytesSent > 0;
            }
            catch (AggregateException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                UpdateActionError($"The connection to {hostname}:{port} was lost");
                CloseTcpConnection();
                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while sending the request as bytes {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return false;
            }
        }

        private void SetTCPClientTimeout()
        {
            if (!string.IsNullOrEmpty(mAct.Timeout?.ToString()))
            {
                int timeoutMilliseconds = ((int)mAct.Timeout) * 1000;
                mTcpClient.SendTimeout = timeoutMilliseconds;
                mTcpClient.ReceiveTimeout = timeoutMilliseconds;
            }
        }

        private bool ValidateResponse()
        {
            if (Response == null)
            {
                UpdateActionError($"Error occurred trying to process the response");
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

        public bool ReceiveResponse(string hostname, string port)
        {
            byte[] responseDataBytes = ReceiveDiameterResponse(hostname, port);
            if (responseDataBytes == null)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to receive Diameter response from: {hostname}:{port}");
                UpdateActionError($"Failed to receive Diameter response from: {hostname}:{port}");
                return false;
            }

            Response = ProcessDiameterResponse(responseDataBytes);

            bool isResponseValid = ValidateResponse();
            if (!isResponseValid)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Invalid response - error: '{mAct.Error}'");
                return false;
            }

            return true;
        }

        public void SetTcpClient(ref TcpClient tcpClient)
        {
            if (tcpClient != null)
            {
                mTcpClient = tcpClient;
            }
        }
    }
}
