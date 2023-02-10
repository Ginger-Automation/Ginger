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
using GingerCore.Helpers;
using System;
using System.Collections.Generic;
using Open3270;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.MainFrame
{
    public class ActMainframeSendKey : Act
    {
        public override string ActionDescription { get { return "Send Key to MainFrame"; } }
        public override string ActionUserDescription { get { return "Send Key to MainFrame"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this to Send Keys to Mainframe");
        }

        public override string ActionEditPage { get { return "Mainframe.ActMainFrameSendKeyEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public new static partial class Fields
        {
            public static string KeyToSend = "KeyToSend";
        }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                List<ePlatformType> mPf = new List<ePlatformType>();

                mPf.Add(ePlatformType.MainFrame);
                return mPf;
            }
        }

        public override String ActionType
        {
            get
            {
                return "Send Key to MainFrame";
            }
        }

        public TnKey KeyToSend
        {
            get
            {
                return (TnKey)GetOrCreateInputParam<TnKey>(nameof(KeyToSend), TnKey.Key);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(KeyToSend), value.ToString());
            }
        }
    }
}
