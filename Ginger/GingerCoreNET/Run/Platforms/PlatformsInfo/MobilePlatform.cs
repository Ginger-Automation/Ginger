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
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class MobilePlatform : WebPlatform
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
                mElementsTypeList = base.GetPlatformUIElementsType();//taken from WebPlatform

                //Do Changes from Web
                mElementsTypeList.Remove(eElementType.Window);
            }
            return mElementsTypeList;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {           
            List<ActUIElement.eElementAction> list = base.GetPlatformUIClickTypeList();//taken from WebPlatform

            list.Remove(ActUIElement.eElementAction.MouseClick);
            list.Remove(ActUIElement.eElementAction.MousePressRelease);

            return list;
        }

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList = base.GetPlatformBrowserControlOperations();//taken from WebPlatform

            //Do Changes from Web
            //browserActElementList.Remove(ActBrowserElement.eControlAction.OpenURLNewTab);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.Maximize);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.Close);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.SwitchWindow);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.GetWindowTitle);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.CloseTabExcept);
            //browserActElementList.Remove(ActBrowserElement.eControlAction.CloseAll);

            return browserActElementList;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = base.GetPlatformDragDropTypeList();//taken from WebPlatform

            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list = base.GetPlatformUIValidationTypesList();//taken from WebPlatform

            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> list = base.GetPlatformUIElementActionsList(ElementType);//taken from WebPlatform

            return list;
        }

        public override ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            ObservableList<Act> UIElementsActionsList = base.GetPlatformElementActions(elementInfo);//taken from WebPlatform
           
            return UIElementsActionsList;
        }

        //new public List<ElementTypeData> GetPlatformElementTypesData()
        //{
        //    mPlatformElementTypeOperations = (new WebPlatform()).GetPlatformElementTypesData();//taken from WebPlatform
        //    return mPlatformElementTypeOperations;
        //}
        public override List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = base.GetPlatformElementTypesData();//taken from WebPlatform

                //Changes from Web
                foreach (ElementTypeData etd in mPlatformElementTypeOperations)
                {
                    etd.ElementOperationsList.Remove(ActUIElement.eElementAction.MouseClick);
                    etd.ElementOperationsList.Remove(ActUIElement.eElementAction.MousePressRelease);
                    etd.ElementOperationsList.Remove(ActUIElement.eElementAction.MouseRightClick);

                    if (etd.ElementOperationsList.Contains(ActUIElement.eElementAction.Click) == false)//add click to all elements
                    {
                        etd.ElementOperationsList.Add(ActUIElement.eElementAction.Click);
                    }
                }
                if (mPlatformElementTypeOperations.Where(x => x.ElementType == eElementType.Window).FirstOrDefault() != null)
                {
                    mPlatformElementTypeOperations.Remove(mPlatformElementTypeOperations.Where(x => x.ElementType == eElementType.Window).FirstOrDefault());
                }
                //ElementTypeData List = mPlatformElementTypeOperations.Where(x => x.ElementType == eElementType.List).FirstOrDefault();
                //if (List != null)
                //{
                //    List.ElementOperationsList.Add(ActUIElement.eElementAction.Click);
                //}
            }
            return mPlatformElementTypeOperations;
        }

        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = base.GetPlatformUIElementLocatorsList();//taken from WebPlatform
                mElementLocatorsTypeList.Add(eLocateBy.iOSPredicateString);
                mElementLocatorsTypeList.Add(eLocateBy.iOSClassChain);
            }

            return mElementLocatorsTypeList;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {            
            List<string> list = base.GetPlatformUIElementPropertiesList(ElementType);//taken from WebPlatform
          
            return list;
        }

        public override bool IsPlatformSupportPOM()
        {
            return true;
        }

        public override string GetPageUrlRadioLabelText()
        {
            return "App/URL";
        }

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            //ObservableList<ElementLocator> learningLocatorsList = base.GetLearningLocators();//taken from WebPlatform
            ObservableList<ElementLocator> learningLocatorsList = new ObservableList<ElementLocator>();
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Very Recommended (usually unique)" });

            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.iOSPredicateString, Help = "Highly Recommended as Predicate Matching is built into XCUITest" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByResourceID, Help = "Highly Recommended for Resource-Ids being unique" });

            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByTagName, Help = "Recommended" });
            return learningLocatorsList;
        }

    }
}
