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


namespace GingerATS
{
    public enum eGingerRepoItemType
    {
        ActivitiesGroup,Activity,Action,Variable
    }

    public enum eGeneralGingerRepoAttributes
    {
        Guid, ExternalID,LastUpdate
    }

    public class GingerRepositoryItem
    {
        public eGingerRepoItemType Type { get; set; }
        public string Name { get; set; }
        public string GUID { get; set; }
        public string ExternalID { get; set; }
        public string FilePath { get; set; }
        public string LastUpdated { get; set; }
        public string XmlFileDetails { get; set; }        

        public static string ActivityAutomationStatusAttributeName { get { return "AutomationStatus"; } }
        public static string ActivityAutomationStatusAutomatedValue { get { return "Automated"; } }

        public static string ActivityVariablesAttributeName { get { return "Variables"; } }
        public static string ActivityVariableNameAttribute { get { return "Name"; } }

        public static string VariableSelectionListNodeName { get { return ".VariableSelectionList"; } }
        public static string VariableSelectionListNameAttribute { get { return "Name"; } }
        public static string VariableSelectionListOptionalValuesAttribute { get { return "OptionalValues"; } }

        public GingerRepositoryItem(eGingerRepoItemType type)
        {
            this.Type = type;
        }

        public static string GetRepoItemMainFolderName(eGingerRepoItemType itemType)
        {
            switch (itemType)
            {
                case (eGingerRepoItemType.ActivitiesGroup):
                    return "ActivitiesGroup";
                case (eGingerRepoItemType.Activity):
                    return "Activities";
                case (eGingerRepoItemType.Action):
                    return "Actions";
                case (eGingerRepoItemType.Variable):
                    return "Variables";
            }

            return string.Empty;
        }

        public static string GetRepoItemTypeLabelInIndexer(eGingerRepoItemType itemType)
        {
            switch (itemType)
            {
                case (eGingerRepoItemType.ActivitiesGroup):
                    return "#ACTIVITIES GROUP#";
                case (eGingerRepoItemType.Activity):
                    return "#ACTIVITY#";
                case (eGingerRepoItemType.Action):
                    return "#ACTION#";
                case (eGingerRepoItemType.Variable):
                    return "#VARIABLE#";
            }

            return string.Empty;
        }

        public static string GetRepoItemMainXmlNodeName(eGingerRepoItemType itemType)
        {
            switch (itemType)
            {
                case (eGingerRepoItemType.ActivitiesGroup):
                    return "ActivitiesGroup";//GingerCore.Activities.ActivitiesGroup
                case (eGingerRepoItemType.Activity):
                    return "Activity";//GingerCore.Activity
                case (eGingerRepoItemType.Action):
                    return "Act";//been changed for each Action type like: GingerCore.Actions.ActTextBox
                case (eGingerRepoItemType.Variable):
                    return "Variable";//been changed for each Variable type like: GingerCore.Variables.VariableString
            }

            return string.Empty;
        }

        public static string GetRepoItemNameFieldLabel(eGingerRepoItemType itemType)
        {
            switch (itemType)
            {
                case (eGingerRepoItemType.ActivitiesGroup):
                    return "Name";
                case (eGingerRepoItemType.Activity):
                    return "ActivityName";
                case (eGingerRepoItemType.Action):
                    return "Name";
                case (eGingerRepoItemType.Variable):
                    return "Name";
            }

            return string.Empty;
        }
    }
}
