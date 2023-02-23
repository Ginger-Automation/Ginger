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

using Amdocs.Ginger.CoreNET.RunLib;
using Ginger.Plugin.Platform.Web.Elements;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Ginger.Plugin.Platform.Web.Execution
{/// <summary>
/// Handles the Browser Action
/// </summary>
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

        public Dictionary<string, object> InputParams {get;set ;}

        public BrowserActionhandler(IWebPlatform mPlatformService)
        {
            PlatformService = mPlatformService;            
            BrowserService = PlatformService.BrowserActions;
        }

        public void ExecuteAction(ref NodePlatformAction platformAction)
        {
        
                InputParams = platformAction.InputParams;
       
        

            string Value = (string)InputParams["Value"];
            List<NodeActionOutputValue> AOVs = new List<NodeActionOutputValue>();

            try
            {
                // use enum or string/const ??? 
                eControlAction ElementAction = (eControlAction)Enum.Parse(typeof(eControlAction), InputParams["ControlAction"].ToString());
                switch (ElementAction)
                {
                    case eControlAction.GotoURL:                    
                        string GotoURLType=String.Empty;

                        if (InputParams.ContainsKey("GotoURLType"))
                        {

                            GotoURLType = (string)InputParams["GotoURLType"];
                        }
                        if (string.IsNullOrEmpty(GotoURLType))
                        {
                            GotoURLType = "Current";

                        }

                        BrowserService.Navigate(Value, GotoURLType);

                  

                  
                        platformAction.exInfo +=  "Navigated to: " + Value;
            
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
                        BrowserService.DismissAlert();
                        break;
                    case eControlAction.DeleteAllCookies:
                        BrowserService.DeleteAllCookies();
                        break;
                    case eControlAction.AcceptMessageBox:

                        BrowserService.AcceptAlert();
                        break;
                    case eControlAction.GetWindowTitle:

                        string Title = BrowserService.GetTitle();
                     
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Title });
                        break;
                    case eControlAction.GetMessageBoxText:

                       string AlertText= BrowserService.GetAlertText();
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = AlertText });
                        break;
                    case eControlAction.SetAlertBoxText:

             
                        string value = (string)InputParams["Value"];
                        BrowserService.SendAlertText(value);
                        break;
                    case eControlAction.SwitchFrame:
                        string ElementLocateBy;
                        string Locatevalue;
                        string mElementType = "";
                        Locatevalue = (string)InputParams["LocateValue"];

                        object elb;
                        InputParams.TryGetValue("ElementLocateBy",out elb);

                        ElementLocateBy= elb!=null? elb.ToString():"";
   
                        if (string.IsNullOrEmpty(ElementLocateBy))
                        {
                            ElementLocateBy = (string)InputParams["LocateBy"];
        
                        }

                        if (string.IsNullOrEmpty(Locatevalue))
                        {
                            Locatevalue = (string)InputParams["Value"];
                  
                        }


                        eElementType ElementType = eElementType.WebElement;
                        _ = Enum.TryParse<eElementType>(mElementType, out ElementType);
                        IGingerWebElement Element = LocateElement(ElementType, ElementLocateBy, Locatevalue);
                        BrowserService.SwitchToFrame(Element);
                        break;
                    case eControlAction.RunJavaScript:
                        string javascript = (string)InputParams["javascript"];
                        object Output = BrowserService.ExecuteScript(javascript);
                        if (Output != null)
                        {
                            platformAction.Output.Add("Actual", Output.ToString());
                        }
                        break;

                    case eControlAction.GetPageSource:

                        string PageSource = BrowserService.GetPageSource();
                        AOVs.Add(new NodeActionOutputValue() { Param = "PageSource", Value = PageSource });

                        break;

                    case eControlAction.InjectJS:

                       

                        break;
                }
            }
            catch(Exception ex)
            {
                platformAction.addError(ex.Message);
              
            }

            finally
            {
                platformAction.Output.OutputValues.AddRange( AOVs);
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
