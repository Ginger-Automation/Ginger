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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.Common
{
    // this action is for Phone Actions Like:
    // Dial a number
    // Wait for ring
    // Hang up
    // Get IMEI - *#06#
    // and more

    public class ActPhone : Act
    {
        public override string ActionDescription { get { return "Phone Action"; } }
        public override string ActionUserDescription { get { return "Phone Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need run Phone action like: Dial, Wait for ring");
        }

        public override string ActionEditPage { get { return "_Common.ActPhoneEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static String PhoneAction = "PhoneAction";
            public static String PhoneNumber = "PhoneNumber";            
        }

        public enum ePhoneAction
        {
            Dial,
            EndCall,
            WaitForRing            
        }

        [IsSerializedForLocalRepository]
        public ePhoneAction PhoneAction { get; set; }

        public override String ActionType
        {
            get
            {
                return "PhoneAction. " + PhoneAction;
            }
        }

        public override System.Drawing.Image Image { get { return Resources.MobileDevice_16x16; } }
    }
}