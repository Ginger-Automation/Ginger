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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using Couchbase.Utils;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NJsonSchema.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

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

 
        public enum eSyncOperation
        {
            [EnumValueDescription("Element is Visible")]
            ElementIsVisible = 1,

            [EnumValueDescription("Element Exists")]
            ElementExists = 2,

            [EnumValueDescription("Element to be Clickable")]
            ElementToBeClickable = 3,

            [EnumValueDescription("Text Matches")]
            TextMatches = 4,

            [EnumValueDescription("Alert is Present")]
            AlertIsPresent = 5,

            [EnumValueDescription("Element is Selected")]
            ElementIsSelected = 6,

            [EnumValueDescription("Attribute Matches")]
            AttributeMatches = 7,

            [EnumValueDescription("URL Matches")]
            UrlMatches = 8,

            [EnumValueDescription("Page has been Loaded")]
            PageHasBeenLoaded = 9,

            [EnumValueDescription("Invisibility of Element Located By")]
            InvisibilityOfElementLocated = 10,

            [EnumValueDescription("Presence of All Elements Located By")]
            PresenceOfAllElementsLocatedBy = 11,

            [EnumValueDescription("Visibility of All Elements Located By")]
            VisibilityOfAllElementsLocatedBy = 12,

            [EnumValueDescription("Invisibility of All Elements Located By")]
            InvisibilityOfAllElementsLocatedBy = 13,

            [EnumValueDescription("Frame to be Available and Switch to it")]
            FrameToBeAvailableAndSwitchToIt = 14,

            [EnumValueDescription("Selected of All Elements Located By")]
            SelectedOfAllElementsLocatedBy = 15,

            [EnumValueDescription("Enability of All Elements Located By")]
            EnabilityOfAllElementsLocatedBy = 16,

            
        }

        public eSyncOperation SyncOperations
        {
            get { return GetOrCreateInputParam<eSyncOperation>(nameof(SyncOperations), eSyncOperation.ElementIsVisible); }
            set
            {
                GetOrCreateInputParam(nameof(SyncOperations)).Value = value.ToString();
            }
        }



        public int? WaitTime
        {
            get
            {


                int i;
                return int.TryParse(GetOrCreateInputParam(nameof(WaitTime)).Value, out i) ? i : (int?)null;

            }
            set
            {
                GetOrCreateInputParam(nameof(WaitTime)).Value = value.ToString();
            }
        }

        public string AttributeValue
        {
            get
            {
                return GetOrCreateInputParam(nameof(AttributeValue)).Value;
            }
            set
            {
                GetOrCreateInputParam(nameof(AttributeValue)).Value = value;
            }
        }
        public string AttributeName
        {
            get
            {
                return GetOrCreateInputParam(nameof(AttributeName)).Value;
            }
            set
            {
                GetOrCreateInputParam(nameof(AttributeName)).Value = value;
            }
        }
        public string TxtMatchInput
        {
            get
            {
                return GetOrCreateInputParam(nameof(TxtMatchInput)).Value;
            }
            set
            {
                GetOrCreateInputParam(nameof(TxtMatchInput)).Value = value;
            }
        }
        public string UrlMatches
        {
            get
            {
                return GetOrCreateInputParam(nameof(UrlMatches)).Value;
            }
            set
            {
                GetOrCreateInputParam(nameof(UrlMatches)).Value = value;
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
                return "NewSmartSync: " + SyncOperations.ToString();
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

