#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    //This class is for UI link element
    public class ActNewSmartSync : Act, IActPluginExecution
    {
        public override string ActionDescription { get { return "New Smart Sync Action"; } }
        public override string ActionUserDescription { get { return "New Smart Sync"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate smart sync.");
        }

        public override string ActionEditPage { get { return "ActNewSmartSyncEditPage"; } }
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
                 
                }
                return mPlatforms;
            }
        }

        public enum eNewSmartSyncAction
        {
            ElementExists = 1,
            ElementIsVisible = 2,
            AlertIsPresent = 3,
            ElementIsSelected=4,
            PageHasBeenLoaded=5,

            /* EnabilityOfAllElementsLocatedBy = 4,
             ElementIsSelected = 5,
             InvisibilityOfElementLocated = 6,
             InvisibilityOfElementLocated = 7,
             UrlMatches*/

        }
        public new static partial class Fields
        {
            public static string WaitTime = "WaitTime";
            public static string NewSmartSyncAction = "NewSmartSyncAction";

        }

        public int? WaitTime
        {
            get
            {


                int i;
                return int.TryParse(GetOrCreateInputParam(Fields.WaitTime).Value, out i) ? i : (int?)null; ;

            }
            set
            {
                GetOrCreateInputParam(Fields.WaitTime).Value = value.ToString();
            }
        }

        public eNewSmartSyncAction NewSmartSyncAction
        {
            get { return GetOrCreateInputParam<eNewSmartSyncAction>(Fields.NewSmartSyncAction, eNewSmartSyncAction.ElementExists); }
            set
            {
                GetOrCreateInputParam(Fields.NewSmartSyncAction).Value = value.ToString();
            }
        }

        public override String ToString()
        {
            return "NewSmartSync: " + GetInputParamValue("Value");
        }


        public string GetName()
        {
            return "NewSmartSyncAction";
        }

        public override String ActionType
        {
            get
            {
                return "NewSmartSync: " + NewSmartSyncAction.ToString();
            }
        }
        public override eImageType Image { get { return eImageType.Refresh; } }



        public PlatformAction GetAsPlatformAction()
        {

            PlatformAction platformAction = new PlatformAction(this);

            foreach (ActInputValue aiv in this.InputValues)
            {

                string ValueforDriver = aiv.ValueForDriver;
                if (!platformAction.InputParams.ContainsKey(aiv.Param) && !String.IsNullOrEmpty(ValueforDriver))
                {
                    platformAction.InputParams.Add(aiv.Param, ValueforDriver);
                }
            }


            return platformAction;
        }
    }
}

