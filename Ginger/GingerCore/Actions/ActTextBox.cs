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

namespace GingerCore.Actions
{
    //This class is for Text Box actions
    public class ActTextBox : Act
    {
        public override string ActionDescription { get { return "TextBox Action"; } }
        public override string ActionUserDescription { get { return "Click on a TextBox object"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action when working with TextBox control");
            TBH.AddLineBreak();
            TBH.AddText("For example setting value in First Name TextBox for login screen");
            TBH.AddLineBreak();
            TBH.AddImage("TextBox.png",200,50);
            TBH.AddLineBreak();
            TBH.AddHeader1("the following sub actions can be selected:");
            TBH.AddLineBreak();
            TBH.AddText("Set Value");
            TBH.AddLineBreak();
            TBH.AddText("Set focus");
            TBH.AddLineBreak();
            TBH.AddText("Clear");
            TBH.AddLineBreak();
            TBH.AddText("Get Value");
            TBH.AddLineBreak();
            TBH.AddText("Is Required");
            TBH.AddLineBreak();
            TBH.AddText("Get Font");
            TBH.AddLineBreak();
            TBH.AddText("Is Prepopulated");
            TBH.AddLineBreak();
            TBH.AddText("Is Displayed");
            TBH.AddLineBreak();
            TBH.AddText("Get Input Length");
            TBH.AddLineBreak();
            TBH.AddText("Get Width");
            TBH.AddLineBreak();
            TBH.AddText("Get Height");
            TBH.AddLineBreak();
            TBH.AddText("Get Style");
            
        }        

        public override string ActionEditPage { get { return "ActTextBoxEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    // Since, the action isn't supported by Windows Platform hence, it's commented
                    
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public enum eTextBoxAction
        {
            SetValueFast = 0,
            SetValue = 1,
            SetFocus = 2,
            Clear = 3,
            GetValue = 4,
            IsDisabled=5,
            IsRequired = 6,
            GetFont = 7,
            IsPrepopulated= 8,
            IsDisplayed = 9,
            GetInputLength = 10,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        [IsSerializedForLocalRepository]
        public eTextBoxAction TextBoxAction { get; set; }

        public override String ActionType
        {
            get
            {
                return "TextBox:" + TextBoxAction.ToString();
            }
        }
        
        public override System.Drawing.Image Image { get { return Resources.ActTextBox; } } 
    }
}
