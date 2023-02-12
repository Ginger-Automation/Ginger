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

using System.Collections.Generic;
using System.Linq;
using System;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Actions;
using System.ComponentModel;

namespace Amdocs.Ginger.CoreNET
{
    public class ConvertableActionDetails : INotifyPropertyChanged
    {
        // holds the list of parent activities containing convertible actions
        public List<string> ActivityList { get; set; }

        // holds the list of convertible actions
        public List<Act> Actions { get; set; }

        // whether selected to be converted or not
        bool mSelected;
        public bool Selected
        {
            get { return mSelected; }
            set
            {
                if (mSelected != value)
                {
                    mSelected = value;
                    OnPropertyChanged(nameof(ConvertableActionDetails.Selected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public string SourceActionTypeName { get; set; }
        public string TargetActionTypeName { get; set;}

        // holds occurrences/count of convertible action types in the list
        public int ActionCount { get; set; }

        public Type TargetActionType { get; set; }
        public Type SourceActionType { get; set; }

        public ePlatformType TargetPlatform
        {
            get
            {
                ePlatformType str = ePlatformType.NA;
                foreach (var action in Actions.Distinct().ToList())
                {
                    if (action != null && action is IObsoleteAction)
                    {
                        str = ((IObsoleteAction)action).GetTargetPlatform();

                    }
                }
                return str;
            }
            set { }
        }

        public string Activities
        {
            get
            {
                string str = "";
                var wordCount =
                            from word in ActivityList.ToList()
                            group word by word into g
                            select new { g.Key, Count = g.Count() };

                foreach (var action in wordCount)
                {
                    if (action != null)
                    {
                        str += " " + action.Key + " (" + action.Count + "),";
                    }
                }
                if (str.EndsWith(","))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                return str;

            }
            set { }
        }

        public ConvertableActionDetails()
        {
            ActivityList = new List<string>();
            Actions = new List<Act>();
        }      
    }
}
