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
using Amdocs.Ginger.Repository;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.Common;

namespace GingerCore.Actions.Java
{
    public class ActJavaElement : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Java Element Action"; } }
        public override string ActionUserDescription { get { return "Java Element Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public new static partial class Fields
        {
            public static string ControlAction = "ControlAction";
            public static string RowNum = "RowNum";
            public static string ColomnNum = "ColomnNum";
            public static string WaitforIdle = "WaitforIdle";
        }

        public override string ActionEditPage { get { return "Java.ActJavaElementEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Java);
                }
                return mPlatforms;
            }
        }

        public enum eWaitForIdle
        {
            [EnumValueDescription("0. None - default and recommended for most actions")]
            None,
            [EnumValueDescription("1. Short - Response expected in less than 1 second (test for idle every 0.1 second, max 30 seconds wait)")]
            Short,
            [EnumValueDescription("2. Medium - Between 1 to 5 seconds (test for idle every 0.5 second , max 60 seconds wait)")]
            Medium,
            [EnumValueDescription("3. Long - Between 5 to 30 seconds (test for idle every 1 second, max 2 Minutes wait)")]
            Long,
            [EnumValueDescription("4. Very Long - more than 30 seconds (test for idle every 5 seconds , max 5 minutes wait)")]
            VeryLong,
        }

        public enum eControlAction
        {
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Async Click")]
            AsyncClick,
            [EnumValueDescription("Win Click")]
            WinClick,
            [EnumValueDescription("Win Double Click")]
            winDoubleClick,
            [EnumValueDescription("Mouse Click")]
            MouseClick,
            [EnumValueDescription("Mouse Press/Release")]
            MousePressRelease,
            [EnumValueDescription("Get Value")]
            GetValue, // TODO: not supported remove
            Click,
            Toggle,
            Select,
            [EnumValueDescription("Async Select")]
            AsyncSelect,
            //Hover4,
            [EnumValueDescription("Is Visible")]
            IsVisible,
            [EnumValueDescription("Is Mandatory")]
            IsMandatory,
            [EnumValueDescription("Is Enabled")]
            IsEnabled,
            [EnumValueDescription("Get Name")]
            GetName,
            [EnumValueDescription("Is Checked")]
            IsChecked,
            [EnumValueDescription("Get Dialog Text")]
            GetDialogText,
            [EnumValueDescription("Accept Dialog")]
            AcceptDialog  ,
            [EnumValueDescription("Dismiss Dialog")]
            DismissDialog  ,
            [EnumValueDescription("Set Date")]
            SelectDate,
            ScrollUp,
            ScrollDown,
            ScrollLeft,
            ScrollRight,
            [EnumValueDescription("Select By Index")]
            SelectByIndex,
            [EnumValueDescription("Get Value By Index")]
            GetValueByIndex,
            [EnumValueDescription("Get Item Count")]
            GetItemCount,
            [EnumValueDescription("Send Keys")]
            SendKeys,
             
            SendKeyPressRelease,
            [EnumValueDescription("Double Click")]
            DoubleClick,
            [EnumValueDescription("Get State")]
            GetState,
            [EnumValueDescription("Type")]
            Type,
            [EnumValueDescription("Set Focus")]
            SetFocus
        }
        
        public eControlAction ControlAction
        {
            get
            {
                return (eControlAction)GetOrCreateInputParam<eControlAction>(nameof(ControlAction), eControlAction.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ControlAction), value.ToString());
            }
        }

        public eWaitForIdle WaitforIdle
        {
            get
            {
                return (eWaitForIdle)GetOrCreateInputParam<eWaitForIdle>(nameof(WaitforIdle), eWaitForIdle.None);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(WaitforIdle), value.ToString());
            }
        }

        //TODO: ColomnNum should not be here
        public string ColomnNum
        {
            get
            {
                return GetInputParamValue("ColomnNum");
            }
            set
            {
                AddOrUpdateInputParamValue("ColomnNum", value);
            }
        }

        //TODO: RowNum should not be here
        public string RowNum
        {
            get
            {
                return GetInputParamValue("RowNum");
            }
            set
            {
                AddOrUpdateInputParamValue("RowNum", value);
            }
        }

        public override String ToString()
        {
            return "JavaElement - " + ControlAction;
        }

        public override String ActionType
        {
            get
            {
                return "JavaElement: " + ControlAction.ToString();
            }
        }

        //TODO: Change icon to Java
        public override eImageType Image { get { return eImageType.Java; } }
        
        public PayLoad Pack()
        {
            //TODO: not used!? remove as in java driver there is special pack per action type
            PayLoad pl = new PayLoad("ActJavaElement");
            pl.AddEnumValue(WaitforIdle);
            pl.AddEnumValue(LocateBy);
            pl.AddValue(LocateValue);
            pl.AddValue(Value);
            pl.AddEnumValue(ControlAction);
            pl.ClosePackage();
            return pl;            
        }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return new List<ePlatformType>() { ePlatformType.Java }; } }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType actionPlatform)
        {
            if (actionPlatform == ePlatformType.Java)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapperConfiguration = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement convertedActUIElement = mapperConfiguration.CreateMapper().Map<Act, ActUIElement>(this);

            return  MapActJavaToActUIFields(convertedActUIElement);
        }

        private ActUIElement MapActJavaToActUIFields(ActUIElement convertedActUIElement)
        {
            convertedActUIElement.ElementLocateBy = this.LocateBy;
            convertedActUIElement.ElementLocateValue = this.LocateValue;

            ActUIElement.eElementAction elmentActionType = GetElementActionType(this.ControlAction);
            convertedActUIElement.ElementAction = elmentActionType;
            
            convertedActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, this.WaitforIdle.ToString());
            
            if (elmentActionType.Equals(ActUIElement.eElementAction.GetControlProperty))
            {
                string propertyName = GetPropertyName(this.ControlAction);
                convertedActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, propertyName);
            }
            else
            {
                convertedActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, this.Value);
            }

            if (this.ReturnValues.Count > 0)
            {
                if (this.ReturnValues[0].Param == "Actual")
                {
                    convertedActUIElement.ReturnValues[0].Param = "Actual0";
                }
            }
            return convertedActUIElement;
        }

        private string GetPropertyName(eControlAction controlAction)
        {
            if(controlAction == eControlAction.GetState)
            {
                return ActUIElement.eElementProperty.ToggleState.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private ActUIElement.eElementAction GetElementActionType(eControlAction controlAction)
        {
            ActUIElement.eElementAction type;
            switch (controlAction)
            {
                case  eControlAction.SetValue:
                    type = ActUIElement.eElementAction.SetValue;
                    break;

                case eControlAction.GetValue:
                    type = ActUIElement.eElementAction.GetValue;
                    break;

                case eControlAction.Toggle:
                    type = ActUIElement.eElementAction.Toggle;
                    break;

                case eControlAction.Select:
                    type = ActUIElement.eElementAction.Select;
                    break;

                case eControlAction.AsyncSelect:
                    type = ActUIElement.eElementAction.AsyncSelect;
                    break;

                case eControlAction.Click:
                    type = ActUIElement.eElementAction.Click;
                    break;

                case eControlAction.AsyncClick:
                    type = ActUIElement.eElementAction.AsyncClick;
                    break;

                case eControlAction.WinClick:
                    type = ActUIElement.eElementAction.WinClick;
                    break;

                case eControlAction.winDoubleClick:
                    type = ActUIElement.eElementAction.winDoubleClick;
                    break;

                case eControlAction.MouseClick:
                    type = ActUIElement.eElementAction.MouseClick;
                    break;

                case eControlAction.MousePressRelease:
                    type = ActUIElement.eElementAction.MousePressRelease;
                    break;

                case eControlAction.IsVisible:
                    type = ActUIElement.eElementAction.IsVisible;
                    break;

                case eControlAction.IsMandatory:
                    type = ActUIElement.eElementAction.IsMandatory;
                    break;

                case eControlAction.IsEnabled:
                    type = ActUIElement.eElementAction.IsEnabled;
                    break;

                case eControlAction.GetName:
                    type = ActUIElement.eElementAction.GetName;
                    break;

                case eControlAction.AcceptDialog:
                    type = ActUIElement.eElementAction.AcceptDialog;
                    break;

                case eControlAction.DismissDialog:
                    type = ActUIElement.eElementAction.DismissDialog;
                    break;

                case eControlAction.ScrollUp:
                    type = ActUIElement.eElementAction.ScrollUp;
                    break;

                case eControlAction.ScrollDown:
                    type = ActUIElement.eElementAction.ScrollDown;
                    break;

                case eControlAction.ScrollLeft:
                    type = ActUIElement.eElementAction.ScrollLeft;
                    break;

                case eControlAction.ScrollRight:
                    type = ActUIElement.eElementAction.ScrollRight;
                    break;

                case eControlAction.SelectByIndex:
                    type = ActUIElement.eElementAction.SelectByIndex;
                    break;

                case eControlAction.GetValueByIndex:
                    type = ActUIElement.eElementAction.GetValueByIndex;
                    break;

                case eControlAction.GetItemCount:
                    type = ActUIElement.eElementAction.GetItemCount;
                    break;

                case eControlAction.SendKeys:
                    type = ActUIElement.eElementAction.SetText;
                    break;

                case eControlAction.SendKeyPressRelease:
                    type = ActUIElement.eElementAction.SendKeyPressRelease;
                    break;

                case eControlAction.DoubleClick:
                    type = ActUIElement.eElementAction.DoubleClick;
                    break;

                case eControlAction.SetFocus:
                    type = ActUIElement.eElementAction.SetFocus;
                    break;

                case eControlAction.GetState:
                    type = ActUIElement.eElementAction.GetControlProperty;
                    break;

                case eControlAction.GetDialogText:
                    type = ActUIElement.eElementAction.GetDialogText;
                    break;

                case eControlAction.Type:
                    type = ActUIElement.eElementAction.SendKeys;
                    break;

                case eControlAction.SelectDate:
                    type = ActUIElement.eElementAction.SetDate;
                    break;

                case eControlAction.IsChecked:
                    type = ActUIElement.eElementAction.IsChecked;
                    break;

                default:
                    type = ActUIElement.eElementAction.Unknown;
                    break;
            }
            return type;
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActUIElement);
        }

        string IObsoleteAction.TargetActionTypeName()
        {
            ActUIElement newUIElementAction = new ActUIElement();
            return newUIElementAction.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Java;
        }
    }
}