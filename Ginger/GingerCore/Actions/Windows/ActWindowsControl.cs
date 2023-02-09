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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions.Windows
{
    public class ActWindowsControl : Act
    {
        public new static class Fields
        {
            public static string ControlAction = "ControlAction";
        }

        public override string ActionDescription { get { return "Windows Control Action"; } }
        public override string ActionUserDescription { get { return "Windows Control Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override string ActionEditPage { get { return "Windows.ActWindowsControlEditPage"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return true; } }

        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Windows);
                }
                return mPlatforms;
            }
        }

        public enum eControlAction
        {
            // Since, some operations aren't supported by Windows Platform hence, they're commented for now

            SetValue = 1,
            GetValue = 2,
            Click = 3,
            IsEnabled = 6,
            GetControlProperty = 16,
            Select = 17,
            GetSelected=18,
            GetTitle=19,
            ScrollUp=20,
            Scrolldown=21,
            IsSelected=22,
            Highlight=23,            
            Toggle=25,
            RightClick=26,
            ClickXY=27,
            DoubleClick = 28,
            SendKeys=29,
            AsyncClick=30,
            IsExist=31,
            Repaint=32,
            SelectContextMenuItem,
            GetText,
            Restore,
            Maximize,
            Minimize,
            Resize,
            Expand
        }

        public enum eControlProperty
        {
            NA =0,
            Value =1,
            Text =2,
            Type=3,
            Enabled=4,
            Visible=5,
            List =6
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
        public eControlProperty ControlProperty { get; set; }

        public override String ToString()
        {
            return "WindowsDriver - " + ControlAction;
        }

        public override String ActionType
        {
            get
            {
                return "WindowsDriver: " + ControlAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.WindowsIcon; } }
    }
}
