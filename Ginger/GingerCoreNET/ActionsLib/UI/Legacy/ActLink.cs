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
    //This class is for UI link element
    public class ActLink : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Link Action"; } }
        public override string ActionUserDescription { get { return "Click on a link object"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate a click on an object from type Link."
                                        + Environment.NewLine + Environment.NewLine +
                                           "For Mobile use this action only in case running the flow on the native browser.");
        }        

        public override string ActionEditPage { get { return "ActLinkEditPage"; } }
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

        public enum eLinkAction
        {
            Click = 0,
            Hover= 2, //This is needed for hovering to expand menus.
            GetValue=3, //for validation
            Visible=4, //for validation
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        public eLinkAction LinkAction
        {
            get
            {
                return (eLinkAction)GetOrCreateInputParam<eLinkAction>(nameof(LinkAction), eLinkAction.Click);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(LinkAction), value.ToString());
            }
        }

        public override String ToString()
        {            
                return "Link: " + GetInputParamValue("Value");            
        }

        public override String ActionType
        {
            get
            {
                return "Link: " + LinkAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.Link; } }

        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.LinkAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.LinkAction);
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

            Type currentType = GetActionTypeByElementActionName(this.LinkAction);
            if (currentType == typeof(ActUIElement))
            {
                // check special cases, where name should be changed. Than at default case - all names that have no change
                switch (this.LinkAction)
                {
                    case eLinkAction.Click:
                        newAct.ElementAction = ActUIElement.eElementAction.JavaScriptClick;
                        break;
                    case eLinkAction.Visible:
                        newAct.ElementAction = ActUIElement.eElementAction.IsVisible;
                        break;
                    default:
                        newAct.ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), this.LinkAction.ToString());
                        break;
                }
            }

            newAct.ElementLocateBy = (eLocateBy)((int)this.LocateBy);
            if (!string.IsNullOrEmpty(this.LocateValue))
                newAct.ElementLocateValue = String.Copy(this.LocateValue);
            if (!uIElementTypeAssigned)
                newAct.ElementType = eElementType.HyperLink;
            newAct.Active = true;

            return newAct;
        }

        Type GetActionTypeByElementActionName(eLinkAction dropDownElementAction)
        {
            Type currentType = null;
            switch (dropDownElementAction)
            {
                case eLinkAction.Click:
                case eLinkAction.Hover:
                case eLinkAction.GetValue:
                case eLinkAction.Visible:
                case eLinkAction.GetWidth:
                case eLinkAction.GetHeight:
                case eLinkAction.GetStyle:
                    currentType = typeof(ActUIElement);
                    break;
                    //default:
                    //    throw new Exception("Converter error, missing Action translator for - " + dropDownElementAction);
            }
            return currentType;
        }
    }
}
