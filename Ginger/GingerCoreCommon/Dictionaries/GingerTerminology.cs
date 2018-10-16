using System;
using System.Collections.Generic;
using System.Text;

//namespace Amdocs.Ginger.Common.Dictionaries
namespace GingerCore
{
    public class GingerTerminology
    {
        public static eTerminologyDicsType SET_TERMINOLOGY_TYPE { get; set; }

        static List<KeyValuePair<string, string>> gingerTermDefaultList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("BusinessFlow", "Business Flow"),
            new KeyValuePair<string, string>("BusinessFlows", "Business Flows"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Activities Group"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Activities Groups"),
            new KeyValuePair<string, string>("Activity", "Activity"),
            new KeyValuePair<string, string>("Activities", "Activities"),
            new KeyValuePair<string, string>("Variable", "Variable"),
            new KeyValuePair<string, string>("Variables", "Variables"),
            new KeyValuePair<string, string>("RunSet", "Run Set"),
            new KeyValuePair<string, string>("RunSets", "Run Sets"),
            new KeyValuePair<string, string>("ALM", "ALM")
        };

        static List<KeyValuePair<string, string>> gingerTermGherkinList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("BusinessFlow", "Business Flow Feature"),
            new KeyValuePair<string, string>("BusinessFlows", "Business Flow Features"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Scenario"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Scenarios"),
            new KeyValuePair<string, string>("Activity", "Step"),
            new KeyValuePair<string, string>("Activities", "Steps"),
            new KeyValuePair<string, string>("Variable", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Parameters"),
            new KeyValuePair<string, string>("RunSet", "Run Set"),
            new KeyValuePair<string, string>("RunSets", "Run Sets")
        };


        static List<KeyValuePair<string, string>> gingerTermTestingList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("BusinessFlow", "Test Set"),
            new KeyValuePair<string, string>("BusinessFlows", "Test Sets"),
            new KeyValuePair<string, string>("ActivitiesGroup", "Test Case"),
            new KeyValuePair<string, string>("ActivitiesGroups", "Test Cases"),
            new KeyValuePair<string, string>("Activity", "Step"),
            new KeyValuePair<string, string>("Activities", "Steps"),
            new KeyValuePair<string, string>("Variable", "Parameter"),
            new KeyValuePair<string, string>("Variables", "Parameters"),
            new KeyValuePair<string, string>("RunSet", "Calendar"),
            new KeyValuePair<string, string>("RunSets", "Calendars")
        };

        public static string GetTerminologyValue(eTermResKey key)
        {
            KeyValuePair<string, string> result = new KeyValuePair<string, string>();
            switch (SET_TERMINOLOGY_TYPE)
            {
                case eTerminologyDicsType.Default:
                    result = gingerTermDefaultList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyDicsType.Gherkin:
                    result = gingerTermGherkinList.Find(kvp => kvp.Key == key.ToString());
                    break;
                case eTerminologyDicsType.Testing:
                    result = gingerTermTestingList.Find(kvp => kvp.Key == key.ToString());
                    break;
            }
            return result.Value;
        }

    }


}
