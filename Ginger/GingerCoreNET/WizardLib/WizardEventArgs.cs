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

using System;
using System.Collections.Generic;

namespace GingerWPF.WizardLib
{
    public enum EventType
    {
        Init,  // happen when the Wizard load the page before showing the window
        Next,  // When user click Next button 
        Prev,  // When user click Prev button
        Reset,  // when user click Reset button
        Validate,  // When user click finish each page is validate before calling finish
        Finish,  // called after user click Finish and all pages passed validation
        Active, // The page became active can be from next/prev or click on the list of pages
        LeavingForNextPage,
        Cancel// The page became active can be from next/prev or click on the list of pages        
    }
   
    public class WizardEventArgs : EventArgs
    {
        private WizardBase mWizard;
        private EventType mEventType;
        public bool CancelEvent = false;

        public WizardBase Wizard { get { return mWizard; } }

        public EventType EventType { get { return mEventType; } }

        // public static bool IgnoreDefaultNextButtonSettings { get; set; }
        // public static bool IgnoreDefaultPrevButtonSettings { get; set; }

        // when calling Validation for each page in case of errors the errors are added here.
        //public List<string> Errors = new List<string>();

        public WizardEventArgs(WizardBase Wizard, EventType EventType)
        {
            mWizard = Wizard;
            mEventType = EventType;
        }

        //public void AddError(string err)
        //{
        //    Errors.Add(err);
        //}
    }
}
