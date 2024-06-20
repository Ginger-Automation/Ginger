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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using Couchbase.Utils;
using GingerCore.Actions.Common;
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
        public class ActWebSmartSync : Act, IActPluginExecution
    {
    
        public override string ActionDescription { get { return "Web Smart Sync Action"; } }
        public override string ActionUserDescription { get { return "Web Smart Sync"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("The following operations are commonly used in automated web testing, especially with tools like Selenium WebDriver. These operations help ensure that certain conditions are met before moving on to the next step in a test script. Here’s a brief explanation of each one:\r\n1.  ElementIsVisible: Verifies if an element is present on both the webpage’s structure (DOM) and visible to the user.\r\n2.  ElementExists: Checks if an element exists on the webpage, regardless of whether it is visible.\r\n3.  AlertIsPresent: Waits until an alert message pops up on the webpage.\r\n4.  ElementIsSelected: Confirms if a form element like a checkbox or radio button is currently selected.\r\n5.  PageHasBeenLoaded: Ensures that the entire webpage has been loaded and is ready for interaction.\r\n6.  ElementToBeClickable: Waits until an element is both visible and enabled, indicating that it can be clicked.\r\n7.  TextMatches: Waits until the text of an element matches a specified pattern. Input Text is case-sensitive and does the contains search.\r\n8.  AttributeMatches: Waits until a specific attribute of an element matches a specified pattern.\r\n9.  EnabilityOfAllElementsLocatedBy: Checks if all the elements found by a given locator are enabled and can be interacted.\r\n10.  FrameToBeAvailableAndSwitchToIt: Waits until a frame is available to switch to and then switches to it.\r\n11.  InvisibilityOfAllElementsLocatedBy: Waits until all the elements found by a given locator are invisible or not present.\r\n12.  InvisibilityOfElementLocated: Waits until a specific element is no longer visible or not present.\r\n13.  PresenceOfAllElementsLocatedBy: Ensures that all the elements found by a given locator are present in the webpage’s structure (DOM).\r\n14.  SelectedOfAllElementsLocatedBy: Ensures that all the elements found by a given locator are selected.\r\n15.  UrlMatches: Waits until the URL of the current page matches a specified pattern.\r\n16.  VisibilityOfAllElementsLocatedBy: Ensures that all the elements found by a given locator are visible on the webpage.\r\n\n \"Supported locator values include: ByXPath, ByID, ByName, ByClassName, ByCssSelector, ByLinkText, and ByTagName.\"");
        }

        public override string ActionEditPage { get { return "ActWebSmartSyncEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
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

        public new static partial class Fields
        {
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ValueToSelect = "ValueToSelect";
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

        public eLocateBy ElementLocateBy
        {
            get { return GetOrCreateInputParam(nameof(ElementLocateBy), eLocateBy.ByClassName); }
            set
            {
                GetOrCreateInputParam(nameof(ElementLocateBy)).Value = value.ToString();

                OnPropertyChanged(nameof(ElementLocateBy));

            }
        }

        public string ElementLocateValue
        {
            get
            {
                return GetOrCreateInputParam(nameof(ElementLocateValue)).Value;
            }
            set
            {
                GetOrCreateInputParam(nameof(ElementLocateValue)).Value = value;
                OnPropertyChanged(nameof(ElementLocateValue));
            }
        }
        public string ElementLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(nameof(ElementLocateValue));
            }
        }
        public eSyncOperation SyncOperations
        {
            get { return GetOrCreateInputParam<eSyncOperation>(nameof(SyncOperations), eSyncOperation.ElementIsVisible); }
            set
            {
                GetOrCreateInputParam(nameof(SyncOperations)).Value = value.ToString();
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
                OnPropertyChanged(nameof(AttributeValue));
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
                OnPropertyChanged(nameof(AttributeName));
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
                OnPropertyChanged(nameof(TxtMatchInput));
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
                OnPropertyChanged(nameof(UrlMatches));
            }
        }

       
     
        public override String ToString()
        {
            return "WebSmartSync: " + GetInputParamValue("Value");
        }


        public string GetName()
        {
            return "WebSmartSyncAction";
        }

        public override String ActionType
        {
            get
            {
                return "WebSmartSync: " + SyncOperations.ToString();
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

