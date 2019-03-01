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
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;

namespace GingerCore.Actions
{
    public class ActMenuItem : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Menu Item Action"; } }
        public override string ActionUserDescription { get { return "Menu Item"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate menu items.");
        }        

        public override string ActionEditPage { get { return "ActMenuItemEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {                    
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.ASCF);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                }
                return mPlatforms;
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
        public enum eMenuAction
        {
            Click = 1,
            Expand=6,
            Collapse=7
        }

        [IsSerializedForLocalRepository]
        public eMenuAction MenuAction { get; set; }

        public override String ToString()
        {
            return "MenuItem: " + GetInputParamValue("Value");
        }
        public override String ActionType
        {
            get
            {
                return "MenuItem:" + MenuAction.ToString();
            }
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

            switch (this.MenuAction)
            {
                case eMenuAction.Click:
                    newAct.ElementAction = ActUIElement.eElementAction.Click;
                    break;
                case eMenuAction.Expand:
                    newAct.ElementAction = ActUIElement.eElementAction.Expand;
                    break;
                case eMenuAction.Collapse:
                    newAct.ElementAction = ActUIElement.eElementAction.Collapse;
                    break;
                default:
                    newAct.ElementAction = ActUIElement.eElementAction.Unknown;
                    break;
            }
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
            MapMenuActionItems(this.LocateValue, newAct);
            newAct.Active = true;
            return newAct;
        }

        public void MapMenuActionItems(String locateValue, ActUIElement newAct)
        {
            if (!String.IsNullOrEmpty(locateValue))
            {
                // check if the current action is a garbage action or not
                if (locateValue.Contains("com.amdocs.crm.workspace.OpenWindows:OpenedWindowsTree"))
                {
                    newAct.Active = false;
                    newAct.Description = "Please remove this action - " + this.Description;
                    return;
                }
                int slash = locateValue.IndexOf("/");
                string menuLocateValue = SplitWithEscape(locateValue, '/');
                foreach (ActUIElement.eElementAction menuAction in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                {
                    if (this.MenuAction.ToString().Equals(menuAction.ToString()))
                    {
                        newAct.ElementAction = menuAction;
                    }
                }

                newAct.ElementType = eElementType.MenuItem;
                if (menuLocateValue.IndexOf(";") != -1)
                {
                    newAct.ElementLocateValue = menuLocateValue.Substring(0, menuLocateValue.IndexOf(';'));
                    newAct.AddOrUpdateInputParamValue(ActUIElement.Fields.Value, locateValue.Substring(menuLocateValue.IndexOf(';') + 1));// menuLocateValue.Substring(menuLocateValue.IndexOf(';') + 1, menuLocateValue.Length - menuLocateValue.IndexOf(';') - 2));
                }
                else
                    newAct.ElementLocateValue = menuLocateValue;
            }
        }

        public string SplitWithEscape(String sourceString, Char Delimiter)
        {
            List<string> tempList = new List<string>();

            if (sourceString.IndexOf("/") != -1)
            {
                sourceString = sourceString.Replace("/" + Delimiter, "%");

                string[] SplitedString = sourceString.Split(Delimiter);
                string returnList = string.Empty;

                for (int i = 0; i < SplitedString.Length; i++)
                {
                    string TempStr = SplitedString[i];
                    TempStr = TempStr.Replace('%', Delimiter);
                    tempList.Add(TempStr);
                }
                for (int i = 0; i < tempList.Count - 1; i++)
                {
                    returnList += tempList[i] + ";";
                }
                return returnList;
            }
            else
                return sourceString;
        }

    }
}
