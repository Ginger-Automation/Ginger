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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCore.Properties;
using GingerCore.Repository;
using Amdocs.Ginger.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions.ASCF
{
    public class ActASCFBrowserElement : Act
    {
            public new static partial class Fields
            {
                public static string ControlAction = "ControlAction";
            }

            public override string ActionDescription { get { return "ASCF Browser Control Action"; } }
            public override string ActionUserDescription { get { return "ASCF Browser Control Action"; } }

            public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
            {
                TBH.AddText("Action to handle ASCF Widgets");
                TBH.AddLineBreak();
                TBH.AddText("Widget are Internet Explorer browser inside ASCF form, they contain their own HTML and can have many HTML controls");
                TBH.AddLineBreak();
                TBH.AddText("This action enable you to set the current active browser control and then locate a specific HTML element and run action on this element");
            }

            public override string ActionEditPage { get { return "ActASCFBrowserControlEditPage"; } }
            public override bool ObjectLocatorConfigsNeeded { get { return true; } }
            public override bool ValueConfigsNeeded { get { return true; } }

            // return the list of platforms this action is supported on
            public override List<ePlatformType> Platforms
            {
                get
                {
                    if (mPlatforms.Count == 0)
                    {
                        mPlatforms.Add(ePlatformType.ASCF);
                    }
                    return mPlatforms;
                }
            }

            public enum eControlAction
            {
                [EnumValueDescription("Set Value")]
                SetValue,
                [EnumValueDescription("Get Value")]
                GetValue, // TODO: not supported remove
                Click,

                //remove
                [EnumValueDescription("Inject Xpath Java Script")]
                InjectGingerHTMLHelper,
                [EnumValueDescription("Set Browser Control")]
                SetBrowserControl,
                [EnumValueDescription("Selected Index")]
                SelectedIndex
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

        public override String ToString()
            {
                return "ASCFBrowserControl - " + ControlAction;
            }

            public override String ActionType
            {
                get
                {
                    return "ASCFBrowserControl: " + ControlAction.ToString();
                }
            }

            public override eImageType Image { get { return eImageType.Globe; } }
        }
}
