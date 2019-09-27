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
using Ginger.Plugin.Platform.Web.Actions;
using Ginger.Plugin.Platform.Web.Elements;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ginger.Plugin.Platform.Web.Execution
{
    public class UIELementActionHandler : IActionHandler
    {

        public enum eElementAction
        {
            #region Generic Action Types

            Unknown,
            Hover,
            Visible,
            Click,
            JavaScriptClick,
            GetCustomAttribute,//keeping for backward support
            ClickAndValidate,
            AsyncClick,
            // not here at all ?
            WinClick,
            MouseClick,
            ClickXY,
            SetText,
            GetText,
            SetValue,
            GetValue,
            GetXY,
            GetSize,
            OpenDropDown,
            SelectandValidate,
            CloseDropDown,
            GetAllValues,
            GetAttrValue,
            SetFocus,
            IsEnabled,
            Toggle,
            Select,
            IsVisible,
            IsMandatory,
            Exist,
            NotExist,
            Enabled,
            GetName,
            GetDialogText,
            AcceptDialog,
            DismissDialog,
            SetDate,
            ScrollUp,
            ScrollDown,
            ScrollLeft,
            ScrollRight,
            SelectByIndex,
            GetValueByIndex,
            GetItemCount,
            SendKeys,
            DragDrop,
            IsExist,
            GetContexts,
            SetContext,
            MouseRightClick,
            GetFont,
            GetWidth,
            GetHeight,
            GetStyle,
            MultiClicks,
            MultiSetValue,
            GetWindowTitle,
            IsDisabled,
            Switch,
            SendKeysXY,
            #endregion Generic Action Types

            #region TextBox Action Types
            ClearValue,
            GetTextLength,
            #endregion TextBox Action Types


            #region ComboBox related Types
            SetSelectedValueByIndex,
            SelectByText,
            GetValidValues,
            GetSelectedValue,
            IsValuePopulated,
            #endregion Usable Action Types

            Submit,
            RunJavaScript,
            DrawObject,
        }

        //#region PomProperties
        //private bool IsPOM;
        //List<string> FrameXpaths = new List<string>();

        //List<KeyValuePair<string, string>> Locators = new List<KeyValuePair<string, string>>();
        //#endregion


        //#region commonproperties
        string mElementType = string.Empty;


        //string ElementLocateBy = string.Empty;
        //string LocateByValue = string.Empty;

            // Move from here !!!!!

         eElementAction ElementAction;
        
        //#endregion
  

        string Value
        {
            get
            {
                return (string)mPlatformAction.InputParams["Value"];
            }
        }
        readonly IWebPlatform mPlatformService;

        public Dictionary<string, object> InputParams { get; set; }


        // Remove !!!!!!!!!!!
        internal List<NodeActionOutputValue> AOVs = new List<NodeActionOutputValue>();

        // Remove !!!!!!!!!!!
        public string ExecutionInfo { get ; set; }

        // Remove !!!!!!!!!!!
        public string Error { get; set; }

        public UIELementActionHandler(IWebPlatform platformService)
        {
            mPlatformService = platformService;
            // InputParams = platformActionData.InputParams;
        }



        public struct Locator
        {
            public string By;
            public string Value;
        }

        NodePlatformAction mPlatformAction;

        public void ExecuteAction(ref NodePlatformAction platformAction)
        {         
            try
            {
                mPlatformAction = platformAction;

                InputParams = platformAction.InputParams;
           
                // convert the JArray to list of locators
                JObject Locators =(JObject) InputParams["Locators"];
            
                eElementType ElementType = (eElementType)Enum.Parse(typeof(eElementType), (string)InputParams["ElementType"]);
                IGingerWebElement uiElement = null;
                JArray Frames = null;
                if (InputParams.ContainsKey("Frames"))
                {
                    Frames = (JArray)InputParams["Frames"];

                    if (Frames != null && Frames.Children().Count() > 0) {

                        mPlatformService.BrowserActions.SwitchToDefaultContent();
                        foreach (JToken jf in Frames.Children())
                        {

                            IGingerWebElement GWA = mPlatformService.LocateWebElement.LocateElementByXPath(eElementType.WebElement, jf.ToString());
                            mPlatformService.BrowserActions.SwitchToFrame(GWA);
                        }
                    }
                }
             
                foreach (JProperty locator in Locators.Children())
                {
                    uiElement = WebPlatformActionHandler.LocateElement(ref ElementType, locator.Name, locator.Value.ToString(),mPlatformService);
                    if (uiElement != null)
                    {
                        platformAction.exInfo += "UI Element Located using: " + locator.Name + "=" + locator.Value;
                        break;
                    }
                }
                if (uiElement == null)
                {
                    platformAction.error += "Element not found";
            
                    return;
                }

                ElementAction = (eElementAction)Enum.Parse(typeof(eElementAction), (string)platformAction.InputParams["ElementAction"]);


          

                RunActionOnUIElement(uiElement, ElementType);

                //TODO: remove  // update output values - TODO: do it directlyon platformAction !!!
                foreach (NodeActionOutputValue nodeActionOutputValue in AOVs)
                {
                    platformAction.Output.Add(nodeActionOutputValue.Param, (string)nodeActionOutputValue.Value); // TODO: add path !!!
                }
            }

            catch (Exception Ex)
            {
                Error = Ex.Message;
                ExecutionInfo = Ex.StackTrace;  // DO not put stacktrace !!!!
            }
        }

        private void RunActionOnUIElement(IGingerWebElement uiElement, eElementType ElementType)
        {
        // Try if it is common action first
            bool ActionPerformed = PerformCommonActions(uiElement);

            if (!ActionPerformed)
            {
                switch (ElementType)
                {
                    case eElementType.Button:
                        ButtonActions((IButton)uiElement);                            
                        break;
                    case eElementType.Canvas:
                        CanvasAction(uiElement);
                        break;
                    case eElementType.CheckBox:
                        CheckBoxActions(uiElement);
                        break;
                    case eElementType.ComboBox:
                        ComboBoxActions(uiElement);
                        break;
                    case eElementType.Div:
                        DivActions(uiElement);
                        break;
                    case eElementType.Image:
                        ImageActions(uiElement);
                        break;
                    case eElementType.Label:
                        LabelActions(uiElement);
                        break;
                    case eElementType.List:
                        ListActions(uiElement);
                        break;
                    case eElementType.RadioButton:
                        RadioButtonActions(uiElement);
                        break;
                    case eElementType.Span:
                        SpanActions(uiElement);
                        break;
                    case eElementType.Table:
                        TableActions(uiElement);
                        break;
                    case eElementType.TextBox:
                        TextBoxActions((ITextBox)uiElement);
                        break;
                    case eElementType.HyperLink:
                        HyperLinkActions(uiElement, ElementAction);
                        break;


                }
            }
        }


   

        private void HyperLinkActions(IGingerWebElement element, eElementAction mElementAction)
        {
            if (element is IHyperLink Hyperlink)
            {
                if (element is IClick ClickElement)
                {
                    ClickActions(ClickElement,ElementAction);
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
            string ValidationElementLocateBy = (string)InputParams["ValidationElementLocateBy"];
            string ValidationElementLocatorValue = (string)InputParams["ValidationElementLocatorValue"]; 
            string mValidationElement = (string)InputParams["ValidationElement"];
            eElementType validationElementType = (eElementType)Enum.Parse(typeof(eElementType), mValidationElement);
            IGingerWebElement ValidationElement = WebPlatformActionHandler.LocateElement(ref validationElementType, ValidationElementLocateBy, ValidationElementLocatorValue,mPlatformService);
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

        private void TextBoxActions(ITextBox TextBox)
        {            
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
                    TextBox.SetText(Value);
                    break;
                case eElementAction.GetText:
                    string txt = TextBox.GetText();
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = txt });
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
            throw new NotImplementedException();
        }

        private void RadioButtonActions(IGingerWebElement element)
        {

            if (element is IClick ClickElement)
            {
                ClickActions(ClickElement, ElementAction);
            }

            else if (element is IGetValue GetElementValue)
            {

            }
            else
            {
                Error = "Not Supported Action";
            }

        }

        private void ListActions(IGingerWebElement element)
        {
            if (element is IWebList Element)
            {
                string ValueToSelect;
                ValueToSelect = (string)InputParams["ValueToSelect"];
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
                       

                        AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = Label.GetFont()    });

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
                ValueToSelect = (string)InputParams["ValueToSelect"];
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

        private void ButtonActions(IButton element)
        {                            
            switch (ElementAction)
            {
                case eElementAction.Click:
                    element.Click();                    
                    break;
                case eElementAction.Submit: // !!!!!!!!!!! remove from here need special handling ??!!
                    element.Submit();
                    break;
                case eElementAction.GetValue:
                
                    AOVs.Add(new NodeActionOutputValue() { Param = "Actual", Value = element.GetValue() });
                    break;

                case eElementAction.ClickAndValidate: // !!!!!!!!!!! remove from here need special handling ??!!
                    ClickActions(element, eElementAction.ClickAndValidate);
                    break;
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
                 
                    string ValidationType = (string)InputParams["ValidationType"];
                    string mClickType = (string)InputParams["ClickType"];
                    eElementAction ClickType = (eElementAction)Enum.Parse(typeof(eElementAction), mClickType);

                    ClickActions(Element, ClickType);
                    IGingerWebElement ValidationElement = GetValidationElement();


                    if ("IsVisible".Equals(ValidationType))
                    {
                     
                        if(ValidationElement.IsVisible())
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
         
                    throw new  NotImplementedException("Get Item count is not implementd");
             
                case eElementAction.GetSize:
                   Size s= Element.GetSize();
                    AOVs.Add(new NodeActionOutputValue() { Param = "Height", Value = s.Height});
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

                default:
                    performed = false;
                    break;
            }

            return performed;
        }


    }
}
