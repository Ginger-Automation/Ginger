using System.Data;
using System.Reflection;
using System;
using System.IO;
using Amdocs.Ginger.Common;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public static class DiameterUtils
    {
        private const string DIAMETER_AVP_DICTIONARY = "AVPDictionary.xml";
        public static ObservableList<DiameterAVP> AvpDictionaryList
        {
            get
            {
                if (AvpDictionaryList == null)
                {
                    return LoadDictionary();
                }
                else
                {
                    return AvpDictionaryList;
                }
            }
        }
        public static ObservableList<DiameterAVP> LoadDictionary()
        {
            ObservableList<DiameterAVP> diameterAVPs = new ObservableList<DiameterAVP>();
            string resourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DiameterLib", DIAMETER_AVP_DICTIONARY);
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
                                DiameterAVP avp = new DiameterAVP() {
                                    Name = row["name"].ToString(),
                                    Code = Convert.ToInt32(row["code"]),
                                    DataType = (eDiameterAvpDataType)Enum.Parse(typeof(eDiameterAvpDataType), row["type"].ToString())
                                };
                                diameterAVPs.Add(avp);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to load avps dictionary from file '{0}', Issue:'{1}'", DIAMETER_AVP_DICTIONARY, ex.Message));
                }
            }
            return diameterAVPs;
        }
    }
}
