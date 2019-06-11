using Amdocs.Ginger.CoreNET.RunLib;
using Ginger.Plugin.Platform.Web.Elements;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web.Execution
{/// <summary>
/// Handles the Browser Action
/// </summary>
    class BrowserActionhandler:IActionHandler
    {
        public enum eControlAction
        {

            InitializeBrowser,

            GetPageSource,

            GetPageURL,

            SwitchFrame,

            SwitchToDefaultFrame,

            SwitchToParentFrame,

            Maximize,

            Close,

            SwitchWindow,

            SwitchToDefaultWindow,

            InjectJS,

            CheckPageLoaded,

            OpenURLNewTab,

            GotoURL,

            CloseTabExcept,

            CloseAll,

            Refresh,

            NavigateBack,

            DismissMessageBox,

            DeleteAllCookies,

            AcceptMessageBox,

            GetWindowTitle,

            GetMessageBoxText,

            SetAlertBoxText,

            RunJavaScript
        }
        eControlAction ElementAction;

        internal List<NodeActionOutputValue> AOVs = new List<NodeActionOutputValue>();
        string Value;

        private Dictionary<string, string> InputParams;
        IBrowserActions BrowserService = null;

        public string ExecutionInfo { get; set; }
        public string Error { get ; set; }
        readonly IWebPlatform PlatformService;
        public BrowserActionhandler(IWebPlatform mPlatformService, Dictionary<string, string> minputParams)
        {
            PlatformService = mPlatformService;
            InputParams = minputParams;
            BrowserService = PlatformService.BrowserActions;
            InputParams.TryGetValue("Value", out Value);
            ElementAction = (eControlAction)Enum.Parse(typeof(eControlAction), InputParams["ControlAction"]);


        }


        internal void ExecuteAction()
        {

            try
            {

                switch (ElementAction)
                {

                    case eControlAction.GotoURL:
                        Console.WriteLine();

                        string GotoURLType;

                        InputParams.TryGetValue("GotoURLType", out GotoURLType);

                        if (string.IsNullOrEmpty(GotoURLType))
                        {
                            GotoURLType = "Current";

                        }
     
                        BrowserService.Navigate(Value, GotoURLType);


                        break;

                    case eControlAction.GetPageURL:


                        AOVs.Add(new NodeActionOutputValue() { Param = "PageUrl", Value = BrowserService.GetCurrentUrl() });

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

                        BrowserService.GetTitle();
                        break;
                    case eControlAction.GetMessageBoxText:

                        BrowserService.GetTitle();
                        break;
                    case eControlAction.SetAlertBoxText:

                        BrowserService.SendAlertText(Value);
                        break;
                    case eControlAction.SwitchFrame:
                        string ElementLocateBy;
                        string Locatevalue;
                        string mElementType;
                                           InputParams.TryGetValue("LocateValue", out Locatevalue);
                        InputParams.TryGetValue("ElementLocateBy", out ElementLocateBy);
                        if(string.IsNullOrEmpty(ElementLocateBy))
                        {
                            InputParams.TryGetValue("LocateBy", out ElementLocateBy);
                        }
     
                        if (string.IsNullOrEmpty(Locatevalue))
                        {
                            InputParams.TryGetValue("Value", out Locatevalue);
                        }
                        InputParams.TryGetValue("ElementType", out mElementType);
              
                        eElementType ElementType = eElementType.WebElement;
                            _=Enum.TryParse<eElementType>( mElementType,out ElementType);
                        IGingerWebElement   Element = LocateElement(ElementType, ElementLocateBy, Locatevalue);
                        BrowserService.SwitchToFrame(Element);
                        break;
                    case eControlAction.RunJavaScript:

                        object Output = BrowserService.ExecuteScript(Value);
                        if (Output != null)
                        {
                            AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Output.ToString() });
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
