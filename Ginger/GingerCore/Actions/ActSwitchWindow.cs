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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCore.Repository;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;

namespace GingerCore.Actions
{
    public class ActSwitchWindow : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Switch Window Action"; } }
        public override string ActionUserDescription { get { return "Performs Switch Window Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Switch Window actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Switch Window action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property " +
            "that you set in Locate By type.Then enter the page url in value textbox and run the action.");
        }

        public new static partial class Fields
        {
            public static string WaitTime = "WaitTime";
        }

        public override string ActionEditPage { get { return "ActSwitchWindowEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return false; } }
        
        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.ASCF);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Java);
                }
                return mPlatforms;
            }
        }
        
        public override String ActionType
        {
            get
            {
                return "Window";
            }
        }

        public Int32 WaitTime
        {
            get
            {
                string val = GetInputParamValue("WaitTime");
                Int32 intVal = 30;
                if (!String.IsNullOrEmpty(val))
                {
                    Int32.TryParse(val, out intVal);
                }
                if (intVal <= 0)
                {
                    intVal = 30;
                }
                return intVal;
            }
            set
            {
                AddOrUpdateInputParamValue("WaitTime", value.ToString());
            }
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
            newAct.ElementType = eElementType.Unknown;
            newAct.ElementType = eElementType.Window;
            newAct.ElementAction = ActUIElement.eElementAction.Switch;
            newAct.ElementLocateValue = this.LocateValue;
           
            switch (LocateBy)
            {
                case eLocateBy.ByName:
                    newAct.ElementLocateBy = eLocateBy.ByName;
                    break;
                case eLocateBy.ByTitle:
                    newAct.ElementLocateBy = eLocateBy.ByTitle;
                    break;
                default:
                    newAct.ElementLocateBy = eLocateBy.Unknown;
                    break;
            }

            // check if the current action is a garbage action or not
            newAct.Active = false;
            newAct.Description = "*** Partially Converted *** - " + this.Description + System.Environment.NewLine + "Please update the Locate By and Locate Value as shown in Windows Explorer for Java Driver.";
            return newAct;
        }

        internal PayLoad GetPayLoad()
        {
            PayLoad PL = new PayLoad("SwitchWindow");
            if(string.IsNullOrEmpty(LocateValueCalculated) == false)
                PL.AddValue(LocateValueCalculated);
            else
                PL.AddValue(ValueForDriver);
            PL.ClosePackage();
            return PL;
        }
    }
}
