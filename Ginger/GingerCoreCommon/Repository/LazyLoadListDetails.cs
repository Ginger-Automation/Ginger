using System;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository
{
    public class LazyLoadListDetails
    {
        public LazyLoadListConfig Config { get; set; }

        public string XmlFilePath { get; set; }

        string mDataAsString = null;
        public string DataAsString
        {
            get
            {
                if (Config != null && Config.LazyLoadType == LazyLoadListConfig.eLazyLoadType.NodePath && mDataAsString == null)
                {
                    LoadXMLDataIntoString();
                }
                return mDataAsString;
            }
            set
            {
                mDataAsString = value;
            }
        }

        public bool DataWasLoaded = false;
        

        private void LoadXMLDataIntoString()
        {
            lock (this)
            {
                if (mDataAsString != null)
                {
                    return;
                }

                XmlReaderSettings xdrSettings = new XmlReaderSettings()
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    CloseInput = true
                };
                try
                {
                    using (XmlReader xdr = XmlReader.Create(XmlFilePath, xdrSettings))
                    {
                        xdr.MoveToContent();
                        if (xdr.Name != Config.ListName)
                        {
                            xdr.ReadToDescendant(Config.ListName);
                        }
                        mDataAsString = xdr.ReadOuterXml();
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to pull the XML data of the list '{0}' from file '{1}'", Config.ListName, XmlFilePath), ex);
                    mDataAsString = string.Empty;
                }
            }
        }
    }
}
