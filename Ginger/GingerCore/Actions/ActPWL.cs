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
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    //This class is for UI link element
    public class ActPWL : Act
    {
        public override string ActionDescription { get { return "Two web elements distances"; } }
        public override string ActionUserDescription { get { return "Get two web elements distances"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to get two different element distances");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action, select property type from Locate By drop down and then enter property value.Then select the Action Type and url and run the action.");
        }        

        public override string ActionEditPage { get { return "ActPWLEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }


        private eLocateBy mOLocateBy{set;get;}
        private string mOLocateValue { set; get; }
        [IsSerializedForLocalRepository]
        public eLocateBy OLocateBy { get { return mOLocateBy; } set { if (mOLocateBy != value) { mOLocateBy = value; OnPropertyChanged(Fields.OLocateBy); } } }
        [IsSerializedForLocalRepository]
        public string OLocateValue { get { return mOLocateValue; } set { mOLocateValue = value; OnPropertyChanged(Fields.OLocateValue); } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }
        public new static partial class Fields
        {
            public static string OLocateValue = "OLocateValue";
            public static string OLocateBy = "OLocateBy";
        }
    
        public enum ePWLAction
        {
            GetVDistanceTop2Top=0,
            GetVDistanceTop2Bottom = 1,
            GetVDistanceBottom2Top = 2,
            GetVDistanceBottom2Bottom = 3,
            GetHDistanceLeft2Left = 4,
            GetHDistanceLeft2Right = 5,
            GetHDistanceRight2Left = 6,
            GetHDistanceRight2Right = 7,
        }

        [IsSerializedForLocalRepository]
        public ePWLAction PWLAction { get; set; }

        public override String ToString()
        {
            return "Two web elements distances:  " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Two web elements distances: " + PWLAction.ToString();
            }
        }
        public override System.Drawing.Image Image { get { return Resources.ActLink; } }
    }
}
