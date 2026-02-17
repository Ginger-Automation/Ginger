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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{

    public class ElementWrapperInfo
    {
        public List<ElementWrapper> elements { get; set; }
    }

    public class ElementWrapper
    {
        public Element elementinfo { get; set; }
    }

    public class Element
    {
        public Guid elementGuid { get; set; }
        public ElementwrapperProperties Properties { get; set; }
        public ElementwrapperLocators locators { get; set; }
        public ElementwrapperContext context { get; set; }
    }

    public class ElementwrapperProperties
    {
        [JsonProperty("Platform element Type")]
        public string PlatformElementType { get; set; }

        [JsonProperty("Element Type")]
        public string ElementType
        {
            get; set;
        }
        public string name { get; set; }
        public string Xpath { get; set; }
        [JsonProperty("Relative Xpath")]
        public string RelativeXpath { get; set; }
        public string TagName { get; set; }
        public string Displayed { get; set; }
        public string Enabled { get; set; }
        public string placeholder { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string Selected { get; set; }
        public string Text { get; set; }
        public string autocapitalize { get; set; }
        public string autocorrect { get; set; }
        [JsonProperty("data-test")]
        public string DataTest { get; set; }
        public string id { get; set; }
        public string @class { get; set; }
        public Dictionary<string, string> attributes { get; set; }
        public string style { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string EnhanceName { get; set; }
        public string EnhanceDescription { get; set; }


        public string GetPropertyValue(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) { return null; }
            var t = typeof(ElementwrapperProperties);
            // Try direct, case-insensitive
            var prop = t.GetProperty(propertyName,System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (prop != null) { return prop.GetValue(this)?.ToString(); }
            // Try by JsonProperty attribute match
            foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var jp = (JsonPropertyAttribute)Attribute.GetCustomAttribute(p, typeof(JsonPropertyAttribute));
                if (jp != null && string.Equals(jp.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return p.GetValue(this)?.ToString();
                }
            }
            return null;
        }

    }

    public class ElementwrapperLocators
    {
        public string ByXPath { get; set; }
        public string ByRelXPath { get; set; }
        public string ByCSS { get; set; }

        public string ByID { get; set; }

        public string ByName { get; set; }
        public string ByClassName { get; set; }
        public string ByCSSSelector { get; set; }
        public string ByTagName { get; set; }
        public string ByPlaceholder { get; set; }
        public string ByLinkText { get; set; }
        public string ByHref { get; set; }
        public string ByAriaLabel { get; set; }
        public string ByDataTestId { get; set; }
        public string ByTitle { get; set; }
        public object EnhanceLocatorsByAI { get; set; }



    }



    public class ElementwrapperContext
    {
        public bool insideIframe { get; set; }
        public string iframeXPath { get; set; }
        public bool insideShadowDOM { get; set; }
        public string shadowHostXPath { get; set; }
    }

}
