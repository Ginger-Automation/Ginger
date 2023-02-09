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
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    //This class is for submit
    //TODO: do we need a separate class or can combine with ActBrowser ?!
    public class ActSubmit : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Submit Action"; } }
        public override string ActionUserDescription { get { return "Performs Submit Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Submit actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Submit action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property " +
            "that you set in Locate By type.Then select Action Type and then enter the page url in value textbox and run the action.");
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

        public override String ActionType
        {
            get
            {
                return "Submit";
            }
        }

        //
        // IObsoleteAction part
        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.ActionType);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(ActionType);
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
            if (platform == ePlatformType.Web ||  platform == ePlatformType.NA || platform == ePlatformType.Mobile)
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


            Type currentType = GetActionTypeByElementActionName(ActionType);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where neame should be changed. Than at default case - all names that have no change
                switch (ActionType)
                {
                    case "Submit":
                        newAct.ElementAction = ActUIElement.eElementAction.Submit;
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            if (!string.IsNullOrEmpty(this.LocateValue))
                newAct.ElementLocateValue = String.Copy(this.LocateValue);
            if (!uIElementTypeAssigned)
                newAct.ElementType = eElementType.Button;
            newAct.Active = true;

            return newAct;
        }

        Type GetActionTypeByElementActionName(string actionType)
        {
            Type currentType = null;
            switch (actionType)
            {
                case "Submit":
                    currentType = typeof(ActUIElement);
                    break;
            }
            return currentType;
        }
    }
}
