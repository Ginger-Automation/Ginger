#region License
/*
Copyright © 2014-2023 European Support Limited

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
