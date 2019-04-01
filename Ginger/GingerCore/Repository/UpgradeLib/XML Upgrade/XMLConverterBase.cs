#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Repository;
using GingerCore.Repository;
using System.IO;
using System.Xml.Linq;

namespace GingerCore.XMLConverters
{
    public class XMLConverterBase
    {
        public string XmlFilePath { get; set; }
        public string OriginalXML { get; set; }
        public string UpdatedXML { get; set; }
        public eGingerFileType RepoType { get; set; }
        public XDocument xmlDoc{ get; set; } 

        public enum eGingerFileType
        {            
            Unknown = 0,
            Solution = 1,
            BusinessFlow = 2,
            Agent = 3,
            Environment = 4,
            RunSet = 5,
            Action = 6,
            Activity = 7,
            Validation = 8,
            Variable = 9,
            UserProfile = 10,
            ReportTemplate =11
        }

        public void Init(eGingerFileType RepoType, string xml, string xmlFilePath)
        {
            this.XmlFilePath = xmlFilePath;
            this.RepoType = RepoType;
            this.OriginalXML = xml;
            this.UpdatedXML = xml;

            this.xmlDoc = XDocument.Parse(xml);
        }

        public virtual void Convert()
        {
            //TODO: impl in subclass
        }

        public void UpdateXMLVersion(string newGingerVersion)
        {
            if (RepositorySerializer.IsLegacyXmlType(xmlDoc.ToString()) == true)
                RepositorySerializer.UpdateXMLGingerVersion(xmlDoc, newGingerVersion);
            else
                NewRepositorySerializer.UpdateXMLGingerVersion(xmlDoc, newGingerVersion);//New XML type
        }

        /// <summary>
        /// Get Ginger file type from filename of XML file being parsed.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static eGingerFileType GetGingerFileTypeFromFilename(string filename)
        {
            //TODO: merge this w GingerFileType as struct?
            #region define variables
            string sSolution = "Ginger.Solution.xml";
            string sUserProfile = "Ginger.UserProfile.xml";
            string sBF = "Ginger.BusinessFlow.xml";
            string sAgent = "Ginger.Agent.xml";
            string sEnv = "Ginger.Environment.xml";
            string sRunSet = "Ginger.RunSetConfig.xml";
            string sRepoAction = "Ginger.Action.xml";
            string sRepoActivity = "Ginger.Activity.xml";
            string sRepoVariable = "Ginger.Variable.xml";
            string sRepoortTempalte = "Ginger.RepoortTempalte.xml";            
            #endregion

            string p = Path.GetFileName(filename);

            eGingerFileType o;
            if (p == sSolution) { o = eGingerFileType.Solution; }
            else if (p == sUserProfile) { o = eGingerFileType.UserProfile; }
            else if ((p.Length - sBF.Length) >= 0 && p.Substring(p.Length - sBF.Length, sBF.Length) == sBF) { o = eGingerFileType.BusinessFlow; }
            else if ((p.Length - sAgent.Length) >= 0 && p.Substring(p.Length - sAgent.Length, sAgent.Length) == sAgent) { o = eGingerFileType.Agent; }
            else if ((p.Length - sEnv.Length) >= 0 && p.Substring(p.Length - sEnv.Length, sEnv.Length) == sEnv) { o = eGingerFileType.Environment; }
            else if ((p.Length - sRunSet.Length) >= 0 && p.Substring(p.Length - sRunSet.Length, sRunSet.Length) == sRunSet) { o = eGingerFileType.RunSet; }
            else if ((p.Length - sRepoAction.Length) >= 0 && p.Substring(p.Length - sRepoAction.Length, sRepoAction.Length) == sRepoAction) { o = eGingerFileType.Action; }
            else if ((p.Length - sRepoActivity.Length) >= 0 && p.Substring(p.Length - sRepoActivity.Length, sRepoActivity.Length) == sRepoActivity) { o = eGingerFileType.Activity; }
            else if ((p.Length - sRepoVariable.Length) >= 0 && p.Substring(p.Length - sRepoVariable.Length, sRepoVariable.Length) == sRepoVariable) { o = eGingerFileType.Variable; }
            else if ((p.Length - sRepoortTempalte.Length) >= 0 && p.Substring(p.Length - sRepoortTempalte.Length, sRepoortTempalte.Length) == sRepoortTempalte) { o = eGingerFileType.ReportTemplate; }
            else { o = eGingerFileType.Unknown; }
            return o;
        }

    }
}
