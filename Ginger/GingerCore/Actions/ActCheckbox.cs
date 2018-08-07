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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Actions
{
    //This class is for UI checkbox elemnet
    public class ActCheckbox : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Check Box Action"; } }
        public override string ActionUserDescription { get { return "Check/Un-Check a checkbox object"; } }
        
        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to automate a check/Un-check an object from type Checkbox." + Environment.NewLine + Environment.NewLine + "For Mobile use this action only in case running the flow on the native browser.");
        }        

        public override string ActionEditPage { get { return null; } }
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
                    mPlatforms.Add(ePlatformType.Web);
                    // Since, the action isn't supported by Windows Platform hence, it's commented
                    
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string CheckboxAction = "CheckboxAction";         
        }
        
        public enum eCheckboxAction
        {
            Check = 1,
            Uncheck = 2,
            SetFocus = 3,
            GetValue=4,
            IsDisplayed = 5,
            Click=6,
            IsDisabled=7,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        [IsSerializedForLocalRepository]
        public eCheckboxAction CheckboxAction { get; set; }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return new List<ePlatformType>() { ePlatformType.Web, ePlatformType.Mobile }; } }

        public override String ActionType
        {
            get
            {
                return "Checkbox:" + CheckboxAction.ToString();
            }
        }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.CheckboxAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.CheckboxAction);
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
            bool uIElementTypeAssigned = false;
            AutoMapper.MapperConfiguration mapConfig = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newAct = mapConfig.CreateMapper().Map<Act, ActUIElement>(this);


            Type currentType = GetActionTypeByElementActionName(this.CheckboxAction);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where neame should be changed. Than at default case - all names that have no change
                switch (this.CheckboxAction)
                {
                    case eCheckboxAction.Check:
                    case eCheckboxAction.Uncheck:
                        newAct.ElementAction = ActUIElement.eElementAction.Click;
                        break;
                    case eCheckboxAction.IsDisabled:
                        newAct.ElementAction = ActUIElement.eElementAction.IsVisible;
                        break;
                    default:
                        newAct.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), this.CheckboxAction.ToString());
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            newAct.ElementLocateValue = String.Copy(this.LocateValue);
            if (!uIElementTypeAssigned)
                newAct.ElementType = eElementType.CheckBox;
            newAct.Active = true;

            return newAct;
        }

        Type GetActionTypeByElementActionName(eCheckboxAction dropDownElementAction)
        {
            Type currentType = null;
            switch (dropDownElementAction)
            {
                case eCheckboxAction.Check:
                case eCheckboxAction.Uncheck:
                case eCheckboxAction.GetValue:
                case eCheckboxAction.IsDisplayed:
                case eCheckboxAction.Click:
                case eCheckboxAction.IsDisabled:
                case eCheckboxAction.GetWidth:
                case eCheckboxAction.GetHeight:
                case eCheckboxAction.GetStyle:
                    currentType = typeof(ActUIElement);
                    break;
                    //default:
                    //    throw new Exception("Converter error, missing Action translator for - " + dropDownElementAction);
            }
            return currentType;
        }
    }
}
