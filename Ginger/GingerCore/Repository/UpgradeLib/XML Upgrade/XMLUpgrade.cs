#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.IO;
using Amdocs.Ginger.Common;
using GingerCore.Repository;
using GingerCore.Repository.UpgradeLib;

namespace GingerCore.XMLConverters
{
    public static class XMLUpgrade
    {
        /// <summary>
        /// Update XML from old version to current version.
        /// </summary>
        /// <param name="GingerFileType"></param>
        /// <param name="xml"></param>
        /// <param name="xmlVersion"></param>
        /// <returns></returns>
        public static string Upgrade(XMLConverterBase.eGingerFileType GingerFileType, string xml, long xmlVersion, string xmlFilePath)
        {
            try
            {
                long currentXmlVersion = xmlVersion;
                long latestGingerVersion = RepositorySerializer.GetCurrentGingerVersionAsLong();

                string updatedXML = null;
                string inputXML = xml;

                // Need to repeat until we upgrade to latest version
                while (currentXmlVersion < latestGingerVersion)
                {
                    switch (currentXmlVersion)
                    {
                        case 10203:  // v 0.1.2.3
                            v10203_TO_v1020000 v1 = new v10203_TO_v1020000();
                            v1.Init(GingerFileType, inputXML, xmlFilePath);
                            v1.Convert();
                            updatedXML = v1.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1020000:  // v 1.2.2
                            v1020000_to_V1030000 v2 = new v1020000_to_V1030000();
                            v2.Init(GingerFileType, inputXML, xmlFilePath);
                            v2.Convert();
                            updatedXML = v2.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1030000:
                            v1030000_to_V1040000 v3 = new v1030000_to_V1040000();
                            v3.Init(GingerFileType, inputXML, xmlFilePath);
                            v3.Convert();
                            updatedXML = v3.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1040000:
                            v1040000_to_V1050000 v4 = new v1040000_to_V1050000();
                            v4.Init(GingerFileType, inputXML, xmlFilePath);
                            v4.Convert();
                            updatedXML = v4.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1050000:
                            v1050000_to_V1060000 v5 = new v1050000_to_V1060000();
                            v5.Init(GingerFileType, inputXML, xmlFilePath);
                            v5.Convert();
                            updatedXML = v5.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1060000:
                            v1060000_to_V1070000 v6 = new v1060000_to_V1070000();
                            v6.Init(GingerFileType, inputXML, xmlFilePath);
                            v6.Convert();
                            updatedXML = v6.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1070000:
                            v1070000_to_V1080000 v7 = new v1070000_to_V1080000();
                            v7.Init(GingerFileType, inputXML, xmlFilePath);
                            v7.Convert();
                            updatedXML = v7.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1080000:
                            v1080000_to_V1090000 v8 = new v1080000_to_V1090000();
                            v8.Init(GingerFileType, inputXML, xmlFilePath);
                            v8.Convert();
                            updatedXML = v8.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 1090000:
                            v1090000_to_V2000000 v9 = new v1090000_to_V2000000();
                            v9.Init(GingerFileType, inputXML, xmlFilePath);
                            v9.Convert();
                            updatedXML = v9.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2000000:
                            v2000000_to_V2010000 v200 = new v2000000_to_V2010000();
                            v200.Init(GingerFileType, inputXML, xmlFilePath);
                            v200.Convert();
                            updatedXML = v200.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2010000:
                            v2010000_to_V2020000 v210 = new v2010000_to_V2020000();
                            v210.Init(GingerFileType, inputXML, xmlFilePath);
                            v210.Convert();
                            updatedXML = v210.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2020000:
                            v2020000_to_V2030000 v220 = new v2020000_to_V2030000();
                            v220.Init(GingerFileType, inputXML, xmlFilePath);
                            v220.Convert();
                            updatedXML = v220.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2030000:
                            v2030000_to_V2040000 v230 = new v2030000_to_V2040000();
                            v230.Init(GingerFileType, inputXML, xmlFilePath);
                            v230.Convert();
                            updatedXML = v230.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2040000:
                            v2040000_to_V2050000 v240 = new v2040000_to_V2050000();
                            v240.Init(GingerFileType, inputXML, xmlFilePath);
                            v240.Convert();
                            updatedXML = v240.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2050000:
                            v2050000_to_V2060000 v250 = new v2050000_to_V2060000();
                            v250.Init(GingerFileType, inputXML, xmlFilePath);
                            v250.Convert();
                            updatedXML = v250.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2060000:
                            v2060000_to_V2070000 v260 = new v2060000_to_V2070000();
                            v260.Init(GingerFileType, inputXML, xmlFilePath);
                            v260.Convert();
                            updatedXML = v260.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        case 2070000:
                            v2070000_to_V3000000 v270 = new v2070000_to_V3000000();
                            v270.Init(GingerFileType, inputXML, xmlFilePath);
                            v270.Convert();
                            updatedXML = v270.UpdatedXML;
                            inputXML = updatedXML;
                            break;
                        default:
                            Reporter.ToLog(eAppReporterLogLevel.WARN, string.Format("Failed to upgrade the XML of the file '{0}' due to unkonwn version", xmlFilePath));
                            return null;
                    }

                    if (string.IsNullOrEmpty(updatedXML) || currentXmlVersion < 0)
                    {
                        Reporter.ToLog(eAppReporterLogLevel.WARN, string.Format("Failed to upgrade the XML of the file '{0}'", xmlFilePath));
                        return null;
                    }

                    currentXmlVersion = SolutionUpgrade.GetSolutonFileGingerVersionAsLong(xmlFilePath, updatedXML);
                    if (currentXmlVersion <= 0) return null;//failed to get the version
                }

                return updatedXML;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.WARN, string.Format("Failed to upgrade the XML of the file '{0}'", xmlFilePath), ex);
                return null;
            }
        }
    

        public static string UpgradeSolutionXMLFile(string xmlFilePath, string xml="")
        {
            if (string.IsNullOrEmpty(xml))
                xml = File.ReadAllText(xmlFilePath);

            long xmlGingerVersion = SolutionUpgrade.GetSolutonFileGingerVersionAsLong(xmlFilePath, xml);
            if (xmlGingerVersion <= 0) return null;//failed to get the version

            XMLConverterBase.eGingerFileType FileType = XMLConverterBase.GetGingerFileTypeFromFilename(xmlFilePath);            
        
            return Upgrade(FileType, xml, xmlGingerVersion, xmlFilePath);
        }

        public static string UpgradeSolutionXMLFileIfNeeded(string xmlFilePath, string xml="")
        {
            if (string.IsNullOrEmpty(xml))
                xml = File.ReadAllText(xmlFilePath);

            long xmlGingerVersion = SolutionUpgrade.GetSolutonFileGingerVersionAsLong(xmlFilePath, xml);
            if (xmlGingerVersion <= 0) return null;//failed to get the version
            
            if (RepositorySerializer.GetCurrentGingerVersionAsLong() > xmlGingerVersion)
            {
                //upgrade
                GingerCore.XMLConverters.XMLConverterBase.eGingerFileType FileType = XMLConverterBase.GetGingerFileTypeFromFilename(xmlFilePath);
                return Upgrade(FileType, xml, xmlGingerVersion, xmlFilePath);
            }
            else
                return xml;
        }
    }
}