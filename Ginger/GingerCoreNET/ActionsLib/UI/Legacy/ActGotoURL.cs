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
using Amdocs.Ginger.CoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    //This class is for Browser actions
    //TODO: Replace to ActBrowser !? what if it is not browser? TBD

    public class ActGotoURL : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Goto URL Action"; } }
        public override string ActionUserDescription { get { return "Goto URL Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to open a url in ginger.To open an url,just put url in value and run the action.");            
        }        

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
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
                return "Goto URL";
            }
        }
        public override eImageType Image { get { return eImageType.Globe; } }

        //
        // IObsoleteAction part
        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(this.ActionType);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(this.ActionType);
            if (currentType == typeof(ActBrowserElement))
            {
                ActBrowserElement actBrowserElement = new ActBrowserElement();
                return actBrowserElement.ActionDescription;
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
            AutoMapper.MapperConfiguration mapConfigBrowserElementt = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActBrowserElement>(); });
            ActBrowserElement NewActBrowserElement = mapConfigBrowserElementt.CreateMapper().Map<Act, ActBrowserElement>(this);

            Type currentType = GetActionTypeByElementActionName(this.ActionType);
            if (currentType == typeof(ActBrowserElement))
            {
                switch (this.ActionType)
                {
                    case "Goto URL":
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.GotoURL;
                        break;
                }
            }

            if (currentType == typeof(ActBrowserElement))
            {
                return NewActBrowserElement;
            }
            return null;
        }

        Type GetActionTypeByElementActionName(string actionType)
        {
            Type currentType = null;
            switch (actionType)
            {
                case "Goto URL":
                    currentType = typeof(ActBrowserElement);
                    break;
            }
            return currentType;
        }
    }
}
