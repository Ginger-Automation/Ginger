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
using Amdocs.Ginger.Common.UIElement;
using System.Linq;

namespace GingerCore.Drivers.ASCF
{
    public class ASCFBrowserElementInfo
    {
        public enum eControlType
        {
            //TODO: add all ASCF controls
            Unknown,
            TextBox,
            DropDown,
            Label,
            Browser,
            Grid,
            Link,
            Button,
            CheckBox
        }

        public string Name { get; set; }
        public string Path { get; set; }        
        public eControlType ControlType { get; set; }

        public ObservableList<ControlProperty> Properties = new ObservableList<ControlProperty>();

        internal void SetPath()
        {
            Path = GetProperty("XPath");            
        }

        internal void SetControlType()
        {

            string tagName = (from x in Properties where x.Name == "tagName" select x.Value).FirstOrDefault();

            switch (tagName)
            {
                case "INPUT":
                    string InputType = (from x in Properties where x.Name == "type" select x.Value).FirstOrDefault();

                    if (InputType == "text") ControlType = ASCFBrowserElementInfo.eControlType.TextBox;
                    if (InputType == "button") ControlType = ASCFBrowserElementInfo.eControlType.Button;
                    if (InputType == "checkbox") ControlType = ASCFBrowserElementInfo.eControlType.CheckBox;
                    break;
                case "A":
                    ControlType = ASCFBrowserElementInfo.eControlType.Link;
                    break;
                case "LABEL":
                    ControlType = ASCFBrowserElementInfo.eControlType.Label;
                    break;
                case "SELECT":
                    ControlType = ASCFBrowserElementInfo.eControlType.DropDown;
                    break;

                    //TODO: add more HTML tags

                default:
                    break;
            }
        }

        internal void SetName()
        {
            // try ID
            Name = GetProperty("id");
            if (Name != null) return;

            //Try Name
            Name = GetProperty("name");
            if (Name != null) return;

            //XPath
            Name = GetProperty("XPath");
            if (Name != null) return;
        }

        internal void SetInfo()
        {
            SetControlType();
            SetPath();
            SetName();
        }

        public string GetProperty(string PropertyName)
        {
            string val = (from x in Properties where x.Name == PropertyName select x.Value).FirstOrDefault();
            return val;
        }

        public bool HasID()
        {
            string s = GetID();
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetID()
        {
            string s = GetProperty("id");
            if (s == "undefined") return "";
            return s;
        }
    }
}
