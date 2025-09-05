#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Platforms.PlatformsInfo
{
    public abstract class PlatformInfoBase : IPlatformInfo
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
        internal List<ePosition> mElementPositionList = null;

        public abstract ePlatformType PlatformType();
        public abstract List<eLocateBy> GetPlatformUIElementLocatorsList();
        public abstract List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations();
        public abstract List<string> GetPlatformUIElementPropertiesList(eElementType ElementType);

        public abstract List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList();

        public abstract List<ActUIElement.eElementAction> GetPlatformUIClickTypeList();
        public abstract List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList();

        public abstract ObservableList<ElementLocator> GetLearningLocators();

        public virtual string GetDefaultElementOperation(eElementType ElementTypeEnum)
        {
            return string.Empty;
        }

        public virtual string GetPageUrlRadioLabelText()
        {
            return "URL";
        }
        public virtual string GetNextBtnToolTip()
        {
            return "Go To Page";
        }

        public static PlatformInfoBase GetPlatformImpl(ePlatformType Platform)
        {
            return Platform switch
            {
                //case ePlatformType.AndroidDevice:
                //    return mAndroidPlatform;
                ePlatformType.Java => mJavaPlatform,
                ePlatformType.Web => mWebPlatform,
                ePlatformType.PowerBuilder => mPowerBuilderPlatform,
                ePlatformType.Windows => mWindowsPlatform,
                ePlatformType.Mobile => mMobilePlatform,
                //TODO: add the rest
                _ => null,
            };
        }

        /// <summary>
        /// This method is used to check if the paltform supports POM
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPlatformSupportPOM()
        {
            return false;
        }

        /// <summary>
        /// This method is used to return possible POM elements categories per platform
        /// </summary>
        /// <returns></returns>
        public virtual List<ePomElementCategory> GetPlatformPOMElementCategories()
        {
            return null;
        }

        /// <summary>
        /// This method is used to check if the paltform includes GUI based Application Instance and thus, supports Sikuli based operations
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSikuliSupported()
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
            public List<Enum> ElementOperationsList = [];
            private bool mIsCommonElementType;

            public bool IsCommonElementType { get { return mIsCommonElementType; } set { mIsCommonElementType = value; } }
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
            List<ActUIElement.eElementProperty> elementPropertyList =
            [
                ActUIElement.eElementProperty.Enabled,
                ActUIElement.eElementProperty.Color,
                ActUIElement.eElementProperty.Text,
                ActUIElement.eElementProperty.ToolTip,
                ActUIElement.eElementProperty.Type,
                ActUIElement.eElementProperty.Value,
                ActUIElement.eElementProperty.Visible,
            ];
            return elementPropertyList;
        }

        public virtual List<eElementType> GetPlatformUIElementsType()
        {
            if (mElementsTypeList == null)
            {
                mElementsTypeList =
                [
                    eElementType.Unknown,
                    eElementType.Button,
                    eElementType.ScrollBar,
                    eElementType.ComboBox,
                    eElementType.RadioButton,
                    eElementType.TextBox,
                    eElementType.CheckBox,
                    eElementType.Image,
                    eElementType.Label,
                    eElementType.List,
                    eElementType.Table,
                    eElementType.MenuItem,
                    eElementType.Window,
                    eElementType.Tab,
                ];
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
            ObservableList<Act> UIElementsActionsList = [];
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
            List<ActUIElement.eElementAction> list = [];

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
                    list.Add(ActUIElement.eElementAction.GetValueByOCR);
                    break;
                case eElementType.ComboBox:
                    list.Add(ActUIElement.eElementAction.SetText);
                    list.Add(ActUIElement.eElementAction.Click);
                    list.Add(ActUIElement.eElementAction.SetValue);
                    list.Add(ActUIElement.eElementAction.GetText);
                    list.Add(ActUIElement.eElementAction.GetAllValues);
                    list.Add(ActUIElement.eElementAction.GetValueByOCR);
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

        public abstract Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList();


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

        public abstract List<ePosition> GetElementPositionList();
    }
}
