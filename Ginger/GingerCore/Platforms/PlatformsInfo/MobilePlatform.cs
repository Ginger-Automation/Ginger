using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class MobilePlatform : PlatformInfoBase
    {
        public override ePlatformType PlatformType()
        {
            return ePlatformType.Mobile;
        }

        public override List<eElementType> GetPlatformUIElementsType()
        {
            // We cache the results
            if (mElementsTypeList == null)
            {
                mElementsTypeList = GetPlatformElementTypesData().Where(y => y.ActionType == typeof(ActUIElement)).Select(z => z.ElementType).ToList();
            }
            return mElementsTypeList;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();

            list.Add(ActUIElement.eElementAction.Click);

            return list;
        }

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList = new List<ActBrowserElement.eControlAction>();

            browserActElementList.Add(ActBrowserElement.eControlAction.GotoURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.AcceptMessageBox);
            browserActElementList.Add(ActBrowserElement.eControlAction.OpenURLNewTab);
            browserActElementList.Add(ActBrowserElement.eControlAction.Refresh);
            browserActElementList.Add(ActBrowserElement.eControlAction.RunJavaScript);
            browserActElementList.Add(ActBrowserElement.eControlAction.Maximize);
            browserActElementList.Add(ActBrowserElement.eControlAction.Close);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToDefaultFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToParentFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchWindow);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetWindowTitle);
            browserActElementList.Add(ActBrowserElement.eControlAction.DeleteAllCookies);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageSource);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.InjectJS);
            browserActElementList.Add(ActBrowserElement.eControlAction.CheckPageLoaded);
            browserActElementList.Add(ActBrowserElement.eControlAction.CloseTabExcept);
            browserActElementList.Add(ActBrowserElement.eControlAction.CloseAll);
            browserActElementList.Add(ActBrowserElement.eControlAction.NavigateBack);
            browserActElementList.Add(ActBrowserElement.eControlAction.DismissMessageBox);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetMessageBoxText);
            browserActElementList.Add(ActBrowserElement.eControlAction.SetAlertBoxText);

            return browserActElementList;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = new List<ActUIElement.eElementDragDropType>();

            list.Add(ActUIElement.eElementDragDropType.DragDropJS);
            list.Add(ActUIElement.eElementDragDropType.DragDropSelenium);
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();

            list.Add(ActUIElement.eElementAction.IsEnabled);
            list.Add(ActUIElement.eElementAction.IsVisible);

            return list;
        }


        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            ElementTypeData elementTypeOperations = GetPlatformElementTypesData().Where(x => x.ElementType == ElementType).FirstOrDefault();
            if (elementTypeOperations != null)
            {
                if (elementTypeOperations.ActionType == typeof(ActUIElement))
                {
                    elementTypeOperations.ElementOperationsList.ForEach(z => list.Add((ActUIElement.eElementAction)(object)z));
                }
            }
            return list;
        }

        public override ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            ObservableList<Act> UIElementsActionsList = new ObservableList<Act>();

            ElementTypeData elementTypeOperations = GetPlatformElementTypesData().Where(x => x.ElementType == elementInfo.ElementTypeEnum).FirstOrDefault();
            if ((elementTypeOperations != null) && ((elementTypeOperations.ElementOperationsList != null)) && (elementTypeOperations.ElementOperationsList.Count > 0))
            {
                if (elementTypeOperations.ActionType == typeof(ActBrowserElement))
                {
                    elementTypeOperations.ElementOperationsList.ForEach(z => UIElementsActionsList.Add
                        (new ActBrowserElement()
                        {
                            Description = ((EnumValueDescriptionAttribute[])typeof(ActBrowserElement.eControlAction).GetField(z.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false)).ToString() + elementInfo.ElementTitle,
                            ControlAction = (ActBrowserElement.eControlAction)System.Enum.Parse(typeof(ActBrowserElement.eControlAction), z.ToString()),
                            Value = "true"
                        }));
                }
                else
                {
                    if (elementTypeOperations.ActionType == typeof(ActUIElement))
                    {
                        elementTypeOperations.ElementOperationsList.ForEach(z => UIElementsActionsList.Add
                            (new ActUIElement()
                            {
                                Description = ((EnumValueDescriptionAttribute[])typeof(ActUIElement.eElementAction).GetField(z.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription + " : " + elementInfo.ElementTitle,
                                ElementAction = (ActUIElement.eElementAction)z,
                                ElementType = elementInfo.ElementTypeEnum,
                            }));
                    }
                }
            }
            return UIElementsActionsList;
        }




        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = new List<ElementTypeData>();

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Button,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>(){   ActUIElement.eElementAction.Click
                                                                },

                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.CheckBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click
                                                                 },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TextBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SendKeys,
                                                                ActUIElement.eElementAction.GetValue
                                                                },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ComboBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SelectByIndex,
                                                                ActUIElement.eElementAction.SelectByText
                                                                }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ScrollBar,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>()
                    {  ActUIElement.eElementAction.ScrollUp, 
                       ActUIElement.eElementAction.ScrollDown,
                       ActUIElement.eElementAction.ScrollLeft,
                       ActUIElement.eElementAction.ScrollRight
                    }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.HyperLink,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.GetValue}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Label,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue}
                });


                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.RadioButton,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Image,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.Click }
                });


                // adding generic/common actions per each ElementType
                List<Enum> ElementCommonActionsList = new List<Enum>();

                mPlatformElementTypeOperations.Where(y => y.ActionType == typeof(ActUIElement)).ToList()
                                                 .ForEach(z => z.ElementOperationsList = z.ElementOperationsList.Union(ElementCommonActionsList).ToList());


                //------Must be last one for calculating all supported Element operations
                List<Enum> allSupportedOperations = new List<Enum>();
                foreach (ElementTypeData elemData in mPlatformElementTypeOperations)
                {
                    foreach (ActUIElement.eElementAction operation in elemData.ElementOperationsList)
                    {
                        if (!allSupportedOperations.Contains(operation))
                        {
                            allSupportedOperations.Add(operation);
                        }
                    }
                }
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Unknown,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = allSupportedOperations
                });
            }

            return mPlatformElementTypeOperations;
        }


        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            // We cache the results
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.POMElement);
                mElementLocatorsTypeList.Add(eLocateBy.ByModelName);
                mElementLocatorsTypeList.Add(eLocateBy.ByID);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSS);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByXY);
                mElementLocatorsTypeList.Add(eLocateBy.ByMulitpleProperties);
                mElementLocatorsTypeList.Add(eLocateBy.NA);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.Unknown);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByContainerName);
                mElementLocatorsTypeList.Add(eLocateBy.ByHref);
                mElementLocatorsTypeList.Add(eLocateBy.ByLinkText);
                mElementLocatorsTypeList.Add(eLocateBy.ByValue);
                mElementLocatorsTypeList.Add(eLocateBy.ByIndex);
                mElementLocatorsTypeList.Add(eLocateBy.ByAutomationID);
                mElementLocatorsTypeList.Add(eLocateBy.ByLocalizedControlType);
                mElementLocatorsTypeList.Add(eLocateBy.ByBoundingRectangle);
                mElementLocatorsTypeList.Add(eLocateBy.IsEnabled);
                mElementLocatorsTypeList.Add(eLocateBy.IsOffscreen);
                mElementLocatorsTypeList.Add(eLocateBy.ByTitle);
                mElementLocatorsTypeList.Add(eLocateBy.ByCaretPosition);
                mElementLocatorsTypeList.Add(eLocateBy.ByUrl);
                mElementLocatorsTypeList.Add(eLocateBy.ByngModel);
                mElementLocatorsTypeList.Add(eLocateBy.ByngRepeat);
                mElementLocatorsTypeList.Add(eLocateBy.ByngBind);
                mElementLocatorsTypeList.Add(eLocateBy.ByngSelectedOption);
                mElementLocatorsTypeList.Add(eLocateBy.ByResourceID);
                mElementLocatorsTypeList.Add(eLocateBy.ByContentDescription);
                mElementLocatorsTypeList.Add(eLocateBy.ByText);
                mElementLocatorsTypeList.Add(eLocateBy.ByElementsRepository);
                mElementLocatorsTypeList.Add(eLocateBy.ByModelName);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSSSelector);
            }
            return mElementLocatorsTypeList;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type

            List<string> list = new List<string>();

            //TODO: map all missing HTML tags and common attrs

            // add attr which exist for all HTML tags
            list.Add("id");
            list.Add("name");
            list.Add("TagName");
            list.Add("class");

            // Per element add the attr 
            switch (ElementType)
            {
                case eElementType.TextBox:
                    list.Add("text");
                    list.Add("enabled");
                    list.Add("value");
                    break;
                case eElementType.ComboBox:
                    list.Add("Options");
                    break;
                case eElementType.HyperLink:
                    list.Add("href");
                    break;
                case eElementType.Image:
                    list.Add("src");
                    list.Add("width");
                    list.Add("height");
                    break;
                default:
                    break;

            }
            return list;
        }
    }
}
