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
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    public class ActHandleBrowserAlert : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Handle Browser Alerts"; } }
        public override string ActionUserDescription { get { return "Handle Browser Alerts"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to test/handle browser alert on any web pages");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select type of action from Action Type drop down and then enter the page url in value textbox and run the action.");
        }

        public override string ActionEditPage { get { return "ActHandleBrowserAlert"; } }
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

        public enum eHandleBrowseAlert
        {
            AcceptAlertBox = 0,
            DismissAlertBox = 2,
            GetAlertBoxText = 3,
            SendKeysAlertBox = 4,
        }

        public eHandleBrowseAlert GenElementAction
        {
            get
            {
                return (eHandleBrowseAlert)GetOrCreateInputParam<eHandleBrowseAlert>(nameof(GenElementAction), eHandleBrowseAlert.AcceptAlertBox);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(GenElementAction), value.ToString());
            }
        }

        public override String ToString()
        {
            return "Generic Web Element: " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Generic Web Element: " + GenElementAction.ToString();
            }
        }
        public override eImageType Image { get { return eImageType.Warn; } }


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

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Web;
        }
        Type IObsoleteAction.TargetAction()
        {
            return GetActionTypeByElementActionName(GenElementAction);
        }

        String IObsoleteAction.TargetActionTypeName()
        {
            Type currentType = GetActionTypeByElementActionName(GenElementAction);
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
        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfigBrowserElementt = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActBrowserElement>(); });
            ActBrowserElement NewActBrowserElement = mapConfigBrowserElementt.CreateMapper().Map<Act, ActBrowserElement>(this);

            Type currentType = GetActionTypeByElementActionName(GenElementAction);
            if (currentType == typeof(ActBrowserElement))
            {
                switch (GenElementAction)
                {
                    case eHandleBrowseAlert.AcceptAlertBox:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.AcceptMessageBox;
                        break;
                    case eHandleBrowseAlert.DismissAlertBox:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.DismissMessageBox;
                        break;
                    case eHandleBrowseAlert.GetAlertBoxText:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.GetMessageBoxText;
                        break;
                    case eHandleBrowseAlert.SendKeysAlertBox:
                        NewActBrowserElement.ControlAction = ActBrowserElement.eControlAction.SetAlertBoxText;
                        break;
                    default:
                        NewActBrowserElement.ControlAction = (ActBrowserElement.eControlAction)System.Enum.Parse(typeof(ActBrowserElement.eControlAction), GenElementAction.ToString());
                        break;
                }
            }

            if (currentType == typeof(ActBrowserElement))
            {
                return NewActBrowserElement;
            }
            return null;
        }

        Type GetActionTypeByElementActionName(eHandleBrowseAlert genElementAction)
        {
            Type currentType = null;

            switch (genElementAction)
            {
                case eHandleBrowseAlert.AcceptAlertBox:
                case eHandleBrowseAlert.DismissAlertBox:
                case eHandleBrowseAlert.GetAlertBoxText:
                case eHandleBrowseAlert.SendKeysAlertBox:
                    currentType = typeof(ActBrowserElement);
                    break;

                    //default:
                    //    throw new Exception("Converter error, missing Action translator for - " + GenElementAction);
            }
            return currentType;
        }
    }
}