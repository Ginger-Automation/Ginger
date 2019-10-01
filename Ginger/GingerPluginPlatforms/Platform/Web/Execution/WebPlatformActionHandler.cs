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
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform.Web.Elements;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Ginger.Plugin.Platform.Web.Execution
{
    /// <summary>
    /// Default Implementation of IPlatformActionHandler for WebPlatform. A Plugin Implementing IwebPlatform can use this.
    /// </summary>
    public class WebPlatformActionHandler : IPlatformActionHandler
    {

        /// <summary>
        /// Tells to shift Frames automatically in case of a POM Element
        /// </summary>
        bool AutomaticallyShiftIframe { get; set; }


        /// <summary>
        /// It Takes an instance of IWebPlatform(ehich extends IPlatformService ) and Action payload and call the required functions for execution.
        /// Supported actions are Browser Action and Ui Element action with Page object Model Support
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="platformAction"></param>
        /// <returns></returns>
        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            // add try catch !!!!!!!!!!


            IWebPlatform webPlatformService = (IWebPlatform)service;



            switch (platformAction.ActionType)
            {
                case "BrowserAction":
                    //TODO: cache
                    BrowserActionhandler Handler = new BrowserActionhandler(webPlatformService);
                    Handler.ExecuteAction(ref platformAction);
                    break;
                case "UIElementAction":

                    UIELementActionHandler Handler2 = new UIELementActionHandler(webPlatformService);
                    Handler2.ExecuteAction(ref platformAction);
                    break;
                case "SmartSyncAction":

                    ExecuteSmartSyncAction(webPlatformService, ref platformAction);

                    break;
                default:
                    platformAction.error += "HandleRunAction: handler not found: ";
                    break;
            }


        }


        private NewPayLoad CreateActionResult(NodePlatformAction platformAction)
        {
            return GingerNode.CreateActionResult(platformAction.exInfo, platformAction.error, null);              // platformAction.outputValues
        }


        #region ActionHandlers
        private void ExecuteSmartSyncAction(IWebPlatform webPlatformService, ref NodePlatformAction platformAction)
        {
            Dictionary<string, object> InputParams = platformAction.InputParams;

            int MaxTimeout = Int32.Parse(InputParams.ContainsKey("WaitTime") ? InputParams["WaitTime"].ToString() : (string.IsNullOrEmpty(InputParams["Value"].ToString()) ? "5" : InputParams["Value"].ToString()));

            string SmartSyncAction = InputParams["SmartSyncAction"] as string;

            string LocateValue= InputParams["LocateValue"] as string;

            string LocateBy = InputParams["LocateBy"] as string;
            eElementType ElementType = eElementType.WebElement;

            IGingerWebElement WebElement = null;
            Stopwatch st = new Stopwatch();

            switch (SmartSyncAction)
            {
                case "WaitUntilDisplay":
                    st.Reset();
                    st.Start();
                    WebElement= LocateElement(ref ElementType, LocateBy, LocateValue, webPlatformService);

                    while (!(WebElement != null && (WebElement.IsVisible() || WebElement.IsEnabled())))
                    {
                        Task.Delay(100);
                        WebElement = LocateElement(ref ElementType, LocateBy, LocateValue, webPlatformService);

                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                        {
                            platformAction.addError("Smart Sync of WaitUntilDisplay is timeout");
                            break;
                        }
                    }
                    break;

                case "WaitUntilDisapear":
                    st.Reset();
                    st.Start();

                    WebElement = LocateElement(ref ElementType, LocateBy, LocateValue, webPlatformService);

                    if (WebElement == null)
                    {
                        return;
                    }
                    else
                    {
                        st.Start();

                        while (WebElement != null && WebElement.IsVisible())
                        {
                            Task.Delay(100);
                            WebElement = LocateElement(ref ElementType, LocateBy, LocateValue, webPlatformService);
                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                platformAction.addError("Smart Sync of WaitUntilDisapear is timeout");
                                break;
                            }
                        }

                    }
                    break;

                default:
                    platformAction.error = "Smart Sync " + SmartSyncAction + "Action Not found";
                    break;

            }


        }


       internal static IGingerWebElement LocateElement(ref eElementType ElementType, string ElementLocateBy, string LocateByValue, IWebPlatform mPlatformService)
        {
            IGingerWebElement Element = null;
            try
            {
                switch (ElementLocateBy)
                {
                    case "ByID":
                        Element = mPlatformService.LocateWebElement.LocateElementByID(ElementType, LocateByValue);
                        break;
                    case "ByCSSSelector":
                    case "ByCSS":
                        Element = mPlatformService.LocateWebElement.LocateElementByCss(ElementType, LocateByValue);
                        break;
                    case "ByLinkText":
                        Element = mPlatformService.LocateWebElement.LocateElementByLinkTest(ElementType, LocateByValue);
                        break;
                    case "ByName":
                        Element = mPlatformService.LocateWebElement.LocateElementByName(ElementType, LocateByValue);
                        break;
                    case "ByRelXPath":
                    case "ByXPath":
                        Element = mPlatformService.LocateWebElement.LocateElementByXPath(ElementType, LocateByValue);
                        break;
                }

                if (Element != null && (ElementType == eElementType.WebElement || ElementType == eElementType.Unknown))
                {
                    if (Element is IButton)
                    {
                        ElementType = eElementType.Button;
                    }
                    else if (Element is ICanvas)
                    {
                        ElementType = eElementType.Canvas;
                    }
                    else if (Element is ICheckBox)
                    {
                        ElementType = eElementType.CheckBox;
                    }
                    else if (Element is IComboBox)
                    {
                        ElementType = eElementType.ComboBox;
                    }
                    else if (Element is IDiv)
                    {
                        ElementType = eElementType.Div;
                    }
                    else if (Element is IHyperLink)
                    {
                        ElementType = eElementType.HyperLink;
                    }
                    else if (Element is IImage)
                    {
                        ElementType = eElementType.Image;
                    }
                    else if (Element is ILabel)
                    {
                        ElementType = eElementType.Label;
                    }
                    else if (Element is IWebList)
                    {
                        ElementType = eElementType.List;
                    }
                    else if (Element is IRadioButton)
                    {
                        ElementType = eElementType.RadioButton;
                    }
                    else if (Element is ISpan)
                    {
                        ElementType = eElementType.Span;
                    }
                    else if (Element is ITable)
                    {
                        ElementType = eElementType.Table;
                    }
                    else if (Element is ITextBox)
                    {
                        ElementType = eElementType.TextBox;
                    }
                }
            }

            catch
            {

            }
            return Element;
        }
        #endregion


    }
}