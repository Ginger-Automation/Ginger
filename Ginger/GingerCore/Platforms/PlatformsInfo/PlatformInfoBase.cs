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

using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;

namespace GingerCore.Platforms.PlatformsInfo
{
    public abstract class PlatformInfoBase: IPlatformInfo
    {
        static JavaPlatform mJavaPlatform = new JavaPlatform();
        static WebPlatform mWebPlatform = new WebPlatform();
        static AndroidPlatform mAndroidPlatform = new AndroidPlatform();
        static PowerBuilderPlatform mPowerBuilderPlatform = new PowerBuilderPlatform();
        static WindowsPlatform mWindowsPlatform = new WindowsPlatform();
        static MobilePlatform mMobilePlatform = new MobilePlatform();
        internal List<eElementType> mElementsTypeList = null;
        internal List<ElementTypeData> mPlatformElementTypeOperations = null;
        internal List<eLocateBy> mElementLocatorsTypeList = null;

        public abstract ePlatformType PlatformType();
        public abstract List<eLocateBy> GetPlatformUIElementLocatorsList();
        public abstract List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations();
        public abstract List<string> GetPlatformUIElementPropertiesList(eElementType ElementType);

        public abstract List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList();

        public abstract List<ActUIElement.eElementAction> GetPlatformUIClickTypeList();
        public abstract List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList();

        public abstract ObservableList<ElementLocator> GetLearningLocators();
        public static PlatformInfoBase GetPlatformImpl(ePlatformType Platform)
        {
            switch (Platform)
            {
                case ePlatformType.AndroidDevice:
                    return mAndroidPlatform;
                case ePlatformType.Java:
                    return mJavaPlatform;
                case ePlatformType.Web:
                    return mWebPlatform;
                case ePlatformType.PowerBuilder:
                    return mPowerBuilderPlatform;
                case ePlatformType.Windows:
                    return mWindowsPlatform;
                case ePlatformType.Mobile:
                    return mMobilePlatform;
                //TODO: add the rest

                default:
                    return null;
            }
        }

        /// <summary>
        /// This method is used to check if the paltform supports POM
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPlatformSupportPOM()
        {
            return false;
        }

        public static List<eElementType> GetPlatformUIElementsList(ePlatformType Platform)
        {
            PlatformInfoBase PB = GetPlatformImpl(Platform);
            return PB.GetPlatformUIElementsType();
        }

        public class ElementTypeData
        {
            public eElementType ElementType;
            public Type ActionType;
            public List<Enum> ElementOperationsList = new List<Enum>();
            private bool mIsCommonElementType;

            public bool IsCommonElementType { get { return mIsCommonElementType;} set { mIsCommonElementType = value; } }
        }

        internal static List<eLocateBy> GetPlatformUIElementLocatorsList(ePlatformType Platform)
        {
            PlatformInfoBase PB = GetPlatformImpl(Platform);
            return PB.GetPlatformUIElementLocatorsList();
        }
        internal static List<string> GetPlatformUIElementPropertiesList(ePlatformType Platform, eElementType ElementType)
        {
            PlatformInfoBase PB = GetPlatformImpl(Platform);
            return PB.GetPlatformUIElementPropertiesList(ElementType);
        }
        internal static List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(ePlatformType Platform, eElementType ElementType)
        {
            PlatformInfoBase PB = GetPlatformImpl(Platform);

            return PB.GetPlatformUIElementActionsList(ElementType);
        }

        public virtual string GetPlatformGenericElementEditControls()
        {
            return null;
        }

        public virtual List<ActUIElement.eElementProperty> GetPlatformElementProperties()
        {
            List<ActUIElement.eElementProperty> elementPropertyList = new List<ActUIElement.eElementProperty>();

            elementPropertyList.Add(ActUIElement.eElementProperty.Enabled);
            elementPropertyList.Add(ActUIElement.eElementProperty.Color);
            elementPropertyList.Add(ActUIElement.eElementProperty.Text);
            elementPropertyList.Add(ActUIElement.eElementProperty.ToolTip);
            elementPropertyList.Add(ActUIElement.eElementProperty.Type);
            elementPropertyList.Add(ActUIElement.eElementProperty.Value);
            elementPropertyList.Add(ActUIElement.eElementProperty.Visible);
            return elementPropertyList;          
        }

        public virtual List<eElementType> GetPlatformUIElementsType()
        {
            if (mElementsTypeList == null)
            {
                mElementsTypeList = new List<eElementType>();
                mElementsTypeList.Add(eElementType.Unknown);
                mElementsTypeList.Add(eElementType.Button);
                mElementsTypeList.Add(eElementType.ScrollBar);
                mElementsTypeList.Add(eElementType.ComboBox);
                mElementsTypeList.Add(eElementType.RadioButton);
                mElementsTypeList.Add(eElementType.TextBox);
                mElementsTypeList.Add(eElementType.CheckBox);
                mElementsTypeList.Add(eElementType.Image);
                mElementsTypeList.Add(eElementType.Label);
                mElementsTypeList.Add(eElementType.List);
                mElementsTypeList.Add(eElementType.Table);
                mElementsTypeList.Add(eElementType.MenuItem);
                mElementsTypeList.Add(eElementType.Window);
                mElementsTypeList.Add(eElementType.Tab);
            }
            return mElementsTypeList;
        }

        public virtual List<eElementType> GetPlatformWidgetsUIElementsType()
        {
            return null;
        }

        public virtual List<ActUIElement.eElementAction> GetPlatformWidgetsUIActionsList(eElementType ElementType)
        {
            return null;
        }

        public virtual ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            ObservableList<Act> UIElementsActionsList = new ObservableList<Act>();
            return UIElementsActionsList;
        }

        public virtual Act GetPlatformActionByElementInfo(ElementInfo elementInfo, ElementActionCongifuration actConfig)
        {           
            return null;
        }

        Act IPlatformInfo.GetPlatformAction(ElementInfo eInfo, ElementActionCongifuration actConfig)
        {
            return GetPlatformActionByElementInfo(eInfo, actConfig);
        }
        
        public virtual List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();

            switch (ElementType)
            {
                case eElementType.Unknown:
                    foreach (ActUIElement.eElementAction item in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                    {
                        if (!item.Equals(ActUIElement.eElementAction.Unknown))
                        {
                            list.Add(item);
                        }
                    }

                    break;
                case eElementType.Button:
                    list.Add(ActUIElement.eElementAction.Click);
                    break;
                case eElementType.TextBox:
                    list.Add(ActUIElement.eElementAction.SetValue);
                    list.Add(ActUIElement.eElementAction.GetValue);
                    break;
                case eElementType.ComboBox:
                    list.Add(ActUIElement.eElementAction.SetText);
                    list.Add(ActUIElement.eElementAction.Click);
                    list.Add(ActUIElement.eElementAction.SetValue);
                    list.Add(ActUIElement.eElementAction.GetText);
                    list.Add(ActUIElement.eElementAction.GetAllValues);
                    break;
                case eElementType.CheckBox:
                    list.Add(ActUIElement.eElementAction.SetValue);
                    break;
                case eElementType.Table:
                    list.Add(ActUIElement.eElementAction.TableAction);
                    list.Add(ActUIElement.eElementAction.TableCellAction);
                    list.Add(ActUIElement.eElementAction.TableRowAction);
                    break;
                case eElementType.ScrollBar:
                    list.Add(ActUIElement.eElementAction.ScrollUp);
                    list.Add(ActUIElement.eElementAction.ScrollDown);
                    list.Add(ActUIElement.eElementAction.ScrollLeft);
                    list.Add(ActUIElement.eElementAction.ScrollRight);
                    break;
                case eElementType.Dialog:
                    list.Add(ActUIElement.eElementAction.AcceptDialog);
                    list.Add(ActUIElement.eElementAction.DismissDialog);
                    break;
            }


            return list;
        }
        public virtual List<ActUIElement.eTableAction> GetTableControlActions(ActUIElement.eElementAction tableAction)
        {
            return null;
        }

        public virtual List<ActUIElement.eSubElementType> GetSubElementType(eElementType elementType)
        {
            return null;
        }

        public virtual List<ActUIElement.eElementAction> GetSubElementAction(ActUIElement.eSubElementType subElementType)
        {
            return null;
        }
    }
}
