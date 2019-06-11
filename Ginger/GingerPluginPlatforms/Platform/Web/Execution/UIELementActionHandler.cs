using Amdocs.Ginger.CoreNET.RunLib;
using Ginger.Plugin.Platform.Web.Actions;
using Ginger.Plugin.Platform.Web.Elements;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Ginger.Plugin.Platform.Web.Execution
{
    /// <summary>
    /// Class having logic for converting the payloads and calling relevent functions from different element Interfaces to execute actions
    /// </summary>
    public class UIELementActionHandler : IActionHandler
    {
        /// <summary>
        /// Enum Having all the possible Element Action
        /// </summary>
        private enum eElementAction
        {
         
            GetValue,
            Click,
            SetValue,
            GetTextLength,
            SendKeys,
            SetText,
            GetFont,
            ClearValue,
            IsValuePopulated,
            GetValidValues,
            Select,
            SelectByIndex,
            SelectByText,
            GetText,
            DrawObject,
            Submit,
            JavaScriptClick,
            MouseClick,
            ClickAndValidate,
            DragDrop,
            GetAttrValue,
            GetHeight,
            GetItemCount,
            GetSize,
            GetStyle,
            GetWidth,
            Hover,
            IsEnabled,
            IsVisible,
            MouseRightClick,
            RunJavaScript,
            SetFocus,
            ScrollToElement,
            GetSelectedValue
        }

        #region PomProperties
        private bool IsPOM;
        List<string> FrameXpaths = new List<string>();

        List<KeyValuePair<string, string>> Locators = new List<KeyValuePair<string, string>>();
        #endregion


        #region commonproperties
        string mElementType = string.Empty;


        string ElementLocateBy = string.Empty;
        string LocateByValue = string.Empty;
        eElementAction ElementAction;
        eElementType ElementType;
        #endregion


        string Value;
        readonly IWebPlatform PlatformService;
        Dictionary<string, string> InputParams;
        internal List<NodeActionOutputValue> AOVs = new List<NodeActionOutputValue>();
        /// <summary>
        /// Execution details are stored in this.
        /// </summary>
        public string ExecutionInfo { get; set; }
        /// <summary>
        /// If an error happens during execution you should store in this proeprty and it will be 
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// Instantiate the UIElementAction Handler with wervice implementing IWebplatform and Dictionary of InputParams
        /// </summary>
        /// <param name="mplatformService"></param>
        /// <param name="mInputParams"></param>
        public UIELementActionHandler(IWebPlatform mplatformService, Dictionary<string, string> mInputParams)
        {
            PlatformService = mplatformService;

            InputParams = mInputParams;


        }

        private void PreparePOMforExecution(NewPayLoad pomPayload)
        {
            if (pomPayload != null)
            {
                IsPOM = true;

                mElementType = pomPayload.GetValueString();
                //handleAutoShiftFrame 

                List<NewPayLoad> FrameXpathsPayload = pomPayload.GetListPayLoad();

                foreach (NewPayLoad framePathPayload in FrameXpathsPayload)
                {
                    FrameXpaths.Add(framePathPayload.GetValueString());
                }

                //addlocators 

                List<NewPayLoad> locatorsPayload = pomPayload.GetListPayLoad();
                foreach (NewPayLoad locatorpayload in locatorsPayload)
                {
                    KeyValuePair<string, string> locator = new KeyValuePair<string, string>(locatorpayload.GetValueString(), locatorpayload.GetValueString());
                    Locators.Add(locator);
                }
            }

        }
        internal void PrepareforExecution(NewPayLoad pomPayload)
        {
            PreparePOMforExecution(pomPayload);


            if (!IsPOM)
            {
                InputParams.TryGetValue("ElementType", out mElementType);

                InputParams.TryGetValue("ElementLocateBy", out ElementLocateBy);
            }


            _ = Enum.TryParse<eElementType>(mElementType, out ElementType);

            string mElementAction;
            InputParams.TryGetValue("ElementAction", out mElementAction);

            ElementAction = (eElementAction)Enum.Parse(typeof(eElementAction), mElementAction);



            InputParams.TryGetValue("Value", out Value);



        }





        internal void ExecuteAction()
        {

            try
            {
                string Locatevalue = InputParams["ElementLocateValue"];

                IGingerWebElement Element = null;

                LocateByValue = Locatevalue;

                if (IsPOM)
                {
                    Element = LocateElementByPom(ref ElementType);
                }
                else
                {


                    Element = LocateElement(ref ElementType, ElementLocateBy, Locatevalue);
                }
                bool ActionPerformed = PerformCommonActions(Element);


                if (!ActionPerformed)
                {

                    switch (ElementType)
                    {
                        case eElementType.Button:
                            ButtonActions(Element);
                            break;
                        case eElementType.Canvas:

                            CanvasAction(Element);
                            break;
                        case eElementType.CheckBox:
                            CheckBoxActions(Element);
                            break;
                        case eElementType.ComboBox:
                            ComboBoxActions(Element);
                            break;
                        case eElementType.Div:
                            DivActions(Element);
                            break;
                        case eElementType.Image:
                            ImageActions(Element);
                            break;
                        case eElementType.Label:
                            LabelActions(Element);
                            break;
                        case eElementType.List:
                            ListActions(Element);
                            break;
                        case eElementType.RadioButton:
                            RadioButtonActions(Element);
                            break;
                        case eElementType.Span:
                            SpanActions(Element);
                            break;
                        case eElementType.Table:
                            TableActions(Element);
                            break;
                        case eElementType.TextBox:
                            TextBoxActions(Element);
                            break;
                        case eElementType.HyperLink:
                            HyperLinkActions(Element, ElementAction);
                            break;


                    }
                }
            }

            catch (Exception Ex)
            {
                Error = Ex.Message;
                ExecutionInfo = Ex.StackTrace;
            }
        }

        private IGingerWebElement LocateElementByPom(ref eElementType elementType)
        {
            IGingerWebElement pomelement = null;
            if (PlatformService.AutomaticallyShiftIframe)
            {
                AutomaticSwitchFrame();
            }

            foreach (KeyValuePair<string, string> locator in Locators)
            {
                pomelement = LocateElement(ref elementType, locator.Key, locator.Value);
                if (pomelement != null)
                {
                    break;
                }
            }

            return pomelement;
        }

        private void AutomaticSwitchFrame()
        {

#warning implemen Automatic switch frame        
            throw new NotImplementedException();
        }

        private IGingerWebElement LocateElement(ref eElementType ElementType, string ElementLocateBy, string LocateByValue)
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

                case "ByName":
                    Element = PlatformService.LocateWebElement.LocateElementByName(ElementType, LocateByValue);

                    break;
                case "ByRelXPath":
                case "ByXPath":
                    Element = PlatformService.LocateWebElement.LocateElementByXPath(ElementType, LocateByValue);

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


            return Element;
        }

        private void HyperLinkActions(IGingerWebElement element, eElementAction mElementAction)
        {
            if (element is IHyperLink Hyperlink)
            {
                if (element is IClick ClickElement)
                {
                    ClickActions(ClickElement, ElementAction);
                }
                else
                {

                    switch (mElementAction)
                    {


                        case eElementAction.GetValue:
                            AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Hyperlink.GetValue() });

                            break;





                    }
                }
            }
        }
        IGingerWebElement GetValidationElement()
        {
            string ValidationElementLocateBy = InputParams["ValidationElementLocateBy"];
            string ValidationElementLocatorValue = InputParams["ValidationElementLocatorValue"];

            string mValidationElement = InputParams["ValidationElement"];
            eElementType validationElementType = (eElementType)Enum.Parse(typeof(eElementType), mElementType);
            IGingerWebElement ValidationElement = LocateElement(ref validationElementType, ValidationElementLocateBy, ValidationElementLocatorValue);
            return ValidationElement;
        }
        private void TableActions(IGingerWebElement element)
        {
            if (element is ITable Element)
            {
                switch (ElementAction)
                {
                    case eElementAction.Click:
                        Element.Click();
                        break;

                    case eElementAction.GetValue:
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.GetValue() });

                        break;
                    case eElementAction.SetValue:
                        Element.SetValue(Value);

                        break;
                }
            }



        }

        private void TextBoxActions(IGingerWebElement element)
        {

            ITextBox TextBox = (ITextBox)element;
            switch (ElementAction)
            {
                case eElementAction.SetValue:

                    TextBox.SetValue(Value);
                    break;
                case eElementAction.GetValue:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = TextBox.GetValue() });

                    break;

                case eElementAction.GetTextLength:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = TextBox.GetTextLength() });

                    break;
                case eElementAction.SendKeys:

                    TextBox.SendKeys(Value);
                    break;
                case eElementAction.SetText:

                    TextBox.SendKeys(Value);
                    break;

                case eElementAction.GetFont:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = TextBox.GetFont() });

                    break;
                case eElementAction.ClearValue:

                    TextBox.ClearValue();
                    break;
                case eElementAction.IsValuePopulated:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = TextBox.IsValuePopulated() });

                    break;
            }


        }

        private void SpanActions(IGingerWebElement element)
        {
            if (element is ISpan SpanElement)
            {
                switch (ElementAction)
                {
                    case eElementAction.SetValue:

                        SpanElement.SetValue(Value);
                        break;
                    case eElementAction.GetValue:
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = SpanElement.GetValue() });

                        break;
                    default:
                        Error = String.Format("Action {0} not found", ElementAction.ToString());
                        break;
                }
            }
        }

        private void RadioButtonActions(IGingerWebElement element)
        {
            switch (ElementAction)
            {

                case eElementAction.Click:
                case eElementAction.JavaScriptClick:
                case eElementAction.MouseClick:
                case eElementAction.MouseRightClick:

                    ClickActions(element as IClick, ElementAction);
                    break;

                case eElementAction.GetValue:

                    if (element is IRadioButton GetElementValue)
                    {
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = GetElementValue.GetValue() });

                    }

                    break;
                default:

                    Error = "Not Supported Action";
                    break;
            }


        

   

        }

        private void ListActions(IGingerWebElement element)
        {
            if (element is IWebList Element)
            {
                string ValueToSelect;
                InputParams.TryGetValue("ValueToSelect", out ValueToSelect);
                switch (ElementAction)
                {
                    case eElementAction.ClearValue:
                        Element.ClearValue();
                        break;
                    case eElementAction.GetValidValues:
                        Element.GetValidValue();
                        break;
                    case eElementAction.IsValuePopulated:

                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.IsValuePopulated() });
                        break;
                    case eElementAction.GetValue:

                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.GetValue() });
                        break;

                    case eElementAction.Select:
                        Element.Select(ValueToSelect);
                        break;
                    case eElementAction.SelectByIndex:
                        Element.SelectByIndex(0);
                        break;
                    case eElementAction.SelectByText:
                        Element.SelectByText(Value);
                        break;

                }
            }
        }

        private void LabelActions(IGingerWebElement element)
        {
            if (element is ILabel Label)
            {
                switch (ElementAction)
                {

                    case eElementAction.GetFont:


                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Label.GetFont() });

                        break;
                    case eElementAction.GetText:

                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Label.GetText() });

                        break;

                    case eElementAction.GetValue:
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Label.Getvalue() });
                        break;
                }
            }
        }

        private void ImageActions(IGingerWebElement element)
        {
            throw new NotImplementedException();
        }

        private void DivActions(IGingerWebElement element)
        {
            throw new NotImplementedException();
        }


        private void ComboBoxActions(IGingerWebElement element)
        {

            if (element is IComboBox Element)
            {
                string ValueToSelect;
                InputParams.TryGetValue("ValueToSelect", out ValueToSelect);
                switch (ElementAction)
                {
                    case eElementAction.ClearValue:
                        Element.ClearValue();
                        break;
                    case eElementAction.GetValidValues:
                        Element.GetValidValue();
                        break;
                    case eElementAction.IsValuePopulated:

                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.IsValuePopulated() });
                        break;
                    case eElementAction.GetSelectedValue:
                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.GetSelectedValue() });
                        break;

                    case eElementAction.Select:
                        Element.Select(ValueToSelect);
                        break;
                    case eElementAction.SelectByIndex:
                        Element.SelectByIndex(0);
                        break;
                    case eElementAction.SelectByText:
                    case eElementAction.SetValue:
                        Element.SelectByText(Value);


                        break;

                }
            }
        }

        private void CheckBoxActions(IGingerWebElement element)
        {
            if (element is IClick ClickElement)
            {
                ClickActions(ClickElement, ElementAction);
            }
            else
            {
                if (element is ICheckBox Hyperlink)
                {
                    switch (ElementAction)
                    {
                        case eElementAction.GetValue:
                            AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Hyperlink.GetValue() });

                            break;
                    }
                }

            }
        }

        private void CanvasAction(IGingerWebElement element)
        {
            if (element is ICanvas E)
            {

                switch (ElementAction)
                {
                    case eElementAction.DrawObject:
                        E.DrawObject();
                        break;


                }



            }
        }

        private void ButtonActions(IGingerWebElement element)
        {
            if (element is IButton Element)
            {
                string ValueToSelect;
                InputParams.TryGetValue("ValueToSelect", out ValueToSelect);

                if (ElementAction.ToString().ToUpper().Contains("CLICK") && element is IClick ClickElement)
                {
                    ClickActions(ClickElement, ElementAction);
                }
                else
                {
                    switch (ElementAction)
                    {

                        case eElementAction.Submit:
                            Element.Submit();
                            break;
                        case eElementAction.GetValue:
                            AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Element.GetValue() });
                            break;


                    }
                }
            }
        }

        private void ClickActions(IClick Element, eElementAction ClickElementAction)
        {

            switch (ClickElementAction)
            {
                case eElementAction.Click:
                    Element.Click();
                    break;
                case eElementAction.JavaScriptClick:
                    Element.JavascriptClick();
                    break;
                case eElementAction.MouseClick:

                    Element.MouseClick();
                    break;

                case eElementAction.Select:
                    Element.DoubleClick();
                    break;
                case eElementAction.ClickAndValidate:
                    Element.Click();
                    string ValidationType = InputParams["ValidationType"];
                    string mClickType = InputParams["ClickType"];
                    eElementAction ClickType = (eElementAction)Enum.Parse(typeof(eElementAction), mClickType);

                    ClickActions(Element, ClickType);
                    IGingerWebElement ValidationElement = GetValidationElement();


                    if ("IsVisible".Equals(ValidationType))
                    {

                        if (ValidationElement.IsVisible())
                        {
                            ExecutionInfo = "Validation element is Visible";
                        }
                        else
                        {
                            Error = "Validation Element is not visble";
                        }


                    }
                    else if ("IsEnabled".Equals(ValidationType))
                    {

                        if (ValidationElement.IsEnabled())
                        {
                            ExecutionInfo = "Validation element is Enabled";
                        }
                        else
                        {
                            Error = "Validation Element is not Enabled";
                        }

                    }
                    break;
            }

        }





        /// <summary>
        /// Perform Common action on GngerWebelement return true if iction is perfomed 
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        private bool PerformCommonActions(IGingerWebElement Element)
        {
            bool performed = true;

            switch (ElementAction)
            {
                case eElementAction.DragDrop:

                    IGingerWebElement TargetElement = null;


                    Element.DragAndDrop("", TargetElement);
                    break;

                case eElementAction.GetAttrValue:
                    Element.GetAttribute("");

                    break;

                case eElementAction.GetHeight:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Height", Value = Element.GetHeight() });
                    break;

                case eElementAction.GetItemCount:

                    throw new NotImplementedException("Get Item count is not implementd");

                case eElementAction.GetSize:
                    Size s = Element.GetSize();
                    AOVs.Add(new NodeActionOutputValue() { Param = "Height", Value = s.Height });
                    AOVs.Add(new NodeActionOutputValue() { Param = "Width", Value = s.Width });

                    break;
                case eElementAction.GetStyle:

                    AOVs.Add(new NodeActionOutputValue() { Param = "Style", Value = Element.GetStyle() });
                    break;
                case eElementAction.GetWidth:

                    AOVs.Add(new NodeActionOutputValue() { Param = "Width", Value = Element.GetWidth() });
                    break;
                case eElementAction.Hover:
                    Element.Hover();
                    break;
                case eElementAction.IsEnabled:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Enabled", Value = Element.IsEnabled() });
                    break;
                case eElementAction.IsVisible:
                    AOVs.Add(new NodeActionOutputValue() { Param = "Visible", Value = Element.IsVisible() });

                    break;
                case eElementAction.MouseRightClick:
                    Element.RightClick();
                    break;
                case eElementAction.RunJavaScript:
                    Element.RunJavascript("");
                    break;
                case eElementAction.SetFocus:
                    Element.SetFocus();
                    break;
                case eElementAction.ScrollToElement:
                    Element.ScrollToElement();
                    break;

                default:
                    performed = false;
                    break;
            }

            return performed;
        }
    }
}
