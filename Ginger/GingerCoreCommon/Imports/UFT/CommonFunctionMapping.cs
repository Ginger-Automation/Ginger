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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.Actions;


namespace Ginger.Imports.UFT
{
    public class CommonFunctionMapping : RepositoryItemBase
    {
        public  static class Fields
        {
            public static string Function_Name = "Function_Name";
            public static string Action_Description = "Action_Description";
            public static string LocateBy = "LocateBy";
            public static string Value = "Value";
            public static string NoOfParameters = "NoOfParameters";
        }

        [IsSerializedForLocalRepository]
        public string Function_Name { get; set; }

        [IsSerializedForLocalRepository]
        public Act TargetAction; 
        
        [IsSerializedForLocalRepository]
        public string LocateBy { get; set; }

        [IsSerializedForLocalRepository]
        public string Value { get; set; }

        [IsSerializedForLocalRepository]
        public string NoOfParameters { get; set; }
        
        public string Action_Description// ActionInfo
        {
            get
            {
                {
                    return TargetAction.ActionDescription;
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
