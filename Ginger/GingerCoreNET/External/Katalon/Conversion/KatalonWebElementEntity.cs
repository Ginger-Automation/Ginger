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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.Repository;
using DocumentFormat.OpenXml;
using GingerCore.Drivers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

#nullable enable
namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    internal sealed class KatalonWebElementEntity : KatalonElementEntity
    {
        internal IEnumerable<WebElementProperty> Properties { get; }

        internal IEnumerable<WebElementXPath> XPaths { get; }

        private KatalonWebElementEntity(Guid elementGuidId, string name, IEnumerable<WebElementProperty> properties, IEnumerable<WebElementXPath> xpaths) :
            base(elementGuidId, name)
        {
            Properties = properties;
            XPaths = xpaths;
        }

        internal static bool CanParse(XmlElement xmlElement)
        {
            return string.Equals(xmlElement.Name, "WebElementEntity");
        }

        internal static bool TryParse(XmlElement xmlElement, out KatalonWebElementEntity? webElementEntity)
        {
            if (!CanParse(xmlElement))
            {
                webElementEntity = null;
                return false;
            }

            try
            {
                Guid elementGuidId = GetElementGuidId(xmlElement);
                string name = GetName(xmlElement);
                IEnumerable<WebElementProperty> properties = GetProperties(xmlElement);
                IEnumerable<WebElementXPath> xpaths = GetXPaths(xmlElement);

                webElementEntity = new(elementGuidId, name, properties, xpaths);

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Unable to convert XML element to Katalon WebElementEntity", ex);
                webElementEntity = null;
                return false;
            }
        }

        private static Guid GetElementGuidId(XmlElement xmlElement)
        {
            const string elementGuidIdElementName = "elementGuidId";

            XmlNodeList guidElements = xmlElement.GetElementsByTagName(elementGuidIdElementName);
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

        private static string GetName(XmlElement xmlElement)
        {
            const string nameElementName = "name";

            XmlNodeList nameElements = xmlElement.GetElementsByTagName(nameElementName);
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

        private static IEnumerable<WebElementProperty> GetProperties(XmlElement xmlElement)
        {
            List<WebElementProperty> properties = [];

            XmlNodeList webElementProperties = xmlElement.GetElementsByTagName("webElementProperties");
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

                properties.Add(new(name, value));
            }

            return properties;
        }

        private static IEnumerable<WebElementXPath> GetXPaths(XmlElement xmlElement)
        {
            List<WebElementXPath> xpaths = [];

            XmlNodeList webElementXpaths = xmlElement.GetElementsByTagName("webElementXpaths");
            foreach (XmlNode webElementXpath in webElementXpaths)
            {
                if (webElementXpath is not XmlElement)
                {
                    continue;
                }

                XmlNodeList names = ((XmlElement)webElementXpath).GetElementsByTagName("name");
                if (names.Count <= 0 || names[0] == null)
                {
                    continue;
                }
                string name = names[0]!.InnerText;


                XmlNodeList values = ((XmlElement)webElementXpath).GetElementsByTagName("value");
                if (values.Count <= 0 || values[0] == null)
                {
                    continue;
                }
                string value = values[0]!.InnerText;

                xpaths.Add(new(name, value));
            }

            return xpaths;
        }

        internal ElementInfo ToElementInfo(IEnumerable<KatalonWebElementEntity> elements)
        {
            HTMLElementInfo element = new()
            {
                Active = true,
                Guid = ElementGuidId,
                ElementName = Name,
                Path = string.Empty,
                ElementType = GetElementType(),
                ElementTypeEnum = GetElementTypeEnum(),
                IsAutoLearned = true, //static
                Locators = new(GetLocators()),
                Properties = new(GetProperties())
            };

            if (IsRefElementIsShadowRoot())
            {
                KatalonWebElementEntity? refElement = GetRefElement(elements);

                if (refElement != null)
                {
                    element.Properties.Add(new()
                    {
                        Name = ElementProperty.ParentPOMGUID,
                        Value = refElement.ElementGuidId.ToString(),
                        Category = ePomElementCategory.Web,
                    });
                }
            }
            else
            {
                string path = string.Empty;
                GetRefElementCombinedPath(this, elements, ref path);

                element.Path = path;

                element.Properties.Add(new()
                {
                    Name = "Parent IFrame",
                    Value = path,
                    Category = ePomElementCategory.Web,
                });
            }

            return element;
        }

        private bool IsRefElementIsShadowRoot()
        {
            WebElementProperty? refElementProperty = Properties.FirstOrDefault(p => string.Equals(p.Name, "ref_element_is_shadow_root"));
            return refElementProperty != null;
        }

        private KatalonWebElementEntity? GetRefElement(IEnumerable<KatalonWebElementEntity> elements)
        {
            WebElementProperty? refElementProperty = Properties.FirstOrDefault(p => string.Equals(p.Name, "ref_element"));
            if (refElementProperty == null)
            {
                return null;
            }

            string refElementName = refElementProperty.Value.Split('/')[^1];

            return elements.FirstOrDefault(e => string.Equals(e.Name, refElementName));
        }

        private static void GetRefElementCombinedPath(KatalonWebElementEntity element, IEnumerable<KatalonWebElementEntity> elements, ref string path)
        {
            KatalonWebElementEntity? refElement = element.GetRefElement(elements);
            if (refElement == null)
            {
                return;
            }

            string? refWebElementXPath = refElement.XPaths.FirstOrDefault(xpath => string.Equals(xpath.Name, "xpath:position"))?.Value;
            if (refWebElementXPath == null)
            {
                refWebElementXPath = refElement.XPaths.First().Value;
            }

            path = $"{refWebElementXPath},{path}";
            path = path.TrimEnd(',');

            GetRefElementCombinedPath(refElement, elements, ref path);
        }

        private void SetRefElementProperties(HTMLElementInfo element, IEnumerable<KatalonElementEntity> allElements)
        {
            WebElementProperty? refElementProperty = this
                    .Properties
                    .FirstOrDefault(p => string.Equals(p.Name, "ref_element"));
            if (refElementProperty == null)
            {
                return;
            }

            string refElementName = refElementProperty.Value.Split('/')[^1];

            KatalonElementEntity? refElement = allElements.FirstOrDefault(e => string.Equals(e.Name, refElementName));
            if (refElement == null ||
                refElement is not KatalonWebElementEntity refWebElement ||
                !refWebElement.XPaths.Any())
            {
                return;
            }

            bool refElementIsIFrame = this
                .Properties
                .FirstOrDefault(p => string.Equals(p.Name, "ref_element_is_shadow_root")) == null;

            string? refWebElementXPath = refWebElement.XPaths.FirstOrDefault(xpath => string.Equals(xpath.Name, "xpath:position"))?.Value;
            if (refWebElementXPath == null)
            {
                refWebElementXPath = refWebElement.XPaths.First().Value;
            }
            element.Path = refWebElementXPath;

            element.Properties.Add(new()
            {
                Name = ElementProperty.ParentPOMGUID,
                Value = refWebElement.ElementGuidId.ToString(),
                Category = ePomElementCategory.Web,
                ShowOnUI = false,
            });

            if (refElementIsIFrame)
            {
                element.Properties.Add(new()
                {
                    Name = "Parent IFrame",
                    Value = refWebElementXPath,
                    Category = ePomElementCategory.Web,
                });
            }
        }

        private string GetElementType()
        {
            WebElementProperty? tagProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "tag"));
            if (tagProperty == null)
            {
                return string.Empty;
            }

            return tagProperty.Value;
        }

        private eElementType GetElementTypeEnum()
        {
            WebElementProperty? tagProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "tag"));
            if (tagProperty == null)
            {
                return eElementType.Unknown;
            }

            WebElementProperty? typeProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "type"));
            if (typeProperty == null)
            {
                return eElementType.Unknown;
            }

            return POMLearner.GetElementType(tagProperty.Value, typeProperty.Value);
        }

        private IEnumerable<ElementLocator> GetLocators()
        {
            List<ElementLocator> locators = [];

            foreach (WebElementXPath xpath in XPaths)
            {
                locators.Add(new()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = xpath.Value,
                    IsAutoLearned = true, //static
                });
            }

            WebElementProperty? idProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "id"));
            if (idProperty != null)
            {
                locators.Add(new()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByID,
                    LocateValue = idProperty.Value,
                    IsAutoLearned = true, //static
                });
            }

            WebElementProperty? nameProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "name"));
            if (nameProperty != null)
            {
                locators.Add(new()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByName,
                    LocateValue = nameProperty.Value,
                    IsAutoLearned = true, //static
                });
            }

            return locators;
        }

        private IEnumerable<ControlProperty> GetProperties()
        {
            List<ControlProperty> properties = [];

            foreach (WebElementProperty property in Properties)
            {
                properties.Add(new()
                {
                    Name = property.Name,
                    Value = property.Value,
                    Category = ePomElementCategory.Web,
                });
            }

            WebElementProperty? tagProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "tag"));
            if (tagProperty != null)
            {
                properties.Add(new ControlProperty()
                {
                    Name = "Platform Element Type",
                    Value = tagProperty.Value,
                    Category = ePomElementCategory.Web,
                });
            }

            WebElementProperty? typeProperty = Properties.FirstOrDefault(prop => string.Equals(prop.Name, "type"));
            if (tagProperty != null && typeProperty != null)
            {
                properties.Add(new ControlProperty()
                {
                    Name = "Element Type",
                    Value = POMLearner.GetElementType(tagProperty.Value, typeProperty.Value).ToString(),
                    Category = ePomElementCategory.Web,
                });
            }

            return properties;
        }

        internal sealed class WebElementProperty(string name, string value)
        {
            internal string Name { get; } = name;

            internal string Value { get; } = value;
        }

        internal sealed class WebElementXPath(string name, string value)
        {
            internal string Name { get; } = name;

            internal string Value { get; } = value;
        }
    }
}
