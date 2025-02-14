#region License
/*
Copyright © 2014-2025 European Support Limited

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
    public class ActWindow : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Window Action"; } }

        public override string ActionUserDescription { get { return "Window Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate windows action like minimize,maximize,close etc.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select Locate By and locate value and select windows action type and run the action.");
        }

        public override string ActionEditPage { get { return "ActWindowEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    //Unable to use in web platform
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.ASCF);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Java);
                }
                return mPlatforms;
            }
        }

        public override List<eLocateBy> AvailableLocateBy()
        {
            List<eLocateBy> l = [eLocateBy.ByName, eLocateBy.ByTitle, eLocateBy.ByXPath];

            return l;
        }

        //Available window actions
        public enum eWindowActionType
        {
            Switch,
            Close,
            Maximize,
            Minimize,
            Restore,
            IsExist
        }

        public eWindowActionType WindowActionType
        {
            get
            {
                return GetOrCreateInputParam<eWindowActionType>(nameof(WindowActionType), eWindowActionType.Switch);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(WindowActionType), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "Window";
            }
        }
        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if (platform is ePlatformType.Web or ePlatformType.NA or ePlatformType.Mobile)
            {
                return true;
            }
            else
            {
                return false;
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
            return ePlatformType.Java;
        }
        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfig = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newAct = mapConfig.CreateMapper().Map<Act, ActUIElement>(this);
            newAct.ElementType = eElementType.Window;
            newAct.ElementLocateValue = this.LocateValue;

            newAct.ElementAction = this.WindowActionType switch
            {
                eWindowActionType.Close => ActUIElement.eElementAction.CloseWindow,
                eWindowActionType.IsExist => ActUIElement.eElementAction.IsExist,
                eWindowActionType.Switch => ActUIElement.eElementAction.Switch,
                _ => ActUIElement.eElementAction.Unknown,
            };
            newAct.ElementLocateBy = LocateBy switch
            {
                eLocateBy.ByName => eLocateBy.ByName,
                eLocateBy.ByTitle => eLocateBy.ByTitle,
                eLocateBy.ByXPath => eLocateBy.ByXPath,
                _ => eLocateBy.Unknown,
            };
            newAct.Active = false;
            newAct.Description = "*** Partially Converted *** - " + this.Description + System.Environment.NewLine + "Please update the Locate By and Locate Value as shown in Windows Explorer for Java Driver.";
            return newAct;
        }
    }
}