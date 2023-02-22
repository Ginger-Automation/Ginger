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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.MainFrame
{
    public class ActMainframeGetDetails : Act
    {
        public override string ActionDescription { get { return "Get Details from MainFrame"; } }
        public override string ActionUserDescription { get { return "Get Details from MainFrame"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to get Text/Details of a filed at a particular location in Terminal");
        }

        public override string ActionEditPage { get { return "Mainframe.ActMainFrameGetDetailsEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        public new static partial class Fields
        {
            public static string DetailsToFetch = "DetailsToFetch";
            public static string TextInstanceType = "TextInstanceType";
            public static string TextInstanceNumber = "TextInstanceNumber";

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
                return "Get Details from MainFrame";
            }
        }

        public enum eDetailsToFetch
        {
            GetText,
            GetDetailsFromText,
            GetAllEditableFeilds,
            GetCurrentScreenAsXML,
        }
        public enum eTextInstance
        {
            FirstInstance,
            AllInstance,
            InstanceN,
            AfterCaretPosition

        }

        public eDetailsToFetch DetailsToFetch
        {
            get
            {
                return (eDetailsToFetch)GetOrCreateInputParam<eDetailsToFetch>(nameof(DetailsToFetch), eDetailsToFetch.GetText);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DetailsToFetch), value.ToString());
            }
        }
        public eTextInstance TextInstanceType
        {
            get
            {
                return (eTextInstance)GetOrCreateInputParam<eTextInstance>(nameof(TextInstanceType), eTextInstance.FirstInstance);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(TextInstanceType), value.ToString());
            }
        }

        public string TextInstanceNumber
        {
            get
            {
                return GetOrCreateInputParam(nameof(TextInstanceNumber)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(TextInstanceNumber), value);
            }
        }

    }
}
