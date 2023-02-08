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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    //This class is for Text Box actions
    public class ActTextBox : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "TextBox Action"; } }
        public override string ActionUserDescription { get { return "Click on a TextBox object"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
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

        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

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

        public eTextBoxAction TextBoxAction
        {
            get
            {
                return (eTextBoxAction)GetOrCreateInputParam<eTextBoxAction>(nameof(TextBoxAction), eTextBoxAction.SetValueFast);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(TextBoxAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "TextBox:" + TextBoxAction.ToString();
            }
        }
        
        public override eImageType Image { get { return eImageType.TextBox; } }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.TextBoxAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.TextBoxAction);
            if (currentType == typeof(ActUIElement))
            {
                ActUIElement actUIElement = new ActUIElement();
                return actUIElement.ActionDescription;
            }
            else
            {
                return string.Empty;
            }
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Web;
        }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if (platform == ePlatformType.Web || platform == ePlatformType.NA || platform == ePlatformType.Mobile)
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
            AutoMapper.MapperConfiguration mapConfig = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newAct = mapConfig.CreateMapper().Map<Act, ActUIElement>(this);


            Type currentType = GetActionTypeByElementActionName(this.TextBoxAction);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where neame should be changed. Than at default case - all names that have no change
                switch (this.TextBoxAction)
                {
                    case eTextBoxAction.SetValueFast:
                        newAct.ElementAction = ActUIElement.eElementAction.SetValue;
                        break;
                    case eTextBoxAction.SetValue:
                        newAct.ElementAction = ActUIElement.eElementAction.SetText;
                        break;
                    case eTextBoxAction.Clear:
                        newAct.ElementAction = ActUIElement.eElementAction.ClearValue;
                        break;
                    case eTextBoxAction.IsPrepopulated:
                        newAct.ElementAction = ActUIElement.eElementAction.IsValuePopulated;
                        break;
                    case eTextBoxAction.IsDisplayed:
                        newAct.ElementAction = ActUIElement.eElementAction.IsVisible;
                        break;
                    case eTextBoxAction.GetInputLength:
                        newAct.ElementAction = ActUIElement.eElementAction.GetTextLength;
                        break;
                    default:
                        newAct.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), this.TextBoxAction.ToString());
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            if (!string.IsNullOrEmpty(this.LocateValue))
                newAct.ElementLocateValue = String.Copy(this.LocateValue);
            newAct.ElementType = eElementType.TextBox;
            newAct.Active = true;

            return newAct;
        }

        Type GetActionTypeByElementActionName(eTextBoxAction dropDownElementAction)
        {
            Type currentType = null;
            switch (dropDownElementAction)
            {
                case eTextBoxAction.SetValueFast:
                case eTextBoxAction.SetValue:
                case eTextBoxAction.Clear:
                case eTextBoxAction.GetValue:
                case eTextBoxAction.IsDisabled:
                case eTextBoxAction.GetFont:
                case eTextBoxAction.IsPrepopulated:
                case eTextBoxAction.IsDisplayed:
                case eTextBoxAction.GetInputLength:
                    currentType = typeof(ActUIElement);
                    break;
                    //default:
                    //    throw new Exception("Converter error, missing Action translator for - " + dropDownElementAction);
            }
            return currentType;
        }
    }
}