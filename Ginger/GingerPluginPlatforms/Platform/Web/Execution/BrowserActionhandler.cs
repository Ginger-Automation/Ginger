using Amdocs.Ginger.CoreNET.RunLib;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;

namespace Ginger.Plugin.Platform.Web.Execution
{
    class BrowserActionhandler:IActionHandler
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

        eControlAction ElementAction;
        internal List<NodeActionOutputValue> outputValues = new List<NodeActionOutputValue>();
        string Value;
        
        IBrowserActions BrowserService = null;

        public string ExecutionInfo { get; set; }
        public string Error { get ; set; }
        readonly IWebPlatform PlatformService;

        PlatformActionData mPlatformAction;

        public BrowserActionhandler(IWebPlatform mPlatformService, PlatformActionData platformAction)
        {
            PlatformService = mPlatformService;
            mPlatformAction = platformAction;

            // InputParams = minputParams;
            BrowserService = PlatformService.BrowserActions;
            // InputParams.TryGetValue("Value", out Value);  // Need to be ValueForDriver !!!
            ElementAction = eControlAction.GotoURL;  // !!!!!!!!!!!!!!!!!!!
            string val = (string)platformAction.InputParams["URL"];

            // ElementAction = (eControlAction)Enum.Parse(typeof(eControlAction), InputParams["ControlAction"]);

            // platformAction
        }


        internal void ExecuteAction()
        {

            try
            {

                switch (ElementAction)
                {

                    case eControlAction.GotoURL:
                        // Console.WriteLine();

                        string GotoURLType;

                        // InputParams.TryGetValue("GotoURLType", out GotoURLType);
                        //GotoURLType = (string)mPlatformAction.InputValues["URLType"];

                        

                        //if (string.IsNullOrEmpty(GotoURLType))
                        //{
                            GotoURLType = "Current";

                        //}

                        string url = (string)mPlatformAction.InputParams["URL"];

                        BrowserService.Navigate(url, GotoURLType);
                        ExecutionInfo += "Navigated to: " + url;
                        

                        break;

                    case eControlAction.GetPageURL:


                        outputValues.Add(new NodeActionOutputValue() { Param = "PageUrl", Value = BrowserService.GetCurrentUrl() });

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

                        BrowserService.SetAlertBoxText(Value);
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

                        object Output = BrowserService.ExecuteScript(Value);
                        if (Output != null)
                        {
                            outputValues.Add(new NodeActionOutputValue() { Param = "Actual", Value = Output.ToString() });
                        }
                        break;
                }
            }


            catch(Exception ex)
            {
                Error = ex.Message;
                ExecutionInfo = ex.StackTrace;
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
