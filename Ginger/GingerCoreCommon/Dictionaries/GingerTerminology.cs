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

using System.Collections.Generic;


namespace GingerCore
{
    public class GingerTerminology
    {
        public static eTerminologyType TERMINOLOGY_TYPE { get; set; }

        static List<KeyValuePair<string, string>> gingerTermDefaultList =
        [
            new KeyValuePair<string, string>("BusinessFlow", "Business Flow"),
            new KeyValuePair<string, string>("BusinessFlows", "Business Flows"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Activities Group"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Activities Groups"),
            new KeyValuePair<string, string>("Activity", "Activity"),
            new KeyValuePair<string, string>("Activities", "Activities"),
            new KeyValuePair<string, string>("Variable", "Variable"),
            new KeyValuePair<string, string>("Parameter", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Variables"),
            new KeyValuePair<string, string>("RunSet", "Run Set"),
            new KeyValuePair<string, string>("RunSets", "Run Sets"),
            new KeyValuePair<string, string>("ALM", "ALM"),
            new KeyValuePair<string, string>("TargetApplication", "Target Application")
        ];

        static List<KeyValuePair<string, string>> gingerTermGherkinList =
        [
            new KeyValuePair<string, string>("BusinessFlow", "Business Flow Feature"),
            new KeyValuePair<string, string>("BusinessFlows", "Business Flow Features"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Scenario"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Scenarios"),
            new KeyValuePair<string, string>("Activity", "Step"),
            new KeyValuePair<string, string>("Activities", "Steps"),
            new KeyValuePair<string, string>("Variable", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Parameters"),
            new KeyValuePair<string, string>("RunSet", "Run Set"),
            new KeyValuePair<string, string>("RunSets", "Run Sets"),
            new KeyValuePair<string, string>("TargetApplication", "Target Application")
        ];


        static List<KeyValuePair<string, string>> gingerTermTestingList =
        [
            new KeyValuePair<string, string>("BusinessFlow", "Test Set"),
            new KeyValuePair<string, string>("BusinessFlows", "Test Sets"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Test Case"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Test Cases"),
            new KeyValuePair<string, string>("Activity", "Step"),
            new KeyValuePair<string, string>("Activities", "Steps"),
            new KeyValuePair<string, string>("Variable", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Parameters"),
            new KeyValuePair<string, string>("RunSet", "Calendar"),
            new KeyValuePair<string, string>("RunSets", "Calendars"),
            new KeyValuePair<string, string>("TargetApplication", "Target Application")
        ];

        static List<KeyValuePair<string, string>> gingerTermTDMList =
        [
            new KeyValuePair<string, string>("BusinessFlow", "Sub Business Process"),
            new KeyValuePair<string, string>("BusinessFlows", "Sub Business Processes"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Activity"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Activities"),
            new KeyValuePair<string, string>("Activity", "Step"),
            new KeyValuePair<string, string>("Activities", "Steps"),
            new KeyValuePair<string, string>("Variable", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Parameters"),
            new KeyValuePair<string, string>("RunSet", "Business Process"),
            new KeyValuePair<string, string>("RunSets", "Business Processes"),
            new KeyValuePair<string, string>("TargetApplication", "Target Application")
        ];

        static List<KeyValuePair<string, string>> gingerTermMBTList =
        [
            new KeyValuePair<string, string>("BusinessFlow", "Use Case"),
            new KeyValuePair<string, string>("BusinessFlows", "Use Cases"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Sub process"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Sub processes"),
            new KeyValuePair<string, string>("Activity", "Interface"),
            new KeyValuePair<string, string>("Activities", "Interfaces"),
            new KeyValuePair<string, string>("Variable", "Element"),
            new KeyValuePair<string, string>("TargetApplication", "System"),
            new KeyValuePair<string, string>("Variables", "Elements"),
            new KeyValuePair<string, string>("RunSet", "Test Suite"),
            new KeyValuePair<string, string>("RunSets", "Test Suites"),
            new KeyValuePair<string, string>("ALM", "ALM")
        ];

        public static string GetTerminologyValue(eTermResKey key)
        {
            KeyValuePair<string, string> result = new KeyValuePair<string, string>();
            switch (TERMINOLOGY_TYPE)
            {
                case eTerminologyType.Default:
                    result = gingerTermDefaultList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyType.Gherkin:
                    result = gingerTermGherkinList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyType.Testing:
                    result = gingerTermTestingList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyType.TDM:
                    result = gingerTermTDMList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyType.MBT:
                    result = gingerTermMBTList.Find(kvp => kvp.Key == key.ToString());
                    break;
            }
            return result.Value;
        }

    }


}
