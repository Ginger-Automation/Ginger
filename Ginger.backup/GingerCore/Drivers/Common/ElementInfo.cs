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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using GingerCore.Actions.Common;
using System;

namespace GingerCore.Drivers.Common
{
    /// <summary>
    /// Base class for differnet Control type for each driver, enable to show unified list in Window Explorer Grid
    /// </summary>
    /// 
    // We can persist ElementInfo - for example when saving DOR Page UIElements, but when used in Window Explorer there is no save
    public class ElementInfo : RepositoryItem
    {
        public new static class Fields
        {
            public static string Active = "Active";
            public static string ElementName = "ElementName";
            public static string ElementTitle = "ElementTitle";
            public static string ElementType = "ElementType";
            public static string Value = "Value";
            public static string Path = "Path";
            public static string XPath = "XPath";
            public static string IsExpandable = "IsExpandable";
            public static string X = "X";
            public static string Y = "Y";
            public static string Width = "Width";
            public static string Height = "Height";
        }

        public object ElementObject{ get; set; }

        // ---------------------------------------------------------------------------------------------------------------------
        //  ElementTitle
        // ---------------------------------------------------------------------------------------------------------------------

        private string mElementTitle = null;
        [IsSerializedForLocalRepository]
        public string ElementTitle { get 
        { 
            if (mElementTitle == null) mElementTitle = GetElementTitle();            
            return mElementTitle;            
        }
            set { mElementTitle = value; }
        }   // Developer Name

        // Used for Lazy loading when possible
        public virtual string GetElementTitle()
        {
            // we return Name unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return mElementTitle;
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  ElementName
        // ---------------------------------------------------------------------------------------------------------------------

        // elemnt name is given by the user when he maps UI elements and give them name to use in DOR

        private string mElementName = null;
        [IsSerializedForLocalRepository]
        public string ElementName
        {
            get
            {
                return mElementName;
            }
            set
            {
                mElementName = value;
            }
        }


        // ---------------------------------------------------------------------------------------------------------------------
        //  ElementType
        // ---------------------------------------------------------------------------------------------------------------------
        private string mElementType = null;// Class for Java, TagName for HTML...
        [IsSerializedForLocalRepository]
        public string ElementType
        {
            get
            {
                if (mElementType == null) mElementType = GetElementType();                
                return mElementType;                
            }
            set { mElementType = value; }
        }

        // Used for Lazy loading when possible
        public virtual string GetElementType()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return mElementType;
        }


        // ---------------------------------------------------------------------------------------------------------------------
        //  Value
        // ---------------------------------------------------------------------------------------------------------------------
        
        public string mValue = null;
        // Something the user see: label.value, button.text, text.value etc.
        public string Value
        {
            get
            {
                if (mValue == null) mValue = GetValue();
                return mValue;
            }
            set { mValue = value; }
        }

        public virtual string GetValue()
        {
            // we return XPath unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return mValue;
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  Path
        // ---------------------------------------------------------------------------------------------------------------------
        public string Path { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------
        //  AbsoluteXPath
        // ---------------------------------------------------------------------------------------------------------------------
        private string mXPath = null;

        public string XPath
        {
            get
            {
                if (mXPath == null) mXPath = GetAbsoluteXpath();
                return mXPath;
            }
            set { mXPath = value; }
        }

        // should be override in sub class when possible for lazy loading
        public virtual string GetAbsoluteXpath()
        {
            // we return XPath unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return null;            
        }

        // ---------------------------------------------------------------------------------------------------------------------
        //  SmartXPath
        // ---------------------------------------------------------------------------------------------------------------------
        //TODO: Add XPathSmart


        // ---------------------------------------------------------------------------------------------------------------------
        //  AbsoluteXPath
        // ---------------------------------------------------------------------------------------------------------------------
        public Boolean IsExpandable { get; set; }

        // Driver which implement IWindowExplorer
        public IWindowExplorer WindowExplorer { get; set; }
        public override string ItemName { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        object mData = null;

        public ObservableList<ControlProperty> GetElementProperties()
        {
            return this.WindowExplorer.GetElementProperties(this);
        }

        public ObservableList<ElementLocator> GetElementLocators()
        {            
            return this.WindowExplorer.GetElementLocators(this);
        }

        public object GetElementData(ActUIElement.eLocateBy elementLocateBy = ActUIElement.eLocateBy.ByXPath, string elementLocateValue = "")
        {
            //We cache the data, if needed add refresh button
            if (mData == null)
            {
                mData = this.WindowExplorer.GetElementData(this, elementLocateBy, elementLocateValue);    
            }
            return mData;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ElementLocator> Locators;

        [IsSerializedForLocalRepository]
        public int X { get; set; }

        [IsSerializedForLocalRepository]
        public int Y { get; set; }

        [IsSerializedForLocalRepository]
        public int Width { get; set; }

        [IsSerializedForLocalRepository]
        public int Height { get; set; }

        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        [IsSerializedForLocalRepository]
        public bool Mandatory { get; set; }

        //TODO: Add a new class to keep data like for combo box items or datagrid list etc...
        // need to be a class which can hold different types

        public ObservableList<ActUIElement> GetAvailableActions()
        {
            ObservableList<ActUIElement> list = new ObservableList<ActUIElement>();
            //TODO: based on ElementType - the generic one or... get the possiblie actions sorted by priority

            ActUIElement.eElementType EType;

            switch (this.ElementType)
            {
                //TODO: get platform convert from platofrm elem tpye to - ActUIElement.eElementAction - or when creating EI - set the enum - add field
                case "LABEL":
                    EType = ActUIElement.eElementType.Label;
                    //TODO: Add output value with expected
                    list.Add(new ActUIElement() {ElementType = EType,  Description = GetDesc("Validate {0} Value"), ElementAction = ActUIElement.eElementAction.GetValue });

                    list.Add(new ActUIElement() { ElementType = EType, Description = GetDesc("Get Value of {0}"), ElementAction = ActUIElement.eElementAction.GetValue });
                    break;
                case "INPUT.TEXT":
                    EType = ActUIElement.eElementType.TextBox;
                    list.Add(new ActUIElement() { ElementType = EType, Description = GetDesc("Set Value of {0}"), ElementAction = ActUIElement.eElementAction.SetValue });
                    list.Add(new ActUIElement() { ElementType = EType, Description = GetDesc("Get Value of {0}"), ElementAction = ActUIElement.eElementAction.GetValue });
                    break;
                case "INPUT.BUTTON":
                    EType = ActUIElement.eElementType.Button;
                    list.Add(new ActUIElement() { ElementType = EType, Description = GetDesc("Click on {0}"), ElementAction = ActUIElement.eElementAction.Click });
                    break;

                    //TODO: the rest...
            }

            foreach (ActUIElement a in list)
            {
                a.ElementLocateBy = ActUIElement.eLocateBy.ByModelName;
                a.ElementLocateValue = this.ElementName;
                a.Active = true;
            }
            return list;
        }

        private string GetDesc(string v)
        {
            return string.Format(v, "'" + this.ElementName + "'");
        }
    }
}
