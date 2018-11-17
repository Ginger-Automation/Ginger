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

using System.Collections.Generic;
using System.Linq;
using System;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions.ActionConversion
{
    public class ActionConversionHandler
    {
        public static partial class Fields
        {
            public static string Activities = "Activities";
            public static string ActionCount = "ActionCount";
            public static string SourceActionTypeName = "SourceActionTypeName";
            public static string SourceOperationName = "SourceOperationName";
            public static string TargetActionTypeName = "TargetActionTypeName";
            public static string TargetActionPlatform = "TargetActionPlatform";
            public static string Selected = "Selected";
        }

        // holds the list of parent activities containing convertible actions
        public List<string> ActivityList { get; set; }

        // holds the list of convertible actions
        public List<Act> Actions { get; set; }

        // whether selected to be converted or not
        public bool Selected { get; set; }
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

        public ActionConversionHandler()
        {
            ActivityList = new List<string>();
            Actions = new List<Act>();
        }      
    }
}
