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

//---------
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.Enums;
using System.Text;
using Amdocs.Ginger.Common.Repository;
using System.Linq;

namespace Amdocs.Ginger.Common.UIElement
{
    /// <summary>
    /// Base class for different Control type for each driver, enable to show unified list in Window Explorer Grid
    /// </summary>
    /// 
    // We can persist ElementInfo - for example when saving DOR Page UIElements, but when used in Window Explorer there is no save
    public class ElementInfo : RepositoryItemBase, IParentOptionalValuesObject
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

        bool mIsAutoLearned;
        [IsSerializedForLocalRepository]
        public bool IsAutoLearned
        {
            get { return mIsAutoLearned; }
            set { mIsAutoLearned = value; OnPropertyChanged(nameof(IsAutoLearned)); }
        }

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
            Unknown,
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
                        return eImageType.Pending;
                    default:
                        return eImageType.Unknown;
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
            // So we keep backward compatibility until all drivers do it correctly
            return mElementTitle;
        }

        string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                mDescription = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public override string ItemName { get { return this.ElementName; } set { this.ElementName = value; } }

        private string mElementName = null;
        [IsSerializedForLocalRepository]
        public string ElementName // element name is given by the user when he maps UI elements and give them name to use in DOR
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
                OnPropertyChanged(nameof(ElementName));
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
            set
            {
                mElementTypeEnum = value;
                OnPropertyChanged(nameof(ElementTypeEnum));
                OnPropertyChanged(nameof(ElementTypeImage));
            }
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

        public eImageType ElementTypeImage
        {
            get
            {
                return GetElementTypeImage(ElementTypeEnum);
            }
        }


        /// <summary>
        /// Please dont use this property it is obselet use the below property "OptionalValuesObjectsList"
        /// </summary>
        List<String> mOptionalValues = new List<string>();
        public List<String> OptionalValues
        {
            get
            {
                return mOptionalValues;
            }
            set
            {
                mOptionalValues = value;
            }
        }

        ObservableList<OptionalValue> mOptionalValuesObjectsList = new ObservableList<OptionalValue>();
        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesObjectsList
        {
            get
            {
                if (mOptionalValuesObjectsList.Count == 0 && mOptionalValues.Count > 0)//backward support copying values from old list
                {
                    foreach (string opVal in mOptionalValues)
                    {
                        mOptionalValuesObjectsList.Add(new OptionalValue() { ItemName = opVal, IsDefault = false });
                    }
                    if (mOptionalValuesObjectsList.Count > 0)
                    {
                        mOptionalValuesObjectsList[0].IsDefault = true;
                    }
                    mOptionalValues = new List<string>();
                }
                return mOptionalValuesObjectsList;
            }
            set
            {
                mOptionalValuesObjectsList = value;
            }
        }

        public string OptionalValuesObjectsListAsString
        {
            get
            {
                StringBuilder opValsString = new StringBuilder();
                foreach (OptionalValue value in OptionalValuesObjectsList)
                {
                    if (value.IsDefault)
                    {
                        opValsString.Append(value.ItemName + "*,");
                    }
                    else
                    {
                        opValsString.Append(value.ItemName + ",");
                    }
                }
                return opValsString.ToString().TrimEnd(',');
            }
        }

        ObservableList<OptionalValue> IParentOptionalValuesObject.OptionalValuesList { get { return OptionalValuesObjectsList; } set { OptionalValuesObjectsList = value; } }

        void IParentOptionalValuesObject.PropertyChangedEventHandler()
        {
            OnPropertyChanged(nameof(OptionalValuesObjectsList));
            OnPropertyChanged(nameof(OptionalValuesObjectsListAsString));
        }

        /// <summary>
        /// This method is used to check the PossibleValues Supported for any type
        /// </summary>
        /// <param name="ei"></param>
        /// <returns></returns>
        public static bool IsElementTypeSupportingOptionalValues(eElementType ei)
        {
            bool supported = false;
            if (ei == eElementType.TextBox || ei == eElementType.Text ||
                ei == eElementType.ComboBox || ei == eElementType.ComboBoxOption ||
                ei == eElementType.List || ei == eElementType.ListItem)
            {
                supported = true;
            }
            return supported;
        }


        // Used for Lazy loading when possible
        public virtual string GetElementType()
        {
            // we return ElementType unless it was overridden as expected
            // So we keep backward compatibility until all drivers do it correctly
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
            // So we keep backward compatibility until all drivers do it correctly
            return mValue;
        }


        public object mElementGroup;
        public object ElementGroup
        {
            get
            {
                return mElementGroup;
            }
            set
            {
                if (mElementGroup != value)
                {
                    mElementGroup = value;
                    OnPropertyChanged(nameof(ElementGroup));
                }
            }
        }


        [IsSerializedForLocalRepository]
        public string Path { get; set; }

        private string mXPath;

        [IsSerializedForLocalRepository]
        public string XPath
        {
            get
            {
                if (mXPath == null)
                {
                    mXPath = GetAbsoluteXpath();
                }
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
            // So we keep backward compatibility until all drivers do it correctly
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

        public static eImageType GetElementTypeImage(eElementType elementType = eElementType.Unknown)
        {
            switch (elementType)
            {
                case eElementType.Button:
                    return eImageType.Button;
                case eElementType.CheckBox:
                    return eImageType.CheckBox;
                case eElementType.ComboBox:
                    return eImageType.DropList;
                case eElementType.ComboBoxOption:
                    return eImageType.List;
                case eElementType.HyperLink:
                    return eImageType.Link;
                case eElementType.Image:
                    return eImageType.Image;
                case eElementType.Label:
                    return eImageType.Label;
                case eElementType.List:                
                    return eImageType.List;
                case eElementType.ListItem:
                    return eImageType.List;
                case eElementType.MenuBar:
                case eElementType.MenuItem:
                    return eImageType.Menu;
                case eElementType.RadioButton:
                    return eImageType.RadioButton;
                case eElementType.Table:
                case eElementType.TableItem:
                    return eImageType.Table;
                case eElementType.Text:
                    return eImageType.Label;
                case eElementType.TextBox:
                    return eImageType.TextBox;
                case eElementType.Window:
                case eElementType.Dialog:
                    return eImageType.Window;                
            }

            return eImageType.Element;
        }
    }

        public enum eLocateBy
        {
            [EnumValueDescription("NA")]
            NA,
            [EnumValueDescription("")]
            Unknown,
            [EnumValueDescription("An Page Objects Model Element")]
            POMElement,
            [EnumValueDescription("By ID")]
            ByID,
            [EnumValueDescription("By Name")]
            ByName,
            [EnumValueDescription("By CSS")]
            ByCSS,
            [EnumValueDescription("By XPath")]
            ByXPath,
            [EnumValueDescription("By Relative XPath")]
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
            [EnumValueDescription("Tree View")]
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
            [EnumValueDescription("Editor Table")]
            EditorTable,
            //HTML Elements
            Div,
            Span,
            Form,
            Browser,
            [EnumValueDescription("Date Picker")]
            DatePicker
        }

    
}
