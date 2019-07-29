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

using Amdocs.Ginger.CoreNET.RunLib;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;

namespace Ginger.Plugin.Platform.Web.Execution
{
    class BrowserActionhandler : IActionHandler
    {
        public enum eControlAction   // name need to be eBrowserAction !!
        {            
            InitializeBrowser,  // ??
            GetPageSource,
            GetPageURL,
            SwitchFrame,  // all Switch* below in one action with param + add switch to top frame
            SwitchToDefaultFrame,
            SwitchToParentFrame,
            Maximize,
            Close,
            SwitchWindow, 
            SwitchToDefaultWindow,
            InjectJS,
            CheckPageLoaded,
            OpenURLNewTab,  // ??
            GotoURL,
            CloseTabExcept, // ??
            CloseAll, 
            Refresh,
            NavigateBack,
            DismissMessageBox,  // ?? need to be in alert
            DeleteAllCookies,
            AcceptMessageBox,  // ?? need to be in alert
            GetWindowTitle,
            GetMessageBoxText, // ?? need to be in alert
            SetAlertBoxText, // ?? need to be in alert
            RunJavaScript // ?? need to be in alert
        }
        
      
        
        IBrowserActions BrowserService = null;

        
        readonly IWebPlatform PlatformService;
        

        public BrowserActionhandler(IWebPlatform mPlatformService)
        {
            PlatformService = mPlatformService;            
            BrowserService = PlatformService.BrowserActions;
        }

        public void ExecuteAction(ref NodePlatformAction platformAction)
        {            
            try
            {
                // use enum or string/const ??? 
                eControlAction ElementAction = (eControlAction)Enum.Parse(typeof(eControlAction), platformAction.ActionType);
                switch (ElementAction)
                {
                    case eControlAction.GotoURL:                    
                        string GotoURLType;

                        // InputParams.TryGetValue("GotoURLType", out GotoURLType);
                        //GotoURLType = (string)mPlatformAction.InputValues["URLType"];

                        //if (string.IsNullOrEmpty(GotoURLType))
                        //{
                            GotoURLType = "Current";

                        //}

                        string url = (string)platformAction.InputParams["URL"];

                        BrowserService.Navigate(url, GotoURLType);
                        platformAction.exInfo +=  "Navigated to: " + url;
                        platformAction.Output.Add("url", url);
                        break;

                    case eControlAction.GetPageURL:
                        platformAction.Output.Add("PageUrl", BrowserService.GetCurrentUrl());
                        break;
                    case eControlAction.Maximize:
                        BrowserService.Maximize();
                        break;
                    case eControlAction.Close:
                        BrowserService.CloseCurrentTab();
                        break;
                    case eControlAction.CloseAll:
                        BrowserService.CloseWindow();
                        break;
                    case eControlAction.Refresh:
                        BrowserService.Refresh();
                        break;
                    case eControlAction.NavigateBack:
                        BrowserService.NavigateBack();
                        break;
                    case eControlAction.DismissMessageBox:
                        BrowserService.DismissMessageBox();
                        break;
                    case eControlAction.DeleteAllCookies:
                        BrowserService.DeleteAllCookies();
                        break;
                    case eControlAction.AcceptMessageBox:
                        BrowserService.AcceptMessageBox();
                        break;
                    case eControlAction.GetWindowTitle:
                        BrowserService.GetTitle();
                        break;
                    case eControlAction.GetMessageBoxText:
                        BrowserService.GetTitle();
                        break;
                    case eControlAction.SetAlertBoxText:
                        string value = (string)platformAction.InputParams["Value"];
                        BrowserService.SetAlertBoxText(value);
                        break;
                    case eControlAction.SwitchFrame:
                        //string ElementLocateBy;
                        //string Locatevalue;
                        //string mElementType;
                        //InputParams.TryGetValue("ElementLocateBy", out ElementLocateBy);
                        //InputParams.TryGetValue("Locatevalue", out Locatevalue);
                        //InputParams.TryGetValue("ElementType", out mElementType);
                        //if(string.IsNullOrEmpty(Locatevalue))
                        //{
                        //    InputParams.TryGetValue("Value", out Locatevalue);
                        //}
                        //eElementType  ElementType = (eElementType)Enum.Parse(typeof(eElementType), mElementType);
                        //IGingerWebElement   Element = LocateElement(ElementType, ElementLocateBy, Locatevalue);
                        //BrowserService.SwitchToFrame(Element);
                        break;
                    case eControlAction.RunJavaScript:
                        string javascript = (string)platformAction.InputParams["javascript"];
                        object Output = BrowserService.ExecuteScript(javascript);
                        if (Output != null)
                        {
                            platformAction.Output.Add("Actual", Output.ToString());
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                platformAction.addError(ex.Message);                
            }
        }
        private IGingerWebElement LocateElement(eElementType ElementType, string ElementLocateBy, string LocateByValue)
        {
            IGingerWebElement Element = null;
            switch (ElementLocateBy)
            {
                case "ByID":
                    Element = PlatformService.LocateWebElement.LocateElementByID(ElementType, LocateByValue);
                    break;
                case "ByCSSSelector":
                case "ByCSS":
                    Element = PlatformService.LocateWebElement.LocateElementByCss(ElementType, LocateByValue);
                    break;
                case "ByLinkText":
                    Element = PlatformService.LocateWebElement.LocateElementByLinkTest(ElementType, LocateByValue);
                    break;
                case "ByXPath":
                    Element = PlatformService.LocateWebElement.LocateElementByXPath(ElementType, LocateByValue);
                    break;
            }
            return Element;
        }


    }
}
