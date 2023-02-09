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

using Amdocs.Ginger.Common;
using GingerCore;
using System;
using System.Xml;

namespace Ginger.UserConfig
{
    public class UserTypeHelper
    {
        public bool IsSupportAutomate;
        public bool IsSupportExecution;
        public bool IsSupportReports;
        public bool IsSupportSourceControl;
        public bool IsSupportALM;
        public bool IsSupportSupport;
        public bool IsSupportAnalyzer;

        public void Init(eUserType UserType)
        {
            XmlDocument doc = new XmlDocument();
            string ConfigFileSourcePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\UserTypeConfiguration.XML";

            // for unit test we might not have this file
            if (System.IO.File.Exists(ConfigFileSourcePath))
            {
                doc.Load(ConfigFileSourcePath);
                XmlNamespaceManager manager = XMLDocExtended.GetAllNamespaces(doc);
                IsSupportAutomate = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='Automate']", manager).Attributes["Enable"].Value);
                IsSupportExecution = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='Execution']", manager).Attributes["Enable"].Value);
                IsSupportReports = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='Reports']", manager).Attributes["Enable"].Value);
                IsSupportSourceControl = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='SourceControl']", manager).Attributes["Enable"].Value);
                IsSupportALM = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='ALM']", manager).Attributes["Enable"].Value);
                IsSupportSupport = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='Support']", manager).Attributes["Enable"].Value);
                IsSupportAnalyzer = Boolean.Parse(doc.SelectSingleNode("//Ginger/UserType[@Name='" + UserType + "']/Functionality[@Name='Analyzer']", manager).Attributes["Enable"].Value);
            }
            else
            {
                IsSupportAutomate = true;
                IsSupportExecution = 
                IsSupportReports = true;
                IsSupportSourceControl = true;
                IsSupportALM = true;
                IsSupportSupport = true;
                IsSupportAnalyzer = true;
            }
        }
    }
}
