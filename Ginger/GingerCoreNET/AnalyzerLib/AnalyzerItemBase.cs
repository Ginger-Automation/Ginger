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
using System;
using System.ComponentModel;

namespace Ginger.AnalyzerLib
{
    public abstract class AnalyzerItemBase : INotifyPropertyChanged
    {
        public enum eStatus
        {
            OK = 1,
            NeedFix = 2,
            Fixed = 3,
            CannotFix = 4,
            MissingFixHandler =5,
            FixedSaved =6,
            PartiallyFixed = 7,

        }

        public enum eSeverity
        {
            Critical = 1,
            High = 2,
            Medium = 3,            
            Low = 4            
        }

        public enum eCanFix
        {
            Yes = 1,
            No = 2,
            Maybe = 3            
        }

        public enum eType
        {
            Error = 1,
            Warning = 2,
            Info = 3
        }


        public enum eIssueCategory
        {
            MissingVariable = 1,
            VaribleUnused = 2            
        }

        public enum eCheckType
        {
            Yes = 1,
            No = 2,
            Maybe = 3
        }
        /// <summary>
        /// Category of issue e.g. missing variable or missing flow control
        /// </summary>
        public eIssueCategory IssueCategory { get; set; }

        /// <summary>
        /// Object reference which is the root of the issue 
        /// e.g. variable name in case of missing variable issue
        /// or Flow control which is missing configuration etc.
        /// </summary>
        public object IssueReferenceObject { get; set; }

        private bool mSelected;
        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        public string ItemName { get; set; }
        public string ItemParent { get; set; }
        public string ItemClass { get; set; }
        public string Description { get; set; }
        public string UTDescription { get; set; }
        public string Details { get; set; }
        public string HowToFix { get; set; }

        private eCanFix mCanAutoFix;
        public eCanFix CanAutoFix { get { return mCanAutoFix; } set { if (mCanAutoFix != value) { mCanAutoFix = value; OnPropertyChanged(nameof(CanAutoFix)); OnPropertyChanged(nameof(IsAutoFix)); } } }

        public bool IsAutoFix
        {
            get
            {
                if (CanAutoFix == eCanFix.Yes || CanAutoFix == eCanFix.Maybe)
                    return true;
                else
                    return false;
            }
        }
        public eSeverity Severity { get; set; }
        public eType IssueType { get; set; }
        public string Impact { get; set; }
        public EventHandler FixItHandler { get; set; }

        public object ErrorInfoObject = null;

        private eStatus mStatus { get; set; }
        
        public eStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public eImageType SeverityIcon
        {            
            get
            {
               switch(Severity)
                {
                    case eSeverity.Critical:
                    case eSeverity.High:
                        return eImageType.HighWarn;
                    case eSeverity.Medium:
                        return eImageType.MediumWarn;
                    case eSeverity.Low:
                        return eImageType.LowWarn;
                    default:
                        return eImageType.LowWarn;
                }
                           
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
