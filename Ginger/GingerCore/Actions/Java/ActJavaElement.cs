#region License
/*
Copyright © 2014-2018 European Support Limited

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

namespace GingerCore.Actions.Java
{
    public class ActJavaElement : Act
    {
        public override string ActionDescription { get { return "Java Element Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
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
        
        [IsSerializedForLocalRepository]
        public eControlAction ControlAction { get; set; }
        
        [IsSerializedForLocalRepository]
        public eWaitForIdle WaitforIdle { get; set; }        
        
        //TODO: ColomnNum should not be here
        [IsSerializedForLocalRepository]
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
        [IsSerializedForLocalRepository]
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
        public override System.Drawing.Image Image { get { return Resources.ASCF16x16; } }
        
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
    }
}