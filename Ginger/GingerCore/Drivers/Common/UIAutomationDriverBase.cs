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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using mshtml;
using GingerCore.Drivers.PBDriver;
using System.Windows.Automation;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers.Common
{
    public abstract  class UIAutomationDriverBase: DriverBase, IXPath
    {
        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        public enum eUIALibraryType
        {
            ComWrapper = 0,
            FlaUI = 1
        }

        public UIAutomationHelperBase mUIAutomationHelper;

        Boolean retryForCOMExceptionDoneFlag = false;

        public eUIALibraryType LibraryType { get; set; }
        
        #region IXPath
        // ----------------------------------------------------------------------------------------------------------------------------
        // IXPath Implementation
        // ----------------------------------------------------------------------------------------------------------------------------
        
        ElementInfo IXPath.GetRootElement()
        {
            return ((IXPath)mUIAutomationHelper).GetRootElement();
        }

        ElementInfo IXPath.UseRootElement()
        {
            return ((IXPath)mUIAutomationHelper).UseRootElement();
        }

        XPathHelper IXPath.GetXPathHelper(ElementInfo ei)
        {
            return ((IXPath)mUIAutomationHelper).GetXPathHelper();
        }

        ElementInfo IXPath.GetElementParent(ElementInfo ElementInfo)
        {
            return ((IXPath)mUIAutomationHelper).GetElementParent(ElementInfo);
        }

        string IXPath.GetElementProperty(ElementInfo ElementInfo, string PropertyName)
        {
            return ((IXPath)mUIAutomationHelper).GetElementProperty(ElementInfo, PropertyName);
        }

        List<ElementInfo> IXPath.GetElementChildren(ElementInfo ElementInfo)
        {
            return ((IXPath)mUIAutomationHelper).GetElementChildren(ElementInfo);
        }

        ElementInfo IXPath.FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            return ((IXPath)mUIAutomationHelper).FindFirst(ElementInfo, conditions);
        }

        List<ElementInfo> IXPath.FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            return ((IXPath)mUIAutomationHelper).FindAll(ElementInfo, conditions);
        }

        ElementInfo IXPath.GetPreviousSibling(ElementInfo EI)
        {
            return ((IXPath)mUIAutomationHelper).GetPreviousSibling(EI);
        }

        ElementInfo IXPath.GetNextSibling(ElementInfo EI)
        {
            return ((IXPath)mUIAutomationHelper).GetNextSibling(EI);
        }
        
        #endregion IXPath

        public void HighLightElement(ElementInfo ElementInfo)
        {            
            if (ElementInfo.GetType() == typeof(UIAElementInfo))
            {
                mUIAutomationHelper.HiglightElement(((UIAElementInfo)ElementInfo));
            }
            else if (ElementInfo.GetType() == typeof(HTMLElementInfo))
            {
                //TODO:Handle mshtml-element & HtmlAgilityPack-node generically
                HTMLElementInfo htmlInfo = (HTMLElementInfo)ElementInfo;
                string elemType = htmlInfo.ElementObject.GetType().ToString();

                if (elemType.Contains("HtmlAgilityPack"))
                {
                    mUIAutomationHelper.GetHTMLHelper().HighLightElement(ElementInfo.ElementObject);
                }
                if (elemType.ToLower().Contains("mshtml"))
                {
                    mUIAutomationHelper.GetHTMLHelper().HighLightElement(((IHTMLElement)ElementInfo.ElementObject));
                }
            }
        }

        public ObservableList<ElementLocator> GetElementLocators(ElementInfo ElementInfo)
        {
            if (ElementInfo.GetType() == typeof(UIAElementInfo))
            {
                UIAElementInfo UIEI = (UIAElementInfo)ElementInfo;

                return mUIAutomationHelper.GetElementLocators(UIEI);
            }
            else if (ElementInfo.GetType().Equals(typeof(HTMLElementInfo)))
            {
                HTMLElementInfo HtmlEI = (HTMLElementInfo)ElementInfo;

                return mUIAutomationHelper.GetHTMLHelper().GetHTMLElementLocators(HtmlEI);
            }
            else
            { return null; }

        }

        public ObservableList<ControlProperty> GetElementProperties(ElementInfo ElementInfo)
        {
            if (ElementInfo.GetType() == typeof(HTMLElementInfo))
            {
                ObservableList<ControlProperty> list = ((HTMLHelper)mUIAutomationHelper.GetHTMLHelper()).GetHTMLElementProperties(ElementInfo);
                return list;
            }
            return null;
        }

        public List<ElementInfo> GetElementChildren(ElementInfo ElementInfo)
        {
            List<ElementInfo> list = new List<ElementInfo>();
            if (mUIAutomationHelper.IsWindowValid(ElementInfo.ElementObject))
            {
                if (ElementInfo.GetType() == typeof(UIAElementInfo))
                {

                    if (mUIAutomationHelper.GetControlPropertyValue(ElementInfo.ElementObject, "ClassName").Equals("Internet Explorer_Server"))
                    {
                        mUIAutomationHelper.InitializeBrowser(ElementInfo.ElementObject);


                        ElementInfo htmlRootEI = new ElementInfo();
                        htmlRootEI.XPath = "/";
                        htmlRootEI.WindowExplorer = ElementInfo.WindowExplorer;

                        list = mUIAutomationHelper.GetHTMLHelper().GetElementChildren(htmlRootEI);
                    }
                    else
                        list = mUIAutomationHelper.GetElementChilderns(ElementInfo.ElementObject);
                }
                else
                {
                    list = mUIAutomationHelper.GetHTMLHelper().GetElementChildren(ElementInfo);
                } 
            }
            
            return list;
        }

        public ElementInfo GetControlFromMousePosition()
        {
            object obj = mUIAutomationHelper.GetElementFromCursor();
            if (obj == null) return null;
            ElementInfo EI = null;

            if (obj.GetType().Equals(typeof(AutomationElement)))
            {
                EI = mUIAutomationHelper.GetElementInfoFor((AutomationElement)obj);
            }

            else
            {
                EI = mUIAutomationHelper.GetHTMLHelper().GetHtmlElementInfo((IHTMLElement)obj);
            }     
            return EI;
        }

        internal void CheckRetrySwitchWindowIsNeeded()
        {
                // Skip the switch window action
                object obj = mUIAutomationHelper.GetCurrentWindow();
                if (obj != null)
                {
                    if (!mUIAutomationHelper.IsWindowValid(obj))
                    {
                        if (mUIAutomationHelper.CurrentWindowRootElement != null && mUIAutomationHelper.CurrentWindowRootElement.ElementName != null)
                            mUIAutomationHelper.SwitchToWindow(mUIAutomationHelper.CurrentWindowRootElement.ElementName);
                    }
                }
        }

        internal void CheckAndRetryRunAction(Act act, Exception e)
        {
            if (!retryForCOMExceptionDoneFlag && mUIAutomationHelper.GetCurrentWindow() != null && mUIAutomationHelper.CurrentWindowRootElement != null && mUIAutomationHelper.CurrentWindowRootElement.ElementName != null)
            {
                retryForCOMExceptionDoneFlag = true;
                mUIAutomationHelper.SwitchToWindow(mUIAutomationHelper.CurrentWindowRootElement.ElementName);
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Retrying the action" + act.GetType() + " Description is" + act.Description);
                RunAction(act);
                retryForCOMExceptionDoneFlag = false;
            }
            else
            {
                act.Error = e.Message;
            }
        }
    }
}
