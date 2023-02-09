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
    public class ActPassword : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Password Action"; } }
        public override string ActionUserDescription { get { return "Perform Password Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Password actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Password action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property" +
            " that you set in Locate By type then enter the page url in value textbox and run the action.");
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
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

        public enum ePasswordAction
        {
            SetValue = 0,
            SetFocus = 2,
            Clear = 3,
            GetSize=4,
            IsDisabled=5,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        public ePasswordAction PasswordAction
        {
            get
            {
                return (ePasswordAction)GetOrCreateInputParam<ePasswordAction>(nameof(PasswordAction), ePasswordAction.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PasswordAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "TextBox:" + PasswordAction.ToString();
            }
        }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.PasswordAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.PasswordAction);
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


            Type currentType = GetActionTypeByElementActionName(this.PasswordAction);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where neame should be changed. Than at default case - all names that have no change
                switch (this.PasswordAction)
                {
                    case ePasswordAction.SetValue:
                        newAct.ElementAction = ActUIElement.eElementAction.SendKeys;
                        break;
                    default:
                        newAct.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), this.PasswordAction.ToString());
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            if (!string.IsNullOrEmpty(this.LocateValue))
                newAct.ElementLocateValue = String.Copy(this.LocateValue);
            if (!uIElementTypeAssigned)
                newAct.ElementType = eElementType.TextBox;
            newAct.Active = true;

            return newAct;
        }

        Type GetActionTypeByElementActionName(ePasswordAction dropDownElementAction)
        {
            Type currentType = null;
            switch (dropDownElementAction)
            {
                case ePasswordAction.SetValue:
                case ePasswordAction.GetSize:
                case ePasswordAction.IsDisabled:
                case ePasswordAction.GetWidth:
                case ePasswordAction.GetHeight:
                case ePasswordAction.GetStyle:
                    currentType = typeof(ActUIElement);
                    break;
                    //default:
                    //    throw new Exception("Converter error, missing Action translator for - " + dropDownElementAction);
            }
            return currentType;
        }
    }
}