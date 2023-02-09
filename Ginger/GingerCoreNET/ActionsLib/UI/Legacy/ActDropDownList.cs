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
    //This class is for UI DropDownList element
    public class ActDropDownList : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Drop Down Action"; } }
        public override string ActionUserDescription { get { return "Drop Down Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any drop down actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a drop down action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property"+ 
            " that you set in Locate By type.Then select Action Type, e.g- ClearSelectedValue,getFocus,getCSS etc and then enter the page url in value textbox and run the action.");
        }        

        public override string ActionEditPage { get { return "ActDropDownListEditPage"; } }
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
                    mPlatforms.Add(ePlatformType.ASCF);
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

        public new static partial class Fields
        {
            public static string ActDropDownListAction = "ActDropDownListAction";
        }

        public enum eActDropDownListAction
        {
            SetSelectedValueByIndex = 1,
            SetSelectedValueByValue = 0,
            SetSelectedValueByText = 3,
            ClearSelectedValue = 4,
            SetFocus = 5,
            GetValidValues = 6,
            GetSelectedValue=7,
            IsPrepopulated = 8,
            GetFont = 9,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        public eActDropDownListAction ActDropDownListAction
        {
            get
            {
                return (eActDropDownListAction)GetOrCreateInputParam<eActDropDownListAction>(nameof(ActDropDownListAction), eActDropDownListAction.SetSelectedValueByValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActDropDownListAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "DropDownList:" + ActDropDownListAction.ToString();
            }
        }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.ActDropDownListAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.ActDropDownListAction);
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
            newAct.ElementType = eElementType.Unknown;

            Type currentType = GetActionTypeByElementActionName(this.ActDropDownListAction);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where name should be changed. Than at default case - all names that have no change
                switch (this.ActDropDownListAction)
                {
                    case eActDropDownListAction.SetSelectedValueByIndex:
                        newAct.ElementAction = ActUIElement.eElementAction.SelectByIndex;
                        break;
                    case eActDropDownListAction.SetSelectedValueByValue:
                        newAct.ElementAction = ActUIElement.eElementAction.Select;
                        break;
                    case eActDropDownListAction.SetSelectedValueByText:
                        newAct.ElementAction = ActUIElement.eElementAction.SelectByText;
                        break;
                    case eActDropDownListAction.IsPrepopulated:
                        newAct.ElementAction = ActUIElement.eElementAction.IsValuePopulated;
                        break;
                    default:
                        newAct.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), this.ActDropDownListAction.ToString());
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            if (!string.IsNullOrEmpty(this.LocateValue))
                newAct.ElementLocateValue = String.Copy(this.LocateValue);
            newAct.ElementType = eElementType.ComboBox;          
            newAct.Active = true;
            newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.ValueToSelect, this.GetInputParamValue("Value"));

            return newAct;
        }

        Type GetActionTypeByElementActionName(eActDropDownListAction dropDownElementAction)
        {
            Type currentType = null;
            switch (dropDownElementAction)
            {
                case eActDropDownListAction.SetSelectedValueByValue:
                case eActDropDownListAction.SetSelectedValueByIndex:
                case eActDropDownListAction.SetSelectedValueByText:
                case eActDropDownListAction.SetFocus:
                case eActDropDownListAction.GetValidValues:
                case eActDropDownListAction.GetSelectedValue:
                case eActDropDownListAction.IsPrepopulated:
                case eActDropDownListAction.GetFont:
                case eActDropDownListAction.GetWidth:
                case eActDropDownListAction.GetHeight:
                case eActDropDownListAction.GetStyle:
                    currentType = typeof(ActUIElement);
                    break;
                //default:
                //    throw new Exception("Converter error, missing Action translator for - " + dropDownElementAction);
            }
            return currentType;
        }
    }
}
