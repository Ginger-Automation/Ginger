#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers.Common;
using System;
using System.Collections.Generic;
using System.Xml;

#nullable enable
namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    internal static class KatalonWebElementEntityToHTMLElementInfoConverter
    {
        internal static HTMLElementInfo Convert(XmlElement webElementEntity)
        {
            Guid guid = GetGuidFromWebElementEntity(webElementEntity);
            string elementName = GetElementNameFromWebElementEntity(webElementEntity);
            string elementType = GetElementTypeFromWebElementEntity(webElementEntity);
            eElementType elementTypeEnum = GetElementTypeEnumFromWebElementEntity(webElementEntity);
            IEnumerable<ElementLocator> locators = GetLocatorsFromWebElementEntity(webElementEntity);
            IEnumerable<ControlProperty> properties = GetElementProperties(webElementEntity);

            HTMLElementInfo htmlElementInfo = new()
            {
                Active = true,
                Guid = guid,
                ElementName = elementName,
                ElementType = elementType,
                ElementTypeEnum = elementTypeEnum,
                IsAutoLearned = true, //static
                Locators = new(locators),
                Properties = new(properties),
            };

            return htmlElementInfo;
        }

        private static Guid GetGuidFromWebElementEntity(XmlElement webElementEntity)
        {
            const string elementGuidIdElementName = "elementGuidId";

            XmlNodeList guidElements = webElementEntity.GetElementsByTagName(elementGuidIdElementName);
            if (guidElements.Count <= 0)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No '{elementGuidIdElementName}' element found.");
                return Guid.Empty;
            }

            XmlNode? guidElement = guidElements[0];
            if (guidElement == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"'{elementGuidIdElementName}' element is null.");
                return Guid.Empty;
            }

            if (!Guid.TryParse(guidElement.InnerText, out Guid guid))
            {
                Reporter.ToLog(eLogLevel.WARN, $"'{elementGuidIdElementName}' element value is not a valid Guid.");
                return Guid.Empty;
            }

            return guid;
        }

        private static string GetElementNameFromWebElementEntity(XmlElement webElementEntity)
        {
            const string nameElementName = "name";

            XmlNodeList nameElements = webElementEntity.GetElementsByTagName(nameElementName);
            if (nameElements.Count <= 0)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No '{nameElementName}' element found.");
                return string.Empty;
            }

            XmlNode? nameElement = nameElements[0];
            if (nameElement == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"'{nameElementName}' element is null.");
                return string.Empty;
            }

            return nameElement.InnerText;
        }

        private static string GetElementTypeFromWebElementEntity(XmlElement webElementEntity)
        {
            XmlElement? webElementPropertiesWithTagNameElement = GetWebElementPropertyByName(webElementEntity, name: "tag");
            if (webElementPropertiesWithTagNameElement == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'WebElementProperties' element found with 'name' element value as 'tag'.");
                return string.Empty;
            }

            XmlNodeList? valueElements = webElementPropertiesWithTagNameElement.GetElementsByTagName("value");
            if (valueElements.Count <= 0 || valueElements[0] == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'value' element found with 'WebElementProperties' element with 'name' element value as 'tag'.");
                return string.Empty;
            }

            return valueElements[0]!.InnerText;
        }

        private static eElementType GetElementTypeEnumFromWebElementEntity(XmlElement webElementEntity)
        {
            string type = GetElementTypeFromWebElementEntity(webElementEntity);
            if (type == null || string.Equals(type.Trim(), string.Empty))
            {
                Reporter.ToLog(eLogLevel.WARN, $"No element type found.");
                return eElementType.Unknown;
            }

            XmlElement? webElementPropertiesWithTypeNameElement = GetWebElementPropertyByName(webElementEntity, name: "type");
            if (webElementPropertiesWithTypeNameElement == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'WebElementProperties' element found with 'name' element value as 'type'.");
                return eElementType.Unknown;
            }

            XmlNodeList? valueElements = webElementPropertiesWithTypeNameElement.GetElementsByTagName("value");
            if (valueElements.Count <= 0 || valueElements[0] == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'value' element found with 'WebElementProperties' element with 'name' element value as 'type'.");
                return eElementType.Unknown;
            }

            string tag = valueElements[0]!.InnerText;

            return POMLearner.GetElementType(tag, type);
        }

        private static XmlElement? GetWebElementPropertyByName(XmlElement webElementEntity, string name)
        {
            const string webElementPropertiesElementName = "webElementProperties";

            XmlNodeList webElementProperties = webElementEntity.GetElementsByTagName(webElementPropertiesElementName);
            if (webElementProperties.Count <= 0)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No '{webElementPropertiesElementName}' element found.");
                return null;
            }

            XmlElement? webElementPropertiesWithName = null;
            foreach (XmlNode webElementProperty in webElementProperties)
            {
                if (webElementProperty is not XmlElement)
                {
                    continue;
                }

                XmlNodeList nameElements = ((XmlElement)webElementProperty).GetElementsByTagName("name");
                if (nameElements.Count <= 0 || nameElements[0] == null)
                {
                    continue;
                }

                if (!string.Equals(nameElements[0]!.InnerText, name))
                {
                    continue;
                }

                webElementPropertiesWithName = (XmlElement)webElementProperty;
                break;
            }

            return webElementPropertiesWithName;
        }

        private static IEnumerable<ElementLocator> GetLocatorsFromWebElementEntity(XmlElement webElementEntity)
        {
            List<ElementLocator> locators = [];

            IEnumerable<ElementLocator> xpathLocators = GetXPathLocatorsFromWebElementEntity(webElementEntity);
            locators.AddRange(xpathLocators);

            IEnumerable<ElementLocator> idLocators = GetIdLocatorsFromWebElementEntity(webElementEntity);
            locators.AddRange(idLocators);

            IEnumerable<ElementLocator> nameLocators = GetNameLocatorsFromWebElementEntity(webElementEntity);
            locators.AddRange(nameLocators);

            return locators;
        }

        private static IEnumerable<ElementLocator> GetXPathLocatorsFromWebElementEntity(XmlElement webElementEntity)
        {
            List<ElementLocator> locators = [];

            XmlNodeList webElementXpaths = webElementEntity.GetElementsByTagName("webElementXpaths");
            foreach (XmlNode webElementXpath in webElementXpaths)
            {
                if (webElementXpath is not XmlElement)
                {
                    continue;
                }

                XmlNodeList valueElements = ((XmlElement)webElementXpath).GetElementsByTagName("value");
                if (valueElements.Count <= 0 || valueElements[0] == null)
                {
                    continue;
                }

                locators.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = valueElements[0]!.InnerText,
                    IsAutoLearned = true, //static
                });
            }

            return locators;
        }

        private static IEnumerable<ElementLocator> GetIdLocatorsFromWebElementEntity(XmlElement webElementEntity)
        {
            List<ElementLocator> locators = [];

            XmlElement? webElementProperty = GetWebElementPropertyByName(webElementEntity, name: "id");
            if (webElementProperty == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'webElementProperties' found with name 'id'.");
                return locators;
            }

            XmlNodeList values = webElementProperty.GetElementsByTagName("value");
            if (values.Count <= 0 || values[0] == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'value' element found in 'webElementProperties' with name 'id'.");
                return locators;
            }

            locators.Add(new ElementLocator()
            {
                Active = true,
                LocateBy = eLocateBy.ByID,
                LocateValue = values[0]!.InnerText,
                IsAutoLearned = true, //static
            });

            return locators;
        }

        private static IEnumerable<ElementLocator> GetNameLocatorsFromWebElementEntity(XmlElement webElementEntity)
        {
            List<ElementLocator> locators = [];

            XmlElement? webElementProperty = GetWebElementPropertyByName(webElementEntity, name: "name");
            if (webElementProperty == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'webElementProperties' found with name 'name'.");
                return locators;
            }

            XmlNodeList values = webElementProperty.GetElementsByTagName("value");
            if (values.Count <= 0 || values[0] == null)
            {
                Reporter.ToLog(eLogLevel.WARN, $"No 'value' element found in 'webElementProperties' with name 'name'.");
                return locators;
            }

            locators.Add(new ElementLocator()
            {
                Active = true,
                LocateBy = eLocateBy.ByName,
                LocateValue = values[0]!.InnerText,
                IsAutoLearned = true, //static
            });

            return locators;
        }

        private static IEnumerable<ControlProperty> GetElementProperties(XmlElement webElementEntity)
        {
            List<ControlProperty> properties = [];

            string? tagValue = null;
            string? typeValue = null;

            XmlNodeList webElementProperties = webElementEntity.GetElementsByTagName("webElementProperties");
            foreach (XmlNode webElementProperty in webElementProperties)
            {
                if (webElementProperty is not XmlElement)
                {
                    continue;
                }

                XmlNodeList names = ((XmlElement)webElementProperty).GetElementsByTagName("name");
                if (names.Count <= 0 || names[0] == null)
                {
                    continue;
                }
                string name = names[0]!.InnerText;


                XmlNodeList values = ((XmlElement)webElementProperty).GetElementsByTagName("value");
                if (values.Count <= 0 || values[0] == null)
                {
                    continue;
                }
                string value = values[0]!.InnerText;

                if (string.Equals(name, "tag"))
                {
                    tagValue = value;
                    continue;
                }
                else if (string.Equals(name, "type"))
                {
                    typeValue = value;
                    continue;
                }

                properties.Add(new ControlProperty()
                {
                    Name = name,
                    Value = value,
                    Category = ePomElementCategory.Web,
                });
            }

            if (tagValue != null)
            {
                properties.Add(new ControlProperty()
                {
                    Name = "Platform Element Type",
                    Value = tagValue,
                    Category = ePomElementCategory.Web,
                });
            }
            if (tagValue != null && typeValue != null)
            {
                properties.Add(new ControlProperty()
                {
                    Name = "Element Type",
                    Value = POMLearner.GetElementType(tagValue, typeValue).ToString(),
                    Category = ePomElementCategory.Web,
                });
            }

            return properties;
        }
    }
}
