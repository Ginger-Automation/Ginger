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

//---------
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.Enums;

namespace Amdocs.Ginger.Common.UIElement
{
    /// <summary>
    /// Base class for differnet Control type for each driver, enable to show unified list in Window Explorer Grid
    /// </summary>
    /// 
    // We can persist ElementInfo - for example when saving DOR Page UIElements, but when used in Window Explorer there is no save
    public class ElementInfo : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public ObservableList<ElementLocator> Locators = new ObservableList<ElementLocator>();

        [IsSerializedForLocalRepository]
        public ObservableList<ControlProperty> Properties = new ObservableList<ControlProperty>();

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

        [IsSerializedForLocalRepository]
        public bool IsAutoLearned { get; set; }

        public object ElementObject { get; set; }
        public Boolean IsExpandable { get; set; }

        public IWindowExplorer WindowExplorer { get; set; }        

        private string mElementTitle = null;
        [IsSerializedForLocalRepository]
        public string ElementTitle
        {
            get
            {
                if (mElementTitle == null) mElementTitle = GetElementTitle();
                return mElementTitle;
            }
            set { mElementTitle = value; }
        }

        public enum eElementStatus
        {
            Pending,
            Passed,
            Failed
        }

        eElementStatus mElementStatus;
        public eElementStatus ElementStatus
        {
            get
            {
                return mElementStatus;
            }
            set
            {
                mElementStatus = value;
                OnPropertyChanged(nameof(StatusError));
                OnPropertyChanged(nameof(StatusIcon));
            }
        }

        public eImageType StatusIcon
        {
            get
            {
                switch (ElementStatus)
                {
                    case eElementStatus.Passed:
                        return eImageType.Passed;
                    case eElementStatus.Failed:
                        return eImageType.Failed;
                    case eElementStatus.Pending:
                    default:
                        return eImageType.Pending;
                }
            }
        }

        private string mLocateStatusError;
        public string StatusError
        {
            get
            {
                return mLocateStatusError;
            }
            set
            {
                mLocateStatusError = value;
            }
        }


        // Used for Lazy loading when possible
        public virtual string GetElementTitle()
        {
            // we return Name unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return mElementTitle;
        }


        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        
        public override string ItemName { get { return this.ElementName; } set { this.ElementName = value; } }

        private string mElementName = null;
        [IsSerializedForLocalRepository]
        public string ElementName // elemnt name is given by the user when he maps UI elements and give them name to use in DOR
        {
            get
            {
                if (string.IsNullOrEmpty(mElementName))
                    mElementName = mElementTitle;
                return mElementName;
            }
            set
            {
                mElementName = value;
            }
        }

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

        private eElementType mElementTypeEnum = eElementType.Unknown;
        [IsSerializedForLocalRepository]
        public eElementType ElementTypeEnum
        {
            get
            {
                return mElementTypeEnum;
            }
            set { mElementTypeEnum = value; }
        }

        public string ElementTypeEnumDescription
        {
            get
            {
                string enumDescription = mElementTypeEnum.ToString();
                try
                {
                    enumDescription = ((EnumValueDescriptionAttribute[])typeof(eElementType).GetField(mElementTypeEnum.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription;
                }
                catch { }
                return enumDescription;
            }
        }

        [IsSerializedForLocalRepository]
        public List<String> OptionalValues = new List<String>();

        public string OptionalValuesAsString
        {
            get
            {
                string listString = string.Empty;
                foreach (string value in OptionalValues) listString += value + ",";
                listString.TrimEnd(',');
                return listString;
            }
        }

        // Used for Lazy loading when possible
        public virtual string GetElementType()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return mElementType;
        }

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

        [IsSerializedForLocalRepository]
        public string Path { get; set; }

        //  AbsoluteXPath


        private string mXPath;

        [IsSerializedForLocalRepository]
        public string XPath
        {
            get
            {
                if (mXPath == null) mXPath = GetAbsoluteXpath();
                return mXPath;
            }
            set
            {
                mXPath = value;
                OnPropertyChanged(nameof(this.XPath));  // fix for 6342
            }
        }

        public bool Selected { get; set; }

        // should be override in sub class when possible for lazy loading
        public virtual string GetAbsoluteXpath()
        {
            // we return XPath unless it was overridden as expected
            // So we keep backword compatibility until all drivers do it correctly
            return null;
        }

        object mData = null;

        public ObservableList<ControlProperty> GetElementProperties()
        {
            return this.WindowExplorer.GetElementProperties(this);
        }

        public ObservableList<ElementLocator> GetElementLocators()
        {
            return this.WindowExplorer.GetElementLocators(this);
        }

        public object GetElementData(eLocateBy elementLocateBy = eLocateBy.ByXPath, string elementLocateValue = "")
        {
            //We cache the data, if needed add refresh button
            if (mData == null)
            {
                mData = this.WindowExplorer.GetElementData(this, elementLocateBy, elementLocateValue);
            }
            return mData;
        }

    }

    public enum eLocateBy
    {
        [EnumValueDescription("NA")]
        NA,
        [EnumValueDescription("")]
        Unknown,
        [EnumValueDescription("By ID")]
        ByID,
        [EnumValueDescription("By Name")]
        ByName,
        [EnumValueDescription("By CSS")]
        ByCSS,
        [EnumValueDescription("By XPath")]
        ByXPath,
        [EnumValueDescription("By RelXPath")]
        ByRelXPath,
        [EnumValueDescription("By X,Y")]
        ByXY,
        [EnumValueDescription("By Container Name")]
        ByContainerName,
        [EnumValueDescription("By Href")]
        ByHref,
        [EnumValueDescription("By Link Text")]
        ByLinkText,
        [EnumValueDescription("By Value")]
        ByValue,
        [EnumValueDescription("By Index")]
        ByIndex,
        [EnumValueDescription("By Class name")]
        ByClassName, //Android, UI Automation
        [EnumValueDescription("By AutomationId")]
        ByAutomationID,
        [EnumValueDescription("By Localized Control Type")]
        ByLocalizedControlType,
        [EnumValueDescription("By Multiple Properties")]
        ByMulitpleProperties,
        [EnumValueDescription("By Bounding Rectangle")]
        ByBoundingRectangle,
        [EnumValueDescription("Is Enabled")]
        IsEnabled,
        [EnumValueDescription("Is Off Screen")]
        IsOffscreen,
        [EnumValueDescription("By Title")]
        ByTitle,
        [EnumValueDescription("By CaretPosition")]
        ByCaretPosition,
        [EnumValueDescription("By URL")]
        ByUrl,
        [EnumValueDescription("By ng-model")]
        ByngModel,
        [EnumValueDescription("By ng-Repeat")]
        ByngRepeat,
        [EnumValueDescription("By ng-Bind")]
        ByngBind,
        [EnumValueDescription("By ng-SelectedOption")]
        ByngSelectedOption,
        [EnumValueDescription("By Resource ID")]
        ByResourceID,
        [EnumValueDescription("By Content Description")]
        ByContentDescription,
        [EnumValueDescription("By Text")]
        ByText,
        [EnumValueDescription("By Elements Repository")]
        ByElementsRepository,
        [EnumValueDescription("By Model Name")]
        ByModelName,
        [EnumValueDescription("By CSS Selector")]
        ByCSSSelector,
        [EnumValueDescription("Page Objects Model Element")]
        POMElement,
    }

    public enum eElementType
    {
        [EnumValueDescription("")]
        Unknown,
        [EnumValueDescription("Text Box")]
        TextBox,
        Button,
        Dialog,
        [EnumValueDescription("Combo Box/Drop Down")]
        ComboBox,     // HTML Input Select
        [EnumValueDescription("Combo Box Item/Drop Down Option")]
        ComboBoxOption,    // HTML Input Select
        List,
        ListItem,
        [EnumValueDescription("Table Item")]
        TableItem,
        [EnumValueDescription("Radio Button")]
        RadioButton,
        Table,
        CheckBox,
        Image,
        Label,
        [EnumValueDescription("Menu Item")]
        MenuItem,
        [EnumValueDescription("Menu Bar")]
        MenuBar,
        TreeView,
        Window,
        HyperLink,
        ScrollBar,
        Iframe,
        Canvas,
        Text,
        Tab,
        [EnumValueDescription("Editor Pane")]
        EditorPane,
        //HTML Elements
        Div,
        Span,
        Form
    }


   
}
