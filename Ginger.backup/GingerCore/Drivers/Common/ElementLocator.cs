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
using GingerCore.Actions.Common;

namespace GingerCore.Drivers.Common
{
    public class ElementLocator : RepositoryItem
    {
        public new static class Fields
        {
            public static string LocateBy = "LocateBy";
            public static string LocateValue = "LocateValue";
            public static string Help = "Help";
            public static string Count = "Count";            
        }

        private ActUIElement.eLocateBy mLocateBy { get; set; }
        //TODO: need to use the one from ACTUIElement
        [IsSerializedForLocalRepository]
        public ActUIElement.eLocateBy LocateBy { get { return mLocateBy; } set { mLocateBy = value; OnPropertyChanged(Fields.LocateBy); } }

        private string mLocateValue { get; set; }
        [IsSerializedForLocalRepository]
        public string LocateValue { get { return mLocateValue; } set { mLocateValue = value; OnPropertyChanged(Fields.LocateValue); } }

        private string mHelp { get; set; }
        public string Help { get { return mHelp; } set { mHelp = value; OnPropertyChanged(Fields.Help); } }

        private int? mCount { get; set; }
        public int? Count { get { return mCount; } set { mCount = value; OnPropertyChanged(Fields.Count); } }

        public override string ItemName { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
    }
}
