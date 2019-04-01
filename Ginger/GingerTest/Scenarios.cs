#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest
{
    class Scenarios
    {
        public class Scenario
        {
            public string ID;
            public int Priority;
            public string Area;
            public string SubArea;
            public string Description;
        }

        public struct Solution
        {
            public static Scenario Select = new Scenario() { ID = "Solution.New", Area = "Solution", Description = "Create new solution" };
        }

        public struct RunTab
        {
            public static Scenario Select = new Scenario() { ID = "RunTab.Select", Area = "MainWindow", Description = "Select Run Tab" };
        }

        public struct ApplicationModel
        {
            public struct API
            {
                public static Scenario Add = new Scenario() { ID = "ApplicationModel.API", Area = "ApplicationModel", SubArea="Add", Description = "Add Application model" };                
                public static string Delete = "AMAPI_2";
                public static string Update = "AMAPI_3";
            }

            public struct POM
            {
                public static Scenario Add = new Scenario() { ID = "ApplicationModel.POM", Area = "ApplicationModel", SubArea = "Add", Description = "Add POM Application model" };
                public static string Delete = "AMPOM_2";
                public static string Update = "AMPOM_3";
            }

            //Processes,
            //DB
        }
    }
}
