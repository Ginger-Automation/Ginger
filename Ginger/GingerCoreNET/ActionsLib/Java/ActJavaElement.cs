#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

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
            AcceptDialog,
            [EnumValueDescription("Dismiss Dialog")]
            DismissDialog,
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
                return GetOrCreateInputParam<eControlAction>(nameof(ControlAction), eControlAction.SetValue);
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
                return GetOrCreateInputParam<eWaitForIdle>(nameof(WaitforIdle), eWaitForIdle.None);
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

        public override List<ePlatformType> LegacyActionPlatformsList { get { return [ePlatformType.Java]; } }

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

            return MapActJavaToActUIFields(convertedActUIElement);
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
            if (controlAction == eControlAction.GetState)
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
            var type = controlAction switch
            {
                eControlAction.SetValue => ActUIElement.eElementAction.SetValue,
                eControlAction.GetValue => ActUIElement.eElementAction.GetValue,
                eControlAction.Toggle => ActUIElement.eElementAction.Toggle,
                eControlAction.Select => ActUIElement.eElementAction.Select,
                eControlAction.AsyncSelect => ActUIElement.eElementAction.AsyncSelect,
                eControlAction.Click => ActUIElement.eElementAction.Click,
                eControlAction.AsyncClick => ActUIElement.eElementAction.AsyncClick,
                eControlAction.WinClick => ActUIElement.eElementAction.WinClick,
                eControlAction.winDoubleClick => ActUIElement.eElementAction.winDoubleClick,
                eControlAction.MouseClick => ActUIElement.eElementAction.MouseClick,
                eControlAction.MousePressRelease => ActUIElement.eElementAction.MousePressRelease,
                eControlAction.IsVisible => ActUIElement.eElementAction.IsVisible,
                eControlAction.IsMandatory => ActUIElement.eElementAction.IsMandatory,
                eControlAction.IsEnabled => ActUIElement.eElementAction.IsEnabled,
                eControlAction.GetName => ActUIElement.eElementAction.GetName,
                eControlAction.AcceptDialog => ActUIElement.eElementAction.AcceptDialog,
                eControlAction.DismissDialog => ActUIElement.eElementAction.DismissDialog,
                eControlAction.ScrollUp => ActUIElement.eElementAction.ScrollUp,
                eControlAction.ScrollDown => ActUIElement.eElementAction.ScrollDown,
                eControlAction.ScrollLeft => ActUIElement.eElementAction.ScrollLeft,
                eControlAction.ScrollRight => ActUIElement.eElementAction.ScrollRight,
                eControlAction.SelectByIndex => ActUIElement.eElementAction.SelectByIndex,
                eControlAction.GetValueByIndex => ActUIElement.eElementAction.GetValueByIndex,
                eControlAction.GetItemCount => ActUIElement.eElementAction.GetItemCount,
                eControlAction.SendKeys => ActUIElement.eElementAction.SetText,
                eControlAction.SendKeyPressRelease => ActUIElement.eElementAction.SendKeyPressRelease,
                eControlAction.DoubleClick => ActUIElement.eElementAction.DoubleClick,
                eControlAction.SetFocus => ActUIElement.eElementAction.SetFocus,
                eControlAction.GetState => ActUIElement.eElementAction.GetControlProperty,
                eControlAction.GetDialogText => ActUIElement.eElementAction.GetDialogText,
                eControlAction.Type => ActUIElement.eElementAction.SendKeys,
                eControlAction.SelectDate => ActUIElement.eElementAction.SetDate,
                eControlAction.IsChecked => ActUIElement.eElementAction.IsChecked,
                _ => ActUIElement.eElementAction.Unknown,
            };
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