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
using System.Linq;

namespace GingerCore.Actions
{
    //This class is for UI DropDownList elemnet
    public class ActDropDownList : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Drop Down Action"; } }
        public override string ActionUserDescription { get { return "Drop Down Action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to perform any drop down actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a drop down action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property"+ 
            "that you set in Locate By type.Then select Action Type, e.g- ClearSelectedValue,getFocus,getCSS etc and then enter the page url in value textbox and run the action.");
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

        public new static partial class Fields
        {
            public static string ActDropDownListAction = "ActDropDownListAction";
        }

        public enum eActDropDownListAction
        {
            SetSelectedValueByIndex = 1,
            SetSelectedValueByValue = 2,
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

        [IsSerializedForLocalRepository]
        public eActDropDownListAction ActDropDownListAction { get; set; }

        public override String ActionType
        {
            get
            {
                return "DropDownList:" + ActDropDownListAction.ToString();
            }
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActUIElement);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            ActUIElement actUIElement = new ActUIElement();
            return actUIElement.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Web;
        }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if (platform == ePlatformType.ASCF)
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

            switch (this.ActDropDownListAction)
            {
                case eActDropDownListAction.SetSelectedValueByValue:
                    newAct.ElementAction = ActUIElement.eElementAction.SetSelectedValueByValue;
                    break;
                case eActDropDownListAction.SetSelectedValueByIndex:
                    newAct.ElementAction = ActUIElement.eElementAction.SetSelectedValueByIndex;
                    break;
                case eActDropDownListAction.SetSelectedValueByText:
                    newAct.ElementAction = ActUIElement.eElementAction.SetSelectedValueByText;
                    break;
                case eActDropDownListAction.ClearSelectedValue:
                    newAct.ElementAction = ActUIElement.eElementAction.ClearSelectedValue;
                    break;
                case eActDropDownListAction.SetFocus:
                    newAct.ElementAction = ActUIElement.eElementAction.SetFocus;
                    break;
                case eActDropDownListAction.GetValidValues:
                    newAct.ElementAction = ActUIElement.eElementAction.GetValidValues;
                    break;
                case eActDropDownListAction.GetSelectedValue:
                    newAct.ElementAction = ActUIElement.eElementAction.GetSelectedValue;
                    break;
                case eActDropDownListAction.IsPrepopulated:
                    newAct.ElementAction = ActUIElement.eElementAction.IsPrepopulated;
                    break;
                case eActDropDownListAction.GetFont:
                    newAct.ElementAction = ActUIElement.eElementAction.GetFont;
                    break;
                case eActDropDownListAction.GetWidth:
                    newAct.ElementAction = ActUIElement.eElementAction.GetWidth;
                    break;
                case eActDropDownListAction.GetHeight:
                    newAct.ElementAction = ActUIElement.eElementAction.GetHeight;
                    break;
                case eActDropDownListAction.GetStyle:
                    newAct.ElementAction = ActUIElement.eElementAction.GetStyle;
                    break;
                default:
                    newAct.ElementAction = ActUIElement.eElementAction.Unknown;
                    break;
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            newAct.ElementLocateValue = String.Copy(this.LocateValue);
            newAct.ElementType = eElementType.ComboBox;           
            newAct.Active = true;

            return newAct;
        }
    }
}
