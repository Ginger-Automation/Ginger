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

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public static class DiameterUtils
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
                            //return mAvpDictionaryList;
                        }
                    }
                }
                return mAvpDictionaryList;
            }
        }

        public static ObservableList<DiameterAVP> LoadDictionary()
        {
            ObservableList<DiameterAVP> diameterAVPs = new ObservableList<DiameterAVP>();
            string resourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DiameterLib", DIAMETER_AVP_DICTIONARY_FILENAME);
            if (!String.IsNullOrEmpty(resourcePath))
            {
                try
                {
                    DataSet dataSet = new DataSet();
                    dataSet.ReadXml(resourcePath);
                    if (dataSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            if (row != null)
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
                                diameterAVPs.Add(avp);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to load avps dictionary from file '{0}', Issue:'{1}'", DIAMETER_AVP_DICTIONARY_FILENAME, ex.Message));
                }
            }
            return diameterAVPs;
        }

        public static ObservableList<ActDiameterAvp> GetMandatoryAVPForMessage(DiameterEnums.eDiameterMessageType messageType)
        {
            ObservableList<ActDiameterAvp> avpList = null;
            if (messageType == eDiameterMessageType.CapabilitiesExchangeRequest)
            {
                string[] avpsNamesCER = { "Origin-Host", "Origin-Realm", "Host-IP-Address", "Vendor-Id", "Product-Name", "Origin-State-Id" };
                System.Collections.Generic.List<DiameterAVP> avps = AvpDictionaryList.Where(avp => avpsNamesCER.Contains(avp.Name)).ToList();
                if (avps.Any())
                {
                    avpList = new ObservableList<ActDiameterAvp>();
                    foreach (DiameterAVP avp in avps)
                    {
                        avpList.Add(new ActDiameterAvp(avp));
                    }
                }
            }

            return avpList;
        }

        public static ObservableList<string> GetAvpNamesFromDictionary()
        {
            var avpNames = AvpDictionaryList.Select(avp => avp.Name).ToList();
            return new ObservableList<string>(avpNames);
        }

        public static string CreateMessageRawResponse(ActDiameter act)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Diameter Message ::= <" + General.GetEnumValueDescription(typeof(eDiameterMessageType), act.DiameterMessageType) 
                + $", code=\"{act.CommandCode}\"" 
                + $", request=\"{act.SetRequestBit}\"" 
                + $", proxiable=\"{act.SetProxiableBit}\"" 
                + $", error=\"{act.SetErrorBit}\"" 
                + $", retransmit=\"{act.SetRetransmitBit}\"" 
                + $", hopbyhop=\"{act.HopByHopIdentifier}\"" 
                + $", endtoend=\"{act.EndToEndIdentifier}\"" 
                + ">\r\n");
            foreach(ActDiameterAvp avp in act.RequestAvpList)
            {
                if (avp.IsGrouped)
                {

                }
                else
                {
                    stringBuilder.Append($"\t<avp name={avp.Name} mandatory={avp.IsMandatory} value={avp.Value} />\r\n");
                }
            }

            return stringBuilder.ToString();
        }

        public static string GetRawRequestContentPreview(ActDiameter act)
        {
            try
            {
                //Prepare act input values
                Context context = Context.GetAsContext(act.Context);
                if (context != null && context.Runner != null)
                {
                    context.Runner.PrepActionValueExpression(act, context.BusinessFlow);
                }
                //Create Request content
                SetValueFromValueExpression(act);
                string requestMessage = CreateMessageRawResponse(act);
                return requestMessage;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Diameter Message Request preview content", ex);
                return string.Empty;
            }
        }

        private static void SetValueFromValueExpression(ActDiameter act)
        {
            int commandCode = int.Parse(act.GetInputParamCalculatedValue(nameof(ActDiameter.CommandCode)));
        }

        public static void AddAvpToMessage(ActDiameterAvp actDiameterAvp, ref DiameterMessage message)
        {
            if (actDiameterAvp != null)
            {
                DiameterAVP avp = new DiameterAVP(actDiameterAvp);
                if (avp != null)
                {
                    message.AvpList.Add(avp);
                }
            }
        }
    }
}
