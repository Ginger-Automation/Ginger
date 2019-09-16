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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions
{
    // This class is for UI link element
    public class ActGenElement : Act, IObsoleteAction
    {
        // --------------------------------------------------------------------------------------------------------------
        // This Action is OBSOLETE and should be converted to new ActUIElement !!!!!!!!!!!!!!!!!!!!
        // --------------------------------------------------------------------------------------------------------------
        public new static partial class Fields
        {
            public static string Xoffset = "Xoffset";
            public static string Yoffset = "Yoffset";
        }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if (platform == ePlatformType.Web || platform == ePlatformType.Java)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override List<ePlatformType> LegacyActionPlatformsList {
            get {
                var legacyPlatform = new List<ePlatformType>();
                legacyPlatform.Add(ePlatformType.Web);
                legacyPlatform.Add(ePlatformType.Java);
                return legacyPlatform;
            }
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            ePlatformType targetPlatform;
            switch(this.Platform)
            {
                case ePlatformType.Web:
                    targetPlatform = ePlatformType.Web;
                    break;

                case ePlatformType.Java:
                    targetPlatform = ePlatformType.Java;
                    break;

                default:
                    throw new PlatformNotSupportedException();
            }
            return targetPlatform;
        }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(GenElementAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(GenElementAction);
            if (currentType == typeof(ActBrowserElement))
            {
                ActBrowserElement actBrowserElement = new ActBrowserElement();
                return actBrowserElement.ActionDescription;
            }
            else if (currentType == typeof(ActUIElement))
            {
                ActUIElement actUIElement = new ActUIElement();
                return actUIElement.ActionDescription;
            }
            else
            {
                return string.Empty;
            }
        }
        Act IObsoleteAction.GetNewAction()
        {
            Act convertedUIElementAction = null;
            switch(this.Platform)
            {
                case ePlatformType.Web:
                    convertedUIElementAction = GetNewActionForWeb();
                    break;
                case ePlatformType.Java:
                    convertedUIElementAction = GetNewActionForJava();
                    break;
                default:
                    throw new PlatformNotSupportedException();
            }

            return convertedUIElementAction;
        }

        private Act GetNewActionForJava()
        {
            if(GenElementAction.Equals(eGenElementAction.SwitchFrame) || GenElementAction.Equals(eGenElementAction.RunJavaScript))
            {
                ActBrowserElement NewActBrowserElement = GetNewBrowserElementFromAutoMapper();

                if (GenElementAction.Equals(eGenElementAction.SwitchFrame))
                {
                    NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
                }
                else if (GenElementAction.Equals(eGenElementAction.RunJavaScript))
                {
                    NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.RunJavaScript;
                }

                return NewActBrowserElement;
            }
            else
            {
                ActUIElement newActUIElement = GetNewUIElementFromAutoMapper();

                newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.IsWidgetsElement, "true");
                newActUIElement.ElementLocateBy = this.LocateBy;
                newActUIElement.ElementLocateValue = this.LocateValue;

                newActUIElement.ElementAction = MapJavaGenericElementAction(GenElementAction);
                newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, this.Value);

                if (this.ReturnValues.Count > 0)
                {
                    if (this.ReturnValues[0].Param == "Actual")
                    {
                        newActUIElement.ReturnValues[0].Param = "Actual0";
                    }
                }
                return newActUIElement;
            }

        }

        private ActUIElement GetNewUIElementFromAutoMapper()
        {
            AutoMapper.MapperConfiguration mapConfigUIElement = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newActUIElement = mapConfigUIElement.CreateMapper().Map<Act, ActUIElement>(this);
            return newActUIElement;
        }


        private ActBrowserElement GetNewBrowserElementFromAutoMapper()
        {
            AutoMapper.MapperConfiguration mapConfigBrowserElementt = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActBrowserElement>(); });
            ActBrowserElement NewActBrowserElement = mapConfigBrowserElementt.CreateMapper().Map<Act, ActBrowserElement>(this);
            return NewActBrowserElement;
        }

        private ActUIElement.eElementAction MapJavaGenericElementAction(eGenElementAction genElementAction)
        {
            ActUIElement.eElementAction elementAction;
            switch (genElementAction)
            {
                case eGenElementAction.SetValue:
                case eGenElementAction.GetValue:
                case eGenElementAction.Click:
                case eGenElementAction.AsyncClick:
                case eGenElementAction.RunJavaScript:
                case eGenElementAction.ScrollDown:
                case eGenElementAction.ScrollUp:
                    elementAction = (ActUIElement.eElementAction)Enum.Parse(typeof(ActUIElement.eElementAction), GenElementAction.ToString());
                    break;

                case eGenElementAction.Enabled:
                    elementAction = ActUIElement.eElementAction.IsEnabled;
                    break;
                case eGenElementAction.Visible:
                    elementAction = ActUIElement.eElementAction.IsVisible;
                    break;

                case eGenElementAction.SelectFromDropDownByIndex:
                    elementAction = ActUIElement.eElementAction.SelectByIndex;
                    break;

                case eGenElementAction.SelectFromDropDown:
                    elementAction = ActUIElement.eElementAction.Select;
                    break;

                case eGenElementAction.FireMouseEvent:
                case eGenElementAction.FireSpecialEvent:
                    elementAction = ActUIElement.eElementAction.TriggerJavaScriptEvent;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return elementAction;
        }

        private Act GetNewActionForWeb()
        {
            bool uIElementTypeAssigned = false;

            ActUIElement NewActUIElement = GetNewUIElementFromAutoMapper();

            ActBrowserElement NewActBrowserElement = GetNewBrowserElementFromAutoMapper();

            Type currentType = GetActionTypeByElementActionName(GenElementAction);
            if (currentType == typeof(ActBrowserElement))
            {
                switch (GenElementAction)
                {
                    case eGenElementAction.Back:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.NavigateBack;
                        break;
                    case eGenElementAction.CloseBrowser:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.Close;
                        break;
                    case eGenElementAction.StartBrowser:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.InitializeBrowser;
                        break;
                    default:
                        try
                        {
                            NewActBrowserElement.ControlAction = (ActBrowserElement.eControlAction)System.Enum.Parse(typeof(ActBrowserElement.eControlAction), GenElementAction.ToString());
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while mapping the operation to action while conversion", ex);
                            throw;
                        }
                        break;
                }
            }
            else if (currentType == typeof(ActUIElement))
            {
                switch (GenElementAction)
                {
                    case eGenElementAction.Click:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.JavaScriptClick;
                        break;
                    case eGenElementAction.RightClick:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.MouseRightClick;
                        break;
                    case eGenElementAction.Visible:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.IsVisible;
                        break;
                    case eGenElementAction.SelectFromListScr:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SelectByIndex;
                        NewActUIElement.ElementType = eElementType.List;
                        uIElementTypeAssigned = true;
                        break;
                    case eGenElementAction.AsyncSelectFromDropDownByIndex:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SelectByIndex;
                        NewActUIElement.ElementType = eElementType.ComboBox;
                        break;
                    case eGenElementAction.KeyboardInput:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SendKeys;
                        break;
                    case eGenElementAction.KeyType:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SetText;
                        break;
                    case eGenElementAction.SelectFromDropDown:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SelectByText;
                        NewActUIElement.ElementType = eElementType.ComboBox;
                        uIElementTypeAssigned = true;
                        break;
                    case eGenElementAction.GetInnerText:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.GetText;
                        break;
                    case eGenElementAction.GetCustomAttribute:
                    case eGenElementAction.GetElementAttributeValue:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.GetAttrValue;
                        break;
                    case eGenElementAction.BatchClicks:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.MultiClicks;
                        break;
                    case eGenElementAction.BatchSetValues:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.MultiSetValue;
                        break;
                    case eGenElementAction.SimpleClick:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.Click;
                        break;
                    case eGenElementAction.Disabled:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.IsDisabled;
                        break;
                    case eGenElementAction.Enabled:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.IsEnabled;
                        break;
                    case eGenElementAction.GetNumberOfElements:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.GetItemCount;
                        break;
                    case eGenElementAction.XYClick:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.ClickXY;
                        break;
                    case eGenElementAction.Focus:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.SetFocus;
                        break;
                    case eGenElementAction.RunJavaScript:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.RunJavaScript;
                        break;
                    case eGenElementAction.SetAttributeUsingJs:
                        NewActUIElement.ElementAction = ActUIElement.eElementAction.RunJavaScript;
                        break;
                    default:
                        try
                        {
                            NewActUIElement.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), GenElementAction.ToString());
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while mapping the operation to action while conversion", ex);
                            throw;
                        }
                        break;
                }
            }

            if (currentType == typeof(ActUIElement))
            {
                NewActUIElement.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
                if (!string.IsNullOrEmpty(this.LocateValue))
                {
                    if (GenElementAction == eGenElementAction.SetAttributeUsingJs)
                    {
                        NewActUIElement.Value = string.Format("arguments[0].{0}", this.Value);
                    }
                    else
                    {
                        NewActUIElement.ElementLocateValue = String.Copy(this.LocateValue); 
                    }
                }
               
                if (!uIElementTypeAssigned)
                    NewActUIElement.ElementType = eElementType.Unknown;
                if (!NewActUIElement.Platforms.Contains(this.Platform))
                {
                    NewActUIElement.Platform = ePlatformType.Web; // ??? to check
                }

                NewActUIElement.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, this.GetInputParamValue("Value"));
                return NewActUIElement;
            }

            if (currentType == typeof(ActBrowserElement))
            {
                return NewActBrowserElement;
            }
            return null;
        }

        Type GetActionTypeByElementActionName(eGenElementAction genElementAction)
        {
            Type currentType = null;

            switch (genElementAction)
            {
                case eGenElementAction.GotoURL:
                case eGenElementAction.Back:
                case eGenElementAction.SwitchFrame:
                case eGenElementAction.CloseBrowser:
                case eGenElementAction.DismissMessageBox:
                case eGenElementAction.SwitchWindow:
                case eGenElementAction.DeleteAllCookies:
                case eGenElementAction.SwitchToDefaultFrame:
                case eGenElementAction.Refresh:                
                case eGenElementAction.SwitchToParentFrame:
                case eGenElementAction.AcceptMessageBox:
                case eGenElementAction.GetWindowTitle:
                case eGenElementAction.StartBrowser:
                    currentType = typeof(ActBrowserElement);
                    break;

                //
                case eGenElementAction.KeyType:
                case eGenElementAction.SelectFromListScr:
                case eGenElementAction.Hover:
                case eGenElementAction.Click:
                case eGenElementAction.GetValue:
                case eGenElementAction.Visible:
                case eGenElementAction.SetValue:
                case eGenElementAction.MouseClick:
                case eGenElementAction.KeyboardInput:
                case eGenElementAction.GetInnerText:
                case eGenElementAction.GetWidth:
                case eGenElementAction.GetHeight:
                case eGenElementAction.GetStyle:
                case eGenElementAction.GetCustomAttribute:
                case eGenElementAction.AsyncClick:
                case eGenElementAction.ScrollToElement:
                case eGenElementAction.SimpleClick:
                case eGenElementAction.DoubleClick:
                case eGenElementAction.RightClick:
                case eGenElementAction.SelectFromDropDown:
                case eGenElementAction.GetElementAttributeValue:
                case eGenElementAction.BatchClicks:
                case eGenElementAction.BatchSetValues:
                case eGenElementAction.Disabled:
                case eGenElementAction.GetNumberOfElements:
                case eGenElementAction.SendKeys:
                case eGenElementAction.Enabled:
                case eGenElementAction.XYClick:
                case eGenElementAction.Focus:
                case eGenElementAction.RunJavaScript:
                case eGenElementAction.AsyncSelectFromDropDownByIndex:
                case eGenElementAction.SetAttributeUsingJs:
                //Added for Java
                case eGenElementAction.SelectFromDropDownByIndex:
                case eGenElementAction.FireSpecialEvent:
                case eGenElementAction.FireMouseEvent:
                case eGenElementAction.ScrollDown:
                case eGenElementAction.ScrollUp:
                    currentType =  typeof(ActUIElement);
                    break;

                //default:
                //    throw new Exception("Converter error, missing Action translator for - " + GenElementAction);
            }
            return currentType;
        }

        public override string ActionDescription { get { return "Generic Element"; } }
        public override string ActionUserDescription { get { return "Click on a generic control object"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate a click on an object from type generic control.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select property type of label from Locate By drop down and then enter label property value and then select an action type.");
            TBH.AddLineBreak();
            TBH.AddText("For Using Set Attribute using javascript the format should be :  Attribute||Value ");
        }        

        public override string ActionEditPage { get { return "ActGenElementEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }  // This depends on the action...


        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                }
                return mPlatforms;
            }
        }

        public enum eGenElementAction
        {
            Click = 0,
            Hover = 2, //This is needed for hovering to expand menus.
            [EnumValueDescription("Get Value")]
            GetValue = 3, //for validation
            [EnumValueDescription("Is Visible")]
            Visible = 4, //for validation
            [EnumValueDescription("Set Value")]
            SetValue = 5,
            [EnumValueDescription("Mouse Click")]
            MouseClick = 6,
            [EnumValueDescription("Keyboard Input")]
            KeyboardInput = 7, 
            //SwipeUp= 6, //for mobile
            //SwipeDown = 7, //for mobile
            //SwipeRight = 8, //for mobile
            //SwipeLeft = 9, //for mobile
            //Press = 10, //for mobile
            Wait = 11, //for mobile
            Back= 12, //for Browser
            [EnumValueDescription("Select From List Scr")]
            SelectFromListScr = 13, //for Browser
            [EnumValueDescription("Key Type")]
            KeyType = 14, //for Browser
            [EnumValueDescription("GoTo URL")]
            GotoURL = 15, //for Browser
            [EnumValueDescription("Select From Drop Down")]
            SelectFromDropDown = 16, //for Browser
            [EnumValueDescription("Switch Frame")]
            SwitchFrame = 17, //for Browser
            [EnumValueDescription("Get Inner Text")]
            GetInnerText = 18, //for Browser
            [EnumValueDescription("Close Browser")]
            CloseBrowser = 19, //for Browser
            [EnumValueDescription("Dismiss Message Box")]
            DismissMessageBox = 20, //for Browser
            [EnumValueDescription("Switch Window")]
            SwitchWindow=21,
            [EnumValueDescription("Get Width")]
            GetWidth=22,
            [EnumValueDescription("Get Height")]
            GetHeight=23,
            [EnumValueDescription("Get Style")]
            GetStyle=24,
            [EnumValueDescription("Message Box")]
            MsgBox = 25,
            [EnumValueDescription("Get Custom Attribute")]
            GetCustomAttribute=26,
            [EnumValueDescription("Start Browser")]
            StartBrowser=28,
            [EnumValueDescription("Async Click")]
            AsyncClick = 27, //used for clicking on elements which opening Dialog window- because if using regular click then the driver get stuck till the dialog been closed
            [EnumValueDescription("Scroll to Element")]
            ScrollToElement= 29,
            [EnumValueDescription("Simple Click")]
            SimpleClick=30,
            [EnumValueDescription("Switch To Default Frame")]
            SwitchToDefaultFrame=31,
            [EnumValueDescription("Delete All Cookies")]
            DeleteAllCookies=32,
            Refresh = 33,
            [EnumValueDescription("Get Contexts")]
            GetContexts = 34,
            [EnumValueDescription("Set Contexts")]
            SetContext = 35,
            [EnumValueDescription("Select From Dijit List")]
            SelectFromDijitList = 36,//used for selecting from DropDown/Combobox which his values loaded using dijit (Dojo Toolkit)
            [EnumValueDescription("Run Java Script")]
            RunJavaScript= 37,
            [EnumValueDescription("Double Click")]
            DoubleClick= 38,
            [EnumValueDescription("Right Click")]
            RightClick= 39,
            [EnumValueDescription("Switch To Parent Frame")]
            SwitchToParentFrame=40,
            [EnumValueDescription("Async Select From Drop Down (By Index)")]
            AsyncSelectFromDropDownByIndex = 41,
            [EnumValueDescription("Accept Message Box")]
            AcceptMessageBox = 42,
            [EnumValueDescription("Get Element Attribute Value")]
            GetElementAttributeValue, 
			//Dummy = 43,
            [EnumValueDescription("Batch Clicks")]
            BatchClicks = 44,
            [EnumValueDescription("Batch Set Values")]
            BatchSetValues = 45,
            [EnumValueDescription("Get Window Title")]
			GetWindowTitle = 46,
			Disabled=47,
            [EnumValueDescription("Set Attribute using JavaScript")]
            SetAttributeUsingJs=48,
            [EnumValueDescription("Get Number of Elements")]
            GetNumberOfElements = 49,
            [EnumValueDescription("Send Keys")]
            SendKeys=50,
            [EnumValueDescription("Click At")]
            ClickAt = 51,
            [EnumValueDescription("Is Enabled")]
            Enabled = 52,
            [EnumValueDescription("Tap Element")]
            TapElement = 53,
            [EnumValueDescription("Select From Drop Down (By Index)")]
            SelectFromDropDownByIndex = 54,
            XYClick = 55,
            XYDoubleClick = 56,
            XYSendKeys = 57,
            //XYSendkeys2=58,
            [EnumValueDescription("Is Focused")]
            Focus = 59,
            [EnumValueDescription("Simple Double Click")]
            Doubleclick2 = 61,
            HighLightElement=60,
            [EnumValueDescription("Fire Mouse Event")]
            FireMouseEvent = 62,
            [EnumValueDescription("Fire Special Event")]
            FireSpecialEvent = 63,
            [EnumValueDescription("Scroll Down")]
            ScrollDown = 64,
            [EnumValueDescription("Scroll Up")]
            ScrollUp = 65,
            [EnumValueDescription("Set Attribute Value")]
            SetAttributeValue = 66,
            [EnumValueDescription("Expand Drop Down")]
            Expand = 67
        }
        
        [IsSerializedForLocalRepository]
        public eGenElementAction GenElementAction { get; set; }

        string mXoffset = string.Empty;
        public string Xoffset
        {
            get
            {
                //nd for backworth support
                if( GetInputParamValue(Fields.Xoffset) == null && mXoffset != string.Empty)
                    AddOrUpdateInputParamValue(Fields.Xoffset, mXoffset);

                return GetInputParamCalculatedValue(Fields.Xoffset);
            }
            set
            {
                mXoffset = value;
                AddOrUpdateInputParamValue(Fields.Xoffset, value);
            }
        }

        string mYoffset = string.Empty;

        public string Yoffset
        {
            get
            {
                //nd for backworth support
                if (GetInputParamValue(Fields.Yoffset) == null && mYoffset != string.Empty)
                    AddOrUpdateInputParamValue(Fields.Yoffset, mYoffset);

                return GetInputParamCalculatedValue(Fields.Yoffset);
            }
            set
            {
                mYoffset = value;
                AddOrUpdateInputParamValue(Fields.Yoffset, value);
            }
        }
        
        public override String ToString()
        {
            return "Generic Web Element: " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Generic Web Element: " + GenElementAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.Screen; } }  // TODO: make me dynamic based on elem type
        
        public override ActionDetails Details
        {            
            get
            {

                // We create a customized user friendly action details for actions grid and report
                ActionDetails d = base.Details;

                // return params order by priority
                d.Params.Clear();

                d.Params.Add(new ActionParamInfo() { Param = "Action", Value = GenElementAction.ToString() });   

                if (!string.IsNullOrEmpty(this.Value))
                {                    
                    d.Params.Add(new ActionParamInfo() { Param = "Value", Value = this.Value });   
                }
                return d;
            }
        }      
    }
}
